using System;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocShipmentOrdersForm : BaseForm
    {
        public DocShipmentOrdersForm()
        {
            InitializeComponent();
        }

        private BindingSource BSource { get; set; }

        private void DocShipmentOrders_Load(object sender, EventArgs e)
        {
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnEdit.ImageIndex = (int) Images.Edit;
            btnRefresh.ImageIndex = (int) Images.Refresh;
            GetDocOrders();
            gridDocShipmentOrders.DataSource = BSource;
            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "DocShipmentOrderId",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номер",
                    MappingName = "Number",
                    Width = 90
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Покупатель",
                    MappingName = "Buyer",
                    Width = 140
                });
            gridDocShipmentOrders.TableStyles.Add(tableStyle);
        }

        private void GetDocOrders()
        {
            BSource = new BindingSource(Db.PersonDocShipmentOrders(Shared.PersonId), null);
        }

        private void gridDocShipmentOrders_DoubleClick(object sender, EventArgs e)
        {
            EditDocOrder();
        }

        private void EditDocOrder()
        {
            Cursor.Current = Cursors.WaitCursor;
            int row = gridDocShipmentOrders.CurrentRowIndex;
            var id = new Guid(gridDocShipmentOrders[row, 0].ToString());
            var docOrderForm = new DocOrderForm(id, this, gridDocShipmentOrders[row, 1].ToString());
            docOrderForm.Show();
            if (docOrderForm.Enabled)
                Hide();
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
            }
        }
    }
}