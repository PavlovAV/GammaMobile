using System;

namespace gamma_mob.Models
{
    public class MovementProduct
    {
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string SourcePlace { get; set; }
        public string PlaceTo { get; set; }
        public string Barcode { get; set; }
        public Guid ProductId { get; set; }
    }
}