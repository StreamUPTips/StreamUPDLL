using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StreamUP
{
    /// <summary>
    /// A borderless form that supports resizing by intercepting WM_NCHITTEST
    /// </summary>
    public class BorderlessForm : Form
    {
        private const int RESIZE_BORDER = 6;

        // WM_NCHITTEST return values
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                // Get cursor position relative to form BEFORE calling base
                int lParam = m.LParam.ToInt32();
                int x = lParam & 0xFFFF;
                int y = (lParam >> 16) & 0xFFFF;

                // Handle negative coordinates (for multi-monitor setups)
                if (x > 32767) x -= 65536;
                if (y > 32767) y -= 65536;

                Point screenPoint = new Point(x, y);
                Point clientPoint = PointToClient(screenPoint);

                // Check if we're in a resize area
                int hitTest = GetResizeHitTest(clientPoint);
                if (hitTest != HTCLIENT)
                {
                    m.Result = new IntPtr(hitTest);
                    return;
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Determine which resize area the cursor is in
        /// </summary>
        private int GetResizeHitTest(Point clientPoint)
        {
            // Don't resize when maximized
            if (WindowState == FormWindowState.Maximized)
            {
                return HTCLIENT;
            }

            bool left = clientPoint.X >= 0 && clientPoint.X < RESIZE_BORDER;
            bool right = clientPoint.X >= Width - RESIZE_BORDER && clientPoint.X < Width;
            bool top = clientPoint.Y >= 0 && clientPoint.Y < RESIZE_BORDER;
            bool bottom = clientPoint.Y >= Height - RESIZE_BORDER && clientPoint.Y < Height;

            // Corner checks first (they take priority)
            if (top && left) return HTTOPLEFT;
            if (top && right) return HTTOPRIGHT;
            if (bottom && left) return HTBOTTOMLEFT;
            if (bottom && right) return HTBOTTOMRIGHT;

            // Edge checks
            if (left) return HTLEFT;
            if (right) return HTRIGHT;
            if (top) return HTTOP;
            if (bottom) return HTBOTTOM;

            return HTCLIENT;
        }

    }
}
