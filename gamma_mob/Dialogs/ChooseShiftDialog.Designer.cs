﻿namespace gamma_mob.Dialogs
{
    partial class ChooseShiftDialog
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
            this.lblCount = new System.Windows.Forms.Label();
            this.rdbShift1 = new System.Windows.Forms.RadioButton();
            this.rdbShift2 = new System.Windows.Forms.RadioButton();
            this.rdbShift3 = new System.Windows.Forms.RadioButton();
            this.rdbShift4 = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // lblCount
            // 
            this.lblCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCount.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lblCount.Location = new System.Drawing.Point(0, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(238, 24);
            this.lblCount.Text = "Укажите вашу смену";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // rdbShift1
            // 
            this.rdbShift1.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
            this.rdbShift1.Location = new System.Drawing.Point(13, 34);
            this.rdbShift1.Name = "rdbShift1";
            this.rdbShift1.Size = new System.Drawing.Size(87, 55);
            this.rdbShift1.TabIndex = 5;
            this.rdbShift1.Tag = "1";
            this.rdbShift1.Text = " 1 ";
            this.rdbShift1.CheckedChanged += new System.EventHandler(this.rdbShift_CheckedChanged);
            // 
            // rdbShift2
            // 
            this.rdbShift2.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
            this.rdbShift2.Location = new System.Drawing.Point(13, 116);
            this.rdbShift2.Name = "rdbShift2";
            this.rdbShift2.Size = new System.Drawing.Size(87, 55);
            this.rdbShift2.TabIndex = 6;
            this.rdbShift2.Tag = "2";
            this.rdbShift2.Text = " 2 ";
            this.rdbShift2.CheckedChanged += new System.EventHandler(this.rdbShift_CheckedChanged);
            // 
            // rdbShift3
            // 
            this.rdbShift3.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
            this.rdbShift3.Location = new System.Drawing.Point(129, 34);
            this.rdbShift3.Name = "rdbShift3";
            this.rdbShift3.Size = new System.Drawing.Size(89, 55);
            this.rdbShift3.TabIndex = 7;
            this.rdbShift3.Tag = "3";
            this.rdbShift3.Text = " 3 ";
            this.rdbShift3.CheckedChanged += new System.EventHandler(this.rdbShift_CheckedChanged);
            // 
            // rdbShift4
            // 
            this.rdbShift4.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
            this.rdbShift4.Location = new System.Drawing.Point(129, 116);
            this.rdbShift4.Name = "rdbShift4";
            this.rdbShift4.Size = new System.Drawing.Size(89, 55);
            this.rdbShift4.TabIndex = 8;
            this.rdbShift4.Tag = "4";
            this.rdbShift4.Text = " 4 ";
            this.rdbShift4.CheckedChanged += new System.EventHandler(this.rdbShift_CheckedChanged);
            // 
            // ChooseShiftDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 193);
            this.Controls.Add(this.rdbShift4);
            this.Controls.Add(this.rdbShift3);
            this.Controls.Add(this.rdbShift2);
            this.Controls.Add(this.rdbShift1);
            this.Controls.Add(this.lblCount);
            this.Location = new System.Drawing.Point(0, 40);
            this.Name = "ChooseShiftDialog";
            this.Text = "Выберите смену";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.RadioButton rdbShift1;
        private System.Windows.Forms.RadioButton rdbShift2;
        private System.Windows.Forms.RadioButton rdbShift3;
        private System.Windows.Forms.RadioButton rdbShift4;
    }
}