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

        private static ScannedBarcode lastScannedBarcode { get; set; }
        private static ScannedBarcode penultimateScannedBarcode { get; set; }

        public Guid? AddScannedBarcode(string barcode, EndPointInfo endPointInfo, DocDirection docTypeId, Guid? docId, DbProductIdFromBarcodeResult getProductResult)
        {
            var item = new ScannedBarcode(barcode, endPointInfo, (int)docTypeId, docId, getProductResult.ProductId, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts, getProductResult.MeasureUnitId, getProductResult.FromProductId, getProductResult.FromPlaceId, getProductResult.FromPlaceZoneId, getProductResult.NewWeight, getProductResult.CountFractionalProducts, getProductResult.ValidUntilDate);
            if (item != null)
            {
                Barcodes.Add(item);
                Shared.SaveToLog(item.ScanId, item.DateScanned, item.Barcode, item.PlaceId, item.PlaceZoneId, item.DocTypeId, item.DocId, item.IsUploaded, item.ProductId, item.ProductKindId, item.NomenclatureId, item.CharacteristicId, item.QualityId, item.Quantity, item.QuantityFractional, item.MeasureUnitId, item.FromProductId, item.FromPlaceId, item.FromPlaceZoneId, item.NewWeight, item.ValidUntilDate);
                penultimateScannedBarcode = lastScannedBarcode;
                if (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)
                    lastScannedBarcode = null;
                else
                    lastScannedBarcode = item;
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return item.ScanId;
            }
            else
                return null;
        }

        public bool UploadedScan(Guid? scanId, Guid? productId)
        {
            var item = Barcodes.FindLast(
                delegate(ScannedBarcode sb)
            {
                return sb.ScanId == scanId;
            });
            if (item != null)
            {
                item.IsUploaded = true;
                if (item.ProductId != productId)
                    Shared.SaveToLogInformation("Change ProductId from " + item.ProductId + " to " + (productId == null ? "" : productId.ToString()) + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                item.ProductId = productId;
                Shared.SaveToLogInformation("Set IsUploaded => True ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, null);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public bool UploadedScanWithError(Guid? scanId, string errorLog, Guid? productId)
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
                if (item.ProductId != productId)
                    Shared.SaveToLogInformation("Change ProductId from " + item.ProductId + " to " + (productId == null ? "" : productId.ToString()) + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                item.ProductId = productId;
                Shared.SaveToLogInformation("Set IsUploadedWithError => True | " + errorLog + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, errorLog);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public bool NotUploadedScan(Guid? scanId, string errorLog, Guid? productId)
        {
            var item = Barcodes.FindLast(
                delegate(ScannedBarcode sb)
                {
                    return sb.ScanId == scanId;
                });
            if (item != null)
            {
                item.IsUploaded = false;
                item.UploadResult = errorLog;
                if (item.ProductId != productId)
                    Shared.SaveToLogInformation("Change ProductId from " + item.ProductId + " to " + (productId == null ? "" : productId.ToString()) + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                item.ProductId = productId;
                Shared.SaveToLogInformation("Set IsUploaded => False | " + errorLog + " ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
                Shared.SaveToLog(item.ScanId, item.IsUploaded, errorLog);
                if (OnUpdateBarcodesIsNotUploaded != null) OnUpdateBarcodesIsNotUploaded();
                return true;
            }
            else
                return false;
        }

        public List<ScannedBarcode> BarcodesIsNotUploaded(DocDirection docTypeId) 
        {


            if (Barcodes == null || docTypeId == null)
                return null;
            else
            {
                var b = 
                Barcodes.FindAll(
                    delegate(ScannedBarcode sb)
                    {
                        return sb.DocTypeId == (int)docTypeId && (sb.IsUploaded == false || (sb.ToDelete == true && sb.IsDeleted == false));
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

        /*public int BarcodesCollectedCount(int placeId, DocDirection docTypeId)
        {
            {
                return (Barcodes == null) ? 0 :
                Barcodes.FindAll(
                    delegate(ScannedBarcode sb)
                    {//удаленный в тсд, но еще не удаленный в БД одновременно попадает и в количество собранных и в количество невыгруженных
                        return sb.DocTypeId == (int)docTypeId && sb.IsUploaded == true && (sb.ToDelete == false || (sb.ToDelete == true && sb.IsDeleted == false)) && sb.ProductKindId != 3 && sb.PlaceId == placeId;
                    }).Count;
            }
        }*/

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
                Shared.SaveToLogInformation("Set IsDeleted => True ON Barcode " + item.Barcode + " | ScanId " + item.ScanId.ToString());
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
            Db.RefreshIsUplodedFalase();
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

        public bool CheckIsLastBarcode(string barcode, DocDirection docTypeId, Guid? docId, int? placeId, Guid? placeZoneId)
        {
            if (Barcodes == null)
                return false;
            else
            {
                if (lastScannedBarcode == null && penultimateScannedBarcode == null)
                    return false;
                else
                    switch (docTypeId)
                    {
                        case DocDirection.DocOut:
                        case DocDirection.DocIn:
                            return (lastScannedBarcode != null && (lastScannedBarcode.Barcode == barcode && lastScannedBarcode.DocTypeId == (int)docTypeId && lastScannedBarcode.DocId == docId)) || (penultimateScannedBarcode != null && (penultimateScannedBarcode.Barcode == barcode && penultimateScannedBarcode.DocTypeId == (int)docTypeId && penultimateScannedBarcode.DocId == docId));
                            break;
                        case DocDirection.DocOutIn:
                        case DocDirection.DocInventarisation:
                            if (placeZoneId == null)
                                return (lastScannedBarcode != null && (lastScannedBarcode.Barcode == barcode && lastScannedBarcode.DocTypeId == (int)docTypeId && lastScannedBarcode.PlaceId == placeId && lastScannedBarcode.PlaceZoneId == null)) || (penultimateScannedBarcode != null && (penultimateScannedBarcode.Barcode == barcode && penultimateScannedBarcode.DocTypeId == (int)docTypeId && penultimateScannedBarcode.PlaceId == placeId && penultimateScannedBarcode.PlaceZoneId == null));
                            else
                                return (lastScannedBarcode != null && (lastScannedBarcode.Barcode == barcode && lastScannedBarcode.DocTypeId == (int)docTypeId && lastScannedBarcode.PlaceId == placeId && lastScannedBarcode.PlaceZoneId == placeZoneId)) || (penultimateScannedBarcode != null && (penultimateScannedBarcode.Barcode == barcode && penultimateScannedBarcode.DocTypeId == (int)docTypeId && penultimateScannedBarcode.PlaceId == placeId && penultimateScannedBarcode.PlaceZoneId == placeZoneId));
                            break;
                        default:
                            return false;
                    }
            }
        }

        public void ClearLastBarcode()
        {
            if (Barcodes == null)
                return;
            else
            {
                if (lastScannedBarcode == null && penultimateScannedBarcode == null)
                    return;
                else
                {
                    lastScannedBarcode = penultimateScannedBarcode;
                    penultimateScannedBarcode = null;
                }
            }
        }
    }
}
