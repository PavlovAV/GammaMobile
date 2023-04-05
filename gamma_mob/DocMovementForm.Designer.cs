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
            this.lblZoneName = new System.Windows.Forms.Label();
            this.btnChangeZone = new OpenNETCF.Windows.Forms.Button2();
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
            this.pnlZone.Controls.Add(this.btnChangeZone);
            this.pnlZone.Controls.Add(this.lblZoneName);
            this.pnlZone.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlZone.Location = new System.Drawing.Point(0, 0);
            this.pnlZone.Name = "pnlZone";
            this.pnlZone.Size = new System.Drawing.Size(638, 23);
            this.pnlZone.Visible = false;
            //
            // btnChangeZone
            // 
            this.btnChangeZone.Dock = System.Windows.Forms.DockStyle.Left;
            //this.btnChangeZone.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            //this.btnChangeZone.ForeColor = System.Drawing.Color.DarkRed;
            this.btnChangeZone.Location = new System.Drawing.Point(0, 0);
            this.btnChangeZone.Name = "btnChangeZone";
            this.btnChangeZone.Size = new System.Drawing.Size(22, 22);
            this.btnChangeZone.Text = "...";
            //this.btnChangeZone.Visible = false;
            this.btnChangeZone.Click += new System.EventHandler(this.btnChangeZone_Click);
            // 
            // lblZoneName
            // 
            this.lblZoneName.Dock = System.Windows.Forms.DockStyle.None;
            this.lblZoneName.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.lblZoneName.ForeColor = System.Drawing.Color.DarkRed;
            this.lblZoneName.Location = new System.Drawing.Point(23, 0);
            this.lblZoneName.Name = "lblZoneName";
            this.lblZoneName.Size = new System.Drawing.Size(616, 22);
            this.lblZoneName.Text = "lblZoneName";
            //this.lblZoneName.Visible = false;
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
        private System.Windows.Forms.Label lblZoneName;
        private OpenNETCF.Windows.Forms.Button2 btnChangeZone;
    }
}