using System;
using System.Windows.Forms;


namespace gamma_mob
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnDocOrder_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!ConnectionState.CheckConnection())
            {
                MessageBox.Show(@"Нет связи с БД. Повторите попытку в зоне покрытия WiFi",
                    @"Отсутствует WiFi", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
            }
            else
            {
                
                var docOrder = new DocOrder {ParentForm = this};
                docOrder.Show();
                Hide();
            }
            Cursor.Current = Cursors.Default;
        }    
    }
}