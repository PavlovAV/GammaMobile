namespace gamma_mob
{
    partial class DocInventarisationNomenclatureProductsForm
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
            this.lblNomenclature = new System.Windows.Forms.Label();
            this.gridProducts = new System.Windows.Forms.DataGrid();
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnInfoProduct = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // lblNomenclature
            // 
            this.lblNomenclature.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNomenclature.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.lblNomenclature.Location = new System.Drawing.Point(0, 24);
            this.lblNomenclature.Name = "lblNomenclature";
            this.lblNomenclature.Size = new System.Drawing.Size(238, 79);
            this.lblNomenclature.Text = "label1";
            // 
            // gridProducts
            // 
            this.gridProducts.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridProducts.Location = new System.Drawing.Point(0, 103);
            this.gridProducts.Name = "gridProducts";
            this.gridProducts.RowHeadersVisible = false;
            this.gridProducts.Size = new System.Drawing.Size(238, 192);
            this.gridProducts.TabIndex = 4;
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnInfoProduct);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // DocInventarisationNomenclatureProductsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.gridProducts);
            this.Controls.Add(this.lblNomenclature);
            this.Controls.Add(this.tbrMain);
            this.Name = "DocInventarisationNomenclatureProductsForm";
            this.Text = "Продукция";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblNomenclature;
        private System.Windows.Forms.DataGrid gridProducts;
        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnInfoProduct;

    }
}