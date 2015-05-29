namespace gamma_mob
{
    partial class OrderInfo
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
            this.gridOrderInfo = new System.Windows.Forms.DataGrid();
            this.gridtbStyle = new System.Windows.Forms.DataGridTableStyle();
            this.colNomenclature = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colNumGroupPacks = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colWeight = new System.Windows.Forms.DataGridTextBoxColumn();
            this.colGrossWeight = new System.Windows.Forms.DataGridTextBoxColumn();
            this.SuspendLayout();
            // 
            // gridOrderInfo
            // 
            this.gridOrderInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridOrderInfo.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridOrderInfo.Location = new System.Drawing.Point(3, 3);
            this.gridOrderInfo.Name = "gridOrderInfo";
            this.gridOrderInfo.PreferredRowHeight = 32;
            this.gridOrderInfo.RowHeadersVisible = false;
            this.gridOrderInfo.Size = new System.Drawing.Size(632, 449);
            this.gridOrderInfo.TabIndex = 3;
            this.gridOrderInfo.TableStyles.Add(this.gridtbStyle);
            // 
            // gridtbStyle
            // 
            this.gridtbStyle.GridColumnStyles.Add(this.colNomenclature);
            this.gridtbStyle.GridColumnStyles.Add(this.colNumGroupPacks);
            this.gridtbStyle.GridColumnStyles.Add(this.colWeight);
            this.gridtbStyle.GridColumnStyles.Add(this.colGrossWeight);
            // 
            // colNomenclature
            // 
            this.colNomenclature.Format = "";
            this.colNomenclature.FormatInfo = null;
            this.colNomenclature.HeaderText = "Номенклатура";
            this.colNomenclature.MappingName = "Nomenclature";
            this.colNomenclature.NullText = "";
            this.colNomenclature.Width = 145;
            // 
            // colNumGroupPacks
            // 
            this.colNumGroupPacks.Format = "";
            this.colNumGroupPacks.FormatInfo = null;
            this.colNumGroupPacks.HeaderText = "Кол-во";
            this.colNumGroupPacks.MappingName = "NumGroupPacks";
            this.colNumGroupPacks.Width = 20;
            // 
            // colWeight
            // 
            this.colWeight.Format = "";
            this.colWeight.FormatInfo = null;
            this.colWeight.HeaderText = "Нетто";
            this.colWeight.MappingName = "Weight";
            this.colWeight.NullText = "0";
            this.colWeight.Width = 40;
            // 
            // colGrossWeight
            // 
            this.colGrossWeight.Format = "";
            this.colGrossWeight.FormatInfo = null;
            this.colGrossWeight.HeaderText = "Брутто";
            this.colGrossWeight.MappingName = "GrossWeight";
            this.colGrossWeight.NullText = "0";
            this.colGrossWeight.Width = 40;
            // 
            // OrderInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridOrderInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrderInfo";
            this.Text = "Документ";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridOrderInfo;
        private System.Windows.Forms.DataGridTableStyle gridtbStyle;
        private System.Windows.Forms.DataGridTextBoxColumn colNomenclature;
        private System.Windows.Forms.DataGridTextBoxColumn colNumGroupPacks;
        private System.Windows.Forms.DataGridTextBoxColumn colWeight;
        private System.Windows.Forms.DataGridTextBoxColumn colGrossWeight;
    }
}