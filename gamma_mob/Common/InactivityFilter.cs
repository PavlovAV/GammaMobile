using System;
using System.Windows.Forms;
using OpenNETCF.Win32;
using IMessageFilter = OpenNETCF.Windows.Forms.IMessageFilter;
using Message = Microsoft.WindowsCE.Forms.Message;
using MethodInvoker = OpenNETCF.Windows.Forms.MethodInvoker;

namespace gamma_mob.Common
{
    /// <summary>
    ///     Фиговина, вызывающая функцию при бездействии(в приложении используется для возврата курсора в default)
    /// </summary>
    public class InactivityFilter : IMessageFilter
    {
        private readonly Timer m_inactivityTimer;

        public InactivityFilter(int timeoutMilliseconds)
        {
            m_inactivityTimer = new Timer();
            m_inactivityTimer.Interval = timeoutMilliseconds;

            m_inactivityTimer.Tick += m_inactivityTimer_Tick;
            Reset();
        }

/*
    public int Timeout { get; set; }
*/

        public bool Elapsed { get; private set; }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((WM) m.Msg)
            {
                case WM.KEYUP:
                case WM.LBUTTONUP:
                case WM.MOUSEMOVE:
                    // reset the timer
                    m_inactivityTimer.Enabled = false;
                    m_inactivityTimer.Enabled = true;
                    break;
            }
            return false;
        }

        public event MethodInvoker InactivityElapsed;

        private void m_inactivityTimer_Tick(object sender, EventArgs e)
        {
            m_inactivityTimer.Enabled = false;
            Elapsed = true;

            if (InactivityElapsed != null) InactivityElapsed();
        }

        public void Reset()
        {
            Elapsed = false;
            m_inactivityTimer.Enabled = true;
        }
    }
}