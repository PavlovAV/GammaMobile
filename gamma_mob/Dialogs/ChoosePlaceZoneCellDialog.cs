using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Dialogs
{
    public partial class ChoosePlaceZoneCellDialog : BaseForm
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
        public String PlaceZoneName { get; set; }
        public DialogResult result { get; private set; }

        private void ChoosePlaceZoneCellDialog_Load(object sender, EventArgs e)
        {
            BarcodeFunc = BarcodeReaction;
            //sdrd = new ChangeDialogResultDelegate(ChangeDialogResult);
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            var placeZoneCells = PlaceZoneRows.Select(placeZoneRow => Db.GetPlaceZoneChilds(placeZoneRow.PlaceZoneId)).ToList();
            var maxCells = placeZoneCells.Where(c => c.Count > 0).Count();//.Max();          
            //Height = 30+ maxCells * 40;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            //if (Height > Screen.PrimaryScreen.WorkingArea.Height || maxCells == 0) Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
            var buttonWidth = maxCells == 0 ? (PlaceZoneRows.Count <= 7 ? (Width - 5 - 20) : (Width - 5 - 20) / 2) : (Width / PlaceZoneRows.Count - 10) < 50 ? (Width - 5 - 20) / 3 : Width / PlaceZoneRows.Count - 10;
            var buttonHeight = 37;//maxCells == 0 ? 50 : (Height-30)/maxCells - 2;
            for (int i = 0; i < PlaceZoneRows.Count; i++)
            {
                if (placeZoneCells[i] == null || placeZoneCells[i].Count == 0)
                {
                    var button = new ButtonGuidId(PlaceZoneRows[i].PlaceZoneId);
                    button.Click += btnOK_Click;
                    button.Text = PlaceZoneRows[i].Name;
                    button.Width = buttonWidth;
                    button.Height = buttonHeight;// Height - 34;
                    button.Font = new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = maxCells == 0 ? 2 : 2 * (i + 1) + buttonWidth * i;// * (i + 1) + buttonWidth * i;
                    button.Top = maxCells == 0 ? 25 + 2 * (i + 1) + buttonHeight * i : 27;
                    Controls.Add(button);
                    continue;
                }
                for (int k = 0; k < placeZoneCells[i].Count; k++)
                {
                    var button = new ButtonGuidId(placeZoneCells[i][k].PlaceZoneId);
                    button.Click += btnOK_Click;
                    button.Text = PlaceZoneRows[i].Name + "-" +placeZoneCells[i][k].Name;
                    button.Width = buttonWidth;
                    button.Height = buttonHeight;
                    button.Font = new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = 2*(i+1)+ buttonWidth*i;
                    button.Top = 25 + 2*(k+1) + buttonHeight*k;
                    Controls.Add(button);
                }   
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //DialogResult = DialogResult.OK;
            result = DialogResult.OK;
            PlaceZoneId = (sender as ButtonGuidId).Id;
            PlaceZoneName = Convert.ToString((sender as ButtonGuidId).Text.Replace("\r\n", ""));
            Close();
        }
        /*
        delegate void ChangeDialogResultDelegate(DialogResult result);
        ChangeDialogResultDelegate sdrd;
        
        private void ChangeDialogResult(DialogResult result)
        {
            DialogResult = DialogResult.OK;
        }
        */

        private void BarcodeReaction(string barcode)
        {
            barcode = @"000008016032";
            var placeZone = Shared.PlaceZones.Where(p => p.Barcode == barcode).FirstOrDefault();
            if (placeZone != null)
            {
                //DialogResult = DialogResult.OK;
                //result = DialogResult.OK;
                Invoke((MethodInvoker)(() => DialogResult = DialogResult.OK));
                PlaceZoneId = placeZone.PlaceZoneId;
                PlaceZoneName = placeZone.Name;
                //Close();
                //Invoke((MethodInvoker)Close);
            }
            else
            {
                BarcodeFunc = null;
                MessageBox.Show(@"Ошибка! Штрих-код зоны не распознан! Попробуйте еще раз или выберите зону", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
                BarcodeFunc = BarcodeReaction;
            }
        }

    }
}