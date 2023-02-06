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
            this.pnlZone = new System.Windows.Forms.Panel();
            this.gridDocOrder = new System.Windows.Forms.DataGrid();
            this.lblZoneName = new System.Windows.Forms.Label();
            this.pnlZone.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlZone
            // 
            this.pnlZone.Controls.Add(this.gridDocOrder);
            this.pnlZone.Controls.Add(this.lblZoneName);
            this.pnlZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlZone.Location = new System.Drawing.Point(0, 54);
            this.pnlZone.Name = "pnlZone";
            this.pnlZone.Size = new System.Drawing.Size(638, 353);
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
            // lblZoneName
            // 
            this.lblZoneName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblZoneName.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.lblZoneName.ForeColor = System.Drawing.Color.DarkRed;
            this.lblZoneName.Location = new System.Drawing.Point(0, 0);
            this.lblZoneName.Name = "lblZoneName";
            this.lblZoneName.Size = new System.Drawing.Size(638, 22);
            this.lblZoneName.Text = "lblZoneName";
            this.lblZoneName.Visible = false;
            // 
            // DocWithNomenclatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.pnlZone);
            this.Name = "DocWithNomenclatureForm";
            this.Text = "title";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridDocOrder;
        private System.Windows.Forms.Panel pnlZone;
        private System.Windows.Forms.Label lblZoneName;
    }
}