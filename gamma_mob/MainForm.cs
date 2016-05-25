using System;
using System.Windows.Forms;



namespace gamma_mob
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            if (!ConnectionState.CheckConnection()) return;
            if (GammaDataSet.CheckSQLConnection() != 1) return;
            WrongUserPass();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnDocOrder_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConnectionState.CheckConnection())
            {
                switch (GammaDataSet.CheckSQLConnection())
                {
                    case 2:
                        MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                        @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                        break;
                    case 1:
                        WrongUserPass();
                        break;
                    default:
                        var docOrder = new DocOrder { ParentForm = this };
                        docOrder.Show();
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
    }
}