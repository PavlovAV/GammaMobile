using System;

namespace gamma_mob.Models
{
    public class OfflineProduct
    {
        public Guid? DocId { get; set; }
        public string Barcode { get; set; }
        public Guid PersonId { get; set; }
        public string ResultMessage { get; set; }
        public bool Unloaded { get; set; }
        public int? PlaceId { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public Guid? PlaceZoneCellId { get; set; }
    }
}