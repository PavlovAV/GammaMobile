using System;

namespace gamma_mob.Common
{
    public delegate void ConnectStateChangeInvoker(ConnectState state);

    public delegate void LoginStateChangeInvoker(ConnectState state, string message);
    
    public delegate void UpdateOrderGridInvoker(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                    string shortNomenclatureName, decimal quantity, bool add, int CountProductSpools, int CountProductSpoolsWithBreak, int? productKindId);

    public delegate void UpdateInventarisationGridInvoker(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                    string shortNomenclatureName, decimal quantity, int? productKindId);

    public delegate void UpdateMovementGridInvoker(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName, string shortNomenclatureName, Guid? placeZoneId,
                                                decimal quantity, bool add, string barcode, int? productKindId, int? CoefficientPackage, int? CoefficientPallet);

    public delegate void RefreshDocOrderDelegate(Guid docId);

    public delegate void RefreshDocProductDelegate(Guid docId, bool showMessage);
    

}
