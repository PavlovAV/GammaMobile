using System;
using System.Windows.Forms;
using gamma_mob.Common;
using OpenNETCF.Windows.Forms;

namespace gamma_mob
{
    public partial class LoginForm : BaseForm
    {
        public LoginForm()
        {
            InitializeComponent();
            if (!ConnectionState.CheckConnection()) return;
            if (Db.CheckSqlConnection() != 1) return;
            WrongUserPass();
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = AuthorizeByBarcode;
        }

        private void AuthorizeByBarcode(string barcode)
        {
            if (Db.CheckSqlConnection() == 1)
            {
                MessageBox.Show(@"Нет связи с базой");
                return;
            }
            var person = Db.PersonByBarcode(barcode);
            if (person == null)
            {
                MessageBox.Show(@"Неверный шк или нет связи с базой");                
                return;
            }
            Shared.PersonId = person.PersonID;
            Invoke((MethodInvoker)(CloseForm));
        }

        private void CloseForm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void WrongUserPass()
        {
            MessageBox.Show(@"Неверно указан логин или пароль в настройках. Обратитесь к администратору приложения"
                                , @"Ошибка связи с БД",
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            Application.Exit();
        }
    }
}