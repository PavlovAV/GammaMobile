using System.Collections.Generic;

namespace gamma_mob.Models
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public List<PlaceZone> WarehouseZones { get; set; }
        public string Barcode { get; set; }
        public int PlaceGroupId { get; set; }
    }
}