using System;

namespace gamma_mob.Models
{
    public class MeasureUnitNomenclature : MeasureUnit
    {
        public Guid NomenclatureID { get; set; }
        //public string NomenclatureName { get; set; }
        public Guid CharacteristicID { get; set; }
        //public string CharacteristicName { get; set; }
        
    }
}
