namespace gamma_mob
{
    partial class InfoProductForm
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
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlLabel = new System.Windows.Forms.Panel();
            this.btnInfoProduct = new System.Windows.Forms.Button();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.pnlButtons.SuspendLayout();
            this.pnlLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 411);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(638, 44);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.btnClose.Location = new System.Drawing.Point(43, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(159, 34);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Закрыть";
            // 
            // pnlLabel
            // 
            this.pnlLabel.Controls.Add(this.btnInfoProduct);
            this.pnlLabel.Controls.Add(this.edtNumber);
            this.pnlLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLabel.Location = new System.Drawing.Point(0, 0);
            this.pnlLabel.Name = "pnlLabel";
            this.pnlLabel.Size = new System.Drawing.Size(638, 32);
            // 
            // btnInfoProduct
            // 
            this.btnInfoProduct.Location = new System.Drawing.Point(155, 4);
            this.btnInfoProduct.Name = "btnInfoProduct";
            this.btnInfoProduct.Size = new System.Drawing.Size(79, 23);
            this.btnInfoProduct.TabIndex = 4;
            this.btnInfoProduct.Text = "Найти";
            this.btnInfoProduct.Click += new System.EventHandler(this.btnInfoProduct_Click);
            // 
            // edtNumber
            // 
            this.edtNumber.Location = new System.Drawing.Point(3, 3);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(147, 25);
            this.edtNumber.TabIndex = 3;
            // 
            // lblMessage
            // 
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular);
            this.lblMessage.Location = new System.Drawing.Point(0, 32);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(638, 379);
            this.lblMessage.Text = "Просканируйте \r\nштрих-код продукции";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // InfoProductForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.pnlLabel);
            this.Controls.Add(this.pnlButtons);
            this.Name = "InfoProductForm";
            this.Text = "Информация о продукте";
            this.pnlButtons.ResumeLayout(false);
            this.pnlLabel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlLabel;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnInfoProduct;
        private System.Windows.Forms.TextBox edtNumber;
    }
}