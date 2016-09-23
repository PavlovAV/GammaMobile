using System;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class DocMovementOrdersForm : BaseForm
    {
        public DocMovementOrdersForm(Form parentForm)
        {
            ParentForm = parentForm;
            InitializeComponent();
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
                    GetDocMovementOrders();
                    break;
            }
        }

        private void EditDocOrder()
        {
            Cursor.Current = Cursors.WaitCursor;
            int row = gridDocMovementOrders.CurrentRowIndex;
            var id = new Guid(gridDocMovementOrders[row, 0].ToString());
            var docOrderForm = new DocOrderForm(id, this, gridDocMovementOrders[row, 1].ToString(), DocType.DocMovementOrder);
            docOrderForm.Show();
            if (!docOrderForm.IsDisposed && docOrderForm.Enabled)
                Hide();
            Cursor.Current = Cursors.Default;
        }

        private void gridDocMovementOrders_DoubleClick(object sender, EventArgs e)
        {
            EditDocOrder();
        }

        private void DocMovementOrdersForm_Load(object sender, EventArgs e)
        {
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int)Images.Back;
            btnEdit.ImageIndex = (int)Images.Edit;
            btnRefresh.ImageIndex = (int)Images.Refresh;

            GetDocMovementOrders();
            gridDocMovementOrders.DataSource = BSource;
            var tableStyle = new DataGridTableStyle { MappingName = BSource.GetListName(null) };
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                MappingName = "DocId",
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
                HeaderText = "На склад",
                MappingName = "PlaceTo",
                Width = 140
            });
            gridDocMovementOrders.TableStyles.Add(tableStyle);
        }

        private void GetDocMovementOrders()
        {
            if (BSource == null)
                BSource = new BindingSource(Db.DocMovementOrders(Shared.PersonId), null);
            else
            {
                BSource.DataSource = Db.DocMovementOrders(Shared.PersonId);
            }
        }

        private BindingSource BSource { get; set; }
    }
}