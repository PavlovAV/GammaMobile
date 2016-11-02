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
            this.SuspendLayout();
            // 
            // btnDocOrder
            // 
            this.btnDocOrder.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold);
            this.btnDocOrder.Location = new System.Drawing.Point(3, 35);
            this.btnDocOrder.Name = "btnDocOrder";
            this.btnDocOrder.Size = new System.Drawing.Size(232, 46);
            this.btnDocOrder.TabIndex = 0;
            this.btnDocOrder.TabStop = false;
            this.btnDocOrder.Text = "Отгрузки";
            this.btnDocOrder.Click += new System.EventHandler(this.btnDocOrder_Click);
            // 
            // btnDocMovement
            // 
            this.btnDocMovement.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnDocMovement.Location = new System.Drawing.Point(3, 86);
            this.btnDocMovement.Name = "btnDocMovement";
            this.btnDocMovement.Size = new System.Drawing.Size(232, 50);
            this.btnDocMovement.TabIndex = 1;
            this.btnDocMovement.TabStop = false;
            this.btnDocMovement.Text = "Перемещение на склад";
            this.btnDocMovement.Click += new System.EventHandler(this.btnDocMovement_Click);
            // 
            // btnExtAccept
            // 
            this.btnExtAccept.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnExtAccept.Location = new System.Drawing.Point(3, 142);
            this.btnExtAccept.Name = "btnExtAccept";
            this.btnExtAccept.Size = new System.Drawing.Size(232, 50);
            this.btnExtAccept.TabIndex = 3;
            this.btnExtAccept.TabStop = false;
            this.btnExtAccept.Text = "Приемка";
            this.btnExtAccept.Click += new System.EventHandler(this.btnExtAccept_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.btnExtAccept);
            this.Controls.Add(this.btnDocMovement);
            this.Controls.Add(this.btnDocOrder);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MainForm";
            this.Text = "Gamma";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDocOrder;
        private System.Windows.Forms.Button btnDocMovement;
        private System.Windows.Forms.Button btnExtAccept;
    }
}

