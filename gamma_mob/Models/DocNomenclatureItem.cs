using System;
using System.ComponentModel;

namespace gamma_mob.Models
{
    public class DocNomenclatureItem : INotifyPropertyChanged
    {
        private decimal _collectedQuantity;
        public int CountProductSpools { get; set; }
        public int CountProductSpoolsWithBreak { get; set; }
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }
        public int LineNumber { get; set; }
        public int? CoefficientPackage { get; set; }
        public int? CoefficientPallet { get; set; }
        public int CollectedQuantityUnits { get; set; }
        public string CollectedQuantityComputedColumn { get; set; }
        public decimal SpoolWithBreakPercentColumn { get; set; }
        /// <summary>
        ///     Признак превышения собранного количества для подсветки
        /// </summary> 
        public bool IsPercentCollectedExcess { get; set; }

        /// <summary>
        ///     Признак, что номенклатура собрана
        /// </summary> 
        public bool IsCollected { get; set; }

        /// <summary>
        ///     Количество для сбора(может быть LoadToTop)
        /// </summary>
        public string Quantity { get; set; }

        public decimal CollectedQuantity
        {
            get { return _collectedQuantity; }
            set
            {
                _collectedQuantity = value;
                //количество, пересчитанное в групповые упаковки для СГИ
                CollectedQuantityComputedColumn = ((CoefficientPackage == null || CoefficientPackage == 0) ? Convert.ToDecimal(value) : (Convert.ToDecimal(value) / Convert.ToInt32(CoefficientPackage))).ToString("0.###");
                SpoolWithBreakPercentColumn = (CountProductSpools == null || CountProductSpools == 0) ? 0 : (100 * Convert.ToDecimal(CountProductSpoolsWithBreak) / Convert.ToDecimal(CountProductSpools));
                try
                {
                    IsPercentCollectedExcess = (Convert.ToDecimal(CollectedQuantityComputedColumn) > Convert.ToDecimal(Quantity));
                }
                catch
                {
                    IsPercentCollectedExcess = false;
                }
                //NotifyPropertyChanged("CollectedQuantity");
                NotifyPropertyChanged("CollectedQuantityComputedColumn");
                try
                {
                    IsCollected = (Convert.ToDecimal(CollectedQuantityComputedColumn) >= Convert.ToDecimal(Quantity));
                }
                catch
                {
                    IsCollected = false;
                }
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