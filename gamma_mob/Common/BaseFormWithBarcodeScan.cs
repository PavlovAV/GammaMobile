using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Windows.Forms;
using gamma_mob.Models;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithBarcodeScan : BaseFormWithChooseNomenclatureCharacteristic
    {
        #region Panels

        public System.Windows.Forms.TextBox edtNumber { get; private set; }
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.Panel pnlSearchProduct;
        private System.Windows.Forms.Panel pnlSearch;
        private System.Windows.Forms.Button btnStartPoint;
        private System.Windows.Forms.Panel pnlStartPoint;

        public virtual void ActivatePanels()
        {
            ActivatePanelSearch();
            //if (!ConnectionState.IsConnected)
            //    ConnectionState.ConnectionLost(); 
            ConnectionStateChanged(ConnectionState.IsConnected);
        }

        public virtual void ActivatePanels(List<int> pnlToolBar_ActivButtons)
        {
            ActivatePanelSearch();
            base.ActivateToolBar(pnlToolBar_ActivButtons);
            //if (!ConnectionState.IsConnected)
            //    ConnectionState.ConnectionLost(); 
            ConnectionStateChanged(ConnectionState.IsConnected);
        }

        public virtual void ActivatePanelsWithoutSearch(List<int> pnlToolBar_ActivButtons)
        {
            base.ActivateToolBar(pnlToolBar_ActivButtons);
            //if (!ConnectionState.IsConnected)
            //    ConnectionState.ConnectionLost();
            ConnectionStateChanged(ConnectionState.IsConnected);
        }

        protected void ActivatePanelSearch()
        {
            var pnlElementHeight = Shared.ToolBarHeight - 2;
            pnlSearch = new System.Windows.Forms.Panel()
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Location = new System.Drawing.Point(0, Shared.ToolBarHeight),
                Name = "pnlSearch",
                Size = new System.Drawing.Size(638, Shared.ToolBarHeight)
            };
            pnlSearch.SuspendLayout();
            SuspendLayout();
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
                    Size = new System.Drawing.Size(pnlElementHeight, pnlElementHeight),
                    SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
                };
                pnlSearchProduct = new System.Windows.Forms.Panel()
                {
                    Dock = System.Windows.Forms.DockStyle.Top,
                    Location = new System.Drawing.Point(0, Shared.ToolBarHeight),
                    Name = "pnlSearch",
                    Size = new System.Drawing.Size(638, Shared.ToolBarHeight)
                };
            var shortcutStartPoints = new ShortcutStartPoints(BarcodeReaction);//new System.EventHandler(btnStartPoint_Click), new System.EventHandler(btnStartPoint0_Click));
            if (!Shared.VisibleShortcutStartPoints) // Shared.ShortcutStartPoints.Count == 0)
            {
                
                pnlSearchProduct.Controls.Add(btnAddProduct);
                pnlSearchProduct.Controls.Add(imgConnection);
                pnlSearchProduct.Controls.Add(edtNumber);
                pnlSearch.Controls.Add(pnlSearchProduct);
            }
            else
            {
                pnlSearch.Controls.Add(shortcutStartPoints.pnlStartPoint);
            }
            this.Controls.Add(pnlSearch);
            pnlSearch.ResumeLayout(false);

            ResumeLayout(false);
#if DEBUG && !ASRELEASE
            if (edtNumber.Text.Length == 0)
                edtNumber.Text = "20804777671007243010000";
                //edtNumber.Text = "240701410801002";
                //edtNumber.Text = "20300000000692";
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

        public void AddProductClick()
        {
            if (edtNumber.Text.Length < Shared.MinLengthProductBarcode)
            {
                Shared.ShowMessageError(@"Ошибка при сканировании ШК " + edtNumber.Text + Environment.NewLine + @"Штрих-код должен быть длиннее 12 символов");
            }
            else
            {
                var edtNumberText = edtNumber.Text;
                Invoke((MethodInvoker)(() => edtNumber.Text = String.Empty));
                ActionByBarcode(edtNumberText);
            }
        }

        protected virtual void btnAddProductClick()
        {
            AddProductClick();
        }

        public void btnAddProduct_Click(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation(@"Выбрано Добавить ШК: " + edtNumber.Text);
            btnAddProductClick();
        }

        protected void BarcodeReaction(string barcode)
        {
            Invoke((MethodInvoker)(() => edtNumber.Text = barcode));
            if (this.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate()
                {
                    AddProductClick();
                    //this.btnAddProduct_Click(new object(), new EventArgs());
                });
            }
            else
                AddProductClick();
                //btnAddProduct_Click(new object(), new EventArgs());
        }

        #endregion

    }
}
