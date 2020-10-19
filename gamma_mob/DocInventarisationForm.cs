using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using gamma_mob.Dialogs;
using System.Drawing;

namespace gamma_mob
{
    public sealed partial class DocInventarisationForm : BaseForm
    {
        private int _collected;

        private DocInventarisationForm()
        {
            InitializeComponent(); 
        }

        //private DocInventarisationForm(Form parentForm): this()
        //{
        //    ParentForm = parentForm;
        //    //OfflineProducts = new List<OfflineProduct>();
        //}

        /// <summary>
        ///     инициализация формы
        /// </summary>
        /// <param name="docInventarisationId">ID Документа</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="docNumber">Номер инвентаризации</param>
        public DocInventarisationForm(Guid docInventarisationId, Form parentForm, string docNumber, DocDirection docDirection, int placeId)
            : this()
        {
            //FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\DocInventarisationBarcodes.xml";
            ParentForm = parentForm; 
            DocDirection = docDirection;
            DocInventarisationId = docInventarisationId;

            EndPointInfo = new EndPointInfo
            {
                PlaceId = placeId,
                IsSettedDefaultPlaceZoneId = false,
                IsAvailabilityChildPlaceZoneId = false
            };
            bool IsExit = false;
            var war = Shared.Warehouses.FirstOrDefault(w => w.WarehouseId == EndPointInfo.PlaceId);
            if (war != null && war.WarehouseZones != null && war.WarehouseZones.Count > 0)
            {
                //var dialogResult = MessageBox.Show("Вы будете указывать зону сейчас?"
                //        , @"Операция с продуктом",
                //        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                //if (dialogResult == DialogResult.Yes)
                {
                    using (var form = new ChooseZoneDialog(this, EndPointInfo.PlaceId, true))
                    {
                        DialogResult result = form.ShowDialog();
                        Invoke((MethodInvoker)Activate);
                        if (result != DialogResult.OK)
                        {
                            MessageBox.Show(@"Не выбрана зона склада.", @"Выбор зоны склада",
                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                            IsExit = true;
                            Close();
                        }
                        else
                        {
                            EndPointInfo.IsSettedDefaultPlaceZoneId = true;
                            EndPointInfo.PlaceZoneId = form.PlaceZoneId;
                            EndPointInfo.PlaceZoneName = form.PlaceZoneName;
                            lblZoneName.Text = "Зона по умолчанию: " + form.PlaceZoneName;
                            lblZoneName.Visible = true;
                            var placeZoneRows = Db.GetPlaceZoneChilds(form.PlaceZoneId);
                            EndPointInfo.IsAvailabilityChildPlaceZoneId = (placeZoneRows != null && placeZoneRows.Count > 0);

                            if (EndPointInfo.IsAvailabilityChildPlaceZoneId)
                            {
                                var endPointInfo = GetPlaceZoneId(EndPointInfo);
                                if (endPointInfo != null)
                                {
                                    EndPointInfo.PlaceZoneId = endPointInfo.PlaceZoneId;
                                    EndPointInfo.PlaceZoneName = endPointInfo.PlaceZoneName;
                                    lblZoneName.Text = "Зона по умолчанию: " + endPointInfo.PlaceZoneName;
                                }
                                else
                                {
                                    //SetIsLastScanedBarcodeZone(false, getProductResult);
                                }
                                EndPointInfo.IsAvailabilityChildPlaceZoneId = true;
                            }
                        }
                    }
                }
            }

            if (!IsExit)
            {
                Shared.SaveToLog(@"EndPointInfo.PlaceId-" + EndPointInfo.PlaceId + @"; EndPointInfo.PlaceZoneId-" + EndPointInfo.PlaceZoneId);


                RefreshProducts(docInventarisationId);
                /*if (!RefreshProducts(docInventarisationId))
                {
                    MessageBox.Show(@"Не удалось получить информацию о документе");
                    Close();
                    return;
                }*/
                string placeName = "";
                for (int i = 0; i < Shared.Warehouses.Count; i++)
                {
                    if (Shared.Warehouses[i].WarehouseId == placeId)
                    {
                        placeName = Shared.Warehouses[i].WarehouseName;
                        break;
                    }
                }
                Text = "Инвент-я №" + docNumber + " (" + placeName+")";
                var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
                tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Номенклатура",
                        MappingName = "ShortNomenclatureName",
                        Width = 171
                    });
                tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Кол-во",
                        MappingName = "CollectedQuantity",
                        Width = 50
                    });
                gridInventarisation.TableStyles.Add(tableStyle);
                //Barcodes1C = Db.GetBarcodes1C();
                //Shared.RefreshBarcodes1C();
                if (!Shared.InitializationData()) MessageBox.Show(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
                if (Shared.TimerForUnloadOfflineProducts == null) MessageBox.Show(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
                OnUpdateBarcodesIsNotUploaded();
            }
        }

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

        private DocDirection DocDirection { get; set; }
        private Guid DocInventarisationId { get; set; }
        private BindingSource BSource { get; set; }
        private EndPointInfo EndPointInfo { get; set; }
        
        private bool RefreshProducts(Guid docInventarisationId)
        {
            BindingList<DocNomenclatureItem> list = Db.InventarisationProducts(docInventarisationId);
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
                BSource = new BindingSource { DataSource = NomenclatureList };
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridInventarisation.DataSource = BSource;
            gridInventarisation.UnselectAll();
            //Barcodes = Db.GetCurrentInventarisationBarcodes(DocInventarisationId);

            Collected = 0;
            for (var i = 0; i < NomenclatureList.Count; i++)
            {
                Collected += NomenclatureList[i].CollectedQuantityUnits;
            }
            
            /*Collected = 0;
            if (Barcodes != null)
            {
                foreach (Barcodes item in Barcodes)
                {
                    Collected += (item.ProductKindId == 3) ? 0 : 1;
                }
            }
            else
                Barcodes = new List<Barcodes>();*/
            return true;
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

        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnUpload.ImageIndex = (int)Images.UploadToDb;
            btnInspect.ImageIndex = (int) Images.Inspect;
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
                MessageBox.Show("Есть невыгруженные продукты!" + Environment.NewLine + "Выгрузите в зоне связи!");
            base.OnFormClosing(sender, e);
            ConnectionState.OnConnectionRestored -= ConnectionRestored;
            ConnectionState.OnConnectionLost -= ConnectionLost;
            ScannedBarcodes.OnUpdateBarcodesIsNotUploaded -= OnUpdateBarcodesIsNotUploaded;
            ScannedBarcodes.OnUnloadOfflineProducts -= UnloadOfflineProducts;
        }

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
            foreach (ScannedBarcode offlineProduct in Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection))
            {
                if (offlineProduct.DocId == null)
                { 
                    MessageBox.Show(@"Ошибка! Не указан документ инвентаризации, куда выгрузить продукт " + offlineProduct.Barcode, @"Информация о выгрузке");
                    Shared.ScannedBarcodes.UploadedScanWithError(offlineProduct.ScanId, @"Ошибка! Не указан документ инвентаризации, куда выгрузить продукт ", offlineProduct.ProductId);
                }
                else
                {
                    if (AddProductByBarcode(offlineProduct.ScanId, offlineProduct.Barcode, new EndPointInfo() { PlaceId = (int)offlineProduct.PlaceId, PlaceZoneId = offlineProduct.PlaceZoneId }, true, new DbProductIdFromBarcodeResult() { ProductId = offlineProduct.ProductId ?? new Guid(), ProductKindId = offlineProduct.ProductKindId, NomenclatureId = offlineProduct.NomenclatureId ?? new Guid(), CharacteristicId = offlineProduct.CharacteristicId ?? new Guid(), QualityId = offlineProduct.QualityId ?? new Guid(), CountProducts = offlineProduct.Quantity ?? 0 }, (Guid)offlineProduct.DocId) == null)
                        break;
                }
            }
            UIServices.SetNormalState(this);
        }
                
        /*
        /// <summary>
        ///     Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        private void AddOfflineBarcode(string barCode)
        {
            AddOfflineProduct(barCode);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
            ConnectionState.StartChecker();
        }
               
        private void AddOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
            {
                DocId = DocInventarisationId,
                Barcode = barcode,
                PersonId = Shared.PersonId,
                ResultMessage = "Не выгружено"
            });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocId == DocInventarisationId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts);
        }

        private string FileName { get; set; }

        private void SaveToXml(ICollection offlineProducts)
        {
            var ser = new XmlSerializer(typeof(List<OfflineProduct>));
            using (var stream = new FileStream(FileName, FileMode.Create))
            {
                try
                {
                    ser.Serialize(stream, offlineProducts);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.Message);
#endif
                    MessageBox.Show(
                        @"Не удалось сохранить временный файл, возможно нужно проверить последние введенные продукты");
                }
            }
        }*/


        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            //AddProductByBarcode(barcode, fromBuffer, new Guid(), new Guid(), new Guid(), null);
            if (barcode.Length < Shared.MinLengthProductBarcode)
            {
                Shared.SaveToLog(@"Ошибочный ШК! " + barcode);
                MessageBox.Show(@"Ошибка при сканировании ШК " + barcode + Environment.NewLine + @"Штрих-код должен быть длиннее 12 символов", @"Продукция не найдена",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (!fromBuffer && Shared.ScannedBarcodes.CheckIsLastBarcode(barcode, DocDirection, DocInventarisationId, EndPointInfo.PlaceId, EndPointInfo.PlaceZoneId))
                {
                    Shared.SaveToLog(@"Вы уже сканировали этот шк " + barcode);
                    MessageBox.Show(@"Вы уже сканировали этот шк " + barcode, @"Повтор",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, true);
                    Cursor.Current = Cursors.Default;
                    //if (Shared.Barcodes1C.GetExistsBarcodeOrNumberInBarcodes(barcode))
                    
                    {
                        if (getProductResult == null || getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                        {
                            Shared.SaveToLog(@"Продукция не найдена по ШК! " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodes.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                            MessageBox.Show(@"Продукция не найдена по ШК!", @"Продукция не найдена",
                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        }
                        else
                        {
                            /*if (getProductResult.ProductKindId == 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty))
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
                                            SetIsLastScanedBarcodeZone(true, null);
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
                                        SetIsLastScanedBarcodeZone(true, null);
                                        return;
                                    }
                                    else
                                    {
                                        getProductResult.CountProducts = form.Quantity;
                                    }
                                }

                            }

                            if (EndPointInfo.IsAvailabilityPlaceZoneId)
                            {
                                var endPointInfo = GetPlaceZoneId(EndPointInfo);
                                if (endPointInfo != null)
                                {
                                    SetIsLastScanedBarcodeZone(true, getProductResult);
                                    AddProductByBarcode(barcode, endPointInfo, fromBuffer, getProductResult);
                                }
                                else
                                {
                                    SetIsLastScanedBarcodeZone(false, getProductResult);
                                }
                            }
                            else
                            {
                                SetIsLastScanedBarcodeZone(true, getProductResult);
                                AddProductByBarcode(barcode, EndPointInfo, fromBuffer, getProductResult);
                            }*/
                            AddProductByBarcode(barcode, EndPointInfo, fromBuffer, getProductResult);
                        }
                    }
                }
            }
        }

        private void AddProductByBarcode(string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            var scanId = Shared.ScannedBarcodes.AddScannedBarcode(barcode, endPointInfo, DocDirection, DocInventarisationId, getProductResult);
            if (scanId == null || scanId == Guid.Empty)
                MessageBox.Show("Ошибка1 при сохранении отсканированного штрих-кода");

            AddProductByBarcode(scanId, barcode, endPointInfo, fromBuffer, getProductResult, DocInventarisationId);
        }

        private bool? AddProductByBarcode(Guid? scanId, string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult, Guid docInventarisationId)
        {
            Shared.SaveToLog(@"AddInv " + barcode + @"; ScanId-" + scanId.ToString() + @"; fromBuffer-" + fromBuffer.ToString() + @"; docInventarisationId-" + docInventarisationId.ToString());

            Cursor.Current = Cursors.WaitCursor;

            //var addResult = Db.AddProductToInventarisation(DocInventarisationId, barcode);
            var addResult = Db.AddProductIdToInventarisation(scanId, docInventarisationId, Shared.PersonId, endPointInfo, getProductResult.ProductId, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (Shared.LastQueryCompleted == false)
            {
                return null;
            }
            if (addResult == null)
            {
                MessageBox.Show(@"Не удалось добавить продукт" + Environment.NewLine + barcode + " в инвентаризацию!");
                Shared.ScannedBarcodes.ClearLastBarcode();
                return false;
            }
            if (addResult.ResultMessage == string.Empty && addResult.Product != null)
            {
                Shared.ScannedBarcodes.UploadedScan(scanId, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                if (DocInventarisationId == docInventarisationId)
                    Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                           new object[] { addResult.Product.NomenclatureId, addResult.Product.CharacteristicId, addResult.Product.QualityId, addResult.Product.NomenclatureName,
                               addResult.Product.ShortNomenclatureName, addResult.Product.Quantity, getProductResult.ProductKindId });
            }
            else
            {
                Shared.ScannedBarcodes.UploadedScanWithError(scanId, addResult.ResultMessage, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                if (DocInventarisationId == docInventarisationId)
                {
                    MessageBox.Show(fromBuffer ? @"Ошибка при загрузке на сервер невыгруженного" + Environment.NewLine + @" продукта: " : @"Продукт: " + barcode + Environment.NewLine + addResult.ResultMessage);
                    Shared.ScannedBarcodes.ClearLastBarcode();
                }
            }
            return true;
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, int? productKindId)
        {
            DocNomenclatureItem good =
                NomenclatureList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId);
            if (good == null)
            {
                good = new DocNomenclatureItem
                {
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    QualityId = qualityId,
                    NomenclatureName = nomenclatureName,
                    ShortNomenclatureName = shortNomenclatureName,
                    CollectedQuantity = 0
                };
                NomenclatureList.Add(good);
                BSource.DataSource = NomenclatureList;
            }
            //if (!add)
            //{
            //    good.CollectedQuantity -= quantity;
            //    if (productKindId == null || productKindId != 3)
            //    {
            //        Collected--;
            //        //Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));
            //    }
            //    if (good.CollectedQuantity == 0)
            //        NomenclatureList.Remove(good);
            //}
            //else
            {
                good.CollectedQuantity += quantity;
                if (productKindId == null || productKindId != 3)
                {
                    Collected++;
                    //Barcodes.Add(new Barcodes
                    //{
                    //    Barcode = barcode,
                    //    ProductKindId = productKindId
                    //});
                }
            }
            gridInventarisation.UnselectAll();
            int index = NomenclatureList.IndexOf(good);
            if (index > 0)
            {
                gridInventarisation.CurrentRowIndex = index;
                gridInventarisation.Select(index);
            }
            Activate();
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            UIServices.SetBusyState(this);
            if (this.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate()
                {
                    btnAddProduct_Click(new object(), new EventArgs());
                });
            }
            else
                btnAddProduct_Click(new object(), new EventArgs());
            UIServices.SetNormalState(this);
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
                    //ConnectionRestored();
                    break;
            }
        }

        //private List<string> Barcodes { get; set; }
        //private List<Barcodes> Barcodes { get; set; }
        //private List<OfflineProduct> OfflineProducts { get; set; }
        //private List<Barcodes1C> OfflineBarcodes1C { get; set; }
        //private BindingList<ChooseNomenclatureItem> Barcodes1C { get; set; }

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
                    RefreshDocInventarisation(DocInventarisationId,true);
                    break;
                case 3:
                    //if (OfflineProducts.Count(p => p.DocId == DocInventarisationId) > 0)
                        UnloadOfflineProducts();
                    break;
                case 4:
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
            int row = gridInventarisation.CurrentRowIndex;
            if (row >= 0)
            {
                var good = NomenclatureList[row];
                //var form = new DocInventarisationNomenclatureProductsForm(DocInventarisationId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this);
                var form = new DocInventarisationProductsForm(DocInventarisationId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this);//, new RefreshDocProductDelegate(RefreshDocInventarisation));
                if (!form.IsDisposed)
                {
                    //form.Show();
                    //if (form.Enabled)
                    //    Hide();

                    BarcodeFunc = null;
                    DialogResult result = form.ShowDialog();
                    if (form.IsRefreshQuantity)
                        RefreshDocInventarisation(DocInventarisationId,true);
                    BarcodeFunc = BarcodeReaction;
                }
            }
        }

        private void RefreshDocInventarisation(Guid docId, bool showMessage)
        {
            if (!RefreshProducts(docId))
            {
                if (showMessage) MessageBox.Show(@"Не удалось получить информацию о документе");
                //Close();
                return;
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            UIServices.SetBusyState(this);
            AddProductByBarcode(edtNumber.Text, false);
            UIServices.SetNormalState(this);
        }

        private void gridInventarisation_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        private void gridInventarisation_CurrentCellChanged(object sender, EventArgs e)
        {
            gridInventarisation.Select(gridInventarisation.CurrentRowIndex);
        }

        //private void gridInventarisation_DoubleClick(object sender, EventArgs e)
        //{

        //}

        List<ButtonGuidId> buttonsAdded = new List<ButtonGuidId>();

        private void btnZone_Click(object sender, EventArgs e)
        {
            var endPointInfo = new EndPointInfo()
            {
                PlaceId = EndPointInfo.PlaceId,
                PlaceZoneId = (sender as ButtonGuidId).Id,
                PlaceZoneName = Convert.ToString((sender as ButtonGuidId).Text.Replace("\r\n", ""))
            };
            var placeZoneRow = GetPlaceZoneChildId(endPointInfo);
            if (placeZoneRow != null)
            {
                //MessageBox.Show(@"Зона выбрана!" + Environment.NewLine + placeZoneRow.PlaceZoneName);
                EndPointInfo.PlaceZoneId = placeZoneRow.PlaceZoneId;
                EndPointInfo.PlaceZoneName = placeZoneRow.PlaceZoneName;
                lblZoneName.Text = "Зона по умолчанию: " + placeZoneRow.PlaceZoneName;
                lblZoneName.Visible = true;
                IsVisibledPanels = true;            
                //SetPlaceZone(placeZoneRow);
            }
            else
            {
                var placeZoneChilds = Db.GetPlaceZoneChilds((Guid)endPointInfo.PlaceZoneId).ToList();
                if (placeZoneChilds == null || placeZoneChilds.Count == 0)
                {
                    MessageBox.Show(@"Внимание! Зона не выбрана!" + Environment.NewLine + @"Перемещение не выполнено!");
                    //SetIsLastScanedBarcodeZone(true, null);
                }
            }
        }

        private void AddButtonDelegatePatternParams(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate
                {
                    AddButtonDelegatePatternParams(placeZoneId, placeZoneName, i, k, width, height, maxCells);
                };
                this.Invoke(del);
                return;
            }
            AddButton(placeZoneId, placeZoneName, i, k, width, height, maxCells);
        }

        private void AddButton(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells)
        {

            Random random = new Random(2);
            //Thread.Sleep(20);

            var button = new ButtonGuidId(placeZoneId);
            if (k == null && height == null)
            {
                button.Click += btnZone_Click;
                button.Font = new Font("Tahoma", 10, FontStyle.Regular);
                button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                button.Width = (width - 5 - 20) / 2;
                button.Height = 32;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 25 + 2 + 33 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
            }
            else
            {
                if (k == null)
                {
                    button.Click += btnZone_Click;
                    button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                    button.Width = width;
                    button.Height = (int)height;// Height - 34;
                    button.Font = new Font("Tahoma", 10, FontStyle.Regular); //new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = (int)maxCells == 0 ? 2 + (button.Width + 6) * Convert.ToInt32(Math.Floor(i / 7)) : 2 * (i + 1) + width * i;// * (i + 1) + buttonWidth * i;
                    button.Top = (int)maxCells == 0 ? 25 + ((int)height + 2) * (i % 7) : 27;
                    Shared.MakeButtonMultiline(button);
                }
                else
                {
                    button.Click += btnZone_Click;
                    button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                    button.Width = width;
                    button.Height = (int)height;
                    button.Font = new Font("Tahoma", 10, FontStyle.Regular); //new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = 2 * (i + 1) + width * i;
                    button.Top = 25 + 2 * ((int)k + 1) + (int)height * (int)k;
                    Shared.MakeButtonMultiline(button);
                }
            }
            this.Controls.Add(button);
            buttonsAdded.Insert(0, button);

        }

        private void RemoveButtonDelegatePatternParams(ButtonGuidId buttonToRemove)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate
                {
                    RemoveButtonDelegatePatternParams(buttonToRemove);
                };
                this.Invoke(del);
                return;
            }
            RemoveButton(buttonToRemove);
        }

        private void RemoveButton(ButtonGuidId buttonToRemove)
        {
            Random random = new Random(2);
            this.Controls.Remove(buttonToRemove);
        }

        private void RemoveButtons(bool IsVisiblePanels)
        {
            var buttonsMaxIndex = buttonsAdded.Count - 1;
            for (int i = buttonsMaxIndex; i >= 0; i--)
            {
                ButtonGuidId buttonToRemove = buttonsAdded[i];
                buttonsAdded.Remove(buttonToRemove);
                this.RemoveButtonDelegatePatternParams(buttonToRemove);
            }
            if (IsVisiblePanels)
            {
                IsVisibledPanels = true;
            }
        }

        private bool _isVisibledPanels { get; set; }
        private bool IsVisibledPanels
        {
            get
            {
                return _isVisibledPanels;
            }
            set
            {
                _isVisibledPanels = value;
                //if (value)
                //    SetIsLastScanedBarcodeZone(true, null);
                for (int i = 1; i < tbrMain.Buttons.Count; i++)
                {
                    if (tbrMain.InvokeRequired)
                        Invoke((MethodInvoker)(() => tbrMain.Buttons[i].Visible = value));
                    else
                        tbrMain.Buttons[i].Visible = value;
                }
                if (pnlSearch.InvokeRequired)
                    Invoke((MethodInvoker)(() => pnlSearch.Visible = value));
                else
                    pnlSearch.Visible = value;
                if (gridInventarisation.InvokeRequired)
                    Invoke((MethodInvoker)(() => gridInventarisation.Visible = value));
                else
                    gridInventarisation.Visible = value;
                if (pnlInfo.InvokeRequired)
                    Invoke((MethodInvoker)(() => pnlInfo.Visible = value));
                else
                    pnlInfo.Visible = value;
                if (pnlZone.InvokeRequired)
                    Invoke((MethodInvoker)(() => pnlZone.Visible = value));
                else
                    pnlZone.Visible = value;
            }
        }

        private EndPointInfo GetPlaceZoneId(EndPointInfo endPointInfo)
        {
            if (endPointInfo.IsSettedDefaultPlaceZoneId)
            {
                if (!endPointInfo.IsAvailabilityChildPlaceZoneId)
                    return endPointInfo;
                else
                {
                    return GetPlaceZoneChildId(endPointInfo);
                }
            }
            else
            {
                var placeZones = Db.GetWarehousePlaceZones(endPointInfo.PlaceId);
                if (placeZones != null && placeZones.Count > 0)
                {
                    IsVisibledPanels = false;
                    //Height = 30 + placeZones.Count * 30;
                    var width = Screen.PrimaryScreen.WorkingArea.Width;
                    var height = Screen.PrimaryScreen.WorkingArea.Height;
                    //if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
                    //Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
                    for (int i = 0; i < placeZones.Count; i++)
                    {
                        this.AddButtonDelegatePatternParams(placeZones[i].PlaceZoneId, placeZones[i].Name, i, null, width, null, null);
                    }
                }

            }
            return null;
        }

        private EndPointInfo GetPlaceZoneChildId(EndPointInfo endPointInfo)
        {
            {

                var placeZoneRows = Db.GetPlaceZoneChilds((Guid)endPointInfo.PlaceZoneId).ToList();
                if (placeZoneRows == null || placeZoneRows.Count == 0)
                {
                    RemoveButtons(true);
                    return endPointInfo;
                }
                IsVisibledPanels = false;
                RemoveButtons(false);
                var width = Screen.PrimaryScreen.WorkingArea.Width;
                var placeZoneCells = placeZoneRows.Select(placeZoneRow => Db.GetPlaceZoneChilds(placeZoneRow.PlaceZoneId)).ToList();
                var maxCells = placeZoneCells.Where(c => c.Count > 0).Count();//.Max();          
                //Height = 30+ maxCells * 40;
                var height = Screen.PrimaryScreen.WorkingArea.Height;
                //if (Height > Screen.PrimaryScreen.WorkingArea.Height || maxCells == 0) Height = Screen.PrimaryScreen.WorkingArea.Height;
                //Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
                var buttonWidth = maxCells == 0 ? (placeZoneRows.Count <= 7 ? (width - 5 - 20) : (width - 5 - 20) / 2) : (width / placeZoneRows.Count - 10) < 50 ? (width - 5 - 20) / 3 : width / placeZoneRows.Count - 10;
                var buttonHeight = 32;//maxCells == 0 ? 50 : (Height-30)/maxCells - 2;
                for (int i = 0; i < placeZoneRows.Count; i++)
                {
                    if (placeZoneCells[i] == null || placeZoneCells[i].Count == 0)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneRows[i].PlaceZoneId, placeZoneRows[i].Name, i, null, buttonWidth, buttonHeight, maxCells);
                        continue;
                    }
                    for (int k = 0; k < placeZoneCells[i].Count; k++)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneCells[i][k].PlaceZoneId, placeZoneCells[i][k].Name, i, k, buttonWidth, buttonHeight, maxCells);
                    }
                }
            }
            return null;
        }

    }   
}