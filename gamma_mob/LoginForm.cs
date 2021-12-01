using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.IO;
using System.Reflection;
using Datalogic.API;
using OpenNETCF.Net.NetworkInformation;
using Microsoft.Win32;
using System.Net;
using gamma_mob.Dialogs;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace gamma_mob
{
    
    public partial class LoginForm : BaseForm
    {
        public LoginForm()
        {
            InitializeComponent();

           

            //if (!ConnectionState.CheckConnection()) return;
            ///*if (Db.CheckSqlConnection() != 1) return;
            //WrongUserPass();
            // * */
            //switch (Db.CheckSqlConnection())
            //{
            //    case 2:
            //        ConnectionError();
            //        return;
            //    case 1:
            //        WrongUserPass();
            //        return;
            //}


        }

        private string barcode;

        [DllImport("coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);


        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = AuthorizeByBarcode;
            try
            {
                Shared.SaveToLog("Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                Shared.SaveToLog("Model " + Datalogic.API.Device.GetModel().ToString());
                var strHostName = Dns.GetHostName();
                Shared.SaveToLog("HostName " + strHostName);

                IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                IPAddress[] addr = ipEntry.AddressList;
                string ipAdress = "";
                for (int i = 0; i < addr.Length; i++)
                {
                    ipAdress = ipAdress+ "   " + i.ToString() + ": " + addr[i].ToString();
                }
                Shared.SaveToLog("IpAdress " + ipAdress);

                var cerdispProcess = GetProcessRunning(@"cerdisp");
                if (cerdispProcess != null)
                {
                    btnExecRDP.Text = "Останов RDP";
                }
                else
                {
                    btnExecRDP.Text = "Запуск RDP";
                }
                Shared.SaveToLog("DbBarcodes created:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture));
                Shared.UpdateBatterySerialumber();
                var batterySuspendTimeout = Device.GetBatterySuspendTimeout();
                Shared.SaveToLog("BatterySuspendTimeout " + batterySuspendTimeout.ToString());
                Shared.SaveToLog("BatteryLevel " + Device.GetBatteryLevel().ToString());
                Shared.SaveToLog("WiFiPowerStatus " + Device.GetWiFiPowerStatus().ToString());
                if (batterySuspendTimeout != 600)
                    Shared.SaveToLog("SetBatterySuspendTimeout(600) " + Device.SetBatterySuspendTimeout(600).ToString());
                
                UInt64 userFreeBytes, totalDiskBytes, totalFreeBytesExecutable, totalFreeBytesUpdatable;
                GetDiskFreeSpaceEx(@"\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesExecutable);
                GetDiskFreeSpaceEx(@"\FlashDisk\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesUpdatable);
                Shared.SaveToLog("FreeSpace " + totalFreeBytesExecutable.ToString() + "/" +
                    totalFreeBytesUpdatable.ToString());
            }
            catch
            {
            }

            //Подписка на событие восстановления связи
            ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            //Подписка на событие потери связи
            ConnectionState.OnConnectionLost += ConnectionLost;

            ConnectionState.CheckConnection(Settings.CurrentServer);
            if (Shared.TimerForCheckBatteryLevel == null)
            {
                Shared.SaveToLog(@"Внимание! Не запущена автоматическая проверка уровня заряда аккумулятора.");
            }
//#if DEBUG
//            AuthorizeByBarcode("00000000000056");
//#endif
        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
            ConnectionState.OnConnectionRestored -= ConnectionRestored;
            ConnectionState.OnConnectionLost -= ConnectionLost;
        }

        private void ConnectionLost()
        {
            ConnectionLost(string.Empty);
        }

        private void ConnectionRestored()
        {
            ConnectionRestored(string.Empty);            
        }

        private void ConnectionLost(string message)
        {
            Invoke((LoginStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection, message });
        }

        private void ConnectionRestored(string message)
        {
            Invoke((LoginStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore, message });
            //Invoke(new EventHandler(ConnectionRestored));
        }

        private void ShowConnection(ConnectState conState, string message)
        {
            switch (conState)
            {
                //case ConnectState.ConInProgress:
                //    imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                //    break;
                //case ConnectState.NoConInProgress:
                //    imgConnection.Image = null;
                //    break;
                case ConnectState.NoConnection:
                    lblMessage.Text = message == string.Empty ? "Связь потеряна! \r\n\r\nНайдите зону с устойчивой связью" : message ;
                    break;
                case ConnectState.ConnectionRestore:
                    lblMessage.Text = message == string.Empty ? "Связь восстановлена! \r\n\r\nПросканируйте \r\nсвой штрих-код" : message;
                    //ConnectionRestored();
                    break;
            }
        }

        private void AuthorizeByBarcode(string barcode)
        {
            Invoke(
                (MethodInvoker)
                (() => lblMessage.Text = barcode + Environment.NewLine +
                    "\r\n\r\nИдет проверка сети..."));
            if (!ConnectionState.CheckConnection(Settings.CurrentServer))
            {
                ConnectionError();
                return;
            }
            if (!ConnectionState.GetServerPortEnabled)
            {
                ConnectionLost();
                return;
            }
            switch (Db.CheckSqlConnection())
            {
                case 2:
                    ConnectionError();
                    return;
                case 1:
                    WrongUserPass();
                    return;
            }
            
            Person person = Db.PersonByBarcode(barcode);
            if (person == null)
            {
                //MessageBox.Show(@"Неверный шк или нет связи с базой");
                ConnectionLost(@"Неверный шк или нет связи с базой");
                return;
            }

            if (person.UserName == String.Empty)
            {
                //MessageBox.Show(@"Не определен логин." + Environment.NewLine + @"Обратитесь в техподдержку Гаммы!", @"Логин не определен",
                //                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                ConnectionLost(@"Не определен логин." + Environment.NewLine + @"Обратитесь в техподдержку Гаммы!");
                return;
            }
            Shared.IsFindBarcodeFromFirstLocalAndNextOnline = (person.b1 ?? false);
            if (person.UserName.Contains("0"))
            {
               using (var form = new ChooseShiftDialog())
                {
                    DialogResult result = form.ShowDialog();
                    if (result != DialogResult.OK || form.ShiftId < 1)
                    {
                        //MessageBox.Show(@"Не указан номер смены." + Environment.NewLine + @"Попробуйте еще раз!", @"Смена не выбрана",
                        //                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        ConnectionLost(@"Не указан номер смены." + Environment.NewLine + @"Попробуйте еще раз!");
                        return;
                    }

                    Settings.UserName = person.UserName.Replace("0", form.ShiftId.ToString());
                    Settings.Password = "123456";
                    Shared.ShiftId = form.ShiftId;
                }
            }
            else
            {
                using (var form = new ChoosePasswordDialog())
                {
                    DialogResult result = form.ShowDialog();
                    if (result != DialogResult.OK || form.Password == String.Empty)
                    {
                        //MessageBox.Show(@"Не указан пароль." + Environment.NewLine + @"Попробуйте еще раз!", @"Пароль не введен",
                        //                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        ConnectionLost(@"Не указан пароль." + Environment.NewLine + @"Попробуйте еще раз!");
                        return;
                    }
                    Settings.UserName = person.UserName;
                    Settings.Password = form.Password;
                    Shared.ShiftId = 0;
                }
            }
            if (!ConnectionState.CheckConnection()) return;
            Db.SetConnectionString(Settings.CurrentServer, Settings.Database, Settings.UserName, Settings.Password, Settings.TimeOut);
            switch (Db.CheckSqlConnection())
            {
                case 2:
                    ConnectionError();
                    return;
                case 1:
                    WrongUserPass();
                    return;
                case 0:
                    ConnectionState.IsConnected = true;
                    break;
            }

            Shared.PersonId = person.PersonID;
            Shared.PersonName = person.Name;
            Invoke(
                (MethodInvoker)
                (() => lblMessage.Text = "Вы авторизовались " + Environment.NewLine + "как " + person.Name + " (" + Settings.UserName + ")" +
                    "\r\n\r\nИдет загрузка данных..."));
            //Shared.LastTimeBarcodes1C = Db.GetServerDateTime();
            //Shared.Barcodes1C = Db.GetBarcodes1C();
            
            if (!Shared.InitializationData())                
            {
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.Text = "Просканируйте \r\nсвой штрих-код"));
                ConnectionError();
                return;
            }
            //Thread.Sleep(3000);
            Invoke((MethodInvoker) (CloseForm));
        }

        private void CloseForm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ConnectionError()
        {
            //MessageBox.Show(@"Нет связи с базой данных. Попробуйте еще раз." + Environment.NewLine + ConnectionState.GetConnectionState()
            //                , @"Ошибка связи с БД",
            //                MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            ConnectionLost(@"Нет связи с базой данных. Попробуйте еще раз." + Environment.NewLine + ConnectionState.GetConnectionState());
        }

        private void WrongUserPass()
        {
            //MessageBox.Show(@"Неверно указан логин или пароль в настройках. Обратитесь к администратору приложения"
            //                , @"Ошибка связи с БД",
            //                MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            ConnectionLost(@"Неверно указан логин или пароль в настройках. Обратитесь к администратору приложения");
        }

        private void btnExecRDP_Click(object sender, EventArgs e)
        {
            var cerdispProcess = GetProcessRunning(@"cerdisp");
            if (cerdispProcess == null)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CERDISP", true);
                //RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", true);
                //reg.DeleteSubKey(@"CERDISP");
                if (reg == null)
                {
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", true);
                    reg.CreateSubKey(@"CERDISP");
                    reg = reg.OpenSubKey(@"CERDISP", true);
                }
                var serverIP = ConnectionState.GetServerIp();
                var hostname = (string)reg.GetValue("Hostname", "");
                if (serverIP != "" && hostname != serverIP)
                {
                    reg.SetValue("Hostname", serverIP, RegistryValueKind.String);
                }

                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                           @"\cerdisp.exe"))
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                               @"\cerdisp.exe", "-c");
                    lblMessage.Text = "RDP запущен по адресу " + serverIP;
                    btnExecRDP.Text = "Останов RDP";
                }
                else
                {
                    lblMessage.Text = "RDP не запущен. Файл" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                               @"\cerdisp.exe не найден.";
                }
            }
            else
            {
                cerdispProcess.Kill();
                lblMessage.Text = "RDP остановлен";
                btnExecRDP.Text = "Запуск RDP";
            }
        }

        private static OpenNETCF.ToolHelp.ProcessEntry GetProcessRunning(string processsName)
        {
            try
            {
                OpenNETCF.ToolHelp.ProcessEntry cerdispProcess = null;
                foreach (OpenNETCF.ToolHelp.ProcessEntry clsProcess in OpenNETCF.ToolHelp.ProcessEntry.GetProcesses())
                {
                    if (cerdispProcess == null && clsProcess.ExeFile.Contains(processsName))
                    {
                        cerdispProcess = clsProcess;
                    }
                }
                return cerdispProcess;
            }
            catch
            {
                return null;
            }

        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (btnHelp.Text == "Сеть")
            {
                btnHelp.Text = "Скрыть";
                lblMessage.Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Regular);
                lblMessage.Text = "Версия: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine + "Сервер: " + Settings.CurrentServer + Environment.NewLine + "БД/логин: " + Settings.Database + "/" + Settings.UserName;
                lblMessage.Text = lblMessage.Text + Environment.NewLine + "БД ШК создан:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture);
                btnExecRDP.Visible = true;
                btnTestPing.Visible = true;
                btnTestSQL.Visible = true;
                btnTestWiFi.Visible = true;
                btnSetExternalNet.Visible = true;
                btnSetInternalNet.Visible = true;
                RefreshBtnSetNetEnabled();
                try
                {
                    var strHostName = Dns.GetHostName();
                    lblMessage.Text = lblMessage.Text + Environment.NewLine + "Имя ТСД: " + strHostName;

                    IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                    IPAddress[] addr = ipEntry.AddressList;

                    for (int i = 0; i < addr.Length; i++)
                    {
                        lblMessage.Text = lblMessage.Text + Environment.NewLine + "IP адрес " + i.ToString() + ": " + addr[i].ToString();
                    }
                }
                catch
                {
                    lblMessage.Text = lblMessage.Text + Environment.NewLine + "Ошибка при определении IP адреса";
                }
            }
            else
            {
                btnHelp.Text = "Сеть";
                lblMessage.Font = new System.Drawing.Font("Tahoma", 14, System.Drawing.FontStyle.Regular);
                lblMessage.Text = "Просканируйте \r\nсвой штрих-код";
                btnExecRDP.Visible = false;
                btnTestPing.Visible = false;
                btnTestSQL.Visible = false;
                btnTestWiFi.Visible = false;
                btnSetExternalNet.Visible = false;
                btnSetInternalNet.Visible = false;
                try
                {
                    var cerdispProcess = GetProcessRunning(@"cerdisp");
                    if (cerdispProcess != null)
                    {
                        cerdispProcess.Kill();
                    }
                }
                catch
                {
                }
            }
        }

        private void btnTestWiFi_Click(object sender, EventArgs e)
        {
            if (Device.GetWiFiPowerStatus())
            {
                uint quality;
                if (Device.WiFiGetSignalQuality(out quality))
                {
                    lblMessage.Text = "WiFi уровень сигнала " + quality.ToString();
                    if (quality < 3)
                    {
                        lblMessage.Text = lblMessage.Text + " (низкий)";
                    }
                }
                else
                {
                    lblMessage.Text = "WiFi ошибка при проверке уровня сигнала";
                }
            }
            else
            {
                lblMessage.Text = "WiFi сигнал отсутствует";
            }
        }

        private void btnTestPing_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            var ServerIp = ConnectionState.GetServerIp();
            if (ServerIp == "")
            {
                lblMessage.Text = "Не определен IP сервера";
            }
            else
            {
                using (var pinger = new Ping())
                {
                    try
                    {
                        PingReply reply = pinger.Send(ServerIp, 200);
                        if (reply.Status != IPStatus.Success)
                        {
                            lblMessage.Text = "Сервер "+ServerIp+" не пингуется c таймаутом 200 мс";
                            reply = pinger.Send(ServerIp, 400);
                            if (reply.Status != IPStatus.Success)
                            {
                                lblMessage.Text = lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 400 мс";
                                reply = pinger.Send(ServerIp, 800);
                                if (reply.Status != IPStatus.Success)
                                {
                                    lblMessage.Text = lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 800 мс";
                                    reply = pinger.Send(ServerIp, 1600);
                                    if (reply.Status != IPStatus.Success)
                                    {
                                        lblMessage.Text = lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 1600 мс";
                                    }
                                }
                            }
                        }
                        if (reply.Status == IPStatus.Success)
                        {
                            lblMessage.Text = lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " пингуется "+reply.ToString();
                        }
                    }
                    catch (Exception ex) 
                    {
                        lblMessage.Text = lblMessage.Text + Environment.NewLine + "Ошибка при пинге сервера " + ServerIp + ": " + ex.ToString();
                    }
                }
            }
        }

        private void btnTestSQL_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection(Settings.CurrentServer))
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        lblMessage.Text = "Нет связи с БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                    case 1:
                        lblMessage.Text = "Неверно указан логин или пароль к БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                    default:
                        lblMessage.Text = "Соединение с БД " + Settings.CurrentServer + " установлено" + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                }
            }
            else
            {
                lblMessage.Text = "Нет связи с БД " + Settings.CurrentServer + ". Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState();
            }
            Cursor.Current = Cursors.Default;
        }

        private void SetBarcode(char keyChar)
        {
            if (barcode == null) barcode = "";
            if (keyChar == (char)13)
            {
               
                var barcodeLength = barcode.Length;
                for (int i = 0; i < 14 - barcodeLength; i++)
                {
                    barcode = "0" + barcode;
                }
                lblMessage.Text = barcode;
                AuthorizeByBarcode(barcode);
                barcode = String.Empty;
            }
            else
            {
                barcode = barcode + keyChar.ToString();
            }
        }
        
        private void LoginForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            SetBarcode(e.KeyChar);
        }

        private void lblMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            SetBarcode(e.KeyChar);
        }

        private void btnSetInternalNet_Click(object sender, EventArgs e)
        {
            ChangeCurrentServer(false);
        }

        private void btnSetExternalNet_Click(object sender, EventArgs e)
        {
            ChangeCurrentServer(true);
        }

        private void ChangeCurrentServer(bool isChangeExternalServer)
        {
            if (MessageBox.Show("Вы уверены, что хотите изменить адрес сервера?","Изменение адреса сервера", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                if ((!isChangeExternalServer) ? Settings.SetCurrentInternalServer() : Settings.SetCurrentExternalServer())
                {
                    RefreshBtnSetNetEnabled();
                    lblMessage.Text = "Установлен адрес сервера " + Settings.CurrentServer;
                }
                else
                    lblMessage.Text = "Ошибка при установке адреса сервера " + (!isChangeExternalServer ? Settings.ServerIP : Settings.SecondServerIP);
            }
        }

        public void RefreshBtnSetNetEnabled()
        {
            btnSetExternalNet.Enabled = Settings.CurrentServer == Settings.ServerIP;
            btnSetInternalNet.Enabled = Settings.CurrentServer == Settings.SecondServerIP;
        }
    }
}