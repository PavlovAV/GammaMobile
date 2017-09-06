namespace gamma_mob
{
    partial class DocInventarisationListForm
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
            this.btnAdd = new System.Windows.Forms.ToolBarButton();
            this.gridInventarisations = new System.Windows.Forms.DataGrid();
            this.btnInfoProduct = new System.Windows.Forms.ToolBarButton();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.btnBack);
            this.tbrMain.Buttons.Add(this.btnEdit);
            this.tbrMain.Buttons.Add(this.btnRefresh);
            this.tbrMain.Buttons.Add(this.btnAdd);
            this.tbrMain.Buttons.Add(this.btnInfoProduct);
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // gridInventarisations
            // 
            this.gridInventarisations.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridInventarisations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInventarisations.Location = new System.Drawing.Point(0, 24);
            this.gridInventarisations.Name = "gridInventarisations";
            this.gridInventarisations.Size = new System.Drawing.Size(638, 431);
            this.gridInventarisations.TabIndex = 2;
            this.gridInventarisations.DoubleClick += new System.EventHandler(this.gridInventarisations_DoubleClick);
            // 
            // DocInventarisationListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridInventarisations);
            this.Controls.Add(this.tbrMain);
            this.Name = "DocInventarisationListForm";
            this.Text = "Инвентаризации";
            this.Load += new System.EventHandler(this.DocInventarisationListForm_Load);
            this.DoubleClick += new System.EventHandler(this.DocInventarisationListForm_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton btnBack;
        private System.Windows.Forms.ToolBarButton btnEdit;
        private System.Windows.Forms.ToolBarButton btnRefresh;
        private System.Windows.Forms.DataGrid gridInventarisations;
        private System.Windows.Forms.ToolBarButton btnAdd;
        private System.Windows.Forms.ToolBarButton btnInfoProduct;
    }
}