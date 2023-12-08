using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithBarcodeScan : BaseFormWithChooseNomenclatureCharacteristic
    {
        #region Panels

        public System.Windows.Forms.TextBox edtNumber { get; private set; }
        private System.Windows.Forms.Button btnAddProduct;
        protected System.Windows.Forms.PictureBox imgConnection;
        private System.Windows.Forms.Panel pnlSearch;

        public virtual void ActivatePanels()
        {
            ActivatePanelSearch();
        }

        public virtual void ActivatePanels(List<int> pnlToolBar_ActivButtons)
        {
            ActivatePanelSearch();
            base.ActivateToolBar(pnlToolBar_ActivButtons);
        }

        private void ActivatePanelSearch()
        {
            var pnlElementHeight = Shared.ToolBarHeight - 2;
            edtNumber = new System.Windows.Forms.TextBox()
            {
                Location = new System.Drawing.Point(0, 1),
                Name = "edtNumber",
                Size = new System.Drawing.Size(127, pnlElementHeight),
                TabIndex = 1
            };
            btnAddProduct = new System.Windows.Forms.Button()
            {
                Location = new System.Drawing.Point(133, 1),
                Name = "btnAddProduct",
                Size = new System.Drawing.Size(72, pnlElementHeight),
                TabIndex = 2,
                Text = "Добавить"
            };
            btnAddProduct.Click += new System.EventHandler(btnAddProduct_Click);
            imgConnection = new System.Windows.Forms.PictureBox()
            {
                Location = new System.Drawing.Point(211, 1),
                Name = "imgConnection",
                Size = new System.Drawing.Size(pnlElementHeight, pnlElementHeight)
            };
            pnlSearch = new System.Windows.Forms.Panel()
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Location = new System.Drawing.Point(0, Shared.ToolBarHeight),
                Name = "pnlSearch",
                Size = new System.Drawing.Size(638, Shared.ToolBarHeight)
            };
            pnlSearch.SuspendLayout();
            SuspendLayout();
            pnlSearch.Controls.Add(btnAddProduct);
            pnlSearch.Controls.Add(imgConnection);
            pnlSearch.Controls.Add(edtNumber);
            this.Controls.Add(pnlSearch);
            pnlSearch.ResumeLayout(false);
            ResumeLayout(false);
#if DEBUG
            if (edtNumber.Text.Length == 0)
                edtNumber.Text = "000008016032";
#endif
        }

        #endregion

        #region FormActions

        protected override void FormLoad(object sender, EventArgs e)
        {
            base.FormLoad(sender, e);
            BarcodeFunc = BarcodeReaction;
        }

        protected override void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            base.OnFormClosing(sender, e);
        }

        protected override void FormActivated(object sender, EventArgs e)
        {
            BarcodeFunc = BarcodeReaction;
            base.FormActivated(sender, e);
        }

        #endregion

        #region Barcode

        protected virtual void ActionByBarcode(string barcode) {}

        public void btnAddProduct_Click(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation(@"Выбрано Добавить ШК: " + edtNumber.Text);
            if (edtNumber.Text.Length < Shared.MinLengthProductBarcode)
            {
                Shared.ShowMessageError(@"Ошибка при сканировании ШК " + edtNumber.Text + Environment.NewLine + @"Штрих-код должен быть длиннее 12 символов");
            }
            else
                ActionByBarcode(edtNumber.Text);
        }

        protected void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            if (this.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate()
                {
                    this.btnAddProduct_Click(new object(), new EventArgs());
                });
            }
            else
                btnAddProduct_Click(new object(), new EventArgs());
        }

        #endregion

    }
}
