using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using gamma_mob.Common;
using System;
//using Microsoft.Win32;
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
                Shared.SaveToLogError("Error Settings files in " + m_settingsPath.Replace(@"\Settings.xml", @"\") + " could not be found.");
                throw new FileNotFoundException(m_settingsPath + " could not be found.");
            }
            else
            {
                var xdoc = new XmlDocument();
                //Если есть незашифрованный - то используем его
                if (File.Exists(m_settingsPath))
                {
                    xdoc.Load(m_settingsPath);
                    Shared.SaveToLogStartProgramInformation("Settings load from " + m_settingsPath);
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
                    Shared.SaveToLogStartProgramInformation("Settings load from " + m_settingsCPath);
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
                            Shared.SaveToLogError("Error Create(m_currentSecondServerFlag)");
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
                Shared.SaveToLogStartProgramInformation("CurrentServer/DB " + CurrentServer + "/" + Database);
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
                Shared.SaveToLogError("Error Delete(m_currentSecondServerFlag)");
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
                    Shared.SaveToLogError("Error Create(m_currentSecondServerFlag)");
                }
            }
            return ret;
        }
        
        private static void UpdateDeviceSettings()
        {
            Shared.Device.UpdateDeviceSettings(m_settings);
        }


    }
}