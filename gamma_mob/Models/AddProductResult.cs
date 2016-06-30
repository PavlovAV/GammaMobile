using System.Collections.Generic;

namespace gamma_mob.Models
{
    public class DbOperationProductResult
    {
        public List<ProductItem> ProductItems { get; set; }
        public string ResultMessage { get; set; }
        public bool AlreadyMadeChanges { get; set; }
    }
}