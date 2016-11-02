namespace gamma_mob
{
    partial class DocOrdersForm
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
            this.gridDocShipmentOrders = new System.Windows.Forms.DataGrid();
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnEdit = new System.Windows.Forms.ToolBarButton();
            this.btnRefresh = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // gridDocShipmentOrders
            // 
            this.gridDocShipmentOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDocShipmentOrders.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocShipmentOrders.Location = new System.Drawing.Point(0, 23);
            this.gridDocShipmentOrders.Name = "gridDocShipmentOrders";
            this.gridDocShipmentOrders.RowHeadersVisible = false;
            this.gridDocShipmentOrders.Size = new System.Drawing.Size(638, 432);
            this.gridDocShipmentOrders.TabIndex = 1;
            this.gridDocShipmentOrders.DoubleClick += new System.EventHandler(this.gridDocShipmentOrders_DoubleClick);
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnEdit);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // DocShipmentOrdersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.tbrMain);
            this.Controls.Add(this.gridDocShipmentOrders);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "DocShipmentOrdersForm";
            this.Text = "Приказы";
            this.Load += new System.EventHandler(this.DocShipmentOrders_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridDocShipmentOrders;
        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnEdit;
        private System.Windows.Forms.ToolBarButton btnRefresh;
    }
}