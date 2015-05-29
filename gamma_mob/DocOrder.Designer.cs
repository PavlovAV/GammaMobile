namespace gamma_mob
{
    partial class DocOrder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocOrder));
            this.tbrMain = new System.Windows.Forms.ToolBar();
            this.tbBack = new System.Windows.Forms.ToolBarButton();
            this.tbNewDoc = new System.Windows.Forms.ToolBarButton();
            this.btnFindDocOrder = new System.Windows.Forms.ToolBarButton();
            this.imgList = new System.Windows.Forms.ImageList();
            this.edtDocDate = new System.Windows.Forms.DateTimePicker();
            this.docMobGroupPackOrdersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.GammaBase = new gamma_mob.GammaDataSet();
            this.edtNomenclature = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.edtWeight = new System.Windows.Forms.TextBox();
            this.edtGrossWeight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.edtSummaryWeight = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.edtBarCode = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.edtDocNumber = new System.Windows.Forms.TextBox();
            this.DocOrders = new gamma_mob.GammaDataSetTableAdapters.DocOrders();
            this.DocOrderGroupPacks = new gamma_mob.GammaDataSetTableAdapters.DocMobGroupPackOrderGroupPacksTableAdapter();
            this.tbOrderInfo = new System.Windows.Forms.ToolBarButton();
            ((System.ComponentModel.ISupportInitialize)(this.docMobGroupPackOrdersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GammaBase)).BeginInit();
            this.SuspendLayout();
            // 
            // tbrMain
            // 
            this.tbrMain.Buttons.Add(this.tbBack);
            this.tbrMain.Buttons.Add(this.tbNewDoc);
            this.tbrMain.Buttons.Add(this.btnFindDocOrder);
            this.tbrMain.Buttons.Add(this.tbOrderInfo);
            this.tbrMain.ImageList = this.imgList;
            this.tbrMain.Name = "tbrMain";
            this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
            // 
            // tbBack
            // 
            resources.ApplyResources(this.tbBack, "tbBack");
            // 
            // tbNewDoc
            // 
            resources.ApplyResources(this.tbNewDoc, "tbNewDoc");
            // 
            // btnFindDocOrder
            // 
            resources.ApplyResources(this.btnFindDocOrder, "btnFindDocOrder");
            this.imgList.Images.Clear();
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource"))));
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource1"))));
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource2"))));
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource3"))));
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource4"))));
            this.imgList.Images.Add(((System.Drawing.Image)(resources.GetObject("resource5"))));
            // 
            // edtDocDate
            // 
            this.edtDocDate.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.docMobGroupPackOrdersBindingSource, "Date", true));
            resources.ApplyResources(this.edtDocDate, "edtDocDate");
            this.edtDocDate.Name = "edtDocDate";
            // 
            // docMobGroupPackOrdersBindingSource
            // 
            this.docMobGroupPackOrdersBindingSource.DataMember = "DocMobGroupPackOrders";
            this.docMobGroupPackOrdersBindingSource.DataSource = this.GammaBase;
            this.docMobGroupPackOrdersBindingSource.PositionChanged += new System.EventHandler(this.docMobGroupPackOrdersBindingSource_PositionChanged);
            // 
            // GammaBase
            // 
            this.GammaBase.DataSetName = "GammaDataSet";
            this.GammaBase.Prefix = "";
            this.GammaBase.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // edtNomenclature
            // 
            this.edtNomenclature.BackColor = System.Drawing.SystemColors.ScrollBar;
            resources.ApplyResources(this.edtNomenclature, "edtNomenclature");
            this.edtNomenclature.Name = "edtNomenclature";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // edtWeight
            // 
            this.edtWeight.BackColor = System.Drawing.SystemColors.ScrollBar;
            resources.ApplyResources(this.edtWeight, "edtWeight");
            this.edtWeight.Name = "edtWeight";
            // 
            // edtGrossWeight
            // 
            this.edtGrossWeight.BackColor = System.Drawing.SystemColors.ScrollBar;
            resources.ApplyResources(this.edtGrossWeight, "edtGrossWeight");
            this.edtGrossWeight.Name = "edtGrossWeight";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // edtSummaryWeight
            // 
            this.edtSummaryWeight.BackColor = System.Drawing.SystemColors.ScrollBar;
            resources.ApplyResources(this.edtSummaryWeight, "edtSummaryWeight");
            this.edtSummaryWeight.Name = "edtSummaryWeight";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // edtBarCode
            // 
            resources.ApplyResources(this.edtBarCode, "edtBarCode");
            this.edtBarCode.Name = "edtBarCode";
            // 
            // btnSearch
            // 
            resources.ApplyResources(this.btnSearch, "btnSearch");
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // imgConnection
            // 
            resources.ApplyResources(this.imgConnection, "imgConnection");
            this.imgConnection.Name = "imgConnection";
            // 
            // edtDocNumber
            // 
            this.edtDocNumber.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.edtDocNumber.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.docMobGroupPackOrdersBindingSource, "DocMobGroupPackOrderID", true));
            resources.ApplyResources(this.edtDocNumber, "edtDocNumber");
            this.edtDocNumber.Name = "edtDocNumber";
            // 
            // DocOrders
            // 
            this.DocOrders.ClearBeforeFill = true;
            // 
            // DocOrderGroupPacks
            // 
            this.DocOrderGroupPacks.ClearBeforeFill = true;
            // 
            // tbOrderInfo
            // 
            resources.ApplyResources(this.tbOrderInfo, "tbOrderInfo");
            // 
            // DocOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.edtDocNumber);
            this.Controls.Add(this.imgConnection);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.edtBarCode);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.edtSummaryWeight);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.edtGrossWeight);
            this.Controls.Add(this.edtWeight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.edtNomenclature);
            this.Controls.Add(this.edtDocDate);
            this.Controls.Add(this.tbrMain);
            this.Name = "DocOrder";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.DocOrder_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DocOrder_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.docMobGroupPackOrdersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GammaBase)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolBar tbrMain;
        private System.Windows.Forms.ToolBarButton tbBack;
        private System.Windows.Forms.DateTimePicker edtDocDate;
        private System.Windows.Forms.ListBox edtNomenclature;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox edtWeight;
        private System.Windows.Forms.TextBox edtGrossWeight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox edtSummaryWeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox edtBarCode;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ToolBarButton tbNewDoc;
        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.PictureBox imgConnection;
        private GammaDataSet GammaBase;
        private System.Windows.Forms.BindingSource docMobGroupPackOrdersBindingSource;
        private gamma_mob.GammaDataSetTableAdapters.DocOrders DocOrders;
        private gamma_mob.GammaDataSetTableAdapters.DocMobGroupPackOrderGroupPacksTableAdapter DocOrderGroupPacks;
        private System.Windows.Forms.TextBox edtDocNumber;
        private System.Windows.Forms.ToolBarButton btnFindDocOrder;
        private System.Windows.Forms.ToolBarButton tbOrderInfo;
    }
}