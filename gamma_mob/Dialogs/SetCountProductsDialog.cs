using System;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob.Dialogs
{
    public partial class SetCountProductsDialog : Form
    {
        public SetCountProductsDialog()
        {
            InitializeComponent();
            edtQuantity.Minimum = 1;
            Shared.SaveToLogInformation("Open SetCountProductsDialog");
        }

        public SetCountProductsDialog(int maxCount, string measure)
            : this(maxCount.ToString() + " " + measure)
        {
            edtQuantity.Maximum = Convert.ToDecimal(maxCount);
        }

        public SetCountProductsDialog(decimal? maxCount, string measure)
            : this(Convert.ToDecimal(maxCount).ToString("0.###") + " " + measure)
        {
            edtQuantity.Maximum = maxCount ?? 0;
        }
        
        private SetCountProductsDialog(string maxCount)
            : this()
        {
            lblCount.Text = "Укажите количество" + " (максимально " + maxCount + ")";
            Shared.SaveToLogInformation("Open SetCountProductsDialog ('" + maxCount + "')");
        }

        public int Quantity { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Quantity = (int)edtQuantity.Value;
            Close();
        }
    }
}