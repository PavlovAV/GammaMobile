using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using System.Collections.Generic;
using gamma_mob.Models;
using OpenNETCF.Windows.Forms;
using gamma_mob.CustomDataGrid;
using System.ComponentModel;

namespace gamma_mob
{
    public partial class DocProductsBaseForm : BaseFormWithToolBar
    {
        protected DocProductsBaseForm()
        {
            InitializeComponent();
        }

        private RefreshDocProductDelegate RefreshDocOrder;

        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            DocShipmentOrderId = docShipmentOrderId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            //RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }

        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, DocDirection docDirection, bool isMovementForOrder, OrderType orderType, RefreshDocProductDelegate refreshDocOrder)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            DocShipmentOrderId = docShipmentOrderId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            DocDirections = docDirection;
            OrderType = orderType;
            IsMovementForOrder = isMovementForOrder;
            RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }
        }


        public DocProductsBaseForm(int placeId, Guid personId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Guid? placeZoneId, Form parentForm)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            PlaceId = placeId;
            PersonId = personId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            PlaceZoneId = placeZoneId;
            //RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                Shared.ShowMessageError(@"Не удалось получить информацию");
                Close();
                return;
            }

            
        }

        protected int PlaceId { get; set; }
        protected Guid PersonId { get; set; }
        protected Guid DocShipmentOrderId { get; set; }
        protected Guid NomenclatureId { get; set; }
        protected Guid CharacteristicId { get; set; }
        protected Guid QualityId { get; set; }
        protected DocDirection DocDirections { get; set; }
        protected bool IsMovementForOrder { get; set; }
        protected OrderType OrderType { get; set; }
        protected Guid? PlaceZoneId { get; set; }
        public bool IsRefreshQuantity = false;
        protected decimal Quantity { get; set; }

        private BindingList<ProductBase> AcceptedProducts { get; set; }
        //private BindingSource BSource { get; set; }

        private delegate void UpdateGridInvoker(ProductBase t);


        protected virtual BindingList<ProductBase> GetProducts()
        {
            return (BindingList<ProductBase>)null; 
        }

        protected virtual DbOperationProductResult RemovalProduct(Guid scanId)
        {
            return (DbOperationProductResult)null; 
        }

        protected virtual DialogResult GetDialogResult(string number, string place) 
        {
            return DialogResult.Cancel; 
        }
        
        private bool RefreshDatGrid()
        {
            BindingList<ProductBase> list = GetProducts();
            if (Shared.LastQueryCompleted == false)//|| list == null)
            {
                if (AcceptedProducts == null)
                    AcceptedProducts = new BindingList<ProductBase>();
                return false;
            }
            AcceptedProducts = list ?? new BindingList<ProductBase>();
            gridProducts.DataSource = AcceptedProducts;

            var tableStyle = new DataGridTableStyle { MappingName = gridProducts.DataSource.GetType().Name };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Number",
                HeaderText = @"Дата\Номер",
                Width = 110
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "Quantity",
                HeaderText = @"Кол\Обрыв",
                Width = 33,
                Format = "0.###"
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "OutPlace",
                HeaderText = @"Откуда",
                Width = 77,
                NullText = ""
                //Format = "0.#"
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "InPlace",
                HeaderText = @"Куда",
                Width = 77,
                NullText = ""
                //Format = "0.#"
            });
            gridProducts.TableStyles.Clear();
            gridProducts.TableStyles.Add(tableStyle);

            gridProducts.UnselectAll();

            return true;
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)Images.Remove });//, pnlToolBar_ButtonClick);
            BarcodeFunc = BarcodeReaction;
        }

        protected override void RemoveToolBarButton() 
        {
            DeleteMovementProduct();
        }

        private void DeleteMovementProduct()
        {
            if (!ConnectionState.CheckConnection())
            {
                Shared.ShowMessageError(@"Нет связи с сервером" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            var rowIndex = gridProducts.CurrentRowIndex;
            if (rowIndex >= 0)
            {
                if (AcceptedProducts[rowIndex].MovementId == null) //((DataTable)gridProducts.DataSource).Rows[rowIndex]["MovementID"] == null)
                {
                    Shared.ShowMessageError("Ошибка при удалении.");
                }
                else
                {
                    var t = AcceptedProducts[rowIndex];//((DataTable)gridProducts.DataSource).Rows[rowIndex];
                    if (t != null)
                    {
                        Invoke((UpdateGridInvoker)(CancelLastMovement),
                           new object[] { t });
                            return;
                        
                    }
                }
            }
        }

        private void CancelLastMovement(ProductBase t)
        {

            var dialogResult = GetDialogResult(t.Number,t.OutPlace);
            if (dialogResult == DialogResult.Yes)
            {
                var scanId = t.MovementId;
                DbOperationProductResult delResult = null;
                delResult = RemovalProduct(scanId);

                if (delResult == null)
                    Shared.ShowMessageError(@"Связь с сервером потеряна, не удалось отменить операцию.");
                else
                    if (string.IsNullOrEmpty(delResult.ResultMessage))
                    {
                        Shared.ScannedBarcodes.ClearLastBarcode();
                        Shared.ScannedBarcodes.DeletedScan(scanId);
                        AcceptedProducts.Remove(t);
                        IsRefreshQuantity = true;
                    }
                    else
                    {
                        Shared.ShowMessageError(@"Не удалось отменить операцию. " + delResult.ResultMessage);
                    }
            }
        }

        private void BarcodeReaction(string barcode)
        {
            int rowIndex = -1;
            var count = AcceptedProducts.Count;
            bool isFound = false;
            int i = 0;
            while (!isFound && i < count)
                {
                    var item = (AcceptedProducts[i] as ProductBase);
                    if (item.Barcode == barcode)
                    {
                        isFound = true;
                        rowIndex = i;
                    }
                    i++;
                }
            
            
            if (rowIndex < 0)
                Shared.ShowMessageError("Перемещение по ШК " + barcode + " не найдено!");
            else
            {
                var t = AcceptedProducts[rowIndex];
                if (t != null)
                {
                    Invoke((UpdateGridInvoker) (CancelLastMovement),
                               new object[]
                                   {t});
                    return;

                } 
                return;
            }
        }

    }
}