using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Datalogic.API;
using System.Net;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace gamma_mob.Common
{
    public class DeviceDatalogic : IDevice
    {

        #region IDevice Members

        public string GetDeviceName()
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
            bool reboot = false;
//#if !DEBUG
            foreach (var item in m_settings.Keys)
            {
                if (item.ToString().Substring(0, 11) == "dduSettings" || item.ToString().Substring(0, 11) == "dlbSettings" || item.ToString().Substring(0, 11) == "backlightSe")
                    try
                    {
                        var key = item.ToString().Replace("dduSettings.", "").Replace("dlbSettings.", "").Replace("backlightSettings.", "");
                        var l = key.IndexOf(".");
                        var keyName = key.Substring(0, l).Replace("__", "\\");
                        var valueName = key.Substring(l + 1, key.Length - l - 1);
                        var val = m_settings.Get(item.ToString());
                        var regVal = Registry.GetValue(keyName, valueName, "not exist");
                        if (((item.ToString() == @"dduSettings.HKEY_CURRENT_USER__Software__Datalogic__DDU__GENERAL.DduLoad")
                            || (item.ToString() == @"dduSettings.HKEY_CURRENT_USER__Software__Datalogic__DDU__GENERAL.Password")
                            || (item.ToString() == @"dduSettings.HKEY_CURRENT_USER__Software__Datalogic__DDU__UserInterface.EnableStartMenu"))
                            && val != regVal.ToString())
                            reboot = true;
                        
                        if (regVal == null || val != regVal.ToString())
                            Registry.SetValue(keyName, valueName, val, valueName == "Password" || val == "" || !Shared.IsInt(val) ? RegistryValueKind.String : RegistryValueKind.DWord);
                    }
                    catch (Exception ex)
                    {
                        Shared.SaveToLogError("Error Update registry ("+item+")");
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
                Device.Reset(Device.BootType.Warm);
            }
#endif
            return true;
        }

        #endregion

    }
}
