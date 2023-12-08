using System;
using System.Drawing;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    public class NumericUpDownWithButtons : Panel
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUP;
        private TextBoxNumeric edtQuantity;
        
        private System.Drawing.Font _font {get; set; }
        public override System.Drawing.Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                btnDown.Font = value;
                btnUP.Font = value;
                edtQuantity.Font = value;
            }
        }
        
        public int Value
            { 
            get { return edtQuantity.Value; }
            set 
                {
                    edtQuantity.SetValue(value);
                }
        }

        public int MinEditWidth { get; set; }
        public int MaxEditWidth { get; set; }

        public NumericUpDownWithButtons()
        {
            MinEditWidth = 50;
            MaxEditWidth = 80;
            this.panel1 = new System.Windows.Forms.Panel();
            this.label = new System.Windows.Forms.Label();
            this.edtQuantity = new TextBoxNumeric(true);
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUP = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.edtQuantity);
            this.Controls.Add(this.btnUP);
            this.Location = new System.Drawing.Point(1, 2);
            this.Name = "pnlNumericUpDown";
            this.Size = new System.Drawing.Size(160, 31);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label);
            this.panel1.Controls.Add(this.btnDown);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(60, 29);
            this.panel1.SendToBack();
            // 
            // label
            // 
            this.label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            this.label.Location = new System.Drawing.Point(0, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(42, 29);
            this.label.Text = "Кол-во";
            // 
            // edtQuantity
            // 
            this.edtQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.edtQuantity.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            this.edtQuantity.Location = new System.Drawing.Point(77, 0);
            this.edtQuantity.Name = "edtQuantity";
            this.edtQuantity.Text = "1";
            this.edtQuantity.Size = new System.Drawing.Size(42, 29);
            this.edtQuantity.TabIndex = 2;
            this.edtQuantity.WordWrap = false;
            this.edtQuantity.TextAlign = HorizontalAlignment.Center;
            this.edtQuantity.SetValue(1);
            // 
            // btnDown
            // 
            this.btnDown.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnDown.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.btnDown.Location = new System.Drawing.Point(45, 0);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(29, 29);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "-";
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUP
            // 
            this.btnUP.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUP.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.btnUP.Location = new System.Drawing.Point(130, 0);
            this.btnUP.Name = "btnUP";
            this.btnUP.Size = new System.Drawing.Size(29, 29);
            this.btnUP.TabIndex = 0;
            this.btnUP.Text = "+";
            this.btnUP.Click += new System.EventHandler(this.btnUP_Click);
            this.ResumeLayout(false);
        }
        
        private void btnUP_Click(object sender, EventArgs e)
        {
            edtQuantity.Text = (Convert.ToInt32(edtQuantity.Text) + 1).ToString();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            edtQuantity.Text = (Convert.ToInt32(edtQuantity.Text) - 1).ToString();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int edtWidth = Convert.ToInt32(this.Width * 0.5);
            if (edtWidth != edtQuantity.Width)
            {
                var edtQuantityWidth = (edtWidth >= MinEditWidth && edtWidth <= MaxEditWidth ? edtWidth : (edtWidth < MinEditWidth ? MinEditWidth : (edtWidth > MaxEditWidth ? MaxEditWidth : edtQuantity.Width)));
                //edtQuantity.Width = edtQuantityWidth;
                int btnWidth = Convert.ToInt32((this.Width - edtQuantityWidth) * 0.3);
                if (btnWidth > 0)//(btnWidth >= MinButtonWidth && btnWidth <= MaxButtonWidth && btnWidth != btnDown.Width)
                {
                    panel1.Width = btnWidth*2;
                    btnDown.Width = btnWidth;
                    btnUP.Width = btnWidth;
                    if (edtQuantityWidth <= MinEditWidth)
                        edtQuantity.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
                    else if (edtQuantityWidth >= MaxEditWidth)
                        edtQuantity.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
                    else
                        edtQuantity.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
                }
            }
        }
    }
}
