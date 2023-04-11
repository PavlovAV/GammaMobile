namespace gamma_mob
{
    partial class PalletForm
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
            this.gridPalletItems = new System.Windows.Forms.DataGrid();
            this.lblPallet = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // gridPalletItems
            // 
            this.gridPalletItems.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridPalletItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPalletItems.Location = new System.Drawing.Point(0, 0);
            this.gridPalletItems.Name = "gridPalletItems";
            this.gridPalletItems.PreferredRowHeight = 32;
            this.gridPalletItems.RowHeadersVisible = false;
            this.gridPalletItems.Size = new System.Drawing.Size(638, 455);
            this.gridPalletItems.TabIndex = 8;
            this.gridPalletItems.DoubleClick += new System.EventHandler(this.gridPalletItems_DoubleClick);
            // 
            // lblPallet
            // 
            this.lblPallet.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPallet.Location = new System.Drawing.Point(0, 0);
            this.lblPallet.Name = "lblPallet";
            this.lblPallet.Size = new System.Drawing.Size(638, 20);
            this.lblPallet.Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold);
            this.lblPallet.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblPallet.Text = "Паллета";
            this.lblPallet.Visible = false;
            // 
            // PalletForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.lblPallet);
            this.Controls.Add(this.gridPalletItems);
            this.Name = "PalletForm";
            this.Text = "PalletForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridPalletItems;
        private System.Windows.Forms.Label lblPallet;
        /*private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.TextBox edtNumber;
        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnPrint;
        private System.Windows.Forms.ToolBarButton btnDelete;
        */
    }
}