using System;

namespace gamma_mob.Models
{
    public class OfflineProduct
    {
        public Guid? DocShipmentOrderId { get; set; }
        public string Barcode { get; set; }
        public int PersonId { get; set; }
        public string ResultMessage { get; set; }
        public bool Unloaded { get; set; }
    }
}