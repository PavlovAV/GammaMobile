using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    [Serializable]
    public class OfflineProduct
    {
        public Guid DocId { get; set; }
        public string Barcode { get; set; }
    }
}
