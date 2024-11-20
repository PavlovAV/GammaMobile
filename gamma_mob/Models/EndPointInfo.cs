using System;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class EndPointInfo
    {
        public EndPointInfo(int placeId)
        {
            PlaceId = placeId;
        }

        public EndPointInfo(int placeId, string placeName)
        {
            PlaceId = placeId;
            PlaceName = placeName;
        }

        public EndPointInfo(int placeId, Guid? placeZoneId)
        {
            PlaceId = placeId;
            PlaceZoneId = placeZoneId;
        }

        private int _placeId { get; set; }
        public int PlaceId 
        {
            get
            {
                return _placeId;
            }
            set
            {
                _placeId = value;
                if (value == 0)
                {
                    PlaceName = String.Empty;
                    PlaceGroupId = null;
                }
                else
                {
                    var w = Shared.Warehouses.Find(f => f.WarehouseId == value);
                    if (w != null)
                    {
                        PlaceName = w.WarehouseName;
                        PlaceGroupId = w.PlaceGroupId;
                    }
                }
                var placeZone = Shared.PlaceZones.Find(p => p.PlaceId == value);
                
                IsAvailabilityPlaceZoneId = (placeZone != null);
            }
        }
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
                PlaceZoneName = placeZone == null ? @"" : placeZone.Name;
                PlaceZoneBarcode = placeZone == null ? @"" : placeZone.Barcode;
                if (value != null)
                    IsAvailabilityChildPlaceZoneId = (Db.GetPlaceZoneChilds((Guid)value).Count > 0);
            }
        }
        public string PlaceName { get; set; }
        public string PlaceZoneName { get; set; }
        public bool IsSettedDefaultPlaceZoneId { get; set; }
        public bool IsAvailabilityChildPlaceZoneId { get; set; }
        public bool IsAvailabilityPlaceZoneId { get; set; }
        public string PlaceZoneBarcode { get; private set; }
        public int? PlaceGroupId { get; set; }
    }
}