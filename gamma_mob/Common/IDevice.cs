using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace gamma_mob.Common
{
    public interface IDevice
    {
        bool reboot {get; set;}

        string GetDeviceName();

        void EnableWiFi();

        void UpdateRegistry(string keyName, string valueName, string val);

        void UpdateRegistry(string keyName, string valueName, string val, RegistryValueKind? valueKind);

        void UpdateWiFiProfileSettings(string item, string val);

        void UpdateNetworkSettings(string item, string val);

    }
}
