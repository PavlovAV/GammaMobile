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
            OfflineProducts = new List<OfflineProduct>();
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
        public DocWithNomenclatureForm(Guid docOrderId, Form parentForm, string orderNumber, OrderType orderType, string fileName, 
            DocDirection docDirection, int maxAllowedPercentBreak)
            : this(parentForm)
        {
            OrderType = orderType;
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + fileName;
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
                    Text = "Внутренний заказ № " + orderNumber;
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

            //Barcodes1C = Db.GetBarcodes1C();
            //Shared.RefreshBarcodes1C();
        }

        // Устанавливаем цвет фона для ячейки Собрано при превышении собранного количества над требуемым!
        private void ColumnSetCellFormat(object sender, DataGridFormatCellEventArgs e)
        {
            if ((e.Source.List[e.Row] as DocNomenclatureItem).IsPercentCollectedExcess)
                e.BackBrush = new SolidBrush(Color.Red);
        }

        private OrderType OrderType { get; set; }
        private DocDirection DocDirection { get; set; }

        private string FileName { get; set; }
        private BindingSource BSource { get; set; }

        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        //private List<string> Barcodes { get; set; }
        private List<Barcodes> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }
        private List<Barcodes1C> OfflineBarcodes1C { get; set; }
        //private BindingList<ChooseNomenclatureItem> Barcodes1C { get; set; }

        private int MaxAllowedPercentBreak { get; set; }
        public bool IsRefreshQuantity = false;

        /// <summary>
        ///     ID документа, по которому идет работа (основание)
        /// </summary>
        private Guid DocOrderId { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnInspect.ImageIndex = (int) Images.Inspect;
            btnRefresh.ImageIndex = (int) Images.Refresh;
            btnUpload.ImageIndex = (int) Images.UploadToDb;
            btnPallets.ImageIndex = (int) Images.Pallet;
            btnQuestionNomenclature.ImageIndex = (int) Images.Question;
            btnInfoProduct.ImageIndex = (int)Images.InfoProduct;
            BarcodeFunc = BarcodeReaction;


            //Подписка на событие восстановления связи
            ConnectionState.OnConnectionRestored += UnloadOfflineProducts;

            //Получение невыгруженных шк из файла
            if (!File.Exists(FileName)) return;
            var ser = new XmlSerializer(typeof (List<OfflineProduct>));
            using (var reader = new StreamReader(FileName))
            {
                try
                {
                    var list = ser.Deserialize(reader) as List<OfflineProduct>;
                    OfflineProducts = list ?? new List<OfflineProduct>();
                }
                catch (InvalidOperationException)
                {
                }
            }
            if (OfflineProducts != null && OfflineProducts.Count > 0) UnloadOfflineProducts();
        }

        private int _collected;

        private int Collected
        {
            get { return _collected; }
            set 
            { 
                _collected = value;
                lblCollected.Text = Collected.ToString(CultureInfo.InvariantCulture);
            }
        }
        private int _countNomenclatureExceedingMaxPercentWithBreak;

        private int CountNomenclatureExceedingMaxPercentWithBreak
        {
            get { return _countNomenclatureExceedingMaxPercentWithBreak; }
            set
            {
                _countNomenclatureExceedingMaxPercentWithBreak = value;
                lblPercentBreak.Text = (CountNomenclatureExceedingMaxPercentWithBreak > 0) ? "% обрыва превышен" : "% обрыва в норме";//(100 * CountProductSpoolsWithBreak / CountProductSpools).ToString(CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        ///     Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        private void AddOfflineBarcode(string barCode)
        {
            AddOfflineProduct(barCode);
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConnection});
            ConnectionState.StartChecker();
        }

        /// <summary>
        ///     Добавление шк россыпи к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        private void AddOfflineBarcode(string barCode, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int quantity)
        {
            AddOfflineProduct(barCode, nomenclatureId, characteristicId, qualityId, quantity);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
            ConnectionState.StartChecker();
        }
        
        /*
                /// <summary>
                /// ID текущего документа Gamma
                /// </summary>
                private Guid DocId { get; set; }
        */

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
            }
        }

        /*
                private void RemoveOfflineProduct(string barcode)
                {
                    if (OfflineProducts == null) return;
                    OfflineProducts.Remove(OfflineProducts.FirstOrDefault(p => p.Barcode == barcode));
                    SaveToXml(OfflineProducts);
                }
        */

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
            ConnectionState.OnConnectionRestored -= UnloadOfflineProducts;
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
                    RefreshDocOrderGoods(DocOrderId);
                    break;
                case 3:
                    if (OfflineProducts.Count(p => p.DocId == DocOrderId) > 0)
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
                        form.Show();
                        if (form.Enabled)
                        {
                            BarcodeFunc = null;
                            Hide();
                        }
                    }                   
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
                return false;
            }
            if (list != null)
                NomenclatureList = list;
            else
                NomenclatureList = new BindingList<DocNomenclatureItem>();
            if (BSource == null)
                BSource = new BindingSource {DataSource = NomenclatureList};
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridDocOrder.DataSource = BSource;
            
            Barcodes = Db.GetCurrentBarcodes(DocOrderId, DocDirection);
            //Barcodes = Db.CurrentBarcodes(DocOrderId, DocDirection);
            //Collected = Barcodes.Count;
            Collected = 0;
            if (Barcodes != null)
            {
                foreach (Barcodes item in Barcodes)
                {
                    Collected += (item.ProductKindId == 3) ? 0 : 1;
                }
            }
            else
                Barcodes = new List<Barcodes>();
            CountNomenclatureExceedingMaxPercentWithBreak = 0;
            if (NomenclatureList != null)
            {
                foreach (DocNomenclatureItem item in NomenclatureList)
                {
                    CountNomenclatureExceedingMaxPercentWithBreak += (item.SpoolWithBreakPercentColumn > Convert.ToDecimal(MaxAllowedPercentBreak)) ? 1 : 0;
                }
            }
            return true;
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker) (() => edtNumber.Text = barcode));
            UIServices.SetBusyState(this);
            AddProductByBarcode(barcode, false);
            UIServices.SetNormalState(this);
        }

        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            //Guid nomenclatureId = new Guid();
            //Guid characteristicId = new Guid();
            //Guid qualityId = new Guid();
            AddProductByBarcode(barcode, fromBuffer, new Guid(), new Guid(), new Guid(), null);
        }

        private void AddProductByBarcode(string barcode, bool fromBuffer, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int? quantity)
        {
            Shared.SaveToLog(@"AddShip " + barcode + @"; Q-" + quantity + @"; F-" + fromBuffer.ToString());
            var offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            if (!ConnectionState.CheckConnection())
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            //if (Barcodes.Contains(barcode))
            if (Barcodes.Any(b => b.Barcode == barcode & (b.ProductKindId == null || b.ProductKindId != 3)))
            {
                if (DeleteProductByBarcode(barcode) && fromBuffer)
                {
                    if (offlineProduct != null)
                        offlineProduct.Unloaded = true;
                }
                return;
            }
            //DbProductIdFromBarcodeResult getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
            DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode);
            if (getProductResult == null)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
            {
                MessageBox.Show(@"Продукция не найдена по ШК!", @"Продукция не найдена",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                if (offlineProduct != null && fromBuffer)
                {
                    offlineProduct.ResultMessage = @"Продукция не найдена по ШК!";
                    offlineProduct.Unloaded = true;
                }
                return;
            }
            if (getProductResult.ProductKindId == 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty))
            {
                if (!(nomenclatureId == null || nomenclatureId == Guid.Empty || characteristicId == null || characteristicId == Guid.Empty || qualityId == null || qualityId == Guid.Empty || quantity == null))
                {
                    getProductResult.NomenclatureId = nomenclatureId;
                    getProductResult.CharacteristicId = characteristicId;
                    getProductResult.QualityId = qualityId;
                    getProductResult.CountProducts = (int) quantity;
                }
                else
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
            }
            //var addResult = Db.AddProductToOrder(DocOrderId, OrderType, Shared.PersonId, barcode, DocDirection);
            var addResult = Db.AddProductIdToOrder(DocOrderId, OrderType, Shared.PersonId, getProductResult.ProductId, DocDirection, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (addResult == null)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
                return;
            }
            if (addResult.AlreadyMadeChanges)
            {
                if (DeleteProductByBarcode(barcode) && fromBuffer)
                {
                    offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
                    if (offlineProduct != null)
                        offlineProduct.Unloaded = true;
                }
                return;
            }
            if (Shared.LastQueryCompleted == false)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
                return;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                var product = addResult.Product;
                //if (getProductResult.ProductKindId == null || getProductResult.ProductKindId != 3)
                //    Barcodes.Add(barcode);
                Barcodes.Add(new Barcodes
                {
                    Barcode = barcode,
                    ProductKindId = getProductResult.ProductKindId
                });
 
                if (product != null)
                    Invoke((UpdateOrderGridInvoker)(UpdateGrid),
                           new object[] { product.NomenclatureId, product.CharacteristicId, product.QualityId, product.NomenclatureName, 
                                product.ShortNomenclatureName, product.Quantity, true, product.CountProductSpools, product.CountProductSpoolsWithBreak, getProductResult.ProductKindId });                
            }
            else
            {
                if (!fromBuffer)
                    MessageBox.Show(addResult.ResultMessage);
            }
            if (!fromBuffer) return;
            offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            if (offlineProduct == null) return;
            offlineProduct.ResultMessage = addResult.ResultMessage;
            offlineProduct.Unloaded = true;
        }

        private bool DeleteProductByBarcode(string barcode)
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

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, bool add, int countProductSpools, int countProductSpoolsWithBreak, int? productKindId)
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
                good.CountProductSpools += countProductSpools;
                good.CountProductSpoolsWithBreak += countProductSpoolsWithBreak;
                good.CollectedQuantity += quantity;
                if (productKindId == null || productKindId != 3) Collected++;
            }
            else
            {
                good.CountProductSpools -= countProductSpools;
                good.CountProductSpoolsWithBreak -= countProductSpoolsWithBreak;
                good.CollectedQuantity -= quantity;
                if (productKindId == null || productKindId != 3) Collected--;
            }
            CountNomenclatureExceedingMaxPercentWithBreak = 0;
            foreach (DocNomenclatureItem item in NomenclatureList)
            {
                CountNomenclatureExceedingMaxPercentWithBreak += (item.SpoolWithBreakPercentColumn > Convert.ToDecimal(MaxAllowedPercentBreak)) ? 1 : 0;
            }
            gridDocOrder.UnselectAll();
            int index = NomenclatureList.IndexOf(good);
            gridDocOrder.CurrentRowIndex = index;
            gridDocOrder.Select(index);
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            UIServices.SetBusyState(this);
            AddProductByBarcode(edtNumber.Text, false);
            UIServices.SetNormalState(this);
        }

        private void AddOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
                {
                    DocId = DocOrderId,
                    Barcode = barcode,
                    PersonId = Shared.PersonId,
                    ResultMessage = "Не выгружено"
                });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocId == DocOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts,FileName);
        }

        private void AddOfflineProduct(string barcode, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int quantity)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
            {
                DocId = DocOrderId,
                Barcode = barcode,
                PersonId = Shared.PersonId,
                NomenclatureId = nomenclatureId,
                CharacteristicId = characteristicId,
                QualityId = qualityId,
                Quantity = quantity,
                ResultMessage = "Не выгружено"
            });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocId == DocOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts, FileName);
        }

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConInProgress});
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
            UIServices.SetNormalState(this);
        }

        private void SaveToXml<T>(ICollection<T> saveData, string fileName)
        {
            var ser = new XmlSerializer(typeof(List<T>));
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                try
                {
                    ser.Serialize(stream, saveData);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.InnerException.ToString());
                }
            }
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