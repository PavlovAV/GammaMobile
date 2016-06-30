namespace gamma_mob
{
    partial class DocAcceptForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnUpload = new System.Windows.Forms.ToolBarButton();
            this.edtNumber = new System.Windows.Forms.TextBox();
            this.btnAddProduct = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.gridDocAccept = new System.Windows.Forms.DataGrid();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBufferCount = new System.Windows.Forms.Label();
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
            this.edtNumber.Location = new System.Drawing.Point(0, 30);
            this.edtNumber.Name = "edtNumber";
            this.edtNumber.Size = new System.Drawing.Size(129, 23);
            this.edtNumber.TabIndex = 1;
            // 
            // btnAddProduct
            // 
            this.btnAddProduct.Location = new System.Drawing.Point(135, 30);
            this.btnAddProduct.Name = "btnAddProduct";
            this.btnAddProduct.Size = new System.Drawing.Size(72, 23);
            this.btnAddProduct.TabIndex = 2;
            this.btnAddProduct.Text = "Принять";
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            // 
            // imgConnection
            // 
            this.imgConnection.Location = new System.Drawing.Point(214, 30);
            this.imgConnection.Name = "imgConnection";
            this.imgConnection.Size = new System.Drawing.Size(24, 23);
            // 
            // gridDocAccept
            // 
            this.gridDocAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDocAccept.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocAccept.Location = new System.Drawing.Point(0, 59);
            this.gridDocAccept.Name = "gridDocAccept";
            this.gridDocAccept.RowHeadersVisible = false;
            this.gridDocAccept.Size = new System.Drawing.Size(638, 368);
            this.gridDocAccept.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 270);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.Text = "Не выгружено:";
            // 
            // lblBufferCount
            // 
            this.lblBufferCount.Location = new System.Drawing.Point(96, 270);
            this.lblBufferCount.Name = "lblBufferCount";
            this.lblBufferCount.Size = new System.Drawing.Size(72, 20);
            this.lblBufferCount.Text = "0";
            // 
            // DocAcceptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.lblBufferCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gridDocAccept);
            this.Controls.Add(this.imgConnection);
            this.Controls.Add(this.btnAddProduct);
            this.Controls.Add(this.edtNumber);
            this.Controls.Add(this.tbrMain);
            this.Location = new System.Drawing.Point(0, 0);
            this.Menu = this.mainMenu1;
            this.Name = "DocAcceptForm";
            this.Text = "Приемка на склад";
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
    }
}