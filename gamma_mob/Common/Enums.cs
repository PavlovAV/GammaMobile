namespace gamma_mob.Common
{
    public enum Images
    {
        Back,
        Binocle,
        DocPlus,
        NetworkOffline,
        NetworkOfflineSmall,
        NetworkTransmitReceive,
        NetworkTransmitReceiveSmall,
        NetworkTransmitReceiveOff,
        Inspect,
        Edit,
        Refresh,
        UploadToDb,
        Question,
        Save,
        Print,
        Pallet,
        Add,
        Remove,
        InfoProduct,
        RDP
    }

    public enum ConnectState
    {
        ConInProgress,
        NoConInProgress,
        NoConnection,
        ConnectionRestore
    }

   
/*
    /// <summary>
    /// Типы документов
    /// </summary>
    public enum DocType
    {
        /// <summary>
        /// Заказ 1с (приказ или внутренний заказ)
        /// </summary>
        DocShipmentOrder, 
        /// <summary>
        /// Внутренний заказ на перемещение
        /// </summary>
        DocMovementOrder, 
        /// <summary>
        ///  Перемещение без основания
        /// </summary>
        DocMovement       
    }
*/
    public enum OrderType
    {
        /// <summary>
        /// Отгрузка 1с
        /// </summary>
        ShipmentOrder,
        /// <summary>
        /// внутренний заказ 1с
        /// </summary>
        InternalOrder,
        /// <summary>
        /// внутреннее перемещение 
        /// </summary>
        MovementOrder,
        /// <summary>
        /// инвентаризация
        /// </summary>
        Inventarisation
    }


    public enum DocDirection
    {
        DocOut,
        DocIn,
        DocOutIn,
        DocInventarisation
    }

    public enum ConnectServerCe
    {
        LogServer,
        BarcodesServer,
        BackupBarcodesServer
    }

    public enum ProductKind
    {
        /// <summary>
        /// Тамбура
        /// </summary>
        ProductSpool,
        /// <summary>
        /// Паллеты
        /// </summary>
        ProductPallet,
        /// <summary>
        /// Групповые упаковки
        /// </summary>
        ProductGroupPack,
        /// <summary>
        /// Россыпь
        /// </summary>
        ProductPalletR,
        /// <summary>
        /// Кипы
        /// </summary>
        ProductBale,
        /// <summary>
        /// Перемещение продукции поштучно
        /// </summary>
        ProductMovement,
        /// <summary>
        /// Остатки продукции
        /// </summary>
        ProductRest
    }
}