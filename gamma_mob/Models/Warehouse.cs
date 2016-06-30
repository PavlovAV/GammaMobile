using System.Collections.Generic;

namespace gamma_mob.Models
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public List<WarehouseZone> WarehouseZones { get; set; }
    }

    public class WarehouseZone
    {
        public int WarehouseZoneId { get; set; }
        public string Name { get; set; }
        public List<ZoneCell> ZoneCells { get; set; }
    }

    public class ZoneCell
    {
        public int ZoneCellId { get; set; }
        public string Name { get; set; }
    }
}