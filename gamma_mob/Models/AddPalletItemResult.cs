using System;

namespace gamma_mob.Models
{
    public class AddPalletItemResult
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public decimal Quantity { get; set; }
        public string ResultMessage { get; set; }
    }
}
