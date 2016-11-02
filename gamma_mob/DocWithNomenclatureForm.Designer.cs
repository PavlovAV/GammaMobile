namespace gamma_mob
{
    sealed partial class DocWithNomenclatureForm 
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
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnInspect = new System.Windows.Forms.ToolBarButton();
            this.btnRefresh = new System.Windows.Forms.ToolBarButton();
            this.btnUpload = new System.Windows.Forms.ToolBarButton();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.gridDocOrder = new System.Windows.Forms.DataGrid();
            this.lblBufferCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnInspect);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Buttons.Add(this.btnUpload);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // edtNumber
            // 
            this.edtNumber.Location = new System.Drawing.Point(0, 30);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(127, 23);
            this.edtNumber.TabIndex = 1;
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Location = new System.Drawing.Point(133, 30);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(72, 23);
            this.btnAddProduct.TabIndex = 2;
            this.btnAddProduct.Text = "Добавить";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // imgConnection
            // 
            this.imgConnection.Location = new System.Drawing.Point(211, 30);
            this.imgConnection.Name = "imgConnection";
            this.imgConnection.Size = new System.Drawing.Size(22, 23);
            // 
            // gridDocOrder
            // 
            this.gridDocOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDocOrder.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocOrder.Location = new System.Drawing.Point(0, 59);
            this.gridDocOrder.Name = "gridDocOrder";
            this.gridDocOrder.PreferredRowHeight = 32;
            this.gridDocOrder.RowHeadersVisible = false;
            this.gridDocOrder.Size = new System.Drawing.Size(638, 360);
            this.gridDocOrder.TabIndex = 4;
            this.gridDocOrder.CurrentCellChanged += new System.EventHandler(this.gridDocOrder_CurrentCellChanged);
            // 
            // lblBufferCount
            // 
            this.lblBufferCount.Location = new System.Drawing.Point(105, 266);
            this.lblBufferCount.Name = "lblBufferCount";
            this.lblBufferCount.Size = new System.Drawing.Size(100, 20);
            this.lblBufferCount.Text = "0";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(4, 266);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Не выгружено:";
            // 
            // DocWithNomenclatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.lblBufferCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gridDocOrder);
            this.Controls.Add(this.imgConnection);
            this.Controls.Add(this.btnAddProduct);
            this.Controls.Add(this.edtNumber);
            this.Controls.Add(this.tbrMain);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "DocWithNomenclatureForm";
            this.Text = "title";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnInspect;
        private System.Windows.Forms.ToolBarButton btnRefresh;
        private System.Windows.Forms.TextBox edtNumber;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.DataGrid gridDocOrder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblBufferCount;
        private System.Windows.Forms.ToolBarButton btnUpload;
    }
}