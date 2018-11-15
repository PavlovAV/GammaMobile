using System;
using System.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class ChooseShiftDialog : Form
    {
        public ChooseShiftDialog()
        {
            InitializeComponent();
            //lblCount.Text = "Укажите количество" + MaxCount != null ? " (максимально " + MaxCount + ")" : "";
        }

        public ChooseShiftDialog(string maxCount)
            : this()
        {
            
        }

        public int Quantity { get; set; }

        private string MaxCount { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            if (rdbShift1.Checked)
                Quantity = 1;
            else if (rdbShift2.Checked)
                Quantity = 2;
            else if (rdbShift3.Checked)
                Quantity = 3;
            else if (rdbShift4.Checked)
                Quantity = 4;
            else
                Quantity = 0;
            Close();
        }
    }
}