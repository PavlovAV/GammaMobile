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
            : this(maxCount, measure, "Укажите количество")
        { }

        public SetCountProductsDialog(int maxCount, string measure, string name)
            : this(Convert.ToDecimal(maxCount), measure, name)
        { }

        public SetCountProductsDialog(decimal? maxCount, string measure)
            : this(maxCount, measure, "Укажите количество")
        { }

        public SetCountProductsDialog(decimal? maxCount, string measure, string name)
            : this(Convert.ToDecimal(maxCount).ToString("0.###") + " " + measure, name)
        {
            edtQuantity.Maximum = maxCount ?? 0;
        }

        private SetCountProductsDialog(string maxCount, string name)
            : this()
        {
            lblCount.Text = name + " (максимально " + maxCount + ")";
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