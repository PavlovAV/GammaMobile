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
            this.btnDocMovement = new System.Windows.Forms.Button();
            this.btnExtAccept = new System.Windows.Forms.Button();
            this.btnInventarisation = new System.Windows.Forms.Button();
            this.btnComplectPallet = new System.Windows.Forms.Button();
            this.btnCloseShift = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDocOrder
            // 
            this.btnDocOrder.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold);
            this.btnDocOrder.Location = new System.Drawing.Point(3, 3);
            this.btnDocOrder.Name = "btnDocOrder";
            this.btnDocOrder.Size = new System.Drawing.Size(232, 45);
            this.btnDocOrder.TabIndex = 0;
            this.btnDocOrder.TabStop = false;
            this.btnDocOrder.Text = "Отгрузки";
            this.btnDocOrder.Click += new System.EventHandler(this.btnDocOrder_Click);
            // 
            // btnDocMovement
            // 
            this.btnDocMovement.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnDocMovement.Location = new System.Drawing.Point(3, 54);
            this.btnDocMovement.Name = "btnDocMovement";
            this.btnDocMovement.Size = new System.Drawing.Size(232, 45);
            this.btnDocMovement.TabIndex = 1;
            this.btnDocMovement.TabStop = false;
            this.btnDocMovement.Text = "Перемещение на склад";
            this.btnDocMovement.Click += new System.EventHandler(this.btnDocMovement_Click);
            // 
            // btnExtAccept
            // 
            this.btnExtAccept.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnExtAccept.Location = new System.Drawing.Point(3, 105);
            this.btnExtAccept.Name = "btnExtAccept";
            this.btnExtAccept.Size = new System.Drawing.Size(232, 45);
            this.btnExtAccept.TabIndex = 3;
            this.btnExtAccept.TabStop = false;
            this.btnExtAccept.Text = "Приемка";
            this.btnExtAccept.Click += new System.EventHandler(this.btnExtAccept_Click);
            // 
            // btnInventarisation
            // 
            this.btnInventarisation.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnInventarisation.Location = new System.Drawing.Point(3, 156);
            this.btnInventarisation.Name = "btnInventarisation";
            this.btnInventarisation.Size = new System.Drawing.Size(232, 45);
            this.btnInventarisation.TabIndex = 4;
            this.btnInventarisation.TabStop = false;
            this.btnInventarisation.Text = "Инвентаризация";
            this.btnInventarisation.Click += new System.EventHandler(this.btnInventarisation_Click);
            // 
            // btnComplectPallet
            // 
            this.btnComplectPallet.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.btnComplectPallet.Location = new System.Drawing.Point(3, 258);
            this.btnComplectPallet.Name = "btnComplectPallet";
            this.btnComplectPallet.Size = new System.Drawing.Size(232, 45);
            this.btnComplectPallet.TabIndex = 5;
            this.btnComplectPallet.TabStop = false;
            this.btnComplectPallet.Text = "Комплектация паллеты";
            this.btnComplectPallet.Visible = false;
            // 
            // btnCloseShift
            // 
            this.btnCloseShift.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.btnCloseShift.Location = new System.Drawing.Point(3, 207);
            this.btnCloseShift.Name = "btnCloseShift";
            this.btnCloseShift.Size = new System.Drawing.Size(232, 45);
            this.btnCloseShift.TabIndex = 6;
            this.btnCloseShift.TabStop = false;
            this.btnCloseShift.Text = "Закрытие смены";
            this.btnCloseShift.Click += new System.EventHandler(this.btnCloseShift_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.btnCloseShift);
            this.Controls.Add(this.btnComplectPallet);
            this.Controls.Add(this.btnInventarisation);
            this.Controls.Add(this.btnExtAccept);
            this.Controls.Add(this.btnDocMovement);
            this.Controls.Add(this.btnDocOrder);
            this.Name = "MainForm";
            this.Text = "Gamma";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDocOrder;
        private System.Windows.Forms.Button btnDocMovement;
        private System.Windows.Forms.Button btnExtAccept;
        private System.Windows.Forms.Button btnInventarisation;
        private System.Windows.Forms.Button btnComplectPallet;
        private System.Windows.Forms.Button btnCloseShift;
    }
}

