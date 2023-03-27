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
            /*this.pnlSearch = new System.Windows.Forms.Panel();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnPrint = new System.Windows.Forms.ToolBarButton();
            this.btnDelete = new System.Windows.Forms.ToolBarButton();
            this.pnlSearch.SuspendLayout();*/
            this.SuspendLayout();
            // 
            // gridPalletItems
            // 
            this.gridPalletItems.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridPalletItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPalletItems.Location = new System.Drawing.Point(0, 54);
            this.gridPalletItems.Name = "gridPalletItems";
            this.gridPalletItems.PreferredRowHeight = 32;
            this.gridPalletItems.RowHeadersVisible = false;
            this.gridPalletItems.Size = new System.Drawing.Size(638, 401);
            this.gridPalletItems.TabIndex = 8;
            this.gridPalletItems.DoubleClick += new System.EventHandler(this.gridPalletItems_DoubleClick);
/*            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.btnAddProduct);
            this.pnlSearch.Controls.Add(this.imgConnection);
            this.pnlSearch.Controls.Add(this.edtNumber);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 24);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(638, 30);
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Location = new System.Drawing.Point(133, 4);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(72, 23);
            this.btnAddProduct.TabIndex = 2;
            this.btnAddProduct.Text = "Добавить";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // imgConnection
            // 
            this.imgConnection.Location = new System.Drawing.Point(211, 4);
            this.imgConnection.Name = "imgConnection";
            this.imgConnection.Size = new System.Drawing.Size(22, 23);
            // 
            // edtNumber
            // 
            this.edtNumber.Location = new System.Drawing.Point(0, 4);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(127, 23);
            this.edtNumber.TabIndex = 1;
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnDelete);
            this.tbrMain.Buttons.Add(this.btnPrint);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
*/            // 
            // PalletForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridPalletItems);
            //this.Controls.Add(this.pnlSearch);
            //this.Controls.Add(this.tbrMain);
            this.Name = "PalletForm";
            this.Text = "PalletForm";
            //this.pnlSearch.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridPalletItems;
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