using gamma_mob.Common;
namespace gamma_mob.Dialogs
{
    partial class GetNomenclatureCharacteristicQuantityDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pnlNomenclature = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.gridChoose = new System.Windows.Forms.DataGrid();
            this.pnlQuantity = new System.Windows.Forms.Panel();
            //this.cmbMeasureUnits = new System.Windows.Forms.ComboBox();
            this.quantityMeasureUnit = new gamma_mob.Common.QuantityMeasureUnit();
            //this.numericUpDownWithButtons1 = new gamma_mob.Common.NumericUpDownWithButtons();
            this.pnlFromPlace = new System.Windows.Forms.Panel();
            this.cmbFromPlaceZones = new System.Windows.Forms.ComboBox();
            this.lblFromPallet = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.pnlNomenclature.SuspendLayout();
            this.pnlQuantity.SuspendLayout();
            this.pnlFromPlace.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 424);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(638, 31);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(135, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 25);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(17, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 25);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // pnlNomenclature
            // 
            this.pnlNomenclature.Controls.Add(this.label1);
            this.pnlNomenclature.Controls.Add(this.gridChoose);
            this.pnlNomenclature.Controls.Add(this.pnlQuantity);
            this.pnlNomenclature.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNomenclature.Location = new System.Drawing.Point(0, 56);
            this.pnlNomenclature.Name = "pnlNomenclature";
            this.pnlNomenclature.Size = new System.Drawing.Size(638, 368);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(638, 20);
            this.label1.Text = "Номенклатура";
            //this.label1.BackColor = System.Drawing.Color.Green;
            // 
            // gridChoose
            // 
            this.gridChoose.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridChoose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridChoose.Location = new System.Drawing.Point(0, 0);
            this.gridChoose.Name = "gridChoose";
            this.gridChoose.Size = new System.Drawing.Size(638, 304);
            this.gridChoose.TabIndex = 12;
            this.gridChoose.CurrentCellChanged += new System.EventHandler(this.gridChoose_CurrentCellChanged);
            // 
            // pnlQuantity
            // 
            //this.pnlQuantity.Controls.Add(this.cmbMeasureUnits);
            this.pnlQuantity.Controls.Add(this.quantityMeasureUnit);
            //this.pnlQuantity.Controls.Add(this.numericUpDownWithButtons1);
            this.pnlQuantity.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlQuantity.Location = new System.Drawing.Point(0, 304);
            this.pnlQuantity.Name = "pnlQuantity";
            this.pnlQuantity.Size = new System.Drawing.Size(638, 52);
            //// 
            //// cmbMeasureUnits
            //// 
            //this.cmbMeasureUnits.Dock = System.Windows.Forms.DockStyle.Bottom;
            //this.cmbMeasureUnits.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            //this.cmbMeasureUnits.Location = new System.Drawing.Point(0, 35);
            //this.cmbMeasureUnits.Name = "cmbMeasureUnits";
            //this.cmbMeasureUnits.Size = new System.Drawing.Size(638, 29);
            //this.cmbMeasureUnits.TabIndex = 1;
            //this.cmbMeasureUnits.SelectedValueChanged += new System.EventHandler(this.cmbMeasureUnits_SelectedValueChanged);
            // 
            // quantityMeasureUnit
            // 
            this.quantityMeasureUnit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.quantityMeasureUnit.Location = new System.Drawing.Point(0, 35);
            this.quantityMeasureUnit.Name = "quantityMeasureUnits";
            this.quantityMeasureUnit.Size = new System.Drawing.Size(638, 29);
            //// 
            //// numericUpDownWithButtons1
            //// 
            //this.numericUpDownWithButtons1.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.numericUpDownWithButtons1.Location = new System.Drawing.Point(0, 0);
            //this.numericUpDownWithButtons1.Name = "numericUpDownWithButtons1";
            //this.numericUpDownWithButtons1.Size = new System.Drawing.Size(638, 34);
            // 
            // pnlFromPlace
            // 
            this.pnlFromPlace.Controls.Add(this.cmbFromPlaceZones);
            this.pnlFromPlace.Controls.Add(this.lblFromPallet);
            this.pnlFromPlace.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFromPlace.Location = new System.Drawing.Point(0, 0);
            this.pnlFromPlace.Name = "pnlFromPlace";
            this.pnlFromPlace.Size = new System.Drawing.Size(638, 25);
            //this.pnlFromPlace.BackColor = System.Drawing.Color.Red;
            // 
            // cmbFromPlaceZones
            // 
            this.cmbFromPlaceZones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbFromPlaceZones.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.cmbFromPlaceZones.Location = new System.Drawing.Point(0, 110);
            this.cmbFromPlaceZones.Name = "cmbFromPlaceZones";
            this.cmbFromPlaceZones.Size = new System.Drawing.Size(500, 24);
            this.cmbFromPlaceZones.TabIndex = 2;
            this.cmbFromPlaceZones.SelectedValueChanged += new System.EventHandler(this.cmbFromPlaceZones_SelectedValueChanged);
            // 
            // lblFromPallet
            // 
            this.lblFromPallet.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFromPallet.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lblFromPallet.Location = new System.Drawing.Point(0, 0);
            this.lblFromPallet.Name = "lblFromPallet";
            this.lblFromPallet.Size = new System.Drawing.Size(70, 24);
            this.lblFromPallet.Text = "Откуда";
            //this.lblFromPallet.BackColor = System.Drawing.Color.Blue;
            // 
            // GetNomenclatureCharacteristicQuantityDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.pnlNomenclature);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlFromPlace);
            this.Location = new System.Drawing.Point(0, 80);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetNomenclatureCharacteristicQuantityDialog";
            this.Text = "Выбор номенклатуры";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.pnlNomenclature.ResumeLayout(false);
            this.pnlQuantity.ResumeLayout(false);
            this.pnlFromPlace.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel pnlNomenclature;
        private System.Windows.Forms.Panel pnlFromPlace;
        private System.Windows.Forms.ComboBox cmbFromPlaceZones;
        private System.Windows.Forms.Label lblFromPallet;
        private System.Windows.Forms.Panel pnlQuantity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGrid gridChoose;
        //private System.Windows.Forms.ComboBox cmbMeasureUnits;
        private QuantityMeasureUnit quantityMeasureUnit;
        private System.Windows.Forms.Button btnCancel;
//        private NumericUpDownWithButtons numericUpDownWithButtons1;
        
    }
}