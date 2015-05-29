using System;
using System.Windows.Forms;
using OpenNETCF.Threading;
using Datalogic.API;


namespace gamma_mob
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            bool isNew;
            _mutex = new NamedMutex(false,"gammamob",out isNew);
            if (!isNew) return;
            Decode.SetWedge(WedgeType.Barcode, false);
            Application.Run(new MainForm());
        }

        private static NamedMutex _mutex;

    }

    public delegate void EventDelegate(string barCode);

    
    
    
}