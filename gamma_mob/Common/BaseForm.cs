using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    [DesignerCategory("Form")]
    [DesignTimeVisible(false)]
    public partial class BaseForm : Form
    {
        private BarcodeReceivedEventHandler _barcodeFunc;

        protected BaseForm()
        {
            InitializeComponent();
            if (DesignMode.IsTrue) return;
            Scanner = BarcodeScanner.Scanner;
        }

        protected BarcodeScanner Scanner { get; set; }


        protected BarcodeReceivedEventHandler BarcodeFunc
        {
            private get { return _barcodeFunc; }
            set
            {
                Scanner.BarcodeReceived -= BarcodeFunc;
                _barcodeFunc = value;
                if (value == null) return;
                if (DesignMode.IsTrue) return;
                Scanner.BarcodeReceived += BarcodeFunc;
            }
        }

        protected ImageList ImgList { get; set; }

        public Form ParentForm { get; set; }

        protected virtual void FormLoad(object sender, EventArgs e)
        {
            ImgList = Shared.ImgList;
        }

        protected virtual void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (Scanner != null)
                Scanner.BarcodeReceived -= BarcodeFunc;
            if (ParentForm != null)
            {
                ParentForm.Show();
                ParentForm.Activate();
            }
                
        }
    }
}