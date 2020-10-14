using System;
using System.Threading;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using gamma_mob.Models;
using System.Drawing;
using System.Globalization;

namespace gamma_mob
{
    public partial class InfoProductForm : BaseForm
    {
        private InfoProductForm()
        {
            InitializeComponent();
            lblUserInfo.Text = "Логин: " + Settings.UserName + " (" + Shared.PersonName +")";
            textBox1.Text = "Последнее обновление данных: " + Shared.Barcodes1C.GetLastUpdatedTimeBarcodes.AddHours(3).ToString(CultureInfo.InvariantCulture)
                + Environment.NewLine + Shared.Barcodes1C.GetCountBarcodes
                + Environment.NewLine + Environment.NewLine + Environment.NewLine 
                + "Просканируйте \r\nштрих-код\r\nпродукции";
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
                    (() => textBox1.Font = new Font("Tahoma", 14F, System.Drawing.FontStyle.Regular)));
                Invoke(
                    (MethodInvoker)
                    (() => textBox1.ForeColor = Color.DarkRed));
                Invoke(
                    (MethodInvoker)
                    (() => textBox1.Text = "Ошибка при получении данных!"+ Environment.NewLine +"Попробуйте ещё раз."));
            }
            else
            {
                Invoke(
                    (MethodInvoker)
                    (() => textBox1.Font = new Font("Tahoma", 11F, System.Drawing.FontStyle.Regular)));
                Invoke(
                    (MethodInvoker)
                    (() => textBox1.ForeColor = Color.Black)); 
                Invoke(
                    (MethodInvoker)
                    (() => textBox1.Text = "Продукция: " + Environment.NewLine + infoproduct));
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