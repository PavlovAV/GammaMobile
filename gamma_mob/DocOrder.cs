using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;



namespace gamma_mob
{
    public partial class DocOrder : Form
    {
        enum ConnectState { ConInProgress, NoConInProgress, NoConnection }
        private SqlWork SqlBase { get; set; }
        private long DocMobGroupPackOrderId { get; set; }

        private Scanner Scanner { get; set; }
        private List<string> _barcodes = new List<string>();
        private readonly string _fileName;
        public Form ParentForm { private get; set; }

        public DocOrder()
        {
            InitializeComponent();
            DocOrders.ConnectionString = GammaDataSet.ConnectionString;
            DocOrderGroupPacks.ConnectionString = GammaDataSet.ConnectionString;
            _fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\BarCodeList.txt";
        }
        

        //Получение ГУ при сканировании или по кнопке "поиск"
        private void GetGroupPackData(string barCode)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowConnection((int)ConnectState.ConInProgress);
            if (ConnectionState.CheckConnection() && AcceptChangesToBase()) // Проверка связи с сервером и выгрузка изменений, если есть
            {
                var groupPackInfo = SqlBase.GetGroupPackInfo(barCode); // Получение информации по ШК
                if (AddNewGroupPack(groupPackInfo, true))
                {
                    SetFormValues(groupPackInfo);
                    ShowConnection(ConnectState.NoConInProgress);
                }
                else ShowConnection(ConnectState.NoConnection);
            }
            else
            {
                OfflineBarcode(barCode);
            }
            Cursor.Current = Cursors.Default;
        }

        private void OfflineBarcode(string barCode)
        {
            ConnectionState.StartChecker();
            AddBarcodeToBuffer(barCode);
            ShowConnection(ConnectState.NoConnection);
        }

        //Заполнение элементов формы
        private void SetFormValues(GroupPackInfo groupPackInfo) 
        {
            edtWeight.Text = groupPackInfo.Weight.ToString(CultureInfo.InvariantCulture);
            edtGrossWeight.Text = groupPackInfo.GrossWeight.ToString(CultureInfo.InvariantCulture);
            //Вывод номенклатуры
            edtNomenclature.Items.Clear();
            string[] nomenclatureValues = groupPackInfo.NomenclatureName.Split(new[]{'/'});
            string row = "";
            foreach (string nomValue in nomenclatureValues)
            {
                if ((row.Length + nomValue.Length) < 20)
                    row = row + nomValue + "/" ;
                else
                {
                    edtNomenclature.Items.Add(row);
                    row = nomValue + "/";
                }
            }
            edtNomenclature.Items.Add(row); // Добавление последней строки номенлатуры

            if (groupPackInfo.Found)
            {
                var sumWeight = DocOrderGroupPacks.GetNomenclatureSumWeight(groupPackInfo.GroupPackId, DocMobGroupPackOrderId);
                edtSummaryWeight.Text = sumWeight == null ? "0" : sumWeight.ToString();
            }
            else edtSummaryWeight.Text = "0";
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            edtNomenclature.Items.Clear();
            edtWeight.Text = "";
            edtGrossWeight.Text = "";
            edtSummaryWeight.Text = "";
            GetGroupPackData(edtBarCode.Text);
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            { 
                case 0:
                    Close();
                    break;
                case 1:
                    AddNewDoc();
                    break;
                case 2:
                    if (!CheckConnectionMessage()) return;
                    var findDocOrder = new FindDocOrder(docMobGroupPackOrdersBindingSource);
                    findDocOrder.ShowDialog();
                    var row = (DataRowView)docMobGroupPackOrdersBindingSource.Current;
                    Int64 orderId = Convert.ToInt64(row["DocMobGroupPackOrderID"]);
                    if (DocMobGroupPackOrderId != orderId)
                    {
                        try
                        {
                            DocOrderGroupPacks.FillByDocOrderID(GammaBase.DocMobGroupPackOrderGroupPacks, orderId);
                            DocMobGroupPackOrderId = orderId;
                        }
                        catch (SqlException)
                        {
                            docMobGroupPackOrdersBindingSource.Position = docMobGroupPackOrdersBindingSource.Find("DocMobGroupPackOrderID", DocMobGroupPackOrderId);
                            MessageBox.Show(@"Нет связи с сервером. Повторите попытку в зоне покрытия WiFi",
                            @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        }
                    }
                    break;
                case 3:
                    if (!CheckConnectionMessage()) return;
                    var orderInfo = new OrderInfo(DocMobGroupPackOrderId);
                    orderInfo.ShowDialog();
                    break;
            }
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

        private void RetryNewDocument()
        {
            if (MessageBox.Show(@"Нет связи с БД(ТСД не в сети). Повторите попытку в зоне видимости WiFi",
                 @"Отсутствие WiFi соединения", MessageBoxButtons.RetryCancel,
                 MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
            {
                GammaBase.RejectChanges();
                AddNewDoc();
            }
            else
            {
                GammaBase.RejectChanges();
            }
        }

        private void AddNewDoc() //Добавление нового документа
        {
            if (!ConnectionState.CheckConnection())
            {
                RetryNewDocument();
                return;
            }
            var docOrderRow = (GammaDataSet.DocMobGroupPackOrdersRow)GammaBase.DocMobGroupPackOrders.NewRow();
            docOrderRow["SerialNumber"] = Scanner.SerialNumber;
            GammaBase.AcceptChanges();
            GammaBase.DocMobGroupPackOrders.AddDocMobGroupPackOrdersRow(docOrderRow);
            try
            {
                DocOrders.Update(GammaBase.DocMobGroupPackOrders);
                DocOrders.FillBy(GammaBase.DocMobGroupPackOrders, Scanner.SerialNumber);
                EditLastDocument();
                edtBarCode.Text = "";
                edtNomenclature.Items.Clear();
                edtWeight.Text = "";
                edtGrossWeight.Text = "";
                edtSummaryWeight.Text = "";
            }
            catch
            {
                RetryNewDocument();
            }
        }

        private void EditLastDocument() //Установка курсора на последний документ
        {
            var docOrderRow = GammaBase.DocMobGroupPackOrders.OrderByDescending(docOrder => docOrder.Date).FirstOrDefault();
            edtDocNumber.Text = docOrderRow["DocMobGroupPackOrderID"].ToString();
            edtDocDate.Value = Convert.ToDateTime(docOrderRow["Date"]);
            DocMobGroupPackOrderId = Convert.ToInt64(docOrderRow["DocMobGroupPackOrderID"]);
            DocOrderGroupPacks.FillByDocOrderID(GammaBase.DocMobGroupPackOrderGroupPacks, DocMobGroupPackOrderId);
        }

        //Новая ГУ, SaveToList - сохранить в массив при отсутствии связи с БД
        private bool AddNewGroupPack(GroupPackInfo groupPackInfo, Boolean saveToList) 
        {
            var result = false;
            if (groupPackInfo.Connected)
            {
                if (groupPackInfo.Found)
                {
                    var docOrderGroupPacksRow = GammaBase.DocMobGroupPackOrderGroupPacks.FindByDocMobGroupPackOrderIDGroupPackID(DocMobGroupPackOrderId, groupPackInfo.GroupPackId);
                    if (docOrderGroupPacksRow != null)
                    {
                        if (MessageBox.Show(@"Хотите удалить ГУ c ШК " + groupPackInfo.BarCode + @" из документа?",
                        @"Удаление ГУ", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            docOrderGroupPacksRow.Delete();
                            GammaBase.GetChanges();
                        }
                    }
                    else
                    {
                        docOrderGroupPacksRow = (GammaDataSet.DocMobGroupPackOrderGroupPacksRow)GammaBase.DocMobGroupPackOrderGroupPacks.NewRow();
                        docOrderGroupPacksRow["DocMobGroupPackOrderID"] = DocMobGroupPackOrderId;
                        docOrderGroupPacksRow["GroupPackID"] = groupPackInfo.GroupPackId;
                        docOrderGroupPacksRow["Barcode"] = groupPackInfo.BarCode;
                        GammaBase.DocMobGroupPackOrderGroupPacks.AddDocMobGroupPackOrderGroupPacksRow(docOrderGroupPacksRow);
                    }
                    try
                    {
                        DocOrderGroupPacks.Update(GammaBase.DocMobGroupPackOrderGroupPacks);
                    }
                    catch (SqlException)
                    {
                        //AddBarcodeToBuffer(groupPackInfo.BarCode);
                    }
                }
                else
                {
                    MessageBox.Show(@"ГУ с ШК " + groupPackInfo.BarCode + @" не найдена!", @"ГУ не найдена",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
                result = true;
            }
            else if (saveToList)
            {
                AddBarcodeToBuffer(groupPackInfo.BarCode);
            }
            return result;
        }

        private void ShowConnection(ConnectState conState)
        {
            switch (conState) 
            {
                case ConnectState.ConInProgress:
                    imgConnection.Image = imgList.Images[3];
                    break;
                case ConnectState.NoConInProgress:
                    imgConnection.Image = null;
                    break;
                case ConnectState.NoConnection:
                    imgConnection.Image = imgList.Images[2];
                    break;
            }           
        }

        private void UnloadTempGroupPacks()
        {
            Scanner.StopScanListener();
            Cursor.Current = Cursors.WaitCursor;
            if (!AcceptChangesToBase())
            {
                Scanner.StartScanListener();
                return;
            }
            if (_barcodes.Count == 0) { Scanner.StartScanListener(); return;}
                // MessageBox.Show("Связь с базой восстановлена. Будут выгружены несохраненные ГУ");
            BeginInvoke((Action)(() => ShowConnection((int)ConnectState.ConInProgress)));
            var tempBarcodes = new List<string>();
            var tempList = _barcodes.ToList();
            _barcodes.Clear();
            foreach (var barcode in tempList) 
                {
                    var groupPackInfo = SqlBase.GetGroupPackInfo(barcode); // Получение информации по ШК
                    if (AddNewGroupPack(groupPackInfo, false)) continue;
                    BeginInvoke((Action)(() => ShowConnection(ConnectState.NoConnection)));
                    tempBarcodes.Add(barcode);
                }
                if (tempBarcodes.Count == 0)
                {
                    BeginInvoke((Action)(() => ShowConnection(ConnectState.NoConInProgress)));
                    File.Delete(_fileName);
                    MessageBox.Show(@"Было выгружено " + tempList.Count.ToString(CultureInfo.InvariantCulture) + @" упаковок");
                }
                else
                {
                    BeginInvoke((Action)(() => ShowConnection(ConnectState.NoConnection)));
                    MessageBox.Show(@"Связь была прервана. Выгружено " + (tempList.Count - tempBarcodes.Count).ToString(CultureInfo.InvariantCulture) +
                        @" ГУ. Невыгружено " + tempBarcodes.Count.ToString(CultureInfo.InvariantCulture) + @" ГУ");
                    if (_barcodes.Count == 0) _barcodes = tempBarcodes;
                    else
                    {
                        foreach (var barcode in tempBarcodes)
                        {
                            _barcodes.Add(barcode);
                        }
                    }
                    WriteTempFile();
                    ConnectionState.StartChecker();
                }                
            Cursor.Current = Cursors.Default;
            Scanner.StartScanListener();    
        }

        private bool AcceptChangesToBase()
        {
            if (GammaBase.HasChanges())
            {
                try
                {
                    GammaBase.GetChanges();
                    DocOrderGroupPacks.Update(GammaBase.DocMobGroupPackOrderGroupPacks);
                }
                catch (SqlException)
                {
                    return false;
                }
            }
            return true;
        }

        private void GetBarCode(string barCode)
        {
            edtBarCode.BeginInvoke((Action)(()=> edtBarCode.Text = barCode));
            btnSearch.BeginInvoke((Action)(() => btnSearch_Click(btnSearch, null)));
        }

        private void AddBarcodeToBuffer(string barcode)
        {
            if (_barcodes.Exists(el => el == barcode))
            {
                if (MessageBox.Show(@"Хотите удалить данную ГУ из документа?", @"Удаление ГУ", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    _barcodes.Remove(barcode);
                }
            }
            else
            {
                GammaDataSet.DocMobGroupPackOrderGroupPacksRow row = GammaBase.DocMobGroupPackOrderGroupPacks.FindByBarcode(barcode);
                if (row != null)
                {
                    if (MessageBox.Show(@"Хотите удалить данную ГУ из документа?", @"Удаление ГУ", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        row.Delete();
                    }
                }
                else 
                {
                    _barcodes.Add(barcode);
                }
            }
            WriteTempFile();
        }

        private void WriteTempFile()
        {
            using (var fs = new FileStream(_fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var barCode in _barcodes)
                    {
                        sw.WriteLine(barCode);
                    }
                }
            }
        }

        private void DocOrder_Closing(object sender, CancelEventArgs e)
        {
            Scanner.Dispose();
            ConnectionState.OnConnectionRestored -= UnloadTempGroupPacks;
            ParentForm.Show();
        }

        private void DocOrder_Load(object sender, EventArgs e)
        {
            EventDelegate func = GetBarCode;
            Scanner = new Scanner(func, this);
            SqlBase = new SqlWork();
            try
            {
                DocOrders.FillBy(GammaBase.DocMobGroupPackOrders, Scanner.SerialNumber);
                if (GammaBase.DocMobGroupPackOrders.Count == 0)
                {
                    AddNewDoc();
                }
                else
                {
                    EditLastDocument();
                }
            }
            catch (SqlException)
            {
                MessageBox.Show(@"Нет связи с сервером. Повторите попытку в зоне покрытия WiFi",
                    @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace);
                throw;
            }
            ConnectionState.OnConnectionRestored += UnloadTempGroupPacks;
            if (File.Exists(_fileName))
            {
                using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            _barcodes.Add(sr.ReadLine());
                        }
                    }
                }
                UnloadTempGroupPacks();
            }
        }

        private void docMobGroupPackOrdersBindingSource_PositionChanged(object sender, EventArgs e)
        {

        }
    }
}