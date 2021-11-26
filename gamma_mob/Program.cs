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


        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [MTAThread]
        private static void Main()
        
        {
            bool isNew;
            _mutex = new NamedMutex(false, "gammamob", out isNew);
            if (!isNew) return;
            Decode.SetWedge(WedgeType.Barcode, false);
//#if !DEBUG
            UpdateProgram.DropFlagUpdateLoading();
            int num = 0;
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(UpdateProgram.LoadUpdate);
            // создаем таймер
            System.Threading.Timer timer = new System.Threading.Timer(tm, num, 10000, 2700000);
//#endif
            int num_1 = 0;
                // устанавливаем метод обратного вызова
                TimerCallback tm_1 = new TimerCallback(CheckBatteryLevel);
                // создаем таймер
                System.Threading.Timer timer_1 = new System.Threading.Timer(tm_1, num_1, 300000,600000);

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

        public static void CheckBatteryLevel(object obj)
        {
            try
            {
                Shared.SaveToLog("Battery Level " + Device.GetBatteryLevel().ToString());
            }
            catch
            {
            }
        }
    }
}