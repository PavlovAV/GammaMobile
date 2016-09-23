using System;
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
            form.Invoke((MethodInvoker)(() => Cursor.Current = Cursors.WaitCursor));
        }

        public static void SetNormalState(Form form)
        {
            form.Invoke((MethodInvoker) (() => Cursor.Current = Cursors.Default));
        }
    }
}
