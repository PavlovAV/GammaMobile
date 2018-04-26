namespace gamma_mob.Dialogs
{
    partial class SetCountProductsDialog
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
            this.edtQuantity = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCount
            // 
            this.lblCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCount.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lblCount.Location = new System.Drawing.Point(0, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(238, 56);
            this.lblCount.Text = "Укажите количество";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // edtQuantity
            // 
            this.edtQuantity.Location = new System.Drawing.Point(69, 59);
            this.edtQuantity.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.edtQuantity.Name = "edtQuantity";
            this.edtQuantity.Size = new System.Drawing.Size(100, 26);
            this.edtQuantity.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(69, 89);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // SetCountProductsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 132);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.edtQuantity);
            this.Controls.Add(this.lblCount);
            this.Location = new System.Drawing.Point(0, 80);
            this.Name = "SetCountProductsDialog";
            this.Text = "Ввод количества";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.NumericUpDown edtQuantity;
        private System.Windows.Forms.Button btnOK;
    }
}