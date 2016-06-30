namespace gamma_mob
{
    partial class MainForm
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
            this.btnDocOrder = new System.Windows.Forms.Button();
            this.btnDocAccept = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDocOrder
            // 
            this.btnDocOrder.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold);
            this.btnDocOrder.Location = new System.Drawing.Point(3, 73);
            this.btnDocOrder.Name = "btnDocOrder";
            this.btnDocOrder.Size = new System.Drawing.Size(232, 46);
            this.btnDocOrder.TabIndex = 0;
            this.btnDocOrder.TabStop = false;
            this.btnDocOrder.Text = "Сбор приказа";
            this.btnDocOrder.Click += new System.EventHandler(this.btnDocOrder_Click);
            // 
            // btnDocAccept
            // 
            this.btnDocAccept.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnDocAccept.Location = new System.Drawing.Point(3, 124);
            this.btnDocAccept.Name = "btnDocAccept";
            this.btnDocAccept.Size = new System.Drawing.Size(232, 50);
            this.btnDocAccept.TabIndex = 1;
            this.btnDocAccept.TabStop = false;
            this.btnDocAccept.Text = "Приемка на склад";
            this.btnDocAccept.Click += new System.EventHandler(this.btnDocAccept_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.btnDocAccept);
            this.Controls.Add(this.btnDocOrder);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Gamma";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDocOrder;
        private System.Windows.Forms.Button btnDocAccept;
    }
}

