using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using OpenNETCF.Windows.Forms;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    public class PanelToolBar : System.Windows.Forms.Panel
    {
        public void ActivateToolBar(List<int> activButtons, EventHandler pnlToolBar_ButtonClick)
        {
            this.Dock = System.Windows.Forms.DockStyle.Top;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "pnlToolBar";
            this.Size = new System.Drawing.Size(638, Shared.ToolBarHeight);
            int i = 0;
            var buttonSize = new System.Drawing.Size(Shared.ToolBarWeight - 2, Shared.ToolBarHeight - 2);
            Type type = typeof(Images);
            ImageList imageList = Shared.ImgList;
            imageList.ImageSize = new System.Drawing.Size(Shared.ToolBarWeight - 4, Shared.ToolBarHeight - 4);
            foreach (var item in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var value = (Images)item.GetValue(null);
                if (activButtons.Contains((int)value))
                {
                    var btn = new OpenNETCF.Windows.Forms.Button2()
                    {
                        BackgroundImage = null,
                        TransparentImage = false,
                        Location = new System.Drawing.Point(1 + i * (Shared.ToolBarWeight - 2), 1),
                        Name = "btn" + value.ToString(),
                        Size = buttonSize,
                        ImageList = imageList,
                        ImageIndex = (int)Enum.Parse(typeof(Images), value.ToString(), true)
                    };
                    btn.Click += pnlToolBar_ButtonClick;
                    this.Controls.Add(btn);
                    i++;
                }
            }
        }
    }
}
