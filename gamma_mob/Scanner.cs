using System;
using System.Windows.Forms;
using Datalogic.API;
using gamma_mob.Common;

namespace gamma_mob
{
    public class Scanner : IDisposable
    {
        private readonly DecodeEvent _dcdEvent;
        private readonly BarcodeReceivedEventHandler _eventFunc;
        private readonly DecodeHandle _hDcd;
        private bool _disposed;

        /// <summary>
        ///     Конструктор для создания элемента сканера
        /// </summary>
        /// <param name="aEventFunc">Делегат для реакции на сканер</param>
        /// <param name="ctlInvoker"></param>
        public Scanner(BarcodeReceivedEventHandler aEventFunc, Control ctlInvoker)
        {
            _eventFunc = aEventFunc;
            try
            {
                _hDcd = new DecodeHandle(DecodeDeviceCap.Exists | DecodeDeviceCap.Barcode);
            }
            catch (DecodeException)
            {
                Shared.ShowMessageError(@"Exception loading barcode decoder.");
                return;
            }

            // Now that we've got a connection to a barcode reading device, assign a
            // method for the DcdEvent.  A recurring request is used so that we will
            // continue to get barcode data until our dialog is closed.
            const DecodeRequest reqType = (DecodeRequest) 1 | DecodeRequest.PostRecurring;

            // Initialize event
            _dcdEvent = new DecodeEvent(_hDcd, reqType, ctlInvoker);
            _dcdEvent.Scanned += dcdEvent_Scanned;
            SerialNumber = Device.GetSerialNumber();
        }

        public string SerialNumber { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartScanListener()
        {
            if (_dcdEvent != null) _dcdEvent.StartScanListener();
        }

        public void StopScanListener()
        {
            if (_dcdEvent != null) _dcdEvent.StopScanListener();
        }

        /// <summary>
        ///     This method will be called when the DcdEvent is invoked.
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
                Shared.ShowMessageError(@"Error reading string!");
                return;
            }
            dcdData = dcdData.Trim(); // replaceAll("\\p{Cntrl}", "");
            _eventFunc(dcdData);
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