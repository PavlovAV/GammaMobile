using System;

namespace gamma_mob.Models
{
    public class Barcodes1C
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public Guid MeasureUnitId { get; set; }
    }
}