using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using OpenNETCF.Windows.Forms;
using OpenNETCF.ComponentModel;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithShowMessage : BaseForm
    {
        private int xCenterWindow = Screen.PrimaryScreen.WorkingArea.Width / 2;

        private List<System.Windows.Forms.Panel> pnlMessageList = new List<System.Windows.Forms.Panel>();
        public bool ShowMessageError(string message)
        {
            return ShowMessageError(message, @"", null, null);
        }

        public bool ShowMessageError(string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            ShowMessage(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            Shared.SaveToLogError(message, docID, productID);
            return true;
        }

        public DialogResult ShowMessageQuestion(QuestionResultEventHandler returnProcAfterQuestionResult, QuestionResultEventHandlerParameter param, string message)
        {
            return ShowMessageQuestion(returnProcAfterQuestionResult, param, message, @"", null, null);
        }

        protected event QuestionResultEventHandler ReturnProcAfterQuestionResult;
        
        private QuestionResultEventHandlerParameter Param { get; set; }
        
        public DialogResult ShowMessageQuestion(QuestionResultEventHandler returnProcAfterQuestionResult, QuestionResultEventHandlerParameter param, string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            DialogResult res = DialogResult.No;
            Param = param ?? new QuestionResultEventHandlerParameter();
            ReturnProcAfterQuestionResult += returnProcAfterQuestionResult;
            ShowMessage(message, "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            return res;
        }

        public bool ShowMessageInformation(string message)
        {
            return ShowMessageInformation(message, @"", null, null);
        }

        public bool ShowMessageInformation(string message, string technicalMessage, Guid? docID, Guid? productID)
        {
            ShowMessage(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            Shared.SaveToLogInformation(message, docID, productID);
            return true;
        }

        private System.Windows.Forms.Panel NewPnlMessage(string pnlMessageName, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            System.Windows.Forms.Label lblMessage = new System.Windows.Forms.Label();
            System.Windows.Forms.Button btnOK = new System.Windows.Forms.Button();
            System.Windows.Forms.Button btnCancel = new System.Windows.Forms.Button();
            System.Windows.Forms.Panel panel1 = new System.Windows.Forms.Panel();
            System.Windows.Forms.Label lblWin = new System.Windows.Forms.Label();
            System.Windows.Forms.Panel pnlButtons = new System.Windows.Forms.Panel();
            System.Windows.Forms.Panel pnlMessage = new System.Windows.Forms.Panel();
            // 
            // pnlButtons
            // 
            pnlButtons.BackColor = System.Drawing.Color.DarkGray;
            pnlButtons.Controls.Add(btnOK);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.Location = new System.Drawing.Point(0, 157);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new System.Drawing.Size(638, 58);
            // 
            // btnOK
            // 
            //btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(100, 35);
            btnOK.Location = new System.Drawing.Point(xCenterWindow - (btnOK.Width / 2), 12); //new System.Drawing.Point(12, 12);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.Click += new System.EventHandler(btn_Click);
            // 
            // btnCancel
            // 
            //btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            btnCancel.Location = new System.Drawing.Point(xCenterWindow + 5, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(100, 35);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Отмена";
            btnCancel.Visible = false;
            btnCancel.Click += new System.EventHandler(btn_Click);
            // 
            // panel1
            // 
            panel1.Controls.Add(lblMessage);
            panel1.Controls.Add(lblWin);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(638, 157);
            // 
            // lblMessage
            // 
            lblMessage.BackColor = System.Drawing.Color.Silver;
            lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            lblMessage.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            lblMessage.Location = new System.Drawing.Point(0, 31);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new System.Drawing.Size(638, 126);
            lblMessage.Text = text;
            // 
            // lblWin
            // 
            lblWin.BackColor = System.Drawing.Color.DarkGray;
            lblWin.Dock = System.Windows.Forms.DockStyle.Top;
            lblWin.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Bold);
            lblWin.Location = new System.Drawing.Point(0, 0);
            lblWin.Name = "lblWin";
            lblWin.Size = new System.Drawing.Size(638, 31);
            lblWin.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MessageBoxDialog
            // 
            pnlMessage = new System.Windows.Forms.Panel() { Visible = false };
            pnlMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlMessage.Controls.Add(panel1);
            pnlMessage.Controls.Add(pnlButtons);
            pnlMessage.Name = pnlMessageName;

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    btnOK.Text = "OK";
                    btnOK.Location = new System.Drawing.Point(xCenterWindow - (btnOK.Width / 2), btnOK.Location.Y);
                    btnCancel.Visible = false;
                    break;
                case MessageBoxButtons.OKCancel:
                    btnOK.Text = "OK";
                    btnOK.Location = new System.Drawing.Point(xCenterWindow - btnOK.Width - 5, btnOK.Location.Y);
                    btnCancel.Text = "Отмена";
                    btnCancel.Visible = true;
                    break;
                case MessageBoxButtons.YesNo:
                    btnOK.Location = new System.Drawing.Point(xCenterWindow - btnOK.Width - 5, btnOK.Location.Y);
                    btnOK.Text = "Да";
                    btnCancel.Text = "Нет";
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

            return pnlMessage;
        }

        private void ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            System.Windows.Forms.Panel pnlMessage = NewPnlMessage("pnlMessage" + DateTime.Now, text, caption, buttons, icon, defaultButton);
            
            Controls.Add(pnlMessage);
            pnlMessage.Visible = true;
            pnlMessage.BringToFront();
            pnlMessageList.Add(pnlMessage);
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
            var pnlMessageName = (sender as System.Windows.Forms.Button).Parent.Parent.Name;
            var pnlMessage = pnlMessageList.FirstOrDefault(m => m.Name == pnlMessageName);

            if (pnlMessage != null)
            {                
                Controls.Remove(pnlMessage);
                pnlMessageList.Remove(pnlMessage);
            }
            this.Refresh();
            if (ReturnProcAfterQuestionResult != null)
            {
                //ReturnProcAfterQuestionResult.Invoke();
                if (Param == null)
                    Param = new QuestionResultEventHandlerParameter();
                switch ((sender as System.Windows.Forms.Button).Text)
                {
                    case "OK":
                        Param.dialogResult = DialogResult.OK;
                        break;
                    case "Отмена":
                        Param.dialogResult = DialogResult.Cancel;
                        break;
                    case "Да":
                        Param.dialogResult = DialogResult.Yes;
                        break;
                    case "Нет":
                        Param.dialogResult = DialogResult.No;
                        break;
                }
                Invoke((MethodInvoker)delegate()
                {
                    ReturnProcAfterQuestionResult((Param as QuestionResultEventHandlerParameter));
                });
            }
        }

    }
}
