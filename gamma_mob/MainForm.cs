using System;
using System.Windows.Forms;
using gamma_mob.Common;



namespace gamma_mob
{
    public partial class MainForm : BaseForm
    {
        public MainForm()
        {
            InitializeComponent();
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
                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrders = new DocShipmentOrdersForm() { ParentForm = this };
                        docOrders.Show();
                        Hide();
                        break;
                }
            }
            else
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
            Cursor.Current = Cursors.Default;
        }
        private void WrongUserPass()
        {
            MessageBox.Show(@"Неверно указан логин или пароль", @"Ошибка связи с БД",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            Application.Exit();
        }

        protected override void FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            base.FormClosing(sender, e);
            Scanner.Dispose();
        }
   }
}