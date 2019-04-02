using System;

namespace gamma_mob.Models
{
    public class EndPointInfo
    {
        public int PlaceId { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public string PlaceName { get; set; }
        public string PlaceZoneName { get; set; }
        public bool IsSetDefaultPlaceZoneId { get; set; }
    }
}