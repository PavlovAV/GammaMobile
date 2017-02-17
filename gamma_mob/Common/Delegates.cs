using System;

namespace gamma_mob.Common
{
    public delegate void VoidDelagate();
    public delegate void BarcodeGetDelegate(string barcode, bool fromBuffer);
    public delegate void ConnectStateChangeInvoker(ConnectState state);
    public delegate void UpdateOrderGridInvoker(Guid nomenclatureId, Guid characteristicId, string nomenclatureName,
                    string shortNomenclatureName, decimal quantity, bool add);
    public delegate void UpdateInventarisationGridInvoker(Guid nomenclatureId, Guid characteristicId, string nomenclatureName,
                    string shortNomenclatureName, decimal quantity);
}
