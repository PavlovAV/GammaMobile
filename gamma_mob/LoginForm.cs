using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.IO;
using System.Reflection;
using Datalogic.API;
using OpenNETCF.Net.NetworkInformation;
using Microsoft.Win32;
using System.Net;
using System.Runtime.InteropServices;

namespace gamma_mob
{
    
    public partial class LoginForm : BaseForm
    {
        public LoginForm()
        {
            InitializeComponent();
            if (!ConnectionState.CheckConnection()) return;
            if (Db.CheckSqlConnection() != 1) return;
            WrongUserPass();
        }

        private string barcode;

        [DllImport("coredll.dll", SetLastError = true)]
        private extern static uint SetSystemTime(ref SystemTime lpSystemTime);

        protected static void SetSystemDateTime(DateTime dt)
        {
            SystemTime systime = new SystemTime
            {
                wYear = (ushort)dt.Year,
                wMonth = (ushort)dt.Month,
                wDay = (ushort)dt.Day,
                wHour = (ushort)dt.Hour,
                wMinute = (ushort)dt.Minute,
                wSecond = (ushort)dt.Second,
                wMilliseconds = (ushort)dt.Millisecond,
                wDayOfWeek = (ushort)dt.DayOfWeek
            };
            SetSystemTime(ref systime);
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = AuthorizeByBarcode;
            try
            {
                var cerdispProcess = GetProcessRunning(@"cerdisp");
                if (cerdispProcess != null)
                {
                    btnExecRDP.Text = "Останов RDP";
                }
                else
                {
                    btnExecRDP.Text = "Запуск RDP";
                }

                if (Db.CheckSqlConnection() != 1)
                {
                    DateTime serverDateTime = Db.GetServerDateTime();
                    if (serverDateTime != null)
                    {
                        SetSystemDateTime(serverDateTime);
                    }
                }
            }
            catch
            {
            }
        }

        private void AuthorizeByBarcode(string barcode)
        {
            if (Db.CheckSqlConnection() == 1)
            {
                MessageBox.Show(@"Нет связи с базой!" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            Person person = Db.PersonByBarcode(barcode);
            if (person == null)
            {
                MessageBox.Show(@"Неверный шк или нет связи с базой");
                return;
            }
            Shared.PersonId = person.PersonID;
            Invoke(
                (MethodInvoker)
                (() => lblMessage.Text = "Вы авторизовались " + Environment.NewLine + "как " + person.Name));
            Thread.Sleep(3000);
            Invoke((MethodInvoker) (CloseForm));
        }

        private void CloseForm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void WrongUserPass()
        {
            MessageBox.Show(@"Неверно указан логин или пароль в настройках. Обратитесь к администратору приложения"
                            , @"Ошибка связи с БД",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        }

        private void btnExecRDP_Click(object sender, EventArgs e)
        {
            var cerdispProcess = GetProcessRunning(@"cerdisp");
            if (cerdispProcess == null)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CERDISP", true);
                var serverIP = ConnectionState.GetServerIp();
                var hostname = (string)reg.GetValue("Hostname", "");
                if (serverIP != "" && (string)reg.GetValue("Hostname", "") != serverIP)
                {
                    reg.SetValue("Hostname", serverIP, RegistryValueKind.String);
                }
                var hostname1 = (string)reg.GetValue("Hostname", "");
                
                System.Diagnostics.Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                           @"\cerdisp.exe", "-c");
                lblMessage.Text = "RDP запущен по адресу " + serverIP;
                btnExecRDP.Text = "Останов RDP";
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
                lblMessage.Text = "Сервер: " + Settings.ServerIP + Environment.NewLine + "База данных: " + Settings.Database + Environment.NewLine + "Логин: " + Settings.UserName;
                btnExecRDP.Visible = true;
                btnTestPing.Visible = true;
                btnTestSQL.Visible = true;
                btnTestWiFi.Visible = true;
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
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        lblMessage.Text = "Нет связи с БД " + Settings.ServerIP + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                    case 1:
                        lblMessage.Text = "Неверно указан логин или пароль к БД " + Settings.ServerIP + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                    default:
                        lblMessage.Text = "Соединение с БД " + Settings.ServerIP + " установлено" + Environment.NewLine + ConnectionState.GetConnectionState();
                        break;
                }
            }
            else
            {
                lblMessage.Text = "Нет связи с БД " + Settings.ServerIP + ". Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState();
            }
            Cursor.Current = Cursors.Default;
        }

        private void SetBarcode(char keyChar)
        {
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
    }
}