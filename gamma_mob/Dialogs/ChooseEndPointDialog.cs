using System;
using System.Drawing;
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
                    PlaceId = Convert.ToInt32((sender as ButtonIntId).Id),
                    PlaceName = Convert.ToString((sender as ButtonIntId).Text.Replace("\r\n",""))
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

            Height = 100 + (Shared.Warehouses.Count - 1)*35;

            if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
            for (int i = 0; i < Shared.Warehouses.Count; i++ )
            {
                var button = new ButtonIntId(Shared.Warehouses[i].WarehouseId);
                button.Click += btnOK_Click;
                button.Font = new Font("Tahoma",12,FontStyle.Regular);
                button.Text = Shared.Warehouses[i].WarehouseName.Length <= 11 ? Shared.Warehouses[i].WarehouseName : (Shared.Warehouses[i].WarehouseName.Substring(0, 11).Substring(10, 1) == " " ? Shared.Warehouses[i].WarehouseName.Substring(0, 10) : Shared.Warehouses[i].WarehouseName.Substring(0, 11)) + Environment.NewLine + Shared.Warehouses[i].WarehouseName.Substring(11, Math.Min(11, Shared.Warehouses[i].WarehouseName.Length - 11));
                button.Width = (Width- 5 - 20) / 2;
                button.Height = 37;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 2 + 38 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
                Controls.Add(button);
            }
        }
    }
}