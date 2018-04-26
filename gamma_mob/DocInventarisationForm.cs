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

namespace gamma_mob
{
    public sealed partial class DocInventarisationForm : BaseForm
    {
        private int _collected;

        private DocInventarisationForm()
        {
            InitializeComponent();
        }

        private DocInventarisationForm(Form parentForm): this()
        {
            ParentForm = parentForm;
            OfflineProducts = new List<OfflineProduct>();
        }

        /// <summary>
        ///     инициализация формы
        /// </summary>
        /// <param name="docInventarisationId">ID Документа</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="docNumber">Номер инвентаризации</param>
        public DocInventarisationForm(Guid docInventarisationId, Form parentForm, string docNumber)
            : this(parentForm)
        {
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "inventarisation.xml";
            DocInventarisationId = docInventarisationId;
            if (!RefreshProducts(docInventarisationId))
            {
                MessageBox.Show(@"Не удалось получить информацию о документе");
                Close();
                return;
            }
            Text = "Инвентаризация № " + docNumber;
            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
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
            Barcodes1C = Db.GetBarcodes1C();
        }

        private int Collected
        {
            get { return _collected; }
            set
            {
                _collected = value;
                lblCollected.Text = Collected.ToString(CultureInfo.InvariantCulture);
            }
        }

        private BindingSource BSource { get; set; }

        private bool RefreshProducts(Guid docInventarisationId)
        {
            BindingList<DocNomenclatureItem> list = Db.InventarisationProducts(docInventarisationId);
            if (!Shared.LastQueryCompleted)// || list == null)
            {
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            if (list != null)
                NomenclatureList = list;
            else
                NomenclatureList = new BindingList<DocNomenclatureItem>();
            if (BSource == null)
                BSource = new BindingSource { DataSource = NomenclatureList };
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridInventarisation.DataSource = BSource;

            Barcodes = Db.GetCurrentInventarisationBarcodes(DocInventarisationId);
            //Barcodes = Db.CurrentInventarisationBarcodes(DocInventarisationId);
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
            return true;
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
            ConnectionState.OnConnectionRestored += UnloadOfflineProducts;

            //Получение невыгруженных шк из файла
            if (!File.Exists(FileName)) return;
            var ser = new XmlSerializer(typeof(List<OfflineProduct>));
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

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
            string message = "";
            string resultMessage = "";
            foreach (
                OfflineProduct offlineProduct in
                    OfflineProducts.Where(p => p.DocId == DocInventarisationId).ToList())
            {
                AddProductByBarcode(offlineProduct.Barcode, true);
            }
            List<OfflineProduct> tempList = OfflineProducts.Where(p => p.Unloaded
                                                                       && p.DocId == DocInventarisationId)
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
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocId == DocInventarisationId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            if (OfflineProducts.Count(p => p.DocId == DocInventarisationId) > 0)
            {
                ConnectionState.StartChecker();
            }
            SaveToXml(OfflineProducts);
            UIServices.SetNormalState(this);
        }

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

        private Guid DocInventarisationId { get; set; }

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
        }


        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            AddProductByBarcode(barcode, fromBuffer, new Guid(), new Guid(), new Guid(), null);
        }
        
        private void AddProductByBarcode(string barcode, bool fromBuffer, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int? quantity)
        {
            var offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            if (!ConnectionState.CheckConnection())
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (Barcodes.Any(b => b.Barcode == barcode & (b.ProductKindId == null || b.ProductKindId != 3)))
            {
                if (offlineProduct == null)
                    MessageBox.Show(@"Данный продукт уже отмечен");
                else
                {
                    offlineProduct.ResultMessage = @"Данный продукт уже отмечен";
                    offlineProduct.Unloaded = true;
                };
                return;
            }
            DbProductIdFromBarcodeResult getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
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
                    getProductResult.CountProducts = (int)quantity;
                }
                else
                {
                    if (getProductResult.NomenclatureId == null || getProductResult.NomenclatureId == Guid.Empty || getProductResult.CharacteristicId == null || getProductResult.CharacteristicId == Guid.Empty || getProductResult.QualityId == null || getProductResult.QualityId == Guid.Empty)
                    {
                        using (var form = new ChooseNomenclatureCharacteristicDialog(barcode, Barcodes1C))
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
            //var addResult = Db.AddProductToInventarisation(DocInventarisationId, barcode);
            var addResult = Db.AddProductIdToInventarisation(DocInventarisationId, getProductResult.ProductId, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (addResult == null)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (addResult.AlreadyMadeChanges && !fromBuffer)
            {
                if (offlineProduct == null)
                    MessageBox.Show(@"Данный продукт уже отмечен");
                else
                {
                    offlineProduct.ResultMessage = @"Данный продукт уже отмечен";
                    offlineProduct.Unloaded = true;
                };
                return;
            }
            if (!Shared.LastQueryCompleted)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (addResult.ResultMessage == string.Empty && !addResult.AlreadyMadeChanges)
            {
                var product = addResult.Product;
                //Barcodes.Add(barcode);
                Barcodes.Add(new Barcodes
                {
                    Barcode = barcode,
                    ProductKindId = getProductResult.ProductKindId
                });
                if (product != null)
                    Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                           new object[] { product.NomenclatureId, product.CharacteristicId, product.QualityId, product.NomenclatureName,
                               product.ShortNomenclatureName, product.Quantity, getProductResult.ProductKindId });
            }
            else
            {
                if (!fromBuffer)
                    MessageBox.Show(addResult.ResultMessage);
            }
            if (!fromBuffer) return;
            if (offlineProduct == null) return;
            offlineProduct.ResultMessage = addResult.ResultMessage;
            offlineProduct.Unloaded = true;
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
            good.CollectedQuantity += quantity;
            if (productKindId == null || productKindId != 3) Collected++;
            gridInventarisation.UnselectAll();
            int index = NomenclatureList.IndexOf(good);
            gridInventarisation.CurrentRowIndex = index;
            gridInventarisation.Select(index);
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            UIServices.SetBusyState(this);
            AddProductByBarcode(barcode, false);
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
            }
        }

        //private List<string> Barcodes { get; set; }
        private List<Barcodes> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }
        private List<Barcodes1C> OfflineBarcodes1C { get; set; }
        private BindingList<ChooseNomenclatureItem> Barcodes1C { get; set; }

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
                    RefreshProducts(DocInventarisationId);
                    break;
                case 3:
                    if (OfflineProducts.Count(p => p.DocId == DocInventarisationId) > 0)
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
                var form = new DocInventarisationProductsForm(DocInventarisationId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this, new RefreshDocProductDelegate(RefreshDocInventarisation));
                if (!form.IsDisposed)
                {
                    form.Show();
                    if (form.Enabled)
                        Hide();
                }
            }
        }

        private void RefreshDocInventarisation(Guid docId)
        {
            if (!RefreshProducts(docId))
            {
                MessageBox.Show(@"Не удалось получить информацию о документе");
                Close();
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

    }   
}