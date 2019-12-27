using System;
using System.Data;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;

namespace gamma_mob
{
    public partial class DocProductsBaseForm : BaseForm
    {
        protected DocProductsBaseForm()
        {
            InitializeComponent();
            var tableStyle = new DataGridTableStyle();
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
                HeaderText = @"Склад\Зона",
                Width = 77,
                NullText = ""
                //Format = "0.#"
            });
            gridProducts.TableStyles.Add(tableStyle);
        }

        private RefreshDocProductDelegate RefreshDocOrder;
        /*
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
            if (!RefreshDatGrid())
            {
                MessageBox.Show(@"Не удалось получить информацию");
                Close();
                return;
            }
        }
        */
        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, RefreshDocProductDelegate refreshDocOrder)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            DocShipmentOrderId = docShipmentOrderId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                MessageBox.Show(@"Не удалось получить информацию");
                Close();
                return;
            }
        }
        
        public DocProductsBaseForm(Guid docShipmentOrderId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Form parentForm, DocDirection docDirection, RefreshDocProductDelegate refreshDocOrder)
            : this()
        {
            lblNomenclature.Text = nomenclatureName;
            ParentForm = parentForm;
            DocShipmentOrderId = docShipmentOrderId;
            NomenclatureId = nomenclatureId;
            CharacteristicId = characteristicId;
            QualityId = qualityId;
            DocDirections = docDirection;
            RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                MessageBox.Show(@"Не удалось получить информацию");
                Close();
                return;
            }
        }


        public DocProductsBaseForm(int placeId, Guid personId, Guid nomenclatureId, string nomenclatureName
            , Guid characteristicId, Guid qualityId, Guid? placeZoneId, Form parentForm, RefreshDocProductDelegate refreshDocOrder)
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
            RefreshDocOrder = refreshDocOrder;
            if (!RefreshDatGrid())
            {
                MessageBox.Show(@"Не удалось получить информацию");
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
        protected Guid? PlaceZoneId { get; set; }
        public bool IsRefreshQuantity = false;
        protected decimal Quantity { get; set; } 

        protected virtual DataTable GetProducts()
        {
            return (DataTable)null; //Db.DocShipmentOrderGoodProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, DocDirections);
        }

        protected virtual DataTable RemovalRProducts()
        {
            return (DataTable)null; //Db.RemoveProductRFromOrder(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, Quantity);
        }
        
        private bool RefreshDatGrid()
        {
            DataTable table = GetProducts();//Db.DocShipmentOrderGoodProducts(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, DocDirections);
            if (!Shared.LastQueryCompleted)
            {
                //MessageBox.Show(@"Не удалось получить информацию о продукции");
                //Close();
                return false;
            }
            if (table != null)
            {
                gridProducts.DataSource = table;
                DataRow[] rows = table.Select("IsProductR = 1");
                if (rows.Length > 0)
                {
                    btnRemoval.Visible = true;
                    btnRemoval.Tag = rows[0]["Quantity"].ToString();
                }
            }
            return true;
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 2:
                    SetQuantityProductRemoval();
                    break;
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnRemoval.ImageIndex = (int)Images.Remove;
        }

        private void SetQuantityProductRemoval()
        {
            //string MaxCount = btnRemoval.Tag.ToString();
            using (var form = new SetCountProductsDialog(btnRemoval.Tag.ToString()))
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK || form.Quantity == null)
                {
                    MessageBox.Show(@"Не указано количество продукта. Количество продукта не изменено!", @"Продукт не удален",
                                    MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                    
                }
                else
                {
                    Quantity = form.Quantity;
                    DataTable table = RemovalRProducts();// Db.RemoveProductRFromOrder(DocShipmentOrderId, NomenclatureId, CharacteristicId, QualityId, Quantity);
                    if (!Shared.LastQueryCompleted)
                    {
                        MessageBox.Show(@"Не удалось удалить продукт!");
                    }
                    if (!RefreshDatGrid())
                    {
                        MessageBox.Show(@"Не удалось обновить информацию");
                        Close();
                        return;
                    }
                    IsRefreshQuantity = true;
                    RefreshDocOrder(DocShipmentOrderId);
                    //getProductResult.CountProducts = form.Quantity;
                }
            }
        }
    }
}