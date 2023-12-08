using System;

namespace gamma_mob.Models
{
    [Serializable]
    public class MeasureUnit
    {
        public Guid MeasureUnitID { get; set; }
        public string Name { get; set; }
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public bool IsActive { get; set; }
        public Guid BaseMeasureUnitID { get; set; }
        
        [System.Xml.Serialization.XmlIgnore]
        public decimal Coefficient
        {
            get { return Convert.ToDecimal(Numerator) / Denominator; }
        }
    }
}
