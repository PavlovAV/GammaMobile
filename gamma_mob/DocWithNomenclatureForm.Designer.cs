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
            this.btnQuestionNomenclature = new System.Windows.Forms.ToolBarButton();
            this.btnPallets = new System.Windows.Forms.ToolBarButton();
            this.btnInfoProduct = new System.Windows.Forms.ToolBarButton();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.gridDocOrder = new System.Windows.Forms.DataGrid();
            this.lblBufferCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblPercentBreak = new System.Windows.Forms.Label();
            this.lblBreak = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblCollected = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.pnlInfo.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnInspect);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Buttons.Add(this.btnUpload);
            this.tbrMain.Buttons.Add(this.btnQuestionNomenclature);
            this.tbrMain.Buttons.Add(this.btnPallets);
            this.tbrMain.Buttons.Add(this.btnInfoProduct);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // edtNumber
            // 
            this.edtNumber.Location = new System.Drawing.Point(0, 4);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(127, 25);
            this.edtNumber.TabIndex = 1;
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
            // gridDocOrder
            // 
            this.gridDocOrder.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDocOrder.Location = new System.Drawing.Point(0, 54);
            this.gridDocOrder.Name = "gridDocOrder";
            this.gridDocOrder.PreferredRowHeight = 32;
            this.gridDocOrder.RowHeadersVisible = false;
            this.gridDocOrder.Size = new System.Drawing.Size(638, 341);
            this.gridDocOrder.TabIndex = 4;
            this.gridDocOrder.DoubleClick += new System.EventHandler(this.gridDocOrder_DoubleClick);
            this.gridDocOrder.CurrentCellChanged += new System.EventHandler(this.gridDocOrder_CurrentCellChanged);
            // 
            // lblBufferCount
            // 
            this.lblBufferCount.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.lblBufferCount.Location = new System.Drawing.Point(98, 40);
            this.lblBufferCount.Name = "lblBufferCount";
            this.lblBufferCount.Size = new System.Drawing.Size(47, 20);
            this.lblBufferCount.Text = "0";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 20);
            this.label2.Text = "Не выгружено:";
            // 
            // pnlInfo
            // 
            this.pnlInfo.Controls.Add(this.lblPercentBreak);
            this.pnlInfo.Controls.Add(this.lblBreak);
            this.pnlInfo.Controls.Add(this.label1);
            this.pnlInfo.Controls.Add(this.lblCollected);
            this.pnlInfo.Controls.Add(this.label2);
            this.pnlInfo.Controls.Add(this.lblBufferCount);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInfo.Location = new System.Drawing.Point(0, 395);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(638, 60);
            // 
            // lblPercentBreak
            // 
            this.lblPercentBreak.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.lblPercentBreak.Location = new System.Drawing.Point(211, 13);
            this.lblPercentBreak.Name = "lblPercentBreak";
            this.lblPercentBreak.Size = new System.Drawing.Size(51, 20);
            this.lblPercentBreak.Text = "0";
            this.lblPercentBreak.TextChanged += new System.EventHandler(this.lblPercentBreak_TextChanged);
            // 
            // lblBreak
            // 
            this.lblBreak.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.lblBreak.Location = new System.Drawing.Point(146, 13);
            this.lblBreak.Name = "lblBreak";
            this.lblBreak.Size = new System.Drawing.Size(63, 20);
            this.lblBreak.Text = "% обрыва:";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 20);
            this.label1.Text = "Собрано:";
            // 
            // lblCollected
            // 
            this.lblCollected.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular);
            this.lblCollected.Location = new System.Drawing.Point(98, 13);
            this.lblCollected.Name = "lblCollected";
            this.lblCollected.Size = new System.Drawing.Size(47, 20);
            this.lblCollected.Text = "0";
            // 
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
            // DocWithNomenclatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridDocOrder);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.tbrMain);
            this.Name = "DocWithNomenclatureForm";
            this.Text = "title";
            this.Activated += new System.EventHandler(this.DocWithNomenclatureForm_Activated);
            this.pnlInfo.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
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
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCollected;
        private System.Windows.Forms.ToolBarButton btnQuestionNomenclature;
        private System.Windows.Forms.ToolBarButton btnPallets;
        private System.Windows.Forms.ToolBarButton btnInfoProduct;
        private System.Windows.Forms.Label lblBreak;
        private System.Windows.Forms.Label lblPercentBreak;
    }
}