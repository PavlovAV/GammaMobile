using System;
using System.Collections.Generic;
using gamma_mob.Common;
using System.Windows.Forms;

namespace gamma_mob.Models
{
    public class CashedBarcodes
    {

        public CashedBarcodes()
        {
            lastUpdatedTimeBarcodes = Db.GetLastUpdatedTimeBarcodes();
            if (lastUpdatedTimeBarcodes < Convert.ToDateTime("2021/02/10"))
            {
                Shared.SaveToLogInformation("lastUpdatedTimeBarcodes < 20210210 => " + lastUpdatedTimeBarcodes.ToString());
                lastUpdatedTimeBarcodes = Convert.ToDateTime("2021/02/10");
            }
//#if DEBUG && !ASRELEASE
//            lastUpdatedTimeBarcodes = Convert.ToDateTime("2021/02/10 09:00:00");
//#endif
            InitCountBarcodes();
            //UpdateBarcodes(true);
        }

        //public CashedBarcodes(DateTime date)
        //{
        //    lastUpdatedTimeBarcodes = Db.GetLastUpdatedTimeBarcodes();
        //    if (lastUpdatedTimeBarcodes < Convert.ToDateTime("2021/02/10"))
        //    {
        //        Shared.SaveToLog("lastUpdatedTimeBarcodes < 20210210 => " + lastUpdatedTimeBarcodes.ToString());
        //        lastUpdatedTimeBarcodes = Convert.ToDateTime("2021/02/10");
        //    }
        //    //InitCountBarcodes();
        //    //UpdateBarcodes(date, true);
        //}


        public bool UpdateBarcodes(bool isFirst)
        {
//#if DEBUG && !ASRELEASE
//            return true;
//#endif
#if OUTPUTDEBUGINFO
            System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!Db.GetServerDateTime()!" + Environment.NewLine);
#endif
            var serverDateTime = Db.GetServerDateTime();
            if (serverDateTime == null)
                return false;
            else
            {
                try
                {
                    if (!Shared.IsLocalDateTimeUpdated)
                        Shared.SetSystemDateTime((DateTime)serverDateTime);
                }
                catch
                {
                    Shared.ShowMessageError("ERROR SetSystemDateTime");
#if OUTPUTDEBUGINFO
                    System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " ERROR !!!!!UpdateBarcodes(date)!" + Environment.NewLine); 
#endif
                }
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UpdateBarcodes(date)!" + Environment.NewLine);
#endif
                return UpdateBarcodes((DateTime)serverDateTime, isFirst);
            }
        }

        private static DateTime _lastUpdatedTimeBarcodes { get; set; }
        private static DateTime lastUpdatedTimeBarcodes
        {
            get
            {
                return _lastUpdatedTimeBarcodes;
            }

            set
            {
                if (Db.UpdateLastUpdatedTimeBarcodes(value))
                    _lastUpdatedTimeBarcodes = value;
            }
        }

        public DateTime GetLastUpdatedTimeBarcodesMoscowTimeZone
        {
            get
            {
                return lastUpdatedTimeBarcodes.AddHours(3);
            }
        }

        private DateTime localDbBarcodesDateCreated { get; set; }

        public DateTime GetLocalDbBarcodesDateCreated
        {
            get
            {
                return localDbBarcodesDateCreated;
            }
        }

        private static int? countBarcodeNomenclatures { get; set; }

        private static int? countBarcodeProducts { get; set; }

        public void AddedBarcode(int? kindId)
        {
            if (kindId == null)
                countBarcodeNomenclatures = countBarcodeNomenclatures + 1;
            else
                countBarcodeProducts = countBarcodeProducts + 1;
        }
        public void RemovedBarcode(int? kindId)
        {
            if (kindId == null)
                countBarcodeNomenclatures = countBarcodeNomenclatures - 1;
            else
                countBarcodeProducts = countBarcodeProducts -1;
        }

        public bool InitCountBarcodes()
        {
            if (countBarcodeNomenclatures == null && countBarcodeProducts == null)
            {
                var countTable = Db.GetCountBarcodes();
                if (countTable != null && countTable.Rows.Count > 0)
                //    {
                //        ret = table.Rows[0]["CountBarcodes"].ToString() + "/" + table.Rows[0]["CountNomenclatures"].ToString() + "/" + table.Rows[0]["CountProducts"].ToString();
                //    }
                {
                    countBarcodeNomenclatures = countTable.Rows[0].IsNull("CountNomenclatures") ? 0 : Convert.ToInt32(countTable.Rows[0]["CountNomenclatures"]);
                    countBarcodeProducts = countTable.Rows[0].IsNull("CountProducts") ? 0 : Convert.ToInt32(countTable.Rows[0]["CountProducts"]);
                    //if (countBarcodeNomenclatures < 1000)
                    //    countBarcodeNomenclatures = Db.GetBarcodes1C(lastUpdatedTimeBarcodes);
                }
                else
                {
                    countBarcodeNomenclatures = 0;
                    countBarcodeProducts = 0;
                }
                Db.SetCountBarcodeNomenclatures(countBarcodeNomenclatures);
                Db.SetCountBarcodeProducts(countBarcodeProducts);
                localDbBarcodesDateCreated = Db.GetLocalDbBarcodesDateCreated();
            }
                return true;
        }
        
        public string GetCountBarcodes
        {
            get
            {
                return Convert.ToString((countBarcodeNomenclatures ?? 0) + (countBarcodeProducts ?? 0)) + "/" + (countBarcodeNomenclatures ?? 0).ToString() + "/" + (countBarcodeProducts ?? 0).ToString();// Db.GetCountBarcodes();
            }
        }

        public bool UpdateBarcodes(DateTime date, bool IsFirst)
        {
            if (0==0 && (!IsFirst || (IsFirst && !Settings.GetCurrentServerIsExternal())))
                Db.UploadLogToServer();
            if (lastUpdatedTimeBarcodes < date)
            {
                if (!IsFirst || (IsFirst && !Shared.IsNotUpdateCashedBarcodesOnFirst))
                {
                    var _lastDate = Db.UpdateCashedBarcodes(lastUpdatedTimeBarcodes, date, IsFirst);
                    if (_lastDate != null)
                    {
                        if (lastUpdatedTimeBarcodes != _lastDate)
                            lastUpdatedTimeBarcodes = _lastDate;
                        //if (IsFirst)
                        //{
                        //    if (Db.GetCountNomenclatureBarcodes() < 1000)
                        //        Db.GetBarcodes1C(lastUpdatedTimeBarcodes);
                        //}
                        return true;
                    }
                }
            }
            return false;
        }
        /*
        public bool GetExistsBarcodeOrNumberInBarcodes(string barcode)
        {
            DbProductIdFromBarcodeResult getProductResult = Db.GetFirstProductFromCashedBarcodes(barcode);
            if (getProductResult == null)
            {
                getProductResult = Db.GetFirstNomenclatureFromCashedBarcodes(barcode);
                if (getProductResult == null)
                {
                    getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
                    if (getProductResult == null || getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                return true;
        }
        */
        public DbProductIdFromBarcodeResult GetProductFromBarcodeOrNumberInBarcodes(string barcode, bool IsFirstFindInLocalDB)
        {

            DbProductIdFromBarcodeResult getProductResult = null;
            
            {
                if (IsFirstFindInLocalDB) getProductResult = Db.GetFirstProductFromCashedBarcodes(barcode);
                if (getProductResult == null)
                {
                    getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
//#if !(DEBUG && !ASRELEASE)            
                    if (getProductResult == null || getProductResult.ProductKindId == null)
//#endif
                    {
                        if (!IsFirstFindInLocalDB) getProductResult = Db.GetFirstProductFromCashedBarcodes(barcode);
                        if (getProductResult == null)
                        {
                            var list = Db.GetAllNomenclatureFromCashedBarcodes(barcode);
                            if (list == null)
                            {
                                getProductResult = null;
                            }
                            else
                            {
                                if (list.Count == 1)
                                {
                                    getProductResult = list[0];
                                    getProductResult.ProductKindId = ProductKind.ProductMovement;
                                }
                                else
                                {//Россыпь, но кол-во найденных номенклатур больше 1, поэтому нельзя однозначно определить номенклатуру
                                    getProductResult = new DbProductIdFromBarcodeResult()
                                    {
                                        ProductKindId = ProductKind.ProductMovement,
                                        ProductId = new Guid(),
                                        NomenclatureId = new Guid(),
                                        CharacteristicId = new Guid(),
                                        QualityId = new Guid(),
                                        MeasureUnitId = new Guid(),
                                        CountProducts = 0
                                    };
                                }
                            }
                        }
                    }
                }
            }
            return getProductResult;
        }

        public List<ChooseNomenclatureItem> GetNomenclaturesFromBarcodeInBarcodes(string barcode)
        {
            return Db.GetBarcodes1C(barcode);
        }
    }
}
