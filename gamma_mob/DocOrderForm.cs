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
        /// <param name="docOrderId">ID Документа (для отгрузки - ID 1c, для заказа на перемещение ID gamma)</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="orderNumber">Номер приказа из 1С</param>
        /// <param name="docType">Тип документа(отгрузка, перемещение)</param>
        public DocOrderForm(Guid docOrderId, Form parentForm, string orderNumber, DocType docType)
            : this(parentForm)
        {
            DocType = docType;
            switch (docType)
            {
                    case DocType.DocShipmentOrder:
                        Text = "Приказ № " + orderNumber;
                        break;
                    case DocType.DocMovementOrder:
                        Text = "Заказ № " + orderNumber;
                        break;
            }
            
            DocOrderId = docOrderId;
            var id = Db.GetDocId(docOrderId, Shared.PersonId, DocType);
            if (id == null || !RefreshDocOrderGoods(docOrderId))
            {
                MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                Close();
                return;
            }

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
                    MappingName = "Quantity",
                    Width = 38
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Собрано",
                    MappingName = "CollectedQuantity",
                    Width = 38,
                    Format = "0.###"
                });
            gridDocOrder.TableStyles.Add(tableStyle);

            //Получение штрих-кодов текущего документа(Если не получили, то и фиг с ними, будут лишние запросы к базе)
            Barcodes = Db.CurrentBarcodes(docOrderId, Shared.PersonId, DocType);
        }

        private DocType DocType { get; set; }

        private string FileName { get; set; }
        private BindingSource BSource { get; set; }

        private BindingList<DocNomenclatureItem> GoodList { get; set; }

        private List<string> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }

        /// <summary>
        ///     ID документа 1с
        /// </summary>
        private Guid DocOrderId { get; set; }

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
                    DocNomenclatureItem good = GoodList[gridDocOrder.CurrentRowIndex];
                    if (!ConnectionState.CheckConnection())
                    {
                        MessageBox.Show(@"Нет связи с сервером");
                        return;
                    }
                    var form = new DocShipmentGoodProductsForm(DocOrderId, good.NomenclatureId, good.NomenclatureName,
                                                               good.CharacteristicId, this);
                    if (!form.IsDisposed)
                    {
                        form.Show();
                        if (form.Enabled)
                            Hide();
                    }
                    break;
                case 2:
                    RefreshDocOrderGoods(DocOrderId);
                    break;
                case 3:
                    if (OfflineProducts.Count(p => p.DocShipmentOrderId == DocOrderId) > 0)
                        UnloadOfflineProducts();
                    break;
            }
        }

        private bool RefreshDocOrderGoods(Guid docShipmentOrderId)
        {
            BindingList<DocNomenclatureItem> list = Db.DocNomenclatureItems(docShipmentOrderId, DocType);
            if (!Shared.LastQueryCompleted || list == null)
            {
               // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            GoodList = list;
            if (BSource == null)
                BSource = new BindingSource {DataSource = GoodList};
            else
            {
                BSource.DataSource = GoodList;
            }
            gridDocOrder.DataSource = BSource;
            return true;
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker) (() => edtNumber.Text = barcode));
            UIServices.SetBusyState(this);
            AddProductByBarcode(barcode, false);
            UIServices.SetNormalState(this);
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
                DbOperationProductResult addResult = Db.AddProduct(Shared.PersonId, barcode, DocOrderId, DocType);
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
                                                                                  DocOrderId, DocType);
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
            DocNomenclatureItem good =
                GoodList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId);
            if (good == null) return;
            if (add)
                good.CollectedQuantity += quantity;
            else
                good.CollectedQuantity -= quantity;
            gridDocOrder.UnselectAll();
            int index = GoodList.IndexOf(good);
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
                    DocShipmentOrderId = DocOrderId,
                    Barcode = barcode,
                    PersonId = Shared.PersonId,
                    ResultMessage = "Не выгружено"
                });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocShipmentOrderId == DocOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts);
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
                    OfflineProducts.Where(p => p.DocShipmentOrderId == DocOrderId).ToList())
            {
                AddProductByBarcode(offlineProduct.Barcode, true);
            }
            List<OfflineProduct> tempList = OfflineProducts.Where(p => p.Unloaded
                                                                       && p.DocShipmentOrderId == DocOrderId)
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
                (() => lblBufferCount.Text = OfflineProducts.Count(p => p.DocShipmentOrderId == DocOrderId)
                                                            .ToString(CultureInfo.InvariantCulture)));
            if (OfflineProducts.Count(p => p.DocShipmentOrderId == DocOrderId) > 0)
            {
                ConnectionState.StartChecker();
            }
            SaveToXml(OfflineProducts);
            UIServices.SetNormalState(this);
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
            gridDocOrder.Select(gridDocOrder.CurrentRowIndex);
        }

        private delegate void ConnectStateChangeInvoker(ConnectState state);

        private delegate void UpdateGridInvoker(Guid nomenclatureId, Guid characteristicId, decimal quantity, bool add);
    }
}