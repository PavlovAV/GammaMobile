using System;
using System.Collections.Generic;

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

    public class GoodProduct
    {
        public string Number { get; set; }
        public decimal Quantity { get; set; }
    }
}