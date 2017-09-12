using System;

namespace gamma_mob.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public decimal Quantity { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public int CountProductSpools { get; set; }
        public int CountProductSpoolsWithBreak { get; set; }
    }
}