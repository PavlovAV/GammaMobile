using System;
using System.Windows.Forms;
using Datalogic.API;
using System.IO;
using System.Text;
using OpenNETCF.Windows.Forms;
using System.Runtime.InteropServices;

namespace gamma_mob.Common
{
    public class BarcodeScanner : IDisposable
    {
       /*private static IBarcodeScanner _currentScanner { get; set; }
        public static IBarcodeScanner CurrentScanner 
        {
            get
            {
                if (_currentScanner == null)
                {
                    if (Program.deviceName.Contains("Falcon"))
                    {
                        _currentScanner = new BarcodeScannerFalcon();
                    }
                    else if (Program.deviceName.Contains("CPT"))
                    {
                        _currentScanner = new BarcodeScannerCipherlab();
                    }
                }
                return _currentScanner;
            }
        }
        */
        public IBarcodeScanner CurrentScanner { get; set; }

        private BarcodeScanner()
        {
            if (Program.deviceName.Contains("Falcon"))
            {
                CurrentScanner = new BarcodeScannerFalcon();
            }
            else if (Program.deviceName.Contains("CPT"))
            {
                CurrentScanner = new BarcodeScannerCipherlab();
            }
        }

        private static BarcodeScanner Instance { get; set; }

        public static bool IsInstanceInitialized { get { return Instance != null; } }

        public static BarcodeScanner Scanner
        {
            get { return Instance ?? (Instance = new BarcodeScanner()); }
        }

        #region Члены IDisposable

        private bool Disposed { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                CurrentScanner.Dispose();
            }
            Disposed = true;
        }

        #endregion
    }
}