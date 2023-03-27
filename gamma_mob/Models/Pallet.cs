using System;
using System.Collections.Generic;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class Pallet
    {
        public Guid ProductId { get; set; }
        public Guid DocOrderId { get; set; }
        public DocDirection DocDirection { get; set; }
        public bool IsConfirmed { get; set; }
        public List<DocNomenclatureItem> Items { get; set; }
        public string Number { get; set; }
    }
}
