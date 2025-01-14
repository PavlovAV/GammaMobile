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
            int btnX = 1;
            foreach (var item in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var value = (Images)item.GetValue(null);
                if (activButtons.Contains((int)value))
                {
                    var btn = value == Images.PlaceFrom || value == Images.PlaceTo ? 
                        new OpenNETCF.Windows.Forms.Button2()
                        {
                            BackgroundImage = null,
                            TransparentImage = false,
                            Location = new System.Drawing.Point(btnX + buttonSize.Width, 1),
                            Name = "btn" + value.ToString(),
                            Text = value == Images.PlaceFrom ? "Откуда" : "Куда",
                            Font = new System.Drawing.Font("Tahoma",12f,System.Drawing.FontStyle.Bold),
                            Size = new System.Drawing.Size(4*buttonSize.Width, buttonSize.Height),
                            BackColor = this.BackColor,
                            BorderColor = this.BackColor
                        }
                        :
                        new OpenNETCF.Windows.Forms.Button2()
                        {
                            BackgroundImage = null,
                            TransparentImage = false,
                            Location = new System.Drawing.Point(btnX, 1),
                            Name = "btn" + value.ToString(),
                            Size = buttonSize,
                            ImageList = imageList,
                            ImageIndex = (int)Enum.Parse(typeof(Images), value.ToString(), true),
                            Enabled = !((int)value == (int)Images.ShortcutStartPointsPanelEnabled && Shared.VisibleShortcutStartPoints)
                        };
                    btn.Click += pnlToolBar_ButtonClick;
                    this.Controls.Add(btn);
                    btnX += 1 + buttonSize.Width;
                    i++;
                }
            }
        }
    }
}
