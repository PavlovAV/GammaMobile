using System;
using Datalogic.API;
using System.Windows.Forms;

namespace gamma_mob 
{
    class Scanner : IDisposable
    {
        private bool _disposed;
        private readonly DecodeEvent _dcdEvent;
        private readonly DecodeHandle _hDcd;
        private readonly EventDelegate _eventFunc;
        public string SerialNumber { get; private set; }
        
        public void StartScanListener()
        {
            if (_dcdEvent != null) _dcdEvent.StartScanListener();
        }

        public void StopScanListener()
        {
            if (_dcdEvent != null) _dcdEvent.StopScanListener();
        }
        
        //Конструктор
        public Scanner(EventDelegate aEventFunc, Control ctlInvoker)
        {
            _eventFunc = aEventFunc;
            try
            {
                _hDcd = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
            }
            catch (DecodeException)
            {
                MessageBox.Show(@"Exception loading barcode decoder.", @"Decoder Error");
                return;
            }

            // Now that we've got a connection to a barcode reading device, assign a
            // method for the DcdEvent.  A recurring request is used so that we will
            // continue to get barcode data until our dialog is closed.
            const DecodeRequest reqType = (DecodeRequest)1 | DecodeRequest.PostRecurring;

            // Initialize event
            _dcdEvent = new DecodeEvent(_hDcd, reqType, ctlInvoker);
            _dcdEvent.Scanned += dcdEvent_Scanned;
            SerialNumber = Device.GetSerialNumber();          
        }

        /// <summary>
        /// This method will be called when the DcdEvent is invoked.
        /// </summary>
        private void dcdEvent_Scanned(object sender, DecodeEventArgs e)
        {
            var cId = CodeId.NoData;
            string dcdData;

            // Obtain the string and code id.
            try
            {
                dcdData = _hDcd.ReadString(e.RequestID, ref cId);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Error reading string!");
                return;
            }
            dcdData = dcdData.Trim(); // replaceAll("\\p{Cntrl}", "");
            _eventFunc(dcdData);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _dcdEvent.Scanned -= dcdEvent_Scanned;
                _dcdEvent.StopScanListener();
                _dcdEvent.Dispose();
                _hDcd.Dispose();
            }
            _disposed = true;
        }
    }
}
