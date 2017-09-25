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
        public Guid DocMovementId { get; set; }
        public DateTime? Date { get; set; }
        public bool? DocIsConfirmed { get; set; }
        public string NumberAndInPlaceZone { get; set; }
    }
}