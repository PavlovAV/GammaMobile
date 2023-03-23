using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Common
{
    public interface IBarcodeScanner
    {
        //public static BarcodeScanner Scanner { get; private set; }

        event BarcodeReceivedEventHandler BarcodeReceived;

        void SetBarcodeScanner(BarcodeScanner bs);

        void Dispose();
    }
}
