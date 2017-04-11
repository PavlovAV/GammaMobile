using System;
using System.Collections.Generic;

namespace gamma_mob.Models
{
    public class Pallet
    {
        public Guid ProductId { get; set; }
        public Guid DocOrderId { get; set; }
        public bool IsConfirmed { get; set; }
        public List<DocNomenclatureItem> Items { get; set; }
    }
}
