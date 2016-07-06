using System;
using System.Windows.Forms;
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
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application2.Run(new MainForm());
            }
            else BarcodeScanner.Scanner.Dispose();
        }
    }
}