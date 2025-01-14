using System;
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
        RDP,
        ShortcutStartPointsPanelEnabled,
        BackOffline,
        PlaceFrom,
        PlaceTo
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
        Inventarisation,
        /// <summary>
        /// Поступление товаров
        /// </summary>
        Purchase
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

    [Flags]
    public enum VisibleButtonsOnMainWindow
    {
        NONE = 0,
        btnDocOrder = 1,
        btnDocTransfer = 2,
        btnDocMovement = 4,
        btnExtAccept = 8,
        btnMovementForOrder = 16,
        btnInventarisation = 32,
        btnCloseShift = 64,
        btnComplectPallet = 128,
        btnCloseApp = 256,
        btnInfoProduct = 512,
        ALL = ~NONE
    }

    public enum CheckConnectionMethod
    {
        /// <summary>
        /// Проверка используя Ping
        /// </summary>
        Ping,
        /// <summary>
        /// Проверка используя открытие порта на сервере
        /// </summary>
        TcpCl
    }

    public enum CurrentServer
    {
        Internal,
        External
    }
}