using gamma_mob.Common;
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
            this.gridInventarisations = new System.Windows.Forms.DataGrid();
            this.SuspendLayout();
            // 
            // gridInventarisations
            // 
            this.gridInventarisations.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.gridInventarisations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInventarisations.Location = new System.Drawing.Point(0, Shared.ToolBarHeight);
            this.gridInventarisations.Name = "gridInventarisations";
            this.gridInventarisations.Size = new System.Drawing.Size(638, 431);
            this.gridInventarisations.TabIndex = 2;
            this.gridInventarisations.DoubleClick += new System.EventHandler(this.gridInventarisations_DoubleClick);
            this.gridInventarisations.CurrentCellChanged += new System.EventHandler(this.gridInventarisations_CurrentCellChanged);
            // 
            // DocInventarisationListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(638, 455);
            this.Controls.Add(this.gridInventarisations);
            this.Name = "DocInventarisationListForm";
            this.Text = "Инвентаризации";
            this.Load += new System.EventHandler(this.DocInventarisationListForm_Load);
            this.DoubleClick += new System.EventHandler(this.DocInventarisationListForm_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGrid gridInventarisations;
    }
}