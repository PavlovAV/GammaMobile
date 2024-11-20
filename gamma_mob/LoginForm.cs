﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.IO;
using System.Reflection;
using OpenNETCF.Net.NetworkInformation;
using Microsoft.Win32;
using System.Net;
using gamma_mob.Dialogs;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace gamma_mob
{

    public partial class LoginForm : BaseFormWithShowMessage
    {
        public LoginForm()
        {
            InitializeComponent(); 
            //imgConnection.Image = ImgList.Images[(int)Images.NetworkOffline];
        }

        private string barcode;

        [DllImport("coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        private bool initConnectionStarting { get; set; }

        public void InitConnection(object obj)
        {
            if (!initConnectionStarting)
            {
                initConnectionStarting = true;
                SetLblMessageText(Environment.NewLine + Environment.NewLine + 
                        "Идет настройка соединения с сетью...");
                ConnectionState.GetIpFromSettings(Settings.CurrentServer);
            var t = ConnectionState.TimerForCheckConnectionAvialabled;
            try
            {
                Shared.SaveToLogStartProgramInformation(@"Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                Shared.SaveToLogStartProgramInformation(@"Model " + Shared.Device.GetModel());
                Shared.SaveToLogStartProgramInformation(@"HostName " + Shared.Device.GetHostName());
                Shared.SaveToLogStartProgramInformation(@"IpAdress " + Shared.Device.GetDeviceIP());
                var cerdispProcess = Shared.GetProcessRunning(@"cerdisp");
                if (cerdispProcess != null)
                {
                    btnExecRDP.Text = "Останов RDP";
                }
                else
                {
                    btnExecRDP.Text = "Запуск RDP";
                }
                Shared.SaveToLogStartProgramInformation(@"DbBarcodes created:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture));
                Shared.UpdateBatterySerialumber();
                var batterySuspendTimeout = Shared.Device.GetBatterySuspendTimeout();
                Shared.SaveToLogStartProgramInformation(@"BatterySuspendTimeout " + batterySuspendTimeout.ToString());
                Shared.SaveToLogStartProgramInformation(@"BatteryLevel " + Shared.Device.GetBatteryLevel().ToString());
                Shared.SaveToLogStartProgramInformation(@"WiFiPowerStatus " + Shared.Device.GetWiFiPowerStatus().ToString());
                if (batterySuspendTimeout != 600)
                    Shared.SaveToLogStartProgramInformation(@"SetBatterySuspendTimeout(600) " + Shared.Device.SetBatterySuspendTimeout(600).ToString());

                UInt64 userFreeBytes, totalDiskBytes, totalFreeBytesExecutable, totalFreeBytesUpdatable;
                GetDiskFreeSpaceEx(@"\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesExecutable);
                GetDiskFreeSpaceEx(@"\FlashDisk\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesUpdatable);
                Shared.SaveToLogStartProgramInformation(@"FreeSpace " + totalFreeBytesExecutable.ToString() + @"/" +
                    totalFreeBytesUpdatable.ToString());
            }
            catch
            {
                Shared.SaveToLogError(@"Error LoginFormLoad");
            }

            ////Подписка на событие восстановления связи
            //ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            ////Подписка на событие потери связи
            //ConnectionState.OnConnectionLost += ConnectionLost;

            ConnectionState.CheckConnection(Settings.CurrentServer);
            if (Shared.TimerForCheckBatteryLevel == null)
            {
                Shared.SaveToLogError(@"Внимание! Не запущена автоматическая проверка уровня заряда аккумулятора.");
            }

#if !DEBUG
            if (Shared.TimerForCheckUpdateProgram == null)
            {
                Shared.SaveToLogError(@"Внимание! Не запущена автоматическая проверка на наличие новой версии программы.");
            }
#endif
                SetLblMessageText("Просканируйте свой штрих-код");
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = AuthorizeByBarcode;
            //ConnectionState.GetIpFromSettings(Settings.CurrentServer);
#if DEBUG
            InitConnection(null);
            //AuthorizeByBarcode("00000000000217"); //Грузчик мак.участка
            //AuthorizeByBarcode("00000000000068"); //Лобанов
            //AuthorizeByBarcode("00000000000088"); //Багрянцев
            //AuthorizeByBarcode("20100000002142"); //Березгин
            AuthorizeByBarcode("20100000000223"); //солодухин
            //AuthorizeByBarcode("20100000001282"); //Шарыпов
#else
            TimerCallback tc = new TimerCallback(InitConnection);
            // создаем таймер
            var tm = new System.Threading.Timer(tc, null, 1000, Timeout.Infinite);
#endif
            
//            var t = ConnectionState.TimerForCheckConnectionAvialabled;
//            //if (ConnectionState.CheckConnection(Settings.CurrentServer))
//            //{
//            //    switch (Db.CheckSqlConnection())
//            //    {
//            //        case 2:
//            //            ConnectionLost();
//            //            lblMessage.Text = "Нет связи с БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState();
//            //            break;
//            //        case 1:
//            //            lblMessage.Text = "Неверно указан логин или пароль к БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState();
//            //            break;
//            //        default:
//            //            lblMessage.Text = "Соединение с БД " + Settings.CurrentServer + " установлено" + Environment.NewLine + ConnectionState.GetConnectionState();
//            //            break;
//            //    }
//            //}
//            //else
//            //{
//            //    lblMessage.Text = "Нет связи с БД " + Settings.CurrentServer + ". Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState();
//            //}
//            try
//            {
//                Shared.SaveToLogStartProgramInformation(@"Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
//                Shared.SaveToLogStartProgramInformation(@"Model " + Shared.Device.GetModel());
//                Shared.SaveToLogStartProgramInformation(@"HostName " + Shared.Device.GetHostName());
//                Shared.SaveToLogStartProgramInformation(@"IpAdress " + Shared.Device.GetDeviceIP());
//                var cerdispProcess = Shared.GetProcessRunning(@"cerdisp");
//                if (cerdispProcess != null)
//                {
//                    btnExecRDP.Text = "Останов RDP";
//                }
//                else
//                {
//                    btnExecRDP.Text = "Запуск RDP";
//                }
//                Shared.SaveToLogStartProgramInformation(@"DbBarcodes created:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture));
//                Shared.UpdateBatterySerialumber();
//                var batterySuspendTimeout = Shared.Device.GetBatterySuspendTimeout();
//                Shared.SaveToLogStartProgramInformation(@"BatterySuspendTimeout " + batterySuspendTimeout.ToString());
//                Shared.SaveToLogStartProgramInformation(@"BatteryLevel " + Shared.Device.GetBatteryLevel().ToString());
//                Shared.SaveToLogStartProgramInformation(@"WiFiPowerStatus " + Shared.Device.GetWiFiPowerStatus().ToString());
//                if (batterySuspendTimeout != 600)
//                    Shared.SaveToLogStartProgramInformation(@"SetBatterySuspendTimeout(600) " + Shared.Device.SetBatterySuspendTimeout(600).ToString());
                
//                UInt64 userFreeBytes, totalDiskBytes, totalFreeBytesExecutable, totalFreeBytesUpdatable;
//                GetDiskFreeSpaceEx(@"\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesExecutable);
//                GetDiskFreeSpaceEx(@"\FlashDisk\", out userFreeBytes, out totalDiskBytes, out totalFreeBytesUpdatable);
//                Shared.SaveToLogStartProgramInformation(@"FreeSpace " + totalFreeBytesExecutable.ToString() + @"/" +
//                    totalFreeBytesUpdatable.ToString());
//            }
//            catch
//            {
//                Shared.SaveToLogError(@"Error LoginFormLoad");
//            }

//            ////Подписка на событие восстановления связи
//            //ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
//            ////Подписка на событие потери связи
//            //ConnectionState.OnConnectionLost += ConnectionLost;

//            ConnectionState.CheckConnection(Settings.CurrentServer);
//            if (Shared.TimerForCheckBatteryLevel == null)
//            {
//                Shared.SaveToLogError(@"Внимание! Не запущена автоматическая проверка уровня заряда аккумулятора.");
//            }

//#if !DEBUG
//            if (Shared.TimerForCheckUpdateProgram == null)
//            {
//                Shared.SaveToLogError(@"Внимание! Не запущена автоматическая проверка на наличие новой версии программы.");
//            }
//#endif

        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
            //ConnectionState.OnConnectionRestored -= ConnectionRestored;
            //ConnectionState.OnConnectionLost -= ConnectionLost;
        }

        //private void ConnectionLost()
        //{
        //    ConnectionLost(string.Empty);
        //}

        //private void ConnectionRestored()
        //{
        //    ConnectionRestored(string.Empty);            
        //}

        private void ConnectionLost(string message)
        {
            SetLblMessageText(message == string.Empty ? "Связь потеряна! \r\n\r\nНайдите зону с устойчивой связью" : message);
            //Invoke((LoginStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection, message });
        }

        private void ConnectionRestored(string message)
        {
            SetLblMessageText(message == string.Empty ? "Связь восстановлена! \r\n\r\nПросканируйте \r\nсвой штрих-код" : message);
            //Invoke((LoginStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore, message });
            ////Invoke(new EventHandler(ConnectionRestored));
        }

        //private void ShowConnection(ConnectState conState, string message)
        //{
        //    switch (conState)
        //    {
        //        //case ConnectState.ConInProgress:
        //        //    imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
        //        //    break;
        //        //case ConnectState.NoConInProgress:
        //        //    imgConnection.Image = null;
        //        //    break;
        //        case ConnectState.NoConnection:
        //            lblMessage.Text = message == string.Empty ? "Связь потеряна! \r\n\r\nНайдите зону с устойчивой связью" : message ;
        //            imgConnection.Image = ImgList.Images[(int)Images.NetworkOffline];
        //            break;
        //        case ConnectState.ConnectionRestore:
        //            lblMessage.Text = message == string.Empty ? "Связь восстановлена! \r\n\r\nПросканируйте \r\nсвой штрих-код" : message;
        //            imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
        //            //ConnectionRestored();
        //            break;
        //    }
        //    if (conState == ConnectState.NoConnection && message == string.Empty)
        //        Shared.SaveToLogInformation(message);
        //}

        private void SetLblMessageText(string text)
        {
            Invoke((MethodInvoker)(() => lblMessage.Text = text));
        }

        private void AuthorizeByBarcode(string barcode)
        {
            SetLblMessageText(barcode + Environment.NewLine +
                    "\r\n\r\nИдет проверка сети...");
            if (!ConnectionState.CheckConnection(Settings.CurrentServer))
            {
                ConnectionError();
                return;
            }
            if (!ConnectionState.GetServerPortEnabled)
            {
                //ConnectionState.ConnectionLost();
                this.ConnectionLost(); 
                return;
            }
#if !DEBUG
            switch (Db.CheckSqlConnection())
            {
                case 2:
                    ConnectionError();
                    return;
                case 1:
                    WrongUserPass();
                    return;
            }
#endif                 

            Person person = Db.PersonByBarcode(barcode);
            if (person == null)
            {
                //MessageBox.Show(@"Неверный шк или нет связи с базой");
                if (ConnectionState.IsConnected)
                    ConnectionLost(@"Неверный шк");
                else 
                    ConnectionLost(@"Нет связи с базой данных");
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
            Shared.IsAvailabilityCreateNewPalletNotOnOrder = (person.b2 ?? false);
            Shared.IsAvailabilityChoiseNomenclatureForMovingGroupPack = (person.b3 ?? false);
            Shared.IsNotUpdateCashedBarcodesOnFirst = (person.b4 ?? false);
            if (person.UserName.Contains("0"))
            {
               using (var form = new ChooseShiftDialog())
                {
#if !DEBUG
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
#endif
#if DEBUG
                    Shared.ShiftId = 1;
                    Settings.UserName = person.UserName.Replace("0", Shared.ShiftId.ToString());
                    Settings.Password = "123456";
#endif
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
                    //ConnectionState.IsConnected = true;
                    break;
            }

            Shared.PersonId = person.PersonID;
            Shared.PersonName = person.Name;
            Shared.PlaceId = person.PlaceID;
            Shared.VisibledButtonsOnMainWindow = (VisibleButtonsOnMainWindow)person.i1;
            var ii = (int)(VisibleButtonsOnMainWindow.btnExtAccept
                    | VisibleButtonsOnMainWindow.btnDocOrder
                    | VisibleButtonsOnMainWindow.btnDocTransfer
                    | VisibleButtonsOnMainWindow.btnDocMovement
                    //| VisibleButtonsOnMainWindow.btnCloseShift
                    | VisibleButtonsOnMainWindow.btnCloseApp
                    | VisibleButtonsOnMainWindow.btnInfoProduct
                    | VisibleButtonsOnMainWindow.btnInventarisation
                    );


            SetLblMessageText("Вы авторизовались " + Environment.NewLine + "как " + person.Name + " (" + Settings.UserName + ")" +
                    "\r\n\r\nИдет загрузка данных...");
            //Shared.LastTimeBarcodes1C = Db.GetServerDateTime();
            //Shared.Barcodes1C = Db.GetBarcodes1C();
            Cursor.Current = Cursors.WaitCursor;
            SetLblMessageText(("\r\n\r\nИдет выгрузка\r\nлогов на сервер..."));
            Shared.DeleteOldUploadedToServerLogs();
            var res = true;
            SetLblMessageText(("\r\n\r\nИдет загрузка\r\nштрих-кодов с сервера..."));
            res = res && Shared.Barcodes1C != null;
            SetLblMessageText(("\r\n\r\nИдет загрузка\r\nскладов с сервера..."));
            res = res && Shared.Warehouses != null;
            SetLblMessageText(("\r\n\r\nИдет загрузка\r\nзон склада с сервера..."));
            res = res && Shared.PlaceZones != null;
            SetLblMessageText(("\r\n\r\nОбновление\r\nштрих-кодов с сервера..."));
            res = res && Shared.Barcodes1C.UpdateBarcodes(true) != null;
            SetLblMessageText(("\r\n\r\nИдет загрузка\r\nмак. % брака с сервера..."));
            res = res && Shared.MaxAllowedPercentBreak != null;
            SetLblMessageText(("\r\n\r\nИдет загрузка периода\r\nзагрузки ШК с сервера..."));
            res = res && Shared.TimerPeriodForBarcodesUpdate != null;
            SetLblMessageText(("\r\n\r\nИдет загрузка периода\r\nпопыток выгрузки на сервера..."));
            res = res && Shared.TimerPeriodForUnloadOfflineProducts != null;
            SetLblMessageText(("\r\n\r\nИнициализация\r\nсканирования на ТСД..."));
            res = res && Shared.ScannedBarcodes != null;
            SetLblMessageText(("Загрузка закончена"));
            if (!res)//!InitializationData())                
            {
                SetLblMessageText("Просканируйте \r\nсвой штрих-код");
                ConnectionError();
                return;
            }
            //Thread.Sleep(3000);
            Invoke((MethodInvoker)(CloseForm));
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
            var res = Shared.ExecRDP();
            if (res == null)
            {
                SetLblMessageText(@"RDP не запущен. Файл cerdisp.exe не найден.");                
            }
            else if ((bool)res)
            {
                SetLblMessageText("RDP запущен");
                btnExecRDP.Text = "Останов RDP";
            }
            else
            {
                SetLblMessageText("RDP остановлен");
                btnExecRDP.Text = "Запуск RDP";
            }
            /*
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
            }*/
        }

        /*private static OpenNETCF.ToolHelp.ProcessEntry GetProcessRunning(string processsName)
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

        }*/

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Db.AddMessageToLog("btnHelp_Click");
            if (!initConnectionStarting)
                InitConnection(null);
            if (btnHelp.Text == "Сеть")
            {
                btnHelp.Text = "Скрыть";
                lblMessage.Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Regular);
                SetLblMessageText("Версия: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine + "Сервер: " + Settings.CurrentServer + Environment.NewLine + "БД/логин: " + Settings.Database + "/" + Settings.UserName);
                SetLblMessageText(lblMessage.Text + Environment.NewLine + "БД ШК создан:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture));
                btnExecRDP.Visible = true;
                btnTestPing.Visible = true;
                btnTestSQL.Visible = true;
                btnTestWiFi.Visible = true;
                btnSetExternalNet.Visible = true;
                btnSetInternalNet.Visible = true;
                btnUpdateProgram.Visible = true;
                if (Shared.Device is DeviceCipherlab)
                {
                    btnExit.Location = new System.Drawing.Point(btnExit.Left, Height + btnExit.Height + 5);
                    btnExit.Visible = true;
                }
                RefreshBtnSetNetEnabled();
                SetLblMessageText(lblMessage.Text + Environment.NewLine + "IsConnected " + ConnectionState.IsConnected);
                SetLblMessageText(lblMessage.Text + Environment.NewLine + "Имя ТСД: " + Shared.Device.GetHostName() + " (s/n " + Shared.Device.GetDeviceName() + ")" );
                SetLblMessageText(lblMessage.Text + Environment.NewLine + "IP адрес " + Shared.Device.GetDeviceIP());
            }
            else
            {
                btnHelp.Text = "Сеть";
                lblMessage.Font = new System.Drawing.Font("Tahoma", 14, System.Drawing.FontStyle.Regular);
                SetLblMessageText("Просканируйте \r\nсвой штрих-код");
                btnExecRDP.Visible = false;
                btnTestPing.Visible = false;
                btnTestSQL.Visible = false;
                btnTestWiFi.Visible = false;
                btnSetExternalNet.Visible = false;
                btnSetInternalNet.Visible = false;
                btnUpdateProgram.Visible = false;
                btnExit.Visible = false;
                try
                {
                    var cerdispProcess = Shared.GetProcessRunning(@"cerdisp");
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
            if (Shared.Device.GetWiFiPowerStatus())
            {
                uint quality;
                if (Shared.Device.WiFiGetSignalQuality(out quality))
                {
                    SetLblMessageText("WiFi уровень сигнала " + quality.ToString());
                    if (quality < 10)
                    {
                        SetLblMessageText(lblMessage.Text + " (низкий)");
                    }
                }
                else
                {
                    SetLblMessageText("WiFi ошибка при проверке уровня сигнала");
                }
            }
            else
            {
                SetLblMessageText("WiFi сигнал отсутствует");
            }
        }

        private void btnTestPing_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            var ServerIp = ConnectionState.GetServerIp();
            if (ServerIp == "")
            {
                SetLblMessageText("Не определен IP сервера");
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
                            SetLblMessageText("Сервер "+ServerIp+" не пингуется c таймаутом 200 мс");
                            reply = pinger.Send(ServerIp, 400);
                            if (reply.Status != IPStatus.Success)
                            {
                                SetLblMessageText(lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 400 мс");
                                reply = pinger.Send(ServerIp, 800);
                                if (reply.Status != IPStatus.Success)
                                {
                                    SetLblMessageText(lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 800 мс");
                                    reply = pinger.Send(ServerIp, 1600);
                                    if (reply.Status != IPStatus.Success)
                                    {
                                        SetLblMessageText(lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " не пингуется c таймаутом 1600 мс");
                                    }
                                }
                            }
                        }
                        if (reply.Status == IPStatus.Success)
                        {
                            SetLblMessageText(lblMessage.Text + Environment.NewLine + "Сервер " + ServerIp + " пингуется "+reply.ToString());
                        }
                    }
                    catch (Exception ex) 
                    {
                        SetLblMessageText(lblMessage.Text + Environment.NewLine + "Ошибка при пинге сервера " + ServerIp + ": " + ex.ToString());
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
                        ConnectionLost();
                        SetLblMessageText("Нет связи с БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        SetLblMessageText("Неверно указан логин или пароль к БД " + Settings.CurrentServer + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    default:
                        SetLblMessageText("Соединение с БД " + Settings.CurrentServer + " установлено" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                }
            }
            else
            {
                SetLblMessageText("Нет связи с БД " + Settings.CurrentServer + ". Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
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
                SetLblMessageText(barcode);
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
            if (Shared.ShowMessageQuestion("Вы уверены, что хотите изменить адрес сервера?") == DialogResult.Yes)
            {
                if ((!isChangeExternalServer) ? Settings.SetCurrentInternalServer() : Settings.SetCurrentExternalServer())
                {
                    RefreshBtnSetNetEnabled();
                    SetLblMessageText("Установлен адрес сервера " + Settings.CurrentServer);
                    Shared.SaveToLogInformation(lblMessage.Text);
                }
                else
                {
                    SetLblMessageText("Ошибка при установке адреса сервера " + (!isChangeExternalServer ? Settings.ServerIP : Settings.SecondServerIP));
                    Shared.SaveToLogError(lblMessage.Text);
                }
            }
        }

        public void RefreshBtnSetNetEnabled()
        {
            btnSetExternalNet.Enabled = Settings.CurrentServer == Settings.ServerIP;
            btnSetInternalNet.Enabled = Settings.CurrentServer == Settings.SecondServerIP;
        }

        private void btnUpdateProgram_Click(object sender, EventArgs e)
        {
            UpdateProgram.DropFlagUpdateLoading();
            UpdateProgram.LoadUpdate(new object());
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}