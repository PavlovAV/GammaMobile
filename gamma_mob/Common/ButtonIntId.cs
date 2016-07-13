using System.Drawing;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    public sealed class ButtonIntId : Button
    {
        public int Id { get; set; }

        public ButtonIntId(int id)
        {
            Id = id;
            Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        }
    }
}
