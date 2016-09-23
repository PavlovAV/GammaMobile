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
        UploadToDb
    }

    internal enum ConnectState
    {
        ConInProgress,
        NoConInProgress,
        NoConnection
    }

    public enum DocType
    {
        DocShipmentOrder,
        DocMovementOrder,
    }

    public enum MovementType
    {
        Accept,
        Movement
    }
}