using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob.Dialogs
{
    public partial class ChooseZoneDialog : Form
    {
        private ChooseZoneDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Сюда попадет id конечной зоны
        /// </summary>
        public Guid PlaceZoneId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="placeId">ID склада</param>
        public ChooseZoneDialog(int placeId): this()
        {
            PlaceId = placeId;
        }

        private int PlaceId { get; set; }

        private void ChooseZoneDialog_Load(object sender, EventArgs e)
        {
            var placeZones = Db.GetWarehousePlaceZones(PlaceId);
            Height = 30 + placeZones.Count * 30;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
            for (int i = 0; i < placeZones.Count; i++)
            {
                var button = new ButtonGuidId(placeZones[i].PlaceZoneId);
                button.Click += btnOK_Click;
                button.Text = placeZones[i].Name;
                button.Width = Convert.ToInt32(GraphFuncs.TextSize(placeZones[i].Name, button.Font).Width + 8);
                button.Height = 30;
                button.Left = (Width - button.Width) / 2;
                button.Top = 2*(i+1) + 30 * i;
                Controls.Add(button);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PlaceZoneId = (sender as ButtonGuidId).Id;
            var placeZoneRows = Db.GetPlaceZoneChilds(PlaceZoneId);
            if (placeZoneRows != null && placeZoneRows.Count > 0)
            {
                using (var form = new ChoosePlaceZoneCellDialog(placeZoneRows))
                {
                    DialogResult result = form.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        Close();
                        return;
                    }
                    PlaceZoneId = form.PlaceZoneId;
                }
                Hide();
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}