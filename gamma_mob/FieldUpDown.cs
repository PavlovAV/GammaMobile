using System;
using System.Windows.Forms;

namespace gamma_mob
{
    public partial class FieldUpDown : UserControl
    {
        public BindingSource bindingSource {get; set; }
        public string DataMember { get; set; }
        public FieldUpDown()
        {
            InitializeComponent();
            btnUp.Text = "\u25B4";
            btnDown.Text = "\u25BE";
            if (this.bindingSource != null)
                this.textBox.DataBindings.Add(new Binding("Text", this.bindingSource, this.DataMember));
        }

        public FieldUpDown(BindingSource bindingSource, string DataMember)
            : this()
        {
            this.bindingSource = bindingSource;
            this.DataMember = DataMember;
            this.textBox.DataBindings.Clear();
            this.textBox.DataBindings.Add(new Binding("Text",this.bindingSource,this.DataMember));
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            this.bindingSource.MoveNext();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            this.bindingSource.MovePrevious();
        }


    }
}
