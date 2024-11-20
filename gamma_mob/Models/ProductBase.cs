using System;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class ProductBase
    {
        public string Number { get; set; }
        public string Barcode { get; set; }
        public DateTime? Date { get; set; }
        public Guid MovementId { get; set; }
        public string OutPlace { get; set; }
        public string InPlace { get; set; }
        public bool IsProductR { get; set; }
        public string Quantity { get; set; }
        public ProductKind? ProductKind { get; set; }
        public int? OutPlaceID { get; set; }
        public Guid? OutPlaceZoneID { get; set; }
        public int? InPlaceID { get; set; }
        public Guid? InPlaceZoneID { get; set; }
        public DateTime? DateEnd { get; set; }

        /*public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string ResultMessage { get; set; }
        public bool AlreadyAdded { get; set; }
        public Guid? DocMovementId { get; set; }
        public Guid? ProductId { get; set; }
        public string NumberAndInPlaceZone { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public string ShortNomenclatureName { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public int? CoefficientPackage { get; set; }
        public int? CoefficientPallet { get; set; }*/
    }
}