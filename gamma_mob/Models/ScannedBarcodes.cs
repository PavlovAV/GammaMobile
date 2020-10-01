using System;
using System.Collections.Generic;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class ScannedBarcodes
    {
        
        public ScannedBarcodes()
        {
            Barcodes = Shared.LoadFromLogBarcodesForCurrentUser();
            
        }
        
        public delegate void MethodContainer();
        public static event MethodContainer OnUpdateBarcodesIsNotUploaded;
        public static event MethodContainer OnUnloadOfflineProducts;
        

        public Guid? AddScannedBarcode(string barcode, EndPointInfo endPointInfo, DocDirection docTypeId, Guid? docId, DbProductIdFromBarcodeResult getProductResult)
        {
            var item = new ScannedBarcode(barcode, endPointInfo, (int)docTypeId, docId, getProductResult.ProductId, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (item != null)
            {
                Barcodes.Add(item);
                Shared.SaveToLog(item.ScanId, item.DateScanned, item.Barcode, item.PlaceId, item.PlaceZoneId, item.DocTypeId, item.DocId, item.IsUploaded, item.ProductId, item.ProductKindId, item.NomenclatureId, item.CharacteristicId, item.QualityId, item.Quantity);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return item.ScanId;
            }
            else
                return null;
        }

        public bool UploadedScan(Guid? scanId)
        {
            var item = Barcodes.FindLast(
                delegate(ScannedBarcode sb)
            {
                return sb.ScanId == scanId;
            });
            if (item != null)
            {
                item.IsUploaded = true;
                Shared.SaveToLog("Set IsUploaded => True ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, null);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public bool UploadedScanWithError(Guid? scanId, string errorLog)
        {
            var item = Barcodes.FindLast(
                delegate(ScannedBarcode sb)
                {
                    return sb.ScanId == scanId;
                });
            if (item != null)
            {
                item.IsUploaded = true;
                item.UploadResult = errorLog;
                Shared.SaveToLog("Set IsUploadedWithError => True | " + errorLog + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, errorLog);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public List<ScannedBarcode> BarcodesIsNotUploaded 
        {
            get
            {
                if (Barcodes == null)
                    return null;
                else
                {
                    var b = 
                    Barcodes.FindAll(
                        delegate(ScannedBarcode sb)
                        {
                            return sb.IsUploaded == false || (sb.ToDelete == true && sb.IsDeleted == false);
                        })
                        ;
                    b.Sort(
                        delegate(ScannedBarcode p1, ScannedBarcode p2)
                        {
                            return p1.DateScanned.CompareTo(p2.DateScanned);
                        });
                    return b;
                }
            }
        }

        public int BarcodesCollectedCount(int placeId)
        {
            {
                return (Barcodes == null) ? 0 :
                Barcodes.FindAll(
                    delegate(ScannedBarcode sb)
                    {//удаленный в тсд, но еще не удаленный в БД одновременно попадает и в количество собранных и в количество невыгруженных
                        return sb.IsUploaded == true && (sb.ToDelete == false || (sb.ToDelete == true && sb.IsDeleted == false)) && sb.ProductKindId != 3 && sb.PlaceId == placeId;
                    }).Count;
            }
        }

        public bool DeletedScan(Guid? scanId)
        {
            var item = Barcodes.FindLast(
                delegate(ScannedBarcode sb)
                {
                    return sb.ScanId == scanId;
                });
            if (item != null)
            {
                item.IsDeleted = true;
                Shared.SaveToLog("Set IsDeleted => True ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, item.IsDeleted, null);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public List<ScannedBarcode> Barcodes { get; set; }

        public bool UnloadOfflineProducts(bool isSilent)
        {
            System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UnloadOfflineProducts()!" + Environment.NewLine);
            if (OnUnloadOfflineProducts != null) OnUnloadOfflineProducts();
                
            return true;
            //System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!Db.GetServerDateTime()!" + Environment.NewLine);
            //var date = Db.GetServerDateTime();
            //if (date == null)
            //    return false;
            //else
            //{
            //    System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UpdateBarcodes(date)!" + Environment.NewLine);
            //    return UpdateBarcodes(date, isFirst);
            //}
        }
    }
}
