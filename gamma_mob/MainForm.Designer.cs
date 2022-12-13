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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnUserInfo = new System.Windows.Forms.Button();
            this.btnDocTransfer = new System.Windows.Forms.Button();
            this.btnCloseApp = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDocOrder
            // 
            this.btnDocOrder.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold);
            this.btnDocOrder.Location = new System.Drawing.Point(3, 3);
            this.btnDocOrder.Name = "btnDocOrder";
            this.btnDocOrder.Size = new System.Drawing.Size(232, 35);
            this.btnDocOrder.TabIndex = 0;
            this.btnDocOrder.TabStop = false;
            this.btnDocOrder.Text = "Отгрузки";
            this.btnDocOrder.Click += new System.EventHandler(this.btnDocOrder_Click);
            // 
            // btnDocMovement
            // 
            this.btnDocMovement.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnDocMovement.Location = new System.Drawing.Point(3, 84);
            this.btnDocMovement.Name = "btnDocMovement";
            this.btnDocMovement.Size = new System.Drawing.Size(232, 35);
            this.btnDocMovement.TabIndex = 1;
            this.btnDocMovement.TabStop = false;
            this.btnDocMovement.Text = "Перемещение на склад";
            this.btnDocMovement.Click += new System.EventHandler(this.btnDocMovement_Click);
            // 
            // btnExtAccept
            // 
            this.btnExtAccept.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnExtAccept.Location = new System.Drawing.Point(3, 124);
            this.btnExtAccept.Name = "btnExtAccept";
            this.btnExtAccept.Size = new System.Drawing.Size(232, 35);
            this.btnExtAccept.TabIndex = 3;
            this.btnExtAccept.TabStop = false;
            this.btnExtAccept.Text = "Приемка";
            this.btnExtAccept.Click += new System.EventHandler(this.btnExtAccept_Click);
            // 
            // btnInventarisation
            // 
            this.btnInventarisation.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnInventarisation.Location = new System.Drawing.Point(3, 164);
            this.btnInventarisation.Name = "btnInventarisation";
            this.btnInventarisation.Size = new System.Drawing.Size(232, 35);
            this.btnInventarisation.TabIndex = 4;
            this.btnInventarisation.TabStop = false;
            this.btnInventarisation.Text = "Инвентаризация";
            this.btnInventarisation.Click += new System.EventHandler(this.btnInventarisation_Click);
            // 
            // btnComplectPallet
            // 
            this.btnComplectPallet.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.btnComplectPallet.Location = new System.Drawing.Point(3, 253);
            this.btnComplectPallet.Name = "btnComplectPallet";
            this.btnComplectPallet.Size = new System.Drawing.Size(232, 35);
            this.btnComplectPallet.TabIndex = 5;
            this.btnComplectPallet.TabStop = false;
            this.btnComplectPallet.Text = "Комплектация паллеты";
            this.btnComplectPallet.Visible = false;
            // 
            // btnCloseShift
            // 
            this.btnCloseShift.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.btnCloseShift.Location = new System.Drawing.Point(3, 204);
            this.btnCloseShift.Name = "btnCloseShift";
            this.btnCloseShift.Size = new System.Drawing.Size(232, 35);
            this.btnCloseShift.TabIndex = 6;
            this.btnCloseShift.TabStop = false;
            this.btnCloseShift.Text = "Закрытие смены";
            this.btnCloseShift.Click += new System.EventHandler(this.btnCloseShift_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCloseApp);
            this.panel1.Controls.Add(this.btnUserInfo);
            this.panel1.Controls.Add(this.btnDocTransfer);
            this.panel1.Controls.Add(this.btnDocOrder);
            this.panel1.Controls.Add(this.btnCloseShift);
            this.panel1.Controls.Add(this.btnDocMovement);
            this.panel1.Controls.Add(this.btnComplectPallet);
            this.panel1.Controls.Add(this.btnExtAccept);
            this.panel1.Controls.Add(this.btnInventarisation);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(638, 455);
            // 
            // btnUserInfo
            // 
            this.btnUserInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnUserInfo.Location = new System.Drawing.Point(0, 435);
            this.btnUserInfo.Name = "btnUserInfo";
            this.btnUserInfo.Size = new System.Drawing.Size(638, 20);
            this.btnUserInfo.TabIndex = 8;
            this.btnUserInfo.Text = "button1";
            this.btnUserInfo.Click += new System.EventHandler(this.btnUserInfo_Click);
            // 
            // btnDocTransfer
            // 
            this.btnDocTransfer.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.btnDocTransfer.Location = new System.Drawing.Point(3, 44);
            this.btnDocTransfer.Name = "btnDocTransfer";
            this.btnDocTransfer.Size = new System.Drawing.Size(232, 35);
            this.btnDocTransfer.TabIndex = 7;
            this.btnDocTransfer.TabStop = false;
            this.btnDocTransfer.Text = "Заказ на перем.";
            this.btnDocTransfer.Click += new System.EventHandler(this.btnDocTransfer_Click);
            // 
            // btnCloseApp
            // 
            this.btnCloseApp.Location = new System.Drawing.Point(0, 253);
            this.btnCloseApp.Name = "btnCloseApp";
            this.btnCloseApp.Size = new System.Drawing.Size(232, 35);
            this.btnCloseApp.TabIndex = 9;
            this.btnCloseApp.Text = "Выйти";
            this.btnCloseApp.Visible = false;
            this.btnCloseApp.Click += new System.EventHandler(this.btnCloseApp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "Gamma";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDocOrder;
        private System.Windows.Forms.Button btnDocMovement;
        private System.Windows.Forms.Button btnExtAccept;
        private System.Windows.Forms.Button btnInventarisation;
        private System.Windows.Forms.Button btnComplectPallet;
        private System.Windows.Forms.Button btnCloseShift;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDocTransfer;
        private System.Windows.Forms.Button btnUserInfo;
        private System.Windows.Forms.Button btnCloseApp;
    }
}

