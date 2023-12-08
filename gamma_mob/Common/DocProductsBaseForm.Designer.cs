namespace gamma_mob
{
    partial class DocProductsBaseForm
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
            this.gridProducts = new System.Windows.Forms.DataGrid();
            this.pnlQuantity = new System.Windows.Forms.Panel();
            this.qmuQuantity = new gamma_mob.Common.QuantityMeasureUnit();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lblNomenclature = new System.Windows.Forms.Label();
            this.pnlQuantity.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridProducts
            // 
            this.gridProducts.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridProducts.Location = new System.Drawing.Point(0, 119);
            this.gridProducts.Name = "gridProducts";
            this.gridProducts.PreferredRowHeight = 32;
            this.gridProducts.RowHeadersVisible = false;
            this.gridProducts.Size = new System.Drawing.Size(238, 176);
            this.gridProducts.TabIndex = 1;
            // 
            // pnlQuantity
            // 
            this.pnlQuantity.Controls.Add(this.qmuQuantity);
            this.pnlQuantity.Controls.Add(this.btnAdd);
            this.pnlQuantity.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlQuantity.Location = new System.Drawing.Point(0, 61);
            this.pnlQuantity.Name = "pnlQuantity";
            this.pnlQuantity.Size = new System.Drawing.Size(238, 58);
            // 
            // qmuQuantity
            // 
            this.qmuQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.qmuQuantity.Location = new System.Drawing.Point(0, 0);
            this.qmuQuantity.Name = "qmuQuantity";
            this.qmuQuantity.Size = new System.Drawing.Size(177, 58);
            // 
            // btnAdd
            // 
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAdd.Location = new System.Drawing.Point(177, 0);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(61, 58);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Добавить";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lblNomenclature
            // 
            this.lblNomenclature.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNomenclature.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.lblNomenclature.Location = new System.Drawing.Point(0, 0);
            this.lblNomenclature.Name = "lblNomenclature";
            this.lblNomenclature.Size = new System.Drawing.Size(238, 61);
            this.lblNomenclature.Text = "label1";
            // 
            // DocProductsBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.gridProducts);
            this.Controls.Add(this.pnlQuantity);
            this.Controls.Add(this.lblNomenclature);
            this.Name = "DocProductsBaseForm";
            this.Text = "Продукция";
            this.pnlQuantity.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridProducts;
        private System.Windows.Forms.Panel pnlQuantity;
        private gamma_mob.Common.QuantityMeasureUnit qmuQuantity;
        private System.Windows.Forms.Label lblNomenclature;
        private System.Windows.Forms.Button btnAdd;
    }
}