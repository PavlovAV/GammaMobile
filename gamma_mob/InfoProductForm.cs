using System;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Drawing;

namespace gamma_mob
{
    public partial class InfoProductForm : BaseForm
    {
        private InfoProductForm()
        {
            InitializeComponent();
        }

        public InfoProductForm(Form parentForm):this()
        {
            ParentForm = parentForm;
        }

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = InfoProductByBarcode;
        }

        private void InfoProductByBarcode(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            if (Db.CheckSqlConnection() == 1)
            {
                MessageBox.Show(@"Нет связи с базой" + Environment.NewLine + ConnectionState.GetConnectionState());
                return;
            }
            string infoproduct = Db.InfoProductByBarcode(barcode);
            if (infoproduct == null)
            {
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.Font = new Font("Tahoma", 14F, System.Drawing.FontStyle.Regular)));
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.ForeColor = Color.DarkRed));
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.Text = "Неверный штрих-код"));
            }
            else
            {
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.Font = new Font("Tahoma", 11F, System.Drawing.FontStyle.Regular)));
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.ForeColor = Color.Black)); 
                Invoke(
                    (MethodInvoker)
                    (() => lblMessage.Text = "Продукция: " + Environment.NewLine + infoproduct));
            }
        }

        private void CloseForm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnInfoProduct_Click(object sender, EventArgs e)
        {
            InfoProductByBarcode(edtNumber.Text);
        }

        
    }
}