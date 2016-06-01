namespace gamma_mob
{
    partial class DocOrderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocOrderForm));
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.gridDocShipmentOrder = new System.Windows.Forms.DataGrid();
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.btnInspect = new System.Windows.Forms.ToolBarButton();
            this.btnRefresh = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // imgConnection
            // 
            resources.ApplyResources(this.imgConnection, "imgConnection");
            this.imgConnection.Name = "imgConnection";
            // 
            // gridDocShipmentOrder
            // 
            resources.ApplyResources(this.gridDocShipmentOrder, "gridDocShipmentOrder");
            this.gridDocShipmentOrder.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocShipmentOrder.Name = "gridDocShipmentOrder";
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnInspect);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // edtNumber
            // 
            resources.ApplyResources(this.edtNumber, "edtNumber");
            this.edtNumber.Name = "edtNumber";
            // 
            // btnAddProduct
            // 
            resources.ApplyResources(this.btnAddProduct, "btnAddProduct");
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // DocOrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.btnAddProduct);
            this.Controls.Add(this.edtNumber);
            this.Controls.Add(this.tbrMain);
            this.Controls.Add(this.gridDocShipmentOrder);
            this.Controls.Add(this.imgConnection);
            this.Name = "DocOrderForm";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.DataGrid gridDocShipmentOrder;
        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.TextBox edtNumber;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.ToolBarButton btnInspect;
        private System.Windows.Forms.ToolBarButton btnRefresh;
    }
}