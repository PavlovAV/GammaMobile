using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Datalogic.API;

namespace gamma_mob.Common
{
    public class BarcodeScannerFalcon: IBarcodeScanner, IDisposable
    {
        private BarcodeScanner _bs;

        public event BarcodeReceivedEventHandler BarcodeReceived;

        public BarcodeScannerFalcon()
        {
            
            try
            {
                DecodeHandler = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
            }
            catch (DecodeException)
            {
                Shared.ShowMessageError(@"Exception loading barcode decoder.");
            }

            const DecodeRequest reqType = (DecodeRequest)1 | DecodeRequest.PostRecurring;

            //            if (DcdEvent != null)
            //                DcdEvent.Scanned -= DcdEvent_Scanned;
            if (DcdEvent == null)
                DcdEvent = new DecodeEvent(DecodeHandler, reqType);
            DcdEvent.Scanned += DcdEvent_Scanned;

            BarcodeReceived = null;
        }

        public void SetBarcodeScanner(BarcodeScanner bs)
        {
            _bs = bs;
        }

        private DecodeEvent DcdEvent { get; set; }
        private DecodeHandle DecodeHandler { get; set; }

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
                Shared.ShowMessageError(@"Error reading string!");
                return;
            }
            dcdData = dcdData.Trim(); // replaceAll("\\p{Cntrl}", "");
            if (BarcodeReceived != null)
            {
                Shared.SaveToLogInformation(DateTime.Now.ToString("HH:mm:ss.fff") + " scanned barcode: " + dcdData);
                BarcodeReceived(dcdData);
            }
        }

        public void Dispose()
        {
            DcdEvent.Scanned -= DcdEvent_Scanned;
            DcdEvent.StopScanListener();
            DcdEvent.Dispose();
            DecodeHandler.Dispose();
        }

    }
}
