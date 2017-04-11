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
                    Width = 156
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Кол-во",
                    MappingName = "CollectedQuantity",
                    Width = 38
                });
            gridInventarisation.TableStyles.Add(tableStyle);

            Barcodes = Db.CurrentInventarisationBarcodes(DocInventarisationId);
            Collected = Barcodes.Count;
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
            if (!Shared.LastQueryCompleted || list == null)
            {
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            NomenclatureList = list;
            if (BSource == null)
                BSource = new BindingSource { DataSource = NomenclatureList };
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridInventarisation.DataSource = BSource;
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
            var offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            if (!ConnectionState.CheckConnection())
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (Barcodes.Contains(barcode))
            {
                MessageBox.Show(@"Данный продукт уже отмечен");
                return;
            }
            var addResult = Db.AddProductToInventarisation(DocInventarisationId, barcode);
            if (addResult == null)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (addResult.AlreadyMadeChanges && !fromBuffer)
            {
                MessageBox.Show(@"Данный продукт уже отмечен");
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
                Barcodes.Add(barcode);
                if (product != null)
                    Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                           new object[] { product.NomenclatureId, product.CharacteristicId, product.NomenclatureName,
                               product.ShortNomenclatureName, product.Quantity });
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

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity)
        {
            DocNomenclatureItem good =
                NomenclatureList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId);
            if (good == null)
            {
                good = new DocNomenclatureItem
                {
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    NomenclatureName = nomenclatureName,
                    ShortNomenclatureName = shortNomenclatureName,
                    CollectedQuantity = 0
                };
                NomenclatureList.Add(good);
                BSource.DataSource = NomenclatureList;
            }
            good.CollectedQuantity += quantity;
            Collected++;
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

        private List<string> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }

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
            }
        }

        private void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером");
                return;
            }
            var good = NomenclatureList[gridInventarisation.CurrentRowIndex];
            var form = new DocInventarisationNomenclatureProductsForm(DocInventarisationId, good.NomenclatureId, good.NomenclatureName,
                                                       good.CharacteristicId, this);
            if (!form.IsDisposed)
            {
                form.Show();
                if (form.Enabled)
                    Hide();
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            UIServices.SetBusyState(this);
            AddProductByBarcode(edtNumber.Text, false);
            UIServices.SetNormalState(this);
        }
    }   
}