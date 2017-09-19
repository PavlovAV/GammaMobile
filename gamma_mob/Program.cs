using System;
using System.Windows.Forms;
using System.Threading;
using Datalogic.API;
using OpenNETCF.Threading;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;


namespace gamma_mob
{
    internal static class Program
    {
        private static NamedMutex _mutex;

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
            UpdateProgram.DropFlagUpdateLoading();
            int num = 0;
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(UpdateProgram.LoadUpdate);
            // создаем таймер
            System.Threading.Timer timer = new System.Threading.Timer(tm, num, 30000, 300000);
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application2.Run(new MainForm());
            }
            else BarcodeScanner.Scanner.Dispose();
        }
    }
}