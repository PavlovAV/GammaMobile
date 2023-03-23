using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Common
{
    public class BarcodeScannerCipherlab : IBarcodeScanner, IDisposable
    {
        private BarcodeScanner _bs;
        
        public event BarcodeReceivedEventHandler BarcodeReceived;

        public BarcodeScannerCipherlab()
        {
            try
            {
                rdr = new Scancode.BarcodeReaderHelper();
            }
            catch (Exception e)
            {
                Shared.ShowMessageError(@"Exception loading barcode decoder.");
            }

            if (rdr != null)
                rdr.OnScan += new Scancode.ReaderEventHandler(rdr_OnScan);

            int resInit = Scancode.BarcodeReaderHelper.ReaderInit();

            BarcodeReceived = null;
        }

        public void SetBarcodeScanner(BarcodeScanner bs)
        {
            _bs = bs;
        }

        private Scancode.BarcodeReaderHelper rdr { get; set; }

        void rdr_OnScan(Scancode.ReaderEventArgs e)
        {
            // Этот метод вызывается для варианта 2
            // (для CP60, 9700 и CP55 этот делегат вызывается из другого потока, поэтому делаем Invoke)
            //Scancode.ReaderEventHandler d = new Scancode.ReaderEventHandler(ShowScanResult);
            //this.Invoke(d, e);
            if (BarcodeReceived != null)
            {
                Shared.SaveToLogInformation(e.barcode);
                BarcodeReceived(e.barcode);
            }
        }

        void ShowScanResult(Scancode.ReaderEventArgs e)
        {
            //Log(String.Format("barcode: {0} (type={1})", e.barcode, e.barcodeTypeID));
            if (BarcodeReceived != null)
            {
                Shared.SaveToLogInformation(e.barcode);
                BarcodeReceived(e.barcode);
            }
        }

        public void Dispose()
        {
            Scancode.BarcodeReaderHelper.ReaderUninit();
        }

    }
}
