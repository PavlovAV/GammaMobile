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
            this.pnlGrid = new System.Windows.Forms.Panel();
            this.gridDocAccept = new System.Windows.Forms.DataGrid();
            this.pnlZone = new System.Windows.Forms.Panel();
            this.pnlZone.SuspendLayout();
            this.pnlGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlGrid
            // 
            this.pnlGrid.Controls.Add(this.gridDocAccept);
            this.pnlGrid.Controls.Add(this.pnlZone);
            this.pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGrid.Location = new System.Drawing.Point(0, 54);
            this.pnlGrid.Name = "pnlGrid";
            this.pnlGrid.Size = new System.Drawing.Size(638, 353);
            // 
            // gridDocAccept
            // 
            this.gridDocAccept.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocAccept.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDocAccept.Location = new System.Drawing.Point(0, 24);
            this.gridDocAccept.Name = "gridDocAccept";
            this.gridDocAccept.PreferredRowHeight = 32;
            this.gridDocAccept.RowHeadersVisible = false;
            this.gridDocAccept.Size = new System.Drawing.Size(638, 331);
            this.gridDocAccept.TabIndex = 5;
            this.gridDocAccept.DoubleClick += new System.EventHandler(this.gridDocAccept_DoubleClick);
            // 
            // pnlZone
            // 
            this.pnlZone.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlZone.Location = new System.Drawing.Point(0, 0);
            this.pnlZone.Name = "pnlZone";
            this.pnlZone.Size = new System.Drawing.Size(638, 23);
            this.pnlZone.Visible = false;
            // 
            // DocMovementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.pnlGrid);
            this.Name = "DocMovementForm";
            this.Text = "Перемещение на склад";
            this.pnlZone.ResumeLayout(false);
            this.pnlGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridDocAccept;
        private System.Windows.Forms.Panel pnlGrid;
        private System.Windows.Forms.Panel pnlZone;
    }
}