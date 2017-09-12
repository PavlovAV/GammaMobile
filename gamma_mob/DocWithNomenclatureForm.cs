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
            FileName = FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + fileName;
            DocOrderId = docOrderId;
            DocDirection = docDirection;
            
            if (!RefreshDocOrderGoods(docOrderId))
            {
                MessageBox.Show(@"Не удалось получить информацию о документе");
                Close();
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

            MaxAllowedPercentBreak = maxAllowedPercentBreak;

            Barcodes = Db.CurrentBarcodes(DocOrderId, DocDirection);
            Collected = Barcodes.Count;
            CountProductSpoolsWithBreak = Db.CurrentCountProductSpools(DocOrderId, true, DocDirection);
            CountProductSpools = Db.CurrentCountProductSpools(DocOrderId, false, DocDirection);
        }

        private OrderType OrderType { get; set; }
        private DocDirection DocDirection { get; set; }

        private string FileName { get; set; }
        private BindingSource BSource { get; set; }

        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        private List<string> Barcodes { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }

        private int MaxAllowedPercentBreak { get; set; }

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
        private int _countProductSpools;

        private int CountProductSpools
        {
            get { return _countProductSpools; }
            set
            {
                _countProductSpools = value;
                if (CountProductSpools == 0)
                    lblPercentBreak.Text = "";
                else
                    lblPercentBreak.Text = (100 * CountProductSpoolsWithBreak / CountProductSpools).ToString(CultureInfo.InvariantCulture);
            }
        }
        
        private int _countProductSpoolsWithBreak;

        private int CountProductSpoolsWithBreak
        {
            get { return _countProductSpoolsWithBreak; }
            set
            {
                _countProductSpoolsWithBreak = value;
                if (CountProductSpools == 0)
                    lblPercentBreak.Text = "";
                else
                    lblPercentBreak.Text = (100 * CountProductSpoolsWithBreak / CountProductSpools).ToString(CultureInfo.InvariantCulture);
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
                        MessageBox.Show(@"Нет связи с базой", @"Ошибка связи");
                        return;
                    }
                    var nomenclatureItem = NomenclatureList[gridDocOrder.CurrentRowIndex];
                    var resultMessage = Db.FindDocOrderItemPosition(DocOrderId, nomenclatureItem.LineNumber)
                                        ?? "Не удалось получить информацию о расположении продукции";
                    MessageBox.Show(resultMessage, @"Расположение продукции", MessageBoxButtons.OK,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button1);
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
                MessageBox.Show(@"Нет связи с сервером");
                return;
            }
            var good = NomenclatureList[gridDocOrder.CurrentRowIndex];
            var form = new DocShipmentGoodProductsForm(DocOrderId, good.NomenclatureId, good.NomenclatureName,
                                                       good.CharacteristicId, this, DocDirection);
            if (!form.IsDisposed)
            {
                form.Show();
                if (form.Enabled)
                    Hide();
            }
        }


        private bool RefreshDocOrderGoods(Guid docId)
        {
            BindingList<DocNomenclatureItem> list = Db.DocNomenclatureItems(docId, OrderType, DocDirection);
            if (!Shared.LastQueryCompleted || list == null)
            {
               // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            NomenclatureList = list;
            if (BSource == null)
                BSource = new BindingSource {DataSource = NomenclatureList};
            else
            {
                BSource.DataSource = NomenclatureList;
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
                if (DeleteProductByBarcode(barcode) && fromBuffer)
                {
                    if (offlineProduct != null)
                        offlineProduct.Unloaded = true;
                }
                return;
            }
            var addResult = Db.AddProductToOrder(DocOrderId, OrderType, Shared.PersonId, barcode, DocDirection);
            if (addResult == null)
            {
                if (!fromBuffer) 
                    AddOfflineBarcode(barcode);
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
            if (!Shared.LastQueryCompleted)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                var product = addResult.Product;
                Barcodes.Add(barcode);
                if (product != null)
                    Invoke((UpdateOrderGridInvoker)(UpdateGrid),
                           new object[] { product.NomenclatureId, product.CharacteristicId, product.NomenclatureName, 
                                product.ShortNomenclatureName, product.Quantity, true, product.CountProductSpools, product.CountProductSpoolsWithBreak });                
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
                if (deleteResult.ResultMessage == "" && Shared.LastQueryCompleted)
                {
                    result = true;
                    Barcodes.Remove(barcode);
                    var product = deleteResult.Product;
                    if (product != null)
                    {
                        Invoke((UpdateOrderGridInvoker) (UpdateGrid),
                               new object[]
                                   {
                                       product.NomenclatureId, product.CharacteristicId, product.NomenclatureName, product.ShortNomenclatureName,
                                            product.Quantity, 
                                       false, product.CountProductSpools, product.CountProductSpoolsWithBreak
                                   });
                    }
                }                
            }
            else result = true;
            return result;
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, bool add, int countProductSpools, int countProductSpoolsWithBreak)
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
                        CollectedQuantity = 0,
                        Quantity = "0",
                        CountProductSpools = 0,
                        CountProductSpoolsWithBreak = 0
                    };
                NomenclatureList.Add(good);
                BSource.DataSource = NomenclatureList;
            }
            if (add)
            {
                good.CollectedQuantity += quantity;
                Collected++;
                CountProductSpools += countProductSpools;
                CountProductSpoolsWithBreak += countProductSpoolsWithBreak;
            }
            else
            {
                good.CollectedQuantity -= quantity;
                Collected--;
                CountProductSpools -= countProductSpools;
                CountProductSpoolsWithBreak -= countProductSpoolsWithBreak;
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
                    OfflineProducts.Where(p => p.DocId == DocOrderId).ToList())
            {
                AddProductByBarcode(offlineProduct.Barcode, true);
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

        private void gridDocOrder_CurrentCellChanged(object sender, EventArgs e)
        {
            gridDocOrder.Select(gridDocOrder.CurrentRowIndex);
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
                int value;
                if (label.Text == "")
                    value = 0;
                else
                {
                    try
                    {
                        value = Int32.Parse(label.Text);
                    }
                    catch (ArgumentNullException ex)
                    {
                        value = 0;
                    }
                }
                if (MaxAllowedPercentBreak < value)
                {
                    lblPercentBreak.BackColor = Color.Red;
                    lblPercentBreak.ForeColor = Color.White;
                    lblBreak.BackColor = Color.Red;
                    lblBreak.ForeColor = Color.White;
                }
                else
                {
                    lblPercentBreak.BackColor = Color.FromArgb(192,192,192);
                    lblPercentBreak.ForeColor = Color.Black;
                    lblBreak.BackColor = Color.FromArgb(192, 192, 192);
                    lblBreak.ForeColor = Color.Black;
                }
            }
        }

    }
}