using System;

namespace gamma_mob.Models
{
    public class PalletListItem
    {
        public Guid ProductId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
    }
}
