using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;

namespace gamma_mob
{
    public partial class MainForm : BaseForm
    {
        public MainForm()
        {
            InitializeComponent();
            var mFilter = new InactivityFilter(100);
            //mFilter.InactivityElapsed += m_filter_InactivityElapsed;
            Application2.AddMessageFilter(mFilter);
        }

        private void m_filter_InactivityElapsed()
        {
            Cursor.Current = Cursors.Default;
        }


        private void btnDocOrder_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocShipmentOrdersForm {ParentForm = this};
                        docOrders.Show();
                        if (docOrders.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
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

        private void btnDocAccept_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (Db.CheckSqlConnection())
                {
                    case 2:
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                        MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docAcceptForm = new DocAcceptForm(this);
                        docAcceptForm.Show();
                        if (docAcceptForm.Enabled)
                            Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                                @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand,
                                MessageBoxDefaultButton.Button1);
            }
            Cursor.Current = Cursors.Default;
        }
    }
}