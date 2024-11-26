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


        public byte ShiftId { get; set; }

        private void rdbShift_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.RadioButton)
            {
                if ((sender as System.Windows.Forms.RadioButton).Checked)
                {
                    ShiftId = (byte)int.Parse((sender as System.Windows.Forms.RadioButton).Text);
                    DialogResult = DialogResult.OK;
                }
            }    
        }
    }
}