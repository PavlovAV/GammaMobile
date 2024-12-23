using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Windows.Forms;
using System.Windows.Forms;
using gamma_mob.Dialogs;

namespace gamma_mob.Common
{
    public abstract class BaseFormWithToolBar : BaseFormWithShowMessage
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
                case (int)Images.BackOffline:
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
                case (int)Images.Add:
                    AddToolBarButton();
                    break;
                case (int)Images.Edit:
                    EditToolBarButton();
                    break;
                case (int)Images.Remove:
                    RemoveToolBarButton();
                    break;
                case (int)Images.RDP:
                    RDPToolBarButton();
                    break;
                case (int)Images.ShortcutStartPointsPanelEnabled:
                    ShortcutStartPointsPanelEnabled();
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

        protected virtual void AddToolBarButton() { }
        
        protected virtual void EditToolBarButton() { }
        
        protected virtual void RemoveToolBarButton() { }

        protected virtual void RDPToolBarButton()
        {
            var res = Shared.ExecRDP();
            if (res == null)
                Shared.ShowMessageError(@"RDP не запущен. Файл cerdisp.exe не найден.");
            else if ((bool)res)
                Shared.ShowMessageInformation("RDP запущен");
            else
                Shared.ShowMessageInformation("RDP остановлен");
        }

        protected virtual void ShortcutStartPointsPanelEnabled() 
        {
            if (!Shared.VisibleShortcutStartPoints)
            {
                Shared.VisibleShortcutStartPoints = true;
                Shared.ShowMessageInformation("Включена видимость панели с кнопками складов откуда." + Environment.NewLine + "Нажмите Назад и откройте список повторно." + Environment.NewLine + "Затем, при необходимости, добавьте кнопку склада.");
            }
        }

        protected override void ShowConnection(ConnectState conState)
        {
            if (PnlToolBar != null)
            {
                foreach (var control in PnlToolBar.Controls)
                {
                    if (control is OpenNETCF.Windows.Forms.Button2)
                    {
                        var backButton = (control as OpenNETCF.Windows.Forms.Button2);
                        if (backButton.ImageIndex == (int)Images.Back || backButton.ImageIndex == (int)Images.BackOffline)
                        {
                            switch (conState)
                            {
                                case ConnectState.ConInProgress:
                                    if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
                                    break;
                                case ConnectState.NoConInProgress:
                                    //imgConnection.Image = null;
                                    break;
                                case ConnectState.NoConnection:
                                    if (backButton.ImageIndex == (int)Images.Back) backButton.ImageIndex = (int)Images.BackOffline;
                                    break;
                                case ConnectState.ConnectionRestore:
                                    if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
}
