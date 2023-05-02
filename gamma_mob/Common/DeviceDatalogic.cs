using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Datalogic.API;
using System.Net;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Windows.Forms;
using OpenNETCF.Net.NetworkInformation;
//using System.Management;

namespace gamma_mob.Common
{
    public class DeviceDatalogic : Device, IDeviceExtended
    {

        #region IDevice Members

        public override void EnableWiFi()
        {
            Datalogic.API.Device.SetWiFiPowerState(true);
        }

        public override string GetDeviceName()
        {
            return Datalogic.API.Device.GetSerialNumber();
        }

        public string GetDeviceIP()
        {
            IPHostEntry ipEntry = Dns.GetHostByName(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            return addr[0].ToString();
        }

        public bool GetWiFiPowerStatus()
        {
            return Datalogic.API.Device.GetWiFiPowerStatus();
        }

        public bool WiFiGetSignalQuality(out uint quality)
        {
            var onOff = Datalogic.API.Device.WiFiGetSignalQuality(out quality);
            quality = (quality * 3);
            return onOff;
        }

        public string GetModel()
        {
            return Datalogic.API.Device.GetModel().ToString();
        }

        #endregion

        #region IDevice Members


        public int GetBatterySuspendTimeout()
        {
            return Datalogic.API.Device.GetBatterySuspendTimeout();
        }

        #endregion

        #region IDevice Members


        public int GetBatteryLevel()
        {
            return Datalogic.API.Device.GetBatteryLevel();
        }

        #endregion

        #region IDevice Members


        public bool SetBatterySuspendTimeout(uint timeout)
        {
            return Datalogic.API.Device.SetBatterySuspendTimeout(timeout);
        }

        #endregion

        #region IDevice Members


        public string GetBatterySerialNumber()
        {
            Datalogic.API.Device.BatteryInfo bInfo;
            var res = Datalogic.API.Device.GetBatteryInfo(out bInfo);
            return (bInfo != null) ?
                bInfo.SerialNumber : String.Empty ;
        }

        public bool UpdateDeviceSettings(NameValueCollection m_settings)
        {
            reboot = false;

            foreach (var item in m_settings.Keys)
            {
                var r = item.ToString().Substring(0, 11);
                var m = Datalogic.API.Device.GetModelName();

                if ((item.ToString().Substring(0, 11) == "DatalogicX3" && Datalogic.API.Device.GetModelName() == "FalconX3+") || (item.ToString().Substring(0, 11) == "DatalogicX4" && Datalogic.API.Device.GetModelName() == "FalconX4")
                     || item.ToString().Substring(0, 9) == "dduSettin" || item.ToString().Substring(0, 9) == "dlbSettin" || item.ToString().Substring(0, 9) == "backlight")
                    try
                    {
                        var index = item.ToString().IndexOf(".");
                        var set = item.ToString().Substring(0, index);
                        var key = item.ToString().Replace(set + ".", "");
                        var l = key.IndexOf(".");
                        var keyName = key.Substring(0, l).Replace("___", " ").Replace("__", "\\").Replace("SkbOp", "{").Replace("SkbCl", "}");
                        var valueName = key.Substring(l + 1, key.Length - l - 1);
                        var val = m_settings.Get(item.ToString());
                        string regVal = String.Empty;
                        var a1 = (Registry.GetValue(keyName, "Size", "not exist") ?? String.Empty).ToString();
                        var a2 = m_settings.Get(item.ToString().Replace(".Data 0", ".Size"));
                        if (item.ToString().Substring(0, 11) == "DatalogicX4" && valueName == "Data 0")
                        {
                            if ((Registry.GetValue(keyName, "Size", "not exist") ?? String.Empty).ToString() == m_settings.Get(item.ToString().Replace(".Data 0", ".Size")))
                            {
                                regVal = val;
                            }
                            else
                            {
                                regVal = null;
                                reboot = true;
                            }
                        }
                        else
                        {
                            regVal = (Registry.GetValue(keyName, valueName, "not exist") ?? String.Empty).ToString();
                        }
                        /*
                              <HKEY_LOCAL_MACHINE__Comm__Connmgr__Settings__Connections__Gamma>
      <add key="Data 0" value="01,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,40,04,00,00,00,00,00,00,00,00,00,00,00,01,00,00,00,20,00,00,00,fd,24,3f,eb,79,85,76,bd,cc,80,21,c0,03,cf,c6,ee,6b,fc,c0,5d,f5,c1,4a,87,e8,57,3b,d7,3a,64,da,45,00,00,00,00,02,80,00,00,20,00,00,00,f0,b5,c1,2a,f5,95,89,fa,6b,33,29,33,b9,11,9a,c8,77,c4,4d,69,11,56,30,4b,24,0d,9c,2d,56,d2,38,5d,40,0b,00,00,d5,e2,fb,d9,8b,41,4b,8e,df,ef,ea,de,73,3c,7c,18,14,d0,4d,b1,9e,fa,c3,31,9c,ac,68,69,6b,29,b9,6b,5a,c5,cc,3d,cf,c1,eb,c8,4a,e9,38,2c,2c,4f,1b,22,16,2c,61,83,c6,55,62,2c,30,db,fc,a6,12,ce,66,44,c0,d2,72,fd,90,1f,ed,89,f1,c9,db,50,49,43,71,e9,25,43,38,a0,06,5d,68,f1,94,86,7c,30,74,51,46,4a,e9,5e,75,6c,4d,a2,af,55,6c,cb,28,cf,29,ff,8d,da,d3,e2,91,ab,e5,2d,fe,b8,e5,0f,2d,57,53,b1,f5,d1,c2,0d,30,79,e6,05,91,e7,b6,9a,38,2d,5a,14,ec,35,63,9f,7b,f2,ec,19,f2,6c,7c,1e,9a,5d,f2,cd,ef,58,b9,c3,5d,28,aa,e4,eb,40,8b,6a,8d,38,8b,58,5f,bc,\
      b3,79,f9,0f,4a,d6,0a,c3,a2,3c,1c,30,e5,cf,45,ef,73,00,ff,ce,35,f7,fa,4e,10,09,7a,75,41,99,b7,64,7a,e3,15,ed,f3,6e,a3,a3,14,8e,2b,0c,08,0e,b1,61,e2,b1,2e,bd,22,e4,0c,05,98,17,d8,65,69,21,c0,0e,75,dc,59,4f,e2,ed,a8,9b,7b,2f,61,d7,30,a3,e5,ef,4b,e4,21,a4,14,0c,de,c2,00,4c,bf,a7,ce,80,f4,44,16,1b,26,cf,99,ec,7d,ac,67,f9,ea,07,4f,67,20,ed,a7,4c,aa,ce,a2,fd,92,8f,b7,10,a6,fd,d5,85,bc,44,c9,54,0a,51,ab,a2,61,d4,d2,ab,65,d2,a3,1a,26,24,fb,4b,05,e1,cf,1c,d1,84,9c,3e,3b,e0,ac,9d,f0,e5,9a,a2,fb,e2,a7,3e,e3,53,9a,ba,cc,1d,6f,48,34,ee,19,80,0a,6c,ea,63,ac,fe,de,04,b5,b5,18,e9,9a,b2,78,ce,a2,ab,5c,2b,57,9e,bf,19,17,49,ea,77,09,29,3f,02,41,73,a7,a4,c8,c1,f6,2b,78,be,9c,73,2b,a7,ad,22,90,31,f6,9c,4d,b3,a2,29,fe,83,d9,e3,c9,14,91,68,f5,3a,2e,a9,d8,12,d8,4a,b9,02,35,33,5f,0e,39,c0,0f,b7,9c,9a,d5,46,4e,28,a4,88,cb,a0,04,88,e6,77,48,5a,69,66,84,02,e2,11,20,9c,c4,ad,56,fe,3b,79,a2,2f,aa,0e,86,71,c2,1f,4d,96,0d,39,c5,5e,e8,58,3d,c1,3c,a6,ac,14,eb,bc,c7,cd,a3,56,fb,b7,b5,4b,76,e3,cf,b0,b2,fe,7d,0c,2e,6e,d8,e1,8c,ce,45,c9,84,09,67,ad,44,74,1c,0c,e4,32,af,62,4e,d7,e3,e6,48,43,ec,d7,73,f7,80,2c,ae,8a,8e,e3,9c,18,37,7e,df,01,6e,b1,dc,ca,e2,15,ad,28,84,08,b7,f3,15,75,f2,e5,48,ea,44,18,3e,85,00,68,0d,8b,f1,1c,59,ea,4d,8d,ac,d6,f5,78,1e,ad,ad,9c,a6,32,0a,1a,07,c0,aa,c1,49,58,31,1c,62,78,5d,69,70,3a,30,9c,95,8c,1f,51,f5,9b,54,c9,98,2d,5d,f0,4d,14,01,71,71,3f,64,80,34,5c,c7,27,f0,e7,b2,48,dd,4b,94,88,7b,6f,46,e8,31,8d,82,e5,c2,4b,e0,71,57,77,b0,db,07,a7,a9,7b,e7,74,cb,1f,f4,94,67,a3,80,9f,4c,3c,bc,de,ab,be,97,75,99,b3,19,06,cf,83,d5,cd,85,6a,38,a0,91,f4,5c,4c,e2,79,27,5d,6b,b9,29,0b,76,01,48,d9,17,60,93,13,fb,84,fd,45,fe,b7,27,9a,62,c3,5e,75,00,bb,f8,5c,a3,3a,a6,0f,55,61,f8,a7,8b,57,b7,de,c2,e9,67,7d,57,b1,00,f1,2c,d7,11,99,b4,d8,cc,11,3d,4c,44,48,89,fd,4c,cb,2f,93,ea,e9,46,3b,2b,28,06,f1,99,f5,f0,95,3b,e0,76,00,1d,b2,7f,40,cb,75,fb,aa,1d,af,b6,cf,82,1e,21,0b,d7,41,80,65,7f,c2,fe,ad,53,c3,77,bd,5e,76,af,25,63,c9,d0,6d,9f,d5,8e,84,b1,2b,19,d7,2d,3b,81,f2,0a,93,79,17,89,73,c0,1d,62,03,8c,73,81,3e,cf,b3,36,53,47,c3,94,0d,da,67,2d,48,24,3a,a1,2c,ed,c0,94,9f,52,d7,a5,43,a5,4d,75,56,39,ee,9e,28,6f,0b,58,cb,65,e9,50,2e,22,bc,60,dd,cd,e9,71,6f,a0,d0,7a,06,6a,80,58,29,df,c0,02,fc,18,29,cb,c9,27,39,b6,65,37,c5,44,89,d1,3c,00,be,75,c0,53,36,af,83,a9,1a,90,8d,a6,68,1f,96,c5,92,70,5a,7d,7d,86,53,8b,e7,e2,ca,43,91,d6,b8,84,44,19,05,dc,19,11,99,9b,55,b4,79,e8,44,88,7a,cd,69,0b,09,6c,0e,f2,f8,e4,6c,e1,6b,09,c6,8a,56,42,25,f2,23,b9,7d,27,74,fb,ef,f8,31,ab,2b,60,9f,a9,97,36,43,cb,7a,a0,7b,93,8a,b5,c4,d4,de,53,43,97,a3,1f,53,6d,a1,bb,99,60,e4,c9,e3,59,66,1e,4e,26,67,21,50,cd,ad,1c,e7,c1,62,9d,68,43,6a,e9,9f,34,18,39,4e,d9,3a,22,bb,50,4f,a8,d0,77,0f,4f,0a,33,be,cc,a6,2b,e9,43,eb,03,3a,13,49,47,ac,51,7b,0d,9f,63,90,15,46,88,21,ee,2e,f0,ba,66,8a,a4,ad,f0,25,d8,03,10,37,b9,b6,ba,b1,57,08,dc,1e,09,da,aa,57,06,9b,ef,5f,2b,d7,35,b0,23,ab,32,66,9e,2b,77,fc,c5,13,a7,14,f6,80,dd,c4,e2,16,f2,8a,40,e7,7c,f3,0d,bd,0d,52,06,f7,7e,b9,bd,3d,9b,f9,de,b2,85,d6,d2,25,20,72,8d,84,14,57,6b,ec,ea,43,9b,16,7e,0b,ad,46,f9,a5,28,ee,dd,07,5b,bd,81,ed,bb,c8,b8,40,73,da,aa,06,10,23,59,2b,89,ad,33,52,89,fe,fc,0c,9a,ea,9c,00,f0,70,75,8b,83,55,71,ff,97,e9,bb,72,d8,ed,bb,a0,6d,28,1a,24,e5,bf,c2,a6,44,1e,69,c4,87,46,4f,f1,7d,da,26,0e,57,26,76,ef,f7,15,62,ba,76,9c,2e,bf,8b,e7,c3,6c,a3,67,ab,4c,00,50,6a,ce,db,c6,e4,3e,0c,b5,62,e1,5e,48,a1,57,35,5f,30,22,3f,fc,35,88,dd,fd,ca,28,b2,8d,8e,ab,96,dc,05,52,10,8d,9a,bc,1f,14,ad,6d,c8,01,30,66,3b,04,81,ec,e0,62,86,f2,29,0e,22,96,82,73,b1,28,dc,ae,92,9e,98,14,d9,0b,5c,0a,d8,9d,f9,db,38,9a,c0,82,50,a5,55,5e,3f,7d,ad,55,33,33,7f,48,59,38,11,ee,bd,f6,97,be,ee,37,d6,df,bf,44,9c,5b,2c,d9,64,30,7a,86,2c,9a,ff,1b,ad,6c,10,d1,22,f7,25,6c,ff,b1,8b,65,c3,73,95,69,6f,20,71,1f,ec,56,89,6a,6f,62,b2,57,c3,4a,08,9f,e2,4d,33,15,28,46,af,f0,fb,fa,df,89,aa,8e,d0,64,41,49,0a,2e,b5,8e,24,8a,92,48,c2,1c,ba,a5,b4,8d,05,5d,8e,c9,2c,e6,10,56,04,fe,e7,c9,d2,75,25,61,1e,53,75,d4,f5,9f,77,b2,af,af,88,55,47,07,ab,e1,8e,3b,31,ef,f6,29,84,83,b2,13,e1,f8,23,a9,8d,b1,3d,04,26,70,c0,a4,6c,8e,32,d1,17,d1,b2,1b,07,1a,21,bd,ff,12,b2,2f,a2,c9,af,43,d1,ec,2f,70,57,c6,42,6c,3c,1f,69,92,c3,77,2f,8b,63,f2,53,ce,dd,fe,8e,cb,59,e5,a0,55,03,6c,c8,6b,39,33,b8,49,48,8d,e5,d7,21,8f,b1,d5,43,4c,65,3d,22,7b,de,35,58,fb,b5,5a,0f,ba,6b,1d,23,fc,90,86,98,aa,29,14,99,c5,35,fe,1c,b4,71,1e,ef,e2,30,5e,d4,c0,e5,e3,71,2a,6c,68,f8,37,36,d7,f9,e7,d8,22,aa,51,87,83,ff,4f,5c,ac,3f,62,56,1e,f4,a5,ce,81,37,98,06,68,d3,66,69,8a,69,5a,75,5c,8c,d4,81,fd,31,61,5e,ea,3d,3f,a3,21,4f,7a,37,b1,e3,39,4e,68,df,fd,6c,49,0f,7e,10,45,8d,1c,0f,79,1f,7a,81,c1,6f,ec,a1,bb,8a,83,4e,c5,c4,ef,64,8c,5e,cb,2c,74,7b,71,7a,e5,3a,1b,4b,52,5d,d4,03,62,c5,41,2a,62,6e,22,83,3c,99,01,60,7f,6c,8e,c6,58,1a,47,be,f2,26,e1,5d,7a,d4,f8,20,52,7b,17,1b,1d,ce,06,d7,30,ff,5b,51,ee,32,7d,2a,2a,bb,7b,6c,a9,55,25,83,78,a2,19,94,ee,10,a5,33,c8,a0,fe,c1,31,fa,53,b4,36,0b,8e,8d,2a,a6,d4,2a,83,2f,3b,4c,3f,13,d2,ef,88,68,67,a4,cc,65,45,26,ae,09,26,a6,3b,42,86,6b,bb,73,c2,46,17,21,e3,46,ff,17,71,b2,49,52,40,e5,87,35,20,81,44,d2,de,45,c3,6b,e1,e0,bf,de,ab,d5,f0,fb,aa,21,cc,32,5f,42,ae,7d,85,af,30,a8,22,3d,c0,86,35,d6,a2,27,b3,c7,01,28,ea,f7,bb,d1,4c,3c,a0,35,d3,d5,91,1d,81,33,4c,5d,c2,cb,bb,15,89,55,5e,78,0a,35,19,f2,5b,44,77,aa,e7,09,4f,63,25,2f,47,f0,e4,20,68,0e,6e,00,14,26,2c,d7,d8,92,36,44,e5,99,3b,b6,bc,56,cc,ed,bb,38,dc,1d,67,41,e3,98,58,aa,b0,bf,34,7b,cb,58,98,07,1f,a1,f6,dc,2a,3c,e2,ef,5e,b8,0a,7d,f1,88,72,21,37,0c,4b,87,ee,ac,36,ce,e5,4d,af,e8,52,eb,fe,3d,17,10,11,fd,20,00,51,70,ac,f2,e1,be,1d,b1,e4,55,4e,b1,0f,6c,11,56,8c,e1,4a,d3,07,37,fd,cd,f6,d4,41,ee,5e,1b,f8,91,a4,1b,2d,38,ae,53,c1,e9,dc,11,38,a9,1a,02,5d,60,f4,c8,38,60,99,b9,bd,41,0a,3b,e3,19,4b,d7,22,82,91,33,0e,7e,2e,8e,cb,b1,47,67,bb,e3,2f,38,0a,5e,39,67,dc,df,7a,21,9b,70,2c,7d,d0,62,09,8d,b6,19,c5,ce,d4,b5,1a,6e,73,0d,1e,60,cf,62,29,a8,42,76,bb,d4,80,de,5a,0f,db,0c,21,6a,13,81,ac,9e,ca,0f,29,dc,06,32,d2,f1,ee,ad,c5,64,cf,2c,34,e2,8f,fb,82,3c,07,af,66,b3,19,14,e6,ae,f7,60,53,c6,be,7d,b8,b0,a7,c6,b3,ff,a8,a2,d2,62,fb,6b,9f,32,9a,18,1e,71,27,0e,39,3d,1c,8a,02,fa,b1,62,0b,16,d0,6b,6b,11,d6,8b,d1,97,27,fb,b6,17,47,43,19,09,1d,ee,78,ef,73,d8,5b,4d,74,bd,bc,3c,b4,f4,91,62,71,3c,38,e1,8f,27,32,93,25,f6,bc,3d,3d,26,92,c9,b2,6c,7f,a9,b7,28,f5,68,68,3f,e7,6f,92,f5,82,dd,3c,f3,41,3b,7e,b0,b5,1a,ef,66,2d,06,fb,45,3a,62,2d,38,7c,8b,25,dc,9d,d1,c1,44,78,fe,9b,ef,e8,c3,df,01,f7,a0,5f,f0,17,d8,44,58,95,2d,ca,e7,b3,71,be,66,be,08,07,3a,7f,6f,2f,8a,ee,8b,67,1e,f8,57,21,a8,c0,ce,bf,55,b8,ac,d2,31,90,7b,9b,72,a2,51,9f,bc,cd,57,89,ad,8d,b3,f3,25,96,f3,b8,bf,d1,38,2a,8e,26,61,c1,6b,77,c1,da,c1,1c,f7,00,a1,ce,2f,29,2e,3b,11,e6,77,fa,ff,00,10,4b,06,ab,35,68,b4,6b,ee,7c,5a,1d,f7,fb,9d,18,7d,1b,50,c3,3e,5e,e3,d8,26,2a,db,87,ee,1c,61,e9,9f,e6,66,fe,3c,25,51,95,89,12,60,11,c4,c3,4a,3d,c1,d0,f4,c1,12,3a,7e,34,d3,ca,a9,2f,00,0e,9e,07,e8,64,89,38,96,6b,02,56,df,85,03,56,83,58,6d,c1,ad,68,2c,19,ad,50,c0,cf,9b,10,0e,0c,48,a2,cb,e1,ba,b9,44,05,2d,c4,2b,cd,cc,d0,e8,c6,0c,9c,c7,28,59,d7,71,82,de,c3,56,f2,43,3b,53,68,a3,f8,73,3d,9a,61,1e,71,0f,22,7a,c5,d8,cf,a0,8a,54,58,3b,81,37,8a,72,7a,8a,78,1a,6b,01,4a,4b,23,3b,cf,31,d2,c9,f3,4d,99,86,73,2d,a8,0f,5c,6b,de,0b,aa,af,5b,54,d6,77,48,9a,f3,8e,1c,83,67,0c,84,da,50,95,c1,ba,d5,2d,e6,a7,a2,14,50,95,5e,b3,b4,4d,a3,c3,3e,bc,a2,7b,6a,f1,00,e7,fa,57,65,7e,19,09,c6,5d,0d,f8,9c,f0,74,50,ca,09,ca,c8,83,85,f0,d3,0c,8f,89,5e,11,fb,9a,d4,d8,da,74,d4,d2,dd,fe,f6,db,6c,04,97,91,a5,99,29,97,77,4e,2a,90,3a,df,19,26,42,c3,15,f8,b1,82,98,0b,f4,3e,a6,3f,3b,e0,39,ba,d9,c8,38,91,ea,b6,be,53,07,4b,2e,9e,e6,e0,c5,79,1b,eb,fd,65,7d,4d,20,9b,63,cd,5c,b1,fd,8f,4b,41,cc,2b,e9,b6,12,eb,9e,11,28,b6,87,08,5f,00,d0,16,a3,82,9b,25,e3,33,7c,a6,e9,66,23,e9,fc,52,bd,ad,5a,b0,a0,15,34,fb,c8,81,46,ed,d2,cf,ed,5f,ac,51,20,8b,a5,78,b5,23,99,08,93,c3,ce,21,ac,9e,65,58,6d,4e,a4,4e,4b,a8,34,e7,dd,c1,c3,0d,91,ff,f2,b8,f1,91,f6,a7,43,76,c4,fe,91,ae,b4,e4,1a,bc,fb,8f,28,58,fe,74,cb,fa,4f,02,fa,d1,9b,a6,14,fe,b9,88,7e,49,94,5f,82,20,43,7f,49,24,13,a1,08,80,29,af,01,a7,ef,8b,d0,af,a8,51,2a,e3,db,34,c9,17,bb,8c,95,da,c4,2b,f1,f5,45,d5,7e,b3,77,41,e3,fe,12,aa,12,17,08,81,e3,0b,9c,69,2b,bf,5a,4f,ed,a1,a3,eb,eb,b6,17,bb,de,7b,56,eb,f0,e1,3e,83,3f,5b,ad,b5,25,de,9c,71,28,3b,7e,1d,20,00,00,00,32,cb,95,6f,61,0c,bb,1d,93,cf,d3,7a,9a,e6,ee,64,6a,02,a1,30,e9,00,24,0b,b3,43,cb,b9,f1,3e,ec,ce" />
      <add key="Size" value="3040" />
    </HKEY_LOCAL_MACHINE__Comm__Connmgr__Settings__Connections__Gamma>
                         <HKEY_LOCAL_MACHINE__Comm__Connmgr__Settings__Connections__Gamma1>
  <add key="Data 0" value="01,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,40,04,00,00,00,00,00,00,00,00,00,00,00,01,00,00,00,20,00,00,00,3a,fe,d2,9a,0b,c5,42,5b,99,cc,63,8a,b5,0e,06,cb,81,94,f6,7b,47,7e,6d,7d,ec,b1,b3,93,5d,7e,1e,59,00,00,00,00,02,80,00,00,20,00,00,00,62,bf,c5,e5,df,df,5f,62,b0,98,9d,b8,dd,06,f2,65,d3,72,09,d2,c8,7e,b1,3f,cc,91,e6,a3,fd,77,1f,76,50,07,00,00,da,0d,60,ba,28,2e,60,61,68,2b,e1,0c,a1,37,2e,f9,f5,50,c8,10,3b,18,28,86,57,bc,7c,e9,ed,f8,79,9c,fa,32,b9,e5,31,de,2a,04,0d,06,e7,bd,30,34,8f,d9,4c,84,76,b9,55,ae,b5,78,53,bb,c3,64,34,e3,49,c4,d9,65,da,21,6a,08,2d,1d,57,43,10,e2,a5,90,33,81,9f,e8,b5,3b,ca,7b,87,cd,d1,f9,4f,9a,b3,a5,95,d9,36,f0,4a,3f,2e,ef,34,7c,52,f8,c7,cb,ea,3d,37,94,f0,4e,45,cf,99,12,34,63,a6,8f,36,1a,fc,39,fe,b5,50,c1,61,f4,34,fd,e6,d1,58,c6,b5,52,cf,fa,a8,3c,d6,f2,bd,ad,2a,91,d1,28,3d,fb,b0,6f,3e,28,7a,21,a6,40,9a,1f,3e,5b,48,fc,e4,b0,f2,c9,ec,21,f3,eb,e9,80,d3,d1,61,77,d6,47,26,30,6e,05,8c,af,f9,1c,76,4f,d2,50,a2,41,a3,cf,94,73,2d,3f,b6,de,df,dd,23,cd,9f,56,6a,3d,e3,47,7a,cc,b8,27,c8,2d,70,d7,ed,64,06,35,04,a6,66,72,0e,0d,fa,e1,8a,9b,e7,a5,bb,c1,96,73,94,4a,ee,40,fa,c0,69,3a,37,3b,40,f8,70,22,aa,bf,4d,87,cd,f6,8d,c5,7f,7c,db,4e,9e,78,e6,07,9b,2b,92,e4,13,2e,01,86,c9,c8,ce,bd,1d,02,28,0d,ce,10,0e,83,b0,6a,b8,8e,89,e5,3d,1b,7c,48,79,a0,4a,a4,c6,a7,24,0d,1f,cd,91,df,a7,4d,b5,80,bc,7e,94,eb,c1,a4,d6,db,69,de,ad,2c,7f,ba,a5,bd,65,27,85,0c,12,31,08,cd,cc,f7,10,9b,54,1d,21,b2,0e,8e,13,de,0c,47,64,04,76,47,a2,69,60,9e,47,78,21,79,e2,ee,56,4a,f2,e3,30,6b,02,10,7f,56,b6,be,8f,09,d0,93,0e,a8,e3,b1,fe,57,c6,e7,c4,a3,39,ee,a9,4a,c7,cf,56,29,01,45,e0,64,6a,7c,75,9c,28,e8,12,fc,54,f7,48,f6,98,f8,99,59,3a,f4,8c,95,bb,18,12,c1,63,f0,5c,a3,ff,ad,48,77,08,68,87,f7,eb,b5,ed,50,b2,0e,71,ec,19,ad,c1,2a,b2,20,f8,5c,e7,93,dc,0d,89,b2,00,75,6c,9d,92,79,a0,ad,a3,c3,6a,f6,4d,78,a9,33,57,af,ef,d7,a9,47,b3,ac,3c,4a,b6,f0,0f,f1,29,3a,70,f9,18,cd,00,1c,81,5c,47,11,17,82,34,63,28,b6,9b,55,29,78,29,56,95,29,58,7b,3b,1d,f8,b9,08,ff,46,a5,71,8d,8e,b5,ca,9f,14,9a,a5,17,9a,c7,88,54,7c,54,fb,a2,37,8a,07,64,b9,2a,cf,a6,b3,f4,60,d4,e4,4a,75,23,33,71,4e,e7,51,bb,ae,f7,8b,5a,5e,6a,61,74,4c,56,a1,bd,be,ad,f0,21,59,ad,1b,fa,58,bb,25,7e,dd,4c,bb,89,19,a3,8e,0c,27,34,a7,e4,35,d0,08,c4,d0,a6,bc,b6,44,26,12,02,81,da,84,35,a7,ba,24,76,fd,d6,4e,3b,ed,d8,aa,6d,a7,08,3e,fe,2c,a7,ce,39,68,68,ec,c3,c6,e7,8d,5d,c6,8d,ac,8a,d3,45,31,e9,4f,a1,e6,87,9f,08,58,ab,08,b0,63,e0,3b,57,03,e7,98,67,7c,3b,6e,91,2f,20,d2,77,97,32,4e,5d,16,a0,9f,43,71,5c,51,23,98,d8,b3,dd,7d,c1,ae,88,8d,81,81,1d,43,2a,44,ae,27,e4,61,72,0f,66,7a,92,6e,2d,31,26,21,18,aa,6a,f6,49,c0,5c,81,11,69,79,e2,de,9c,0c,4b,12,b7,f0,ae,91,13,a1,a1,34,ce,b1,e3,00,fb,a1,ee,e8,a6,07,2f,e5,48,8a,d6,cb,fc,3b,f9,7b,ef,1e,fb,79,85,f6,e2,d8,85,70,b8,f0,0e,1d,b5,17,bd,cf,2c,2b,c6,3b,c6,b0,a0,57,25,17,b7,fa,a1,fe,75,25,2d,cd,04,77,8c,91,a9,69,c2,ac,06,f1,8a,78,33,dc,43,51,4b,00,1d,cb,c1,fc,60,27,85,a7,d2,58,9a,d3,5b,0b,12,22,ec,68,64,c8,e6,a2,a4,de,12,c4,ca,c3,81,36,ef,97,46,66,38,9b,ec,b4,8d,27,42,b5,48,d3,49,c1,c1,0c,6e,2d,71,70,34,aa,6e,36,67,70,b8,9d,a1,be,79,e8,12,c1,18,53,5e,0a,d3,30,cf,0b,0c,f3,3d,c8,29,09,9a,84,d7,f0,c3,44,b9,58,c6,2e,70,3d,a2,f0,8a,c2,a8,e8,f8,c4,46,6d,d0,32,50,72,0a,95,89,c9,c4,f1,05,fb,8a,62,2d,60,5e,6a,16,62,9a,19,cb,ab,4f,63,08,74,c9,55,99,69,3c,98,be,02,67,94,7f,e2,85,7a,ec,c8,71,7c,d2,97,11,aa,57,b3,ff,cf,72,ab,29,d7,14,8b,38,38,58,5a,2d,0f,2f,b4,f5,f8,6a,8b,ef,76,e0,3b,1b,05,71,09,fd,f3,2e,10,4e,2e,d1,e0,ce,b4,75,6a,78,09,9c,48,2f,72,24,1f,7e,66,99,38,b6,5c,5e,8e,ee,93,26,8a,2d,29,d9,7c,59,81,a9,fe,d6,68,f8,cc,5a,ae,71,84,58,ec,03,3d,ba,45,a1,79,48,e9,f0,e8,17,75,10,65,e9,ab,fd,82,e1,0c,dd,c1,0c,96,97,11,9a,fa,03,78,c4,b2,ea,7f,17,51,7a,53,4e,e8,a8,a5,3b,53,d0,53,aa,27,6f,88,97,bd,f6,84,f2,ae,68,b9,b5,5a,e3,22,79,ec,2c,ad,b0,7f,ee,f8,db,5d,c5,1b,d4,76,bc,2e,a5,aa,24,e4,81,18,58,b2,e7,09,03,a1,f8,e9,c2,ea,84,0a,f0,23,9f,1f,b7,8e,90,e4,a3,95,ea,da,dd,0a,ee,ec,c2,ff,f2,89,80,03,46,8e,e6,af,c8,23,51,5a,33,87,91,de,9e,8f,17,46,ed,ad,af,2b,4d,c9,74,b3,b7,32,1f,18,54,5b,e4,1a,f7,a8,78,16,4d,10,0b,ac,d9,2e,c4,31,81,4e,d8,cd,26,34,fc,66,0c,13,af,df,9d,b1,b0,36,ab,9b,78,71,25,ee,38,43,fd,0b,a5,8a,3b,68,3d,47,4a,f2,08,c8,e0,cf,bf,cc,9d,90,66,ba,bc,95,06,47,e2,15,44,3d,00,68,f7,27,15,73,1a,6a,02,ec,59,88,47,5f,88,98,fd,52,19,97,20,5a,59,33,24,69,ed,51,7f,30,af,47,6c,80,1e,62,a3,8c,50,27,f7,eb,59,b2,c4,3b,79,56,d8,5c,5d,a3,52,f6,33,8d,be,05,ef,cc,84,fc,05,73,5b,9b,ca,c0,5e,6d,2d,90,b4,09,7d,89,fc,c0,d8,e3,02,ab,87,f5,67,3b,73,fb,e9,ae,cb,82,55,64,ae,c2,5b,a9,5a,d8,f4,9a,a1,4a,9d,af,ae,71,b6,f4,46,f6,f2,8d,89,1a,9f,a2,28,ec,4e,58,4e,84,ae,9f,ce,5f,ef,be,5a,40,0c,b0,1b,cd,f5,73,f6,4c,c8,a2,18,21,79,c2,1e,de,db,78,7b,e7,12,a8,7f,c5,85,62,11,d3,64,9e,ad,0e,0a,53,fe,58,04,af,73,c3,fd,a3,a1,cf,24,34,ad,9c,54,e9,f7,dc,18,ad,1c,41,e8,77,f5,c0,d6,f1,21,48,93,29,de,d5,90,2a,6f,b5,86,68,35,b9,21,c1,a1,70,42,5c,00,47,94,3c,11,e8,e6,6a,b2,fd,9c,9e,ae,62,e5,fb,3b,62,b2,cc,a0,fb,3d,c6,3a,3c,8f,fc,2f,94,65,66,d6,b8,24,c7,7f,62,61,bb,ea,76,c7,df,e9,34,8e,85,a3,d2,27,96,22,54,9c,20,55,a8,8b,c4,2f,60,80,09,dd,6c,55,8c,4b,67,8d,ae,67,5a,52,52,53,f2,40,4a,76,30,fa,d1,cf,f2,59,2c,5d,9b,7b,83,09,de,f1,c0,86,42,15,0f,63,28,ec,bb,0a,1b,e5,c6,c5,15,2b,34,18,a3,2d,d6,f8,7c,f2,ee,f7,05,b9,3a,a5,f4,0b,8c,59,05,a4,68,64,6c,d5,9c,8f,2c,7e,24,a4,4f,cb,b8,d2,1f,46,21,a8,58,7b,38,6e,88,a7,72,ea,97,30,8c,12,f0,2b,ab,44,60,a6,34,47,a6,aa,da,69,58,a4,88,e6,8f,0c,3c,3b,f9,8b,1f,f4,1a,8c,70,69,7c,64,93,6e,c9,0b,86,5e,df,37,a9,94,ec,e7,db,ae,27,7f,2e,20,bc,b4,3d,12,ed,12,48,ac,2b,49,de,43,86,f3,88,a5,37,52,10,8d,a8,4a,2d,f1,b3,66,47,91,f6,25,dd,e2,3f,f7,71,23,e6,0d,56,e0,e6,f9,b9,5f,57,b6,7d,80,04,a5,cd,58,c1,e2,39,00,b1,5a,ac,b8,e6,41,fd,4d,07,1f,de,99,2b,a6,fd,5a,dc,60,9f,06,96,85,d4,37,b2,ec,64,b4,fb,62,74,d3,f6,a2,e7,7d,87,7c,dd,cd,4f,bb,e0,e3,d8,bc,b4,6b,4b,ae,61,75,0b,4a,96,5a,84,f1,62,1c,16,21,86,ee,95,33,d1,92,ac,79,f6,ad,a6,43,d8,3b,e4,3f,f0,31,24,a1,ab,4a,f4,8f,5e,a7,5e,d1,a7,01,06,bb,fa,16,50,d6,1f,18,72,23,b7,db,99,2b,48,d9,d6,ab,89,8c,cc,37,9b,3a,6d,66,ea,20,00,00,00,f0,d5,21,b8,0c,f5,0f,21,59,a5,67,59,87,76,3c,be,8d,a0,55,10,5a,81,f2,ff,47,07,bf,88,88,09,c9,13" />
  <add key="Size" value="2032" />
</HKEY_LOCAL_MACHINE__Comm__Connmgr__Settings__Connections__Gamma1>
                         */
                        if (regVal == null || val != regVal)
                        {
                            base.UpdateRegistry(keyName, valueName, val);
                        }
                    }
                    catch (Exception ex)
                    {
                        Shared.SaveToLogError("Error Update registry " + item);
                    }
                else
                    if (wirelessAdapterName != String.Empty && item.ToString().Substring(0, wirelessAdapterName.Length) == wirelessAdapterName)
                    {
                        base.UpdateWiFiProfileSettings(item.ToString(), m_settings.Get(item.ToString()));
                    }
                    else
                        if (item.ToString().Substring(0, 11) == "networkSett" && wirelessAdapterName != String.Empty)
                        {
                            base.UpdateNetworkSettings(item.ToString(), m_settings.Get(item.ToString()));
                        }
                        else
                            if (item.ToString().Substring(0, 11) == "scanSetting")
                            {
                                try
                                {
                                    int val = int.Parse(m_settings.Get(item.ToString()));
                                    int par = -1;
                                    switch (item.ToString())
                                    {
                                        case "scanSettings.EAN13.ENABLE":
                                            par = Param.EAN13_ENABLE;
                                            break;
                                        case "scanSettings.EAN13.TO_ISBN":
                                            par = Param.EAN13_TO_ISBN;
                                            break;
                                        case "scanSettings.EAN13.TO_ISSN":
                                            par = Param.EAN13_TO_ISSN;
                                            break;
                                        case "scanSettings.EAN13.SEND_CHECK":
                                            par = Param.EAN13_SEND_CHECK;
                                            break;
                                        case "scanSettings.EAN13.SEND_SYS":
                                            par = Param.EAN13_SEND_SYS;
                                            break;

                                        case "scanSettings.I25.ENABLE":
                                            par = Param.I25_ENABLE;
                                            break;
                                        case "scanSettings.I25.MIN_LENGTH":
                                            par = Param.I25_MIN_LENGTH;
                                            break;
                                        case "scanSettings.I25.MAX_LENGTH":
                                            par = Param.I25_MAX_LENGTH;
                                            break;
                                        case "scanSettings.I25.ENABLE_CHECK":
                                            par = Param.I25_ENABLE_CHECK;
                                            break;
                                        case "scanSettings.I25.SEND_CHECK":
                                            par = Param.I25_SEND_CHECK;
                                            break;
                                        case "scanSettings.I25.CASE_CODE":
                                            par = Param.I25_CASE_CODE;
                                            break;

                                        case "scanSettings.CODE128.ENABLE":
                                            par = Param.CODE128_ENABLE;
                                            break;
                                        case "scanSettings.CODE128.MIN_LENGTH":
                                            par = Param.CODE128_MIN_LENGTH;
                                            break;
                                        case "scanSettings.CODE128.MAX_LENGTH":
                                            par = Param.CODE128_MAX_LENGTH;
                                            break;
                                        case "scanSettings.CODE128.ENABLE_GS1_128":
                                            par = Param.CODE128_ENABLE_GS1_128;
                                            break;

                                        case "scanSettings.CODE39.ENABLE":
                                            par = Param.CODE39_ENABLE;
                                            break;
                                        case "scanSettings.CODE39.MIN_LENGTH":
                                            par = Param.CODE39_MIN_LENGTH;
                                            break;
                                        case "scanSettings.CODE39.MAX_LENGTH":
                                            par = Param.CODE39_MAX_LENGTH;
                                            break;
                                        case "scanSettings.CODE39.ENABLE_CHECK":
                                            par = Param.CODE39_ENABLE_CHECK;
                                            break;
                                        case "scanSettings.CODE39.SEND_CHECK":
                                            par = Param.CODE39_SEND_CHECK;
                                            break;
                                        case "scanSettings.CODE39.FULL_ASCII":
                                            par = Param.CODE39_FULL_ASCII;
                                            break;

                                        case "scanSettings.CODEBAR.ENABLE":
                                            par = Param.CODABAR_ENABLE;
                                            break;
                                        case "scanSettings.CODE93.ENABLE":
                                            par = Param.CODE93_ENABLE;
                                            break;
                                        case "scanSettings.EAN8.ENABLE":
                                            par = Param.EAN8_ENABLE;
                                            break;
                                        case "scanSettings.GS1_14.ENABLE":
                                            par = Param.GS1_14_ENABLE;
                                            break;
                                        case "scanSettings.GS1_EXP.ENABLE":
                                            par = Param.GS1_EXP_ENABLE;
                                            break;
                                        case "scanSettings.GS1_LIMIT.ENABLE":
                                            par = Param.GS1_LIMIT_ENABLE;
                                            break;
                                        case "scanSettings.MSI.ENABLE":
                                            par = Param.MSI_ENABLE;
                                            break;
                                        case "scanSettings.PHARMA39.ENABLE":
                                            par = Param.PHARMA39_ENABLE;
                                            break;
                                        case "scanSettings.S25.ENABLE":
                                            par = Param.S25_ENABLE;
                                            break;
                                        case "scanSettings.TRIOPTIC.ENABLE":
                                            par = Param.TRIOPTIC_ENABLE;
                                            break;
                                        case "scanSettings.UPCA.ENABLE":
                                            par = Param.UPCA_ENABLE;
                                            break;
                                        case "scanSettings.UPCE0.ENABLE":
                                            par = Param.UPCE0_ENABLE;
                                            break;
                                        case "scanSettings.PDF417.ENABLE":
                                            par = Param.PDF417_ENABLE;
                                            break;
                                        case "scanSettings.MicroPDF417.ENABLE":
                                            par = Param.MICROPDF417_ENABLE;
                                            break;
                                        case "scanSettings.QRCode.ENABLE":
                                            par = Param.QRCODE_ENABLE;
                                            break;
                                        case "scanSettings.MaxiCode.ENABLE":
                                            par = Param.MAXICODE_ENABLE;
                                            break;
                                        case "scanSettings.DataMatrix.ENABLE":
                                            par = Param.DATAMATRIX_ENABLE;
                                            break;
                                        case "scanSettings.Aztec.ENABLE":
                                            par = Param.AZTEC_ENABLE;
                                            break;
                                        case "scanSettings.Plessey.ENABLE":
                                            break;
                                        case "scanSettings.Telepen.ENABLE":
                                            break;
                                        case "scanSettings.USPlanet.ENABLE":
                                            par = Param.US_PLANET_ENABLE;
                                            break;
                                        case "scanSettings.USPostnet.ENABLE":
                                            par = Param.US_POSTNET_ENABLE;
                                            break;
                                        case "scanSettings.Netherlands.ENABLE":
                                            break;
                                        case "scanSettings.JapanPostal.ENABLE":
                                            par = Param.JAPANESE_POST_ENABLE;
                                            break;
                                        case "scanSettings.AustralianPostal.ENABLE":
                                            par = Param.AUSTRALIAN_POST_ENABLE;
                                            break;
                                        case "scanSettings.CompositeCCC.ENABLE":
                                            par = Param.COMPOSITE_ENABLE;
                                            break;
                                        case "scanSettings.CompositeCCAB.ENABLE":
                                            break;
                                        case "scanSettings.Matrix25.ENABLE":
                                            par = Param.M25_ENABLE;
                                            break;
                                        case "scanSettings.Discrete25.ENABLE":
                                            break;
                                        case "scanSettings.ISBT128.ENABLE":
                                            break;
                                        case "scanSettings.CODE11.ENABLE":
                                            break;
                                        case "scanSettings.PICKLIST.ENABLE":
                                            par = Param.PICKLIST_ENABLE;
                                            break;
                                        case "scanSettings.IMAGER_ILLUMINATE_ENABLE":
                                            par = Param.IMAGER_ILLUMINATE_ENABLE;
                                            break;
                                    }
                                    var curVal = Decode.GetParam(par);
                                    if (curVal != val)
                                        Decode.SetParam(par, val);
                                }
                                catch
                                {
                                    Shared.SaveToLogError("Error Update scaner parameter (" + item + ")");
                                }
                            }

            }
#if !DEBUG
            if (reboot)
            {
                Shared.SaveToLogError("Error - Rebooting Device");
                Datalogic.API.Device.Reset(Datalogic.API.Device.BootType.Warm);
            }
#endif
            return true;
        }

        #endregion

    }
}
