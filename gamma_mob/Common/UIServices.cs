﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;

namespace gamma_mob.Common
{
    public static class UIServices
    {
        public static void SetBusyState(Form form)
        {
            try
            {
            if (!form.IsDisposed) 
                form.Invoke((MethodInvoker)(() => Cursor.Current = Cursors.WaitCursor));
            }
            catch (ObjectDisposedException ex)
            {
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.WriteLine("ObjectDisposedEXCEPTION SetBusyState:" + Environment.NewLine + ex.Message);
#endif
            }
        }

        public static void SetNormalState(Form form)
        {
            try
            {
            if (!form.IsDisposed)
                form.Invoke((MethodInvoker)(() => Cursor.Current = Cursors.Default));
            }
            catch (ObjectDisposedException ex)
            {
#if OUTPUTDEBUGINFO
                System.Diagnostics.Debug.WriteLine("ObjectDisposedEXCEPTION SetNormalState:" + Environment.NewLine + ex.Message);
#endif
            }
        }
    }
}
