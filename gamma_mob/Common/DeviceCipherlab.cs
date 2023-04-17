using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using gamma_mob.Common;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using OpenNETCF.Net.NetworkInformation;

namespace gamma_mob.Common
{
    public class DeviceCipherlab : IDevice
    {
        #region WIN32
        // http://msdn.microsoft.com/en-us/library/aa926903.aspx
        public enum ACLineStatus : byte
        {
            AC_LINE_OFFLINE = 0, // Offline
            AC_LINE_ONLINE = 1, // Online
            AC_LINE_BACKUP_POWER = 2, // Backup Power
            AC_LINE_UNKNOWN = 0xFF, //
            Unknown = 0xFF, //status
        }

        public enum BatteryChemistry : byte
        {
            BATTERY_CHEMISTRY_ALKALINE = 0x01,  // Alkaline battery.
            BATTERY_CHEMISTRY_NICD = 0x02, // Nickel Cadmium battery.
            BATTERY_CHEMISTRY_NIMH = 0x03, // Nickel Metal Hydride battery.
            BATTERY_CHEMISTRY_LION = 0x04, // Lithium Ion battery.
            BATTERY_CHEMISTRY_LIPOLY = 0x05, // Lithium Polymer battery.
            BATTERY_CHEMISTRY_ZINCAIR = 0x06, // Zinc Air battery.
            BATTERY_CHEMISTRY_UNKNOWN = 0xFF // Battery chemistry is unknown.
        }

        public enum BatteryFlag : byte
        {
            BATTERY_FLAG_HIGH = 0x01,
            BATTERY_FLAG_CRITICAL = 0x04,
            BATTERY_FLAG_CHARGING = 0x08,
            BATTERY_FLAG_NO_BATTERY = 0x80,
            BATTERY_FLAG_UNKNOWN = 0xFF,
            BATTERY_FLAG_LOW = 0x02
        }

        // http://msdn.microsoft.com/en-us/library/ms941842.aspx
        [StructLayout(LayoutKind.Sequential)]
        public class SYSTEM_POWER_STATUS_EX2
        {
            //AC power status. 
            public ACLineStatus ACLineStatus;
            //Battery charge status
            public BatteryFlag BatteryFlag;
            // Percentage of full battery charge remaining. Must be in 
            // the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN if 
            // percentage of battery life remaining is unknown
            public byte BatteryLifePercent;
            byte Reserved1;
            //Percentage of full battery charge remaining. Must be 
            // in the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN 
            // if percentage of battery life remaining is unknown. 
            public int BatteryLifeTime;
            // Number of seconds of battery life when at full charge, 
            // or BATTERY_LIFE_UNKNOWN if full lifetime of battery is unknown
            public int BatteryFullLifeTime;
            byte Reserved2;
            // Backup battery charge status.
            public BatteryFlag BackupBatteryFlag;
            // Percentage of full backup battery charge remaining. Must be in 
            // the range 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN if percentage 
            // of backup battery life remaining is unknown. 

            public byte BackupBatteryLifePercent;
            byte Reserved3;
            // Number of seconds of backup battery life when at full charge, or 
            // BATTERY_LIFE_UNKNOWN if number of seconds of backup battery life 
            // remaining is unknown. 
            public int BackupBatteryLifeTime;
            // Number of seconds of backup battery life when at full charge, or 
            // BATTERY_LIFE_UNKNOWN if full lifetime of backup battery is unknown
            public int BackupBatteryFullLifeTime;
            // Number of millivolts (mV) of battery voltage. It can range from 0 
            // to 65535
            public int BatteryVoltage;
            // Number of milliamps (mA) of instantaneous current drain. It can 
            // range from 0 to 32767 for charge and 0 to –32768 for discharge. 
            public int BatteryCurrent;
            //Number of milliseconds (mS) that is the time constant interval 
            // used in reporting BatteryAverageCurrent. 
            public int BatteryAverageCurrent;
            // Number of milliseconds (mS) that is the time constant interval 
            // used in reporting BatteryAverageCurrent. 

            public int BatteryAverageInterval;
            // Average number of milliamp hours (mAh) of long-term cumulative 
            // average discharge. It can range from 0 to –32768. This value is 
            // reset when the batteries are charged or changed. 

            public int BatterymAHourConsumed;
            // Battery temperature reported in 0.1 degree Celsius increments. It 
            // can range from –3276.8 to 3276.7. 
            public int BatteryTemperature;
            // Number of millivolts (mV) of backup battery voltage. It can range 
            // from 0 to 65535.
            public int BackupBatteryVoltage;
            // Type of battery.
            public BatteryChemistry BatteryChemistry;
            //  Add any extra information after the BatteryChemistry member.
        }

        [DllImport("CoreDLL")]
        public static extern int GetSystemPowerStatusEx2(
             SYSTEM_POWER_STATUS_EX2 statusInfo,
            int length,
            int getLatest
                );

        public static SYSTEM_POWER_STATUS_EX2 GetSystemPowerStatus()
        {
            SYSTEM_POWER_STATUS_EX2 retVal = new SYSTEM_POWER_STATUS_EX2();
            int result = GetSystemPowerStatusEx2(retVal, Marshal.SizeOf(retVal), 1);
            return retVal;
        }
        
        [DllImport("coredll.dll")]
        private static extern Int32 SetSystemPowerState(Char[] psState, Int32 StateFlags, Int32 Options);
        
        #endregion

        #region IDevice Members

        public string GetDeviceName()
        {
            int b1 = 0;
            Cipherlab.SystemAPI.Member.SysInfo sysInfo = new Cipherlab.SystemAPI.Member.SysInfo();
            b1 = Cipherlab.SystemAPI.Member.GetSysInfo(ref sysInfo);
            return sysInfo.SerialNum; 
        }

        public string GetDeviceIP()
        {
            int b1 = 0;
            Cipherlab.SystemAPI.Member.WlanAdptInfo wlanAdptInfo = new Cipherlab.SystemAPI.Member.WlanAdptInfo();
            b1 = Cipherlab.SystemAPI.Member.GetWlanIpInfo(ref wlanAdptInfo);

            return wlanAdptInfo.IPAddr;
        }

        public bool GetWiFiPowerStatus()
        {
            int b1 = 0;
            byte onOff = new byte();
            b1 = Cipherlab.SystemAPI.Member.GetWiFiPower(ref onOff);
            return onOff == 1;
        }

        public bool WiFiGetSignalQuality(out uint quality)
        {
            int b1 = 0;
            Cipherlab.SystemAPI.Member.CF10G_STATUS cfs = new Cipherlab.SystemAPI.Member.CF10G_STATUS();
            b1 = Cipherlab.SystemAPI.Member.GetCurrentStatus(ref cfs);

            quality = (uint)(100 + cfs.rssi);
            return this.GetWiFiPowerStatus();
        }

        public string GetModel()
        {
            return Scancode.TerminalID.Model.ToString();
        }

        public int GetBatterySuspendTimeout()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Control\Power");
                object oBatteryTimeout = key.GetValue("BattPowerOff");
                //object oACTimeOut = key.GetValue("ExtPowerOff");
                //object oScreenPowerOff = key.GetValue("ScreenPowerOff");

                //if (oBatteryTimeout is int)
                //{
                //    int v = (int)oBatteryTimeout;
                //    if (v > 0)
                //        retVal = Math.Min(retVal, v);
                //    txtBatteryIdleTimeOut.Text = oBatteryTimeout.ToString();
                //}
                return int.Parse(oBatteryTimeout.ToString());
            }
            catch 
            {
                return 0;
            }
        }

        public int GetBatteryLevel()
        {
            //SYSTEM_POWER_STATUS_EX2 _currentStatus = null;
            var _currentStatus = GetSystemPowerStatus();
            var value = _currentStatus.BatteryLifePercent;
            return value;
        }

        public bool SetBatterySuspendTimeout(uint timeout)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Control\Power", true);
                key.SetValue("BattPowerOff", timeout);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GetBatterySerialNumber()
        {
            return "Not exist";
        }

        public bool UpdateDeviceSettings(NameValueCollection m_settings)
        {
            bool reboot = false;
//#if !DEBUG
            int b1 = 0;
            int errCode = new int();
            int checkPar1 = new int(), checkPar2 = new int(), checkPar3 = new int(), checkPar4 = new int(), checkPar5 = new int(), checkPar6 = new int(), checkPar7 = new int(), checkPar8 = new int(), checkPar9 = new int(), checkPar10 = new int(), checkPar11 = new int(), checkPar12 = new int(), checkPar13 = new int(), checkPar14 = new int();
            foreach (var item in m_settings.Keys)
            {
                if (item.ToString().Substring(0, 9) == "Cipherlab")
                    try
                    {
                        var key = item.ToString().Replace("CipherlabReaderSettings.", "").Replace("CipherlabSystemSettings.", "");
                        var l = key.IndexOf(".");
                        var keyName = key.Substring(0, l).Replace("___", " ").Replace("__", "\\");
                        var valueName = key.Substring(l + 1, key.Length - l - 1);
                        var val = m_settings.Get(item.ToString());
                        var regVal = Registry.GetValue(keyName, valueName, "not exist");
                        if (regVal == null || val != regVal.ToString())
                        {
                            if ((item.ToString() == @"CipherlabSystemSettings.HKEY_LOCAL_MACHINE__init.Launch92")
                                //|| (item.ToString() == @"CipherlabSystemSettings.HKEY_LOCAL_MACHINE__Software__READER__2DLR_EX25__Center___Decoding__USER.Enable")
                                //|| (item.ToString() == @"CipherlabSystemSettings.HKEY_LOCAL_MACHINE__Software__READER__2DLR_EX25__Code128__USER.Length1")
                                )
                            {
                                if ((new FileInfo(@"\WINDOWS\CipherLabSettings\AppLock\AppLock_Applications.xml")).Exists)
                                {
                                    Registry.SetValue(keyName, valueName, val, RegistryValueKind.String);
                                    reboot = true;
                                    Shared.SaveToLogError("Set rebooting after update registry (" + item + ")");
                                }
                            }
                            else 
                                Registry.SetValue(keyName, valueName, val, valueName == "Launch92" || val == "" || !Shared.IsInt(val) ? RegistryValueKind.String : RegistryValueKind.DWord);
                        }
                    }
                    catch (Exception ex)
                    {
                        Shared.SaveToLogError("Error Update registry (" + item + ")");
                    }
                else
                    if (item.ToString().Substring(0, 11) == "scanSetting")
                    {
                        try
                        {
                            int val = int.Parse(m_settings.Get(item.ToString()));
                            switch (item.ToString())
                            {
                                case "scanSettings.EAN13.ENABLE":
                                    b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.EAN13.TO_ISBN":
                                    b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                    if (b1 == 0 && checkPar3 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref checkPar1, ref checkPar2, ref val, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.EAN13.TO_ISSN":
                                    b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                    if (b1 == 0 && checkPar9 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref val, ref checkPar10);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.EAN13.SEND_CHECK":
                                    b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                    if (b1 == 0 && checkPar10 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.EAN13.SEND_SYS":
                                    break;

                                case "scanSettings.I25.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.I25.MIN_LENGTH":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.I25.MAX_LENGTH":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar3 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref checkPar1, ref checkPar2, ref val, ref checkPar4, ref checkPar5, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.I25.ENABLE_CHECK":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar4 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref val, ref checkPar5, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.I25.SEND_CHECK":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar5 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref val, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.I25.CONVERT_TO_EAN13":
                                    b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar6 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Interleaved2Of5_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;

                                case "scanSettings.CODE128.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE128.MIN_LENGTH":
                                    break;
                                case "scanSettings.CODE128.MAX_LENGTH":
                                    break;
                                case "scanSettings.CODE128.ENABLE_GS1_128":
                                    b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;

                                case "scanSettings.CODE39.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE39.MIN_LENGTH":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar8 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref val, ref checkPar9);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE39.MAX_LENGTH":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar9 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE39.ENABLE_CHECK":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar5 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref val, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE39.SEND_CHECK":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar6 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref val, ref checkPar7, ref checkPar8, ref checkPar9);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE39.FULL_ASCII":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar7 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref val, ref checkPar8, ref checkPar9);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;

                                case "scanSettings.CODEBAR.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Codabar_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Codabar_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE93.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code93_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code93_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.EAN8.ENABLE":
                                    b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.GS1_14.ENABLE":
                                    b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.GS1_EXP.ENABLE":
                                    b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4);
                                    if (b1 == 0 && checkPar3 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('w', ref checkPar1, ref checkPar2, ref val, ref checkPar4);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.GS1_LIMIT.ENABLE":
                                    b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.GS1_DataBar_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.MSI.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Msi_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Msi_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.PHARMA39.ENABLE":
                                    break;
                                case "scanSettings.S25.ENABLE":
                                    break;
                                case "scanSettings.TRIOPTIC.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code39_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }                                    
                                    break;
                                case "scanSettings.UPCA.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Upc_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12, ref checkPar13, ref checkPar14);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Upc_2D_SE4500('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12, ref checkPar13, ref checkPar14);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.UPCE0.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Upc_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12, ref checkPar13, ref checkPar14);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Upc_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12, ref checkPar13, ref checkPar14);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }            
                                    break;
                                case "scanSettings.PDF417.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.MicroPDF417.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.QRCode.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar8 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref val, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.MaxiCode.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar7 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref val, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.DataMatrix.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar4 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref val, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Aztec.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref checkPar11, ref checkPar12);
                                    if (b1 == 0 && checkPar11 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Symbologies_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10, ref val, ref checkPar12);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Plessey.ENABLE":
                                    break;
                                case "scanSettings.Telepen.ENABLE":
                                    break;
                                case "scanSettings.USPlanet.ENABLE":
                                    b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('w', ref checkPar2, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.USPostnet.ENABLE":
                                    b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Netherlands.ENABLE":
                                    break;
                                case "scanSettings.JapanPostal.ENABLE":
                                    b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                    if (b1 == 0 && checkPar5 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref val, ref checkPar6, ref checkPar7, ref checkPar8);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.AustralianPostal.ENABLE":
                                    b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                    if (b1 == 0 && checkPar6 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.PostalCode_2D_SE4500('w', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref val, ref checkPar7, ref checkPar8);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CompositeCCC.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Composite_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Composite_2D_SE4500('w', ref val, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CompositeCCAB.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Composite_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Composite_2D_SE4500('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Matrix25.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Matrix2Of5_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Matrix2Of5_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Discrete25.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Industrial2Of5_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Industrial2Of5_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.ISBT128.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4, ref checkPar5);
                                    if (b1 == 0 && checkPar3 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code128_2D_SE4500('w', ref checkPar1, ref checkPar2, ref val, ref checkPar4, ref checkPar5);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.CODE11.ENABLE":
                                    b1 = Reader.engine2D_SE4500.Code11_2D_SE4500('r', ref checkPar1);
                                    if (b1 == 0 && checkPar1 != val)
                                    {
                                        b1 = Reader.engine2D_SE4500.Code11_2D_SE4500('w', ref val);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                            }
                            if (errCode != 0)
                                Shared.SaveToLogError("Error Update scaner parameter (errCode = " + errCode + ") :" + item + " = " + val);
                        }
                        catch
                        {
                            Shared.SaveToLogError("Error Update scaner parameter: " + item);
                        }
                    }
            }
             
#if !DEBUG
            if (reboot)
            {
                Shared.SaveToLogError("Error - Rebooting Device");
                const int POWER_STATE_RESET = 0x800000;
                SetSystemPowerState(null, POWER_STATE_RESET, 0);
            }
#endif
            return true;
        }

        #endregion
    }
}
