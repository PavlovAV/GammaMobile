namespace gamma_mob
{
    partial class DocMovementForm
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
            this.btnUpload = new System.Windows.Forms.ToolBarButton();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.gridDocAccept = new System.Windows.Forms.DataGrid();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBufferCount = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCollected = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.pnlInfo.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnUpload);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // edtNumber
            // 
            this.edtNumber.Location = new System.Drawing.Point(0, 6);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(129, 23);
            this.edtNumber.TabIndex = 1;
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Location = new System.Drawing.Point(135, 6);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(72, 23);
            this.btnAddProduct.TabIndex = 2;
            this.btnAddProduct.Text = "Принять";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // gridDocAccept
            // 
            this.gridDocAccept.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocAccept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDocAccept.Location = new System.Drawing.Point(0, 54);
            this.gridDocAccept.Name = "gridDocAccept";
            this.gridDocAccept.PreferredRowHeight = 32;
            this.gridDocAccept.RowHeadersVisible = false;
            this.gridDocAccept.Size = new System.Drawing.Size(638, 341);
            this.gridDocAccept.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 27);
            this.label1.Text = "Не выгружено:";
            // 
            // lblBufferCount
            // 
            this.lblBufferCount.Location = new System.Drawing.Point(109, 33);
            this.lblBufferCount.Name = "lblBufferCount";
            this.lblBufferCount.Size = new System.Drawing.Size(20, 27);
            this.lblBufferCount.Text = "0";
            // 
            // pnlInfo
            // 
            this.pnlInfo.Controls.Add(this.label2);
            this.pnlInfo.Controls.Add(this.lblCollected);
            this.pnlInfo.Controls.Add(this.label1);
            this.pnlInfo.Controls.Add(this.lblBufferCount);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInfo.Location = new System.Drawing.Point(0, 395);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(638, 60);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 27);
            this.label2.Text = "Собрано:";
            // 
            // lblCollected
            // 
            this.lblCollected.Location = new System.Drawing.Point(109, 6);
            this.lblCollected.Name = "lblCollected";
            this.lblCollected.Size = new System.Drawing.Size(20, 27);
            this.lblCollected.Text = "0";
            // 
            // pnlSearch
            // 
            this.pnlSearch.Controls.Add(this.imgConnection);
            this.pnlSearch.Controls.Add(this.btnAddProduct);
            this.pnlSearch.Controls.Add(this.edtNumber);
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location = new System.Drawing.Point(0, 24);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(638, 30);
            // 
            // imgConnection
            // 
            this.imgConnection.Location = new System.Drawing.Point(214, 6);
            this.imgConnection.Name = "imgConnection";
            this.imgConnection.Size = new System.Drawing.Size(24, 23);
            // 
            // DocMovementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridDocAccept);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.tbrMain);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "DocMovementForm";
            this.Text = "Перемещение на склад";
            this.pnlInfo.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.TextBox edtNumber;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.DataGrid gridDocAccept;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblBufferCount;
        private System.Windows.Forms.ToolBarButton btnUpload;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCollected;
    }
}