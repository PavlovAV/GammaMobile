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
using gamma_mob.Dialogs;
using gamma_mob.Models;

namespace gamma_mob
{
    public partial class DocAcceptForm : BaseForm
    {
        private DocAcceptForm()
        {
            InitializeComponent();
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                       @"\DocAcceptBarcodes.xml";
        }

        /// <summary>
        ///     инициализация формы приказа
        /// </summary>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="movementType">Тип перемещения(приемка на склад с передела или со склада на склад)</param>
        public DocAcceptForm(Form parentForm, MovementType movementType)
            : this()
        {
            MovementType = movementType;
            ParentForm = parentForm;

            AcceptedProducts = new BindingList<ShortProduct>();

            var bsource = new BindingSource {DataSource = AcceptedProducts};

            gridDocAccept.DataSource = bsource;

            var tableStyle = new DataGridTableStyle {MappingName = bsource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номер",
                    MappingName = "Number",
                    Width = 94
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номенклатура",
                    MappingName = "NomenclatureName",
                    Width = 100
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Кол-во",
                    MappingName = "Quantity",
                    Width = 38,
                    Format = "0.###"
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Склад",
                    MappingName = "PlaceTo",
                    Width = 40
                });
            gridDocAccept.TableStyles.Add(tableStyle);
        }

        private MovementType MovementType { get; set; }

        private List<OfflineProduct> OfflineProducts { get; set; }
        private string FileName { get; set; }
        private BindingList<ShortProduct> AcceptedProducts { get; set; }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text, false);
        }

        /// <summary>
        ///     Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        private void AddOfflineBarcode(string barCode)
        {
            ConnectionState.StartChecker();
            AddOfflineProduct(barCode);
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConnection});
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
            }
        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
            ConnectionState.OnConnectionRestored -= UnloadOfflineProducts;
        }

        private void AddOfflineProduct(string barcode)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            OfflineProducts.Add(new OfflineProduct
                {
                    Barcode = barcode,
                    PersonId = Shared.PersonId
                });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count.ToString(CultureInfo.InvariantCulture)));
            SaveToXml(OfflineProducts);
        }

        /// <summary>
        ///     Сохранение буферных шк в файл
        /// </summary>
        /// <param name="offlineProducts"></param>
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

        /// <summary>
        ///     Добавление продукта по штрихкоду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        /// <param name="fromBuffer">ШК из буфера невыгруженных</param>
        private void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            OfflineProduct offlineProduct = null;
            if (fromBuffer)
                offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            EndPointInfo endPointInfo;
            if (!ConnectionState.CheckConnection())
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode);
                return;
            }
            if (MovementType == MovementType.Accept)
            {
                ShortProduct acceptedProduct = AcceptedProducts.FirstOrDefault(p => p.Barcode == barcode);
                if (acceptedProduct != null)
                {
                    CancelLastMovement(acceptedProduct.Barcode, acceptedProduct.SourcePlace);
                    return;
                }
            }
            using (var form = new ChooseEndPointDialog())
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK) return;
                endPointInfo = form.EndPointInfo;
            }
                Cursor.Current = Cursors.WaitCursor;
                AcceptProductResult acceptResult = null;
                switch (MovementType)
                {
                    case MovementType.Accept:
                        acceptResult = Db.AcceptProduct(Shared.PersonId, barcode, endPointInfo);
                        break;
                    case MovementType.Movement:

                        break;
                }
                if (!Shared.LastQueryCompleted)
                {
                    if (!fromBuffer)
                        AddOfflineBarcode(barcode);
                    return;
                }
            if (acceptResult == null)
            {
                MessageBox.Show(@"Не удалось переместить продукцию на склад");
                return;
            }
            if (acceptResult.AlreadyAccepted)
                {
                    CancelLastMovement(barcode, acceptResult.SourcePlace);
                    return;
                }
                if (acceptResult.ResultMessage == string.Empty)
                {
                    if (fromBuffer)
                    {
                        if (offlineProduct != null)
                            offlineProduct.Unloaded = true;
                    }
                    Invoke((UpdateGridInvoker) (UpdateGrid),
                           new object[]
                               {
                                   acceptResult.NomenclatureName, acceptResult.Number, acceptResult.Quantity
                                   , true, barcode, acceptResult.SourcePlace
                               });
                }
                else
                {
                    if (!fromBuffer)
                        MessageBox.Show(acceptResult.ResultMessage);
                    else
                    {
                        if (offlineProduct != null)
                        {
                            offlineProduct.Unloaded = true;
                            offlineProduct.ResultMessage = acceptResult.ResultMessage;
                        }
                    }
                }
            
        }

        private void CancelLastMovement(string barcode, string sourcePlace)
        {
            OfflineProduct offlineProduct = null;
            if (OfflineProducts != null)
                offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            DialogResult dlgResult = MessageBox.Show(
                string.Format("Вернуть продукт с шк {0} на передел {1}", barcode, sourcePlace),
                @"Возврат продукта", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (dlgResult == DialogResult.Yes)
            {
                if (Db.CancelLastMovement(barcode))
                {
                    if (offlineProduct != null) offlineProduct.Unloaded = true;
                    Invoke(
                        (MethodInvoker)
                        (() => AcceptedProducts.Remove(AcceptedProducts.FirstOrDefault(p => p.Barcode == barcode))));
                }
                else
                {
                    MessageBox.Show(@"Связь с базой потеряна, не удалось вернуть продукт на передел", @"Ошибка связи");
                    if (offlineProduct == null) AddOfflineBarcode(barcode);
                }
            }
            else
            {
                if (offlineProduct != null) offlineProduct.Unloaded = true;
            }
        }

        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        private void UnloadOfflineProducts()
        {
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConInProgress});
            string message = "";
            string resultMessage = "";
            foreach (OfflineProduct offlineProduct in OfflineProducts.ToList())
            {
                AddProductByBarcode(offlineProduct.Barcode, true);
            }
            List<OfflineProduct> tempList = OfflineProducts.Where(p => p.Unloaded).ToList();
            int unloaded = tempList.Count(p => p.ResultMessage == string.Empty);
            if (unloaded > 0)
                message += @"Связь восстановлена. Принято на склад " + unloaded + " изделий" + Environment.NewLine;
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
                (() => lblBufferCount.Text = OfflineProducts.Count.ToString(CultureInfo.InvariantCulture)));
            if (OfflineProducts.Count > 0)
                ConnectionState.StartChecker();
            SaveToXml(OfflineProducts);
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
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
        }

        private void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker) (() => edtNumber.Text = barcode));
            AddProductByBarcode(barcode, false);
        }

        private void UpdateGrid(string nomenclatureName, string number, decimal quantity, bool add, string barcode,
                                string sourcePlace)
        {
            if (!add)
                AcceptedProducts.Remove(AcceptedProducts.FirstOrDefault(p => p.Number == number));
            else
                AcceptedProducts.Insert(0, new ShortProduct
                    {
                        NomenclatureName = nomenclatureName,
                        Number = number,
                        Quantity = quantity,
                        Barcode = barcode,
                        SourcePlace = sourcePlace
                    });
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    if (OfflineProducts.Count > 0)
                        UnloadOfflineProducts();
                    break;
            }
        }

        private delegate void ConnectStateChangeInvoker(ConnectState state);

        private class ShortProduct
        {
            public string Number { get; set; }
            public string NomenclatureName { get; set; }
            public decimal Quantity { get; set; }
            public string SourcePlace { get; set; }
            public string PlaceTo { get; set; }
            public string Barcode { get; set; }
        }

        private delegate void UpdateGridInvoker(string nomenclatureName, string number
                                                , decimal quantity, bool add, string barcode, string sourcePlace);
    }
}