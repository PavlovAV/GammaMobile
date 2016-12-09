using System;
using System.Drawing;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    public sealed class ButtonGuidId : Button
    {
        public Guid Id { get; set; }

        public ButtonGuidId(Guid id)
        {
            Id = id;
            Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        }
    }
}
