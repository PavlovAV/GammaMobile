using System;

namespace gamma_mob.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Number { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public decimal Quantity { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public int CountProductSpools { get; set; }
        public int CountProductSpoolsWithBreak { get; set; }
        public Guid QualityId { get; set; }
        public int? CoefficientPackage { get; set; }
        public int? CoefficientPallet { get; set; }
    }
}