﻿using System;
using System.Drawing;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Collections.Generic;
using OpenNETCF.Windows.Forms;
using System.Linq;

namespace gamma_mob.Dialogs
{
    public partial class ChooseEndPointDialog : BaseFormWithToolBar
    {
        private ChooseEndPointDialog()
        {
            InitializeComponent();
        }

        public ChooseEndPointDialog(Images? placeFromOrTo)
            : this()
        {
            if (placeFromOrTo != null)
                PlaceFromOrTo = (Images)placeFromOrTo;
        }

        public ChooseEndPointDialog(bool isRequiredPlaceZone, Images? placeFromOrTo)
            : this(placeFromOrTo)
        {
            IsRequiredPlaceZone = isRequiredPlaceZone;
        }

        public ChooseEndPointDialog(int placeId, Images? placeFromOrTo)
            : this(placeId, false, placeFromOrTo)
        {}

        public ChooseEndPointDialog(int placeId, bool checkExistMovementToZone, Images? placeFromOrTo)
            : this(placeFromOrTo)
        {
            IsRequiredPlaceZone = true;
            EndPointInfo = new EndPointInfo(placeId);
            CheckExistMovementToZone = checkExistMovementToZone;
        }

        public ChooseEndPointDialog(int placeId, /*string productBarcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult,*/ Form parentForm, Images? placeFromOrTo)
            : this(placeId, parentForm, false, placeFromOrTo)
        { }

        public ChooseEndPointDialog(int placeId, /*string productBarcode, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult,*/ Form parentForm, bool checkExistMovementToZone, Images? placeFromOrTo)
            : this(placeId, checkExistMovementToZone, placeFromOrTo)
        {
            ParentForm = parentForm;
        }
        private bool IsRequiredPlaceZone { get; set; }

        private int? PlaceId { get; set; }

        private bool CheckExistMovementToZone { get; set; }

        public EndPointInfo EndPointInfo { get; private set; }

        public bool FromBuffer { get; private set; }
        public DbProductIdFromBarcodeResult GetProductResult { get; private set; }
        public string ProductBarcode { get; private set; }
        private Images PlaceFromOrTo { get; set; }

        private void btnUsedPlace_Click(object sender, EventArgs e)
        {
            EndPointInfo = new EndPointInfo(Convert.ToInt32((sender as ButtonIntId).Id),Convert.ToString((sender as ButtonIntId).Text.Replace("\r\n", "")));
                
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
            if (PlaceFromOrTo != null)
                base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)PlaceFromOrTo});
            else
                base.ActivateToolBar(new List<int>() { (int)Images.Back });
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
                EndPointInfo = GetPlaceZoneChildId(new EndPointInfo(placeZone.PlaceId, placeZone.PlaceZoneId));
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
                button.Font = new Font("Tahoma",10,FontStyle.Regular);
                button.Text = Shared.Warehouses[i].WarehouseName.Length <= 11 ? Shared.Warehouses[i].WarehouseName : (Shared.Warehouses[i].WarehouseName.Substring(0, 11).Substring(10, 1) == " " ? Shared.Warehouses[i].WarehouseName.Substring(0, 10) : Shared.Warehouses[i].WarehouseName.Substring(0, 11)) + Environment.NewLine + Shared.Warehouses[i].WarehouseName.Substring(11, Math.Min(11, Shared.Warehouses[i].WarehouseName.Length - 11));
                button.Width = (Width - 5 - 20) / 2;
                button.Height = 33;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 30 + 34 * Convert.ToInt32(Math.Floor(i / 2));
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
                var placeZones = Db.GetWarehousePlaceZones(endPointInfo.PlaceId, CheckExistMovementToZone);
                if (placeZones != null && placeZones.Count > 0)
                {
                    //Height = 30 + placeZones.Count * 30;
                    var width = Screen.PrimaryScreen.WorkingArea.Width;
                    var height = Screen.PrimaryScreen.WorkingArea.Height;
                    //if (Height > Screen.PrimaryScreen.WorkingArea.Height) Height = Screen.PrimaryScreen.WorkingArea.Height;
                    //Location = new Point(0, (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
                    for (int i = 0; i < placeZones.Count; i++)
                    {
                        this.AddButtonDelegatePatternParams(placeZones[i].PlaceZoneId, placeZones[i].Name, i, null, width, null, null, placeZones[i].IsExistMovementToZone);
                    }
                }

            }
            return true;
        }

        private EndPointInfo GetPlaceZoneChildId(EndPointInfo endPointInfo)
        {
            {
                if (endPointInfo.PlaceZoneId == null)
                {
                    RemoveButtons(true);
                    return endPointInfo;
                }
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
                var buttonHeight = 33;//maxCells == 0 ? 50 : (Height-30)/maxCells - 2;
                for (int i = 0; i < placeZoneRows.Count; i++)
                {
                    if (placeZoneCells[i] == null || placeZoneCells[i].Count == 0)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneRows[i].PlaceZoneId, placeZoneRows[i].Name, i, null, buttonWidth, buttonHeight, maxCells, null);
                        continue;
                    }
                    for (int k = 0; k < placeZoneCells[i].Count; k++)
                    {
                        this.AddButtonDelegatePatternParams(placeZoneCells[i][k].PlaceZoneId, placeZoneCells[i][k].Name, i, k, buttonWidth, buttonHeight, maxCells, null);
                    }
                }
            }
            return null;
        }

        List<ButtonGuidId> buttonsAdded = new List<ButtonGuidId>();

        private void AddButtonDelegatePatternParams(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells, bool? isBold)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker del = delegate
                {
                    AddButtonDelegatePatternParams(placeZoneId, placeZoneName, i, k, width, height, maxCells, isBold);
                };
                this.Invoke(del);
                return;
            }
            AddButton(placeZoneId, placeZoneName, i, k, width, height, maxCells, isBold);
        }

        private void AddButton(Guid placeZoneId, string placeZoneName, int i, int? k, int width, int? height, int? maxCells, bool? isBold)
        {

            Random random = new Random(2);
            //Thread.Sleep(20);
            var font = new Font("Tahoma", (isBold ?? false) ? 11 : 10, (isBold ?? false) ? FontStyle.Bold : FontStyle.Regular);
            var button = new ButtonGuidId(placeZoneId);
            if (k == null && height == null)
            {
                button.Click += btnUsedPlaceZone_Click;
                button.Font = font;
                button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                button.Width = (width - 5 - 20) / 2;
                button.Height = 33;
                button.Left = 5 + (button.Width + 6) * (i % 2);
                button.Top = 28 + 2 + 34 * Convert.ToInt32(Math.Floor(i / 2));
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
                    button.Font = font;
                    button.Left = (int)maxCells == 0 ? 2 + (button.Width + 6) * Convert.ToInt32(Math.Floor(i / 7)) : 2 * (i + 1) + width * i;// * (i + 1) + buttonWidth * i;
                    button.Top = (int)maxCells == 0 ? 28 + ((int)height + 2) * (i % 7) : 32;
                    Shared.MakeButtonMultiline(button);
                }
                else
                {
                    button.Click += btnUsedPlaceZone_Click;
                    button.Text = placeZoneName.Length <= 11 ? placeZoneName : (placeZoneName.Substring(0, 11).Substring(10, 1) == " " ? placeZoneName.Substring(0, 10) : placeZoneName.Substring(0, 11)) + Environment.NewLine + placeZoneName.Substring(11, Math.Min(11, placeZoneName.Length - 11));
                    button.Width = width;
                    button.Height = (int)height;
                    button.Font = font;
                    button.Left = 2 * (i + 1) + width * i;
                    button.Top = 28 + 2 * ((int)k + 1) + (int)height * (int)k;
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