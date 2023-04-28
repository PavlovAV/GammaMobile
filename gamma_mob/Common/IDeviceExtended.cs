using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace gamma_mob.Common
{
    public interface IDeviceExtended : IDevice
    {

        string GetDeviceIP();

        bool GetWiFiPowerStatus();

        bool WiFiGetSignalQuality(out uint quality);

        string GetModel();

        int GetBatterySuspendTimeout();

        int GetBatteryLevel();

        bool SetBatterySuspendTimeout(uint timeout);

        string GetBatterySerialNumber();

        bool UpdateDeviceSettings(NameValueCollection m_settings);

    }
}
