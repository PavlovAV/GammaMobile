namespace gamma_mob.Dialogs
{
    partial class ChoosePasswordDialog
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс  следует удалить; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCount = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.txbPassword = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblCount
            // 
            this.lblCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCount.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lblCount.Location = new System.Drawing.Point(0, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(238, 37);
            this.lblCount.Text = "Введите пароль";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(65, 149);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 1;
            this.btnOK.Tag = "";
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txbPassword
            // 
            this.txbPassword.Location = new System.Drawing.Point(13, 40);
            this.txbPassword.Name = "txbPassword";
            this.txbPassword.PasswordChar = '*';
            this.txbPassword.Size = new System.Drawing.Size(212, 23);
            this.txbPassword.TabIndex = 0;
            // 
            // ChoosePasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 193);
            this.Controls.Add(this.txbPassword);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblCount);
            this.Location = new System.Drawing.Point(0, 40);
            this.Name = "ChoosePasswordDialog";
            this.Text = "Введите пароль";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txbPassword;
    }
}