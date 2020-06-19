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
        public static readonly string _logFile = Path.Combine(_executablePath, string.Format("{0:yyyMMdd}.log", DateTime.Now));


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
#if !DEBUG
            UpdateProgram.DropFlagUpdateLoading();
            int num = 0;
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(UpdateProgram.LoadUpdate);
            // создаем таймер
            System.Threading.Timer timer = new System.Threading.Timer(tm, num, 2000, 300000);
#endif
            try
            {
                if (!File.Exists(_logFile)) // may have to specify path here!
                {
                    // may have to specify path here!
                    File.Create(_logFile).Close();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

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

        public static void SaveToLog(string log)
        {
            try
            {
                TextWriter swFile = new StreamWriter(
                new FileStream(_logFile,
                               FileMode.Append),
                System.Text.Encoding.ASCII);
                swFile.WriteLine(string.Format(@"{0:yyyy.MM.dd HH:mm:ss} : ", DateTime.Now) + log);
                swFile.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}