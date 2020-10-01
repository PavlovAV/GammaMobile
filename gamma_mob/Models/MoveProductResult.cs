﻿using System;

namespace gamma_mob.Models
{
    public class MoveProductResult
    {
        public string Number { get; set; }
        public string NomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string ResultMessage { get; set; }
        public bool AlreadyAdded { get; set; }
        public string OutPlace { get; set; }
        public Guid? DocMovementId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime? Date { get; set; }
        public string NumberAndInPlaceZone { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public string ShortNomenclatureName { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public int? CoefficientPackage { get; set; }
        public int? CoefficientPallet { get; set; }
    }
}