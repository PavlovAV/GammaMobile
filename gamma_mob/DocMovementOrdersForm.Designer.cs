namespace gamma_mob
{
    partial class DocMovementOrdersForm
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
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.btnBack = new System.Windows.Forms.ToolBarButton();
            this.btnEdit = new System.Windows.Forms.ToolBarButton();
            this.btnRefresh = new System.Windows.Forms.ToolBarButton();
            this.gridDocMovementOrders = new System.Windows.Forms.DataGrid();
            this.btnMovement = new System.Windows.Forms.Button();
            this.pnlGrid = new System.Windows.Forms.Panel();
            this.pnlGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnEdit);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // gridDocMovementOrders
            // 
            this.gridDocMovementOrders.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocMovementOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDocMovementOrders.Location = new System.Drawing.Point(0, 0);
            this.gridDocMovementOrders.Name = "gridDocMovementOrders";
            this.gridDocMovementOrders.RowHeadersVisible = false;
            this.gridDocMovementOrders.Size = new System.Drawing.Size(238, 251);
            this.gridDocMovementOrders.TabIndex = 2;
            this.gridDocMovementOrders.DoubleClick += new System.EventHandler(this.gridDocMovementOrders_DoubleClick);
            // 
            // btnMovement
            // 
            this.btnMovement.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMovement.Location = new System.Drawing.Point(0, 275);
            this.btnMovement.Name = "btnMovement";
            this.btnMovement.Size = new System.Drawing.Size(238, 20);
            this.btnMovement.TabIndex = 3;
            this.btnMovement.Text = "Без заказа";
            // 
            // pnlGrid
            // 
            this.pnlGrid.Controls.Add(this.gridDocMovementOrders);
            this.pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGrid.Location = new System.Drawing.Point(0, 24);
            this.pnlGrid.Name = "pnlGrid";
            this.pnlGrid.Size = new System.Drawing.Size(238, 251);
            // 
            // DocMovementOrdersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.pnlGrid);
            this.Controls.Add(this.btnMovement);
            this.Controls.Add(this.tbrMain);
            this.Name = "DocMovementOrdersForm";
            this.Text = "Заказы на перемещение";
            this.Load += new System.EventHandler(this.DocMovementOrdersForm_Load);
            this.pnlGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnEdit;
        private System.Windows.Forms.ToolBarButton btnRefresh;
        private System.Windows.Forms.DataGrid gridDocMovementOrders;
        private System.Windows.Forms.Button btnMovement;
        private System.Windows.Forms.Panel pnlGrid;
    }
}