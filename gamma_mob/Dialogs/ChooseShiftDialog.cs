﻿using System;
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            if (rdbShift1.Checked)
                ShiftId = 1;
            else if (rdbShift2.Checked)
                ShiftId = 2;
            else if (rdbShift3.Checked)
                ShiftId = 3;
            else if (rdbShift4.Checked)
                ShiftId = 4;
            else
                ShiftId = 0;
            Close();
        }
    }
}