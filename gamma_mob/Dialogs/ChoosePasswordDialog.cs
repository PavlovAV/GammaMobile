using System;
using System.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class ChoosePasswordDialog : Form
    {
        public ChoosePasswordDialog()
        {
            InitializeComponent();
            //lblCount.Text = "Укажите количество" + MaxCount != null ? " (максимально " + MaxCount + ")" : "";
        }


        public string Password { get; set; }


        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Password = txbPassword.Text;
            Close();
        }
    }
}