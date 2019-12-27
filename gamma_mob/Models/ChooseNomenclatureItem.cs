using System;
using System.ComponentModel;

namespace gamma_mob.Models
{
    public class ChooseNomenclatureItem : INotifyPropertyChanged
    {
        public Guid NomenclatureId { get; set; }
        public Guid CharacteristicId { get; set; }
        public Guid QualityId { get; set; }
        public Guid MeasureUnitId { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public Guid BarcodeId { get; set; }
        
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
