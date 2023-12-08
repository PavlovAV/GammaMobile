using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class Nomenclature
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }

        public Nomenclature() 
        {}

        public Nomenclature(Guid nomenclatureId, Guid characteristicId, Guid qualityId)
        {
            NomenclatureId  = nomenclatureId; 
            CharacteristicId = characteristicId;
            QualityId = qualityId; 
        }
    }
}
