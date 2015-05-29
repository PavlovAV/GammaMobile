using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using OpenNETCF.Net.NetworkInformation;
using Datalogic.API;

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

        static private string _gammaIp = "Gamma";
        public delegate void MethodContainer();
        static public event MethodContainer OnConnectionRestored;

        static private readonly object Locker = new object();
//        static private DateTime TimePingChecked = new DateTime();

        static public Boolean CheckConnection()
        {
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
                            if (_gammaIp == "Gamma")
                            {
                                try
                                {
                                    IPAddress[] ipAddresses = Dns.GetHostEntry("Gamma").AddressList;
                                    _gammaIp = ipAddresses[0].ToString();
                                }
                                catch (SocketException)
                                {
                                        
                                }
                            }
                            PingReply reply = pinger.Send(_gammaIp, 200);
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
                        var reply = newPinger.Send(_gammaIp, 200);
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
    }
}
