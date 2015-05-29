namespace gamma_mob
{
    partial class FindDocOrder
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
            this.gridDocOrders = new System.Windows.Forms.DataGrid();
            this.gridtbStyle = new System.Windows.Forms.DataGridTableStyle();
            this.txtColumnNumber = new System.Windows.Forms.DataGridTextBoxColumn();
            this.txtColumnConsignee = new System.Windows.Forms.DataGridTextBoxColumn();
            this.txtColumnDate = new System.Windows.Forms.DataGridTextBoxColumn();
            this.SuspendLayout();
            // 
            // gridDocOrders
            // 
            this.gridDocOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDocOrders.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocOrders.Location = new System.Drawing.Point(4, 6);
            this.gridDocOrders.Name = "gridDocOrders";
            this.gridDocOrders.RowHeadersVisible = false;
            this.gridDocOrders.Size = new System.Drawing.Size(631, 446);
            this.gridDocOrders.TabIndex = 2;
            this.gridDocOrders.TableStyles.Add(this.gridtbStyle);
            this.gridDocOrders.DoubleClick += new System.EventHandler(this.gridDocOrders_DoubleClick);
            // 
            // gridtbStyle
            // 
            this.gridtbStyle.GridColumnStyles.Add(this.txtColumnNumber);
            this.gridtbStyle.GridColumnStyles.Add(this.txtColumnConsignee);
            this.gridtbStyle.GridColumnStyles.Add(this.txtColumnDate);
            this.gridtbStyle.MappingName = "DocMobGroupPackOrders";
            // 
            // txtColumnNumber
            // 
            this.txtColumnNumber.Format = "";
            this.txtColumnNumber.FormatInfo = null;
            this.txtColumnNumber.HeaderText = "№";
            this.txtColumnNumber.MappingName = "DocMobGroupPackOrderID";
            this.txtColumnNumber.Width = 20;
            // 
            // txtColumnConsignee
            // 
            this.txtColumnConsignee.Format = "";
            this.txtColumnConsignee.FormatInfo = null;
            this.txtColumnConsignee.HeaderText = "Получатель";
            this.txtColumnConsignee.MappingName = "Consignee";
            this.txtColumnConsignee.NullText = "не указан";
            this.txtColumnConsignee.Width = 131;
            // 
            // txtColumnDate
            // 
            this.txtColumnDate.Format = "";
            this.txtColumnDate.FormatInfo = null;
            this.txtColumnDate.HeaderText = "Дата";
            this.txtColumnDate.MappingName = "Date";
            this.txtColumnDate.Width = 75;
            // 
            // FindDocOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridDocOrders);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindDocOrder";
            this.Text = "Выбор документа";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FindDocOrder_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridDocOrders;
        private System.Windows.Forms.DataGridTableStyle gridtbStyle;
        private System.Windows.Forms.DataGridTextBoxColumn txtColumnNumber;
        private System.Windows.Forms.DataGridTextBoxColumn txtColumnConsignee;
        private System.Windows.Forms.DataGridTextBoxColumn txtColumnDate;
    }
}