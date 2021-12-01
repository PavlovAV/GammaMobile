using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Datalogic.API;
using OpenNETCF.Net.NetworkInformation;
using gamma_mob.Common;
using System.Data.SqlClient;

namespace gamma_mob
{
    internal static class ConnectionState
    {
        public delegate void MethodContainer();

        private static bool _isConnected;
        private static bool _checkerRunning;
        public static bool GetCheckerRunning
        { get { return _checkerRunning; } }

        private static string _serverIp = "";
        private static string _serverPort = "1433";
        private static readonly object Locker = new object();

        private static string _stateConnection { get; set; }
        private static string stateConnection 
        {
            get
            {
                return _stateConnection;
            }
            set
            {
                _stateConnection = value;
                if (value != null && value != String.Empty)
                    Shared.SaveToLog(value);
            }
        }

        static ConnectionState()
        {
        }

        public static bool IsConnected
        {
            get { return _isConnected; }
            set 
            {
                if (ipAddress == null && value && !_isConnected)
                {
                    try
                    {
                        //IPHostEntry ipHostEntry = Dns.Resolve(ServerIp);
                        ipAddress = IPAddress.Parse(ServerIp); //ipHostEntry.AddressList[0];
                        iPEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(ServerPort));
                    }
                    catch (Exception)
                    { }
                }
                _isConnected = value;
                
            }
        }

        public static string ServerIp
        {
            get { return _serverIp; }
            set { _serverIp = value; }
        }

        public static string ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }
        public static string GetConnectionState()
        {
            return stateConnection;
        }

        public static event MethodContainer OnConnectionRestored;
        public static event MethodContainer OnConnectionLost;


        //        static private DateTime TimePingChecked = new DateTime();
        public static Boolean CheckConnection(string server)
        {
            if (!GetIpFromSettings(server))
                {
                    stateConnection = @"Не определен IP сервера";
                    IsConnected = false;
                    return false;
                }
            return CheckConnection();
        }

        public static Boolean CheckConnection()
        {
           if (ServerIp == "")
            {
             /*   if (!GetIpFromSettings(Settings.CurrentServer))
                {
                    stateConnection = @"Не определен IP сервера";
                    IsConnected = false;
                    return false;
                }*/
            }
            if (!Device.GetWiFiPowerStatus())
            {
                IsConnected = false; 
                return false;
            }
            else
            {
                uint quality;
                if (Device.WiFiGetSignalQuality(out quality))
                {
                    if (quality < 8)
                    {
                        stateConnection = @"Низкий уровень сигнала";
                        IsConnected = false;
                        return false;
                    }
                    //Не пингуем, незачем
                    return true;
                    if (Settings.CurrentServer.IndexOf(@",") >= 0)
                    {
                        //IsConnected = true;
                        return true;
                    }
                    using (var pinger = new Ping())
                    {
                        try
                        {
                            PingReply reply = pinger.Send(ServerIp, 200);
                            if (reply.Status != IPStatus.Success)
                            {
                                reply = pinger.Send(ServerIp, 400);
                                if (reply.Status != IPStatus.Success)
                                {
                                    reply = pinger.Send(ServerIp, 800);
                                    if (reply.Status != IPStatus.Success)
                                    {
                                        reply = pinger.Send(ServerIp, 1600);
                                    }
                                }
                            }
                            if (reply.Status == IPStatus.Success)
                            {
                                IsConnected = true;
                                return true;
                            }
                            stateConnection = @"Сервер не пингуется";
                            IsConnected = false;
                            return false;
                        }
                        catch (PingException)
                        {
                            stateConnection = @"Ошибка при пинге сервера";
                            IsConnected = false;
                            return false;
                        }
                    }
                }
                stateConnection = @"Сигнал отсутствует";
                IsConnected = false;
                return false;
            }
            return false;
        }

        public static string GetServerIp()
        {
            if (ServerIp == "")
            {
                GetIpFromSettings(Settings.CurrentServer);
            }
            return ServerIp;
        }

        private static IPAddress ipAddress { get; set; }
        private static IPEndPoint iPEndPoint { get; set; }

        public static bool GetServerPortEnabled
        {
            get
            {
                try
                {
                    TcpClient TcpClient = new TcpClient();
                    var client = TcpClient.Client;
                    var result = client.BeginConnect(iPEndPoint, null, client);

                    var success = result.AsyncWaitHandle.WaitOne(1000, false);

                    if (!success)
                    {
                        throw new Exception("Failed to connect.");
                    }

                    // we have connected
                    client.EndConnect(result);
                    return true;
                }
                catch (Exception)
                {
                    Shared.LastQueryCompleted = false;
                    return false;
                }
            }
        }

        public static void StartChecker()
        {
            System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!StartChecker(" + _checkerRunning.ToString() + ")!"+Environment.NewLine);
            if (_checkerRunning) return;

            WaitCallback continuousPing = delegate
            {
                bool isWorking = true;
                while (isWorking)
                {
                    using (var connection = new SqlConnection(Db.GetConnectionString()))
                    {
                        try
                        {
                            System.Diagnostics.Debug.Write("Connection!" + Environment.NewLine);
                            connection.Open();
                            connection.Close();
                            
                            

                            if (!IsConnected)
                            {
                                lock (Locker) IsConnected = true;
                            }
                            isWorking = false;
                            if (OnConnectionRestored != null) OnConnectionRestored();
                            
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Write("Exception!" + Environment.NewLine);
                            if (IsConnected)
                            {
                                lock (Locker) IsConnected = false;
                                if (OnConnectionLost != null) OnConnectionLost();
                            }
                        }
                    }
                    Thread.Sleep(0);
                }
                lock (Locker) _checkerRunning = false;
            };
            ThreadPool.QueueUserWorkItem(continuousPing);
            //var pinger = new Thread(new ThreadStart(continuousPing)) {IsBackground = true};
            //pinger.Start();
            _checkerRunning = true;

        }

        public static bool GetIpFromSettings(string server)
        {
            string ippattern =
                //@"^(?<ip>((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))(\\.*)?$";
                @"^(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})(?<port>,\d{1,5})?$";
            var regEx = new Regex(ippattern);
            Match match = regEx.Match(server);
            if (match.Success)
            {
                ServerIp = match.Groups["ip"].Value;
                ServerPort = match.Groups["port"].Value == "" ? "1433" : match.Groups["port"].Value.Replace(",","");
                Db.SetConnectionString(server, Settings.Database, Settings.UserName, Settings.Password, Settings.TimeOut);
            }
            else
            {
                string namepattern = @"^(?<server>.*?)(?<base>\\.*)?$";
                regEx = new Regex(namepattern);
                match = regEx.Match(server);
                try
                {
                    IPAddress[] ipAddresses = Dns.GetHostEntry(match.Groups["server"].Value).AddressList;
                    ServerIp = ipAddresses[0].ToString();
                    string sqlserver;
                    if (match.Groups.Count > 1)
                    {
                        sqlserver = ServerIp + match.Groups["base"].Value;
                    }
                    else sqlserver = ServerIp;
                    Db.SetConnectionString(sqlserver, Settings.Database,
                                           Settings.UserName, Settings.Password, Settings.TimeOut);
                }
                catch (SocketException)
                {
                    IsConnected = false;
                    return false;
                }
            }
            ipAddress = IPAddress.Parse(ServerIp); //ipHostEntry.AddressList[0];
            iPEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(ServerPort));        
            return true;
        }
    }
}