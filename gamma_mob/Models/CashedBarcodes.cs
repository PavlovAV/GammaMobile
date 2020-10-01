﻿using System;
using System.Collections.Generic;

namespace gamma_mob.Models
{
    public class CashedBarcodes
    {

        public CashedBarcodes()
        {
            lastUpdatedTimeBarcodes = Db.GetLastUpdatedTimeBarcodes();
            UpdateBarcodes(true);
        }

        public CashedBarcodes(DateTime date)
        {
            lastUpdatedTimeBarcodes = Db.GetLastUpdatedTimeBarcodes();
            UpdateBarcodes(date, true);
        }


        public bool UpdateBarcodes(bool isFirst)
        {
            System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!Db.GetServerDateTime()!" + Environment.NewLine);
            var date = Db.GetServerDateTime();
            if (date == null)
                return false;
            else
            {
                System.Diagnostics.Debug.Write(DateTime.Now.ToString() + " !!!!!UpdateBarcodes(date)!" + Environment.NewLine);
                return UpdateBarcodes(date, isFirst);
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

        public bool UpdateBarcodes(DateTime date, bool IsFirst)
        {
            Db.UploadLogToServer();
            if (lastUpdatedTimeBarcodes < date)
            {
                var _lastDate = Db.UpdateCashedBarcodes(lastUpdatedTimeBarcodes, date, IsFirst);
                if (_lastDate != null)
                {
                    if (lastUpdatedTimeBarcodes != _lastDate)
                        lastUpdatedTimeBarcodes = _lastDate;
                    return true;
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
        public DbProductIdFromBarcodeResult GetProductFromBarcodeOrNumberInBarcodes(string barcode)
        {
            
            DbProductIdFromBarcodeResult getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
#if !DEBUG            
            if (getProductResult == null)
#endif
            {
                getProductResult = Db.GetFirstProductFromCashedBarcodes(barcode);
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
                            getProductResult.ProductKindId = 3;
                        }
                        else
                        {//Россыпь, но кол-во найденных номенклатур больше 1, поэтому нельзя однозначно определить номенклатуру
                            getProductResult = new DbProductIdFromBarcodeResult()
                            {
                                ProductKindId = 3,
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
            return getProductResult;
        }

        public List<ChooseNomenclatureItem> GetNomenclaturesFromBarcodeInBarcodes(string barcode)
        {
            return Db.GetBarcodes1C(barcode);
        }
    }
}
