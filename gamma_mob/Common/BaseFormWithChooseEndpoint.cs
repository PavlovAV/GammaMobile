using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithChooseEndpoint : BaseFormWithToolBar
    {
        /// <summary>
        ///     Передел-откуда (включая зону, если есть)
        /// </summary>
        public EndPointInfo StartPointInfo { get; protected set; }
        
        /// <summary>
        ///     Передел-куда (включая зону, если есть)
        /// </summary>
        public EndPointInfo EndPointInfo { get; protected set; }

        private System.Windows.Forms.Panel PnlZone { get; set; }
        private System.Windows.Forms.Label lblEndPointZoneName { get; set; }
        private OpenNETCF.Windows.Forms.Button2 btnChangeEndPointZone { get; set; }

        protected bool checkExistMovementToZone { get; private set; }

        protected void SetCheckExistMovementToZone(bool value)
        {
            checkExistMovementToZone = value;
        }

        protected virtual EndPointInfo GetPlaceZoneId(EndPointInfo endPointInfo)
        {
            return null;
        }
        
        public ChooseEndPointDialog ChooseEndPointForm {get; private set;}

        protected event AddProductReceivedEventHandler ReturnAddProductBeforeChoosedPlaceZone;
        protected event ChoosePlaceZoneEventHandler ReturnPlaceZoneBeforeChoosedPlaceZone;

        private Object Param;

        public void ChooseEndPoint(bool checkExistMovementToZone)
        {
            ChooseEndPointForm = new ChooseEndPointDialog(EndPointInfo.PlaceId, this, checkExistMovementToZone);//, barcode, fromBuffer, getProductResult, this);
            ChooseEndPointForm.TopMost = true;
            ChooseEndPointForm.Show();
            ChooseEndPointForm.SetBarcodeReaction(ChoosePlaceZoneBarcodeReaction);
        }
        public void ChooseEndPoint(AddProductReceivedEventHandler returnBeforeChoosedPlaceZone, AddProductReceivedEventHandlerParameter param)
        {
            ChooseEndPoint(returnBeforeChoosedPlaceZone, param, false);
        }

        public void ChooseEndPoint(AddProductReceivedEventHandler returnBeforeChoosedPlaceZone, AddProductReceivedEventHandlerParameter param, bool checkExistMovementToZone)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Param = param;
            ReturnPlaceZoneBeforeChoosedPlaceZone = null;
            ReturnAddProductBeforeChoosedPlaceZone += returnBeforeChoosedPlaceZone;
            ChooseEndPoint(checkExistMovementToZone);
        }

        public void ChooseEndPoint(ChoosePlaceZoneEventHandler returnBeforeChoosedPlaceZone)
        {
            ChooseEndPoint(returnBeforeChoosedPlaceZone, false);
        }

        public void ChooseEndPoint(ChoosePlaceZoneEventHandler returnBeforeChoosedPlaceZone, bool checkExistMovementToZone)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Param = null;
            ReturnAddProductBeforeChoosedPlaceZone = null;
            ReturnPlaceZoneBeforeChoosedPlaceZone += returnBeforeChoosedPlaceZone;
            ChooseEndPoint(checkExistMovementToZone);
        }

        protected void ChoosePlaceZoneBarcodeReaction(string barcode)
        {
            var placeZone = Shared.PlaceZones.FirstOrDefault(p => p.Barcode == barcode);
            if (placeZone != null)
            {
                if (placeZone.IsValid == true)
                {
                    if (ChooseEndPointForm != null)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            ChooseEndPointForm.TopMost = false;
                        });
                        var endPointInfo = new EndPointInfo(placeZone.PlaceId, placeZone.PlaceZoneId);//, PlaceZoneName = placeZone.Name };
                        if (ReturnAddProductBeforeChoosedPlaceZone != null && (Param is AddProductReceivedEventHandlerParameter))
                        {
                            Invoke((MethodInvoker)(() => ChooseEndPointForm.ReturnedResult = false));
                            Invoke((MethodInvoker)delegate()
                            {
                                ChooseEndPointForm.Close();
                            });
                            (Param as AddProductReceivedEventHandlerParameter).endPointInfo = endPointInfo;
                            Invoke((MethodInvoker)delegate()
                            {
                                ReturnAddProductBeforeChoosedPlaceZone((Param as AddProductReceivedEventHandlerParameter));
                            });
                        }
                        else if (ReturnPlaceZoneBeforeChoosedPlaceZone != null)
                        {
                            Invoke((MethodInvoker)(() => ChooseEndPointForm.ReturnedResult = false));
                            Invoke((MethodInvoker)delegate()
                            {
                                ChooseEndPointForm.Close();
                            });
                            Invoke((MethodInvoker)delegate()
                            {
                                ReturnPlaceZoneBeforeChoosedPlaceZone(endPointInfo);
                            });
                        }
                        else
                            Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите зону");
                    }
                }
                else
                {
                    Shared.ShowMessageError(@"Ошибка! Зона " + placeZone.Name + @" отключена для использования!" + Environment.NewLine + @"Попробуйте еще раз или выберите зону");
                }
            }
            else
            {
                Shared.ShowMessageError(@"Ошибка! Штрих-код зоны не распознан!" + Environment.NewLine + @"Попробуйте еще раз или выберите зону");
            }
        }

        public void ClosingChoosedEndPointDialog()
        {
            if (ChooseEndPointForm != null)
            {
                Invoke((MethodInvoker)delegate()
                {
                    ChooseEndPointForm.TopMost = false;
                });
                        
                var endPointInfo = ChooseEndPointForm.EndPointInfo;
                if (ChooseEndPointForm.DialogResult == DialogResult.OK)
                {
                    if (ReturnAddProductBeforeChoosedPlaceZone != null && (Param is AddProductReceivedEventHandlerParameter))
                    {
                        (Param as AddProductReceivedEventHandlerParameter).endPointInfo = endPointInfo;
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnAddProductBeforeChoosedPlaceZone((Param as AddProductReceivedEventHandlerParameter));
                        });
                    }
                    else if (ReturnPlaceZoneBeforeChoosedPlaceZone != null)
                    {
                        Invoke((MethodInvoker)delegate()
                        {
                            ReturnPlaceZoneBeforeChoosedPlaceZone(endPointInfo);
                        });
                    }
                    else
                        Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите зону");
                }
                else
                    Shared.ShowMessageError(@"Ошибка! Попробуйте еще раз или выберите зону");
            }
        }

        public EndPointInfo ChangeZone(int placeId, bool checkExistMovementToZone)
        {
            using (var formPlaceZone = new ChooseEndPointDialog(placeId, checkExistMovementToZone))
            {
                DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                if (resultPlaceZone != DialogResult.OK)
                {
                    Shared.ShowMessageInformation(@"Не выбрана новая зона склада.");
                    return null;
                }
                else
                {
                    return formPlaceZone.EndPointInfo;                    
                }
            }
        }

        public void EndpointSettedInFormConstructor(EndPointInfo endPointInfo, System.Windows.Forms.Panel pnlZone)
        {
            initPanelPlaceZone(pnlZone);
            if (endPointInfo.IsSettedDefaultPlaceZoneId)
            {
                if (lblEndPointZoneName != null)
                    lblEndPointZoneName.Text = " Зона: " + endPointInfo.PlaceZoneName;
                PnlZone.Visible = true;
            }
            else if (endPointInfo.IsAvailabilityPlaceZoneId && !endPointInfo.IsSettedDefaultPlaceZoneId)
            {
                if (lblEndPointZoneName != null)
                    lblEndPointZoneName.Text = " Зона не выбрана";
                ShowMessageInformation(@"Не выбрана зона хранения по умолчанию.");
            }
        }

        private void initPanelPlaceZone(System.Windows.Forms.Panel pnlZone)
        {
            if (PnlZone == null)
            {
                PnlZone = pnlZone;
                PnlZone.SuspendLayout();
                lblEndPointZoneName = new System.Windows.Forms.Label();
                btnChangeEndPointZone = new OpenNETCF.Windows.Forms.Button2();
            
                //
                // btnChangeEndPointZone
                // 
                btnChangeEndPointZone.Dock = System.Windows.Forms.DockStyle.Left;
                //this.btnChangeEndPointZone.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
                //this.btnChangeEndPointZone.ForeColor = System.Drawing.Color.DarkRed;
                btnChangeEndPointZone.Location = new System.Drawing.Point(0, 0);
                btnChangeEndPointZone.Name = "btnChangeEndPointZone";
                btnChangeEndPointZone.Size = new System.Drawing.Size(22, 22);
                btnChangeEndPointZone.Text = "...";
                btnChangeEndPointZone.Click += new System.EventHandler(this.btnChangeEndPointZone_Click);
                // 
                // lblEndPointZoneName
                // 
                lblEndPointZoneName.Dock = System.Windows.Forms.DockStyle.None;
                lblEndPointZoneName.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
                lblEndPointZoneName.ForeColor = System.Drawing.Color.DarkRed;
                lblEndPointZoneName.Location = new System.Drawing.Point(23, 0);
                lblEndPointZoneName.Name = "lblEndPointZoneName";
                lblEndPointZoneName.Size = new System.Drawing.Size(616, 22);
                lblEndPointZoneName.Text = "lblEndPointZoneName";
                
                PnlZone.Controls.Add(btnChangeEndPointZone);
                PnlZone.Controls.Add(lblEndPointZoneName);
                PnlZone.ResumeLayout(false);
            }

        }

        private void btnChangeEndPointZone_Click(object sender, EventArgs e)
        {
            var newEndpointInfo = ChangeZone(EndPointInfo.PlaceId, checkExistMovementToZone);
            if (newEndpointInfo != null)
            {
                EndPointInfo = newEndpointInfo;
                if (lblEndPointZoneName != null)
                    lblEndPointZoneName.Text = " Зона: " + EndPointInfo.PlaceZoneName;
                EndPointInfo.IsSettedDefaultPlaceZoneId = true;
            }
        }

        protected bool ChangeStartPointZone_Click(object sender, EventArgs e)
        {
            var newStartPointInfo = ChangeZone(StartPointInfo != null && StartPointInfo.PlaceId > 0 ? StartPointInfo.PlaceId : Shared.PlaceId, false);
            if (newStartPointInfo != null)
            {
                StartPointInfo = newStartPointInfo;
                //if (lblStartPointZoneName != null)
                //    lblStartPointZoneName.Text = " Зона: " + StartPointInfo.PlaceZoneName;
                //StartPointInfo.IsSettedDefaultPlaceZoneId = true;
            }
            else
                return false;
            return true;
        }
    }
}
