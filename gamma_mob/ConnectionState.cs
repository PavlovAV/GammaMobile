﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using OpenNETCF.Net.NetworkInformation;
using gamma_mob.Common;
using System.Data.SqlClient;

namespace gamma_mob
{
    internal static class ConnectionState
    {
        public delegate void MethodContainer(bool isConnected);

        private static bool _isConnected;
        private static bool _checkerRunning;
        public static bool GetCheckerRunning
        { get { return _checkerRunning; } }

        private static string _serverIp = "";
        private static string _serverPort = "1433";
        private static readonly object Locker = new object();
        private static readonly object LockerCurrentConnectionState = new object();

        private static bool currentShowConnectionState;

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
                    Shared.SaveToLogInformation(value);
            }
        }

        static ConnectionState()
        {
        }

        public static bool IsConnected
        {
            get { return _isConnected; }
            private set 
            {
                if (ipAddress == null && value && !_isConnected)
                {
                    try
                    {
                        //IPHostEntry ipHostEntry = Dns.Resolve(ServerIp);
                        ipAddress = IPAddress.Parse(ServerIp); //ipHostEntry.AddressList[0];
                        iPEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(ServerPort));
                    }
                    catch (Exception ex)
                    {
#if OUTPUTDEBUGINFO
                        System.Diagnostics.Debug.WriteLine("EXCEPTION IsConnected:" + Environment.NewLine + ex.Message);
#endif
                    }
                }
                //if (!value && _isConnected)
                //{ 
                //    if (OnConnectionLost != null) OnConnectionLost(); 
                //}
                //else if (value && !_isConnected)
                //{ 
                //    if (OnConnectionRestored != null) OnConnectionRestored(); 
                //}
                if (value != _isConnected)
                    if (OnConnectionStateChanged != null) OnConnectionStateChanged(value); 
                _isConnected = value;
                
            }
        }

        public static ConnectState CurrentConnectionState { get; set; }

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

        //public static event MethodContainer OnConnectionRestored;
        //public static event MethodContainer OnConnectionLost;
        public static event MethodContainer OnConnectionStateChanged;

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
#if DEBUG && !ASRELEASE
           return true;
#endif
           if (!Shared.Device.GetWiFiPowerStatus())
            {
                IsConnected = false; 
                return false;
            }
            else
            {
                uint quality;
                if (Shared.Device.WiFiGetSignalQuality(out quality))
                {
                    if (quality < 24)
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
                //Db.AddTimeStampToLog("Start GetServerPortEnabled");
                //if (Settings.GetCurrentServerIsExternal())
                //    return true;
                //else
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
                        //if (Shared.Connection.State == System.Data.ConnectionState.Closed)
                        //    Shared.Connection.Open();
                        //Db.AddTimeStampToLog("SEnd GetServerPortEnabled");

                        return true;
                    }
                    catch (Exception ex)
                    {
#if OUTPUTDEBUGINFO
                        System.Diagnostics.Debug.WriteLine("EXCEPTION GetServerPortEnabled:" + Environment.NewLine + ex.Message);
#endif
                        Shared.LastQueryCompleted = false;
                        return false;
                    }
                //Db.AddTimeStampToLog("End GetServerPortEnabled");
            }
        }

        private static TcpClient TcpClient = new TcpClient();
        private static Socket client = TcpClient.Client;
        private static IAsyncResult result;
        private static bool _checkConnectionAvialabledRunning;
        private static DateTime lastCheckOpennedConnection { get; set; }
        private static Ping _pinger { get; set; }
        private static Ping pinger 
        { 
            get
            {
                if (_pinger == null) _pinger = new Ping();
                return _pinger;
            }
        }


        public static void CheckConnectionAvialabled(object obj)
        {
#if OUTPUTDEBUGINFO
            //var n = DateTime.Now.ToString();
            //System.Diagnostics.Debug.Write(n + " !!!!!CheckConnectionAvialabledFromTimer(" + _checkConnectionAvialabledRunning.ToString() + ")!" + Environment.NewLine);
#endif
            if (_checkConnectionAvialabledRunning) return;
            lock (lockerCheckConnectionAvialabled)
            {
                _checkConnectionAvialabledRunning = true;
#if OUTPUTDEBUGINFO
                //System.Diagnostics.Debug.Write(n + " !!!!!CheckConnectionAvialabled(" + _checkConnectionAvialabledRunning.ToString() + ")!" + Environment.NewLine);
#endif
                //using (var connection = new SqlConnection(Db.GetConnectionString()))
                {
                    try
                    {
#if OUTPUTDEBUGINFO
                        //System.Diagnostics.Debug.Write("Connection!" + Environment.NewLine);
#endif
                        if (Shared.ConnectionCheckStatus.State != System.Data.ConnectionState.Open)
                        {
                            if (Settings.GetCurrentCheckConnectionMethod(CheckConnectionMethod.Ping.ToString())) 
                                pinger.Send(ServerIp, 250); 
                            Shared.ConnectionCheckStatus.Open();
                            lastCheckOpennedConnection = DateTime.Now;
                            //if (client.Connected)
                            //    client.Close();
                            //connection.Close();
                        }
                        else if (Shared.ConnectionCheckStatus.State == System.Data.ConnectionState.Open && !IsConnected)
                        {
                            if (Settings.GetCurrentCheckConnectionMethod(CheckConnectionMethod.Ping.ToString())) 
                                pinger.Send(ServerIp, 250); 
                            Shared.ConnectionCheckStatus.Close();
                            Shared.ConnectionCheckStatus.Open();
                            lastCheckOpennedConnection = DateTime.Now;
                            //if (client.Connected)
                            //    client.Close();
                        }
                        else if (Shared.ConnectionCheckStatus.State == System.Data.ConnectionState.Open && IsConnected)
                        {
                            if (Settings.GetCurrentCheckConnectionMethod(CheckConnectionMethod.Ping.ToString()))
                            {
                                if ((DateTime.Now - lastCheckOpennedConnection).Seconds >= 2)
                                {
                                    pinger.Send(ServerIp, 250);
                                    lastCheckOpennedConnection = DateTime.Now;
                                    if (!currentShowConnectionState)
                                        if (OnConnectionStateChanged != null) OnConnectionStateChanged(true);
                                }
                            }
                            else if ((DateTime.Now - lastCheckOpennedConnection).Seconds >= 5)
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
                                lastCheckOpennedConnection = DateTime.Now;
                                if (!currentShowConnectionState)
                                    if (OnConnectionStateChanged != null) OnConnectionStateChanged(true);
                            }

                        }
                        //else if (Shared.Connection.State == System.Data.ConnectionState.Open && !Shared.LastQueryCompleted)
                        //{
                        //    Shared.Connection.
                        //}
                        //if (client.Connected)
                        //{
                        //    try
                        //    {
                        //        //client.Blocking = false;
                        //        client.SendTo(new byte[1], iPEndPoint);
                        //    }
                        //    catch (SocketException exConnect)
                        //    {
                        //        if (exConnect.ErrorCode == 10054)
                        //        {
                        //            //client.Close();
                        //            client = TcpClient.Client;
                        //        }
                        //    }
                        //}
                        //if (!client.Connected)
                        //{
                        //    result = client.BeginConnect(iPEndPoint, null, client);
                        //    var success = result.AsyncWaitHandle.WaitOne(100, false);
                        //    if (!success)
                        //    {
                        //        throw new Exception("Failed to connect.");
                        //    }
                        //}

                        //if (!IsConnected)
                        //{
                        //    lock (Locker) IsConnected = true;
                        //    if (CurrentConnectionState != ConnectState.ConnectionRestore && OnConnectionRestored != null)
                        //        OnConnectionRestored();
                        //}
                        //else
                        //{
                        //    if (CurrentConnectionState == ConnectState.ConnectionRestore && OnConnectionLost != null)
                        //        OnConnectionLost();
                        //}
                        ConnectionStateChange(true);
                        //isWorking = false;
                        //if (OnConnectionRestored != null) OnConnectionRestored();

                    }
                    catch (Exception ex)
                    {
#if OUTPUTDEBUGINFO
                        System.Diagnostics.Debug.WriteLine("EXCEPTION CheckConnectionAvialabled:" + Environment.NewLine + ex.Message);
#endif

                        //if (Shared.Connection.State == System.Data.ConnectionState.Open)
                        //    StopChecker();
                        var s = ex.Message;
                        //if (IsConnected)
                        //{
                        //    lock (Locker) IsConnected = false;
                        //    //if (OnConnectionLost != null) OnConnectionLost();
                        //}
                        ConnectionStateChange(false);
                    }
                }
            }
            _checkConnectionAvialabledRunning = false;
        }

        public static object lockerCheckConnectionAvialabled = new object();

        private static System.Threading.Timer _timerForCheckConnectionAvialabled { get; set; }
        public static System.Threading.Timer TimerForCheckConnectionAvialabled
        {
            get
            {

                if (_timerForCheckConnectionAvialabled == null)
                {
                    int num = 0;
                    TimerCallback tm = new TimerCallback(CheckConnectionAvialabled);
                    // создаем таймер
                    _timerForCheckConnectionAvialabled = new System.Threading.Timer(tm, num, 0, 100);
                    _checkerRunning = true;
                }
                return _timerForCheckConnectionAvialabled;
            }

        }

        public static void ConnectionStateChange(bool isConnected)
        {
            if (isConnected != ConnectionState.IsConnected)
            {
                //lock (LockerCurrentConnectionState) 
                    ConnectionState.IsConnected = isConnected;
            }
            //else
            //    if (OnConnectionLost != null) OnConnectionLost(); 
        }

        //public static void ConnectionLost()
        //{
        //    if (ConnectionState.IsConnected)
        //        //lock (Locker) 
        //            ConnectionState.IsConnected = false;
        //    else
        //        if (OnConnectionLost != null) OnConnectionLost(); 
        //}

        public static void SetCurrentShowConnectionState(bool isConnectionState)
        {
            currentShowConnectionState = isConnectionState;
        }

        public static void StopChecker()
        {
            //if (!ConnectionState.IsConnected) lock (Locker) ConnectionState.IsConnected = true;
            if (_timerForCheckConnectionAvialabled != null && _checkerRunning)
            {
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.WriteLine("_timerForCheckConnectionAvialabled.Dispose");
#endif
                _timerForCheckConnectionAvialabled.Change(Timeout.Infinite, Timeout.Infinite);
                // if (TimerForCheckConnectionAvialabled != null) 
                //_timerForCheckConnectionAvialabled.Dispose();
                _checkerRunning = false;
            }
        }

        public static void StartChecker()
        {
            //if (ConnectionState.IsConnected) lock (Locker) ConnectionState.IsConnected = false;
            if (_timerForCheckConnectionAvialabled == null || !_checkerRunning)//!_checkConnectionAvialabledRunning)
            {
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.WriteLine("TimerForCheckConnectionAvialabled.Create");
#endif
                //var t = TimerForCheckConnectionAvialabled;
                //_checkerRunning = true;
                _timerForCheckConnectionAvialabled.Change(0, 100);
            }
            /*System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!StartChecker(" + _checkerRunning.ToString() + ")!"+Environment.NewLine);
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
                    Thread.Sleep(100);
                }
                lock (Locker) _checkerRunning = false;
            };
            ThreadPool.QueueUserWorkItem(continuousPing);
            //var pinger = new Thread(new ThreadStart(continuousPing)) {IsBackground = true};
            //pinger.Start();
            _checkerRunning = true;
            */
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