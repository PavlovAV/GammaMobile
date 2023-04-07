namespace gamma_mob.Dialogs
{
    partial class ChooseNomenclatureCharacteristicDialog
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
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.gridChoose = new System.Windows.Forms.DataGrid();
            this.pnlBarcode = new System.Windows.Forms.Panel();
            this.btnAddBarcode = new System.Windows.Forms.Button();
            this.edtBarcode = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.pnlBarcode.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(638, 84);
            this.label4.Text = "Выберите номенклатуру или отсканируйте паллету, из которой упаковка/коробка";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 424);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(638, 31);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(74, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 25);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // gridChoose
            // 
            this.gridChoose.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridChoose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridChoose.Location = new System.Drawing.Point(0, 84);
            this.gridChoose.Name = "gridChoose";
            this.gridChoose.Size = new System.Drawing.Size(638, 340);
            this.gridChoose.TabIndex = 11;
            this.gridChoose.DoubleClick += new System.EventHandler(this.gridChoose_DoubleClick);
            // 
            // pnlBarcode
            // 
            this.pnlBarcode.Controls.Add(this.btnAddBarcode);
            this.pnlBarcode.Controls.Add(this.edtBarcode);
            this.pnlBarcode.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBarcode.Location = new System.Drawing.Point(0, 84);
            this.pnlBarcode.Name = "pnlBarcode";
            this.pnlBarcode.Size = new System.Drawing.Size(638, 30);
            // 
            // btnAddBarcode
            // 
            this.btnAddBarcode.Location = new System.Drawing.Point(143, 1);
            this.btnAddBarcode.Name = "btnAddBarcode";
            this.btnAddBarcode.Size = new System.Drawing.Size(72, 25);
            this.btnAddBarcode.TabIndex = 2;
            this.btnAddBarcode.Text = "Добавить";
            this.btnAddBarcode.Click += new System.EventHandler(this.btnAddBarcode_Click);
            // 
            // edtBarcode
            // 
            this.edtBarcode.Location = new System.Drawing.Point(10, 1);
            this.edtBarcode.Name = "edtBarcode";
            this.edtBarcode.Size = new System.Drawing.Size(127, 25);
            this.edtBarcode.TabIndex = 1;
            // 
            // ChooseNomenclatureCharacteristicDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.pnlBarcode);
            this.Controls.Add(this.gridChoose);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel1);
            this.Location = new System.Drawing.Point(0, 80);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseNomenclatureCharacteristicDialog";
            this.Text = "Выбор паллеты-донора";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.pnlBarcode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGrid gridChoose;
        private System.Windows.Forms.Panel pnlBarcode;
        private System.Windows.Forms.TextBox edtBarcode;
        private System.Windows.Forms.Button btnAddBarcode;
        
    }
}