using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using gamma_mob.Common;
using System;
using Microsoft.Win32;
using Datalogic.API;
using System.Security.Cryptography;

namespace gamma_mob
{
    /// <summary>
    ///     Summary description for Settings.
    /// </summary>
    public class Settings
    {
        private static readonly NameValueCollection m_settings;
        private static readonly string m_settingsPath;
        private static readonly string m_currentSecondServerFlag;
        private static readonly string m_settingsCPath;

        static Settings()
        {
            m_settingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            m_currentSecondServerFlag = m_settingsPath + @"\CurrentSecondServer.flg";
            m_settingsPath += @"\Settings.xml";
            m_settingsCPath = m_settingsPath + @"c";
/*#if DEBUG
//Для формирования нового зашифрованного файла настроек.
//1. внести изменения в settings.xml
//2. снять комментарий и запустить в режиме Debug
//3. новый файл settings.xmlc скопировать из ТСД (Program files\gamma_mob)
            {
                FileStream stream = new FileStream(m_settingsCPath, FileMode.OpenOrCreate, FileAccess.Write);

                DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

                cryptic.Key = ASCIIEncoding.ASCII.GetBytes("GammaMob");
                cryptic.IV = ASCIIEncoding.ASCII.GetBytes("GammaMob");

                CryptoStream crStream = new CryptoStream(stream,
                   cryptic.CreateEncryptor(), CryptoStreamMode.Write);

                var f = File.OpenText(m_settingsPath);
                string ff = f.ReadToEnd();
                byte[] data = ASCIIEncoding.ASCII.GetBytes(ff);

                crStream.Write(data, 0, data.Length);
                f.Close();
                crStream.Close();
                stream.Close();
            }
#endif
*/
            if (!File.Exists(m_settingsPath) && !File.Exists(m_settingsCPath))
            {
                Shared.SaveToLog("Error Settings files in " + m_settingsPath.Replace(@"\Settings.xml", @"\") + " could not be found.");
                throw new FileNotFoundException(m_settingsPath + " could not be found.");
            }
            else
            {
                var xdoc = new XmlDocument();
                //Если есть незашифрованный - то используем его
                if (File.Exists(m_settingsPath))
                {
                    xdoc.Load(m_settingsPath);
                    Shared.SaveToLog("Settings load from " + m_settingsPath);
                }
                else
                {
                    
                                
                    FileStream stream_ = new FileStream(m_settingsCPath,
                                                  FileMode.Open, FileAccess.Read);

                    DESCryptoServiceProvider cryptic_ = new DESCryptoServiceProvider();

                    cryptic_.Key = ASCIIEncoding.ASCII.GetBytes("GammaMob");
                    cryptic_.IV = ASCIIEncoding.ASCII.GetBytes("GammaMob");

                    CryptoStream crStream_ = new CryptoStream(stream_,
                        cryptic_.CreateDecryptor(), CryptoStreamMode.Read);

                    StreamReader reader_ = new StreamReader(crStream_);
                    xdoc.Load(reader_);

                    reader_.Close();
                    stream_.Close();
                    Shared.SaveToLog("Settings load from " + m_settingsCPath);
                }

                //var xdoc = new XmlDocument();
                //xdoc.Load(m_settingsPath);
                XmlElement root = xdoc.DocumentElement;
                
                m_settings = new NameValueCollection();
                if (root.ChildNodes.Count == 1)
                {
                    XmlNode node = root.ChildNodes.Item(0);
                    XmlNodeList nodeList = node.ChildNodes;
                    for (var i = 0; i < nodeList.Count; i++)
                    {
                        m_settings.Add("progSettings.Gamma." + nodeList.Item(i).Attributes["key"].Value, nodeList.Item(i).Attributes["value"].Value);
                    }
                    if ((m_settings.Get("progSettings.Gamma.SecondServerIP") ?? String.Empty) == String.Empty)
                        m_settings.Add("progSettings.Gamma.SecondServerIP", "");

                    if (ServerIP != null && ServerIP.Substring(0, 3) != "192")
                    {
                        if (SecondServerIP == "")
                            SecondServerIP = ServerIP;
                        try
                        {
                            if (!File.Exists(m_currentSecondServerFlag))
                                File.CreateText(m_currentSecondServerFlag);
                        }
                        catch
                        {
                            Shared.SaveToLog("Error Create(m_currentSecondServerFlag)");
                        }
                    }

                }
                else
                {
                    for (var h = 0; h < root.ChildNodes.Count; h++)
                    {
                        XmlNode nodeSettings = root.ChildNodes.Item(h);
                        XmlNodeList nodeSettingsList = nodeSettings.ChildNodes;
                        for (var j = 0; j < nodeSettingsList.Count; j++)
                        {
                            XmlNode node = nodeSettings.ChildNodes.Item(j);
                            XmlNodeList nodeList = node.ChildNodes;
                            for (var i = 0; i < nodeList.Count; i++)
                            {
                                m_settings.Add(nodeSettings.Name + "." + node.Name + "." + nodeList.Item(i).Attributes["key"].Value, nodeList.Item(i).Attributes["value"].Value);
                            }
                        }
                    }
                    UpdateDeviceSettings();
                }
                m_settings.Add("progSettings.Gamma.CurrentServer", File.Exists(m_currentSecondServerFlag)
                     ? m_settings.Get("progSettings.Gamma.SecondServerIP") : m_settings.Get("progSettings.Gamma.ServerIP"));
                Shared.SaveToLog("CurrentServer/DB " + CurrentServer + "/" + Database);
            }
        }

        public static string ServerIP
        {
            get { return m_settings.Get("progSettings.Gamma.ServerIP"); }
            set { m_settings.Set("progSettings.Gamma.ServerIP", value); }
        }

        public static string UserName
        {
            get { return m_settings.Get("progSettings.Gamma.UserName"); }
            set { m_settings.Set("progSettings.Gamma.UserName", value); }
        }

        public static string Password
        {
            get { return m_settings.Get("progSettings.Gamma.Password"); }
            set { m_settings.Set("progSettings.Gamma.Password", value); }
        }

        public static string Database
        {
            get { return m_settings.Get("progSettings.Gamma.Database"); }
            set { m_settings.Set("progSettings.Gamma.Database", value); }
        }

        public static string TimeOut
        {
            get { return m_settings.Get("progSettings.Gamma.TimeOut"); }
            set { m_settings.Set("progSettings.Gamma.TimeOut", value); }
        }

        public static string SecondServerIP
        {
            get { return m_settings.Get("progSettings.Gamma.SecondServerIP"); }
            set { m_settings.Set("progSettings.Gamma.SecondServerIP", value); }
        }

        public static string CurrentServer
        {
            get { return m_settings.Get("progSettings.Gamma.CurrentServer"); }
            //set { m_settings.Set("CurrentServer", value); }
        }

        public static void Update()
        {
            var tw = new XmlTextWriter(m_settingsPath, Encoding.UTF8);
            tw.WriteStartDocument();
            tw.WriteStartElement("configuration");
            tw.WriteStartElement("progSettings");

            for (int i = 0; i < m_settings.Count; ++i)
            {
                tw.WriteStartElement("add");
                tw.WriteStartAttribute("key", string.Empty);
                tw.WriteRaw(m_settings.GetKey(i));
                tw.WriteEndAttribute();

                tw.WriteStartAttribute("value", string.Empty);
                tw.WriteRaw(m_settings.Get(i));
                tw.WriteEndAttribute();
                tw.WriteEndElement();
            }

            tw.WriteEndElement();
            tw.WriteEndElement();

            tw.Close();
        }

        public static bool SetCurrentInternalServer()
        {
            bool ret = false;
            try
            {
                if (File.Exists(m_currentSecondServerFlag)) File.Delete(m_currentSecondServerFlag);
                m_settings.Set("progSettings.Gamma.CurrentServer", ServerIP);
                ret = true;
            }
            catch
            {
                Shared.SaveToLog("Error Delete(m_currentSecondServerFlag)");
            }
            return ret;
        }

        public static bool SetCurrentExternalServer()
        {
            bool ret = false;
            if (SecondServerIP != "")
            {
                try
                {
                    File.CreateText(m_currentSecondServerFlag);
                    m_settings.Set("progSettings.Gamma.CurrentServer", SecondServerIP);
                    ret = true;
                }
                catch
                {
                    Shared.SaveToLog("Error Create(m_currentSecondServerFlag)");
                }
            }
            return ret;
        }
        
        private static void UpdateDeviceSettings()
        {
            bool reboot = false;
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
                            Registry.SetValue(keyName, valueName, val, valueName == "Password" || val == "" || !IsInt(val) ? RegistryValueKind.String : RegistryValueKind.DWord);
                    }
                    catch (Exception ex)
                    {
                        Shared.SaveToLog("Error Update registry ("+item+")");
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
                                    par = Param.CODE39_ENABLE;
                                    break;
                                case "scanSettings.CODE93.ENABLE":
                                    par = Param.CODE39_ENABLE;
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
                            }
                            var curVal = Decode.GetParam(par);
                            if (curVal != val)
                                Decode.SetParam(par, val);
                        }
                        catch
                        {
                            Shared.SaveToLog("Error Update scaner parameter (" + item + ")");
                        }
                    }

            }
#if !DEBUG
            if (reboot)
            {
                Shared.SaveToLog("Error - Rebooting Device");
                Device.Reset(Device.BootType.Warm);
            }
#endif
        }

        private static bool IsInt(string s)
        {
            bool isInt = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsDigit(s[i]))
                {
                    isInt = false;
                    break;
                }
            }
            return isInt;
        }
    }
}