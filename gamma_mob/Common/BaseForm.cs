using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gamma_mob.Common
{
    [DesignerCategory("Form")]
    [DesignTimeVisible(false)]
    public partial class BaseForm : Form
    {
        public BaseForm()
        {
            InitializeComponent();
            Scanner = BarcodeScanner.Scanner;
        }

        protected BarcodeScanner Scanner { get; set; }
        

        private BarcodeReceivedEventHandler _barcodeFunc;
        protected BarcodeReceivedEventHandler BarcodeFunc
        {
            private get { return _barcodeFunc; }
            set 
            { 
                _barcodeFunc = value;
                Scanner.BarcodeReceived += BarcodeFunc;
            }
        }

        protected virtual void FormLoad(object sender, EventArgs e)
        {
        }

        public Form ParentForm { get; set; }

        protected virtual void FormClosing(object sender, CancelEventArgs e)
        {
            if (Scanner != null)
                Scanner.BarcodeReceived -= BarcodeFunc;
            if (ParentForm != null)
                ParentForm.Show();
        }

        
    }
}