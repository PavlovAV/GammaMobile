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
                if (Scanner != null)
                {
                    Scanner.CurrentScanner.BarcodeReceived -= BarcodeFunc;
                }
                _barcodeFunc = value;
                if (value == null) return;
                if (DesignMode.IsTrue) return;
                if (Scanner != null) 
                {
                    Scanner.CurrentScanner.BarcodeReceived += BarcodeFunc;
                }
            }
        }

        protected ImageList ImgList = Shared.ImgList;

        public Form ParentForm { get; set; }

        protected virtual void FormLoad(object sender, EventArgs e)
        {
            //Подписка на событие восстановления связи
            //ConnectionState.OnConnectionRestored += ConnectionRestored;//UnloadOfflineProducts;
            //Подписка на событие потери связи
            //ConnectionState.OnConnectionLost += ConnectionLost;
            ConnectionState.OnConnectionStateChanged += ConnectionStateChanged;
            Shared.SaveToLogInformation("Open " + ((Form)sender).Name + " " + ((Form)sender).Text);
        }

        protected virtual void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (Scanner != null)
            {
                if (BarcodeFunc.Target.ToString().Contains(((Form)sender).Name))
                    Scanner.CurrentScanner.BarcodeReceived -= BarcodeFunc;
            }
            //ConnectionState.OnConnectionRestored -= ConnectionRestored;
            //ConnectionState.OnConnectionLost -= ConnectionLost;
            ConnectionState.OnConnectionStateChanged -= ConnectionStateChanged;
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

        protected void ConnectionStateChanged(bool isConnected)
        {
            if (this.IsDisposed) return;
            try
            {
                Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { isConnected ? ConnectState.ConnectionRestore : ConnectState.NoConnection });
                ConnectionState.SetCurrentShowConnectionState(isConnected);
            }
            catch (ObjectDisposedException ex)
            {
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.WriteLine("ObjectDisposedEXCEPTION ConnectionStateChanged:isConnected=" + isConnected + Environment.NewLine + ex.Message);
#endif
            }
        }

//        protected void ConnectionLost()
//        {
//            if (this.IsDisposed) return;
//            try
//            {
//                Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.NoConnection });
//            }
//            catch (ObjectDisposedException ex)
//            {
//#if OUTPUTDEBUGINFO
//                System.Diagnostics.Debug.WriteLine("ObjectDisposedEXCEPTION ConnectionLost:" + Environment.NewLine + ex.Message);
//#endif
//            }
//        }

//        protected void ConnectionRestored()
//        {
//            try
//            {
//            Invoke((ConnectStateChangeInvoker)(ShowConnection), new object[] { ConnectState.ConnectionRestore });
//            }
//            catch (ObjectDisposedException ex)
//            {
//#if OUTPUTDEBUGINFO
//                System.Diagnostics.Debug.WriteLine("ObjectDisposedEXCEPTION ConnectionRestored:" + Environment.NewLine + ex.Message);
//#endif
//            }
//        }

        protected virtual void ShowConnection(ConnectState conState)
        {
            if (imgConnection != null)
            {
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
            }
        }

        #endregion

    }
}