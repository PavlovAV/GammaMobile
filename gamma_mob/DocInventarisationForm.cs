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
using System.Drawing;

namespace gamma_mob
{
    public partial class DocInventarisationForm : BaseFormWithProducts
    {
        private DocInventarisationForm()
        {
            InitializeComponent(); 
        }

        //private DocInventarisationForm(Form parentForm): this()
        //{
        //    ParentForm = parentForm;
        //    //OfflineProducts = new List<OfflineProduct>();
        //}

        /// <summary>
        ///     инициализация формы
        /// </summary>
        /// <param name="docInventarisationId">ID Документа</param>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="docNumber">Номер инвентаризации</param>
        public DocInventarisationForm(Guid docInventarisationId, Form parentForm, string docNumber, DocDirection docDirection, int placeId)
            : this()
        {
            //FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\DocInventarisationBarcodes.xml";
            ParentForm = parentForm; 
            DocDirection = docDirection;
            DocId = docInventarisationId;

            EndPointInfo = new EndPointInfo(placeId)
            {
                IsSettedDefaultPlaceZoneId = false,
                IsAvailabilityChildPlaceZoneId = false
            };
            bool IsExit = false;
            var war = Shared.Warehouses.FirstOrDefault(w => w.WarehouseId == EndPointInfo.PlaceId);
            if (war != null && war.WarehouseZones != null && war.WarehouseZones.Count > 0)
            {
                {
                    using (var form = new ChooseEndPointDialog(EndPointInfo.PlaceId))
                    {
                        DialogResult result = form.ShowDialog();
                        Invoke((MethodInvoker)Activate);
                        if (result != DialogResult.OK)
                        {
                            Shared.ShowMessageInformation(@"Не выбрана зона склада.");
                            IsExit = true;
                            Close();
                        }
                        else
                        {
                            EndPointInfo = form.EndPointInfo;
                            EndPointInfo.IsSettedDefaultPlaceZoneId = true;
                            lblZoneName.Text = "Зона по умолчани: " + EndPointInfo.PlaceZoneName;
                            lblZoneName.Visible = true;
                        }
                    }
                }
            }

            if (!IsExit)
            {
                Shared.SaveToLogInformation(@"EndPointInfo.PlaceId-" + EndPointInfo.PlaceId + @"; EndPointInfo.PlaceZoneId-" + EndPointInfo.PlaceZoneId);


                RefreshProducts(docInventarisationId);
                /*if (!RefreshProducts(docInventarisationId))
                {
                    MessageBox.Show(@"Не удалось получить информацию о документе");
                    Close();
                    return;
                }*/
                string placeName = "";
                for (int i = 0; i < Shared.Warehouses.Count; i++)
                {
                    if (Shared.Warehouses[i].WarehouseId == placeId)
                    {
                        placeName = Shared.Warehouses[i].WarehouseName;
                        break;
                    }
                }
                Text = "Инвент-я №" + docNumber + " (" + placeName+")";
                var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
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
                //Barcodes1C = Db.GetBarcodes1C();
                //Shared.RefreshBarcodes1C();
                if (!Shared.InitializationData()) Shared.ShowMessageInformation(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
                if (Shared.TimerForUnloadOfflineProducts == null) Shared.ShowMessageInformation(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
                OnUpdateBarcodesIsNotUploaded();
            }
        }

        private BindingSource BSource { get; set; }

        protected override void RefreshToolBarButton()
        {
            RefreshDocInventarisation(DocId, true);
        }
                
        private bool RefreshProducts(Guid docInventarisationId)
        {
            BindingList<DocNomenclatureItem> list = Db.InventarisationProducts(docInventarisationId);
            if (Shared.LastQueryCompleted == false)// || list == null)
            {
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                if (NomenclatureList == null)
                    NomenclatureList = new BindingList<DocNomenclatureItem>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = NomenclatureList };
                return false;
            }
            NomenclatureList = list ?? new BindingList<DocNomenclatureItem>();
            if (BSource == null)
                BSource = new BindingSource { DataSource = NomenclatureList };
            else
            {
                BSource.DataSource = NomenclatureList;
            }
            gridInventarisation.DataSource = BSource;
            gridInventarisation.UnselectAll();
            //Barcodes = Db.GetCurrentInventarisationBarcodes(DocInventarisationId);

            Collected = 0;
            for (var i = 0; i < NomenclatureList.Count; i++)
            {
                Collected += NomenclatureList[i].CollectedQuantityUnits;
            }
            
            /*Collected = 0;
            if (Barcodes != null)
            {
                foreach (Barcodes item in Barcodes)
                {
                    Collected += (item.ProductKindId == 3) ? 0 : 1;
                }
            }
            else
                Barcodes = new List<Barcodes>();*/
            return true;
        }

        private BindingList<DocNomenclatureItem> NomenclatureList { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivatePanels(new List<int>() { (int)Images.Back, (int)Images.Inspect, (int)Images.Refresh, (int)Images.UploadToDb, (int)Images.InfoProduct });//, pnlToolBar_ButtonClick);
        }
       
        protected override DbOperationProductResult AddProductId(Guid? scanId, DbProductIdFromBarcodeResult getProductResult, EndPointInfo endPointInfo)
        {
            var addedProductIdToInventarisationResult = Db.AddProductIdToInventarisation(scanId, DocId, Shared.PersonId, endPointInfo, getProductResult.ProductId, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts);
            return addedProductIdToInventarisationResult == null ? null : (addedProductIdToInventarisationResult as DbOperationProductResult);
        }

        protected override void UpdateGrid(DbOperationProductResult addResult, ProductKind? productKindId, Guid? docInventarisationId, EndPointInfo endPointInfo, Guid? scanId, EndPointInfo startPointInfo)
        {
            if (DocId == docInventarisationId)
                    Invoke((UpdateInventarisationGridInvoker)(UpdateGrid),
                       new object[] { addResult.Product.NomenclatureId, addResult.Product.CharacteristicId, addResult.Product.QualityId, addResult.Product.NomenclatureName,
                               addResult.Product.ShortNomenclatureName, addResult.Product.Quantity, (int?)productKindId });
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName,
                string shortNomenclatureName, decimal quantity, int? productKindId)
        {
            DocNomenclatureItem good = null;
            string error_ch = "";

            try
            {
                good = NomenclatureList.FirstOrDefault(
                    g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId);
                error_ch = "ch1"; 
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
                    error_ch = "ch1_1";
                    NomenclatureList.Add(good);
                    error_ch = "ch1_2";
                    BSource.DataSource = NomenclatureList;
                    error_ch = "ch1_3";
                }
                error_ch = "ch2";
                //if (!add)
                //{
                //    good.CollectedQuantity -= quantity;
                //    if (productKindId == null || productKindId != 3)
                //    {
                //        Collected--;
                //        //Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));
                //    }
                //    if (good.CollectedQuantity == 0)
                //        NomenclatureList.Remove(good);
                //}
                //else
                {
                    good.CollectedQuantity += quantity;
                    if (productKindId == null || productKindId != 3)
                    {
                        Collected++;
                        //Barcodes.Add(new Barcodes
                        //{
                        //    Barcode = barcode,
                        //    ProductKindId = productKindId
                        //});
                    }
                }
                error_ch = "ch4";
                if (gridInventarisation != null && gridInventarisation.DataSource != null)
                {
                    error_ch = "ch5";
                    gridInventarisation.UnselectAll();
                    error_ch = "ch6";
                    int index = NomenclatureList.IndexOf(good);
                    error_ch = "ch7";
                    if (index > 0)
                    {
                        error_ch = "ch7_1";
                        gridInventarisation.CurrentRowIndex = index;
                        gridInventarisation.Select(index);
                    }
                }
                error_ch = "ch8";
                error_ch = "ch9";
            }
            catch
            {
                Shared.ShowMessageError(@"Ошибка при обновлении списка. Нажмите Ок для повтора. (" + error_ch + @")");
                RefreshDocInventarisation(DocId, true);
            }
        }

        protected override bool CheckIsCreatePalletMovementFromBarcodeScan()
        {
            return false;
        }

        protected override void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            int row = gridInventarisation.CurrentRowIndex;
            if (row >= 0)
            {
                var good = NomenclatureList[row];
                //var form = new DocInventarisationNomenclatureProductsForm(DocInventarisationId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, this);
                var form = new DocInventarisationProductsForm(DocId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, good.ProductKindId, this, good.IsEnableAddProductManual );//, new RefreshDocProductDelegate(RefreshDocInventarisation));
                if (!form.IsDisposed)
                {
                    //form.Show();
                    //if (form.Enabled)
                    //    Hide();

                    BarcodeFunc = null;
                    DialogResult result = form.ShowDialog();
                    if (form.IsRefreshQuantity)
                        RefreshDocInventarisation(DocId,true);
                }
            }
        }

        private void RefreshDocInventarisation(Guid docId, bool showMessage)
        {
            if (!RefreshProducts(docId))
            {
                if (showMessage) 
                    Shared.ShowMessageError(@"Не удалось получить информацию о документе");
                else
                    Shared.SaveToLogError(@"Не удалось получить информацию о документе", docId, null);
                //Close();
                return;
            }
        }

        private void gridInventarisation_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        private void gridInventarisation_CurrentCellChanged(object sender, EventArgs e)
        {
            gridInventarisation.Select(gridInventarisation.CurrentRowIndex);
        }

       
        //private bool _isVisibledPanels { get; set; }
        //private bool IsVisibledPanels
        //{
        //    get
        //    {
        //        return _isVisibledPanels;
        //    }
        //    set
        //    {
        //        _isVisibledPanels = value;
        //        //if (value)
        //        //    SetIsLastScanedBarcodeZone(true, null);
        //        foreach (var item in this.Controls)
        //        {
        //            if (item is System.Windows.Forms.Panel)
        //            {
        //                if ((item as System.Windows.Forms.Panel).InvokeRequired)
        //                    Invoke((MethodInvoker)(() => (item as System.Windows.Forms.Panel).Visible = value));
        //                else
        //                    (item as System.Windows.Forms.Panel).Visible = value;
        //            }
        //        }
            
        //        /*for (int i = 1; i < tbrMain.Buttons.Count; i++)
        //        {
        //            if (tbrMain.InvokeRequired)
        //                Invoke((MethodInvoker)(() => tbrMain.Buttons[i].Visible = value));
        //            else
        //                tbrMain.Buttons[i].Visible = value;
        //        }*/
        //        /*if (pnlSearch.InvokeRequired)
        //            Invoke((MethodInvoker)(() => pnlSearch.Visible = value));
        //        else
        //            pnlSearch.Visible = value;
        //        if (gridInventarisation.InvokeRequired)
        //            Invoke((MethodInvoker)(() => gridInventarisation.Visible = value));
        //        else
        //            gridInventarisation.Visible = value;
        //        if (pnlInfo.InvokeRequired)
        //            Invoke((MethodInvoker)(() => pnlInfo.Visible = value));
        //        else
        //            pnlInfo.Visible = value;
        //        if (pnlZone.InvokeRequired)
        //            Invoke((MethodInvoker)(() => pnlZone.Visible = value));
        //        else
        //            pnlZone.Visible = value;*/
        //    }
        //}

        /// <summary>
        ///     Проверка на корректность продукта для выгрузки в базу продуктов, собранных при отсутствии связи
        /// </summary>
        protected override string CheckUnloadOfflineProduct(ScannedBarcode offlineProduct)
        {
            string ret = "";
            if (offlineProduct.DocId == null)
            {
                ret = @"Ошибка! Не указан документ, куда выгрузить продукт " + offlineProduct.Barcode;
            }
            return ret;
        }

        protected override List<Nomenclature> GetNomenclatureGoods()
        {
            return NomenclatureList.Select(n => new Nomenclature(n.NomenclatureId, n.CharacteristicId, n.QualityId)).Distinct().ToList();
        }

    }   
}