using System;
using System.Drawing;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Collections.Generic;
using OpenNETCF.Windows.Forms;
using System.Linq;

namespace gamma_mob.Dialogs
{
    public partial class ChooseEndPointDialog : BaseForm
    {
        private ChooseEndPointDialog()
        {
            InitializeComponent();
        }

        public ChooseEndPointDialog(bool isRequiredPlaceZone) : this()
        {
            IsRequiredPlaceZone = isRequiredPlaceZone;
        }

        public ChooseEndPointDialog(int placeId) : this()
        {
            IsRequiredPlaceZone = true;
            EndPointInfo = new EndPointInfo
            {
                PlaceId = placeId
            };
        }


        public ChooseEndPointDialog(int placeId, /*string productBarcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult,*/ Form parentForm)
            : this(placeId)
        {
            ParentForm = parentForm;
        }
        private bool IsRequiredPlaceZone { get; set; }

        private int? PlaceId { get; set; }

        public EndPointInfo EndPointInfo { get; set; }

        public bool FromBuffer { get; private set; }
        public DbProductIdFromBarcodeResult GetProductResult { get; private set; }
        public string ProductBarcode { get; private set; }

        private void btnUsedPlace_Click(object sender, EventArgs e)
        {
            EndPointInfo = new EndPointInfo
            {
                PlaceId = Convert.ToInt32((sender as ButtonIntId).Id),
                PlaceName = Convert.ToString((sender as ButtonIntId).Text.Replace("\r\n", ""))
            };
                
            if (!IsRequiredPlaceZone)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                SetPlaceZoneId(EndPointInfo);
            }
        }

        private void btnUsedPlaceZone_Click(object sender, EventArgs e)
        {
            EndPointInfo.PlaceZoneId = (sender as ButtonGuidId).Id;
            EndPointInfo.PlaceZoneName = Convert.ToString((sender as ButtonGuidId).Text.Replace("\r\n", ""));
                
            if (!EndPointInfo.IsAvailabilityChildPlaceZoneId)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                var placeZoneRow = GetPlaceZoneChildId(EndPointInfo);
                if (placeZoneRow != null)
                {
                    //MessageBox.Show(@"Зона выбрана!" + Environment.NewLine + placeZoneRow.PlaceZoneName);
                    EndPointInfo.PlaceZoneId = placeZoneRow.PlaceZoneId;
                    EndPointInfo.PlaceZoneName = placeZoneRow.PlaceZoneName;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    var placeZoneChilds = Db.GetPlaceZoneChilds((Guid)EndPointInfo.PlaceZoneId);
                    if (placeZoneChilds == null || placeZoneChilds.Count == 0)
                    {
                        Shared.ShowMessageError(@"Внимание! Зона не выбрана!" + Environment.NewLine + @"Перемещение не выполнено!");
                        DialogResult = DialogResult.Abort;
                        Close();
                    }
                }
            }
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = ChoosePlaceZoneFromBarcode;
            if (EndPointInfo == null || EndPointInfo.PlaceId == null)
            {
                if (!CreatePlaceButtons())
                {
                    DialogResult = DialogResult.Abort;
                    Close();
                    return;
                }
            }
            else
            {
                if (!SetPlaceZoneId(EndPointInfo))
                {
                    DialogResult = DialogResult.Abort;
                    Close();
                    return;
                }
            }
        }

        protected override void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ReturnedResult) 
                ReturnResult();
            base.OnFormClosing(sender, e);
        }

        public bool ReturnedResult = true;
        
        public void ReturnResult()
        {
            if (ParentForm != null && (ParentForm is BaseFormWithChooseEndpoint))
            {
                Invoke((MethodInvoker)delegate()
                    {
                        (ParentForm as BaseFormWithChooseEndpoint).ClosingChoosedEndPointDialog();
                    });
            }
        }

        public void SetBarcodeReaction(BarcodeReceivedEventHandler choosePlaceZoneFromBarcode)
        {
            BarcodeFunc = choosePlaceZoneFromBarcode;
        }

        private void ChoosePlaceZoneFromBarcode(string barcode)
        {
            var placeZone = Shared.PlaceZones.Where(p => p.Barcode == barcode && p.IsValid).FirstOrDefault();
            if (placeZone != null)
            {
                EndPointInfo = GetPlaceZoneChildId(new EndPointInfo() { PlaceId = placeZone.PlaceId, PlaceZoneId = placeZone.PlaceZoneId, PlaceZoneName = placeZone.Name });
                Invoke((MethodInvoker)(() => DialogResult = DialogResult.OK));
            }
            else
            {
                Shared.ShowMessageError(@"Ошибка! Штрих-код зоны не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите зону");
            }
        }

        private bool CreatePlaceButtons()
        {
            if (Shared.Warehouses == null)
            {
                Shared.ShowMessageError(@"Не удалось получить список складов");
                return false;
            }
            
            Height = 100 + (Shared.Warehouses.Count - 1)*35;

            if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
            Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
            for (int i = 0; i < Shared.Warehouses.Count; i++ )
            {
                var button = new ButtonIntId(Shared.Warehouses[i].WarehouseId);
                button.Click += btnUsedPlace_Click;
                button.Font = new Font("Tahoma",12,FontStyle.Regular);
                button.Text = Shared.Warehouses[i].WarehouseName.Length <= 11 ? Shared.Warehouses[i].WarehouseName : (Shared.Warehouses[i].WarehouseName.Substring(0, 11).Substring(10, 1) == " " ? Shared.Warehouses[i].WarehouseName.Substring(0, 10) : Shared.Warehouses[i].WarehouseName.Substring(0, 11)) + Environment.NewLine + Shared.Warehouses[i].WarehouseName.Substring(11, Math.Min(11, Shared.Warehouses[i].WarehouseName.Length - 11));
                button.Width = (Width - 5 - 20) / 2;
                button.Height = 37;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 2 + 38 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
                Controls.Add(button);
            }
            return true;
        }
        
        protected bool SetPlaceZoneId(EndPointInfo endPointInfo)
        {
            if (endPointInfo.IsSettedDefaultPlaceZoneId)
            {
                if (!endPointInfo.IsAvailabilityChildPlaceZoneId)
                {
                    EndPointInfo = endPointInfo;
                    return true;
                }
                else
                {
                    EndPointInfo = GetPlaceZoneChildId(endPointInfo);
                    return true;
                }
            }
            else
            {
                var placeZones = Db.GetWarehousePlaceZones(endPointInfo.PlaceId);
                if (placeZones != null && placeZones.Count > 0)
                {
                    //Height = 30 + placeZones.Count * 30;
                    var width = Screen.PrimaryScreen.WorkingArea.Width;
                    var height = Screen.PrimaryScreen.WorkingArea.Height;
                    //if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
                    //Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
                    for (int i = 0; i < placeZones.Count; i++)
                    {
                        this.AddButtonDelegatePatternParams(placeZones[i].PlaceZoneId, placeZones[i].Name, i, null, width, null, null);
                    }
                }

            }
            return true;
        }

        private EndPointInfo GetPlaceZoneChildId(EndPointInfo endPointInfo)
        {
            {
                var placeZoneRows = Db.GetPlaceZoneChilds((Guid)endPointInfo.PlaceZoneId);
                if (placeZoneRows == null || placeZoneRows.Count == 0)
                {
                    RemoveButtons(true);
                    return endPointInfo;
                }
                RemoveButtons(false);
                var width = Screen.PrimaryScreen.WorkingArea.Width;
                var placeZoneCells = placeZoneRows.Select(placeZoneRow => Db.GetPlaceZoneChilds(placeZoneRow.PlaceZoneId)).ToList();
                var maxCells = placeZoneCells.Where(c => c.Count > 0).Count();//.Max();          
                //Height = 30+ maxCells * 40;
                var height = Screen.PrimaryScreen.WorkingArea.Height;
                //if (Height > Screen.PrimaryScreen.WorkingArea.Height || maxCells == 0) Height = Screen.PrimaryScreen.WorkingArea.Height;
                //Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
                var buttonWidth = maxCells == 0 ? (placeZoneRows.Count <= 7 ? (width - 5 - 20) : (width - 5 - 20) / 2) : (width / placeZoneRows.Count - 10) < 50 ? (width - 5 - 20) / 3 : width / placeZoneRows.Count - 10;
                var buttonHeight = 32;//maxCells == 0 ? 50 : (Height-30)/maxCells - 2;
                for (int i = 0; i < placeZoneRows.Count; i++)
                {
                    if (placeZoneCells[i] == null || placeZoneCells[i].Count == 0)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneRows[i].PlaceZoneId, placeZoneRows[i].Name, i, null, buttonWidth, buttonHeight, maxCells);
                        continue;
                    }
                    for (int k = 0; k < placeZoneCells[i].Count; k++)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneCells[i][k].PlaceZoneId, placeZoneCells[i][k].Name, i, k, buttonWidth, buttonHeight, maxCells);
                    }
                }
            }
            return null;
        }

        List<ButtonGuidId> buttonsAdded = new List<ButtonGuidId>();

        private void AddButtonDelegatePatternParams(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate
                {
                    AddButtonDelegatePatternParams(placeZoneId, placeZoneName, i, k, width, height, maxCells);
                };
                this.Invoke(del);
                return;
            }
            AddButton(placeZoneId, placeZoneName, i, k, width, height, maxCells);
        }

        private void AddButton(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells)
        {

            Random random = new Random(2);
            //Thread.Sleep(20);

            var button = new ButtonGuidId(placeZoneId);
            if (k == null && height == null)
            {
                button.Click += btnUsedPlaceZone_Click;
                button.Font = new Font("Tahoma", 10, FontStyle.Regular);
                button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                button.Width = (width - 5 - 20) / 2;
                button.Height = 32;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 25 + 2 + 33 * Convert.ToInt32(Math.Floor(i / 2));
                Shared.MakeButtonMultiline(button);
            }
            else
            {
                if (k == null)
                {
                    button.Click += btnUsedPlaceZone_Click;
                    button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                    button.Width = width;
                    button.Height = (int)height;// Height - 34;
                    button.Font = new Font("Tahoma", 10, FontStyle.Regular); //new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = (int)maxCells == 0 ? 2 + (button.Width + 6) * Convert.ToInt32(Math.Floor(i / 7)) : 2 * (i + 1) + width * i;// * (i + 1) + buttonWidth * i;
                    button.Top = (int)maxCells == 0 ? 25 + ((int)height + 2) * (i % 7) : 27;
                    Shared.MakeButtonMultiline(button);
                }
                else
                {
                    button.Click += btnUsedPlaceZone_Click;
                    button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                    button.Width = width;
                    button.Height = (int)height;
                    button.Font = new Font("Tahoma", 10, FontStyle.Regular); //new Font(button.Font.Name, 10, button.Font.Style);
                    button.Left = 2 * (i + 1) + width * i;
                    button.Top = 25 + 2 * ((int)k + 1) + (int)height * (int)k;
                    Shared.MakeButtonMultiline(button);
                }
            }
            this.Controls.Add(button);
            buttonsAdded.Insert(0, button);

        }

        private void RemoveButtonDelegatePatternParams(ButtonGuidId buttonToRemove)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate
                {
                    RemoveButtonDelegatePatternParams(buttonToRemove);
                };
                this.Invoke(del);
                return;
            }
            RemoveButton(buttonToRemove);
        }

        private void RemoveButton(ButtonGuidId buttonToRemove)
        {
            Random random = new Random(2);
            this.Controls.Remove(buttonToRemove);
        }

        private void RemoveButtons(bool IsVisiblePanels)
        {
            var buttonsMaxIndex = buttonsAdded.Count - 1;
            for (int i = buttonsMaxIndex; i >= 0; i--)
            {
                ButtonGuidId buttonToRemove = buttonsAdded[i];
                buttonsAdded.Remove(buttonToRemove);
                this.RemoveButtonDelegatePatternParams(buttonToRemove);
            }
        }
    }
}