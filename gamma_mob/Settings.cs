using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace gamma_mob
{
    /// <summary>
    ///     Summary description for Settings.
    /// </summary>
    public class Settings
    {
        private static readonly NameValueCollection m_settings;
        private static readonly string m_settingsPath;

        static Settings()
        {
            m_settingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            m_settingsPath += @"\Settings.xml";

            if (!File.Exists(m_settingsPath))
                throw new FileNotFoundException(m_settingsPath + " could not be found.");

            var xdoc = new XmlDocument();
            xdoc.Load(m_settingsPath);
            XmlElement root = xdoc.DocumentElement;
            XmlNodeList nodeList = root.ChildNodes.Item(0).ChildNodes;

            // Add settings to the NameValueCollection.
            m_settings = new NameValueCollection();
            m_settings.Add("ServerIP", nodeList.Item(0).Attributes["value"].Value);
            m_settings.Add("Database", nodeList.Item(1).Attributes["value"].Value);
            m_settings.Add("UserName", nodeList.Item(2).Attributes["value"].Value);
            m_settings.Add("Password", nodeList.Item(3).Attributes["value"].Value);
            m_settings.Add("TimeOut", nodeList.Item(4).Attributes["value"].Value);
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
    }
}