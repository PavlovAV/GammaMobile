using System;
using System.ComponentModel;
using System.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Dialogs;
using gamma_mob.Models;
using System.IO;
using System.Reflection;

namespace gamma_mob
{
    public partial class MainForm : BaseForm
    {
        public MainForm()
        {
            InitializeComponent();
            if (Shared.ShiftId > 0)
            {
                //btnCloseShift.Visible = true;
                btnCloseShift.Text = "Закрытие смены";
                btnInventarisation.Visible = false;
            }
            else
            {
                //btnCloseShift.Visible = true;
                btnCloseShift.Text = "Информация";
                btnInventarisation.Visible = true;
            }
            lblUserInfo.Text = "Логин: " + Settings.UserName + " (" + Shared.PersonName + ")";
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
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm (this, DocDirection.DocOut);
                        docOrders.Show();
                        if (docOrders.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1);
            }
            Cursor.Current = Cursors.Default;
        }

        private void WrongUserPass()
        {
            MessageBox.Show(@"Неверно указан логин или пароль", @"Ошибка связи с БД",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            Application.Exit();
        }

        protected override void OnFormClosing(object sender, CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
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
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocOrdersForm(this, DocDirection.DocIn);
                        docOrders.Show();
                        if (docOrders.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnDocMovement_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        Cursor.Current = Cursors.Default;
                        EndPointInfo endPointInfo;
                        using (var form = new ChooseEndPointDialog())
                        {
                            DialogResult result = form.ShowDialog();
                            if (result != DialogResult.OK) return;
                            endPointInfo = form.EndPointInfo;
                        }
                        Cursor.Current = Cursors.WaitCursor;
                        var docMovementForm = new DocMovementForm(this, endPointInfo.PlaceId);
                        docMovementForm.Text = "Перем-е на " + endPointInfo.PlaceName;
                        docMovementForm.Show();
                        if (docMovementForm.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1);
            }
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
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var form = new DocInventarisationListForm(this);
                        form.Show();
                        if (form.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1);
            }
            Cursor.Current = Cursors.Default;
        }


        private void btnCloseShift_Click(object sender, EventArgs e)
        {
            if (Shared.ShiftId > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                if (ConnectionState.CheckConnection())
                {
                    switch (Db.CheckSqlConnection())
                    {
                        case 2:
                            MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                            @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                            MessageBoxDefaultButton.Button1);
                            break;
                        case 1:
                            WrongUserPass();
                            break;
                        default:
                            Cursor.Current = Cursors.Default;
                            using (var form = new ChooseShiftDialog())
                            {
                                if (Shared.ShiftId == null || Shared.ShiftId == 0)
                                {
                                    DialogResult result = form.ShowDialog();
                                    if (result != DialogResult.OK || form.ShiftId < 1)
                                    {
                                        MessageBox.Show(@"Не указан номер смены." + Environment.NewLine + @"Смена не закрыта!", @"Смена не закрыта",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                                        return;
                                    }
                                    Shared.ShiftId = form.ShiftId;
                                }
                                else
                                {
                                    var resultMessage = Db.CloseShiftWarehouse(Shared.PersonId, Shared.ShiftId);
                                    switch (resultMessage)
                                    {
                                        case 1:
                                            MessageBox.Show(@"Смена закрыта." + Environment.NewLine + @"Распечатайте рапорт на компьютере в Гамме.", @"Закрытие смены", MessageBoxButtons.OK,
                                                        MessageBoxIcon.None,
                                                        MessageBoxDefaultButton.Button1);
                                            break;
                                        case -1:
                                            MessageBox.Show(@"Смена не закрыта." + Environment.NewLine + @"Произошла ошибка." + Environment.NewLine + @"Попробуйте снова.", @"Закрытие смены", MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk,
                                                        MessageBoxDefaultButton.Button1);
                                            break;
                                        case 0:
                                            MessageBox.Show(@"Смена не закрыта." + Environment.NewLine + @"Уже есть рапорт за эту смену." + Environment.NewLine + @"Закройте смену в Гамме повторно.", @"Закрытие смены", MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk,
                                                        MessageBoxDefaultButton.Button1);
                                            break;


                                    }

                                }
                            }
                            break;
                    }
                }
                else
                {
                    MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi" + Environment.NewLine + ConnectionState.GetConnectionState(),
                                    @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                    MessageBoxDefaultButton.Button1);
                }
                Cursor.Current = Cursors.Default;
            }
            else
            {
                var InfoProduct = new InfoProductForm(this);
                DialogResult result = InfoProduct.ShowDialog();
            }
        }

        
    }
}