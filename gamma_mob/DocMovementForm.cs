using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using gamma_mob.CustomDataGrid;

namespace gamma_mob
{
    public partial class DocMovementForm : BaseForm
    {
        private DocMovementForm()
        {
            InitializeComponent();
            SetIsLastScanedBarcodeZone(true, null);
            IsVisibledPanels = true;
        }

        /// <summary>
        ///     инициализация 
        /// </summary>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="placeIdTo">ID склада приемки</param>
        public DocMovementForm(Form parentForm, DocDirection docDirection, int placeIdTo)
            : this()
        {
            ParentForm = parentForm;
            DocDirection = docDirection;
     
            EndPointInfo = new EndPointInfo
            {
                PlaceId = placeIdTo,
                IsSettedDefaultPlaceZoneId = false,
                IsAvailabilityChildPlaceZoneId = false
            };
            if (Shared.Warehouses.First(w => w.WarehouseId == EndPointInfo.PlaceId).WarehouseZones != null
                && Shared.Warehouses.First(w => w.WarehouseId == EndPointInfo.PlaceId).WarehouseZones.Count > 0)
            {
                var dialogResult = MessageBox.Show("Вы будете указывать зону сейчас?"
                        , @"Операция с продуктом",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (dialogResult == DialogResult.Yes)
                {
                    using (var form = new ChooseZoneDialog(this, EndPointInfo.PlaceId, true))
                    {
                        DialogResult result = form.ShowDialog();
                        Invoke((MethodInvoker)Activate);
                        if (result != DialogResult.OK)
                        {
                            MessageBox.Show(@"Не выбрана зона склада.", @"Выбор зоны склада",
                                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        }
                        else
                        {
                            EndPointInfo.IsSettedDefaultPlaceZoneId = true;
                            EndPointInfo.PlaceZoneId = form.PlaceZoneId;
                            EndPointInfo.PlaceZoneName = form.PlaceZoneName;
                            lblZoneName.Text = "Зона по умолчанию: " + form.PlaceZoneName;
                            lblZoneName.Visible = true;
                            var placeZoneRows = Db.GetPlaceZoneChilds(PlaceZoneId);
                            EndPointInfo.IsAvailabilityChildPlaceZoneId = (placeZoneRows != null && placeZoneRows.Count > 0);
                            
                        }
                    }
                }
            }
            Shared.SaveToLog(@"EndPointInfo.PlaceId-" + EndPointInfo.PlaceId + @"; EndPointInfo.PlaceZoneId-" + EndPointInfo.PlaceZoneId);

            AcceptedProducts = new BindingList<MovementProduct>();

            GetLastMovementProducts();

            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номенклатура",
                    MappingName = "ShortNomenclatureName",
                    Width = 175
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Кол-во",
                    MappingName = "CollectedQuantityComputedColumn",
                    Width = 38,
                    Format = "0.###"
                });
            gridDocAccept.TableStyles.Add(tableStyle);
            //Barcodes1C = Db.GetBarcodes1C();
            //Shared.RefreshBarcodes1C();
            if (!Shared.InitializationData()) MessageBox.Show(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
            if (Shared.TimerForUnloadOfflineProducts == null) MessageBox.Show(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
            OnUpdateBarcodesIsNotUploaded();
        }

        private DocDirection DocDirection { get; set; }
        private EndPointInfo EndPointInfo { get; set; }
        
        private BindingList<MovementProduct> AcceptedProducts { get; set; }
        private BindingSource BSource { get; set; }
        
        //private delegate void ConnectStateChangeInvoker(ConnectState state);
        
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

        
        private void ShowConnection(ConnectState conState)
        {
            switch (conState)
            {
                case ConnectState.ConInProgress:
                    imgConnection.Image = ImgList.Images[(int) Images.NetworkTransmitReceive];
                    break;
                case ConnectState.NoConInProgress:
                    imgConnection.Image = null;
                    break;
                case ConnectState.NoConnection:
                    imgConnection.Image = ImgList.Images[(int) Images.NetworkOffline];
                    break;
                case ConnectState.ConnectionRestore:
                    imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnInspect.ImageIndex = (int)Images.Inspect;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnUpload.ImageIndex = (int)Images.UploadToDb;
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
                MessageBox.Show("Есть невыгруженные продукты!"+Environment.NewLine+"Сначала выгрузите в базу в зоне связи!");
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
                    if (IsVisibledPanels)
                        Close();
                    else
                    {
                        RemoveButtons(true);
                        MessageBox.Show(@"Внимание! Зона не выбрана, перемещение не выполнено!", @"Ошибка!");
                    }
                    break;
                case 1:
                    OpenDetails();
                    break;
                case 2:
                    RefreshDocMovementProducts(new Guid(), true);
                    break;
                case 3:
                    //if (OfflineProducts.Count > 0)
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
                
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text, false);
        }
                
        /// <summary>
        ///     Добавление продукта по штрихкоду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        /// <param name="fromBuffer">ШК из буфера невыгруженных</param>
        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            if (barcode.Length < Shared.MinLengthProductBarcode)
            {
                SetIsLastScanedBarcodeZone(true, null);
                Shared.SaveToLog(@"Ошибочный ШК! " + barcode);
                MessageBox.Show(@"Ошибка при сканировании ШК " + barcode + Environment.NewLine + @"Штрих-код должен быть длиннее 12 символов", @"Продукция не найдена",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                Cursor.Current = Cursors.Default;
                //if (Shared.Barcodes1C.GetExistsBarcodeOrNumberInBarcodes(barcode))
                
                {
                    if (getProductResult == null || getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                    {
                        SetIsLastScanedBarcodeZone(true, null);
                        Shared.SaveToLog(@"Продукция не найдена по ШК! " + barcode);
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
                        }
                    }
                }
            }
        }

        private void AddProductByBarcode(string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            if (!fromBuffer && getProductResult.ProductId != null && getProductResult.ProductId != Guid.Empty && Shared.ScannedBarcodes.CheckIsLastBarcode(barcode, DocDirection, null, endPointInfo.PlaceId, endPointInfo.PlaceZoneId))
            {
                Shared.SaveToLog(@"Вы уже сканировали этот шк " + barcode);
                MessageBox.Show(@"Вы уже сканировали этот шк " + barcode, @"Повтор",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
            else
            {
                SetIsLastScanedBarcodeZone(true, getProductResult);
                var scanId = Shared.ScannedBarcodes.AddScannedBarcode(barcode, endPointInfo, DocDirection, null, getProductResult);
                if (scanId == null || scanId == Guid.Empty)
                    MessageBox.Show("Ошибка1 при сохранении отсканированного штрих-кода");

                AddProductByBarcode(scanId, barcode, endPointInfo, fromBuffer, getProductResult);
            }
        }

        private bool AddProductByBarcode(Guid? scanId, string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Shared.SaveToLog(@"AddMov " + barcode + @"; P-" + endPointInfo.PlaceId + @"; Z-" + endPointInfo.PlaceZoneBarcode + @"; Q-" + getProductResult.CountProducts + @"; F-" + fromBuffer.ToString());
            
            Cursor.Current = Cursors.WaitCursor;

            var acceptResult = Db.MoveProduct(scanId, Shared.PersonId, getProductResult.ProductId, endPointInfo, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (Shared.LastQueryCompleted == false)
                {
                    return false;
                }
            if (acceptResult == null)
            {
                MessageBox.Show(@"Не удалось переместить продукт " + barcode);
                return false;
            }
            
            if (acceptResult.ResultMessage == string.Empty)
            {
               Shared.ScannedBarcodes.UploadedScan(scanId);
               if (endPointInfo.PlaceId == EndPointInfo.PlaceId)
                    Invoke((UpdateMovementGridInvoker) (UpdateGrid),
                        new object[]
                                   {
                                        acceptResult.NomenclatureId, acceptResult.CharacteristicId, acceptResult.QualityId, acceptResult.NomenclatureName, acceptResult.ShortNomenclatureName, acceptResult.PlaceZoneId,  acceptResult.Quantity
                                       , true, barcode, getProductResult.ProductKindId, acceptResult.CoefficientPackage, acceptResult.CoefficientPallet
                                   });
            }
            else
            {
               Shared.ScannedBarcodes.UploadedScanWithError(scanId, acceptResult.ResultMessage);
               if (endPointInfo.PlaceId == EndPointInfo.PlaceId)
                   MessageBox.Show(fromBuffer ? @"Ошибка при загрузке на сервер невыгруженного" + Environment.NewLine + @" продукта: " : @"Продукт: " + barcode + Environment.NewLine + acceptResult.ResultMessage);
            }
            return true;
        }
        
        private bool GetLastMovementProducts()
        {
            BindingList<MovementProduct> list = Db.GetMovementProductsList(EndPointInfo.PlaceId, Shared.PersonId);
            if (Shared.LastQueryCompleted == false)//|| list == null)
            {
                if (AcceptedProducts == null)
                    AcceptedProducts = new BindingList<MovementProduct>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = AcceptedProducts };
            
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            AcceptedProducts = list ?? new BindingList<MovementProduct>();
            if (BSource == null)
                BSource = new BindingSource {DataSource = AcceptedProducts};
            else
            {
                //if (BSource.DataSource == null) 
                    BSource.DataSource = AcceptedProducts;
            }
            //if (gridDocAccept.DataSource == null) 
               gridDocAccept.DataSource = BSource;
            gridDocAccept.UnselectAll();
            Collected = 0;
            for (var i = 0; i < AcceptedProducts.Count; i++)
            {
                Collected += AcceptedProducts[i].CollectedQuantityUnits;
            }
                //Shared.ScannedBarcodes.BarcodesCollectedCount(EndPointInfo.PlaceId);

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

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()//(object sender, EventArgs e)
        {
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
            foreach (ScannedBarcode offlineProduct in Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection))
            {
                if (offlineProduct.PlaceId == null)
                { 
                    MessageBox.Show(@"Ошибка! Не указан передел, куда выгрузить продукт " + offlineProduct.Barcode, @"Информация о выгрузке");
                    Shared.ScannedBarcodes.UploadedScanWithError(offlineProduct.ScanId, @"Ошибка! Не указан передел, куда выгрузить продукт ");
                }
                else
                {
                    if (!AddProductByBarcode(offlineProduct.ScanId, offlineProduct.Barcode, new EndPointInfo() { PlaceId = (int)offlineProduct.PlaceId, PlaceZoneId = offlineProduct.PlaceZoneId }, true, new DbProductIdFromBarcodeResult() { ProductId = offlineProduct.ProductId ?? new Guid(), ProductKindId = offlineProduct.ProductKindId, NomenclatureId = offlineProduct.NomenclatureId ?? new Guid(), CharacteristicId = offlineProduct.CharacteristicId ?? new Guid(), QualityId = offlineProduct.QualityId ?? new Guid(), CountProducts = offlineProduct.Quantity ?? 0 }))
                        break;
                }
            }            
            UIServices.SetNormalState(this);
        }

        private void BarcodeReaction(string barcode)
        {
            if (GetIsLastScanedBarcodeZone)
            {
                Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
                //GetPlaceZoneId(EndPointInfo);
                //isLastScaneBarcodeZone = false;
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

            }
            else
            {
                var endPointInfo = GetPlaceZoneFromBarcode(barcode);
                if (endPointInfo != null)
                {
                    //lastScannedProduct = GetLastScannedProduct;
                    //isLastScanedBarcodeZone = true;
                    //RemoveButtons(true);
                    if (this.InvokeRequired)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            AddProductByBarcode(edtNumber.Text, endPointInfo, false, GetLastScannedProduct);
                        });
                    }
                    else
                        AddProductByBarcode(edtNumber.Text, endPointInfo, false, GetLastScannedProduct);
                }
                else
                {
                    MessageBox.Show(@"Ошибка! Зона по штрих-коду не опознана!" + Environment.NewLine + @"Перемещение не выполнено!");
                    SetIsLastScanedBarcodeZone(true, null);
                }
            }
        }

        /*private bool DeleteProductByBarcode(string barcode, string outPlace, Guid docMovementId, bool showWarningMessage)
        {
            bool result = false;
            DialogResult dlgresult = MessageBox.Show(@"Вы хотите отменить последнее перемещение продукта с шк: " + barcode+@" и вернуть на передел "+ outPlace +"?",
                                                     @"Операция с продуктом",
                                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                     MessageBoxDefaultButton.Button1);
            if (dlgresult == DialogResult.Yes)
            {
                var deleteResult = Db.DeleteProductFromMovement(barcode, docMovementId, DocDirection.DocOutIn);
                if (deleteResult == null) return false;
                if (string.IsNullOrEmpty(deleteResult.ResultMessage) && Shared.LastQueryCompleted == true)
                {
                    result = true;
                    //Barcodes.Remove(barcode);
                    //Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));

                    var product = deleteResult.Product;
                    //var product = AcceptedProducts.FirstOrDefault(p => p.Barcode == barcode || p.Number == barcode);
                    
                    if (product != null)
                    {
                        if (deleteResult.DocIsConfirmed)
                        {
                            //product.DocIsConfirmed = true;
                            MessageBox.Show(
                                @"Перемещение уже подтверждено. Удалять продукт из подтвержденного перемещения нельзя",
                                @"Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
                                MessageBoxDefaultButton.Button1);
                            return true;
                        }
                        Invoke((UpdateGridInvoker) (UpdateGrid),
                               new object[]
                                   {
                                       product.NomenclatureId, product.CharacteristicId, product.QualityId, product.NomenclatureName, product.ShortNomenclatureName, deleteResult.PlaceZoneId, product.Quantity
                                       , false, barcode, deleteResult.ProductKindId, null, null
                                   });
                    }
                }                
            }
            else result = true;
            return result;
        }*/

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName, string shortNomenclatureName, Guid? placeZoneId, decimal quantity, bool add, string barcode,
                                int? productKindId, int? coefficientPackage, int? coefficientPallet)
        {
            MovementProduct good = null;
            good = AcceptedProducts.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId && g.PlaceZoneId == placeZoneId);
            if (good == null)
            {
                good = new MovementProduct
                {
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    QualityId = qualityId,
                    NomenclatureName = nomenclatureName,
                    ShortNomenclatureName = shortNomenclatureName,
                    PlaceZoneId = placeZoneId,
                    CollectedQuantity = 0,
                    CoefficientPackage = coefficientPackage,
                    CoefficientPallet = coefficientPallet
                };
                AcceptedProducts.Add(good);
                BSource.DataSource = AcceptedProducts;
            }
            if (!add)
            {
                good.CollectedQuantity -= quantity;
                if (productKindId == null || productKindId != 3)
                {
                    Collected--;
                    //Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));
                }
                if (good.CollectedQuantity == 0)
                    AcceptedProducts.Remove(good);
            }
            else
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
            gridDocAccept.UnselectAll();
            int index = AcceptedProducts.IndexOf(good);
            if (index > 0)
            {
                gridDocAccept.CurrentRowIndex = index;
                gridDocAccept.Select(index);
            }
            Activate();
        }

        private void gridDocAccept_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        private void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            var row = gridDocAccept.CurrentRowIndex;
            if (row >= 0)
            {
                var good = AcceptedProducts[row];
                var form = new DocMovementProductsForm(EndPointInfo.PlaceId, Shared.PersonId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, good.PlaceZoneId, this);
                if (!form.IsDisposed)
                {
                    //form.Show();
                    //if (form.Enabled)
                    //    Hide();
                    
                    BarcodeFunc = null;
                    DialogResult result = form.ShowDialog();
                    if (form.IsRefreshQuantity)
                        RefreshDocMovementProducts(Guid.Empty, true);
                    BarcodeFunc = BarcodeReaction;
                }
            }
        }

        private void RefreshDocMovementProducts(Guid docId, bool showMessage)
        {
            if (!GetLastMovementProducts())
            {
                if (showMessage) MessageBox.Show(@"Не удалось получить информацию о документе!" + Environment.NewLine + @"Попробуйте ещё раз обновить!");
                //Close();
                return;
            }
        }
        
        private bool _isLastScanedBarcodeZone { get; set; }
        private DbProductIdFromBarcodeResult _lastScannedProduct { get; set; }

        private bool GetIsLastScanedBarcodeZone
        {
            get { return _isLastScanedBarcodeZone; }
        }

        private DbProductIdFromBarcodeResult GetLastScannedProduct
        {
            get { return _lastScannedProduct; }
        }


        private void SetIsLastScanedBarcodeZone(bool value, DbProductIdFromBarcodeResult getProductResult)
        {
            _isLastScanedBarcodeZone = value;
            if (getProductResult != null) _lastScannedProduct = getProductResult;
        }

        /// <summary>
        /// Сюда попадет id конечной зоны
        /// </summary>
        public Guid PlaceZoneId { get; set; }
        public String PlaceZoneName { get; set; }

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
                AddProductByBarcode(edtNumber.Text, endPointInfo, false, GetLastScannedProduct);
            }
            else
            {
                var placeZoneChilds = Db.GetPlaceZoneChilds((Guid)endPointInfo.PlaceZoneId).ToList();
                if (placeZoneChilds == null || placeZoneChilds.Count == 0)
                {
                    MessageBox.Show(@"Внимание! Зона не выбрана!" + Environment.NewLine + @"Перемещение не выполнено!");
                    SetIsLastScanedBarcodeZone(true, null);
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
                        button.Top = 25 + 2 * ((int)k + 1) + (int)height * (int)k ;
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

        private bool _isVisibledPanels {get; set;}
        private bool IsVisibledPanels
        {
            get
            {
                return _isVisibledPanels;
            }
            set
            {
                _isVisibledPanels = value;
                if (value)
                    SetIsLastScanedBarcodeZone(true, null);
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
                if (gridDocAccept.InvokeRequired)
                    Invoke((MethodInvoker)(() => gridDocAccept.Visible = value));
                else
                    gridDocAccept.Visible = value;
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

        public EndPointInfo GetPlaceZoneFromBarcode(string barcode)
        {
            var placeZone = Shared.PlaceZones.Where(p => p.Barcode == barcode && p.IsValid).FirstOrDefault();
            if (placeZone != null)
            {
                    return GetPlaceZoneChildId(new EndPointInfo() { PlaceId = EndPointInfo.PlaceId, PlaceZoneId = placeZone.PlaceZoneId, PlaceZoneName = placeZone.Name });
            }
            else
            {
                MessageBox.Show(@"Ошибка! Штрих-код зоны не распознан! Попробуйте еще раз или выберите зону", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
                return null;
            }
        }
    }
}