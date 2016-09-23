using System;
using System.ComponentModel;

namespace gamma_mob.Models
{
    public class DocNomenclatureItem : INotifyPropertyChanged
    {
        private decimal _collectedQuantity;
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public string NomenclatureName { get; set; }
        public string ShortNomenclatureName { get; set; }

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
                NotifyPropertyChanged("CollectedQuantity");
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