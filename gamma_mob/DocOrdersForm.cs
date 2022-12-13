using System;
using System.Windows.Forms;
using gamma_mob.Common;
using System.Collections.Generic;

namespace gamma_mob
{
    public partial class DocOrdersForm: BaseFormWithToolBar
    {
        private DocOrdersForm()
        {
            InitializeComponent();
        }
              
        public DocOrdersForm(Form parentForm, DocDirection docDirection):this()
        {
            ParentForm = parentForm;
            DocDirection = docDirection;
            
        }

        private DocDirection DocDirection { get; set; }

        private BindingSource BSource { get; set; }

        private void DocShipmentOrders_Load(object sender, EventArgs e)
        {
            switch (DocDirection)
            {
                case DocDirection.DocOut:
                    Text = "Отгрузка";
                    break;
                case DocDirection.DocIn:
                    Text = "Приемка";
                    break;
                case DocDirection.DocOutIn:
                    Text = "Заказы на перемещение";
                    break;
            }

            GetDocOrders();
            gridDocShipmentOrders.DataSource = BSource;
            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "DocOrderId",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "OrderType",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номер",
                    MappingName = "Number",
                    Width = 80
                });
            switch (DocDirection)
            {
                case DocDirection.DocOut:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Получатель",
                        MappingName = "Consignee",
                        Width = 140
                    });
                    break;
                case DocDirection.DocIn:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Получатель",
                        MappingName = "Consignee",
                        Width = 140
                    });
                    break;
                case DocDirection.DocOutIn:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Откуда",
                        MappingName = "OutPlaceName",
                        Width = 70
                    });
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Куда",
                        MappingName = "InPlaceName",
                        Width = 70
                    });
                    break;
            }
            gridDocShipmentOrders.TableStyles.Add(tableStyle);
            //Shared.RefreshBarcodes1C();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)Images.Edit, (int)Images.Refresh, (int)Images.InfoProduct });//, pnlToolBar_ButtonClick);
        }

        protected override void EditToolBarButton()
        {
            EditDocOrder();
        }

        protected override void RefreshToolBarButton()
        {
            GetDocOrders();
        }

        private void GetDocOrders()
        {
            if (BSource == null)
                BSource = new BindingSource(Db.PersonDocOrders(Shared.PersonId, DocDirection), null);
            else
            {
                BSource.DataSource = Db.PersonDocOrders(Shared.PersonId, DocDirection);
            }
        }

        private void gridDocShipmentOrders_DoubleClick(object sender, EventArgs e)
        {
            EditDocOrder();
        }

        private void EditDocOrder()
        {
            Cursor.Current = Cursors.WaitCursor;
            int row = gridDocShipmentOrders.CurrentRowIndex;
            if (row >= 0)
            {
            var id = new Guid(gridDocShipmentOrders[row, 0].ToString());
            var orderType = (OrderType) Convert.ToInt32(gridDocShipmentOrders[row, 1]);
            var docOrderForm = new DocWithNomenclatureForm(id, this, gridDocShipmentOrders[row, 2].ToString(),
                orderType, DocDirection, Shared.MaxAllowedPercentBreak);
            docOrderForm.Show();
            if (!docOrderForm.IsDisposed && docOrderForm.Enabled)
                Hide();
            } 
            Cursor.Current = Cursors.Default;
            
        }
    }
}