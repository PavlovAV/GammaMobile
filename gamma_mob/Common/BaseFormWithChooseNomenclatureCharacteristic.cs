using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithChooseNomenclatureCharacteristic : BaseFormWithChooseEndpoint
    {
        /// <summary>
        ///     Зона склада (заполнена, если ипсользуется, т.е. в складе выделены зоны и эта зона выбрана)
        /// </summary>
        //protected EndPointInfo EndPointInfo { get; set; }

        //protected virtual EndPointInfo GetNomenclatureCharacteristicId(EndPointInfo endPointInfo)
        //{
        //    return null;
        //}
        
        private ChooseNomenclatureCharacteristicDialog ChooseNomenclatureCharacteristicForm;

        protected event AddProductReceivedEventHandler ReturnAddProductBeforeChoosedNomenclatureCharacteristic;
        protected event ChooseNomenclatureCharacteristicEventHandler ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic;

        private Object Param;

        public void ChooseNomenclatureCharacteristic(string barcode)
        {
            ChooseNomenclatureCharacteristicForm = new ChooseNomenclatureCharacteristicDialog(barcode, this);//, barcode, fromBuffer, getProductResult, this);
            ChooseNomenclatureCharacteristicForm.Show();
            ChooseNomenclatureCharacteristicForm.SetBarcodeReaction(ChooseNomenclatureCharacteristicBarcodeReaction);
        }

        public void ChooseNomenclatureCharacteristic(AddProductReceivedEventHandler returnBeforeChoosedNomenclatureCharacteristic, AddProductReceivedEventHandlerParameter param)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Param = param;
            ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic = null;
            ReturnAddProductBeforeChoosedNomenclatureCharacteristic += returnBeforeChoosedNomenclatureCharacteristic;
            ChooseNomenclatureCharacteristic(param.barcode);
        }

        //public void ChooseNomenclatureCharacteristic(ChooseNomenclatureCharacteristicEventHandler returnBeforeChoosedNomenclatureCharacteristic)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        //{
        //    Param = null;
        //    ReturnAddProductBeforeChoosedNomenclatureCharacteristic = null;
        //    ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic += returnBeforeChoosedNomenclatureCharacteristic;
        //    ChooseNomenclatureCharacteristic();
        //}

        protected void ChooseNomenclatureCharacteristicBarcodeReaction(string barcode)
        {
            /*if (ChooseNomenclatureCharacteristicForm != null)
            {
                DbProductIdFromBarcodeResult getFromProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                PlaceZone fromPlaceZone = null;
                if (!(getFromProductResult != null && getFromProductResult.ProductId != null && getFromProductResult.ProductId != Guid.Empty))
                {
                    fromPlaceZone = Shared.PlaceZones.FirstOrDefault(z => z.Barcode == barcode);
                    if (fromPlaceZone != null)
                    {
                        if (fromPlaceZone.IsValid)
                            getFromProductResult = Db.GetSingleProductInPlaceZone(fromPlaceZone.PlaceZoneId);
                    }
                }
                if (ReturnAddProductBeforeChoosedNomenclatureCharacteristic != null && (Param is AddProductReceivedEventHandlerParameter))
                {
                    if (fromPlaceZone != null && !(fromPlaceZone.IsValid == true))
                    {
                        Shared.ShowMessageError(@"Ошибка! Зона " + fromPlaceZone.Name + @" отключена для использования!");
                        Param = null;
                    }
                    else if (!(getFromProductResult != null && getFromProductResult.ProductId != null && getFromProductResult.ProductId != Guid.Empty))
                    {
                        if (fromPlaceZone != null && fromPlaceZone.IsValid == true)
                            Shared.ShowMessageError(@"Ошибка при выборе паллеты: в зоне " + fromPlaceZone.Name + Environment.NewLine + @" сейчас ни одного или сразу несколько паллет!");
                        else
                            Shared.ShowMessageError(@"Ошибка! Штрих-код " + barcode + @" не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру(если возможно)");
                        Param = null;
                    }
                    else if (!(ChooseNomenclatureCharacteristicForm.CheckNomenclatureInNomenclatureList(getFromProductResult.ProductId)
                        //ChooseNomenclatureCharacteristicForm.NomenclatureId == getFromProductResult.NomenclatureId  
                        //&& (((ChooseNomenclatureCharacteristicForm.CharacteristicId ?? Guid.Empty) == Guid.Empty && (getFromProductResult.CharacteristicId ?? Guid.Empty) == Guid.Empty) || (ChooseNomenclatureCharacteristicForm.CharacteristicId != Guid.Empty && getFromProductResult.CharacteristicId != Guid.Empty && ChooseNomenclatureCharacteristicForm.CharacteristicId == getFromProductResult.CharacteristicId))
                        //&& (((ChooseNomenclatureCharacteristicForm.QualityId ?? Guid.Empty) == Guid.Empty && (getFromProductResult.QualityId ?? Guid.Empty) == Guid.Empty) || (ChooseNomenclatureCharacteristicForm.QualityId != Guid.Empty && getFromProductResult.QualityId != Guid.Empty && ChooseNomenclatureCharacteristicForm.QualityId == getFromProductResult.QualityId))
                            ))
                    {
                        Shared.ShowMessageError(@"Ошибка! Не совпадает номенклатура продукта упаковки и паллеты!");
                        Shared.SaveToLogError(@"Паллета: " + getFromProductResult.NomenclatureId + @"/" + getFromProductResult.CharacteristicId + @"/" + getFromProductResult.QualityId);
                        //return;
                        Param = null;
                    }
                    else
                    {
                        using (var form = new SetCountProductsDialog())
                        {
                            DialogResult result = form.ShowDialog();
                            Invoke((MethodInvoker)Activate);
                            if (result != DialogResult.OK || form.Quantity == null)
                            {
                                Shared.ShowMessageError(@"Не указано количество продукта. Продукт не добавлен!");
                                //return;
                                Param = null;
                            }
                            else
                            {
                                if (Db.CheckWhetherProductCanBeWithdrawal(getFromProductResult.ProductId, form.Quantity))
                                    getFromProductResult.CountProducts = form.Quantity;
                                else
                                {
                                    Shared.ShowMessageError(@"Ошибка! Недостаточно количества продукта в паллете " + barcode + @" для списания!", @"CheckWhetherProductCanBeWithdrawal (getFromProductResult.ProductId = " + getFromProductResult.ProductId +@", form.Quantity = " + form.Quantity +@") return false", null, getFromProductResult.ProductId);
                                    //return;
                                    Param = null;
                                }
                            }
                        }
                    }
                    Invoke((MethodInvoker)(() => ChooseNomenclatureCharacteristicForm.ReturnedResult = false));
                    Invoke((MethodInvoker)delegate()
                    {
                        ChooseNomenclatureCharacteristicForm.Close();
                    });
                    if (Param != null)
                    {
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.NomenclatureId = getFromProductResult.NomenclatureId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CharacteristicId = getFromProductResult.CharacteristicId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.QualityId = getFromProductResult.QualityId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CountProducts = getFromProductResult.CountProducts;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.FromProductId = getFromProductResult.ProductId;
                    }
                    Invoke((MethodInvoker)delegate()
                    {
                        ReturnAddProductBeforeChoosedNomenclatureCharacteristic((Param as AddProductReceivedEventHandlerParameter));
                    });
                }
                else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                {
                    Invoke((MethodInvoker)(() => ChooseNomenclatureCharacteristicForm.ReturnedResult = false));
                    Invoke((MethodInvoker)delegate()
                    {
                        ChooseNomenclatureCharacteristicForm.Close();
                    });
                    Invoke((MethodInvoker)delegate()
                    {
                        ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(getFromProductResult);
                    });
                }
            }
            */
            if (ChooseNomenclatureCharacteristicForm != null)
            {
                Invoke((MethodInvoker)delegate()
                {
                    ChooseNomenclatureCharacteristicForm.SetBarcodeText(barcode);
                });
            }
            DbProductIdFromBarcodeResult getFromProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
            PlaceZone fromPlaceZone = null;
            if (!(getFromProductResult != null && getFromProductResult.ProductId != null && getFromProductResult.ProductId != Guid.Empty))
            {
                fromPlaceZone = Shared.PlaceZones.FirstOrDefault(z => z.Barcode == barcode);
                if (fromPlaceZone != null)
                {
                    if (fromPlaceZone.IsValid)
                        getFromProductResult = Db.GetSingleProductInPlaceZone(fromPlaceZone.PlaceZoneId);
                    else
                        Shared.ShowMessageError(@"Ошибка! Зона " + fromPlaceZone.Name + @" отключена для использования!"
                            + Environment.NewLine + @"Выберите другую зону.");
                }
            }
            if (getFromProductResult != null && getFromProductResult.ProductId != null && getFromProductResult.ProductId != Guid.Empty)
            {
                if (ChooseNomenclatureCharacteristicForm != null) 
                {
                    //var endPointInfo = new EndPointInfo() { PlaceId = placeZone.PlaceId, NomenclatureCharacteristicId = placeZone.NomenclatureCharacteristicId, NomenclatureCharacteristicName = placeZone.Name };
                    if (ReturnAddProductBeforeChoosedNomenclatureCharacteristic != null && (Param is AddProductReceivedEventHandlerParameter))
                    {
                        if (!(ChooseNomenclatureCharacteristicForm.CheckNomenclatureInNomenclatureList(getFromProductResult.ProductId)
                            //ChooseNomenclatureCharacteristicForm.NomenclatureId == getFromProductResult.NomenclatureId  
                            //&& (((ChooseNomenclatureCharacteristicForm.CharacteristicId ?? Guid.Empty) == Guid.Empty && (getFromProductResult.CharacteristicId ?? Guid.Empty) == Guid.Empty) || (ChooseNomenclatureCharacteristicForm.CharacteristicId != Guid.Empty && getFromProductResult.CharacteristicId != Guid.Empty && ChooseNomenclatureCharacteristicForm.CharacteristicId == getFromProductResult.CharacteristicId))
                            //&& (((ChooseNomenclatureCharacteristicForm.QualityId ?? Guid.Empty) == Guid.Empty && (getFromProductResult.QualityId ?? Guid.Empty) == Guid.Empty) || (ChooseNomenclatureCharacteristicForm.QualityId != Guid.Empty && getFromProductResult.QualityId != Guid.Empty && ChooseNomenclatureCharacteristicForm.QualityId == getFromProductResult.QualityId))
                            ))
                        {
                            Shared.ShowMessageError(@"Ошибка! Не совпадает номенклатура продукта упаковки и паллеты!");
                            Shared.SaveToLogError(@"Паллета: " + getFromProductResult.NomenclatureId + @"/" + getFromProductResult.CharacteristicId + @"/" + getFromProductResult.QualityId);
                            //return;
                            Param = null;
                        }
                        else
                        {
                            using (var form = new SetCountProductsDialog())
                            {
                                DialogResult result = form.ShowDialog();
                                Invoke((MethodInvoker)Activate);
                                if (result != DialogResult.OK || form.Quantity == null)
                                {
                                    Shared.ShowMessageError(@"Не указано количество продукта. Продукт не добавлен!");
                                    //return;
                                    Param = null;
                                }
                                else
                                {
                                    if (Db.CheckWhetherProductCanBeWithdrawal(getFromProductResult.ProductId, form.Quantity))
                                        getFromProductResult.CountProducts = form.Quantity;
                                    else
                                    {
                                        Shared.ShowMessageError(@"Ошибка! Недостаточно количества продукта в паллете для списания!");
                                        //return;
                                        Param = null;
                                    }
                                }
                            }
                        }
                        Invoke((MethodInvoker)(() => ChooseNomenclatureCharacteristicForm.ReturnedResult = false));
                        Invoke((MethodInvoker)delegate()
                        {
                            ChooseNomenclatureCharacteristicForm.Close();
                        });
                        if (Param != null)
                        {
                            (Param as AddProductReceivedEventHandlerParameter).getProductResult.NomenclatureId = getFromProductResult.NomenclatureId;
                            (Param as AddProductReceivedEventHandlerParameter).getProductResult.CharacteristicId = getFromProductResult.CharacteristicId;
                            (Param as AddProductReceivedEventHandlerParameter).getProductResult.QualityId = getFromProductResult.QualityId;
                            (Param as AddProductReceivedEventHandlerParameter).getProductResult.CountProducts = getFromProductResult.CountProducts;
                            (Param as AddProductReceivedEventHandlerParameter).getProductResult.FromProductId = getFromProductResult.ProductId;
                        }
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeChoosedNomenclatureCharacteristic((Param as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                    {
                        Invoke((MethodInvoker)(() => ChooseNomenclatureCharacteristicForm.ReturnedResult = false));
                        Invoke((MethodInvoker)delegate()
                        {
                            ChooseNomenclatureCharacteristicForm.Close();
                        });
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(getFromProductResult);
                        });
                    }
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите номенклатуру");
                }
            }
            else
            {
                if (getFromProductResult != null)
                {
                    if (getFromProductResult.CountProducts == 0)
                        Shared.ShowMessageError(@"Ошибка! В зоне нет никакой продукции!" + Environment.NewLine + @"Выберите другую зону");
                    else if (getFromProductResult.CountProducts > 1)
                        Shared.ShowMessageError(@"Ошибка! В зоне больше одной паллеты!" + Environment.NewLine + @"Выберите другую зону или отсканируйте этикетку паллеты(если возможно)");
                }
                else
                    Shared.ShowMessageError(@"Ошибка! Штрих-код не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите номенклатуру(если возможно)");
            }
        }

        public void ClosingChooseNomenclatureCharacteristicDialog()
        {
            if (ChooseNomenclatureCharacteristicForm != null)
            {
                var getFromProductResult = new DbProductIdFromBarcodeResult() { 
                        NomenclatureId = ChooseNomenclatureCharacteristicForm.NomenclatureId,
                        CharacteristicId = ChooseNomenclatureCharacteristicForm.CharacteristicId,
                        QualityId = ChooseNomenclatureCharacteristicForm.QualityId,
                        CountProducts = ChooseNomenclatureCharacteristicForm.CountProducts,
                        FromProductId = null
                };
                if (ChooseNomenclatureCharacteristicForm.DialogResult == DialogResult.OK)
                {
                    if (ReturnAddProductBeforeChoosedNomenclatureCharacteristic != null && (Param is AddProductReceivedEventHandlerParameter))
                    {
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.NomenclatureId = getFromProductResult.NomenclatureId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CharacteristicId = getFromProductResult.CharacteristicId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.QualityId = getFromProductResult.QualityId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CountProducts = getFromProductResult.CountProducts;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.FromProductId = getFromProductResult.FromProductId;
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeChoosedNomenclatureCharacteristic((Param as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(getFromProductResult);
                        });
                    }
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите номенклатуру");
                }
                else
                {
                    if (ReturnAddProductBeforeChoosedNomenclatureCharacteristic != null)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeChoosedNomenclatureCharacteristic((null as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(null as DbProductIdFromBarcodeResult);
                        });
                    }
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите номенклатуру");
                }
            }
        }

        
    }
}
