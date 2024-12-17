using gamma_mob.Common;
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
            this.SuspendLayout();
            // 
            // gridDocShipmentOrders
            // 
            this.gridDocShipmentOrders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDocShipmentOrders.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridDocShipmentOrders.Location = new System.Drawing.Point(0, Shared.ToolBarHeight);
            this.gridDocShipmentOrders.Name = "gridDocShipmentOrders";
            this.gridDocShipmentOrders.RowHeadersVisible = false;
            this.gridDocShipmentOrders.Size = new System.Drawing.Size(638, 432);
            this.gridDocShipmentOrders.TabIndex = 1;
            this.gridDocShipmentOrders.DoubleClick += new System.EventHandler(this.gridDocShipmentOrders_DoubleClick);
            this.gridDocShipmentOrders.PreferredRowHeight = 10;
            this.gridDocShipmentOrders.Font = new System.Drawing.Font("Tahoma", 11, System.Drawing.FontStyle.Regular);
            this.gridDocShipmentOrders.GridLineColor = System.Drawing.SystemColors.ControlDarkDark;
            // 
            // DocOrdersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridDocShipmentOrders);
            this.Name = "DocOrdersForm";
            this.Text = "Приказы";
            this.Load += new System.EventHandler(this.DocShipmentOrders_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridDocShipmentOrders;
    }
}