namespace gamma_mob.Dialogs
{
    partial class ChooseEndPointDialog
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
            this.SuspendLayout();
            // 
            // ChooseEndPointDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 85);
            this.Location = new System.Drawing.Point(0, 100);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseEndPointDialog";
            this.Text = "Выбор склада";
            this.TopMost = true;
            //this.Load += new System.EventHandler(this.ChooseEndPointDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

    }
}