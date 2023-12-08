using System;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using System.IO;
using System.Reflection;
using OpenNETCF.Windows.Forms;

namespace gamma_mob
{
    public partial class MainForm : BaseFormWithShowMessage
    {
        public MainForm()
        {
            InitializeComponent();
            if (Shared.IsAvailabilityCreateNewPalletNotOnOrder)
                btnComplectPallet.Visible = Shared.IsAvailabilityCreateNewPalletNotOnOrder;
            else
                btnCloseApp.Top = btnComplectPallet.Top;
            if (Shared.ShiftId > 0)
            {
                //btnCloseShift.Visible = true;
                btnCloseShift.Text = "Закрытие смены";
                //btnInventarisation.Visible = false;
            }
            else
            {
                //btnCloseShift.Visible = true;
                btnCloseShift.Text = "Информация";
                //btnInventarisation.Visible = true;
            }
            //btnUserInfo.Text = "Логин: " + Settings.UserName + " (" + Shared.PersonName + ")";
            userInfoTextId = 0;
            if (Shared.TimerForBarcodesUpdate == null)
            {
                ShowMessageInformation(@"Внимание! Не запущена автоматическая" + Environment.NewLine + @"загрузка штрих-кодов.");
            }
            Shared.SaveToLogStartProgramInformation(@"Локальные база ШК " + Shared.Barcodes1C.GetCountBarcodes + "; посл.обн " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture)
                 + "; создан " + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (Program.deviceName.Contains("CPT"))
                btnCloseApp.Visible = true;
 //           var mFilter = new InactivityFilter(100);
            //mFilter.InactivityElapsed += m_filter_InactivityElapsed;
 //           Application2.AddMessageFilter(mFilter);
        }
/*
        private void m_filter_InactivityElapsed()
        {
            Cursor.Current = Cursors.Default;
        }
*/

        private void btnDocOrder_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        Shared.ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm (this, DocDirection.DocOut);
                        if (docOrders != null && !docOrders.IsDisposed)
                        {
                            DialogResult result = docOrders.ShowDialog();
                        }
                        //docOrders.Show();
                        //if (docOrders.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                Shared.ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

        private void WrongUserPass()
        {
            Shared.ShowMessageError(@"Неверно указан логин или пароль");
            Application.Exit();
        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
            if (Scanner != null)
                Scanner.Dispose();
        }

        private void btnExtAccept_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        Shared.ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm(this, DocDirection.DocIn);
                        if (docOrders != null && !docOrders.IsDisposed)
                        {
                            DialogResult result = docOrders.ShowDialog();
                        }
                        //docOrders.Show();
                        //if (docOrders.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                Shared.ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnDocMovement_Click(object sender, EventArgs e)
        {
            EndPointInfo endPointInfo;
            using (var form = new ChooseEndPointDialog(false))
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK) return;
                endPointInfo = form.EndPointInfo;
                if (!endPointInfo.IsAvailabilityPlaceZoneId)
                {
                    endPointInfo.IsSettedDefaultPlaceZoneId = false;
                    btnDocMovementClicked(endPointInfo);
                }
                else if ((endPointInfo.IsAvailabilityPlaceZoneId && endPointInfo.PlaceZoneId == null) || (endPointInfo.IsAvailabilityChildPlaceZoneId && endPointInfo.PlaceZoneId != null))
                {
                    string message = (endPointInfo.IsAvailabilityChildPlaceZoneId && endPointInfo.PlaceZoneId != null) ? "Вы не до конца указали зону. Попробуете еще раз?" : "Вы будете сейчас указывать зону хранения по умолчанию?";
                    ShowMessageQuestion(btnDocMovementClick, new QuestionResultEventHandlerParameter { endPointInfo = endPointInfo}, message);
                }
                else if (endPointInfo.PlaceZoneId != null)
                {
                    endPointInfo.IsSettedDefaultPlaceZoneId = true;
                    btnDocMovementClicked(endPointInfo);
                }                
            }
        }
        private void btnDocMovementClick(QuestionResultEventHandlerParameter param)
        {
            ReturnProcAfterQuestionResult -= btnDocMovementClick;
            EndPointInfo endPointInfo = param.endPointInfo;
            if (param.dialogResult == DialogResult.Yes)
            {
                using (var formPlaceZone = new ChooseEndPointDialog(endPointInfo.PlaceId))
                {
                    DialogResult resultPlaceZone = formPlaceZone.ShowDialog();
                    if (resultPlaceZone != DialogResult.OK)
                    {
                        //ShowMessageInformation(@"Не выбрана зона склада по умолчанию.");
                        endPointInfo.IsSettedDefaultPlaceZoneId = false;
                    }
                    else
                    {
                        endPointInfo = formPlaceZone.EndPointInfo;
                        endPointInfo.IsSettedDefaultPlaceZoneId = true;

                    }
                }               
            }
            btnDocMovementClicked(endPointInfo);
        }

        private void btnDocMovementClicked(EndPointInfo endPointInfo)
        {
            Cursor.Current = Cursors.WaitCursor;
            //if (ConnectionState.CheckConnection())
            //{
                //switch (Db.CheckSqlConnection())
                //{
                //    case 2:
                //        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                //                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                //                        MessageBoxDefaultButton.Button1);
                //        break;
                //    case 1:
                //        WrongUserPass();
                //        break;
                //    default:
                        Cursor.Current = Cursors.Default;
                        
                        var docMovementForm = new DocMovementForm(this, DocDirection.DocOutIn, endPointInfo);
                        if (docMovementForm.Text != "Ошибка при обновлении с сервера!")
                        {
                            docMovementForm.Text = "Перем-е на " + endPointInfo.PlaceName;
                            if (docMovementForm != null && !docMovementForm.IsDisposed)
                            {
                                DialogResult result = docMovementForm.ShowDialog();
                            }
                        }
                        ////docMovementForm.Show();
                        ////if (docMovementForm.Enabled)
                        ////    Hide();
                        //break;
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
            //                    @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
            //                    MessageBoxDefaultButton.Button1);
            //}
            Cursor.Current = Cursors.Default;
        }

        private void btnInventarisation_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var form = new DocInventarisationListForm(this, DocDirection.DocInventarisation);
                        if (form != null && !form.IsDisposed)
                        {
                            DialogResult result = form.ShowDialog();
                        }
                        //form.Show();
                        //if (form.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseShift_Click(object sender, EventArgs e)
        {
            if (Shared.ShiftId > 0)
            {
                ShowMessageQuestion(btnCloseShiftClick, null, @"Вы уверены, что хотите закрыть смену?");
            }
            else
            {
                var InfoProduct = new InfoProductForm(this);
                DialogResult result = InfoProduct.ShowDialog();
            }
        }

        private void btnCloseShiftClick(QuestionResultEventHandlerParameter param)
        {
            ReturnProcAfterQuestionResult -= btnCloseShiftClick;
            if (param.dialogResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                if (ConnectionState.CheckConnection())
                {
                    switch (Db.CheckSqlConnection())
                    {
                        case 2:
                            ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                            break;
                        case 1:
                            WrongUserPass();
                            break;
                        default:
                            Cursor.Current = Cursors.Default;
                            //if (Shared.ShowMessageQuestion(@"Вы уверены, что хотите закрыть смену?") == DialogResult.Yes)
                            {
                                using (var form = new ChooseShiftDialog())
                                {
                                    if (Shared.ShiftId == null || Shared.ShiftId == 0)
                                    {
                                        DialogResult result = form.ShowDialog();
                                        if (result != DialogResult.OK || form.ShiftId < 1)
                                        {
                                            ShowMessageError(@"Не указан номер смены." + Environment.NewLine + @"Смена не закрыта!");
                                            return;
                                        }
                                        Shared.ShiftId = form.ShiftId;
                                    }
                                    else
                                    {
                                        var resultMessage = Db.CloseShiftWarehouse(Shared.PersonId, Shared.ShiftId);
                                        if (Shared.LastQueryCompleted == false || resultMessage == null)
                                        {
                                            ShowMessageError(@"Смена не закрыта!" + Environment.NewLine + @"Произошла ошибка." + Environment.NewLine + @"Попробуйте снова.");
                                            return;
                                        }
                                        switch (resultMessage)
                                        {
                                            case 1:
                                                ShowMessageInformation(@"Смена закрыта." + Environment.NewLine + @"Распечатайте рапорт на компьютере в Гамме.");
                                                break;
                                            case -1:
                                                ShowMessageError(@"Смена не закрыта." + Environment.NewLine + @"Произошла ошибка." + Environment.NewLine + @"Попробуйте снова.");
                                                break;
                                            case 0:
                                                ShowMessageError(@"Смена не закрыта." + Environment.NewLine + @"Уже есть рапорт за эту смену." + Environment.NewLine + @"Закройте смену в Гамме повторно.");
                                                break;


                                        }

                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                }
                Cursor.Current = Cursors.Default;
            }            
        }

        private void btnDocTransfer_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm(this, DocDirection.DocOutIn);
                        if (docOrders != null && !docOrders.IsDisposed)
                        {
                            DialogResult result = docOrders.ShowDialog();
                        }
                        //docOrders.Show();
                        //if (docOrders.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

        private short _userInfoTextId { get; set; }
        private short userInfoTextId 
        {
            get
            {
                return _userInfoTextId;
            }
            set
            {
                _userInfoTextId = value;
                try
                {
                    switch (value)
                    {
                        case 0:
                            btnUserInfo.Text = "Логин: " + Settings.UserName + " (" + Shared.PersonName + ")";
                            break;
                        case 1:
                            btnUserInfo.Text = "Сервер: " + Settings.CurrentServer;
                            break;
                        case 2:
                            btnUserInfo.Text = "БД: " + Settings.Database;
                            break;
                        case 3:
                            btnUserInfo.Text = "Версия: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                            break;
                        case 4:
                            btnUserInfo.Text = "БД ШК создан:" + Db.GetLocalDbBarcodesDateCreated().ToString(System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case 5:
                            btnUserInfo.Text = "БД ШК обновлен: " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodesMoscowTimeZone.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case 6:
                            btnUserInfo.Text = "БД ШК кол-во: " + Shared.Barcodes1C.GetCountBarcodes;
                            break;
                    }
                }
                catch
                {
                    Shared.SaveToLogError(@"Error btnUserInfo");
                }
            }
        }

        private void btnUserInfo_Click(object sender, EventArgs e)
        {
            userInfoTextId = userInfoTextId == 6 ? (short)0 : (short)(userInfoTextId + 1);
        }

        private void btnCloseApp_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMovementForOrder_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm(this, DocDirection.DocOutIn, true);
                        if (docOrders != null && !docOrders.IsDisposed)
                        {
                            DialogResult result = docOrders.ShowDialog();
                        }
                        //docOrders.Show();
                        //if (docOrders.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnComplectPallet_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var palletsForm = new PalletsForm(this);
                        if (palletsForm != null && !palletsForm.IsDisposed)
                        {
                            BarcodeFunc = null;
                            DialogResult result = palletsForm.ShowDialog();
                        }
                        //docOrders.Show();
                        //if (docOrders.Enabled)
                        //    Hide();
                        break;
                }
            }
            else
            {
                ShowMessageError(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState());
            }
            Cursor.Current = Cursors.Default;
        }

    }
}