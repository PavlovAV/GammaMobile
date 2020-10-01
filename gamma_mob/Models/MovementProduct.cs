using System;
using System.ComponentModel;

namespace gamma_mob.Models
{
    public class MovementProduct
    {
        private decimal _collectedQuantity;
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public int? ProductKindId { get; set; }
        public int? CoefficientPackage { get; set; }
        public int? CoefficientPallet { get; set; }
        public int QuantityUnits { get; set; }
        public string CollectedQuantityComputedColumn { get; set; }
        public decimal CollectedQuantity
        {
            get { return _collectedQuantity; }
            set
            {
                _collectedQuantity = value;
                //количество, пересчитанное в групповые упаковки для СГИ
                CollectedQuantityComputedColumn = ((CoefficientPackage == null || CoefficientPackage == 0) ? Convert.ToDecimal(value) : (Convert.ToDecimal(value) / Convert.ToInt32(CoefficientPackage))).ToString("0.###");
                //NotifyPropertyChanged("CollectedQuantity");
                NotifyPropertyChanged("CollectedQuantityComputedColumn");
            }
        }
        #region Члены INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}