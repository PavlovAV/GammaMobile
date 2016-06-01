using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        private void DocShipmentOrders_Load(object sender, EventArgs e)
        {
            ImgList = Shared.ImgList;
            tbrMain.ImageList = ImgList;
            btnBack.ImageIndex = (int) Images.Back;
            btnEdit.ImageIndex = (int) Images.Edit;
            var bindingSource = new BindingSource(Db.PersonDocShipmentOrders(Shared.PersonId), null);
            gridDocShipmentOrders.DataSource = bindingSource;
            var tableStyle = new DataGridTableStyle {MappingName = bindingSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn()
                {
                    MappingName = "DocShipmentOrderId",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn()
            {
                HeaderText = "Номер",
                MappingName = "Number",
                Width = 100
            });
            gridDocShipmentOrders.TableStyles.Add(tableStyle);
            
        }

        private ImageList ImgList { get; set; }
               
        private void gridDocShipmentOrders_DoubleClick(object sender, EventArgs e)
        {
            EditDocOrder();
        }

        private void EditDocOrder()
        {
            var row = gridDocShipmentOrders.CurrentRowIndex;
            var id = new Guid(gridDocShipmentOrders[row, 0].ToString());
            Hide();
            var docOrderForm = new DocOrderForm(id, this);
            docOrderForm.Show();
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
            }
        }
    }
}