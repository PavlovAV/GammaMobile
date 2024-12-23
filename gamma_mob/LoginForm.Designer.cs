﻿namespace gamma_mob
{
    partial class LoginForm
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
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnTestWiFi = new System.Windows.Forms.Button();
            this.btnTestPing = new System.Windows.Forms.Button();
            this.btnTestSQL = new System.Windows.Forms.Button();
            this.btnExecRDP = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.TextBox();
            this.btnSetCheckConnectionMethod = new System.Windows.Forms.Button();
            this.btnSetCurrentServer = new System.Windows.Forms.Button();
            this.btnUpdateProgram = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.imgConnection = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(83, 3);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(63, 20);
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "Сеть";
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnTestWiFi
            // 
            this.btnTestWiFi.Location = new System.Drawing.Point(13, 47);
            this.btnTestWiFi.Name = "btnTestWiFi";
            this.btnTestWiFi.Size = new System.Drawing.Size(94, 20);
            this.btnTestWiFi.TabIndex = 4;
            this.btnTestWiFi.Text = "Тест WiFi";
            this.btnTestWiFi.Visible = false;
            this.btnTestWiFi.Click += new System.EventHandler(this.btnTestWiFi_Click);
            // 
            // btnTestPing
            // 
            this.btnTestPing.Location = new System.Drawing.Point(123, 47);
            this.btnTestPing.Name = "btnTestPing";
            this.btnTestPing.Size = new System.Drawing.Size(98, 20);
            this.btnTestPing.TabIndex = 5;
            this.btnTestPing.Text = "Тест ping";
            this.btnTestPing.Visible = false;
            this.btnTestPing.Click += new System.EventHandler(this.btnTestPing_Click);
            // 
            // btnTestSQL
            // 
            this.btnTestSQL.Location = new System.Drawing.Point(13, 69);
            this.btnTestSQL.Name = "btnTestSQL";
            this.btnTestSQL.Size = new System.Drawing.Size(94, 20);
            this.btnTestSQL.TabIndex = 6;
            this.btnTestSQL.Text = "Тест SQL";
            this.btnTestSQL.Visible = false;
            this.btnTestSQL.Click += new System.EventHandler(this.btnTestSQL_Click);
            // 
            // btnExecRDP
            // 
            this.btnExecRDP.Location = new System.Drawing.Point(123, 69);
            this.btnExecRDP.Name = "btnExecRDP";
            this.btnExecRDP.Size = new System.Drawing.Size(98, 20);
            this.btnExecRDP.TabIndex = 7;
            this.btnExecRDP.Text = "Запуск RDP";
            this.btnExecRDP.Visible = false;
            this.btnExecRDP.Click += new System.EventHandler(this.btnExecRDP_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.SystemColors.Control;
            this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblMessage.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.lblMessage.Location = new System.Drawing.Point(13, 117);
            this.lblMessage.Multiline = true;
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.ReadOnly = true;
            this.lblMessage.Size = new System.Drawing.Size(210, 134);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.Text = "Запуск...";
            this.lblMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lblMessage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lblMessage_KeyPress);
            // 
            // btnSetCheckConnectionMethod
            // 
            this.btnSetCheckConnectionMethod.Location = new System.Drawing.Point(123, 25);
            this.btnSetCheckConnectionMethod.Name = "btnSetCheckConnectionMethod";
            this.btnSetCheckConnectionMethod.Size = new System.Drawing.Size(98, 20);
            this.btnSetCheckConnectionMethod.TabIndex = 9;
            this.btnSetCheckConnectionMethod.Text = "";
            this.btnSetCheckConnectionMethod.Visible = false;
            this.btnSetCheckConnectionMethod.Click += new System.EventHandler(this.btnSetCheckConnectionMethod_Click);
            // 
            // btnSetCurrentServer
            // 
            this.btnSetCurrentServer.Location = new System.Drawing.Point(13, 25);
            this.btnSetCurrentServer.Name = "btnSetCurrentServer";
            this.btnSetCurrentServer.Size = new System.Drawing.Size(94, 20);
            this.btnSetCurrentServer.TabIndex = 8;
            this.btnSetCurrentServer.Text = "";
            this.btnSetCurrentServer.Visible = false;
            this.btnSetCurrentServer.Click += new System.EventHandler(this.btnSetCurrentServer_Click);
            // 
            // btnUpdateProgram
            // 
            this.btnUpdateProgram.Location = new System.Drawing.Point(13, 92);
            this.btnUpdateProgram.Name = "btnUpdateProgram";
            this.btnUpdateProgram.Size = new System.Drawing.Size(208, 20);
            this.btnUpdateProgram.TabIndex = 10;
            this.btnUpdateProgram.Text = "Проверить обновление";
            this.btnUpdateProgram.Visible = false;
            this.btnUpdateProgram.Click += new System.EventHandler(this.btnUpdateProgram_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(83, 272);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(63, 20);
            this.btnExit.TabIndex = 11;
            this.btnExit.Text = "Выход";
            this.btnExit.Visible = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // imgConnection
            // 
            this.imgConnection.Location = new System.Drawing.Point(4, 2);
            this.imgConnection.Name = "imgConnection";
            this.imgConnection.Size = new System.Drawing.Size(27, 21);
            this.imgConnection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.imgConnection);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnUpdateProgram);
            this.Controls.Add(this.btnSetCheckConnectionMethod);
            this.Controls.Add(this.btnSetCurrentServer);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnExecRDP);
            this.Controls.Add(this.btnTestSQL);
            this.Controls.Add(this.btnTestPing);
            this.Controls.Add(this.btnTestWiFi);
            this.Controls.Add(this.btnHelp);
            this.Name = "LoginForm";
            this.Text = "Авторизация";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LoginForm_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnTestWiFi;
        private System.Windows.Forms.Button btnTestPing;
        private System.Windows.Forms.Button btnTestSQL;
        private System.Windows.Forms.Button btnExecRDP;
        private System.Windows.Forms.TextBox lblMessage;
        private System.Windows.Forms.Button btnSetCheckConnectionMethod;
        private System.Windows.Forms.Button btnSetCurrentServer;
        private System.Windows.Forms.Button btnUpdateProgram;
        private System.Windows.Forms.Button btnExit;
        //private System.Windows.Forms.PictureBox imgConnection;
    }
}