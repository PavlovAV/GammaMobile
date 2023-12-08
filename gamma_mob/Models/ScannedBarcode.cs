using System;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class ScannedBarcode
    {

        public ScannedBarcode()
        {}

        public ScannedBarcode(string barcode, EndPointInfo endPointInfo, int docTypeId, Guid? docId, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity)
            : this(barcode, endPointInfo, docTypeId, docId, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, null)
        {
        }

        public ScannedBarcode(string barcode, EndPointInfo endPointInfo, int docTypeId, Guid? docId, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity, Guid? fromProductId)
            :this(barcode, Guid.NewGuid(), endPointInfo, docTypeId, docId, false, false, false, DateTime.Now, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, fromProductId)
        {
        }

        public ScannedBarcode(string barcode, Guid scanId, EndPointInfo endPointInfo, int docTypeId, Guid? docId, bool isUploaded, bool toDelete, bool isDeleted, DateTime dateScanned, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity)
            : this(barcode, scanId, endPointInfo, docTypeId, docId, isUploaded, toDelete, isDeleted, dateScanned, productId, productKindId, nomenclatureId, characteristicId, qualityId, quantity, null)
        {
        }

        public ScannedBarcode(string barcode, Guid scanId, EndPointInfo endPointInfo, int docTypeId, Guid? docId, bool isUploaded, bool toDelete, bool isDeleted, DateTime dateScanned, Guid? productId, int? productKindId, Guid? nomenclatureId, Guid? characteristicId, Guid? qualityId, int? quantity, Guid? fromProductId)
        {
            Barcode = barcode;
            ScanId = scanId;
            PlaceId = endPointInfo == null ? 0 : endPointInfo.PlaceId;
            PlaceZoneId = endPointInfo == null ? (Guid?)null : endPointInfo.PlaceZoneId;
            DocTypeId = docTypeId;
            DocId = docId;
            IsUploaded = IsUploaded;
            ToDelete = toDelete;
            IsDeleted = isDeleted;
            DateScanned = dateScanned;
            ProductId = productId;
            ProductKindId = productKindId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            Quantity = quantity;
            FromProductId = fromProductId;
        }

        /*public ScannedBarcode(string barcode, EndPointInfo endPointInfo, int docTypeId, Guid? docId)
        {
            Barcode = barcode;
            ScanId = Guid.NewGuid();
            PlaceId = endPointInfo.PlaceId;
            PlaceZoneId = endPointInfo.PlaceZoneId;
            DocTypeId = docTypeId;
            DocId = docId;
            IsUploaded = false;
            DateScanned = DateTime.Now;
        }*/

       /* public ScannedBarcode(string barcode, bool isUploaded):this()
        {
            Barcode = barcode;
            ScanId = Guid.NewGuid();
            IsUploaded = isUploaded;
            Shared.SaveToLog(ScanId, Barcode, PlaceId, PlaceZoneId, DocTypeId, DocId, IsUploaded);
        }
        */

        public string Barcode { get; set; }
        public DateTime DateScanned { get; set; }
        public int PlaceId { get; set; }
        public Guid? PlaceZoneId { get; set; }
        public int DocTypeId { get; set; }
        public Guid ScanId { get; set; }
        public Guid? DocId { get; set; }
        public bool IsUploaded { get; set; }
        public Guid? ProductId { get; set; }
        public int? ProductKindId { get; set; }
        /// <summary>
        /// Номенклатура для россыпи ( и только для россыпи)
        /// </summary>
        public Guid? NomenclatureId { get; set; }
        /// <summary>
        /// Характеристика для россыпи ( и только для россыпи)
        /// </summary>
        public Guid? CharacteristicId { get; set; }
        /// <summary>
        /// Качество для россыпи ( и только для россыпи)
        /// </summary>
        public Guid? QualityId { get; set; }
        /// <summary>
        /// Количество групповых упаковок для россыпи ( и только для россыпи)
        /// </summary>
        public int? Quantity { get; set; }
        /// <summary>
        /// ИД паллеты, из которой вытащили упаковку/коробку
        /// </summary>
        public Guid? FromProductId { get; set; }
        public string UploadResult { get; set; }
        public bool ToDelete { get; set; }
        public bool IsDeleted { get; set; }
    }
}
