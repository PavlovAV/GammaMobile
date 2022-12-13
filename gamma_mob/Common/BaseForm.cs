﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;

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
            Scanner = BarcodeScanner.Scanner;
        }

        protected static BarcodeScanner Scanner { get; set; }

        private static BarcodeReceivedEventHandler _barcodeFunc;

        protected static BarcodeReceivedEventHandler BarcodeFunc
        {
            private get { return _barcodeFunc; }
            set
            {
                if (value == BarcodeFunc)
                    return;
                Scanner.BarcodeReceived -= BarcodeFunc;
                _barcodeFunc = value;
                if (value == null) return;
                if (DesignMode.IsTrue) return;
                Scanner.BarcodeReceived += BarcodeFunc;
            }
        }

        protected ImageList ImgList = Shared.ImgList;

        public Form ParentForm { get; set; }

        protected virtual void FormLoad(object sender, EventArgs e)
        {
            Shared.SaveToLog("Open " + ((Form)sender).Name + " " + ((Form)sender).Text);
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

        protected virtual void FormActivated(object sender, EventArgs e)
        {
            Shared.SaveToLog("Activate " + ((Form)sender).Name + " " + ((Form)sender).Text);
        }
    }
}