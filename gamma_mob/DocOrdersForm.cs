using System;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocOrdersForm: BaseForm
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
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnEdit.ImageIndex = (int) Images.Edit;
            btnRefresh.ImageIndex = (int) Images.Refresh;
            btnInfoProduct.ImageIndex = (int) Images.InfoProduct;
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
                    HeaderText = "Номер",
                    MappingName = "Number",
                    Width = 80
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Получатель",
                    MappingName = "Consignee",
                    Width = 140
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "OrderType",
                    Width = -1
                });
            gridDocShipmentOrders.TableStyles.Add(tableStyle);
            //Shared.RefreshBarcodes1C();
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
            var orderType = (OrderType) Convert.ToInt32(gridDocShipmentOrders[row, 3]);
            var docOrderForm = new DocWithNomenclatureForm(id, this, gridDocShipmentOrders[row, 1].ToString(),
                orderType, "DocOrderBarcodes.xml", DocDirection, (Shared.MaxAllowedPercentBreak ?? 0));
            docOrderForm.Show();
            if (!docOrderForm.IsDisposed && docOrderForm.Enabled)
                Hide();
            } 
            Cursor.Current = Cursors.Default;
            
        }

        private void tbrMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (tbrMain.Buttons.IndexOf(e.Button))
            {
                case 0:
                    Close();
                    break;
                case 1:
                    EditDocOrder();
                    break;
                case 2:
                    GetDocOrders();
                    break;
                case 3:
                    var InfoProduct = new InfoProductForm(this);
                    //BarcodeFunc = null;
                    DialogResult result = InfoProduct.ShowDialog();
                    //Invoke((MethodInvoker)Activate);
                    //BarcodeFunc = BarcodeReaction;
                    break;
            }
        }
    }
}