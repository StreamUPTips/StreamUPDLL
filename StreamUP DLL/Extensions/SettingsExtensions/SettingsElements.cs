using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class RoundedButton : Button
    {
        // Corner radius property
        public int CornerRadius { get; set; } = 20;

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw the button background with rounded corners
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(new Rectangle(0, 0, CornerRadius, CornerRadius), 180, 90);
                path.AddArc(new Rectangle(this.Width - CornerRadius, 0, CornerRadius, CornerRadius), 270, 90);
                path.AddArc(new Rectangle(this.Width - CornerRadius, this.Height - CornerRadius, CornerRadius, CornerRadius), 0, 90);
                path.AddArc(new Rectangle(0, this.Height - CornerRadius, CornerRadius, CornerRadius), 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw the button text
            TextRenderer.DrawText(g, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class RoundedCheckBox : CheckBox
    {
        // Corner radius property
        public int CornerRadius { get; set; } = 3;
        public Color BoolTrueColor { get; set; } = Color.SeaGreen;
        public Color BoolFalseColor { get; set; } = Color.IndianRed;


        public RoundedCheckBox()
        {
            this.Appearance = Appearance.Button;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.LightGray; // Default unchecked background color
            this.ForeColor = Color.Black;
            this.Cursor = Cursors.Hand;
            this.TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color backColor = this.Checked ? BoolTrueColor : BoolFalseColor; // Change colors as needed

            // Draw the button background with rounded corners
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(new Rectangle(0, 0, CornerRadius, CornerRadius), 180, 90);
                path.AddArc(new Rectangle(this.Width - CornerRadius, 0, CornerRadius, CornerRadius), 270, 90);
                path.AddArc(new Rectangle(this.Width - CornerRadius, this.Height - CornerRadius, CornerRadius, CornerRadius), 0, 90);
                path.AddArc(new Rectangle(0, this.Height - CornerRadius, CornerRadius, CornerRadius), 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw the button text
            TextRenderer.DrawText(g, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class CustomNumericUpDown : NumericUpDown
    {
        private const int EM_SETMARGINS = 0xD3;
        private const int EC_LEFTMARGIN = 0x0001;
        private const int EC_RIGHTMARGIN = 0x0002;

        private int margin = 5;
        private int cornerRadius = 3;
        private TextBox textBox;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeTextBox();
            SetTextBoxMargins();
            SetRegion();
        }

        private void InitializeTextBox()
        {
            if (textBox == null)
            {
                textBox = (TextBox)typeof(NumericUpDown)
                    .GetField("upDownEdit", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(this);

                if (textBox != null)
                {
                    // Subscribe to handle created event to ensure margin setting
                    textBox.HandleCreated += TextBox_HandleCreated;
                }
            }
        }

        private void TextBox_HandleCreated(object sender, EventArgs e)
        {
            SetTextBoxMargins();
        }

        private void SetTextBoxMargins()
        {
            if (textBox != null && textBox.IsHandleCreated)
            {
                SendMessage(textBox.Handle, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, (IntPtr)((margin << 16) | margin));
            }
        }

        private void SetRegion()
        {
            IntPtr hRgn = CreateRoundRectRgn(0, 0, Width, Height, cornerRadius, cornerRadius);
            SetWindowRgn(this.Handle, hRgn, true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRegion();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // Properties to control the appearance
        public int MarginSize
        {
            get { return margin; }
            set { margin = value; SetTextBoxMargins(); }
        }

        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; SetRegion(); }
        }
    }
    
    public class CustomTextBox : TextBox
    {
        private const int EM_SETMARGINS = 0xD3;


        private int margin = 5; // Default margin
        private int cornerRadius = 3; // Default corner radius


        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetTextBoxMargins();
            SetRegion();
        }

        private void SetTextBoxMargins()
        {
            SendMessage(this.Handle, EM_SETMARGINS, 1, (IntPtr)((margin << 16) | margin));
            SendMessage(this.Handle, EM_SETMARGINS, 3, (IntPtr)((margin << 16) | margin));



        }


        private void SetRegion()
        {
            IntPtr hRgn = CreateRoundRectRgn(0, 0, Width, Height, cornerRadius, cornerRadius);
            SetWindowRgn(this.Handle, hRgn, true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRegion();
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // Properties to control the appearance
        public int MarginSize
        {
            get { return margin; }
            set { margin = value; SetTextBoxMargins(); }
        }

        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; SetRegion(); }
        }
    }

    public class CustomDataGridView : DataGridView
    {
        private const int EM_SETMARGINS = 0xD3;
        private int margin = 15;
        private int cornerRadius = 3;

        public CustomDataGridView()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetTextBoxMargins();
        }

        private void SetTextBoxMargins()
        {

            SendMessage(this.Controls[1].Handle, EM_SETMARGINS, 1, (IntPtr)((margin << 16) | margin));
        }


        private void SetRegion()
        {
            IntPtr hRgn = CreateRoundRectRgn(0, 0, Width, Height, cornerRadius, cornerRadius);
            SetWindowRgn(this.Handle, hRgn, true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRegion();
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // Properties to control the appearance
        public int MarginSize
        {
            get { return margin; }
            set { margin = value; SetTextBoxMargins(); }
        }

        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; SetRegion(); }
        }
    }

    public class BorderlessTabControl : TabControl
    {
        public Color TabBarBackColor { get; set; } = Color.Gray;
        public Color ActiveBackground { get; set; } = Color.Yellow;
        public Color ActiveText { get; set; } = Color.Green;
        public Color InactiveBackground { get; set; } = Color.Red;
        public Color InactiveText { get; set; } = Color.Blue;


        public BorderlessTabControl()
        {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.DrawItem += new DrawItemEventHandler(DrawTab);
            //Console.WriteLine("BorderlessTabControl initialized");
            this.Padding = new Point(20, 10);
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x1328 && !DesignMode) // 0x1328 is TCM_ADJUSTRECT
            {
                RECT rect = (RECT)m.GetLParam(typeof(RECT));
                rect.Left -= 5;  // Adjust as needed to remove the border
                rect.Top -= 5;
                rect.Right += 5;
                rect.Bottom += 5;
                Marshal.StructureToPtr(rect, m.LParam, true);
                // Console.WriteLine("WndProc called with TCM_ADJUSTRECT");
            }
        }

        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            //Console.WriteLine("Drawing tab index: " + e.Index);
            Graphics g = e.Graphics;
            TabPage tabPage = this.TabPages[e.Index];
            Rectangle tabBounds = this.GetTabRect(e.Index);

            Brush textBrush;
            Brush backgroundBrush;

            if (e.State == DrawItemState.Selected)
            {
                textBrush = new SolidBrush(ActiveText);
                backgroundBrush = new SolidBrush(ActiveBackground);
            }
            else
            {
                textBrush = new SolidBrush(InactiveText);
                backgroundBrush = new SolidBrush(InactiveBackground);
            }


            // Draw the tab background with rounded corners (top only)
            using (GraphicsPath path = CreateTabPath(tabBounds))
            {
                g.FillPath(backgroundBrush, path);
            }

            // Use StringFormat to center the text in the tab
            StringFormat stringFlags = new StringFormat();
            stringFlags.Alignment = StringAlignment.Center;
            stringFlags.LineAlignment = StringAlignment.Center;

            // Draw text
            g.DrawString(tabPage.Text, this.Font, textBrush, tabBounds, stringFlags);

            // Clean up
            textBrush.Dispose();
            backgroundBrush.Dispose();
        }

        private GraphicsPath CreateTabPath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 10; // Change this value for more or less rounded corners
            path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top + radius);
            path.AddArc(rect.Left, rect.Top, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Top, radius, radius, 270, 90);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Draw the background of the tab control
            using (Brush backBrush = new SolidBrush(this.TabBarBackColor))
            {
                e.Graphics.FillRectangle(backBrush, this.ClientRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Ensure the background of the tab control is painted correctly
            using (Brush backBrush = new SolidBrush(this.TabBarBackColor))
            {
                e.Graphics.FillRectangle(backBrush, this.ClientRectangle);
            }

            // Custom paint the tabs
            for (int i = 0; i < this.TabCount; i++)
            {
                DrawTab(this, new DrawItemEventArgs(e.Graphics, this.Font, this.GetTabRect(i), i, (this.SelectedIndex == i) ? DrawItemState.Selected : DrawItemState.None));
            }
        }
    }

    public partial class CustomSplashScreen : Form
    {
        private const int CORNER_RADIUS = 75;
        public CustomSplashScreen(Image background)
        {

            // Configure the form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.BackgroundImage = background;
            // Adjust the size of the form to fit the background image
            this.ClientSize = background.Size;
            // Set the rounded region (if this is necessary)
            SetRoundedRegion();
        }


        private void SetRoundedRegion()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, CORNER_RADIUS * 2, CORNER_RADIUS * 2, 180, 90); // Top-left corner
            path.AddArc(this.Width - CORNER_RADIUS * 2, 0, CORNER_RADIUS * 2, CORNER_RADIUS * 2, 270, 90); // Top-right corner
            path.AddArc(this.Width - CORNER_RADIUS * 2, this.Height - CORNER_RADIUS * 2, CORNER_RADIUS * 2, CORNER_RADIUS * 2, 0, 90); // Bottom-right corner
            path.AddArc(0, this.Height - CORNER_RADIUS * 2, CORNER_RADIUS * 2, CORNER_RADIUS * 2, 90, 90); // Bottom-left corner
            path.CloseAllFigures();

            this.Region = new Region(path);
        }




    }

}