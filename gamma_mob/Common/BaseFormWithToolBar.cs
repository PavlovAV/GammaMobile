using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Windows.Forms;
using System.Windows.Forms;
using gamma_mob.Dialogs;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithToolBar : BaseForm
    {
        private Common.PanelToolBar _pnlToolBar { get; set; }
        public Common.PanelToolBar PnlToolBar
        {
            get
            {
                if (_pnlToolBar == null)
                {
                    _pnlToolBar = new PanelToolBar();
                }
                return _pnlToolBar;
            }
        }

        protected void ActivateToolBar(List<int> activButtons)
        {
            this.ActivateToolBar(activButtons, PnlToolBar_ButtonClick);
        }

        protected void ActivateToolBar(List<int> activButtons, EventHandler pnlToolBar_ButtonClick)
        {
            PnlToolBar.SuspendLayout();
            PnlToolBar.ActivateToolBar(activButtons, pnlToolBar_ButtonClick);
            Controls.Add(PnlToolBar);
            PnlToolBar.ResumeLayout(false);
        }

        protected void PnlToolBar_ButtonClick(object sender, EventArgs e)
        {
            switch (((OpenNETCF.Windows.Forms.Button2)sender).ImageIndex)
            {
                case (int)Images.Back:
                    CloseToolBarButton();
                    break;
                case (int)Images.Inspect:
                    OpenDetails();
                    break;
                case (int)Images.Refresh:
                    RefreshToolBarButton();
                    break;
                case (int)Images.UploadToDb:
                    UnloadOfflineProducts();
                    break;
                case (int)Images.Question:
                    QuestionToolBarButton();
                    break;
                case (int)Images.Pallet:
                    PalletToolBarButton();
                    break;
                case (int)Images.InfoProduct:
                    InfoProduct();
                    break;
                case (int)Images.DocPlus:
                    NewToolBarButton();
                    break;
                case (int)Images.Edit:
                    EditToolBarButton();
                    break;
                case (int)Images.Remove:
                    RemoveToolBarButton();
                    break;
            }
        }

        protected virtual void CloseToolBarButton() 
        {
            Close();
        }

        protected virtual void OpenDetails() { }

        protected virtual void RefreshToolBarButton() { }

        protected virtual void UnloadOfflineProducts() { }

        protected virtual void QuestionToolBarButton() { }

        protected virtual void PalletToolBarButton() { }

        protected virtual void InfoProduct()
        {
            var InfoProduct = new InfoProductForm(this);
            BarcodeFunc = null;
            DialogResult result = InfoProduct.ShowDialog();
            Invoke((MethodInvoker)Activate);
            //BarcodeFunc = BarcodeReaction;
        }

        protected virtual void NewToolBarButton() { }

        protected virtual void EditToolBarButton() { }
        
        protected virtual void RemoveToolBarButton() { }
    }
}
