using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace gamma_mob.Common
{
    [DesignerCategory("Form")]
    [DesignTimeVisible(false)]
    public partial class BaseForm : Form
    {
        protected BaseForm()
        {
            InitializeComponent();
            if (DesignMode.IsTrue) return;
#if !DISABLESCANER
            Scanner = BarcodeScanner.Scanner;
#endif
        }

        protected static BarcodeScanner Scanner { get; set; }

        private static SortedList<int, string> ListBarcodeFunc { get; set; }

        private static BarcodeReceivedEventHandler _barcodeFunc;

        protected static BarcodeReceivedEventHandler BarcodeFunc
        {
            get { return _barcodeFunc; }
            set
            {
                //if ((value != null && Scanner.BarcodeReceived != null && value == BarcodeFunc)
                //    || (value == null && Scanner.BarcodeReceived == null && BarcodeFunc == null))
                //    return;
                if (Scanner != null)
                {
                    Scanner.CurrentScanner.BarcodeReceived -= BarcodeFunc;
                    //if (ListBarcodeFunc != null && ListBarcodeFunc.Count > 0 && ListBarcodeFunc[(ListBarcodeFunc.Count - 1)] == (BarcodeFunc.Target.ToString() + "/" + BarcodeFunc.Method.ToString()))
                    //    ListBarcodeFunc.Remove(ListBarcodeFunc.Count-1);
                }
                _barcodeFunc = value;
                if (value == null) return;
                if (DesignMode.IsTrue) return;
                if (Scanner != null) 
                {
                    Scanner.CurrentScanner.BarcodeReceived += BarcodeFunc;
                    //if (ListBarcodeFunc == null)
                    //    ListBarcodeFunc = new SortedList<int, string>();
                    //if (ListBarcodeFunc != null && (ListBarcodeFunc.Count == 0 || (ListBarcodeFunc.Count > 0 && ListBarcodeFunc[(ListBarcodeFunc.Count - 1)] == (BarcodeFunc.Target.ToString() + "/" + BarcodeFunc.Method.ToString()))))
                    //    ListBarcodeFunc.Add(ListBarcodeFunc.Count, BarcodeFunc.Target.ToString() + "/" + BarcodeFunc.Method.ToString());
                }
            }
        }

        //protected void SetBarcodeFunc(BarcodeReceivedEventHandler barcodeFunc)
        //{
        //    BarcodeFunc = barcodeFunc;
        //}

        protected ImageList ImgList = Shared.ImgList;

        public Form ParentForm { get; set; }

        protected virtual void FormLoad(object sender, EventArgs e)
        {
            //Подписка на событие восстановления связи
            ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            //Подписка на событие потери связи
            ConnectionState.OnConnectionLost += ConnectionLost;
            Shared.SaveToLogInformation("Open " + ((Form)sender).Name + " " + ((Form)sender).Text);
        }

        protected virtual void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (Scanner != null)
            {
                if (BarcodeFunc.Target.ToString().Contains(((Form)sender).Name))
                    Scanner.CurrentScanner.BarcodeReceived -= BarcodeFunc;
                //if (ListBarcodeFunc != null && ListBarcodeFunc.Count > 0 && ListBarcodeFunc[(ListBarcodeFunc.Count - 1)] == (BarcodeFunc.Target.ToString() + "/" + BarcodeFunc.Method.ToString()))
                //    ListBarcodeFunc.Remove(ListBarcodeFunc.Count - 1);
            }
            ConnectionState.OnConnectionRestored -= ConnectionRestored;
            ConnectionState.OnConnectionLost -= ConnectionLost;
            if (ParentForm != null)
            {
                ParentForm.Show();
                ParentForm.Activate();
            }
        }

        protected virtual void FormActivated(object sender, EventArgs e)
        {
            Shared.SaveToLogInformation("Activate " + ((Form)sender).Name + " " + ((Form)sender).Text);
        }

        #region Connection

        protected void ConnectionLost()
        {
            if (this.IsDisposed) return;
            try
            {
                Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
            }
            catch (ObjectDisposedException ex)
            { }
        }

        protected void ConnectionRestored()
        {
            try
            {
            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore });
            }
            catch (ObjectDisposedException ex)
            { }
        }

        protected virtual void ShowConnection(ConnectState conState)
        {
            if (imgConnection != null)
                switch (conState)
                {
                    case ConnectState.ConInProgress:
                        imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                        break;
                    case ConnectState.NoConInProgress:
                        imgConnection.Image = null;
                        break;
                    case ConnectState.NoConnection:
                        imgConnection.Image = ImgList.Images[(int)Images.NetworkOffline];
                        break;
                    case ConnectState.ConnectionRestore:
                        imgConnection.Image = ImgList.Images[(int)Images.NetworkTransmitReceive];
                        break;
                }
            //if (PnlToolBar != null)
            //{
            //    foreach (var control in PnlToolBar.Controls)
            //    {
            //        if (control is OpenNETCF.Windows.Forms.Button2)
            //        {
            //            var backButton = (control as OpenNETCF.Windows.Forms.Button2);
            //            if (backButton.ImageIndex == (int)Images.Back || backButton.ImageIndex == (int)Images.BackOffline)
            //            {
            //                switch (conState)
            //                {
            //                    case ConnectState.ConInProgress:
            //                        if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
            //                        break;
            //                    case ConnectState.NoConInProgress:
            //                        //imgConnection.Image = null;
            //                        break;
            //                    case ConnectState.NoConnection:
            //                        if (backButton.ImageIndex == (int)Images.Back) backButton.ImageIndex = (int)Images.BackOffline;
            //                        break;
            //                    case ConnectState.ConnectionRestore:
            //                        if (backButton.ImageIndex == (int)Images.BackOffline) backButton.ImageIndex = (int)Images.Back;
            //                        break;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        #endregion

    }
}