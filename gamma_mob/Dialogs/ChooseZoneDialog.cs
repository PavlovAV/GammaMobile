﻿using System;
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
        public String PlaceZoneName { get; set; }

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
                /*button.Text = placeZones[i].Name;
                button.Width = Convert.ToInt32(GraphFuncs.TextSize(placeZones[i].Name, button.Font).Width + 8);
                button.Height = 30;
                button.Left = (Width - button.Width) / 2;
                button.Top = 2*(i+1) + 30 * i;
                */
                button.Font = new Font("Tahoma", 12, FontStyle.Regular);
                button.Text = placeZones[i].Name.Length <= 11 ? placeZones[i].Name : (placeZones[i].Name.Substring(0, 11).Substring(10,1) == " " ? placeZones[i].Name.Substring(0, 10) : placeZones[i].Name.Substring(0, 11)) + Environment.NewLine + placeZones[i].Name.Substring(11, Math.Min(11, placeZones[i].Name.Length - 11));
                button.Width = (Width - 5 - 20) / 2;
                button.Height = 37;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 2 + 38 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
                Controls.Add(button);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PlaceZoneId = (sender as ButtonGuidId).Id;
            PlaceZoneName = Convert.ToString((sender as ButtonGuidId).Text.Replace("\r\n", ""));
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
                    PlaceZoneName = form.PlaceZoneName;
                }
                Hide();
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}