﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class DbProductIdFromBarcodeResult
    {
        public Guid ProductId { get; set; }
        public ProductKind? ProductKindId { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid MeasureUnitId { get; set; }
        public Guid QualityId { get; set; }
        public int CountProducts { get; set; }
        public Guid? FromProductId { get; set; }
    }
}
