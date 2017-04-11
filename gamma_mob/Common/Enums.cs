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
        Remove
    }

    public enum ConnectState
    {
        ConInProgress,
        NoConInProgress,
        NoConnection
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
        /// внутреннее перемещение(на данный момент пока отказались)
        /// </summary>
        MovementOrder
    }


    public enum DocDirection
    {
        DocOut,
        DocIn,
        DocOutIn
    }
}