using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;

namespace gamma_mob.Dialogs
{
    public partial class ChoosePlaceZoneCellDialog : Form
    {
        private ChoosePlaceZoneCellDialog()
        {
            InitializeComponent();
        }

        public ChoosePlaceZoneCellDialog(List<PlaceZone> placeZoneRows):this()
        {
            PlaceZoneRows = placeZoneRows;
        }

        private List<PlaceZone> PlaceZoneRows { get; set; }

        public Guid PlaceZoneId { get; set; }

        private void ChoosePlaceZoneCellDialog_Load(object sender, EventArgs e)
        {
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            var buttonWidth = Width/PlaceZoneRows.Count-3;          
            var placeZoneCells = PlaceZoneRows.Select(placeZoneRow => Db.GetPlaceZoneChilds(placeZoneRow.PlaceZoneId)).ToList();
            var maxCells = placeZoneCells.Where(c => c != null).Select(c => c.Count).Max();          
            Height = 30+ maxCells * 40;
            if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
            var buttonHeight = (Height-30)/maxCells - 2;
            for (int i = 0; i < placeZoneCells.Count; i++)
            {
                if (placeZoneCells[i] == null)
                {
                    var button = new ButtonGuidId(PlaceZoneRows[i].PlaceZoneId);
                    button.Click += btnOK_Click;
                    button.Text = PlaceZoneRows[i].Name;
                    button.Width = buttonWidth;
                    button.Height = Height - 34;
                    button.Font = new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = 2 * (i + 1) + buttonWidth * i;
                    button.Top = 2;
                    Controls.Add(button);
                    continue;
                }
                for (int k = 0; k < placeZoneCells[i].Count; k++)
                {
                    var button = new ButtonGuidId(placeZoneCells[i][k].PlaceZoneId);
                    button.Click += btnOK_Click;
                    button.Text = placeZoneCells[i][k].Name;
                    button.Width = buttonWidth;
                    button.Height = buttonHeight;
                    button.Font = new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = 2*(i+1)+ buttonWidth*i;
                    button.Top = 2*(k+1) + buttonHeight*k;
                    Controls.Add(button);
                }   
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            PlaceZoneId = (sender as ButtonGuidId).Id;
            Close();
        }
    }
}