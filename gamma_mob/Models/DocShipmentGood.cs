using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace gamma_mob.Models
{
    public class DocShipmentGood : INotifyPropertyChanged
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public string NomenclatureName { get; set; }

        /// <summary>
        /// Количество для сбора(может быть LoadToTop)
        /// </summary>
        public string Quantity { get; set; }

        private decimal _collectedQuantity;

        public decimal CollectedQuantity
        {
            get { return _collectedQuantity; }
            set { 
                _collectedQuantity = value;
                NotifyPropertyChanged("CollectedQuantity");
            }
        }

        #region Члены INotifyPropertyChanged

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
