using System;
using System.Windows.Forms;
using gamma_mob.Common;
using System.Collections.Generic;
using gamma_mob.Models;
using gamma_mob.Dialogs;
using System.ComponentModel;
using System.Globalization;

namespace gamma_mob
{
    public partial class DocOrdersForm: BaseFormWithToolBar
    {
        private DocOrdersForm()
        {
            InitializeComponent();
        }
              
        public DocOrdersForm(Form parentForm, DocDirection docDirection):this()
        {
            ParentForm = parentForm;
            DocDirection = docDirection;
        }

        public DocOrdersForm(Form parentForm, DocDirection docDirection, bool isMovementForOrder)
            : this(parentForm, docDirection)
        {
            IsMovementForOrder = isMovementForOrder;
        }

        private DocDirection DocDirection { get; set; }

        private BindingSource BSource { get; set; }

        private BindingList<DocOrder> DocOrders { get; set; }

        private bool IsMovementForOrder { get; set; } 

        private void DocShipmentOrders_Load(object sender, EventArgs e)
        {
            switch (DocDirection)
            {
                case DocDirection.DocOut:
                    Text = "Отгрузка";
                    break;
                case DocDirection.DocIn:
                    Text = "Приемка";
                    break;
                case DocDirection.DocOutIn:
                    if (IsMovementForOrder)
                        Text = "Сборка под отгрузку";
                    else
                        Text = "Заказы на перемещение";
                    break;
            }

            GetDocOrders();
            gridDocShipmentOrders.DataSource = BSource;
            var tableStyle = new DataGridTableStyle {MappingName = BSource.GetListName(null)};
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "DocOrderId",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    MappingName = "OrderType",
                    Width = -1
                });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                //HeaderText = "Откуда Id",
                MappingName = "OutPlaceId",
                Width = -1
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                //HeaderText = "Куда Id",
                MappingName = "InPlaceId",
                Width = -1
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
            {
                HeaderText = " ",
                MappingName = "State",
                Width = 14
            });
            tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                {
                    HeaderText = "Номер",
                    MappingName = "Number",
                    Width = 80
                });
            switch (DocDirection)
            {
                case DocDirection.DocOut:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Получатель",
                        MappingName = "Consignee",
                        Width = 140
                    });
                    break;
                case DocDirection.DocIn:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Получатель",
                        MappingName = "Consignee",
                        Width = 140
                    });
                    break;
                case DocDirection.DocOutIn:
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Откуда",
                        MappingName = "OutPlaceName",
                        Width = 70
                    });
                    tableStyle.GridColumnStyles.Add(new DataGridTextBoxColumn
                    {
                        HeaderText = "Куда",
                        MappingName = "InPlaceName",
                        Width = 70
                    });
                    break;
            }
            gridDocShipmentOrders.TableStyles.Add(tableStyle);
            //Shared.RefreshBarcodes1C();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            base.ActivateToolBar(new List<int>() { (int)Images.Back, (int)Images.Edit, (int)Images.Refresh, (int)Images.InfoProduct, (int)Images.RDP, (int)Images.Remove });//, pnlToolBar_ButtonClick);
        }

        protected override void EditToolBarButton()
        {
            EditDocOrder();
        }

        protected override void RefreshToolBarButton()
        {
            GetDocOrders();
        }

        private void GetDocOrders()
        {
            DocOrders = Db.PersonDocOrders(Shared.PersonId, DocDirection, IsMovementForOrder);
            if (BSource == null)
                BSource = new BindingSource(DocOrders, null);
            else
            {
                BSource.DataSource = DocOrders;
            }
        }

        private void gridDocShipmentOrders_DoubleClick(object sender, EventArgs e)
        {
            EditDocOrder();
        }

        private void EditDocOrder()
        {
            Cursor.Current = Cursors.WaitCursor;
            int row = gridDocShipmentOrders.CurrentRowIndex;
            if (row >= 0)
            {
                DocOrder selectedDocOrder = null;
                foreach (var item in DocOrders)
                {
                    if (item.DocOrderId == new Guid(gridDocShipmentOrders[row, 0].ToString()))
                    {
                        selectedDocOrder = item;
                        break;
                    }
                }
                /*var id = new Guid(gridDocShipmentOrders[row, 0].ToString());
                var orderType = (OrderType) Convert.ToInt32(gridDocShipmentOrders[row, 1]);
                var inPlaceId = gridDocShipmentOrders[row, 3] == null ? (int?)null : Convert.ToInt32(gridDocShipmentOrders[row, 3]);
                 */
                if (selectedDocOrder != null)
                {
                    EndPointInfo endPointInfo = null;
                    var inPlaceID = selectedDocOrder.InPlaceID;
                    if (IsMovementForOrder)
                    {
                        inPlaceID = (int)selectedDocOrder.OutPlaceID;
                        string message = "Укажите зону сборки продкции под отгрузку";
                            var dialogResult = Shared.ShowMessageInformation(message);
                            //if (dialogResult == DialogResult.Yes)
                            {
                                using (var formPlaceZone = new ChooseEndPointDialog((int)inPlaceID))
                                {
                                    DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                                    if (resultPlaceZone != DialogResult.OK)
                                    {
                                        Shared.ShowMessageInformation(@"Не выбрана зона склада.");
                                        //endPointInfo.IsSettedDefaultPlaceZoneId = false;
                                        return;
                                    }
                                    else
                                    {
                                        endPointInfo = formPlaceZone.EndPointInfo;
                                        endPointInfo.IsSettedDefaultPlaceZoneId = true;

                                    }
                                }
                            }
                    }
                    else if (selectedDocOrder.InPlaceID != null)
                    {
                        endPointInfo = new EndPointInfo((int)selectedDocOrder.InPlaceID);
                        //using (var form = new ChooseEndPointDialog(false))
                        //{
                        //    DialogResult result = form.ShowDialog();
                        //    if (result != DialogResult.OK) return;
                        //    endPointInfo = form.EndPointInfo;
                        if ((endPointInfo.IsAvailabilityPlaceZoneId && endPointInfo.PlaceZoneId == null) || (endPointInfo.IsAvailabilityChildPlaceZoneId && endPointInfo.PlaceZoneId != null))
                        {
                            string message = (endPointInfo.IsAvailabilityChildPlaceZoneId && endPointInfo.PlaceZoneId != null) ? "Вы не до конца указали зону. Попробуете еще раз?" : "Вы будете сейчас указывать зону хранения по умолчанию?";
                            var dialogResult = Shared.ShowMessageQuestion(message);
                            if (dialogResult == DialogResult.Yes)
                            {
                                using (var formPlaceZone = new ChooseEndPointDialog(endPointInfo.PlaceId))
                                {
                                    DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                                    if (resultPlaceZone != DialogResult.OK)
                                    {
                                        Shared.ShowMessageInformation(@"Не выбрана зона склада.");
                                        endPointInfo.IsSettedDefaultPlaceZoneId = false;
                                    }
                                    else
                                    {
                                        endPointInfo = formPlaceZone.EndPointInfo;
                                        endPointInfo.IsSettedDefaultPlaceZoneId = true;

                                    }
                                }
                            }
                        }
                        else if (endPointInfo.PlaceZoneId != null)
                        {
                            endPointInfo.IsSettedDefaultPlaceZoneId = true;
                        }
                        //}
                    }
                    EndPointInfo startPointInfo = selectedDocOrder.OutPlaceID == null ? null : new EndPointInfo((int)selectedDocOrder.OutPlaceID);
                    if (selectedDocOrder.IsControlExec && selectedDocOrder.StartExec == null)
                    {
                        if (Shared.ShowMessageQuestion(@"Вы начинаете погрузку? (приказ " + selectedDocOrder.Number + ")") == DialogResult.Yes)
                        {
                            UIServices.SetBusyState(this);
                            var result = Db.UpdateStartExecInDocOrder(selectedDocOrder.DocOrderId, Shared.PersonId);
                            UIServices.SetNormalState(this);
                            if (result == null)
                            {
                                Shared.ShowMessageError("Ошибка при сохранении времени начала погрузки. Попробуйте еще раз.");
                            }
                            else
                            {
                                try
                                {
                                    CultureInfo culture = new CultureInfo("ru-RU");
                                    selectedDocOrder.StartExec = Convert.ToDateTime(result, culture);
                                }
                                catch 
                                {
                                    Shared.ShowMessageError(result);
                                }
                            }
                        }
                    }
                    var docOrderForm = inPlaceID == null
                        ? new DocWithNomenclatureForm(selectedDocOrder.DocOrderId, this, selectedDocOrder.Number,
                        selectedDocOrder.OrderType, DocDirection, IsMovementForOrder, Shared.MaxAllowedPercentBreak, startPointInfo, selectedDocOrder.IsControlExec, selectedDocOrder.StartExec)
                        : new DocWithNomenclatureForm(selectedDocOrder.DocOrderId, this, selectedDocOrder.Number,
                        selectedDocOrder.OrderType, DocDirection, IsMovementForOrder, Shared.MaxAllowedPercentBreak, startPointInfo, endPointInfo, selectedDocOrder.IsControlExec, selectedDocOrder.StartExec);
                    docOrderForm.Show();
                    if (!docOrderForm.IsDisposed && docOrderForm.Enabled)
                        Hide();
                }
                else
                    Shared.SaveToLogError(@"Ошибка при поиске строки {"+row.ToString()+"} в списке приказов");
            } 
            Cursor.Current = Cursors.Default;
            
        }

        protected override void RemoveToolBarButton()
        {
            int row = gridDocShipmentOrders.CurrentRowIndex;
            if (row >= 0)
            {
                DocOrder selectedDocOrder = null;
                foreach (var item in DocOrders)
                {
                    if (item.DocOrderId == new Guid(gridDocShipmentOrders[row, 0].ToString()))
                    {
                        selectedDocOrder = item;
                        break;
                    }
                }
                /*var id = new Guid(gridDocShipmentOrders[row, 0].ToString());
                var orderType = (OrderType) Convert.ToInt32(gridDocShipmentOrders[row, 1]);
                var inPlaceId = gridDocShipmentOrders[row, 3] == null ? (int?)null : Convert.ToInt32(gridDocShipmentOrders[row, 3]);
                 */
                if (selectedDocOrder != null && selectedDocOrder.IsControlExec)
                {
                    if (Shared.ShowMessageQuestion(@"Вы закончили погрузку? (приказ " + selectedDocOrder.Number+ ")") == DialogResult.Yes)
                    {
                        UIServices.SetBusyState(this);
                        var result = Db.UpdateEndExecInDocOrder(selectedDocOrder.DocOrderId, Shared.PersonId);
                        UIServices.SetNormalState(this);
                        if (result == null)
                        {
                            Shared.ShowMessageError("Ошибка при сохранении времени окончания погрузки. Попробуйте еще раз.");
                        }
                        else
                        {
                            try
                            {
                                CultureInfo culture = new CultureInfo("ru-RU");
                                selectedDocOrder.EndExec = Convert.ToDateTime(result, culture);
                            }
                            catch
                            {
                                Shared.ShowMessageError(result);
                            }
                        }
                    }
                }
            }
        }
    }
}