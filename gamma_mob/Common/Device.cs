using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Net.NetworkInformation;
using Microsoft.Win32;

namespace gamma_mob.Common
{
    public class Device : IDevice
    {
        #region IDevice Members

        public bool reboot { get; set; }

        public virtual void EnableWiFi()
        {

        }

        public virtual string GetDeviceName()
        {
            return "Error";
        }

        private string _wirelessAdapterName { get; set;}
        public string wirelessAdapterName
        {
            get
            {
                if (_wirelessAdapterName == null || _wirelessAdapterName == String.Empty)
                    _wirelessAdapterName = GetWirelessAdapterName();
                return _wirelessAdapterName;
            }
        }
        
        private string GetWirelessAdapterName()
        {
            EnableWiFi();
            string wirelessAdapter = String.Empty;
            string associatedAccessPoint = String.Empty;
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    wirelessAdapter = adapter.Name;
                }
                else if (adapter is WirelessNetworkInterface)
                {
                    var wni = (adapter as WirelessNetworkInterface);
                    wirelessAdapter = adapter.ToString();
                    associatedAccessPoint = wni.AssociatedAccessPoint;
                }
            }
            return wirelessAdapter;
        }

        public void UpdateRegistry(string keyName, string valueName, string val)
        {
            UpdateRegistry(keyName, valueName, val, null);
        }

        public void UpdateRegistry(string keyName, string valueName, string val, RegistryValueKind? valueKind)
        {
            if ((valueKind == null && valueName == "Save4" || valueName == "Save3" || valueName == "Save2" || valueName == "Save1" || valueName == "Data 0" || valueName == "succeeded")
                || (valueKind != null && valueKind == RegistryValueKind.Binary))
            {
                var dataR = val.Split(',')
                            .Select(x => Convert.ToByte(x, 16))
                            .ToArray();
                Registry.SetValue(keyName, valueName, dataR, RegistryValueKind.Binary);
                Shared.SaveToLogInformation("Update registry " + keyName + ": " + valueName + " = " + val);
            }
            else if ((valueKind == null && valueName == "DefaultGateway" || valueName == "IpAddress" || valueName == "Subnetmask")
                || (valueKind != null && valueKind == RegistryValueKind.MultiString))
            {
                string[] dataR = { val };
                Registry.SetValue(keyName, valueName, dataR, RegistryValueKind.MultiString);
                Shared.SaveToLogInformation("Update registry " + keyName + ": " + valueName + " = " + val);
            }
            else
            {
                if (valueKind == null)
                {
                    valueKind = //valueName == "ProfileList" ? RegistryValueKind.MultiString :
                                valueName == "Password" || val == "" || !Shared.IsInt(val)
                                ? RegistryValueKind.String
                                : RegistryValueKind.DWord;
                }
                if (valueKind != null)
                {
                    Registry.SetValue(keyName, valueName, val, (RegistryValueKind)valueKind);
                    Shared.SaveToLogInformation("Update registry " + keyName + ": " + valueName + " = " + val);
                }
                else
                    Shared.SaveToLogError("Error Update registry " + keyName + ": " + valueName + " = " + val + "; valKind = null");
            }
            if ((valueKind == null) &&
                (valueName == "Launch92" || valueName == "DduLoad" || valueName == "Password" || valueName == "EnableStartMenu" || valueName == "ActiveConfig" || valueName == "SSID" || valueName == "EnableDHCP" || valueName == "DefaultGateway" || valueName == "IpAddress" || valueName == "Subnetmask"))
                reboot = true;
        }

        public void UpdateWiFiProfileSettings(string item, string val)
        {
            try
            {
                var index = item.ToString().IndexOf(".");
                var set = item.ToString().Substring(0, index);
                var key = item.ToString().Replace(set + ".", "");
                var l = key.IndexOf(".");
                var keyName = key.Substring(0, l).Replace("__", "\\").Replace("SkbOp", "{").Replace("SkbCl", "}");
                var valueName = key.Substring(l + 1, key.Length - l - 1);
                //var val = m_settings.Get(item.ToString());
                var regVal = (Registry.GetValue(keyName, valueName, "not exist") ?? String.Empty).ToString();
                
                if (regVal == null || val != regVal)
                {
                    UpdateRegistry(keyName, valueName, val);
                }
            }
            catch (Exception ex)
            {
                Shared.SaveToLogError("Error Update registry " + item);
            }
        }

        public void UpdateNetworkSettings(string item, string val)
        {
            try
            {
                var key = item.ToString().Replace("networkSettings.DeviceIP.", "");
                var devN = GetDeviceName();
                if (key == GetDeviceName())
                {
                    string regKey = @"HKEY_LOCAL_MACHINE\Comm\" + wirelessAdapterName + @"\Parms\TcpIp";
                    string enableDhcpKey = "EnableDHCP";
                    string gatewayKey = "DefaultGateway";
                    string ipAddressKey = "IpAddress";
                    string subnetMaskKey = "Subnetmask";

                    //var val = m_settings.Get(item.ToString());
                    string regVal = null;
                    if (val == "DHCP")
                    {
                        regVal = (Registry.GetValue(regKey, enableDhcpKey, "not exist") ?? String.Empty).ToString();
                        if (regVal == null || "1" != regVal)
                        {
                            UpdateRegistry(regKey, enableDhcpKey, "1");
                        }
                    }
                    else if (val.Length > 0)
                    {
                        regVal = (Registry.GetValue(regKey, enableDhcpKey, "not exist") ?? String.Empty).ToString();
                        if (regVal == null || "0" != regVal)
                        {
                            UpdateRegistry(regKey, enableDhcpKey, "0");
                        }
                        object regValMultiString;
                        var index1 = val.IndexOf("_");
                        var ip = val.Substring(0, index1);
                        regValMultiString = Registry.GetValue(regKey, ipAddressKey, new string[1] { "not exist" });
                        regVal = (regValMultiString is string[]) ? ((string[])regValMultiString)[0].ToString()
                            : (regValMultiString is string) ? regValMultiString.ToString() 
                            : null;
                        if (regVal == null || ip != regVal)
                        {
                            UpdateRegistry(regKey, ipAddressKey, ip);
                        }
                        var index2 = val.IndexOf("#");
                        if (index2 > 0)
                        {
                            var mask = val.Substring(index1 + 1, index2 - index1 - 1);
                            regValMultiString = Registry.GetValue(regKey, subnetMaskKey, new string[1] { "not exist" });
                            regVal = (regValMultiString is string[]) ? ((string[])regValMultiString)[0].ToString()
                            : (regValMultiString is string) ? regValMultiString.ToString()
                            : null;
                            if (regVal == null || mask != regVal)
                            {
                                UpdateRegistry(regKey, subnetMaskKey, mask);
                            }
                            if (index2 < val.Length)
                            {
                                var strGateway = val.Substring(index2 + 1, val.Length - index2 - 1);
                                regValMultiString = Registry.GetValue(regKey, gatewayKey, new string[1] { "not exist" });
                                regVal = (regValMultiString is string[]) ? ((string[])regValMultiString)[0].ToString()
                                    : (regValMultiString is string) ? regValMultiString.ToString()
                                    : null;
                                if (regVal == null || strGateway != regVal)
                                {
                                    UpdateRegistry(regKey, gatewayKey, strGateway);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Shared.SaveToLogError("Error Update registry " + item);
            }
        }

        #endregion
    }
}
