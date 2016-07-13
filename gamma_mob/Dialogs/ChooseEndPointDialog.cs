using System;
using System.Windows.Forms;
using Datalogic.API;
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
                    WarehouseId = Convert.ToInt32((sender as ButtonIntId).Id)
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

            Height = 100 + (Shared.Warehouses.Count - 1)*40;

            if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;

            for (int i = 0; i < Shared.Warehouses.Count; i++ )
            {
                var button = new ButtonIntId(Shared.Warehouses[i].WarehouseId);
                button.Click += btnOK_Click;
                button.Text = Shared.Warehouses[i].WarehouseName;
                button.Width = Convert.ToInt32(GraphFuncs.TextSize(Shared.Warehouses[i].WarehouseName, button.Font).Width + 8);
                button.Height = 30;
                button.Left = (Width - button.Width) / 2;
                button.Top = 10 + 40*i;
                Controls.Add(button);
            }
        }
    }
}