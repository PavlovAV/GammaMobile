using System;
using System.Windows.Forms;
using Datalogic.API;

namespace gamma_mob.Common
{
    public class BarcodeScanner : IDisposable
    {
        private BarcodeScanner()
        {
            try
            {
                DecodeHandler = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
            }
            catch (DecodeException)
            {
                MessageBox.Show(@"Exception loading barcode decoder.", @"Decoder Error");
            }

            const DecodeRequest reqType = (DecodeRequest) 1 | DecodeRequest.PostRecurring;

            //            if (DcdEvent != null)
            //                DcdEvent.Scanned -= DcdEvent_Scanned;
            if (DcdEvent == null)
                DcdEvent = new DecodeEvent(DecodeHandler, reqType);
            DcdEvent.Scanned += DcdEvent_Scanned;

            BarcodeReceived = null;
        }

        private DecodeEvent DcdEvent { get; set; }
        private DecodeHandle DecodeHandler { get; set; }

        private static BarcodeScanner Instance { get; set; }

        public static BarcodeScanner Scanner
        {
            get { return Instance ?? (Instance = new BarcodeScanner()); }
        }

        public static Control CurrentListener { get; private set; }
        public event BarcodeReceivedEventHandler BarcodeReceived;

        /// <summary>
        ///     This method will be called when the DcdEvent is invoked.
        /// </summary>
        private void DcdEvent_Scanned(object sender, DecodeEventArgs e)
        {
            var cId = CodeId.NoData;
            string dcdData;

            // Obtain the string and code id.
            try
            {
                dcdData = DecodeHandler.ReadString(e.RequestID, ref cId);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Error reading string!");
                return;
            }
            dcdData = dcdData.Trim(); // replaceAll("\\p{Cntrl}", "");
            if (BarcodeReceived != null)
            {
                BarcodeReceived(dcdData);
            }
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
                DcdEvent.Scanned -= DcdEvent_Scanned;
                DcdEvent.StopScanListener();
                DcdEvent.Dispose();
                DecodeHandler.Dispose();
            }
            Disposed = true;
        }

        #endregion
    }
}