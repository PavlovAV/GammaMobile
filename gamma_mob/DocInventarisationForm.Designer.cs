namespace gamma_mob
{
    partial class DocInventarisationForm
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
            this.gridInventarisation = new System.Windows.Forms.DataGrid();
            this.pnlZone = new System.Windows.Forms.Panel();
            this.lblZoneName = new System.Windows.Forms.Label();
            this.pnlZone.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridInventarisation
            // 
            this.gridInventarisation.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridInventarisation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInventarisation.Location = new System.Drawing.Point(0, 22);
            this.gridInventarisation.Name = "gridInventarisation";
            this.gridInventarisation.PreferredRowHeight = 32;
            this.gridInventarisation.RowHeadersVisible = false;
            this.gridInventarisation.Size = new System.Drawing.Size(638, 309);
            this.gridInventarisation.TabIndex = 8;
            this.gridInventarisation.DoubleClick += new System.EventHandler(this.gridInventarisation_DoubleClick);
            this.gridInventarisation.CurrentCellChanged += new System.EventHandler(this.gridInventarisation_CurrentCellChanged);
            // 
            // pnlZone
            // 
            this.pnlZone.Controls.Add(this.gridInventarisation);
            this.pnlZone.Controls.Add(this.lblZoneName);
            this.pnlZone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlZone.Location = new System.Drawing.Point(0, 64);
            this.pnlZone.Name = "pnlZone";
            this.pnlZone.Size = new System.Drawing.Size(638, 331);
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
            // DocInventarisationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.pnlZone);
            this.Name = "DocInventarisationForm";
            this.Text = "DocInventarisationForm";
            this.pnlZone.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridInventarisation;
        private System.Windows.Forms.Panel pnlZone;
        private System.Windows.Forms.Label lblZoneName;
    }
}