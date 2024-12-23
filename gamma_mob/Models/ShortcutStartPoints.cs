using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using System.Windows.Forms;

namespace gamma_mob.Models
{
    public class ShortcutStartPoints
    {
        private System.Windows.Forms.Button btnStartPoint;
        public System.Windows.Forms.Panel pnlStartPoint {get; private set; }

        //public EventHandler btnStartPoint_Click;
        //public EventHandler btnStartPoint0_Click;
        private event BarcodeReceivedEventHandler BtnStartPoint0ClickReaction;
        public bool IsExist { get { return pnlStartPoint != null; } }
        //private List<System.Windows.Forms.Button> StartPointButtons { get; set; }

        private List<Warehouse> _shortcutStartPoints { get; set; }
        public List<Warehouse> shortcutStartPoints
        {
            get
            {
                if (_shortcutStartPoints == null)
                {
                    List<Warehouse> list = Db.GetShortcutStartPoints();
                    if (list == null) return new List<Warehouse>();
                    if (list.Count > 0 && !Shared.VisibleShortcutStartPoints) Shared.VisibleShortcutStartPoints = true;
                    _shortcutStartPoints = list;
                }
                return _shortcutStartPoints;
            }
        }


        public ShortcutStartPoints(BarcodeReceivedEventHandler btnStartPoint0ClickReaction)//EventHandler btnStartPoint_Click, EventHandler btnStartPoint0_Click)
        {
            //if (shortcutStartPoints != null && shortcutStartPoints.Count > 0)
            {
                BtnStartPoint0ClickReaction = btnStartPoint0ClickReaction;
                pnlStartPoint = new System.Windows.Forms.Panel()
                {
                    Dock = System.Windows.Forms.DockStyle.Top,
                    Location = new System.Drawing.Point(0, Shared.ToolBarHeight),
                    Name = "pnlStartPoint",
                    Size = new System.Drawing.Size(638, Shared.ToolBarHeight)
                };
                btnStartPoint = new System.Windows.Forms.Button()
                {
                    Location = new System.Drawing.Point(0, 1),
                    Name = "btnStartPoint",
                    Size = new System.Drawing.Size(20, Shared.ToolBarHeight - 2),
                    Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Bold),
                    Text = "|->",
                };
                btnStartPoint.Click += new System.EventHandler(btnStartPoint_Click);
                pnlStartPoint.Controls.Add(btnStartPoint);
                //StartPointButtons = new List<Button>();
                int i = 0;
                foreach (var warehouse in shortcutStartPoints)
                {
                    i++;
                    var btn = new System.Windows.Forms.Button()
                    {
                        Location = new System.Drawing.Point(21 + (54 * (i - 1)), 1),
                        //Dock = System.Windows.Forms.DockStyle.Left,
                        Name = "btnStartPoint" + i.ToString(),
                        Size = new System.Drawing.Size(54, Shared.ToolBarHeight - 2),
                        //TabIndex = 2,
                        Text = warehouse.WarehouseShortName,
                        Tag = warehouse.WarehouseId
                    };
                    btn.Click += new System.EventHandler(btnStartPoint0_Click);
                    //StartPointButtons.Add(btn);
                    pnlStartPoint.Controls.Add(btn);
                }
                pnlStartPoint.Controls.Add(btnStartPoint);
                //pnlSearch.Controls.Add(pnlStartPoint);
            }
        }

        public void btnStartPoint_Click(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation(@"Нажата кнопка Изменение списка складов откуда для быстрого запуска ");
            //var btn = (sender as System.Windows.Forms.Button);
            using (var form = new ChooseEndPointDialog(false))
            {
                System.Windows.Forms.DialogResult result = form.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK) return;
                //ChangeShortcutStartPoints(form.EndPointInfo.PlaceId);
                //MessageBox.Show(form.EndPointInfo.PlaceName);
                if (shortcutStartPoints.Any(b => b.WarehouseId == form.EndPointInfo.PlaceId))
                {
                    if (Shared.ShowMessageQuestion(form.EndPointInfo.PlaceName + " уже выбран." + Environment.NewLine + "Вы хотите удалить кнопку?", "QUEST DelShortcutStartPoint button", null, null) == DialogResult.Yes)
                    {
                        if (Db.DelShortcutStartPoint(form.EndPointInfo.PlaceId))
                        {
                            Control removeControl = null;
                            foreach(Control c in pnlStartPoint.Controls)
                            {
                                if (c is Button && (c as Button).Tag != null && (int)(c as Button).Tag == form.EndPointInfo.PlaceId)
                                {
                                    removeControl = c;
                                    break;
                                }
                            }
                            if (removeControl!= null) pnlStartPoint.Controls.Remove(removeControl);
                            shortcutStartPoints.Remove(shortcutStartPoints.First(b => b.WarehouseId == form.EndPointInfo.PlaceId));
                        }
                        else
                            Shared.ShowMessageError("Ошибка при удалении кнопки " + form.EndPointInfo.PlaceName);
                    }
                }
                else
                {
                    if (shortcutStartPoints.Count < 4)
                    {
                        var i = shortcutStartPoints.Count+1;//без +1 так как в if(Db.AddShortcutStartPoint(form.EndPointInfo.PlaceId)) уже добавилась запись и count уже 1
                        if (Db.AddShortcutStartPoint(form.EndPointInfo.PlaceId))
                        {
                            var warehouse = Shared.Warehouses.FirstOrDefault(w => w.WarehouseId == form.EndPointInfo.PlaceId);
                            if (warehouse != null)
                            {
                                //var i = shortcutStartPoints.Count;//без +1 так как в if(Db.AddShortcutStartPoint(form.EndPointInfo.PlaceId)) уже добавилась запись и count уже 1
                                var btn = new System.Windows.Forms.Button()
                                {
                                    Location = new System.Drawing.Point(21 + (54 * (i - 1)), 1),
                                    //Dock = System.Windows.Forms.DockStyle.Left,
                                    Name = "btnStartPoint" + i.ToString(),
                                    Size = new System.Drawing.Size(54, Shared.ToolBarHeight - 2),
                                    //TabIndex = 2,
                                    Text = warehouse.WarehouseShortName,
                                    Tag = warehouse.WarehouseId
                                };
                                btn.Click += new System.EventHandler(btnStartPoint0_Click);
                                shortcutStartPoints.Add(warehouse);
                                pnlStartPoint.Controls.Add(btn);
                                if (!Shared.VisibleShortcutStartPoints) Shared.VisibleShortcutStartPoints = true;
                            }
                        }
                        else
                            Shared.ShowMessageError("Ошибка при добавлении кнопки " + form.EndPointInfo.PlaceName);
                    }
                    else
                        Shared.ShowMessageInformation("Кнопка <" + form.EndPointInfo.PlaceName + "> не добавлена." + Environment.NewLine + "Уже выведено максимальное кол-во кнопок." + Environment.NewLine + "Сначала удалите какую-нибудь кнопку.");
                }
            }
        }

        public void btnStartPoint0_Click(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation(@"Нажата кнопка склада откуда: " + (sender as System.Windows.Forms.Button).Tag.ToString());
            BtnStartPoint0ClickReaction(Shared.Warehouses.FirstOrDefault(w => w.WarehouseId == (int)(sender as System.Windows.Forms.Button).Tag).Barcode);
        }

    }
}
