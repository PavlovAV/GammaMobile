using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Xml;
using System.Text;


namespace gamma_mob
{
    public partial class DocOrderForm : BaseForm
    {
        enum ConnectState { ConInProgress, NoConInProgress, NoConnection }
      
        private string FileName { get; set; }

        private DocOrderForm()
        {
            InitializeComponent();
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\BarCodeList.xml";
        }

        private BindingSource BSource { get; set; }

        private BindingList<DocShipmentGood> GoodList { get; set; }

        private DocOrderForm(Form parentForm) : this()
        {
            ParentForm = parentForm;
        }

        

        private ImageList ImgList { get; set; }

        /// <summary>
        /// инициализация формы приказа
        /// </summary>
        /// <param name="docShipmentOrderId">ID Документа 1С</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        public DocOrderForm(Guid docShipmentOrderId, Form parentForm) : this(parentForm)
        {
            DocShipmentOrderId = docShipmentOrderId;
            var id = Db.GetDocId(docShipmentOrderId, Shared.PersonId);
            if (id == null)
            {
                MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                Close();
                return;
            }
            DocId = (Guid) id;

            if (!RefreshDocOrderGoods(docShipmentOrderId))
            {
                Close(); 
                return;
            }       

            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
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
                    Width = 39
                });
            gridDocShipmentOrder.TableStyles.Add(tableStyle);
            
            //Получение шк текущего документа(Если не получили, то и фиг с ними, будут лишние запросы к базе)
            Barcodes = Db.CurrentBarcodes(docShipmentOrderId, Shared.PersonId);
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnInspect.ImageIndex = (int) Images.Inspect;
            btnRefresh.ImageIndex = (int) Images.Refresh;
            BarcodeFunc = BarcodeReaction;

            //Подписка на событие восстановления связи
            ConnectionState.OnConnectionRestored += UnloadOfflineProducts;

            //Получение невыгруженных шк из файла
            if (!File.Exists(FileName)) return;
            var ser = new XmlSerializer(typeof(OfflineProduct));
            var reader = new StreamReader(FileName);
            var list = ser.Deserialize(reader) as List<OfflineProduct>;
            OfflineProducts = list ?? new List<OfflineProduct>();
        }

        private List<string> Barcodes { get; set; }

        /// <summary>
        /// Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        private void AddOfflineBarcode(string barCode)
        {
            ConnectionState.StartChecker();
            AddOfflineProduct(barCode);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
        }

        private void RemoveOfflineBarcode(string barcode)
        {
            RemoveOfflineProduct(barcode);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
        }

        private bool CheckConnectionMessage()
        {
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с сервером. Повторите попытку в зоне покрытия WiFi",
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ID текущего документа Gamma
        /// </summary>
        private Guid DocId { get; set; }

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

        private void RemoveOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) return;
            OfflineProducts.Remove(OfflineProducts.FirstOrDefault(p => p.Barcode == barcode));
            SaveToXml(OfflineProducts);
        }

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
                    break;
                case 2:
                    RefreshDocOrderGoods(DocShipmentOrderId);
                    break;
            }
        }

        private bool RefreshDocOrderGoods(Guid docShipmentOrderId)
        {
            var list = Db.DocShipmentGoods(docShipmentOrderId);
            if (!Shared.LastQueryCompleted || list == null)
            {
                MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            GoodList = list;
            BSource = new BindingSource { DataSource = GoodList };
            gridDocShipmentOrder.DataSource = BSource;
            return true;
        }

        private void BarcodeReaction(string barcode)
        {
            AddProductByBarcode(barcode, false);
        }

        /// <summary>
        /// Добавление продукта по штрихкоду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        /// <param name="fromBuffer">ШК из буфера невыгруженных</param>
        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            if (!ConnectionState.CheckConnection() && !fromBuffer)
            {
                AddOfflineBarcode(barcode);                
                return;
            }
            if (Barcodes.Contains(barcode))
            {
                DeleteProductByBarcode(barcode);
                return;
            }
            var addResult = Db.AddProduct(Shared.PersonId, barcode, DocShipmentOrderId);
            if (addResult.AlreadyMadeChanges)
            {
                DeleteProductByBarcode(barcode);
                return;
            }
            if (!Shared.LastQueryCompleted && !fromBuffer)
            {
                AddOfflineBarcode(barcode);
                return;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                if (fromBuffer)
                    OfflineProducts.Remove(OfflineProducts.First(p => p.Barcode == barcode));
                foreach (var productItem in addResult.ProductItems)
                {
                    Invoke((UpdateGridInvoker) (UpdateGrid),
                           new object[]
                               {productItem.NomenclatureId, productItem.CharacteristicId, productItem.Quantity, true});
                }
            }
            else
            {
                MessageBox.Show(addResult.ResultMessage);
            }
        }

        private void DeleteProductByBarcode(string barcode)
        {
            var dlgresult = MessageBox.Show(@"Хотите удалить данный продукт из приказа?", @"Удаление продукта",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dlgresult == DialogResult.Yes)
            {
                var deleteResult = Db.DeleteProductFromOrder(Shared.PersonId, barcode, DocShipmentOrderId);
                if (deleteResult.ResultMessage == "" && Shared.LastQueryCompleted)
                {
                    Barcodes.Remove(barcode);
                    foreach (var productItem in deleteResult.ProductItems)
                    {
                        Invoke((UpdateGridInvoker)(UpdateGrid),
                               new object[] { productItem.NomenclatureId, productItem.CharacteristicId, productItem.Quantity, false });
                    }
                }
            }
        }

        private delegate void UpdateGridInvoker(Guid nomenclatureId, Guid characteristicId, decimal quantity, bool add);

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, decimal quantity, bool add)
        {
            if (add)
                GoodList.First(g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId)
                        .CollectedQuantity += quantity;
            else
            {
                GoodList.First(g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId)
                        .CollectedQuantity -= quantity;
            }
        }

        private delegate void ConnectStateChangeInvoker(ConnectState state);

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text, false);
        }

        private List<OfflineProduct> OfflineProducts { get; set; }

        private void AddOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
                {
                    DocId = DocId,
                    Barcode = barcode
                });
            SaveToXml(OfflineProducts);
        }

        /// <summary>
        /// ID документа 1с
        /// </summary>
        private Guid DocShipmentOrderId { get; set; }

        /// <summary>
        /// Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            foreach (var offlineProduct in OfflineProducts)
            {
                AddProductByBarcode(offlineProduct.Barcode, false);
            }
            OfflineProducts.Clear();
            SaveToXml(OfflineProducts);
        }

        private void SaveToXml(List<OfflineProduct> offlineProducts)
        {
            var ser = new XmlSerializer(typeof (OfflineProduct));
            var writer = new XmlTextWriter(FileName, Encoding.UTF8);
            ser.Serialize(writer, offlineProducts);
        }
    }
}