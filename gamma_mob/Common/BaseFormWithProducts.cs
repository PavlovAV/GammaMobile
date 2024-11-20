using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using gamma_mob.Models;
using gamma_mob.Dialogs;
using OpenNETCF.Windows.Forms;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithProducts : BaseFormWithBarcodeScan
    {        
        #region Panels

/*        public System.Windows.Forms.TextBox edtNumber { get; private set; }
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.Panel pnlSearch;
*/
        private System.Windows.Forms.Label lblBufferCount;
        private System.Windows.Forms.Label lblCollected;
        private System.Windows.Forms.Label lblPercentBreak;
        private System.Windows.Forms.Panel pnlInfo;
        
        public override void ActivatePanels()
        {
            base.ActivatePanels();
            ActivatePanelInfo();
        }

        public override void ActivatePanels(List<int> pnlToolBar_ActivButtons)
        {
            base.ActivatePanels(pnlToolBar_ActivButtons);
            ActivatePanelInfo();
        }
        
        private void ActivatePanelInfo()
        {
            var pnlElementHeight = (int)(Shared.ToolBarHeight * 0.7) - 2;
            var label1 = new System.Windows.Forms.Label()
            {
                Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(3, 1),
                Name = "label1",
                Size = new System.Drawing.Size(60, pnlElementHeight),
                Text = "Собрано:"
            };
            lblCollected = new System.Windows.Forms.Label()
            {
                Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(63, 1),
                Name = "lblCollected",
                Size = new System.Drawing.Size(40, pnlElementHeight),
                Text = "0"
            };
            var label2 = new System.Windows.Forms.Label()
            {
                Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(105, 1),
                Name = "label2",
                Size = new System.Drawing.Size(89, pnlElementHeight),
                Text = "Не выгружено:"
            };
            lblBufferCount = new System.Windows.Forms.Label()
            {
                Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(194, 1),
                Name = "lblBufferCount",
                Size = new System.Drawing.Size(40, pnlElementHeight),
                Text = "0"
            };
            lblPercentBreak = new System.Windows.Forms.Label()
            {
                Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(3, pnlElementHeight),
                Name = "lblPercentBreak",
                Size = new System.Drawing.Size(300, pnlElementHeight),
                Text = "% обрыва",
                Visible = false
            };
            TextChanged += new System.EventHandler(this.lblPercentBreak_TextChanged);
            pnlInfo = new System.Windows.Forms.Panel()
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Location = new System.Drawing.Point(0, 395),
                Name = "pnlInfo",
                Size = new System.Drawing.Size(638, pnlElementHeight)
            };

            pnlInfo.SuspendLayout();
            SuspendLayout();
            pnlInfo.Controls.Add(lblPercentBreak);
            pnlInfo.Controls.Add(label1);
            pnlInfo.Controls.Add(lblCollected);
            pnlInfo.Controls.Add(label2);
            pnlInfo.Controls.Add(lblBufferCount);
            this.Controls.Add(pnlInfo);
            pnlInfo.ResumeLayout(false);
            ResumeLayout(false);
            var c = this;
        }
        
        private void lblPercentBreak_TextChanged(object sender, EventArgs e)
        {

            Label label = sender as Label;
            if (label != null)
            {
                if (label.Text == "% обрыва превышен")
                {
                    label.BackColor = Color.Red;
                    label.ForeColor = Color.White;
                }
                else
                {
                    label.BackColor = Color.FromArgb(192, 192, 192);
                    label.ForeColor = Color.Black;
                }
            }
        }

        /// <summary>
        ///     Установка видимости % обрыва
        /// </summary>
        private void SetVisibledlblPercentBreak()
        {
            if (lblPercentBreak != null)
            {
                if (!lblPercentBreak.Visible)
                {
                    Invoke(
                        (MethodInvoker)
                    (() => pnlInfo.Height = pnlInfo.Height * 2));
                    Invoke(
                        (MethodInvoker)
                    (() => lblPercentBreak.Visible = true));
                }
            }
        }

        #endregion
/*
        #region Connection

        protected void ConnectionLost()
        {
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
        }

        protected void ConnectionRestored()
        {
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore });
        }

        private void ShowConnection(ConnectState conState)
        {
            if (imgConnection != null)
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
                    case ConnectState.ConnectionRestore:
                        imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                        break;
                }
            if (PnlToolBar != null)
            {
                foreach (var control in PnlToolBar.Controls)
                {
                    if (control is OpenNETCF.Windows.Forms.Button2)
                    {
                        var backButton = (control as OpenNETCF.Windows.Forms.Button2);
                        if (backButton.ImageIndex == (int)Images.Back || backButton.ImageIndex == (int)Images.BackOffline)
                        {
                            switch (conState)
                            {
                                case ConnectState.ConInProgress:
                                    if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
                                    break;
                                case ConnectState.NoConInProgress:
                                    //imgConnection.Image = null;
                                    break;
                                case ConnectState.NoConnection:
                                    if (backButton.ImageIndex == (int)Images.Back) backButton.ImageIndex = (int)Images.BackOffline;
                                    break;
                                case ConnectState.ConnectionRestore:
                                    if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        #endregion
*/
        #region FormActions

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            ////Подписка на событие восстановления связи
            //ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            ////Подписка на событие потери связи
            //ConnectionState.OnConnectionLost += ConnectionLost;
            //Подписка на событие +1 не выгружено (ошибка при сохранении в БД остканированной продукции)
            ScannedBarcodes.OnUpdateBarcodesIsNotUploaded += OnUpdateBarcodesIsNotUploaded;

            //Подписка на событие Выгрузить невыгруженную продукцию
            ScannedBarcodes.OnUnloadOfflineProducts += UnloadOfflineProducts;

            Shared.RefreshIsScanGroupPackOnlyFromProduct();
        }

        protected override void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection).Count > 0)
                Shared.ShowMessageInformation("Есть невыгруженные продукты!" + Environment.NewLine + "Сначала выгрузите в базу в зоне связи!");
            base.OnFormClosing(sender, e);
            //ConnectionState.OnConnectionRestored -= ConnectionRestored;
            //ConnectionState.OnConnectionLost -= ConnectionLost;
            ScannedBarcodes.OnUpdateBarcodesIsNotUploaded -= OnUpdateBarcodesIsNotUploaded;
            ScannedBarcodes.OnUnloadOfflineProducts -= UnloadOfflineProducts;
        }

        protected override void FormActivated(object sender, EventArgs e)
        {
            base.FormActivated(sender, e);
            RefreshCollected();
        }
       

        #endregion

        #region Products

        /// <summary>
        ///     Используются ли зоны
        /// </summary>
        protected bool IsUsedEndPoints { get; set; }

        /// <summary>
        ///     Вид документа, по которому идет работа (направление)
        /// </summary>
        protected DocDirection DocDirection { get; set; }

        /// <summary>
        ///     ID документа, по которому идет работа (основание)
        /// </summary>
        protected Guid DocId { get; set; }

        protected bool IsMovementForOrder { get; set; }

        protected int _collected;

        protected int Collected
        {
            get { return _collected; }
            set
            {
                _collected = value;
                RefreshCollected();
            }
        }

        private void RefreshCollected()
        {
            try
            {
                if (lblCollected != null)
                {
                    Invoke(
                        (MethodInvoker)
                        (() => lblCollected.Text = Collected.ToString(CultureInfo.InvariantCulture)));
                }
            }
            catch (ObjectDisposedException ex)
            { }
        }

        protected bool IsControlExec { get; set; }
        protected DateTime? StartExec { get; set; }
        protected int? countScan { get; set; }

        protected GetNomenclatureCharacteristicQuantityDialog GetNomenclatureCharacteristicQuantityForm;
        /// <summary>
        ///     Добавление продукта по штрихкоду
        /// </summary>
        /// <param name="barcode">штрих-код</param>
        /// <param name="fromBuffer">ШК из буфера невыгруженных</param>
        protected void AddProductByBarcode(string barcode, bool fromBuffer)
        {
            {
                {
                    var placeZones = Shared.PlaceZones.Where(p => p.Barcode == barcode); // && (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId)));
                    if (!fromBuffer && placeZones != null && placeZones.Count() > 0)
                    {
                        if (placeZones.FirstOrDefault(p => (StartPointInfo == null || (StartPointInfo != null && (p.PlaceId == StartPointInfo.PlaceId || (p.AllowedUseZonesOfPlaceGroup && p.RootPlaceId == StartPointInfo.PlaceId))))) == null)
                            Shared.ShowMessageError(@"Найденная по ШК " + barcode + " зона не принадлежит складу отгрузки!" + Environment.NewLine + "Невозможно определить зону склада");
                        else if (placeZones.Count() > 1)
                            Shared.ShowMessageError(@"По ШК " + barcode + " найдено несколько зон!" + Environment.NewLine + "Невозможно определить зону склада");
                        else
                        {
                            var placeZone = placeZones.FirstOrDefault();
                            //GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(new EndPointInfo(placeZone.PlaceId, placeZone.PlaceZoneId), EndPointInfo, this, false, true, GetNomenclatureGoods(), checkExistMovementToZone);//, barcode, fromBuffer, getProductResult, this);
                            GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(this, new NomenclatureCharacteristicQuantityDialogParameter() { StartPointInfo = new EndPointInfo(placeZone.PlaceId, placeZone.PlaceZoneId), EndPointInfo = EndPointInfo, IsFilteringOnNomenclature = false, IsFilteringOnEndpoint = true, NomenclatureGoods = GetNomenclatureGoods(), CheckExistMovementToZone = checkExistMovementToZone });//, barcode, fromBuffer, getProductResult, this);
                            GetNomenclatureCharacteristicQuantityForm.Show();
                            Param1 = new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer };
                            if (ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity == null)
                                ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity += this.ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct;
                        }
                    }
                    else if (!fromBuffer && Shared.Warehouses.Any(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.WarehouseId == StartPointInfo.PlaceId))))
                    {
                        if (Shared.Warehouses.Count(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.WarehouseId == StartPointInfo.PlaceId))) > 1)
                            Shared.ShowMessageError(@"По ШК " + barcode + " найдено несколько переделов!" + Environment.NewLine + "Невозможно определить передел");
                        else
                        {
                            //GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(new EndPointInfo(/*Shared.Warehouses.First(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId))).WarehouseId*/StartPointInfo.PlaceId), EndPointInfo, this, false, true, GetNomenclatureGoods());//, barcode, fromBuffer, getProductResult, this);
                            GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(this, new NomenclatureCharacteristicQuantityDialogParameter() { StartPointInfo = new EndPointInfo(/*Shared.Warehouses.First(p => p.Barcode == barcode && (StartPointInfo == null || (StartPointInfo != null && p.PlaceId == StartPointInfo.PlaceId))).WarehouseId*/StartPointInfo.PlaceId), EndPointInfo = EndPointInfo, IsFilteringOnNomenclature = false, IsFilteringOnEndpoint = true, NomenclatureGoods = GetNomenclatureGoods() });//, barcode, fromBuffer, getProductResult, this);
                            GetNomenclatureCharacteristicQuantityForm.Show();
                            Param1 = new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer };
                            if (ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity == null)
                                ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity += this.ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct;
                        }
                    }
                    else
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        DbProductIdFromBarcodeResult getProductResult = Shared.Barcodes1C.GetProductFromBarcodeOrNumberInBarcodes(barcode, false);
                        Cursor.Current = Cursors.Default;

                        if (getProductResult == null || getProductResult.ProductKindId == null || (Shared.FactProductKinds.Contains((ProductKind)getProductResult.ProductKindId) && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty)))
                        {
                            Shared.ShowMessageError(@"Продукция не найдена по ШК! " + barcode + " (Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                        }
                        else
                        {
                            if (!fromBuffer)
                            {
                                if ((this is DocWithNomenclatureForm) && IsControlExec && StartExec == null && (countScan ?? 3) > 2)
                                {
                                    countScan = 0;
                                    if (Shared.ShowMessageQuestion(@"Вы не отметили, что погрузка начата. Вы уже начали погрузку?") == DialogResult.Yes)
                                    {
                                        UIServices.SetBusyState(this);
                                        var result = Db.UpdateStartExecInDocOrder(DocId, Shared.PersonId);
                                        UIServices.SetNormalState(this);
                                        if (result == null)
                                        {
                                            Shared.ShowMessageError("Ошибка при сохранении времени начала погрузки. Попробуйте еще раз.");
                                        }
                                        else
                                        {
                                            try
                                            {
                                                CultureInfo culture = new CultureInfo("ru-RU");
                                                StartExec = Convert.ToDateTime(result, culture);
                                            }
                                            catch
                                            {
                                                Shared.ShowMessageError(result);
                                            }
                                        }
                                    }
                                }
                                countScan++;
                            }

                            if (getProductResult.ProductKindId == ProductKind.ProductMovement && (getProductResult.ProductId == null || getProductResult.ProductId == Guid.Empty))
                            {
                                if (!getProductResult.IsMovementFromPallet)
                                {
                                    //GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(StartPointInfo, EndPointInfo, this, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, (byte?)getProductResult.ProductKindId, getProductResult.MeasureUnitId, getProductResult.CountProducts, getProductResult.CountFractionalProducts);//, barcode, fromBuffer, getProductResult, this);
                                    GetNomenclatureCharacteristicQuantityForm = new GetNomenclatureCharacteristicQuantityDialog(this, new NomenclatureCharacteristicQuantityDialogParameter() { StartPointInfo = StartPointInfo, EndPointInfo = EndPointInfo, IsFilteringOnNomenclature = true, IsFilteringOnEndpoint = (StartPointInfo != null && StartPointInfo.PlaceId > 0), NomenclatureId = getProductResult.NomenclatureId, CharacteristicId = getProductResult.CharacteristicId, QualityId = getProductResult.QualityId, ProductKindId = (byte?)getProductResult.ProductKindId, MeasureUnitId = getProductResult.MeasureUnitId, Quantity = getProductResult.CountProducts, QuantityFractional = getProductResult.CountFractionalProducts, ValidUntilDate = getProductResult.ValidUntilDate });//, barcode, fromBuffer, getProductResult, this);
                                    GetNomenclatureCharacteristicQuantityForm.Show();
                                    Param1 = new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer };
                                    if (ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity == null)
                                        ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity += this.ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct;
                                } 
                                else if (CheckIsCreatePalletMovementFromBarcodeScan())
                                {
                                    base.ChooseNomenclatureCharacteristic(this.ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct, new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer, getProductResult = getProductResult });
                                    /*if (getProductResult.NomenclatureId == null || getProductResult.NomenclatureId == Guid.Empty || getProductResult.CharacteristicId == null || getProductResult.CharacteristicId == Guid.Empty || getProductResult.QualityId == null || getProductResult.QualityId == Guid.Empty)
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
                                    }*/
                                }                                
                            }
                            else
                            {
                                AddProductByBarcode(barcode, fromBuffer, getProductResult);
                                /*if (EndPointInfo != null && EndPointInfo.IsAvailabilityPlaceZoneId && !EndPointInfo.IsSettedDefaultPlaceZoneId)
                                {
                                    base.ChooseEndPoint(this.ChoosePlaceZoneBarcodeReactionInAddProduct, new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer, getProductResult = getProductResult });
                                }
                                else
                                {
                                    AddProductByBarcode(barcode, EndPointInfo, fromBuffer, getProductResult);
                                }*/
                            }
                        }
                    }
                }
            }
        }

        protected void AddProductByBarcode(string barcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            if (EndPointInfo != null && EndPointInfo.IsAvailabilityPlaceZoneId && !EndPointInfo.IsSettedDefaultPlaceZoneId)
            {
                base.ChooseEndPoint(this.ChoosePlaceZoneBarcodeReactionInAddProduct, new AddProductReceivedEventHandlerParameter() { barcode = barcode, endPointInfo = EndPointInfo, fromBuffer = fromBuffer, getProductResult = getProductResult });
            }
            else
            {
                AddProductByBarcode(barcode, EndPointInfo, fromBuffer, getProductResult);
            }
        }

        protected void AddProductByBarcode(string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            if (!fromBuffer && getProductResult.ProductId != null && getProductResult.ProductId != Guid.Empty && Shared.ScannedBarcodes.CheckIsLastBarcode(barcode, DocDirection, DocId, (endPointInfo == null ? null : (int?)endPointInfo.PlaceId), (endPointInfo == null ? null : endPointInfo.PlaceZoneId)))
            {
                Shared.ShowMessageInformation(@"Вы уже сканировали этот шк " + barcode);
            }
            else
            {
                var scanId = Shared.ScannedBarcodes.AddScannedBarcode(barcode, endPointInfo, DocDirection, DocId, getProductResult);
                if (scanId == null || scanId == Guid.Empty)
                    Shared.ShowMessageError("Ошибка1 при сохранении отсканированного штрих-кода");

                AddProductByBarcode(scanId, barcode, endPointInfo, fromBuffer, getProductResult, DocId);
            }   
        }

        protected bool? AddProductByBarcode(Guid? scanId, string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult, Guid? id)
        {
            Shared.SaveToLogInformation(@"Add " + barcode + @"; scanId-" + scanId.ToString() + @"; Q-" + getProductResult.CountProducts + @"; F-" + fromBuffer.ToString());
            Cursor.Current = Cursors.WaitCursor;
            var addResult = AddProductId(scanId, getProductResult, endPointInfo);
            if (Shared.LastQueryCompleted == false)
            {
                Shared.ScannedBarcodes.NotUploadedScan(scanId, "Shared.LastQueryCompleted == false", addResult == null || addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                Shared.SaveToLogError(@"AddProductId.LastQueryCompleted is null (scanId = " + scanId.ToString() + ")");
                return null;
            }
            if (addResult == null)
            {
                Shared.ScannedBarcodes.NotUploadedScan(scanId, "addResult == nul", addResult == null || addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                Shared.ShowMessageError(@"Не удалось добавить продукт" + Environment.NewLine + barcode + " в приказ!");
                Shared.ScannedBarcodes.ClearLastBarcode();
                return false;
            }
            if (addResult.ResultMessage == string.Empty)
            {
                Shared.ScannedBarcodes.UploadedScan(scanId, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                UpdateGrid(addResult, getProductResult.ProductKindId, id, endPointInfo, scanId, getProductResult != null && getProductResult.FromPlaceId != null ? new EndPointInfo((int)getProductResult.FromPlaceId, getProductResult.FromPlaceZoneId) : null);
            }
            else
            {
                Shared.ScannedBarcodes.UploadedScanWithError(scanId, addResult.ResultMessage, addResult.Product == null ? (Guid?)null : addResult.Product.ProductId);
                if (!addResult.AlreadyMadeChanges && ((DocId != null && id == DocId) || (EndPointInfo != null && endPointInfo.PlaceId == EndPointInfo.PlaceId)))
                {
                    Shared.ShowMessageError(fromBuffer ? @"Ошибка при загрузке на сервер невыгруженного" + Environment.NewLine + @" продукта: " : @"Продукт: " + barcode + Environment.NewLine + addResult.ResultMessage);
                    Shared.ScannedBarcodes.ClearLastBarcode();
                }
            }
            return true;
        }

        protected virtual DbOperationProductResult AddProductId(Guid? scanId, DbProductIdFromBarcodeResult getProductResult, EndPointInfo endPointInfo) 
        { 
            return null;
        }

        protected virtual void UpdateGrid(DbOperationProductResult addResult, ProductKind? productKindId, Guid? id, EndPointInfo endPointInfo, Guid? scanId, EndPointInfo startPointInfo) { }

        protected abstract bool CheckIsCreatePalletMovementFromBarcodeScan();

        protected abstract List<Nomenclature> GetNomenclatureGoods();

        protected override void ActionByBarcode(string barcode)
        {
            AddProductByBarcode(barcode, false);
        }

        /// <summary>
        ///     Установка % обрыва
        /// </summary>
        protected void SetPercentBreak(string percentBreakText)
        {
            try
            {
                if (lblPercentBreak != null)
                {
                    if (!lblPercentBreak.Visible)
                        SetVisibledlblPercentBreak();
                    Invoke(
                        (MethodInvoker)
                    (() => lblPercentBreak.Text = percentBreakText));
                }
            }
            catch (ObjectDisposedException ex)
            { }
        }

        #endregion

        #region Offline_Online

        /// <summary>
        ///     Установка кол-ва невыгруженных продуктов
        /// </summary>
        protected void SetBufferCount(string bufferCountText)
        {
            if (!IsDisposed && lblBufferCount != null)
            {
                lblBufferCount.Text = bufferCountText;
                //Invoke(
                //    (MethodInvoker)
                //    (() => lblBufferCount.Text = bufferCountText));
            }
        }
        
        /// <summary>
        ///     Обновление кол-ва невыгруженных продуктов
        /// </summary>
        protected void OnUpdateBarcodesIsNotUploaded()
        {
            try
            {
                string bufferCountText = "";
                if (Shared.ScannedBarcodes != null && Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection) != null)
                {
                    bufferCountText = Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection).Count.ToString(CultureInfo.InvariantCulture);
                    Invoke(
                        new MethodInvoker(delegate
                        {
                            if (!IsDisposed && lblBufferCount != null) lblBufferCount.Text = bufferCountText;
                        }));
                    //SetBufferCount(bufferCountText);
                }
                Invoke(
                        (MethodInvoker)
                        (() => Shared.IsExistsUnloadOfflineProducts = !(bufferCountText == "0")));
            }
            catch (ObjectDisposedException ex)
            { }
        }

        /// <summary>
        ///     Проверка на корректность продукта для выгрузки в базу продуктов, собранных при отсутствии связи
        /// </summary>
        protected abstract string CheckUnloadOfflineProduct(ScannedBarcode offlineProduct);
        
        /// <summary>
        ///     Выгрузка в базу продуктов, собранных при отсутствии связи
        /// </summary>
        protected override void UnloadOfflineProducts()
        {
            //UIServices.SetBusyState(this);
            ////Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConInProgress });
            //ConnectionLost();
            if (Shared.ScannedBarcodes != null && Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection) != null)
            {
                foreach (ScannedBarcode offlineProduct in Shared.ScannedBarcodes.BarcodesIsNotUploaded(DocDirection))
                {
                    var checkRes = CheckUnloadOfflineProduct(offlineProduct);
                    if (checkRes != "")
                    {
                        Shared.ShowMessageError(checkRes);
                        Shared.ScannedBarcodes.UploadedScanWithError(offlineProduct.ScanId, checkRes, offlineProduct.ProductId);
                    }
                    else
                    {
                        if (AddProductByBarcode(offlineProduct.ScanId, offlineProduct.Barcode, new EndPointInfo((int)offlineProduct.PlaceId, offlineProduct.PlaceZoneId), true, new DbProductIdFromBarcodeResult() { ProductId = offlineProduct.ProductId ?? new Guid(), ProductKindId = (ProductKind?)offlineProduct.ProductKindId, NomenclatureId = offlineProduct.NomenclatureId ?? new Guid(), CharacteristicId = offlineProduct.CharacteristicId ?? new Guid(), QualityId = offlineProduct.QualityId ?? new Guid(), CountProducts = offlineProduct.Quantity ?? 0, MeasureUnitId = offlineProduct.MeasureUnitId ?? Guid.Empty, FromProductId = offlineProduct.FromProductId, FromPlaceId = offlineProduct.FromPlaceId, FromPlaceZoneId = offlineProduct.FromPlaceZoneId, NewWeight = offlineProduct.NewWeight, ValidUntilDate = offlineProduct.ValidUntilDate }, (Guid)offlineProduct.DocId) == null)
                            break;
                    }
                }
            }
            //UIServices.SetNormalState(this);
        }

        #endregion

        #region Barcode

        protected void ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct(AddProductReceivedEventHandlerParameter param)
        {
            base.ReturnAddProductBeforeChoosedNomenclatureCharacteristic -= ChooseNomenclatureCharacteristicBarcodeReactionInAddProduct;
            BarcodeFunc = this.BarcodeReaction;
            if (param != null)
                if (param.endPointInfo == null)
                    AddProductByBarcode(param.barcode, param.fromBuffer, param.getProductResult);
                else
                    AddProductByBarcode(param.barcode, param.endPointInfo, param.fromBuffer, param.getProductResult);
        }

        protected void ChoosePlaceZoneBarcodeReactionInAddProduct(AddProductReceivedEventHandlerParameter param)
        {
            base.ReturnAddProductBeforeChoosedPlaceZone -= ChoosePlaceZoneBarcodeReactionInAddProduct;
            BarcodeFunc = this.BarcodeReaction;
            if (param != null)
                AddProductByBarcode(param.barcode, param.endPointInfo, param.fromBuffer, param.getProductResult);
        }
                
        #endregion

        public event AddProductReceivedEventHandler ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity;
        protected Object Param1;

        public void ClosingGetNomenclatureCharacteristicQuantityDialog()
        {
            if (GetNomenclatureCharacteristicQuantityForm != null)
            {
                var getFromProductResult = new DbProductIdFromBarcodeResult()
                {
                    NomenclatureId = GetNomenclatureCharacteristicQuantityForm.NomenclatureId,
                    CharacteristicId = GetNomenclatureCharacteristicQuantityForm.CharacteristicId,
                    QualityId = GetNomenclatureCharacteristicQuantityForm.QualityId,
                    CountProducts = GetNomenclatureCharacteristicQuantityForm.CountProducts,
                    CountFractionalProducts = GetNomenclatureCharacteristicQuantityForm.CountFractionalProducts ?? 0,
                    MeasureUnitId = GetNomenclatureCharacteristicQuantityForm.MeasureUnitId,
                    FromProductId = null,
                    FromPlaceId = GetNomenclatureCharacteristicQuantityForm.StartPointInfo.PlaceId,
                    FromPlaceZoneId = GetNomenclatureCharacteristicQuantityForm.StartPointInfo.PlaceZoneId,
                    ProductKindId = GetNomenclatureCharacteristicQuantityForm.ProductKindId == null ? (ProductKind?)null : (ProductKind)GetNomenclatureCharacteristicQuantityForm.ProductKindId,
                    NewWeight = GetNomenclatureCharacteristicQuantityForm.BaleWeight,
                    ValidUntilDate = GetNomenclatureCharacteristicQuantityForm.ValidUntilDate
                };
                
                if (GetNomenclatureCharacteristicQuantityForm.DialogResult == DialogResult.OK)
                {
                    if (ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity != null && (Param1 is AddProductReceivedEventHandlerParameter))
                    {
                        (Param1 as AddProductReceivedEventHandlerParameter).endPointInfo = GetNomenclatureCharacteristicQuantityForm.EndPointInfo;
                        (Param1 as AddProductReceivedEventHandlerParameter).getProductResult = getFromProductResult;
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity((Param1 as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    //else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                    //{
                    //    Invoke((MethodInvoker)delegate()
                    //    {
                    //        ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(getFromProductResult);
                    //    });
                    //}
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите номенклатуру");
                }
                else
                {
                    if (ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity != null)
                    {
                        Db.AddMessageToLog("BaseFormWithProducts.ClosingGetNomenclatureCharacteristicQuantityDialog:1-Invoke ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity");
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeGetNomenclatureCharacteristicQuantity((null as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    //else if (ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic != null)
                    //{
                    //    Invoke((MethodInvoker)delegate()
                    //    {
                    //        ReturnNomenclatureCharacteristicBeforeChoosedNomenclatureCharacteristic(null as DbProductIdFromBarcodeResult);
                    //    });
                    //}
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите номенклатуру");
                }
            }
        }

    }
}
