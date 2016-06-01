using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public List<ProductItem> ProductItems { get; set; }
    }

    public class ProductItem
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public decimal Quantity { get; set; }
    }
}
