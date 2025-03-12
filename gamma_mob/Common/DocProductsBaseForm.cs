using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using System.Collections.Generic;
using gamma_mob.Models;
using OpenNETCF.Windows.Forms;
using gamma_mob.CustomDataGrid;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class DocProductsBaseForm : BaseFormWithProducts
    {
        protected DocProductsBaseForm()
        {
            InitializeComponent();
            Shared.MakeButtonMultiline(btnAdd);
            btnAdd.Text = "Доба" + Environment.NewLine + "вить";
        }

        protected DocProductsBaseForm(Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, bool isEnableAddProductManual)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            ProductKindId = productKindId;
            pnlQuantity.Visible = isEnableAddProductManual;
            if (isEnableAddProductManual)
                qmuQuantity.FillMeasureUnitList(Shared.GetMeasureUnitsForNomenclature(NomenclatureId, CharacteristicId));
        }

        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Form parentForm, bool isEnableAddProductManual)
            : this(nomenclatureId, nomenclatureName, characteristicId, qualityId, productKindId, isEnableAddProductManual)
        {
            ParentForm = parentForm;
            DocId = docShipmentOrderId;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }

        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Form parentForm, DocDirection docDirection, bool isMovementForOrder, OrderType orderType, RefreshDocProductDelegate refreshDocOrder, EndPointInfo startPointInfo, EndPointInfo endPointInfo, bool isEnableAddProductManual)
            : this(nomenclatureId, nomenclatureName, characteristicId, qualityId, productKindId, isEnableAddProductManual)
        {
            ParentForm = parentForm;
            DocId = docShipmentOrderId;
            DocDirection = docDirection;
            OrderType = orderType;
            IsMovementForOrder = isMovementForOrder;
            RefreshDocOrder = refreshDocOrder;
            StartPointInfo = startPointInfo;
            EndPointInfo = endPointInfo;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }


        public DocProductsBaseForm(int placeId, Guid personId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Guid? placeZoneId, Form parentForm, bool isEnableAddProductManual)
            : this(nomenclatureId, nomenclatureName, characteristicId, qualityId, productKindId, isEnableAddProductManual)
        {
            ParentForm = parentForm;
            PlaceId = placeId;
            PersonId = personId;
            PlaceZoneId = placeZoneId;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }

        public DocProductsBaseForm(Guid productId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, byte? productKindId, Form parentForm, RefreshPalletItemsDelegate refreshPalletItems)
            : this(nomenclatureId, nomenclatureName, characteristicId, qualityId, productKindId, false)
        {
            ParentForm = parentForm;
            ProductId = productId;
            RefreshPalletItems = refreshPalletItems;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }

        protected int PlaceId { get; set; }
        protected Guid PersonId { get; set; }
        protected Guid NomenclatureId { get; set; }
        protected Guid CharacteristicId { get; set; }
        protected Guid QualityId { get; set; }
        protected byte? ProductKindId { get; set; }
        protected DocDirection DocDirection { get; set; }
        protected bool IsMovementForOrder { get; set; }
        protected OrderType OrderType { get; set; }
        protected Guid? PlaceZoneId { get; set; }
        public bool IsRefreshQuantity = false;
        protected decimal Quantity { get; set; }
        protected Guid ProductId { get; set; }

        private RefreshDocProductDelegate RefreshDocOrder;
        private RefreshPalletItemsDelegate RefreshPalletItems;

        private BindingList<ProductBase> AcceptedProducts { get; set; }
        //private BindingSource BSource { get; set; }

        private delegate void UpdateGridInvoker(ProductBase t);


        protected virtual BindingList<ProductBase> GetProducts()
        {
            return (BindingList<ProductBase>)null; 
        }

        protected virtual DbOperationProductResult RemovalProduct(Guid scanId)
        {
            return RemovalProduct(scanId, null, null, null, null, null, null);
        }

        protected virtual DbOperationProductResult RemovalProduct(Guid scanId, DateTime? dateBeg, DateTime? dateEnd, int? outPlaceID, Guid? outPlaceZoneID, int? inPlaceID, Guid? inPlaceZoneID)
        {
            return (DbOperationProductResult)null; 
        }

        protected virtual DialogResult GetDialogResult(string number, string place) 
        {
            return DialogResult.Cancel; 
        }
        
        private bool RefreshDatGrid()
        {
            BindingList<ProductBase> list = GetProducts();
            if (Shared.LastQueryCompleted == false)//|| list == null)
            {
                if (AcceptedProducts == null)
                    AcceptedProducts = new BindingList<ProductBase>();
                return false;
            }
            AcceptedProducts = list ?? new BindingList<ProductBase>();
            gridProducts.DataSource = AcceptedProducts;

            var tableStyle = new DataGridTableStyle { MappingName = gridProducts.DataSource.GetType().Name };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Number",
                HeaderText = @"Дата\Номер",
                Width = 110
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Quantity",
                HeaderText = @"Кол\Обрыв",
                Width = 33,
                Format = "0.###"
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "OutPlace",
                HeaderText = @"Откуда",
                Width = 77,
                NullText = ""
                //Format = "0.#"
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "InPlace",
                HeaderText = @"Куда",
                Width = 77,
                NullText = ""
                //Format = "0.#"
            });
            gridProducts.TableStyles.Clear();
            gridProducts.TableStyles.Add(tableStyle);

            gridProducts.UnselectAll();

            return true;
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)Images.Remove });//, pnlToolBar_ButtonClick);
        }

        protected override void RemoveToolBarButton() 
        {
            DeleteMovementProduct();
        }

        private void DeleteMovementProduct()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            var rowIndex = gridProducts.CurrentRowIndex;
            if (rowIndex >= 0)
            {
                if (AcceptedProducts[rowIndex].MovementId == null) //((DataTable)gridProducts.DataSource).Rows[rowIndex]["MovementID"] == null)
                {
                    Shared.ShowMessageError("Ошибка при удалении.");
                }
                else
                {
                    var t = AcceptedProducts[rowIndex];//((DataTable)gridProducts.DataSource).Rows[rowIndex];
                    if (t != null)
                    {
                        Invoke((UpdateGridInvoker)(CancelLastMovement),
                           new object[] { t });
                            return;
                        
                    }
                }
            }
        }

        private void CancelLastMovement(ProductBase t)
        {

            var dialogResult = GetDialogResult(t.Number,t.OutPlace);
            if (dialogResult == DialogResult.Yes)
            {
                var scanId = t.MovementId;
                DbOperationProductResult delResult = null;
                delResult = t.ProductKind == ProductKind.ProductBale ? RemovalProduct(scanId, t.Date, t.DateEnd, t.OutPlaceID, t.OutPlaceZoneID, t.InPlaceID, t.InPlaceZoneID) : RemovalProduct(scanId);

                if (delResult == null)
                    Shared.ShowMessageError(@"Связь с сервером потеряна, не удалось отменить операцию.");
                else
                    if (string.IsNullOrEmpty(delResult.ResultMessage))
                    {
                        Shared.ScannedBarcodes.ClearLastBarcode();
                        Shared.ScannedBarcodes.DeletedScan(scanId);
                        AcceptedProducts.Remove(t);
                        IsRefreshQuantity = true;
                    }
                    else
                    {
                        Shared.ShowMessageError(@"Не удалось отменить операцию. " + delResult.ResultMessage);
                    }
            }
        }

        protected override void ActionByBarcode(string barcode)
        {
            int rowIndex = -1;
            var count = AcceptedProducts.Count;
            bool isFound = false;
            int i = 0;
            while (!isFound && i < count)
                {
                    var item = (AcceptedProducts[i] as ProductBase);
                    if (item.Barcode == barcode)
                    {
                        isFound = true;
                        rowIndex = i;
                    }
                    i++;
                }
            
            
            if (rowIndex < 0)
                Shared.ShowMessageError("Перемещение по ШК " + barcode + " не найдено!");
            else
            {
                var t = AcceptedProducts[rowIndex];
                if (t != null)
                {
                    Invoke((UpdateGridInvoker)(CancelLastMovement),
                               new object[]
                                   {t});
                    return;

                } 
                return;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ParentForm is BaseFormWithProducts)
            {
                var parentForm = (ParentForm as BaseFormWithProducts);
                if ((DocDirection == DocDirection.DocOut || DocDirection == DocDirection.DocOutIn) && (parentForm.StartPointInfo == null || (parentForm.StartPointInfo.IsAvailabilityPlaceZoneId && !parentForm.StartPointInfo.IsSettedDefaultPlaceZoneId)))
                {
                    //GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(StartPointInfo, EndPointInfo, this, NomenclatureId, CharacteristicId, QualityId, ProductKindId, qmuQuantity.MeasureUnit, qmuQuantity.Value, null);//, barcode, fromBuffer, getProductResult, this);
                    GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(this, new NomenclatureCharacteristicQuantityDialogParameter() { StartPointInfo = StartPointInfo, EndPointInfo = EndPointInfo, IsFilteringOnNomenclature = true, IsFilteringOnEndpoint = (StartPointInfo != null && StartPointInfo.PlaceId > 0), NomenclatureId = NomenclatureId, CharacteristicId = CharacteristicId, QualityId = QualityId, ProductKindId = ProductKindId, MeasureUnit = qmuQuantity.MeasureUnit, Quantity = qmuQuantity.Value, QuantityFractional = qmuQuantity.ValueFractional < 10 ? qmuQuantity.ValueFractional * 100 : qmuQuantity.ValueFractional < 100 ? qmuQuantity.ValueFractional * 10 : qmuQuantity.ValueFractional });//, barcode, fromBuffer, getProductResult, this);
                    GetNomenclatureCharacteristicQuantityForm.Show();
                    Param1 = new AddProductReceivedEventHandlerParameter() { barcode = "barcode", endPointInfo = EndPointInfo, fromBuffer = false };
                    //if (base.ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity == null)
                    ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity += this.ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct;
                }
                else if ((DocDirection == DocDirection.DocIn || DocDirection == DocDirection.DocOutIn) && (parentForm.EndPointInfo == null || (parentForm.EndPointInfo.IsAvailabilityPlaceZoneId && !parentForm.EndPointInfo.IsSettedDefaultPlaceZoneId)))
                {
                    base.ChooseEndPoint(this.ChoosePlaceZoneBarcodeReactionInAddProduct, new AddProductReceivedEventHandlerParameter() { barcode = "barcode", endPointInfo = EndPointInfo, fromBuffer = false, getProductResult = new DbProductIdFromBarcodeResult() { ProductKindId = (ProductKind)ProductKindId, NomenclatureId = NomenclatureId, CharacteristicId = CharacteristicId, MeasureUnitId = qmuQuantity.MeasureUnit.MeasureUnitID, QualityId = QualityId, CountProducts = qmuQuantity.Value, CountFractionalProducts = qmuQuantity.ValueFractional < 10 ? qmuQuantity.ValueFractional * 100 : qmuQuantity.ValueFractional < 100 ? qmuQuantity.ValueFractional * 10 : qmuQuantity.ValueFractional, FromPlaceId = StartPointInfo == null ? (int?)null : StartPointInfo.PlaceId, FromPlaceZoneId = StartPointInfo == null ? (Guid?)null : StartPointInfo.PlaceZoneId } });
                }
                else if (!(((DocDirection == DocDirection.DocOut || DocDirection == DocDirection.DocOutIn) && parentForm.StartPointInfo == null) || ((DocDirection == DocDirection.DocIn || DocDirection == DocDirection.DocOutIn) && parentForm.EndPointInfo == null)))
                {
                    AddProductByBarcode("barcode", EndPointInfo, false, new DbProductIdFromBarcodeResult() { ProductKindId = (ProductKind)ProductKindId, NomenclatureId = NomenclatureId, CharacteristicId = CharacteristicId, MeasureUnitId = qmuQuantity.MeasureUnit.MeasureUnitID, QualityId = QualityId, CountProducts = qmuQuantity.Value, CountFractionalProducts = qmuQuantity.ValueFractional < 10 ? qmuQuantity.ValueFractional * 100 : qmuQuantity.ValueFractional < 100 ? qmuQuantity.ValueFractional * 10 : qmuQuantity.ValueFractional, FromPlaceId = StartPointInfo == null ? (int?)null : StartPointInfo.PlaceId, FromPlaceZoneId = StartPointInfo == null ? null : StartPointInfo.PlaceZoneId });
                }
            }
        }

        protected override bool CheckIsCreatePalletMovementFromBarcodeScan()
        {
            return true;
        }

        protected override string CheckUnloadOfflineProduct(ScannedBarcode offlineProduct)
        {
            string ret = "";
            if (offlineProduct.DocId == null)
            {
                ret = @"Ошибка! Не указан документ, куда выгрузить продукт " + offlineProduct.Barcode;
            }
            return ret;
        }

        protected override void UpdateGrid(DbOperationProductResult addResult, ProductKind? productKindId, Guid? docOrderId, EndPointInfo endPointInfo, Guid? scanId, EndPointInfo startPointInfo)
        {
            if (DocId == docOrderId)
            {
                ProductBase t = new ProductBase() { Number = DateTime.Now.ToString(), Barcode = "", Date = DateTime.Now, InPlace = endPointInfo.PlaceName + Environment.NewLine + endPointInfo.PlaceZoneName, IsProductR = true, MovementId = scanId ?? Guid.Empty, OutPlace = addResult.OutPlace + Environment.NewLine + addResult.OutPlaceZone, Quantity = Convert.ToDouble(addResult.Product.Quantity) - Math.Floor(Convert.ToDouble(addResult.Product.Quantity)) != 0 ? addResult.Product.Quantity.ToString() : Convert.ToInt32(addResult.Product.Quantity).ToString(), ProductKind = productKindId, OutPlaceID = startPointInfo != null ? (int?)startPointInfo.PlaceId : (int?)null, OutPlaceZoneID = startPointInfo != null ? startPointInfo.PlaceZoneId : null, InPlaceID = endPointInfo.PlaceId, InPlaceZoneID = endPointInfo.PlaceZoneId };
                AcceptedProducts.Insert(0,t);
                IsRefreshQuantity = true;
            }
        }

        protected override List<Nomenclature> GetNomenclatureGoods()
        {
            return new List<Nomenclature>();
        }
            
    }
}