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
    public partial class DocWithNomenclatureForm : BaseForm
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
            DocDirection docDirection, int maxAllowedPercentBreak)
            : this(parentForm)
        {
            OrderType = orderType;
            DocOrderId = docOrderId;
            DocDirection = docDirection;
            
            if (!RefreshDocOrderGoods(docOrderId))
            {
                MessageBox.Show(@"Не удалось получить информацию о документе!/r/nПопробуйте ещё раз обновить!");
                //Close();
                return;
            }
            switch (orderType)
            {
                case OrderType.ShipmentOrder:
                    Text = "Приказ № " + orderNumber;
                    break;
                case OrderType.InternalOrder:
                    Text = "Внутрен. заказ № " + orderNumber;
                    break;
                case OrderType.MovementOrder:
                    Text = "Заказ на перем.№ " + orderNumber;
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

            if (!Shared.InitializationData()) MessageBox.Show(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
            if (Shared.TimerForUnloadOfflineProducts == null) MessageBox.Show(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
            OnUpdateBarcodesIsNotUploaded();
        }

        // Устанавливаем цвет фона для ячейки Собрано при превышении собранного количества над требуемым!
        private void ColumnSetCellFormat(object sender, DataGridFormatCellEventArgs e)
        {
            if ((e.Source.List[e.Row] as DocNomenclatureItem).IsPercentCollectedExcess)
                e.BackBrush = new SolidBrush(Color.Red);
        }

        private OrderType OrderType { get; set; }
        private DocDirection DocDirection { get; set; }

        private BindingSource BSource { get; set; }
        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        //private List<OfflineProduct> OfflineProducts { get; set; }
        //private List<Barcodes1C> OfflineBarcodes1C { get; set; }
        
        private int MaxAllowedPercentBreak { get; set; }
        public bool IsRefreshQuantity = false;

        /// <summary>
        ///     ID документа, по которому идет работа (основание)
        /// </summary>
        private Guid DocOrderId { get; set; }

        private int _collected;

        private int Collected
        {
            get { return _collected; }
            set 
            { 
                _collected = value;
                Invoke(
                    (MethodInvoker)
                    (() => lblCollected.Text = Collected.ToString(CultureInfo.InvariantCulture)));
            }
        }
        private int _countNomenclatureExceedingMaxPercentWithBreak;

        private int CountNomenclatureExceedingMaxPercentWithBreak
        {
            get { return _countNomenclatureExceedingMaxPercentWithBreak; }
            set
            {
                _countNomenclatureExceedingMaxPercentWithBreak = value;
                Invoke(
                    (MethodInvoker)
                    (() => lblPercentBreak.Text = (CountNomenclatureExceedingMaxPercentWithBreak > 0) ? "% обрыва превышен" : "% обрыва в норме"));//(100 * CountProductSpoolsWithBreak / CountProductSpools).ToString(CultureInfo.InvariantCulture);
            }
        }

        private void ShowConnection(ConnectState conState)
        {
            switch (conState)
            {
                case ConnectState.ConInProgress:
                    imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                    break;
                case ConnectState.NoConInProgress:
                    imgConnection.Image = null;
                    break;
                case ConnectState.NoConnection:
                    imgConnection.Image = ImgList.Images[(int)Images.NetworkOffline];
                    break;
                case ConnectState.ConnectionRestore:
                    imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnInspect.ImageIndex = (int)Images.Inspect;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnUpload.ImageIndex = (int)Images.UploadToDb;
            btnPallets.ImageIndex = (int)Images.Pallet;
            btnQuestionNomenclature.ImageIndex = (int)Images.Question;
            btnInfoProduct.ImageIndex = (int)Images.InfoProduct;
            BarcodeFunc = BarcodeReaction;

            //Подписка на событие восстановления связи
            ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            //Подписка на событие потери связи
            ConnectionState.OnConnectionLost += ConnectionLost;

            //Подписка на событие +1 не выгружено (ошибка при сохранении в БД остканированной продукции)
            ScannedBarcodes.OnUpdateBarcodesIsNotUploaded += OnUpdateBarcodesIsNotUploaded;

            //Подписка на событие Выгрузить невыгруженную продукцию
            ScannedBarcodes.OnUnloadOfflineProducts += UnloadOfflineProducts;

        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection).Count > 0)
                MessageBox.Show("Есть невыгруженные продукты!" + Environment.NewLine + "Сначала выгрузите в базу в зоне связи!");
            base.OnFormClosing(sender, e);
            ConnectionState.OnConnectionRestored -= ConnectionRestored;
            ConnectionState.OnConnectionLost -= ConnectionLost;
            ScannedBarcodes.OnUpdateBarcodesIsNotUploaded -= OnUpdateBarcodesIsNotUploaded;
            ScannedBarcodes.OnUnloadOfflineProducts -= UnloadOfflineProducts;
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    OpenDetails();
                    break;
                case 2:
                    RefreshDocOrder(DocOrderId,true);
                    break;
                case 3:
                    //if (OfflineProducts.Count(p => p.DocId == DocOrderId) > 0)
                        UnloadOfflineProducts();
                    break;
                case 4:
                    if (!ConnectionState.CheckConnection())
                    {
                        MessageBox.Show(@"Нет связи с базой" + Environment.NewLine + ConnectionState.GetConnectionState(), @"Ошибка связи");
                        return;
                    }
                    var nomenclatureItem = NomenclatureList[gridDocOrder.CurrentRowIndex];
                    var resultMessage = Db.FindDocOrderNomenclatureStoragePlaces(DocOrderId, nomenclatureItem.NomenclatureId, nomenclatureItem.CharacteristicId, nomenclatureItem.QualityId)
                                        ?? "Не удалось получить информацию о расположении продукции";
                    if (resultMessage != null)
                        MessageBox.Show(resultMessage, @"Расположение продукции", MessageBoxButtons.OK,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1);
                    break;
                case 5:
                    var form = new PalletsForm(this, DocOrderId);
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
                    BarcodeFunc = BarcodeReaction;
                    break;
                case 6:
                    var InfoProduct = new InfoProductForm(this);
                    BarcodeFunc = null;
                    DialogResult result = InfoProduct.ShowDialog();
                    Invoke((MethodInvoker)Activate);
                    BarcodeFunc = BarcodeReaction;
                    break;
            }
        }

        private void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            var good = NomenclatureList[gridDocOrder.CurrentRowIndex];
            //var form = new DocShipmentGoodProductsForm(DocOrderId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this, DocDirection, new RefreshDocOrderDelegate(RefreshDocOrder));
            var form = new DocShipmentProductsForm(DocOrderId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this, DocDirection, new RefreshDocProductDelegate(RefreshDocOrder));
            if (!form.IsDisposed)
            {
                BarcodeFunc = null;
                DialogResult result = form.ShowDialog();
                if (form.IsRefreshQuantity)
                    RefreshDocOrder(DocOrderId, true);
                BarcodeFunc = BarcodeReaction;
            }
        }

        private void RefreshDocOrder(Guid docId, bool showMessage)
        {
            if (!RefreshDocOrderGoods(docId))
            {
                if (showMessage) MessageBox.Show(@"Не удалось получить информацию о документе!/r/nПопробуйте ещё раз обновить!");
                //Close();
                return;
            }
        }

        private bool RefreshDocOrderGoods(Guid docId)
        {
            BindingList<DocNomenclatureItem> list = Db.DocNomenclatureItems(docId, OrderType, DocDirection);
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

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker) (() => edtNumber.Text = barcode));
            //UIServices.SetBusyState(this);
            if (this.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate()
                {
                    btnAddProduct_Click(new object(), new EventArgs());
                });
            }
            else
                btnAddProduct_Click(new object(), new EventArgs());
            //AddProductByBarcode(barcode, false);
            //UIServices.SetNormalState(this);
        }

        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            if (barcode.Length < Shared.MinLengthProductBarcode)
            {
                Shared.SaveToLog(@"Ошибочный ШК! " + barcode);
                MessageBox.Show(@"Ошибка при сканировании ШК " + barcode + Environment.NewLine + @"Штрих-код должен быть длиннее 12 символов", @"Продукция не найдена",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (!fromBuffer && Shared.ScannedBarcodes.CheckIsLastBarcode(barcode, DocDirection, DocOrderId, null, null))
                {
                    Shared.SaveToLog(@"Вы уже сканировали этот шк " + barcode);
                    MessageBox.Show(@"Вы уже сканировали этот шк " + barcode, @"Повтор",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                    Cursor.Current = Cursors.Default;

                    if (getProductResult == null || getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                    {
                        Shared.SaveToLog(@"Продукция не найдена по ШК! " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                        MessageBox.Show(@"Продукция не найдена по ШК!", @"Продукция не найдена",
                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                    }
                    else
                    {
                        if (getProductResult.ProductKindId == 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty))
                        {
                            if (getProductResult.NomenclatureId == null || getProductResult.NomenclatureId == Guid.Empty || getProductResult.CharacteristicId == null || getProductResult.CharacteristicId == Guid.Empty || getProductResult.QualityId == null || getProductResult.QualityId == Guid.Empty)
                            {
                                using (var form = new ChooseNomenclatureCharacteristicDialog(barcode))
                                {
                                    DialogResult result = form.ShowDialog();
                                    Invoke((MethodInvoker)Activate);
                                    if (result != DialogResult.OK || form.NomenclatureId == null || form.CharacteristicId == null || form.QualityId == null)
                                    {
                                        MessageBox.Show(@"Не выбран продукт. Продукт не добавлен!", @"Продукт не добавлен",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                                        return;
                                    }
                                    else
                                    {
                                        getProductResult.NomenclatureId = form.NomenclatureId;
                                        getProductResult.CharacteristicId = form.CharacteristicId;
                                        getProductResult.QualityId = form.QualityId;
                                    }
                                }
                            }
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
                                    getProductResult.CountProducts = form.Quantity;
                                }
                            }

                        }
                        AddProductByBarcode(barcode, fromBuffer, getProductResult);
                    }
                }
            }
        }

        private void AddProductByBarcode(string barcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            var scanId = Shared.ScannedBarcodes.AddScannedBarcode(barcode, new EndPointInfo(), DocDirection, DocOrderId, getProductResult);
            if (scanId == null || scanId == Guid.Empty)
                MessageBox.Show("Ошибка1 при сохранении отсканированного штрих-кода");
            
            AddProductByBarcode(scanId, barcode, fromBuffer, getProductResult, DocOrderId);
        }

        private bool? AddProductByBarcode(Guid? scanId, string barcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult, Guid docOrderId)
        {
            Shared.SaveToLog(@"AddShip " + barcode + @"; Q-" + getProductResult.CountProducts + @"; F-" + fromBuffer.ToString());
            Cursor.Current = Cursors.WaitCursor;
            
            var addResult = Db.AddProductIdToOrder(scanId, DocOrderId, OrderType, Shared.PersonId, getProductResult.ProductId, DocDirection, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (Shared.LastQueryCompleted == false)
            {
                return null;
            }
            if (addResult == null)
            {
                MessageBox.Show(@"Не удалось добавить продукт" + Environment.NewLine + barcode + " в приказ!");
                Shared.ScannedBarcodes.ClearLastBarcode();
                return false;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                Shared.ScannedBarcodes.UploadedScan(scanId, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                if (docOrderId == DocOrderId)
                    Invoke((UpdateOrderGridInvoker)(UpdateGrid),
                           new object[] { addResult.Product.NomenclatureId, addResult.Product.CharacteristicId, addResult.Product.QualityId, addResult.Product.NomenclatureName, 
                                addResult.Product.ShortNomenclatureName, addResult.Product.Quantity, true, addResult.Product.CountProductSpools, addResult.Product.CountProductSpoolsWithBreak, getProductResult.ProductKindId });                
            }
            else
            {
                Shared.ScannedBarcodes.UploadedScanWithError(scanId, addResult.ResultMessage, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                if (docOrderId == DocOrderId)
                {
                    MessageBox.Show(fromBuffer ? @"Ошибка при загрузке на сервер невыгруженного" + Environment.NewLine + @" продукта: " : @"Продукт: " + barcode + Environment.NewLine + addResult.ResultMessage);
                    Shared.ScannedBarcodes.ClearLastBarcode();
                }
            }
            return true;
        }

        /*private bool DeleteProductByBarcode(string barcode)
        {
            bool result = false;
            DialogResult dlgresult = MessageBox.Show(@"Хотите удалить продукт c шк: " + barcode + @" из приказа?",
                                                     @"Удаление продукта",
                                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                     MessageBoxDefaultButton.Button1);
            if (dlgresult == DialogResult.Yes)
            {
                DbOperationProductResult deleteResult = Db.DeleteProductFromOrder(barcode, DocOrderId, DocDirection);
                if (deleteResult == null) return false;
                if (deleteResult.ResultMessage == "" && Shared.LastQueryCompleted == true)
                {
                    result = true;
                    //Barcodes.Remove(barcode);
                    Barcodes.RemoveAll(b => b.Barcode == barcode);

                    var product = deleteResult.Product;
                    if (product != null)
                    {
                        Invoke((UpdateOrderGridInvoker) (UpdateGrid),
                               new object[]
                                   {
                                       product.NomenclatureId, product.CharacteristicId, product.QualityId, product.NomenclatureName, product.ShortNomenclatureName,
                                            product.Quantity, 
                                       false, product.CountProductSpools, product.CountProductSpoolsWithBreak, null
                                   });
                    }
                }                
            }
            else result = true;
            return result;
        }
        */
        
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
                Shared.SaveToLog("Error DocWithNomenclatureForms " + error_ch);
                MessageBox.Show("Ошибка при обновлении списка. Нажмите Ок для повтора.");
                RefreshDocOrder(DocOrderId, true);
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            //UIServices.SetBusyState(this);
            AddProductByBarcode(edtNumber.Text, false);
            //UIServices.SetNormalState(this);
        }

        private void ConnectionLost()
        {
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
        }

        private void ConnectionRestored()
        {
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore });
            //Invoke(new EventHandler(ConnectionRestored));
        }

        private void OnUpdateBarcodesIsNotUploaded()
        {
            if (Shared.ScannedBarcodes != null && Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection) != null)
                Invoke(
                        (MethodInvoker)
                        (() => lblBufferCount.Text = Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection).Count.ToString(CultureInfo.InvariantCulture)));
            Invoke(
                    (MethodInvoker)
                    (() => Shared.IsExistsUnloadOfflineProducts = !(lblBufferCount.Text == "0")));

        }

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConInProgress});
            foreach (ScannedBarcode offlineProduct in Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection))
            {
                if (offlineProduct.DocId == null)
                {
                    MessageBox.Show(@"Ошибка! Не указан документ инвентаризации, куда выгрузить продукт " + offlineProduct.Barcode, @"Информация о выгрузке");
                    Shared.ScannedBarcodes.UploadedScanWithError(offlineProduct.ScanId, @"Ошибка! Не указан документ инвентаризации, куда выгрузить продукт ", offlineProduct.ProductId);
                }
                else
                {
                    if (AddProductByBarcode(offlineProduct.ScanId, offlineProduct.Barcode, true, new DbProductIdFromBarcodeResult() { ProductId = offlineProduct.ProductId ?? new Guid(), ProductKindId = offlineProduct.ProductKindId, NomenclatureId = offlineProduct.NomenclatureId ?? new Guid(), CharacteristicId = offlineProduct.CharacteristicId ?? new Guid(), QualityId = offlineProduct.QualityId ?? new Guid(), CountProducts = offlineProduct.Quantity ?? 0 }, (Guid)offlineProduct.DocId) == null)
                        break;
                }
            }
            /*
            string message = "";
            string resultMessage = "";
            foreach (
                OfflineProduct offlineProduct in
                    OfflineProducts.Where(p => p.DocId == DocOrderId).ToList())
            {
                if (offlineProduct.NomenclatureId == null || offlineProduct.NomenclatureId == Guid.Empty)
                    AddProductByBarcode(offlineProduct.Barcode, true);
                else
                    AddProductByBarcode(offlineProduct.Barcode, true, offlineProduct.NomenclatureId, offlineProduct.CharacteristicId, offlineProduct.QualityId, (int?)offlineProduct.Quantity);
            }
            List<OfflineProduct> tempList = OfflineProducts.Where(p => p.Unloaded
                                                                       && p.DocId == DocOrderId)
                                                           .ToList();
            int unloaded = tempList.Count(p => p.ResultMessage == "");
            if (unloaded > 0)
                message += @"Выгружено изделий: " + unloaded + Environment.NewLine;
            foreach (
                OfflineProduct offlineProduct in
                    tempList.Where(p => p.ResultMessage != string.Empty).OrderBy(p => p.ResultMessage))
            {
                if (offlineProduct.ResultMessage != resultMessage)
                {
                    resultMessage = offlineProduct.ResultMessage;
                    message += resultMessage + Environment.NewLine;
                }
                message += "шк: " + offlineProduct.Barcode + Environment.NewLine;
            }
            if (message != string.Empty)
                MessageBox.Show(message, @"Информация о выгрузке");
            foreach (OfflineProduct product in tempList)
            {
                OfflineProducts.Remove(product);
            }
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocId == DocOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            if (OfflineProducts.Count(p => p.DocId == DocOrderId) > 0)
            {
                ConnectionState.StartChecker();
            }
            SaveToXml(OfflineProducts,FileName);
             */
            UIServices.SetNormalState(this);
        }

        private void gridDocOrder_CurrentCellChanged(object sender, EventArgs e)
        {
            //gridDocOrder.Select(gridDocOrder.CurrentRowIndex);
        }

        
        private void gridDocOrder_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        private void DocWithNomenclatureForm_Activated(object sender, EventArgs e)
        {
            BarcodeFunc = BarcodeReaction;
        }

        private void lblPercentBreak_TextChanged(object sender, EventArgs e)
        {

            Label label = sender as Label;
            if (label != null)
            {
                if (label.Text == "% обрыва превышен")
                {
                    lblPercentBreak.BackColor = Color.Red;
                    lblPercentBreak.ForeColor = Color.White;
                }
                else
                {
                    lblPercentBreak.BackColor = Color.FromArgb(192,192,192);
                    lblPercentBreak.ForeColor = Color.Black;
                }
            }
        }

    }
}