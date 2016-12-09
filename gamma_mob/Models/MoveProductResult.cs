using System;

namespace gamma_mob.Models
{
    public class MoveProductResult
    {
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string ResultMessage { get; set; }
        public bool AlreadyAdded { get; set; }
        public string OutPlace { get; set; }
        public Guid? DocMovementId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? Date { get; set; }

    }
}