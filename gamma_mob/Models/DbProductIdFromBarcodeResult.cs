using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DbProductIdFromBarcodeResult
    {
        public Guid ProductId { get; set; }
        public int? ProductKindId { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid MeasureUnitId { get; set; }
        public Guid QualityId { get; set; }
        public int CountProducts { get; set; }
    }
}
