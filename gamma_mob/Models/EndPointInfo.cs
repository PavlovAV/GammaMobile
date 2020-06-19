using System;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class EndPointInfo
    {
        public int PlaceId { get; set; }
        private Guid? _placeZoneId { get; set; }
        public Guid? PlaceZoneId 
        {
            get
            {
                return _placeZoneId;
            }
            set
            {
                _placeZoneId = value;
                var placeZone = Shared.PlaceZones.Find(p => p.PlaceZoneId == value);
                PlaceZoneBarcode = placeZone == null ? @"" : placeZone.Barcode;
            }
        }
        public string PlaceName { get; set; }
        public string PlaceZoneName { get; set; }
        public bool IsSettedDefaultPlaceZoneId { get; set; }
        public bool IsAvailabilityChildPlaceZoneId { get; set; }
        public string PlaceZoneBarcode { get; private set; }
    }
}