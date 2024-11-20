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
using System.Reflection;

namespace gamma_mob.Common
{
    public class DeviceCipherlab : Device, IDeviceExtended
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

        private Assembly assembly_system_net;
        private Type type_system_net;
        private Assembly assembly_reader_net;
        private Type type_reader_engine1D;
        private Type type_reader_engine2D;
        private Type type_reader_other;

        public DeviceCipherlab()
        {
            //{5.2.29058}
            //{6.0.0}
            assembly_system_net = Environment.OSVersion.Version.Major.ToString() == "6" ? Assembly.LoadFrom("SystemCE_Net.dll") : Environment.OSVersion.Version.Major.ToString() == "5" ? Assembly.LoadFrom("SystemMobile_Net.dll") : null;
            type_system_net = assembly_system_net != null ? assembly_system_net.GetType("Cipherlab.SystemAPI.Member") : null;
            assembly_reader_net = Environment.OSVersion.Version.Major.ToString() == "6" ? Assembly.LoadFrom("Reader_Ce_Net.dll") : Environment.OSVersion.Version.Major.ToString() == "5" ? Assembly.LoadFrom("ReaderDllMobile_Net.dll") : null;
            type_reader_engine1D = assembly_reader_net != null ? assembly_reader_net.GetType("Reader.engine1D_SE955") : null;
            type_reader_engine2D = assembly_reader_net != null ? assembly_reader_net.GetType("Reader.engine2D_SE4500") : null;
            type_reader_other = assembly_reader_net != null ? assembly_reader_net.GetType("Reader.other") : null;
        }

        #region IDevice Members

        public override void EnableWiFi()
        {
            //int b1 = 0;
            //b1 = Cipherlab.SystemAPI.Member.SetWiFiPower(1);
            if (type_system_net != null)
            {
                MethodInfo methodInfo = type_system_net.GetMethod("SetWiFiPower");
                if (methodInfo != null)
                {
                    object result = null;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type_system_net);
                    if (parameters.Length == 0)
                    {
                        //This works fine
                        result = methodInfo.Invoke(classInstance, null);
                    }
                    else
                    {
                        byte par = 1;
                        object[] parametersArray = new object[] { par };

                        //The invoke does NOT work it throws "Object does not match target type"             
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }

        }

        public override string GetDeviceName()
        {
            //int b1 = 0;
            string SerialNum = "SerialNum";/*
            Cipherlab.SystemAPI.Member.SysInfo sysInfo = new Cipherlab.SystemAPI.Member.SysInfo();
#if !DEBUG
            b1 = Cipherlab.SystemAPI.Member.GetSysInfo(ref sysInfo);
            return sysInfo.SerialNum; 
#else
            return "SerialNum";
#endif*/
            try
            {
                if (type_system_net != null)
                {
                    MethodInfo methodInfo = type_system_net.GetMethod("GetSysInfo");
                    var parameterInfo = type_system_net.GetNestedType("SysInfo", BindingFlags.Public);
                    object[] parametersArray;
                    if (methodInfo != null)
                    {
                        object result = null;
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object classInstance = Activator.CreateInstance(type_system_net);
                        //if (parameters.Length == 0)
                        //{
                        //This works fine
                        //    result = methodInfo.Invoke(classInstance, null);
                        //}
                        //else
                        //{
                        //var t = new parameters[0].ReflectedType();
                        //var t = parameters[0];
                        //Type genericType = t.ParameterType;
                        object parserFunctionParameter = Activator.CreateInstance(parameterInfo);
                        parametersArray = new object[] { parserFunctionParameter };
                        //The invoke does NOT work it throws "Object does not match target type"             
                        result = methodInfo.Invoke(classInstance, parametersArray);
                        var r = parserFunctionParameter.GetType().GetField("SerialNum", BindingFlags.Public | BindingFlags.Instance);
                        SerialNum = r.GetValue(parserFunctionParameter).ToString();
                        //}
                    }
                }
            }
            catch
            {
                SerialNum = "Error";
            }
            return SerialNum;
        }

        public string GetDeviceIP()
        {
            //int b1 = 0;
            string IPAddr = "000.000.000.000";
            //Cipherlab.SystemAPI.Member.WlanAdptInfo wlanAdptInfo = new Cipherlab.SystemAPI.Member.WlanAdptInfo();
//#if !DEBUG
//            b1 = Cipherlab.SystemAPI.Member.GetWlanIpInfo(ref wlanAdptInfo);

//            return wlanAdptInfo.IPAddr;
//#else
//            return "000.000.000.000";
//#endif
            try
            {
                if (type_system_net != null)
                {
                    MethodInfo methodInfo = type_system_net.GetMethod("GetWlanIpInfo");
                    var parameterInfo = type_system_net.GetNestedType("WlanAdptInfo", BindingFlags.Public);
                    object[] parametersArray;
                    if (methodInfo != null)
                    {
                        object result = null;
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object classInstance = Activator.CreateInstance(type_system_net);
                        object parserFunctionParameter = Activator.CreateInstance(parameterInfo);
                        parametersArray = new object[] { parserFunctionParameter };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                        var r = parserFunctionParameter.GetType().GetField("IPAddr", BindingFlags.Public | BindingFlags.Instance);
                        IPAddr = r.GetValue(parserFunctionParameter).ToString();
                        //}
                    }
                }
            }
            catch
            {
                IPAddr = "Error";
            }
            return IPAddr;
        }

        public bool GetWiFiPowerStatus()
        {
            //int b1 = 0;
            byte onOff = new byte();
            bool WiFiStatus = false;
//#if !DEBUG
//            b1 = Cipherlab.SystemAPI.Member.GetWiFiPower(ref onOff);
//            return onOff == 1;
//#else
//            return true;
//#endif
            try
            {
                if (type_system_net != null)
                {
                    MethodInfo methodInfo = type_system_net.GetMethod("GetWiFiPower");
                    //var parameterInfo = type.GetNestedType("WlanAdptInfo", BindingFlags.Public);
                    object[] parametersArray;
                    if (methodInfo != null)
                    {
                        object result = null;
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object classInstance = Activator.CreateInstance(type_system_net);
                        //object parserFunctionParameter = Activator.CreateInstance(parameterInfo);
                        parametersArray = new object[] { onOff };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                        onOff = Convert.ToByte(parametersArray[0].ToString());
                        WiFiStatus = onOff == 1;
                        //var r = parserFunctionParameter.GetType().GetField("IPAddr", BindingFlags.Public | BindingFlags.Instance);
                        //IPAddr = r.GetValue(parserFunctionParameter).ToString();
                        //}
                    }
                }
            }
            catch
            {
                WiFiStatus = true;
            }
            return WiFiStatus;
        }

        public bool WiFiGetSignalQuality(out uint quality)
        {
            int b1 = 0;
            bool ReturnSinalQuality = false;
            quality = 0;
            //Cipherlab.SystemAPI.Member.CF10G_STATUS cfs = new Cipherlab.SystemAPI.Member.CF10G_STATUS();
            //b1 = Cipherlab.SystemAPI.Member.GetCurrentStatus(ref cfs);

            //quality = cfs.rssi < -100 ? 0 : (uint)(100 + cfs.rssi);
            //return quality > 0 && this.GetWiFiPowerStatus();
            try
            {
                if (type_system_net != null)
                {
                    MethodInfo methodInfo = type_system_net.GetMethod("GetCurrentStatus");
                    var parameterInfo = type_system_net.GetNestedType("CF10G_STATUS", BindingFlags.Public);
                    object[] parametersArray;
                    if (methodInfo != null)
                    {
                        object result = null;
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object classInstance = Activator.CreateInstance(type_system_net);
                        object parserFunctionParameter = Activator.CreateInstance(parameterInfo);
                        parametersArray = new object[] { parserFunctionParameter };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                        var r = parserFunctionParameter.GetType().GetField("rssi", BindingFlags.Public | BindingFlags.Instance);
                        var rssi = r.GetValue(parserFunctionParameter).ToString();
                        quality = Convert.ToInt32(rssi) < -100 ? (uint)0 : (uint)(100 + Convert.ToInt32(rssi));
                        ReturnSinalQuality = quality > 0 && this.GetWiFiPowerStatus();
                        //}
                    }
                }
            }
            catch
            {
                ReturnSinalQuality = true;
            }
            return ReturnSinalQuality;
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
            reboot = false;
            int b1 = 0;
            int errCode = new int();
            int checkPar1 = new int(), checkPar2 = new int(), checkPar3 = new int(), checkPar4 = new int(), checkPar5 = new int(), checkPar6 = new int(), checkPar7 = new int(), checkPar8 = new int(), checkPar9 = new int(), checkPar10 = new int(), checkPar11 = new int(), checkPar12 = new int(), checkPar13 = new int(), checkPar14 = new int();
            foreach (var item in m_settings.Keys)
            {
                if (item.ToString().Substring(0, 9) == "Cipherlab")
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
                        regVal = (Registry.GetValue(keyName, valueName, "not exist") ?? String.Empty).ToString();
                        if ((item.ToString() == @"CipherlabSystemSettings.HKEY_LOCAL_MACHINE__init.Launch92")
                            && (regVal == null || val != regVal))
                        {
                            if ((new FileInfo(@"\WINDOWS\CipherLabSettings\AppLock\AppLock_Applications.xml")).Exists)
                            {
                                base.UpdateRegistry(keyName, valueName, val, RegistryValueKind.String);
                            }
                        }
                        else
                        {
                            if (regVal == null || val != regVal)
                            {
                                base.UpdateRegistry(keyName, valueName, val);
                            }
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
                       /* рабочий вариант через библиотеку cipherlab
                        try
                        {
                            var key = item.ToString().Replace("networkSettings.DeviceIP.", "");
                            var devN = GetDeviceName();
                            if (key == GetDeviceName())
                            {
                                Cipherlab.SystemAPI.Member.WlanAdptInfo wlanAdptInfo = new Cipherlab.SystemAPI.Member.WlanAdptInfo();
                                b1 = Cipherlab.SystemAPI.Member.GetWlanIpInfo(ref wlanAdptInfo);
                                bool editable = false;
                                var val = m_settings.Get(item.ToString());
                                if (val == "DHCP")
                                {
                                    if (wlanAdptInfo.fUseDHCP == 0)
                                    {
                                        wlanAdptInfo.fUseDHCP = 1;
                                        b1 = Cipherlab.SystemAPI.Member.SetWlanIpInfo(ref wlanAdptInfo);
                                        reboot = true;
                                    }
                                }
                                else if (val.Length > 0)
                                {
                                    if (wlanAdptInfo.fUseDHCP == 1)
                                    {
                                        wlanAdptInfo.fUseDHCP = 0;
                                        editable = true;
                                    }
                                    var index1 = val.IndexOf("_");
                                    var ip = val.Substring(0, index1);
                                    if (wlanAdptInfo.IPAddr != ip)
                                    {
                                        wlanAdptInfo.IPAddr = ip;
                                        editable = true;
                                    }
                                    var index2 = val.IndexOf("#");
                                    if (index2 > 0)
                                    {
                                        var mask = val.Substring(index1 + 1, index2 - index1 - 1);
                                        if (wlanAdptInfo.SubnetMask != mask)
                                        {
                                            wlanAdptInfo.SubnetMask = mask;
                                            editable = true;
                                        }
                                        if (index2 < val.Length)
                                        {
                                            var strGateway = val.Substring(index2 + 1, val.Length - index2 - 1);
                                            if (wlanAdptInfo.Gateway != strGateway)
                                            {
                                                wlanAdptInfo.Gateway = strGateway;
                                                editable = true;
                                            }
                                        }
                                    }
                                    if (editable)
                                    {
                                        b1 = Cipherlab.SystemAPI.Member.SetWlanIpInfo(ref wlanAdptInfo);
                                        reboot = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Shared.SaveToLogError("Error Update registry " + item);
                        }*/
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
                                    b1 = Reader.engine1D_SE955.EanJan_1D_SE955('r', ref checkPar1, ref checkPar2, ref checkPar3);//, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8);
                                    if (type_reader_engine1D != null)
                                    {
                                        MethodInfo methodInfo = null;
                                        var methods = type_reader_engine1D.GetMethods();
                                        foreach (var method in methods)
                                        {
                                            if (method.Name == "EanJan_1D_SE955" && method.GetParameters().Count() == 4)
                                            {
                                                methodInfo = method;
                                                break;
                                            }
                                        }
                                        //var t = new Type[] { typeof(int), typeof(int).ReflectedType, typeof(int).ReflectedType, typeof(int).ReflectedType };
                                        //MethodInfo methodInfo = type_reader_engine1D.GetMethod("EanJan_1D_SE955", t);
                                        //var parameterInfo = type_system_net.GetNestedType("SysInfo", BindingFlags.Public);
                                        object[] parametersArray;
                                        if (methodInfo != null)
                                        {
                                            object result = null;
                                            ParameterInfo[] parameters = methodInfo.GetParameters();
                                            object classInstance = Activator.CreateInstance(type_reader_engine1D);
                                            //if (parameters.Length == 0)
                                            //{
                                            //This works fine
                                            //    result = methodInfo.Invoke(classInstance, null);
                                            //}
                                            //else
                                            //{
                                            //var t = new parameters[0].ReflectedType();
                                            //var t = parameters[0];
                                            //Type genericType = t.ParameterType;
                                            //object parserFunctionParameter = Activator.CreateInstance(parameterInfo);
                                            //parametersArray = new object[] { parserFunctionParameter };
                                            parametersArray = new object[] { 'r', checkPar1, checkPar2, checkPar3} ;//, checkPar4, checkPar5, checkPar6, checkPar7, checkPar8 };
                                            //The invoke does NOT work it throws "Object does not match target type"             
                                            result = methodInfo.Invoke(classInstance, parametersArray);
                                            //var r = parserFunctionParameter.GetType().GetField("SerialNum", BindingFlags.Public | BindingFlags.Instance);
                                            //SerialNum = r.GetValue(parserFunctionParameter).ToString();
                                            if ((int?)result == 0 && checkPar2 != val)
                                            {
                                                //b1 = Reader.engine2D_SE4500.EanJan_2D_SE4500_Ex('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4, ref checkPar5, ref checkPar6, ref checkPar7, ref checkPar8, ref checkPar9, ref checkPar10);
                                                parametersArray = new object[] { 'w', checkPar1, val, checkPar3} ;//, checkPar4, checkPar5, checkPar6, checkPar7, checkPar8 };
                                                result = methodInfo.Invoke(classInstance, parametersArray);
                                                //if (result == 0)
                                                //    errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                            }

                                            //}
                                        }
                                    }
                                    break;
                            }
                           /* switch (item.ToString())
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
                                case "scanSettings.Vibration.ENABLE":
                                    b1 = Reader.other.NotificationSettings('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4);
                                    if (b1 == 0 && checkPar2 != val)
                                    {
                                        b1 = Reader.other.NotificationSettings('w', ref checkPar1, ref val, ref checkPar3, ref checkPar4);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                                case "scanSettings.Vibration.TIME":
                                    b1 = Reader.other.NotificationSettings('r', ref checkPar1, ref checkPar2, ref checkPar3, ref checkPar4);
                                    if (b1 == 0 && checkPar3 != val)
                                    {
                                        b1 = Reader.other.NotificationSettings('w', ref checkPar1, ref checkPar2, ref val, ref checkPar4);
                                        if (b1 == 0)
                                            errCode = Reader.ReaderEngineAPI.GetErrorCode();
                                    }
                                    break;
                            }
                            if (errCode != 0)
                                Shared.SaveToLogError("Error Update scaner parameter (errCode = " + errCode + ") :" + item + " = " + val);
                        */
                        }
                        catch (Exception e)
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
