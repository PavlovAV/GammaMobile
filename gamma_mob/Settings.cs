using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using gamma_mob.Common;
using System;

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

        static Settings()
        {
            m_settingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            m_currentSecondServerFlag = m_settingsPath + @"\CurrentSecondServer.flg";
            m_settingsPath += @"\Settings.xml";

            if (!File.Exists(m_settingsPath))
                throw new FileNotFoundException(m_settingsPath + " could not be found.");

            var xdoc = new XmlDocument();
            xdoc.Load(m_settingsPath);
            XmlElement root = xdoc.DocumentElement;
            XmlNodeList nodeList = root.ChildNodes.Item(0).ChildNodes;

            // Add settings to the NameValueCollection.
            m_settings = new NameValueCollection();
            for (var i = 0; i < nodeList.Count; i++ )
            {
                m_settings.Add(nodeList.Item(i).Attributes["key"].Value, nodeList.Item(i).Attributes["value"].Value);
            }
            if ((m_settings.Get("SecondServerIP") ?? String.Empty) == String.Empty)
                m_settings.Add("SecondServerIP", "");
            if (ServerIP.Substring(0,3) != "192")
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
            m_settings.Add("CurrentServer", File.Exists(m_currentSecondServerFlag)
                 ? m_settings.Get("SecondServerIP") : m_settings.Get("ServerIP"));
            
        }

        public static string ServerIP
        {
            get { return m_settings.Get("ServerIP"); }
            set { m_settings.Set("ServerIP", value); }
        }

        public static string UserName
        {
            get { return m_settings.Get("UserName"); }
            set { m_settings.Set("UserName", value); }
        }

        public static string Password
        {
            get { return m_settings.Get("Password"); }
            set { m_settings.Set("Password", value); }
        }

        public static string Database
        {
            get { return m_settings.Get("Database"); }
            set { m_settings.Set("Database", value); }
        }

        public static string TimeOut
        {
            get { return m_settings.Get("TimeOut"); }
            set { m_settings.Set("TimeOut", value); }
        }

        public static string SecondServerIP
        {
            get { return m_settings.Get("SecondServerIP"); }
            set { m_settings.Set("SecondServerIP", value); }
        }

        public static string CurrentServer
        {
            get { return m_settings.Get("CurrentServer"); }
            //set { m_settings.Set("CurrentServer", value); }
        }

        public static void Update()
        {
            var tw = new XmlTextWriter(m_settingsPath, Encoding.UTF8);
            tw.WriteStartDocument();
            tw.WriteStartElement("configuration");
            tw.WriteStartElement("appSettings");

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
                m_settings.Set("CurrentServer", ServerIP);
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
                    m_settings.Set("CurrentServer", SecondServerIP);
                    ret = true;
                }
                catch
                {
                    Shared.SaveToLog("Error Create(m_currentSecondServerFlag)");
                }
            }
            return ret;
        }

    }
}