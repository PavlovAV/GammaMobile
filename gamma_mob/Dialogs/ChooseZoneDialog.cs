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
using OpenNETCF.Windows.Forms;

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
            IsSetDefaultPlaceZoneId = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="placeId">ID склада</param>
        /// <param name="isSetDefaultPlaceZoneId">Признак выбора зоны по умолчанию</param>
        public ChooseZoneDialog(Form parentForm, int placeId, bool isSetDefaultPlaceZoneId)
            : this()
        {
            PlaceId = placeId;
            IsSetDefaultPlaceZoneId = isSetDefaultPlaceZoneId;
        }

        private int PlaceId { get; set; }
        private bool IsSetDefaultPlaceZoneId { get; set; } 

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
                button.Font = new Font("Tahoma", 10, FontStyle.Regular);
                button.Text = placeZones[i].Name.Length <= 11 ? placeZones[i].Name : (placeZones[i].Name.Substring(0, 11).Substring(10,1) == " " ? placeZones[i].Name.Substring(0, 10) : placeZones[i].Name.Substring(0, 11)) + Environment.NewLine + placeZones[i].Name.Substring(11, Math.Min(11, placeZones[i].Name.Length - 11));
                button.Width = (Width - 5 - 20) / 2;
                button.Height = 32;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 2 + 33 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
                Controls.Add(button);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PlaceZoneId = (sender as ButtonGuidId).Id;
            PlaceZoneName = Convert.ToString((sender as ButtonGuidId).Text.Replace("\r\n", ""));
            DialogResult = DialogResult.OK;
            //GetPlaceZoneChilds(true);
        }
        /*
        private void GetPlaceZoneChilds(bool isFromButton)
        {
            if (!IsSetDefaultPlaceZoneId)
            {
                var placeZoneRows = Db.GetPlaceZoneChilds(PlaceZoneId);
                if (placeZoneRows != null && placeZoneRows.Count > 0)
                {
                    using (var form = new ChoosePlaceZoneCellDialog(placeZoneRows))
                    {
                        //BarcodeFunc = null;
                        DialogResult result = form.ShowDialog();
                        //BarcodeFunc = BarcodeReaction1;
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
            }
            if (isFromButton)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Invoke((MethodInvoker)(() => DialogResult = DialogResult.OK));
                //Close();
                //Invoke((MethodInvoker)Close);
            }
        }

        public void BarcodeReaction1(string barcode)
        {
            barcode = @"000000015024";
            var placeZone = Shared.PlaceZones.Where(p => p.Barcode == barcode).FirstOrDefault();
            if (placeZone != null)
            {
                if (IsSetDefaultPlaceZoneId && placeZone.PlaceZoneParentId != null && placeZone.PlaceZoneParentId != Guid.Empty)
                {
                    MessageBox.Show(@"Ошибка! Нельзя указывать по умолчанию зону " + placeZone.Name + @"! Попробуйте еще раз или выберите зону", @"Ошибка. Зона " + placeZone.Name, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button3);
                    
                }
                else
                {
                    PlaceZoneId = placeZone.PlaceZoneId;
                    PlaceZoneName = placeZone.Name;
                    GetPlaceZoneChilds(false);
                }
            }
            else
            {
                MessageBox.Show(@"Ошибка! Штрих-код зоны не распознан! Попробуйте еще раз или выберите зону",@"Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Asterisk,MessageBoxDefaultButton.Button3);
                
            }
        }*/
    }
}