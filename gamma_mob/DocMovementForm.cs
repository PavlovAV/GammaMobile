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
            FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                       @"\DocMovementBarcodes.xml";
            OfflineProducts = new List<OfflineProduct>();
            isLastScanedBarcodeZone = true;
            IsVisibledPanels = true;
        }

        /// <summary>
        ///     инициализация 
        /// </summary>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="placeIdTo">ID склада приемки</param>
        public DocMovementForm(Form parentForm, int placeIdTo)
            : this()
        {
            ParentForm = parentForm;
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
            Shared.SaveToLog(@"E.P-" + EndPointInfo.PlaceId + @"; E.Z-" + EndPointInfo.PlaceZoneId);

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
            Shared.RefreshBarcodes1C();
            
        }

        private EndPointInfo EndPointInfo { get; set; }
        private List<OfflineProduct> OfflineProducts { get; set; }
        private string FileName { get; set; }
        private BindingList<MovementProduct> AcceptedProducts { get; set; }
        private BindingSource BSource { get; set; }
        private List<Barcodes> Barcodes { get; set; }
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            AddProductByBarcode(edtNumber.Text, false);
        }

        /// <summary>
        ///     Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        /// <param name="placeId"></param>
        /// <param name="placeZoneId"></param>
        private void AddOfflineBarcode(string barCode, EndPointInfo endPointInfo)// int placeId, Guid? placeZoneId)
        {
            ConnectionState.StartChecker();
            AddOfflineProduct(barCode, endPointInfo);// placeId, placeZoneId);
            Invoke((ConnectStateChangeInvoker) (ShowConnection), new object[] {ConnectState.NoConnection});
        }

        /// <summary>
        ///     Добавление шк к буферу
        /// </summary>
        /// <param name="barCode">Штрихкод</param>
        /// <param name="placeId"></param>
        /// <param name="placeZoneId"></param>
        private void AddOfflineBarcode(string barCode, EndPointInfo endPointInfo/*, int placeId, Guid? placeZoneId*/, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int quantity)
        {
            ConnectionState.StartChecker();
            AddOfflineProduct(barCode, endPointInfo /*placeId, placeZoneId*/, nomenclatureId, characteristicId, qualityId, quantity);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
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

        private void AddOfflineProduct(string barcode, EndPointInfo endPointInfo)// int placeId, Guid? placeZoneId)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            if (!OfflineProducts.Any(p => p.Barcode == barcode && p.PlaceId == endPointInfo.PlaceId && ((p.PlaceZoneId == null && endPointInfo.PlaceZoneId == null) || p.PlaceZoneId == endPointInfo.PlaceZoneId)))
            {
            OfflineProducts.Add(new OfflineProduct
                {
                    Barcode = barcode,
                    PersonId = Shared.PersonId,
                    PlaceId = endPointInfo.PlaceId,
                    PlaceZoneId = endPointInfo.PlaceZoneId
                });
            Invoke(
                (MethodInvoker)
                (() => lblBufferCount.Text = OfflineProducts.Count.ToString(CultureInfo.InvariantCulture)));
            }
            SaveToXml(OfflineProducts);
        }

        private void AddOfflineProduct(string barcode, EndPointInfo endPointInfo/* int placeId, Guid? placeZoneId*/, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int quantity)
        {
            if (OfflineProducts == null) OfflineProducts = new List<OfflineProduct>();
            if (!OfflineProducts.Any(p => p.Barcode == barcode && p.PlaceId == endPointInfo.PlaceId && ((p.PlaceZoneId == null && endPointInfo.PlaceZoneId == null) || p.PlaceZoneId == endPointInfo.PlaceZoneId)))
            {
                OfflineProducts.Add(new OfflineProduct
                {
                    Barcode = barcode,
                    PersonId = Shared.PersonId,
                    PlaceId = endPointInfo.PlaceId,
                    PlaceZoneId = endPointInfo.PlaceZoneId,
                    NomenclatureId = nomenclatureId,
                    CharacteristicId = characteristicId,
                    QualityId = qualityId,
                    Quantity = quantity
                });
                Invoke(
                    (MethodInvoker)
                    (() => lblBufferCount.Text = OfflineProducts.Count.ToString(CultureInfo.InvariantCulture)));
            }
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
            if (GetProductIdFromBarcodeOrNumber(barcode) != null)
            {
                if (EndPointInfo.IsAvailabilityPlaceZoneId)
                {
                    var endPointInfo = GetPlaceZoneId(EndPointInfo);
                    if (endPointInfo != null)
                    {
                        isLastScanedBarcodeZone = true;
                        AddProductByBarcode(barcode, endPointInfo, fromBuffer);
                    }
                    else
                    {
                        isLastScanedBarcodeZone = false;
                    }
                }
                else
                {
                    isLastScanedBarcodeZone = true;
                    AddProductByBarcode(barcode, EndPointInfo, fromBuffer);
                }
            }
            else
            {
                isLastScanedBarcodeZone = true;
                MessageBox.Show(@"Продукция не найдена по ШК!", @"Продукция не найдена",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
        }

        private void AddProductByBarcode(string barcode, EndPointInfo endPointInfo, bool fromBuffer)
        {
            isLastScanedBarcodeZone = true;
            AddProductByBarcode(barcode, endPointInfo, fromBuffer, new Guid(), new Guid(), new Guid(), null);
        }

        private DbProductIdFromBarcodeResult GetProductIdFromBarcodeOrNumber(string barcode)
        {
            DbProductIdFromBarcodeResult getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
            if (getProductResult == null)
            {
                //if (!fromBuffer)
                //    AddOfflineBarcode(barcode, endPointInfo);// EndPointInfo.PlaceId, EndPointInfo.PlaceZoneId);
                return null;
            }
            if (getProductResult.ProductKindId == null || (getProductResult.ProductKindId != 3 && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
            {
                //MessageBox.Show(@"Продукция не найдена по ШК!", @"Продукция не найдена",
                //                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                //if (offlineProduct != null && fromBuffer)
                //{
                //    offlineProduct.ResultMessage = @"Продукция не найдена по ШК!";
                //    offlineProduct.Unloaded = true;
                //}
                return null;
            }
            return getProductResult;
        }

        private void AddProductByBarcode(string barcode, EndPointInfo endPointInfo, bool fromBuffer, Guid nomenclatureId, Guid characteristicId, Guid qualityId, int? quantity)
        {
            Shared.SaveToLog(@"AddMov " + barcode + @"; P-" + endPointInfo.PlaceId + @"; Z-" + endPointInfo.PlaceZoneBarcode + @"; Q-" + quantity + @"; F-" + fromBuffer.ToString());
            OfflineProduct offlineProduct = null;
            if (fromBuffer)
                offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            else
            {
                Barcodes acceptedProduct = null;
                if (Barcodes != null)
                    acceptedProduct = Barcodes.FirstOrDefault(p => p.Barcode == barcode);// & (p.ProductKindId == null || p.ProductKindId != 3));
                if (acceptedProduct != null)
                    if (acceptedProduct.ProductKindId == null || acceptedProduct.ProductKindId != 3)
                {
                        var dialogResult = MessageBox.Show(string.Format("Вы хотите отменить последнее перемещение продукта с шк {0}?", barcode)
                        , @"Операция с продуктом",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dialogResult == DialogResult.Yes)
                        {
                            CancelLastMovement(barcode, null, null, false);
                            return;
                        }
                        if (dialogResult != DialogResult.Cancel)
                        {
                            return;
                        }
                }
                //if (!SetPlaceZoneId(endPointInfo)) return;
            }
            
            if (!ConnectionState.CheckConnection())
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode, endPointInfo);//, EndPointInfo.PlaceId, EndPointInfo.PlaceZoneId);
                return;
            }           
            
            Cursor.Current = Cursors.WaitCursor;
            
            DbProductIdFromBarcodeResult getProductResult = Db.GetProductIdFromBarcodeOrNumber(barcode);
            if (getProductResult == null)
            {
                if (!fromBuffer)
                    AddOfflineBarcode(barcode, endPointInfo);// EndPointInfo.PlaceId, EndPointInfo.PlaceZoneId);
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

            var acceptResult = Db.MoveProduct(Shared.PersonId, getProductResult.ProductId, endPointInfo, getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            if (!Shared.LastQueryCompleted)
                {
                    if (!fromBuffer)
                        AddOfflineBarcode(barcode, endPointInfo /* EndPointInfo.PlaceId, EndPointInfo.PlaceZoneId*/, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
                    return;
                }
            if (acceptResult == null)
            {
                MessageBox.Show(@"Не удалось переместить продукцию на склад");
                return;
            }
            //надо подумать - никогда не сработает - DocMovementId всегда null
            if (acceptResult.AlreadyAdded && acceptResult.DocMovementId != null)
            {
                CancelLastMovement(barcode, acceptResult.OutPlace, (Guid)acceptResult.DocMovementId, true);
                return;
            }
            if (acceptResult.ResultMessage == string.Empty)
            {
                if (fromBuffer)
                {
                    if (offlineProduct != null)
                        offlineProduct.Unloaded = true;
                }
                if (endPointInfo.PlaceId == EndPointInfo.PlaceId)
                    Invoke((UpdateGridInvoker) (UpdateGrid),
                        new object[]
                                   {
                                        acceptResult.NomenclatureId, acceptResult.CharacteristicId, acceptResult.QualityId, acceptResult.NomenclatureName, acceptResult.ShortNomenclatureName, acceptResult.PlaceZoneId,  acceptResult.Quantity
                                       , true, barcode, getProductResult.ProductKindId, acceptResult.CoefficientPackage, acceptResult.CoefficientPallet
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
        /*
        private bool SetPlaceZoneId(EndPointInfo endPointInfo)
        {
            if (!EndPointInfo.IsSettedDefaultPlaceZoneId && (Shared.Warehouses.First(w => w.WarehouseId == EndPointInfo.PlaceId).WarehouseZones != null
                && Shared.Warehouses.First(w => w.WarehouseId == EndPointInfo.PlaceId).WarehouseZones.Count > 0))
            {
                //var form = new ChooseZoneDialog(this, endPointInfo.PlaceId, false);
                BarcodeFunc = null;
                using (var form = new ChooseZoneDialog(this,endPointInfo.PlaceId, false))
                {
                    //BarcodeFunc = null;
                    //BarcodeFunc = form.BarcodeReaction1;
                    DialogResult result = form.ShowDialog();
                    //Invoke((MethodInvoker)Hide);    
                    //form.Show();
                    //if (form.Enabled)
                    //    Invoke((MethodInvoker)Hide);
                    //    //Hide();
                        
                    //BarcodeFunc = BarcodeReaction;
                    //Invoke((MethodInvoker)Activate);
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show(@"Не выбрана зона склада. Перемещение не закончено", @"Отмена перемещения",
                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                        return false;
                    }
                    endPointInfo.PlaceZoneId = form.PlaceZoneId;
                }
                BarcodeFunc = BarcodeReaction;
            }
            return true;
        }*/

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

        private bool GetLastMovementProducts()
        {
            BindingList<MovementProduct> list = Db.GetMovementProductsList(EndPointInfo.PlaceId, Shared.PersonId);
            if (!Shared.LastQueryCompleted )//|| list == null)
            {
                if (AcceptedProducts == null)
                    AcceptedProducts = new BindingList<MovementProduct>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = AcceptedProducts };
            
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            if (list != null)
                AcceptedProducts = list;
            if (BSource == null)
                BSource = new BindingSource {DataSource = AcceptedProducts};
            else
            {
                BSource.DataSource = AcceptedProducts;
            }
            gridDocAccept.DataSource = BSource;
            
            Barcodes = Db.GetCurrentMovementBarcodes(EndPointInfo.PlaceId, Shared.PersonId);
            
            //Collected = list.Count;
            Collected = 0;
            if (Barcodes != null)
            {
                foreach (Barcodes item in Barcodes)
                {
                    Collected += (item.ProductKindId == 3) ? 0 : 1;
                }
            }
            else
            {
                Barcodes = new List<Barcodes>();
            }
            return true;
        }

        private void CancelLastMovement(string barcode, string outPlace, Guid? docMovementId, bool showWarningMessage)
        {
            OfflineProduct offlineProduct = null;
            if (OfflineProducts != null)
                offlineProduct = OfflineProducts.FirstOrDefault(p => p.Barcode == barcode);
            if (!showWarningMessage || MessageBox.Show(
                            string.Format("Вернуть продукт с шк {0} на передел {1}", barcode, outPlace),
                            @"Возврат продукта", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                DbOperationProductResult delResult = null;
                if (docMovementId == null)
                {
                    delResult = Db.DeleteLastProductFromMovement(barcode, EndPointInfo.PlaceId, Shared.PersonId, DocDirection.DocOutIn);
                }
                else
                {
                    delResult = Db.DeleteProductFromMovement(barcode, (Guid)docMovementId, DocDirection.DocOutIn);
                }
                if (delResult != null & string.IsNullOrEmpty(delResult.ResultMessage))
                {
                    if (offlineProduct != null) offlineProduct.Unloaded = true;
                    var barcodesItem = Barcodes.FirstOrDefault(p => p.Barcode == barcode);// || p.Number == barcode);
                    if (barcodesItem != null)
                    {
                        if (delResult.DocIsConfirmed)
                        {
                            //delResult.DocIsConfirmed = true;
                            MessageBox.Show(
                                @"Перемещение уже подтверждено. Удалять продукт из подтвержденного перемещения нельзя",
                                @"Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Asterisk,
                                MessageBoxDefaultButton.Button1);
                            return;
                        }
                        Invoke((UpdateGridInvoker)(UpdateGrid),
                               new object[]
                                   {
                                       delResult.Product.NomenclatureId, delResult.Product.CharacteristicId, delResult.Product.QualityId, delResult.Product.NomenclatureName, delResult.Product.ShortNomenclatureName, delResult.PlaceZoneId, delResult.Product.Quantity
                                       , false, barcodesItem.Barcode, barcodesItem.ProductKindId, null, null
                                   });
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(delResult.ResultMessage))
                        MessageBox.Show(@"Не удалось вернуть продукт на передел. " + delResult.ResultMessage, @"Ошибка");
                    else
                        MessageBox.Show(@"Связь с базой потеряна, не удалось вернуть продукт на передел", @"Ошибка связи");
                    if (offlineProduct == null) AddOfflineBarcode(barcode, new EndPointInfo() { PlaceId = EndPointInfo.PlaceId, PlaceZoneId = EndPointInfo.PlaceZoneId });  
                    //TODO: Вернуться и перепроверить, возможно херня с offlineProduct
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
            UIServices.SetBusyState(this);
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
            string message = "";
            string resultMessage = "";
            foreach (OfflineProduct offlineProduct in OfflineProducts.ToList())
            {
                if (offlineProduct.PlaceId == null)
                { MessageBox.Show(@"Ошибка! Не указан передел, куда выгрузить продукт " + offlineProduct.Barcode, @"Информация о выгрузке"); }
                else
                {
                    if (offlineProduct.NomenclatureId == null || offlineProduct.NomenclatureId == Guid.Empty)
                        AddProductByBarcode(offlineProduct.Barcode, new EndPointInfo() { PlaceId = (int)offlineProduct.PlaceId, PlaceZoneId = offlineProduct.PlaceZoneId }, true);
                    else
                        AddProductByBarcode(offlineProduct.Barcode, new EndPointInfo() { PlaceId = (int)offlineProduct.PlaceId, PlaceZoneId = offlineProduct.PlaceZoneId }, true, offlineProduct.NomenclatureId, offlineProduct.CharacteristicId, offlineProduct.QualityId, (int?)offlineProduct.Quantity);
                }
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
            UIServices.SetNormalState(this);
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnInspect.ImageIndex = (int)Images.Inspect;
            btnRefresh.ImageIndex = (int)Images.Refresh;
            btnUpload.ImageIndex = (int)Images.UploadToDb;
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
                    OfflineProducts = new List<OfflineProduct>();
                }
            }
        }

        private void BarcodeReaction(string barcode)
        {
            if (isLastScanedBarcodeZone)
            {
                Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
                //GetPlaceZoneId(EndPointInfo);
                //isLastScaneBarcodeZone = false;
                AddProductByBarcode(barcode, false);
            }
            else
            {
                var endPointInfo = GetPlaceZoneFromBarcode(barcode);
                if (endPointInfo != null)
                {
                    isLastScanedBarcodeZone = true;
                    //RemoveButtons(true);
                    if (this.InvokeRequired)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            AddProductByBarcode(edtNumber.Text, endPointInfo, false);
                        });
                    }
                    else
                        AddProductByBarcode(edtNumber.Text, endPointInfo, false);
                }
            }
        }

        private bool DeleteProductByBarcode(string barcode, string outPlace, Guid docMovementId, bool showWarningMessage)
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
                if (string.IsNullOrEmpty(deleteResult.ResultMessage) && Shared.LastQueryCompleted)
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
        }

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
                    //Barcodes.RemoveAll(b => b.Barcode == barcode);
                    Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));
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
                    Barcodes.Add(new Barcodes
                    {
                        Barcode = barcode,
                        ProductKindId = productKindId
                    });
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
                    RefreshDocMovementProducts(new Guid());
                    break;
                case 3:
                    if (OfflineProducts.Count > 0)
                        UnloadOfflineProducts();
                    break;
                case 4:
                    var InfoProduct = new InfoProductForm(this);
                    BarcodeFunc = null;
                    DialogResult result = InfoProduct.ShowDialog();
                    BarcodeFunc = BarcodeReaction;
                    break;
            }
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
                var form = new DocMovementProductsForm(EndPointInfo.PlaceId, Shared.PersonId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, good.PlaceZoneId, this, new RefreshDocProductDelegate(RefreshDocMovementProducts));
                if (!form.IsDisposed)
                {
                    //form.Show();
                    //if (form.Enabled)
                    //    Hide();
                    
                    BarcodeFunc = null;
                    DialogResult result = form.ShowDialog();
                    BarcodeFunc = BarcodeReaction;
                }
            }
        }

        private void RefreshDocMovementProducts(Guid docId)
        {
            if (!GetLastMovementProducts())
            {
                MessageBox.Show(@"Не удалось получить информацию о документе!" + Environment.NewLine + @"Попробуйте ещё раз обновить!");
                //Close();
                return;
            }
        }
        private delegate void ConnectStateChangeInvoker(ConnectState state);

        private delegate void UpdateGridInvoker(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName, string shortNomenclatureName, Guid? placeZoneId, 
                                                decimal quantity, bool add, string barcode, int? productKindId, int? CoefficientPackage, int? CoefficientPallet);

        private bool isLastScanedBarcodeZone { get; set; }

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
                AddProductByBarcode(edtNumber.Text, endPointInfo, false);
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
                    isLastScanedBarcodeZone = true;
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