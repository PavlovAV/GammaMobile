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
            DbProductIdFromBarcodeResult getFromProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
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
                            MessageBox.Show(@"Ошибка! Не совпадает номенклатура продукта упаковки и паллеты!", @"Продукт не добавлен",
                                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        else
                        {
                        using (var form = new SetCountProductsDialog())
                        {
                            DialogResult result = form.ShowDialog();
                            Invoke((MethodInvoker)Activate);
                            if (result != DialogResult.OK || form.Quantity == null)
                            {
                                MessageBox.Show(@"Не указано количество продукта. Продукт не добавлен!", @"Продукт не добавлен",
                                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                                return;
                            }
                            else
                            {
                                if (Db.CheckWhetherProductCanBeWithdrawal(getFromProductResult.ProductId, form.Quantity))
                                    getFromProductResult.CountProducts = form.Quantity;
                                else
                                {
                                    MessageBox.Show(@"Ошибка! Недостаточно количества продукта в паллете для списания!", @"Продукт не добавлен",
                                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                                    return;
                                }
                            }
                        }
                        }
                        Invoke((MethodInvoker)(() => ChooseNomenclatureCharacteristicForm.ReturnedResult = false));
                        Invoke((MethodInvoker)delegate()
                        {
                            ChooseNomenclatureCharacteristicForm.Close();
                        });
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.NomenclatureId = getFromProductResult.NomenclatureId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CharacteristicId = getFromProductResult.CharacteristicId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.QualityId = getFromProductResult.QualityId;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.CountProducts = getFromProductResult.CountProducts;
                        (Param as AddProductReceivedEventHandlerParameter).getProductResult.FromProductId = getFromProductResult.ProductId;
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
                        MessageBox.Show(@"Ошибка! Попробуйте еще раз или выберите номенклатуру", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
                }
            }
            else
            {
                MessageBox.Show(@"Ошибка! Штрих-код продукта не распознан! Попробуйте еще раз или выберите номенклатуру", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
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
                        MessageBox.Show(@"Ошибка! Попробуйте еще раз или выберите номенклатуру", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
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
                        MessageBox.Show(@"Ошибка! Попробуйте еще раз или выберите номенклатуру", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
                }
            }
        }

        
    }
}
