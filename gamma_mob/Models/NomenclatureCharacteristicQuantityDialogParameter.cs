using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class NomenclatureCharacteristicQuantityDialogParameter
    {
        public EndPointInfo StartPointInfo { get; set; }
        public EndPointInfo EndPointInfo { get; set; }
        public Guid? NomenclatureId { get; set; }
        public Guid? CharacteristicId { get; set; }
        public Guid? QualityId { get; set; }
        public byte? ProductKindId { get; set; }
        public Guid? MeasureUnitId { get; set; }
        public MeasureUnit MeasureUnit { get; set; }
        public int Quantity { get; set; }
        public int? QuantityFractional { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public bool IsFilteringOnNomenclature  { get; set; }
        public bool IsFilteringOnEndpoint { get; set; }
        public List<Nomenclature> NomenclatureGoods { get; set; }
        public bool CheckExistMovementToZone { get; set; }
    }
}
