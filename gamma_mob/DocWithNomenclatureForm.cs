using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Drawing;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using gamma_mob.Dialogs;
using gamma_mob.CustomDataGrid;

namespace gamma_mob
{
    [DesignerCategory(@"Form")]
    [DesignTimeVisible(false)]
    public partial class DocWithNomenclatureForm : BaseFormWithProducts
    {
        private DocWithNomenclatureForm()
        {
            InitializeComponent();
        }

        private DocWithNomenclatureForm(Form parentForm):this()
        {
            ParentForm = parentForm;
        }

        /// <summary>
        ///     инициализация формы
        /// </summary>
        /// <param name="docOrderId">ID Документа (для отгрузки - ID 1c, для заказа на перемещение ID gamma)</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="orderNumber">Номер приказа из 1С</param>
        /// <param name="orderType">Тип документа(приказ 1с, перемещение)</param>
        /// <param name="fileName">Имя файла для хранения информации о невыгруженных продуктах</param>
        /// <param name="docDirection">Направление движения продукции(in, out, outin)</param>
        public DocWithNomenclatureForm(Guid docOrderId, Form parentForm, string orderNumber, OrderType orderType,
            DocDirection docDirection, bool isMovementForOrder, int maxAllowedPercentBreak, EndPointInfo startPointInfo, bool isControlExec, DateTime? startExec, bool checkExistMovementToZone)
            : this(parentForm)
        {
            StartPointInfo = startPointInfo;
            OrderType = orderType;
            DocId = docOrderId;
            DocDirection = docDirection;
            IsMovementForOrder = isMovementForOrder;
            SetCheckExistMovementToZone(checkExistMovementToZone);
            if (!RefreshDocOrderGoods(docOrderId))
            {
                Shared.ShowMessageInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + "Попробуйте ещё раз обновить!");
                //Close();
                //return;
            }
            switch (orderType)
            {
                case OrderType.ShipmentOrder:
                    Text = "Приказ № " + orderNumber;
                    IsShowOrderComplitedMessage = !(StartPointInfo == null || StartPointInfo.PlaceId == 36 || StartPointInfo.PlaceId == 37 || StartPointInfo.PlaceId == 38);
                    break;
                case OrderType.InternalOrder:
                    Text = "Внутрен. заказ № " + orderNumber;
                    IsShowOrderComplitedMessage = !(StartPointInfo == null || StartPointInfo.PlaceId == 36 || StartPointInfo.PlaceId == 37 || StartPointInfo.PlaceId == 38);
                    break;
                case OrderType.MovementOrder:
                    Text = "Заказ на перем.№ " + orderNumber;
                    IsShowOrderComplitedMessage = !(StartPointInfo == null || StartPointInfo.PlaceId == 36 || StartPointInfo.PlaceId == 37 || StartPointInfo.PlaceId == 38);
                    break;
                case OrderType.Purchase:
                    Text = "Приемка № " + orderNumber;
                    IsShowOrderComplitedMessage = false;
                    break;
            }
            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номенклатура",
                    MappingName = "ShortNomenclatureName",
                    Width = 135
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Кол-во",
                    MappingName = "Quantity",
                    Width = 37
                });
            /*
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Собрано",
                    MappingName = "CollectedQuantity",
                    Width = 38,
                    Format = "0.###"
                });
             */
            FormattableTextBoxColumn CollectedQuantityComputedColumn = new FormattableTextBoxColumn
            {
                HeaderText = "Собр",
                MappingName = "CollectedQuantityComputedColumn",
                Width = 37
            };
            tableStyle.GridColumnStyles.Add(CollectedQuantityComputedColumn);
            CollectedQuantityComputedColumn.SetCellFormat += new FormatCellEventHandler(ColumnSetCellFormat);
            /*tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "Собр",
                MappingName = "CollectedQuantityComputedColumn",
                Width = 37
            });*/
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = "% обр",
                MappingName = "SpoolWithBreakPercentColumn",
                Width = 30,
                Format = "0.#",
                NullText = ""
            });
            gridDocOrder.TableStyles.Add(tableStyle);

            MaxAllowedPercentBreak = maxAllowedPercentBreak;

            IsControlExec = isControlExec;
            StartExec = startExec;
            
            if (!Settings.GetCurrentServerIsExternal() && !Shared.InitializationData()) Shared.ShowMessageInformation(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
            if (Shared.TimerForUnloadOfflineProducts == null) Shared.ShowMessageInformation(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
            OnUpdateBarcodesIsNotUploaded();
        }

        public DocWithNomenclatureForm(Guid docOrderId, Form parentForm, string orderNumber, OrderType orderType,
            DocDirection docDirection, bool isMovementForOrder, int maxAllowedPercentBreak, EndPointInfo startPointInfo, EndPointInfo endPointInfo, bool isControlExec, DateTime? startExec, bool checkExistMovementToZone)
            : this(docOrderId, parentForm, orderNumber, orderType,
            docDirection, isMovementForOrder, maxAllowedPercentBreak, startPointInfo, isControlExec, startExec, checkExistMovementToZone) 
        {
            EndPointInfo = endPointInfo;
            EndpointSettedInFormConstructor(EndPointInfo, pnlZone);
            Shared.SaveToLogInformation(StartPointInfo != null ? @"StartPointInfo.PlaceId-" + StartPointInfo.PlaceId + @"; StartPointInfo.PlaceZoneId-" + StartPointInfo.PlaceZoneId : @"" + EndPointInfo != null ? @"; EndPointInfo.PlaceId-" + EndPointInfo.PlaceId + @"; EndPointInfo.PlaceZoneId-" + EndPointInfo.PlaceZoneId : @"");
        }

        // Устанавливаем цвет фона для ячейки Собрано при превышении собранного количества над требуемым!
        private void ColumnSetCellFormat(object sender, DataGridFormatCellEventArgs e)
        {
            if ((e.Source.List[e.Row] as DocNomenclatureItem).IsPercentCollectedExcess)
                e.BackBrush = new SolidBrush(Color.Red);
        }

        private OrderType OrderType { get; set; }

        private BindingSource BSource { get; set; }
        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        private int MaxAllowedPercentBreak { get; set; }
        public bool IsRefreshQuantity = false;
        public bool IsShowOrderComplitedMessage = false;

        private int _countNomenclatureExceedingMaxPercentWithBreak;

        private int CountNomenclatureExceedingMaxPercentWithBreak
        {
            get { return _countNomenclatureExceedingMaxPercentWithBreak; }
            set
            {
                _countNomenclatureExceedingMaxPercentWithBreak = value;
                base.SetPercentBreak((CountNomenclatureExceedingMaxPercentWithBreak > 0) ? "% обрыва превышен" : "% обрыва в норме");//(100 * CountProductSpoolsWithBreak / CountProductSpools).ToString(CultureInfo.InvariantCulture);
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            if (OrderType != OrderType.Purchase)
                base.ActivatePanels(new List<int>() { (int)Images.Back, (int)Images.Inspect, (int)Images.Refresh, (int)Images.UploadToDb, (int)Images.Pallet, (int)Images.Question, (int)Images.InfoProduct});//, pnlToolBar_ButtonClick);
            else
                base.ActivatePanelsWithoutSearch(new List<int>() { (int)Images.Back, (int)Images.Inspect, (int)Images.Refresh, (int)Images.UploadToDb, (int)Images.Pallet, (int)Images.Question, (int)Images.InfoProduct});//, pnlToolBar_ButtonClick);
        }

        protected override void RefreshToolBarButton()
        {
            RefreshDocOrder(DocId, true);
        }

        protected override void QuestionToolBarButton()
        {
            if (!ConnectionState.CheckConnection())
                    {
                        Shared.ShowMessageError(@"Нет связи с базой" + Environment.NewLine + ConnectionState.GetConnectionState());
                        return;
                    }
                    var nomenclatureItem = NomenclatureList[gridDocOrder.CurrentRowIndex];
                    var resultMessage = Db.FindDocOrderNomenclatureStoragePlaces(DocId, nomenclatureItem.NomenclatureId, nomenclatureItem.CharacteristicId, nomenclatureItem.QualityId)
                                        ?? "Не удалось получить информацию о расположении продукции";
                    if (resultMessage != null)
                        Shared.ShowMessageInformation(resultMessage);
        }

        protected override void PalletToolBarButton() 
        {
            var form = new PalletsForm(this, DocId, DocDirection);
            if (!form.IsDisposed)
            {
                BarcodeFunc = null;
                DialogResult resultForm = form.ShowDialog();
                /*form.Show();
                if (form.Enabled)
                {
                    BarcodeFunc = null;
                    Hide();
                }*/
            }
        }

        protected override void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            if (!(gridDocOrder.CurrentRowIndex>=0))
            {
                Shared.ShowMessageError(@"Не выбрана запись");
                return;
            }
            var good = NomenclatureList[gridDocOrder.CurrentRowIndex];
            var form = new DocShipmentProductsForm(DocId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, good.ProductKindId, this, DocDirection, IsMovementForOrder, OrderType, new RefreshDocProductDelegate(RefreshDocOrder), StartPointInfo, EndPointInfo, good.IsEnableAddProductManual);
            if (!form.IsDisposed)
            {
                BarcodeFunc = null;
                DialogResult result = form.ShowDialog();
                if (form.IsRefreshQuantity)
                    RefreshDocOrder(DocId, true);
            }
        }

        private void RefreshDocOrder(Guid docId, bool showMessage)
        {
            if (!RefreshDocOrderGoods(docId))
            {
                if (showMessage)
                {
                    //ConnectionState.ConnectionLost();
                    Shared.ShowMessageInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + "Попробуйте ещё раз обновить!");
                }
                else
                    Shared.SaveToLogInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + "Попробуйте ещё раз обновить!");
                //Close();
                return;
            }
        }

        private bool RefreshDocOrderGoods(Guid docId)
        {
            BindingList<DocNomenclatureItem> list = Db.DocNomenclatureItems(docId, OrderType, DocDirection, IsMovementForOrder);
            if (Shared.LastQueryCompleted == false)// || list == null)
            {
               // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                if (NomenclatureList == null)
                    NomenclatureList = new BindingList<DocNomenclatureItem>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = NomenclatureList };
                return false;
            }
            NomenclatureList = list ?? new BindingList<DocNomenclatureItem>();
            if (BSource == null)
                BSource = new BindingSource {DataSource = NomenclatureList};
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridDocOrder.DataSource = BSource;
            gridDocOrder.UnselectAll();
            //Barcodes = Db.GetCurrentBarcodes(DocOrderId, DocDirection);
            Collected = 0;
            CountNomenclatureExceedingMaxPercentWithBreak = 0;
            for (var i = 0; i < NomenclatureList.Count; i++)
            {
                Collected += NomenclatureList[i].CollectedQuantityUnits;
                CountNomenclatureExceedingMaxPercentWithBreak += (NomenclatureList[i].SpoolWithBreakPercentColumn > Convert.ToDecimal(MaxAllowedPercentBreak)) ? 1 : 0;
            }
            
            return true;
        }

        protected override DbOperationProductResult AddProductId(Guid? scanId, DbProductIdFromBarcodeResult getProductResult, EndPointInfo endPointInfo)
        {
            var addedProductIdToOrderResult = Db.AddProductIdToOrder(scanId, DocId, OrderType, Shared.PersonId, getProductResult.ProductId, DocDirection, endPointInfo, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts, getProductResult.CountFractionalProducts, getProductResult.MeasureUnitId, getProductResult.FromProductId, getProductResult.FromPlaceId, getProductResult.FromPlaceZoneId, getProductResult.NewWeight, getProductResult.ValidUntilDate);
            return addedProductIdToOrderResult == null ? null : (addedProductIdToOrderResult as DbOperationProductResult);
        }

        protected override void UpdateGrid(DbOperationProductResult addResult, ProductKind? productKindId, Guid? docOrderId, EndPointInfo endPointInfo, Guid? scanId, EndPointInfo startPointInfo)
        {
            if (DocId == docOrderId)
            {
                Invoke((UpdateOrderGridInvoker)(UpdateGrid),
                   new object[] { addResult.Product.NomenclatureId, addResult.Product.CharacteristicId, addResult.Product.QualityId, addResult.Product.NomenclatureName, 
                                addResult.Product.ShortNomenclatureName, addResult.Product.Quantity, true, addResult.Product.CountProductSpools, addResult.Product.CountProductSpoolsWithBreak, (int?)productKindId });
            }
        }
     
        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, bool add, int countProductSpools, int countProductSpoolsWithBreak, int? productKindId)
        {
            DocNomenclatureItem good = null;
            string error_ch = "";

            try
            {
                good = NomenclatureList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId);
                error_ch = "ch1";
                if (good == null)
                {
                    good = new DocNomenclatureItem
                        {
                            NomenclatureId = nomenclatureId,
                            CharacteristicId = characteristicId,
                            QualityId = qualityId,
                            NomenclatureName = nomenclatureName,
                            ShortNomenclatureName = shortNomenclatureName,
                            CollectedQuantity = 0,
                            Quantity = "0",
                            CountProductSpools = 0,
                            CountProductSpoolsWithBreak = 0,
                            CoefficientPackage = null,
                            CoefficientPallet = null
                        };
                    NomenclatureList.Add(good);
                    BSource.DataSource = NomenclatureList;
                }
                
                if (add)
                {
                    error_ch = "ch2";
                    good.CountProductSpools += countProductSpools;
                    good.CountProductSpoolsWithBreak += countProductSpoolsWithBreak;
                    good.CollectedQuantity += quantity;
                    if (productKindId == null || productKindId != 3) Collected++;
                }
                else
                {
                    error_ch = "ch3";
                    good.CountProductSpools -= countProductSpools;
                    good.CountProductSpoolsWithBreak -= countProductSpoolsWithBreak;
                    good.CollectedQuantity -= quantity;
                    if (productKindId == null || productKindId != 3) Collected--;
                }
                error_ch = "ch4";
                if (IsShowOrderComplitedMessage && NomenclatureList.Count > 0 && !NomenclatureList.Any(n => !n.IsCollected))
                {
                    Shared.ShowMessageInformation("Собраны все позиции. Не забудьте закрыть заказ.");
                }
                CountNomenclatureExceedingMaxPercentWithBreak = 0;
                error_ch = "ch5";
                foreach (DocNomenclatureItem item in NomenclatureList)
                {
                    CountNomenclatureExceedingMaxPercentWithBreak += (item.SpoolWithBreakPercentColumn > Convert.ToDecimal(MaxAllowedPercentBreak)) ? 1 : 0;
                }
                error_ch = "ch6";
                gridDocOrder.UnselectAll();
                error_ch = "ch7";
                int index = NomenclatureList.IndexOf(good);
                error_ch = "ch8";
                if (index > 0)
                {
                    gridDocOrder.CurrentRowIndex = index;
                    gridDocOrder.Select(index);
                }
                error_ch = "ch9";
            }
            catch
            {
                Shared.ShowMessageError(@"Ошибка при обновлении списка. Нажмите Ок для повтора. (" + error_ch + @")");
                RefreshDocOrder(DocId, true);
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

        private void gridDocOrder_CurrentCellChanged(object sender, EventArgs e)
        {
            //gridDocOrder.Select(gridDocOrder.CurrentRowIndex);
        }

        
        private void gridDocOrder_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        protected override List<Nomenclature> GetNomenclatureGoods()
        {
            return NomenclatureList.Select(n => new Nomenclature(n.NomenclatureId, n.CharacteristicId, n.QualityId)).Distinct().ToList();   
        }

        protected override void btnAddProductClick()
        {
            Shared.SaveToLogInformation(@"Начало выбора зоны откуда");
            var res = false;
            if (edtNumber.Text.Length == 14 && (edtNumber.Text.Substring(0, 3) == "202" || edtNumber.Text.Substring(0, 3) == "203"))
            {
                if (StartPointInfo != null && !StartPointInfo.IsAvailabilityPlaceZoneId)
                    res = true;
                else
                {
                    if (this.InvokeRequired)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            res = this.ChangeStartPointZone_Click(new object(), new EventArgs());
                        });
                    }
                    else
                        res = this.ChangeStartPointZone_Click(new object(), new EventArgs());
                }

                if (res && StartPointInfo != null)
                {
                    if (edtNumber.Text == String.Empty && StartPointInfo.PlaceZoneBarcode != "")
                        Invoke((MethodInvoker)(() => edtNumber.Text = StartPointInfo.PlaceZoneBarcode));
                    AddProductClick();
                    Shared.SaveToLogInformation(@"Выбрано Добавить ШК: " + edtNumber.Text);
                }
                else
                {
                    Shared.ShowMessageError(@"Ошибка! Не выбрана зона Откуда");
                }
            }
            else
                AddProductClick();
            
        }

    }
}