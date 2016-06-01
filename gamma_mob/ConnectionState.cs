using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using OpenNETCF.Net.NetworkInformation;
using Datalogic.API;
using System.Text.RegularExpressions;

namespace gamma_mob
{
    static class ConnectionState
    {
        static private bool _isConnected;
        static private bool _checkerRunning;
        private static bool IsConnected 
        {
            get {return _isConnected;}
            set { _isConnected = value; }
        }

        private static string ServerIp
        {
            get { return _serverIp; }
            set { _serverIp = value; }
        }

        private static string _serverIp = "";
        public delegate void MethodContainer();
        static public event MethodContainer OnConnectionRestored;

        static private readonly object Locker = new object();
//        static private DateTime TimePingChecked = new DateTime();

        static public Boolean CheckConnection()
        {
            if (ServerIp == "")
            {
                if (!GetIpFromSettings(Settings.ServerIP)) return false;
            }
            if (Device.GetWiFiPowerStatus())
            {
                uint quality;
                if (Device.WiFiGetSignalQuality(out quality))
                {
                    if (quality < 3)
                    {
                        IsConnected = false;
                        return false;
                    }
  
                    using (var pinger = new Ping())
                    {
                        try
                        {
                            PingReply reply = pinger.Send(ServerIp, 200);
                            if (reply.Status == IPStatus.Success)
                            {
                                IsConnected = true;
                                return true;
                            }
                            IsConnected = false;
                            return false;
                        }
                        catch (PingException)
                        {
                            IsConnected = false;
                            return false;
                        }
                    }
                }
                IsConnected = false;
                return false;
            }
            return false; 
        }

        static ConnectionState()
        {
            
        }

        static public void StartChecker()
        {
            if (_checkerRunning) return;
            WaitCallback continuousPing = delegate
            {
                var newPinger = new Ping();
                var isWorking = true;
                while (isWorking)
                {

                    try
                    {
                        var reply = newPinger.Send(ServerIp, 200);
                        if (reply.Status == IPStatus.Success && !_isConnected)
                        {
                            lock (Locker) IsConnected = true;
                            if (OnConnectionRestored != null) OnConnectionRestored();
                            isWorking = false;
                        }
                        else if (reply.Status != IPStatus.Success && _isConnected)
                        {
                            lock (Locker) IsConnected = false;
                        }
                    }
                    catch (PingException)
                    {
                        if (IsConnected)
                        {
                            lock (Locker) IsConnected = false;
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
        private static bool GetIpFromSettings(string server)
        {    
            string ippattern = @"^(?<ip>((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))(\\.*)?$";
            var regEx = new Regex(ippattern);
            var match = regEx.Match(server);
            if (match.Success)
            {
                ServerIp = match.Groups["ip"].Value;
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
            return true;
        }
    }
}
