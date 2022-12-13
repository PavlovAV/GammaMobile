using System;
using System.Windows.Forms;
using System.Threading;
using Datalogic.API;
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
            if (deviceName.Contains("Falcon"))
            {
                Decode.SetWedge(WedgeType.Barcode, false);
            }
#if !DEBUG
            UpdateProgram.DropFlagUpdateLoading();
            int num = 0;
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(UpdateProgram.LoadUpdate);
            // создаем таймер
            System.Threading.Timer timer = new System.Threading.Timer(tm, num, 30000, 1800000);
#endif

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