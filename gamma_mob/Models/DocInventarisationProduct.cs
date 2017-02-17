using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DocInventarisationProduct
    {
        public Guid ProductId { get; set; }
        public string Barcode { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; }
    }
}
