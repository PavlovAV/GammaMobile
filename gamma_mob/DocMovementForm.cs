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
    public partial class DocMovementForm : BaseFormWithProducts
    {
        private DocMovementForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     инициализация 
        /// </summary>
        /// <param name="parentForm">Форма, вызвавшая данную форму</param>
        /// <param name="placeIdTo">ID склада приемки</param>
        public DocMovementForm(Form parentForm, DocDirection docDirection, EndPointInfo endPointInfo)
            : this()
        {
            ParentForm = parentForm;
            DocDirection = docDirection;

            EndPointInfo = endPointInfo;
            if (endPointInfo.IsSettedDefaultPlaceZoneId)
            {
                lblZoneName.Text = "Зона: " + EndPointInfo.PlaceZoneName;
                pnlZone.Visible = true;
            }

            Shared.SaveToLogInformation(@"EndPointInfo.PlaceId-" + EndPointInfo.PlaceId + @"; EndPointInfo.PlaceZoneId-" + EndPointInfo.PlaceZoneId);

            AcceptedProducts = new BindingList<MovementProduct>();

            GetLastMovementProducts();
                var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
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
                //Shared.RefreshBarcodes1C();
                if (BSource == null)
                {
                    Shared.ShowMessageError(@"Внимание! Ошибка при обновлении с сервера." + Environment.NewLine + @"Попробуйте еще раз.");
                    Text = "Ошибка при обновлении с сервера!";
                }
                else
                {
                    if (!Shared.InitializationData()) Shared.ShowMessageInformation(@"Внимание! Не обновлены" + Environment.NewLine + @" данные с сервера.");
                    if (Shared.TimerForUnloadOfflineProducts == null) Shared.ShowMessageInformation(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"выгрузка на сервер.");
                    OnUpdateBarcodesIsNotUploaded();
                }
        }

        private BindingList<MovementProduct> _acceptedProducts { get; set; }
        private BindingList<MovementProduct> AcceptedProducts
        {
            get
            {
                if (_acceptedProducts == null)
                    _acceptedProducts = new BindingList<MovementProduct>();
                return _acceptedProducts;
            }
            set
            {
                _acceptedProducts = value;
            }
        }
        private BindingSource BSource { get; set; }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivatePanels(new List<int>() { (int)Images.Back, (int)Images.Inspect, (int)Images.Refresh, (int)Images.UploadToDb, (int)Images.InfoProduct, (int)Images.RDP });//, pnlToolBar_ButtonClick);
        }

        protected override void RefreshToolBarButton()
        {
            RefreshDocMovementProducts(new Guid(), true);
        }

        protected override DbOperationProductResult AddProductId(Guid? scanId, DbProductIdFromBarcodeResult getProductResult, EndPointInfo endPointInfo)
        {
            var addedMoveProductResult = Db.MoveProduct(scanId, Shared.PersonId, getProductResult.ProductId, endPointInfo, (int?)getProductResult.ProductKindId, getProductResult.NomenclatureId, getProductResult.CharacteristicId, getProductResult.QualityId, getProductResult.CountProducts, getProductResult.FromProductId);
            return addedMoveProductResult == null ? null : (addedMoveProductResult as DbOperationProductResult);
        }

        protected override void UpdateGrid(DbOperationProductResult acceptResult, ProductKind? productKindId, Guid? docId, EndPointInfo endPointInfo, Guid? scanId)
        {
            if (endPointInfo.PlaceId == EndPointInfo.PlaceId)
                    Invoke((UpdateMovementGridInvoker)(UpdateGrid),
                        new object[]
                                   {
                                        acceptResult.Product.NomenclatureId, acceptResult.Product.CharacteristicId, acceptResult.Product.QualityId, acceptResult.Product.NomenclatureName, acceptResult.Product.ShortNomenclatureName, acceptResult.PlaceZoneId,  acceptResult.Product.Quantity
                                       , true, (int?)productKindId, acceptResult.Product.CoefficientPackage, acceptResult.Product.CoefficientPallet
                                   });
        }

        protected override bool CheckIsCreatePalletMovementFromBarcodeScan()
        {
            return true;
        }

        private bool GetLastMovementProducts()
        {
            BindingList<MovementProduct> list = Db.GetMovementProductsList(EndPointInfo.PlaceId, Shared.PersonId);
            if (Shared.LastQueryCompleted == false)//|| list == null)
            {
                if (AcceptedProducts == null)
                    AcceptedProducts = new BindingList<MovementProduct>();
                if (BSource == null)
                    BSource = new BindingSource { DataSource = AcceptedProducts };
            
                // MessageBox.Show(@"Не удалось получить информацию о текущем документе");
                return false;
            }
            AcceptedProducts = list ?? new BindingList<MovementProduct>();
            if (BSource == null)
                BSource = new BindingSource {DataSource = AcceptedProducts};
            else
            {
                //if (BSource.DataSource == null) 
                    BSource.DataSource = AcceptedProducts;
            }
            //if (gridDocAccept.DataSource == null) 
               gridDocAccept.DataSource = BSource;
            gridDocAccept.UnselectAll();
            Collected = 0;
            for (var i = 0; i < AcceptedProducts.Count; i++)
            {
                Collected += AcceptedProducts[i].CollectedQuantityUnits;
            }
                //Shared.ScannedBarcodes.BarcodesCollectedCount(EndPointInfo.PlaceId);

            return true;
        }
        

        protected override string CheckUnloadOfflineProduct(ScannedBarcode offlineProduct)
        {
            string ret = "";
            if (offlineProduct.PlaceId == null)
            {
                ret = @"Ошибка! Не указан передел, куда выгрузить продукт " + offlineProduct.Barcode;
            }
            return ret;
        }

        private void UpdateGrid(Guid nomenclatureId, Guid characteristicId, Guid qualityId, string nomenclatureName, string shortNomenclatureName, Guid? placeZoneId, decimal quantity, bool add,// string barcode,
                                int? productKindId, int? coefficientPackage, int? coefficientPallet)
        {
            MovementProduct good = null;
            string error_ch = "";
                
            try
            {
                good = AcceptedProducts.FirstOrDefault(
                        g => g.NomenclatureId == nomenclatureId && g.CharacteristicId == characteristicId && g.QualityId == qualityId && g.PlaceZoneId == placeZoneId);
                error_ch = "ch1";
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
                    error_ch = "ch1_1";
                    AcceptedProducts.Add(good);
                    error_ch = "ch1_2";
                    BSource.DataSource = AcceptedProducts;
                    error_ch = "ch1_3";
                }
                error_ch = "ch2";
                if (!add)
                {
                    good.CollectedQuantity -= quantity;
                    if (productKindId == null || productKindId != 3)
                    {
                        Collected--;
                        //Barcodes.Remove(Barcodes.FirstOrDefault(b => b.Barcode == barcode));
                    }
                    if (good.CollectedQuantity == 0)
                        AcceptedProducts.Remove(good);
                    error_ch = "ch3";
                }
                else
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
                    error_ch = "ch4";
                }
                //if (gridDocAccept != null && gridDocAccept.DataSource != null)
                {
                    gridDocAccept.UnselectAll();
                    error_ch = "ch5";
                    int index = AcceptedProducts.IndexOf(good);
                    error_ch = "ch6";
                    if (index > 0)
                    {
                        gridDocAccept.CurrentRowIndex = index;
                        gridDocAccept.Select(index);
                    }
                    error_ch = "ch7";
                }
                error_ch = "ch8";
            }
            catch
            {
                Shared.ShowMessageError(@"Ошибка при обновлении списка. Нажмите Ок для повтора. (" + error_ch + @")");
                RefreshDocMovementProducts(new Guid(), true);
            }
        }

        private void gridDocAccept_DoubleClick(object sender, EventArgs e)
        {
            OpenDetails();
        }

        protected override void OpenDetails()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            var row = gridDocAccept.CurrentRowIndex;
            if (row >= 0)
            {
                var good = AcceptedProducts[row];
                var form = new DocMovementProductsForm(EndPointInfo.PlaceId, Shared.PersonId, good.NomenclatureId, good.NomenclatureName, good.CharacteristicId, good.QualityId, good.PlaceZoneId, this);
                if (!form.IsDisposed)
                {
                    //form.Show();
                    //if (form.Enabled)
                    //    Hide();
                    
                    BarcodeFunc = null;
                    DialogResult result = form.ShowDialog();
                    if (form.IsRefreshQuantity)
                        RefreshDocMovementProducts(Guid.Empty, true);
                }
            }
        }

        private void RefreshDocMovementProducts(Guid docId, bool showMessage)
        {
            if (!GetLastMovementProducts())
            {
                if (showMessage) 
                    Shared.ShowMessageInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + @"Попробуйте ещё раз обновить!");
                else
                    Shared.SaveToLogInformation(@"Не удалось получить информацию о документе!" + Environment.NewLine + @"Попробуйте ещё раз обновить!", docId, null);
                //Close();
                return;
            }
        }

        private void btnChangeZone_Click(object sender, EventArgs e)
        {
            using (var formPlaceZone = new ChooseEndPointDialog(EndPointInfo.PlaceId))
            {
                DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                if (resultPlaceZone != DialogResult.OK)
                {
                    Shared.ShowMessageInformation(@"Не выбрана новая зона склада.");
                    //endPointInfo.IsSettedDefaultPlaceZoneId = false;
                }
                else
                {
                    EndPointInfo = formPlaceZone.EndPointInfo;
                    lblZoneName.Text = "Зона: " + EndPointInfo.PlaceZoneName;
                    EndPointInfo.IsSettedDefaultPlaceZoneId = true;

                }
            }
        }
    }
}