using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using gamma_mob.WCF;
using System.Collections.Generic;


namespace gamma_mob
{
    public partial class PalletForm : BaseFormWithBarcodeScan, INotifyPropertyChanged
    {
        private PalletForm()
        {   
            Items = new BindingList<DocNomenclatureItem>();
            BSource = new BindingSource {DataSource = Items} ;
            InitializeComponent();
        }

        private PalletForm(Form parentForm) : this()
        {
            ParentForm = parentForm;
        }

        public PalletForm(Form parentForm, Pallet pallet) : this(parentForm)
        {
            DocOrderId = pallet.DocOrderId;
            ProductId = pallet.ProductId;
            DocDirection = pallet.DocDirection;
            Number = pallet.Number;
            Text = "Паллета " + Number;
            var list = pallet.Items;
            if (Shared.LastQueryCompleted == false || list == null)
            {
                Shared.ShowMessageError(@"Не удалось получить информацию о текущем документе");
                Close();
                return;
            }
            Items = new BindingList<DocNomenclatureItem>(list);
            BSource.DataSource = Items;
            gridPalletItems.DataSource = BSource;
        }
        
        private Guid DocOrderId { get; set; }

        private DocDirection DocDirection { get; set; }

        private BindingSource BSource { get; set; }
        
        private BindingList<DocNomenclatureItem> Items { get; set; }
        
        private Guid ProductId { get; set; }

        private string Number { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivatePanels(new List<int>() { (int)Images.Back, (int)Images.Inspect, /*(int)Images.Remove, (int)Images.Print,*/ (int)Images.InfoProduct, (int)Images.RDP });//, pnlToolBar_ButtonClick);
            // Оформление грида
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Номенклатура",
                MappingName = "ShortNomenclatureName",
                Width = 156
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Кол-во",
                MappingName = "CollectedQuantity",
                Width = 38
            });
            gridPalletItems.TableStyles.Add(tableStyle);
        }

        protected override void ActionByBarcode(string barcode)
        {
            if (Number == String.Empty)
            {
                int placeId = Shared.PlaceId;
                Guid? placeZoneId = null;
                using (var formPlaceZone = new ChooseEndPointDialog(placeId))
                {
                    DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                    if (resultPlaceZone != DialogResult.OK)
                    {
                        Shared.ShowMessageInformation(@"Не выбрана зона склада.");
                        return;
                    }
                    else
                    {
                        placeId = formPlaceZone.EndPointInfo.PlaceId;
                        placeZoneId = formPlaceZone.EndPointInfo.PlaceZoneId;
                    }
                }
                var result = Db.CreateNewPallet(ProductId, DocOrderId, placeId, placeZoneId);
                if (result == null)
                {
                    Shared.ShowMessageError("Непредвиденная ошибка при выполнении операции");
                    return;
                } else if (!String.IsNullOrEmpty(result.ResultMessage))
                {
                    Shared.ShowMessageError(result.ResultMessage);
                    return;
                }
                Number = result.Number;
                Text = "Паллета " +  Number;
                NotifyPropertyChanged("Text");
                (ParentForm as PalletsForm).AddPalletToPallets(new PalletListItem(ProductId, Number, (DateTime)result.Date, result.Person));
            }
            AddNomenclatureByBarcode(barcode);
        }

        protected override void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            if (gridPalletItems == null || gridPalletItems.CurrentRowIndex < 0)
            {
                Shared.ShowMessageError(@"Выберите номенклатуру!");
                return;
            }
            var good = Items[gridPalletItems.CurrentRowIndex];
            var form = new PalletItemProductsForm(ProductId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this, new RefreshPalletItemsDelegate(RefreshPallet));
            if (!form.IsDisposed)
            {
                BarcodeFunc = null;
                DialogResult result = form.ShowDialog();
                if (form.IsRefreshQuantity)
                    RefreshPallet(ProductId, true);
            }
        }

        private void RefreshPallet(Guid productId, bool showMessage)
        {
            if (!RefreshPalletItems(productId))
            {
                if (showMessage)
                    Shared.ShowMessageInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + "Попробуйте ещё раз обновить!");
                else
                    Shared.SaveToLogInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + "Попробуйте ещё раз обновить!");
                //Close();
                return;
            }
        }

        private bool RefreshPalletItems(Guid productId)
        {
            List<DocNomenclatureItem> list = Db.GetPalletItems(productId);
            if (Shared.LastQueryCompleted == false)// || list == null)
            {
               // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                if (Items == null)
                    Items = new BindingList<DocNomenclatureItem>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = Items };
                return false;
            }
            Items = new BindingList<DocNomenclatureItem>(list) ?? new BindingList<DocNomenclatureItem>();
            if (BSource == null)
                BSource = new BindingSource {DataSource = Items};
            else
            {
                BSource.DataSource = Items;
            }

            gridPalletItems.DataSource = BSource;
            gridPalletItems.UnselectAll();
            
            return true;
        }

        private void AddNomenclatureByBarcode(string barcode)
        {
            //UIServices.SetBusyState(this);

            {
                {
                    Cursor.Current = Cursors.WaitCursor;
                    DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                    Cursor.Current = Cursors.Default;

                    if (getProductResult == null || getProductResult.ProductKindId == null || (Shared.FactProductKinds.Contains((ProductKind)getProductResult.ProductKindId) && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                    {
                        Shared.ShowMessageError(@"Продукция не найдена по ШК! " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                    }
                    else
                    {
                        if (getProductResult.ProductKindId == ProductKind.ProductMovement && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty))
                        {
                            //if (CheckIsCreatePalletMovementFromBarcodeScan())
                            {
                                base.ChooseNomenclatureCharacteristic(this.ChooseNomenclatureCharacteristicBarcodeReactionInAddNomenclature, new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, getProductResult = getProductResult });
                            }
                        }
                        else
                        {
                            AddNomenclatureByBarcode(barcode, getProductResult);
                        }
                    }
                }
            }
        }

        private void AddNomenclatureByBarcode(string barcode, DbProductIdFromBarcodeResult getProductResult)
        {
            var scanId = Shared.ScannedBarcodes.AddScannedBarcode(barcode, new EndPointInfo(), DocDirection, DocOrderId, getProductResult);
            if (scanId == null || scanId == Guid.Empty)
                Shared.ShowMessageError("Ошибка2 при сохранении отсканированного штрих-кода");
            else 
                AddNomenclatureByBarcode(scanId, barcode, getProductResult);
        }

        private bool? AddNomenclatureByBarcode(Guid? scanId, string barcode, DbProductIdFromBarcodeResult getProductResult)
        {
            Shared.SaveToLogInformation(@"AddNomenclature " + barcode + @"; scanId-" + scanId.ToString() + @"; Q-" + getProductResult.CountProducts);
            Cursor.Current = Cursors.WaitCursor;
            Shared.ScannedBarcodes.UploadedScan(scanId, (Guid?)null);
            var addResult = Db.AddItemToPallet(scanId, ProductId, DocOrderId, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts, getProductResult.FromProductId);
            //var addResult = Db.AddItemToPallet(ProductId, DocOrderId, barcode, getProductResult.CountProducts);
            if (Shared.LastQueryCompleted == false)
            {
                Shared.SaveToLogError(@"AddItemToPallet.LastQueryCompleted is null (scanId = " + scanId.ToString() + ")");
                return null;
            }
            if (addResult == null)
            {
                Shared.ShowMessageError(@"Не удалось добавить номенклатуру" + Environment.NewLine + barcode + " в продукт "+ ProductId.ToString());
                Shared.ScannedBarcodes.ClearLastBarcode();
                return false;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                //Shared.ScannedBarcodes.UploadedScan(scanId, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                //UpdateGrid(addResult, getProductResult.ProductKindId, endPointInfo, scanId);
                Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                          new object[] { addResult.NomenclatureId, addResult.CharacteristicId, addResult.QualityId, addResult.NomenclatureName, 
                                addResult.ShortNomenclatureName, addResult.Quantity, null});
            }
            else
            {
                //Shared.ScannedBarcodes.UploadedScanWithError(scanId, addResult.ResultMessage, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                Shared.ShowMessageError(@"ШК: " + barcode + Environment.NewLine + addResult.ResultMessage);
                Shared.ScannedBarcodes.ClearLastBarcode();
            }
            //Shared.ScannedBarcodes.UploadedScan(scanId, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
            //Shared.ScannedBarcodes.UploadedScan(scanId, (Guid?)null);
            UIServices.SetNormalState(this);
            return true;
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, int? productKindId)
        {
            DocNomenclatureItem item =
                Items.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId);
            if (item == null)
            {
                item = new DocNomenclatureItem
                {
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    QualityId = qualityId,
                    NomenclatureName = nomenclatureName,
                    ShortNomenclatureName = shortNomenclatureName,
                    CollectedQuantity = 0,
                };
                Items.Add(item);
                BSource.DataSource = Items;
            }
            item.CollectedQuantity += quantity;
            gridPalletItems.UnselectAll();
            int index = Items.IndexOf(item);
            gridPalletItems.CurrentRowIndex = index;
            gridPalletItems.Select(index);
        }

        private void ChooseNomenclatureCharacteristicBarcodeReactionInAddNomenclature(AddProductReceivedEventHandlerParameter param)
        {
            base.ReturnAddProductBeforeChoosedNomenclatureCharacteristic -= ChooseNomenclatureCharacteristicBarcodeReactionInAddNomenclature;
            BarcodeFunc = this.BarcodeReaction;
            if (param != null)
                AddNomenclatureByBarcode(param.barcode, param.getProductResult);
        }

        private void gridPalletItems_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

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