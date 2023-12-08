using System;

namespace gamma_mob.Models
{
    public class DbOperationProductResult
    {
//        public List<ProductItem> ProductItems { get; set; }
        public Product Product { get; set; }
        public string ResultMessage { get; set; }
        //public bool AlreadyMadeChanges { get; set; }
        ///// <summary>
        ///// Признак того, что документ, с которым проводилась операция, уже подтвержден
        ///// </summary>
        //public bool DocIsConfirmed { get; set; }
        public int? ProductKindId { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public Guid? ScanId { get; set; }
        public string OutPlace { get; set; }
        public string OutPlaceZone { get; set; }
        public string InPlace { get; set; }
        public string InPlaceZone { get; set; }
    }
}