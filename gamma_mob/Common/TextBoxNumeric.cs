using System;
using System.Drawing;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    public class TextBoxNumeric : TextBox
    {
        public int? MinValue;
        public int? MaxValue;
        public int Value { get; private set; }

        public TextBoxNumeric()
        {
            //var t = this.On;
            //Controls[0].Visible = false;
        }

        public TextBoxNumeric(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public TextBoxNumeric(bool AllowOnlyPositiveValue)
        {
            MinValue = 0;
        }

        public bool SetValue(int value)
        {
            if ((MinValue == null || (MinValue != null && value >= MinValue))
                && (MaxValue == null || (MaxValue != null && value <= MaxValue)))
            {
                Value = value;
                this.Text = Value == 0 ? "" : Value.ToString();
                return true;
            }
            else 
                return false;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //int isNumber = 0;
            //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
            e.Handled = !(!char.IsLetterOrDigit(e.KeyChar) || char.IsNumber(e.KeyChar));
            base.OnKeyPress(e);
        }

        private bool undoing { get; set; }

        protected override void OnTextChanged(EventArgs e)
        {
            //int isNumber = 0;
            //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
            if (!undoing)
            {
                try
                {
                    var value = this.Text == "" ? 0 : Convert.ToInt32(this.Text);
                    if ((MinValue == null || (MinValue != null && value >= MinValue))
                        & (MaxValue == null || (MaxValue != null && value <= MaxValue)))
                    {
                        Value = value;
                        //e.Handled = !(!char.IsLetterOrDigit(e.KeyChar) || char.IsNumber(e.KeyChar));
                        base.OnTextChanged(e);
                    }
                    else
                    {
                        undoing = true;
                        this.Text = Value == 0 ? "" : Value.ToString();
                        undoing = false;
                    }
                }
                catch
                { }
            }
            else
                base.OnTextChanged(e);
        }
    }
}
