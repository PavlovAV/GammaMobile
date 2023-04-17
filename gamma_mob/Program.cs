using System;
using System.Windows.Forms;
using System.Threading;
using OpenNETCF.Threading;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using System.IO;
using System.Reflection;

namespace gamma_mob
{
    internal static class Program
    {
        private static NamedMutex _mutex;
        private static readonly string _executablePath = Application2.StartupPath + @"\";
        public static readonly string deviceName = System.Net.Dns.GetHostName();

        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [MTAThread]
        private static void Main()
        
        {
            bool isNew;
            _mutex = new NamedMutex(false, "gammamob", out isNew);
            if (!isNew) return;
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application2.Run(new MainForm());
                try
                {
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Doc*.xml");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }
            else BarcodeScanner.Scanner.Dispose();
        }       
    }
}