using System;
using System.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class SetPacksNumberDialog : Form
    {
        public SetPacksNumberDialog()
        {
            InitializeComponent();
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