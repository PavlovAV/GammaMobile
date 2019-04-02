using System;
using System.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class SetCountProductsDialog : Form
    {
        public SetCountProductsDialog()
        {
            InitializeComponent();
            //lblCount.Text = "Укажите количество" + MaxCount != null ? " (максимально " + MaxCount + ")" : "";
        }

        public SetCountProductsDialog(string maxCount)
            : this()
        {
            lblCount.Text = "Укажите количество" + " (максимально " + (maxCount.Length == 0  ? "(null)" : Convert.ToDecimal(maxCount).ToString("0.###")) + ")";
            MaxCount = maxCount;
            edtQuantity.Maximum = maxCount.Length == 0 ? 0 : Convert.ToDecimal(maxCount);
        }

        public int Quantity { get; set; }

        private string MaxCount { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Quantity = (int)edtQuantity.Value;
            Close();
        }
    }
}