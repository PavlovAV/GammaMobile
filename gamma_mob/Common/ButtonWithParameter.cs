using System.Windows.Forms;

namespace gamma_mob.Common
{
    public partial class ButtonWithParameter : UserControl
    {
        public ButtonWithParameter()
        {
            InitializeComponent();
        }

        public ButtonWithParameter(int id) : this()
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}