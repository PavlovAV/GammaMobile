using System;
using System.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob.Dialogs
{
    public partial class MessageBoxDialog : Form
    {
        public MessageBoxDialog(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            InitializeComponent();
            Text = caption;
            lblMessage.Text = text;
            Size = new System.Drawing.Size(Screen.PrimaryScreen.WorkingArea.Width, (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.5));
            var xCenterWindow = Screen.PrimaryScreen.WorkingArea.Width / 2;
            btnOK.Location = new System.Drawing.Point(xCenterWindow - (btnOK.Width / 2), btnOK.Location.Y);
            switch (buttons)
            {
                //case MessageBoxButtons.OK:
                //    btnOK.Location = new System.Drawing.Point(xCenterWindow - (btnOK.Width / 2), btnOK.Location.Y);
                //case MessageBoxButtons.Y:
                //    btnOK.Location = new System.Drawing.Point(xCenterWindow - (btnOK.Width / 2), btnOK.Location.Y);
                //    btnOK.Text = "Yes";
                case MessageBoxButtons.OKCancel:
                    btnOK.Location = new System.Drawing.Point(xCenterWindow - btnOK.Width - 5, btnOK.Location.Y);
                    btnCancel.Visible = true;
                    break;
                case MessageBoxButtons.YesNo:
                    btnOK.Location = new System.Drawing.Point(xCenterWindow - btnOK.Width - 5, btnOK.Location.Y);
                    btnOK.Text = "Да";
                    btnOK.DialogResult = DialogResult.Yes;
                    btnCancel.Location = new System.Drawing.Point(xCenterWindow + 5, btnOK.Location.Y);
                    btnCancel.Text = "Нет";
                    btnCancel.DialogResult = DialogResult.No;
                    btnCancel.Visible = true;
                    break;
            }

            switch (icon)
            {
                case MessageBoxIcon.Asterisk:
                    lblWin.Text = "Информация.";
                    break;
                case MessageBoxIcon.Hand:
                    lblWin.Text = "Ошибка!";
                    break;
                case MessageBoxIcon.Question:
                    lblWin.Text = "Вопрос?";
                    break;
            }

            switch (defaultButton)
            {
                case MessageBoxDefaultButton.Button1:
                    btnOK.Focus();
                    break;
                case MessageBoxDefaultButton.Button2:
                    btnCancel.Focus();
                    break;
            }
            switch (icon)
            {
                case MessageBoxIcon.Asterisk:
                    Shared.Device.PlayBeep(64); 
                    break;
                case MessageBoxIcon.Hand:
                    Shared.Device.PlayBeep(16);
                    break;
                case MessageBoxIcon.Question:
                    Shared.Device.PlayBeep(32);
                    break;
            }
            
        }

        private void btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}