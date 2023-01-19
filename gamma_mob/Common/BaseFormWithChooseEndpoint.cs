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
        ///     Зона склада (заполнена, если ипсользуется, т.е. в складе выделены зоны и эта зона выбрана)
        /// </summary>
        protected EndPointInfo EndPointInfo { get; set; }

        protected virtual EndPointInfo GetPlaceZoneId(EndPointInfo endPointInfo)
        {
            return null;
        }
        
        private ChooseEndPointDialog ChooseEndPointForm;

        protected event AddProductReceivedEventHandler ReturnAddProductBeforeChoosedPlaceZone;
        protected event ChoosePlaceZoneEventHandler ReturnPlaceZoneBeforeChoosedPlaceZone;

        private Object Param;

        public void ChooseEndPoint()
        {
            ChooseEndPointForm = new ChooseEndPointDialog(EndPointInfo.PlaceId, this);//, barcode, fromBuffer, getProductResult, this);
            ChooseEndPointForm.Show();
            ChooseEndPointForm.SetBarcodeReaction(ChoosePlaceZoneBarcodeReaction);
        }

        public void ChooseEndPoint(AddProductReceivedEventHandler returnBeforeChoosedPlaceZone, AddProductReceivedEventHandlerParameter param)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Param = param;
            ReturnPlaceZoneBeforeChoosedPlaceZone = null;
            ReturnAddProductBeforeChoosedPlaceZone += returnBeforeChoosedPlaceZone;
            ChooseEndPoint();
        }

        public void ChooseEndPoint(ChoosePlaceZoneEventHandler returnBeforeChoosedPlaceZone)// string barcode, EndPointInfo endPointInfo, bool fromBuffer, DbProductIdFromBarcodeResult getProductResult)
        {
            Param = null;
            ReturnAddProductBeforeChoosedPlaceZone = null;
            ReturnPlaceZoneBeforeChoosedPlaceZone += returnBeforeChoosedPlaceZone;
            ChooseEndPoint();
        }

        protected void ChoosePlaceZoneBarcodeReaction(string barcode)
        {
            var placeZone = Shared.PlaceZones.Where(p => p.Barcode == barcode && p.IsValid).FirstOrDefault();
            if (placeZone != null)
            {
                if (ChooseEndPointForm != null) 
                {
                    var endPointInfo = new EndPointInfo() { PlaceId = placeZone.PlaceId, PlaceZoneId = placeZone.PlaceZoneId, PlaceZoneName = placeZone.Name };
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
                Shared.ShowMessageError(@"Ошибка! Штрих-код зоны не распознан!" + Environment.NewLine + "Попробуйте еще раз или выберите зону");
            }
        }

        public void ClosingChoosedEndPointDialog()
        {
            if (ChooseEndPointForm != null)
            {
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

        
    }
}
