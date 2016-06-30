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
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob
{
    public sealed partial class DocOrderForm : BaseForm
    {
        private DocOrderForm()
        {
            InitializeComponent();
        }

        private DocOrderForm(Form parentForm)
            : this()
        {
            ParentForm = parentForm;
        }

        /// <summary>
        ///     инициализация формы приказа
        /// </summary>
        /// <param name="docShipmentOrderId">ID Документа 1С</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="orderNumber">Номер приказа из 1С</param>
        public DocOrderForm(Guid docShipmentOrderId, Form parentForm, string orderNumber)
            : this(parentForm)
        {
            Text = "Приказ № " + orderNumber;
            DocShipmentOrderId = docShipmentOrderId;
            Guid? id = Db.GetDocId(docShipmentOrderId, Shared.PersonId);
            if (id == null)
            {
                MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                Close();
                return;
            }
            //DocId = (Guid)id;

            if (!RefreshDocOrderGoods(docShipmentOrderId))
            {
                Close();
                return;
            }

            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номенклатура",
                    MappingName = "NomenclatureName",
                    Width = 160
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Кол-во",
                    MappingName = "Quantity",
                    Width = 39
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Собрано",
                    MappingName = "CollectedQuantity",
                    Width = 39,
                    Format = "0.###"
                });
            gridDocShipmentOrder.TableStyles.Add(tableStyle);

            //Получение шк текущего документа(Если не получили, то и фиг с ними, будут лишние запросы к базе)
            Barcodes = Db.CurrentBarcodes(docShipmentOrderId, Shared.PersonId);
        }

        private string FileName { get; set; }
        private BindingSource BSource { get; set; }

        private BindingList<DocShipmentGood> GoodList { get; set; }

        private List<string> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }

        /// <summary>
        ///     ID документа 1с
        /// </summary>
        private Guid DocShipmentOrderId { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\BarCodeList.xml";
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnInspect.ImageIndex = (int) Images.Inspect;
            btnRefresh.ImageIndex = (int) Images.Refresh;
            btnUpload.ImageIndex = (int) Images.UploadToDb;
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

        protected override void FormClosing(object sender, CancelEventArgs e)
        {
            base.FormClosing(sender, e);
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
                    DocShipmentGood good = GoodList[gridDocShipmentOrder.CurrentRowIndex];
                    var form = new DocShipmentGoodProductsForm(DocShipmentOrderId, good.NomenclatureId, good.NomenclatureName,
                                                               good.CharacteristicId, this);
                    form.Show();
                    if (form.Enabled)
                        Hide();
                    break;
                case 2:
                    RefreshDocOrderGoods(DocShipmentOrderId);
                    break;
                case 3:
                    if (OfflineProducts.Count(p => p.DocShipmentOrderId == DocShipmentOrderId) > 0)
                        UnloadOfflineProducts();
                    break;
            }
        }

        private bool RefreshDocOrderGoods(Guid docShipmentOrderId)
        {
            BindingList<DocShipmentGood> list = Db.DocShipmentGoods(docShipmentOrderId);
            if (!Shared.LastQueryCompleted || list == null)
            {
                MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            GoodList = list;
            BSource = new BindingSource {DataSource = GoodList};
            gridDocShipmentOrder.DataSource = BSource;
            return true;
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker) (() => edtNumber.Text = barcode));
            AddProductByBarcode(barcode, false);
        }

        /// <summary>
        ///     Добавление продукта по штрихкоду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        /// <param name="fromBuffer">ШК из буфера невыгруженных</param>
        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            OfflineProduct offlineProduct;
                if (!ConnectionState.CheckConnection())
                {
                    if (!fromBuffer)
                        AddOfflineBarcode(barcode);
                    return;
                }
                if (Barcodes.Contains(barcode))
                {
                    if (DeleteProductByBarcode(barcode) && fromBuffer)
                    {
                        offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
                        if (offlineProduct != null)
                            offlineProduct.Unloaded = true;
                    }
                    return;
                }
                DbOperationProductResult addResult = Db.AddProduct(Shared.PersonId, barcode, DocShipmentOrderId);
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
                if (!Shared.LastQueryCompleted)
                {
                    if (!fromBuffer)
                        AddOfflineBarcode(barcode);
                    return;
                }
                if (addResult.ResultMessage == string.Empty)
                {
                    foreach (ProductItem productItem in addResult.ProductItems)
                    {
                        Invoke((UpdateGridInvoker) (UpdateGrid),
                               new object[]
                                   {productItem.NomenclatureId, productItem.CharacteristicId, productItem.Quantity, true});
                    }
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
                DbOperationProductResult deleteResult = Db.DeleteProductFromOrder(Shared.PersonId, barcode,
                                                                                  DocShipmentOrderId);
                if (deleteResult.ResultMessage == "" && Shared.LastQueryCompleted)
                {
                    result = true;
                    Barcodes.Remove(barcode);
                    foreach (ProductItem productItem in deleteResult.ProductItems)
                    {
                        Invoke((UpdateGridInvoker) (UpdateGrid),
                               new object[]
                                   {
                                       productItem.NomenclatureId, productItem.CharacteristicId, productItem.Quantity,
                                       false
                                   });
                    }
                }
            }
            else result = true;
            return result;
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, decimal quantity, bool add)
        {
            DocShipmentGood good =
                GoodList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId);
            if (good == null) return;
            if (add)
                good.CollectedQuantity += quantity;
            else
                good.CollectedQuantity -= quantity;
            gridDocShipmentOrder.UnselectAll();
            int index = GoodList.IndexOf(good);
            gridDocShipmentOrder.CurrentRowIndex = index;
            gridDocShipmentOrder.Select(index);
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text, false);
        }

        private void AddOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
                {
                    DocShipmentOrderId = DocShipmentOrderId,
                    Barcode = barcode,
                    PersonId = Shared.PersonId,
                    ResultMessage = "Не выгружено"
                });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocShipmentOrderId == DocShipmentOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts);
        }

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConInProgress});
            string message = "";
            string resultMessage = "";
            foreach (
                OfflineProduct offlineProduct in
                    OfflineProducts.Where(p => p.DocShipmentOrderId == DocShipmentOrderId).ToList())
            {
                AddProductByBarcode(offlineProduct.Barcode, true);
            }
            List<OfflineProduct> tempList = OfflineProducts.Where(p => p.Unloaded
                                                                       && p.DocShipmentOrderId == DocShipmentOrderId)
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
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocShipmentOrderId == DocShipmentOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            if (OfflineProducts.Count(p => p.DocShipmentOrderId == DocShipmentOrderId) > 0)
            {
                ConnectionState.StartChecker();
            }
            SaveToXml(OfflineProducts);
        }

        private void SaveToXml(ICollection offlineProducts)
        {
            var ser = new XmlSerializer(typeof (List<OfflineProduct>));
            using (var stream = new FileStream(FileName, FileMode.Create))
            {
                try
                {
                    ser.Serialize(stream, offlineProducts);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.InnerException.ToString());
                }
            }
        }

        private void gridDocShipmentOrder_CurrentCellChanged(object sender, EventArgs e)
        {
            gridDocShipmentOrder.Select(gridDocShipmentOrder.CurrentRowIndex);
        }

        private delegate void ConnectStateChangeInvoker(ConnectState state);

        private delegate void UpdateGridInvoker(Guid nomenclatureId, Guid characteristicId, decimal quantity, bool add);
    }
}