using System;

namespace gamma_mob.Models
{
    public class EndPointInfo
    {
        public int WarehouseId { get; set; }
        public Guid? WarehouseZoneId { get; set; }
        public Guid? ZoneCellId { get; set; }
    }
}