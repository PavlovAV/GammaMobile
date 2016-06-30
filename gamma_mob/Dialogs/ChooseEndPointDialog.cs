using System;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob.Dialogs
{
    public partial class ChooseEndPointDialog : Form
    {
        public ChooseEndPointDialog()
        {
            InitializeComponent();
        }

        public EndPointInfo EndPointInfo { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            EndPointInfo = new EndPointInfo
                {
                    WarehouseId = Convert.ToInt32(edtWarehouse.SelectedValue)
                };
            Close();
        }

        private void ChooseEndPointDialog_Load(object sender, EventArgs e)
        {
            if (Shared.Warehouses == null)
            {
                MessageBox.Show(@"Не удалось получить список складов");
                DialogResult = DialogResult.Abort;
                Close();
                return;
            }
            edtWarehouse.DataSource = Shared.Warehouses;
            edtWarehouse.DisplayMember = "WarehouseName";
            edtWarehouse.ValueMember = "WarehouseId";
            if (Shared.Warehouses.Count > 0)
                edtWarehouse.SelectedIndex = 0;
        }
    }
}