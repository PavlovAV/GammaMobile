using gamma_mob.Models;
namespace gamma_mob.Common
{
    public delegate void BarcodeReceivedEventHandler(string barcode);
    public delegate void AddProductReceivedEventHandler(AddProductReceivedEventHandlerParameter param);
    public delegate void ChoosePlaceZoneEventHandler(EndPointInfo param);
    public class AddProductReceivedEventHandlerParameter
    {
        public string barcode {get; set;}
        public EndPointInfo endPointInfo { get; set; }
        public bool fromBuffer { get; set; }
        public DbProductIdFromBarcodeResult getProductResult { get; set; }
    }
}