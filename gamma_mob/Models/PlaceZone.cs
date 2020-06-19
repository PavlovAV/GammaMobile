using System;

namespace gamma_mob.Models
{
    public class PlaceZone
    {
        public Guid PlaceZoneId { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public Guid? PlaceZoneParentId { get; set; }
        public bool IsValid { get; set; }
    }
}
