using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Reflection.Emit;
using Label = System.Windows.Forms.Label;
using Streamer.bot.Plugin.Interface.Model;


namespace StreamUP
{
    #region CustomElements

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


    #endregion

    public class SaveSettings
    {

        public string SettingName { get; set; }
        public object SettingValue { get; set; }

    }


    public static class Extensions
    {
        public static void SUSBRunActionButton(this IInlineInvokeProxy CPH, string actionName, bool runImmediately)
        {
            CPH.RunAction(actionName, runImmediately);
        }
    }


    public static class StreamUpSettingsBuilder
    {
        public static TableLayoutPanel settingsTable;
        private static FlowLayoutPanel buttonPanel;
        private static TabControl tabControl;
        private static Button saveButton;
        private static Button resetButton;
        private static ToolTip toolTip = new ToolTip();
        private static Random random = new Random();

        private static readonly Color backColour1 = ColorTranslator.FromHtml("#121212");
        private static readonly Color backColour2 = ColorTranslator.FromHtml("#676767");
        private static readonly Color backColour3 = ColorTranslator.FromHtml("#212121");
        private static readonly Color forecolour1 = Color.WhiteSmoke;
        private static readonly Color forecolour2 = Color.SkyBlue;
        private static readonly Color forecolour3 = Color.Black;
        private static readonly Color linkColour = ColorTranslator.FromHtml("#FF86BD");
        private static readonly Color boolTrueColor = Color.SeaGreen;
        private static readonly Color boolFalseColor = Color.IndianRed;
        private static readonly Font entryFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular);
        private static readonly Font valueFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private static readonly Font labelFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular);
        private static readonly Font headingFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Bold);
        private static readonly Font descriptionFont = new Font("Segoe UI Emoji", 10.0F, FontStyle.Italic);
        private static readonly Font linkFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private static readonly Font spaceFont = new Font("Segoe UI Emoji", 15.0F, FontStyle.Bold);
        private static readonly Font buttonFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Bold);
        private static readonly Font buttonUIFont = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular);
        private static readonly Font buttonFileFont = new Font("Segoe UI Emoji", 15.0F, FontStyle.Bold);
        private static readonly Font tabFont = new Font("Segoe UI Emoji", 11.0F, FontStyle.Bold);
        private static readonly Font productInfoFont = new Font("Monospace", 8.0F, FontStyle.Bold);
        //private static SplashScreen splashScreen;

        // List to hold controls
        private static readonly List<Control> controls = new List<Control>();
        public static Form SUSBBuildForm(this IInlineInvokeProxy CPH, string title, List<Control> layout, ProductInfo productInfo, int imageFilePath = -1)
        {
            Image background = SUSBGetRandomImageIcon();
            CustomSplashScreen splashScreen = new CustomSplashScreen(background);
            splashScreen.Show();

            Thread.Sleep(3000);

            splashScreen.Close();

            Form form = CPH.SUSBCreateMainForm(title, layout, productInfo, imageFilePath);

            return form;

        }
        public static Form SUSBCreateMainForm(this IInlineInvokeProxy CPH, string title, List<Control> layout, ProductInfo productInfo, int imageFilePath = -1)
        {




            // Dictionary to hold controls for each tab
            Dictionary<string, List<Control>> tabControls = new Dictionary<string, List<Control>>();

            // Separate controls based on their associated tab name
            foreach (Control control in layout)
            {
                string tabName = control.Tag as string ?? "General"; // Default tab if no tab specified

                if (!tabControls.ContainsKey(tabName))
                {
                    tabControls[tabName] = new List<Control>();
                }
                tabControls[tabName].Add(control);
            }

            tabControl = new BorderlessTabControl()
            {
                Dock = DockStyle.Top | DockStyle.Fill,
                Font = tabFont,
                ActiveText = forecolour1,
                ActiveBackground = backColour1,
                InactiveText = forecolour2,
                InactiveBackground = backColour3,
                TabBarBackColor = backColour3,


            };


            // Create a form
            var form = new Form
            {
                Text = title,
                Width = 800,
                Height = 800,
                FormBorderStyle = FormBorderStyle.Sizable,
                Icon = SUSBGetIconOrDefault(imageFilePath),
                BackColor = backColour1,

            };

            // Create and add tab pages
            foreach (var tab in tabControls)
            {
                TabPage tabPage = new TabPage(tab.Key)
                {

                    BackColor = backColour1,

                };

                TableLayoutPanel tableLayout = new TableLayoutPanel
                {
                    ColumnCount = 1,
                    AutoSize = true,
                    Dock = DockStyle.Top | DockStyle.Fill,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoScroll = true
                };



                int modValue = 0;
                foreach (Control control in tab.Value)
                {
                    int rowIndex = tableLayout.RowCount++;
                    tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    tableLayout.Controls.Add(control, 0, rowIndex);
                    tableLayout.SetColumnSpan(control, 2);
                    if (control.Name == "thisisjustaline")
                    {
                        modValue = modValue == 0 ? 1 : 0;
                    }
                    control.BackColor = (rowIndex % 2) == modValue ? backColour1 : backColour3;
                }




                tabPage.Controls.Add(tableLayout);
                tabControl.TabPages.Add(tabPage);
            }

            //Create "About" tab page
            TabPage aboutTabPage = new TabPage("About")
            {
                BackColor = backColour1
            };

            // Add your specific controls to the About tab
            aboutTabPage.Controls.Add(SUSBCreateAboutPanel(productInfo));

            tabControl.TabPages.Add(aboutTabPage);

            buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5),
                AutoSize = true,
                ForeColor = forecolour1,

                Font = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular)
            };
            saveButton = new RoundedButton
            {
                Text = "💾 Save",
                Padding = new Padding(10),
                Font = buttonUIFont,
                AutoSize = true,
                ForeColor = forecolour1,
                BackColor = boolTrueColor,
                FlatStyle = FlatStyle.Flat,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
            };

            saveButton.Click += (sender, e) => CPH.SUSaveButton_Click(sender, e, layout);

            resetButton = new RoundedButton
            {
                Text = "🔄 Reset",
                Padding = new Padding(10),
                AutoSize = true,
                BackColor = boolFalseColor,
                ForeColor = forecolour1,
                Font = buttonUIFont,
                FlatStyle = FlatStyle.Flat,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
            };
            resetButton.Click += (sender, e) => CPH.SUResetButton_Click(sender, e, layout, form);

            saveButton.FlatAppearance.BorderSize = 0;

            toolTip.SetToolTip(saveButton, "Save the Data");
            resetButton.FlatAppearance.BorderSize = 0;

            toolTip.SetToolTip(resetButton, "Reset the Data to Defaults");



            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(resetButton);



            var statusBar = new StatusStrip
            {
                BackColor = Color.DimGray,
                SizingGrip = false
            };
            var statusLabel = new ToolStripStatusLabel
            {
                Text = $"© StreamUP {DateTime.UtcNow.Year}",
                AutoSize = true,
                ForeColor = Color.Ivory
            };
            statusBar.Items.Add(statusLabel);

            form.Controls.Add(tabControl);
            form.Controls.Add(buttonPanel);
            form.Controls.Add(statusBar);
            //form.ShowDialog();



            return form;
        }
        private static Panel SUSBCreateAboutPanel(ProductInfo productInfo)
        {
            Panel aboutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backColour1,
                AutoScroll = true
            };

            FlowLayoutPanel flowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                //AutoSize = true,
                WrapContents = false
            };


            Label outtro = new Label
            {
                Text = "This UI Settings is brought to you by The StreamUP Team. \n You can support The StreamUP through the following methods:",
                AutoSize = true,
                Padding = new Padding(6),
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = labelFont
            };



            Label creditsHeading = new Label
            {
                Text = "Special Thanks",
                AutoSize = true,
                ForeColor = forecolour2,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = headingFont
            };

            Label creditsIntro = new Label
            {
                Text = "This UI would not be possibile with out the help and support of the following people. \n In no particular order:",
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = labelFont,
                Padding = new Padding(0, 0, 0, 25)
            };

            Label creditsPeople = new Label
            {
                Text = SUSBGetNamesForCredits(),
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = labelFont,
                Padding = new Padding(0, 0, 0, 25)
            };



            Label disclaimer = new Label
            {
                Text = "DISCLAIMER - By downloading this product you agree to not redistribute AND/OR sell it for any profit whatsoever. Feel free to completely edit this product and change it for your needs. We do not offer support on any modifications.",
                AutoSize = true,
                Padding = new Padding(6),
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.Bottom,
                //TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI Emoji", 8.0F, FontStyle.Regular)
            };

            Label productInfoLabel = new Label
            {
                Text = "Product Info",
                AutoSize = true,
                ForeColor = forecolour2,
                Anchor = AnchorStyles.Bottom,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font("Segoe UI Emoji", 12.0F, FontStyle.Regular)
            };



            Label lblProductName = SUSBCreateInfoLabel("Product Name:", productInfo.ProductName);
            Label lblProductVersionNumber = SUSBCreateInfoLabel("Product Version:", productInfo.ProductVersionNumber.ToString());
            Label lblProductNumber = SUSBCreateInfoLabel("Product Number:", productInfo.ProductNumber);
            Label lblRequiredLibraryVersion = SUSBCreateInfoLabel("Required Library Version:", productInfo.RequiredLibraryVersion.ToString());
            Label lblSceneName = SUSBCreateInfoLabel("Scene Name:", productInfo.SceneName);
            Label lblSettingsAction = SUSBCreateInfoLabel("Settings Action:", productInfo.SettingsAction);
            Label lblSourceNameVersionCheck = SUSBCreateInfoLabel("Source Name Version Check:", productInfo.SourceNameVersionCheck);
            Label lblSourceNameVersionNumber = SUSBCreateInfoLabel("Source Name Version Number:", productInfo.SourceNameVersionNumber.ToString());

            Label line1 = SUSBCreateLine();
            Label line2 = SUSBCreateLine();
            Label line3 = SUSBCreateLine();
            Label line4 = SUSBCreateLine();



            LinkLabel streamUp = SUSBCreateLinkLabel("StreamUP", "https://doras.to/streamup");
            LinkLabel andi = SUSBCreateLinkLabel("Andilippi", "https://doras.to/andi", "Founder");
            LinkLabel waldo = SUSBCreateLinkLabel("WaldoAndFriends", "https://doras.to/waldo", "Founder");
            LinkLabel silver = SUSBCreateLinkLabel("Silverlink", "https://doras.to/silverlink", "Founder");
            LinkLabel terrierdarts = SUSBCreateLinkLabel("TerrierDarts", "https://doras.to/terrierdarts", "Developer");


            flowLayout.Controls.Add(productInfoLabel);
            flowLayout.Controls.Add(lblProductName);
            flowLayout.Controls.Add(lblProductVersionNumber);
            flowLayout.Controls.Add(lblProductNumber);
            flowLayout.Controls.Add(lblRequiredLibraryVersion);
            flowLayout.Controls.Add(lblSceneName);
            flowLayout.Controls.Add(lblSettingsAction);
            flowLayout.Controls.Add(lblSourceNameVersionCheck);
            flowLayout.Controls.Add(lblSourceNameVersionNumber);
            flowLayout.Controls.Add(line1);
            flowLayout.Controls.Add(outtro);
            flowLayout.Controls.Add(streamUp);
            flowLayout.Controls.Add(andi);
            flowLayout.Controls.Add(waldo);
            flowLayout.Controls.Add(silver);
            flowLayout.Controls.Add(terrierdarts);
            flowLayout.Controls.Add(line2);
            flowLayout.Controls.Add(creditsHeading);
            flowLayout.Controls.Add(creditsIntro);
            flowLayout.Controls.Add(creditsPeople);
            flowLayout.Controls.Add(line3);
            flowLayout.Controls.Add(disclaimer);




            aboutPanel.Controls.Add(flowLayout);
            return aboutPanel;
        }
        private static string SUSBGetNamesForCredits()
        {
            List<string> nonFinanceSupporters = new List<string>
        {
            "ConfuzzedCat",
            "BitGamey",
            "GoWMan",
            "Web_Mage",
            "tawmae",
            "Mustached_Maniac",
            "Rondhi",
            "Lyfesaver74",
            "Geocym"
        };

            List<string> paidSupporters = new List<string>
        {
            "Bongo1986",
            "Jesus_Pals"
        };

            // Combine the lists
            List<string> allSupporters = nonFinanceSupporters.Concat(paidSupporters).ToList();

            // Randomize the list
            Random rng = new Random();
            int n = allSupporters.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = allSupporters[k];
                allSupporters[k] = allSupporters[n];
                allSupporters[n] = value;
            }

            // Convert list to string
            string result = string.Join(", ", allSupporters);


            result += " and all the people who support us financially or whom we may have missed.";

            return result;


        }
        private static LinkLabel SUSBCreateLinkLabel(string text, string url, string subhead = "")
        {
            LinkLabel linkLabel = new LinkLabel
            {
                Text = text, // Add a space before the text for better spacing
                AutoSize = true,
                LinkColor = linkColour,
                Anchor = AnchorStyles.None,
                Padding = new Padding(3),
                Font = linkFont,
            };



            linkLabel.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(url);
            return linkLabel;
        }
        private static Label SUSBCreateLine()
        {

            Label line = new Label
            {

                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(10),
                ForeColor = forecolour1,
            };

            return line;


        }
        private static Label SUSBCreateInfoLabel(string text, string value)
        {
            Label label = new Label
            {
                Text = $"{text} {value}", // Add a space before the text for better spacing
                AutoSize = true,
                ForeColor = forecolour1,
                Padding = new Padding(3),
                Anchor = AnchorStyles.None,
                Font = productInfoFont,
            };

            return label;
        }
        private static Icon SUSBGetIconOrDefault(int imageNumber = -1)
        {


            byte[] bytes;// = Convert.FromBase64String(UIResources.supIconString); 
            Icon icon;
            if (imageNumber == -1)
            {
                imageNumber = random.Next(1, 3);
            }
            switch (imageNumber)
            {
                case 1:
                    bytes = Convert.FromBase64String(UIResources.cyanStacked);
                    break;
                case 2:
                    bytes = Convert.FromBase64String(UIResources.midnightStacked);
                    break;
                default:
                    bytes = Convert.FromBase64String(UIResources.supIconString);
                    break;
            }

            var ms = new MemoryStream(bytes);
            icon = new Icon(ms);

            return icon;
        }

        public static void SUSBSetButtonColor(Button button, string defaultValue)
        {
            // Convert the default value (assumed to be a hex color string) to a Color object
            if (ColorTranslator.FromHtml(defaultValue) is Color defaultColor)
            {
                button.BackColor = defaultColor;
                // Set the button's text color based on the brightness of the background color
                button.ForeColor = defaultColor.GetBrightness() < 0.5 ? Color.White : Color.Black;
            }
        }

        private static Image SUSBGetRandomImageIcon()
        {
            // Logic to return one of the 7 base64 icons
            string[] base64Icons = new string[] {
            UIResources.midnightWhiteSplash,
            UIResources.midnightCyanSplash,
            UIResources.creamMidnightSplash,
            UIResources.grayMidightSplash,
            UIResources.whiteBlackSplash,
            UIResources.cyanMidnightSplash,
            UIResources.pinkMidnightSplash,
            };

            Random random = new Random();
            int index = random.Next(base64Icons.Length);

            string base64Icon = base64Icons[index];

            // Convert base64 string to Image
            byte[] imageBytes = Convert.FromBase64String(base64Icon);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        #region Methods

        //HEADINGS AND LABELS
        public static List<Control> SUSBAddHeading(this IInlineInvokeProxy CPH, string headingText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var heading = new Label
            {
                Text = headingText,
                Font = headingFont,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                ForeColor = forecolour2,
                Tag = tabName
            };

            settings.Add(heading); // Add the label directly to the list
            return settings;
        }
        public static List<Control> SUSBAddLabel(this IInlineInvokeProxy CPH, string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Font = labelFont,
                ForeColor = forecolour1,
                Tag = tabName

            };

            settings.Add(label);
            return settings;
        }

        public static List<Control> SUSBAddLink(this IInlineInvokeProxy CPH, string labelText, string url, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            LinkLabel linkLabel = new LinkLabel
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                LinkColor = linkColour,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Font = linkFont,
                Tag = tabName
            };

            linkLabel.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(url);

            settings.Add(linkLabel);
            return settings;
        }
        public static List<Control> SUSBAddLine(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var rule = new Label
            {
                Name = "thisisjustaline",
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Dock = DockStyle.Top,
                ForeColor = forecolour1,
                Tag = tabName,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = false,
            };

            settings.Add(rule);
            return settings;

        }
        public static List<Control> SUSBAddDescription(this IInlineInvokeProxy CPH, string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();


            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Font = descriptionFont,
                Tag = tabName
            };

            settings.Add(label); // Add control to the list
            return settings;
        }
        public static List<Control> SUSBAddSpace(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var space = new Label
            {
                Name = "thisisjustaline",
                Text = Environment.NewLine,
                Font = spaceFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Tag = tabName,


            };

            settings.Add(space);
            return settings;

        }
        public static List<Control> SUSBAddEnd(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var space = new Label
            {

                Text = Environment.NewLine,
                Font = spaceFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Tag = tabName,


            };

            settings.Add(space);
            return settings;

        }


        //TEXT
        public static List<Control> SUSBAddTextBox(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                //AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));


            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };



            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);


            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddMultiLineTextBox(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(0),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Multiline = true,
                Height = 100,
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,


            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddLimitText(this IInlineInvokeProxy CPH, string description, string defaultValue, int textLimit, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Font = labelFont,
                ForeColor = forecolour1,
                //Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                MaxLength = textLimit,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,



            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddList(this IInlineInvokeProxy CPH, string description, List<string> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("Items", "Items");
            List<string> rowsToAdd = CPH.SUGetSetting<List<string>>(saveName, defaultValue);
            foreach (string entry in rowsToAdd)
            {
                input.Rows.Add(entry); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }

        public static List<Control> SUSBAddPassword(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName



            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, ""),
                UseSystemPasswordChar = true,
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                Height = 23,

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddDropDown(this IInlineInvokeProxy CPH, string description, string[] dropdown, string defaultValue, string saveName, string tabName = "General")
        {

            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                ForeColor = forecolour1,
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };

            var input = new ComboBox
            {
                Name = saveName,
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,


            };

            input.Items.AddRange(dropdown);
            input.SelectedItem = CPH.SUGetSetting<string>(saveName, defaultValue);

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;



        }
        public static List<Control> SUSBAddStringDictionary(this IInlineInvokeProxy CPH, string description, string columnOneName, string columnTwoName, Dictionary<string, string> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = true,
                RowHeadersVisible = true,
                RowHeadersWidth = 25,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("columnOne", columnOneName);

            input.Columns.Add("columnTwo", columnTwoName);
            Dictionary<string, string> rowsToAdd = CPH.SUGetSetting<Dictionary<string, string>>(saveName, defaultValue);
            foreach (KeyValuePair<string, string> entry in rowsToAdd)
            {
                input.Rows.Add(entry.Key, entry.Value); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 0, 1);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        //NUMBERS
        public static List<Control> SUSBAddInt(this IInlineInvokeProxy CPH, string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = CPH.SUGetSetting<int>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Left,


            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddDecimal(this IInlineInvokeProxy CPH, string description, decimal defaultValue, int decimalPlaces, decimal increments, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                DecimalPlaces = decimalPlaces,
                Increment = increments,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                Value = CPH.SUGetSetting<decimal>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddTrackbar(this IInlineInvokeProxy CPH, string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var input = new TrackBar
            {

                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = CPH.SUGetSetting<int>(saveName, defaultValue),
                //Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TickFrequency = (max - min) / 2, // Adjust according to your preference
                TickStyle = TickStyle.TopLeft

            };


            var valueLabel = new Label
            {
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                Text = input.Value.ToString(),
                ForeColor = forecolour2,
                Font = valueFont,
                TextAlign = ContentAlignment.BottomCenter,

            };

            input.ValueChanged += (sender, e) =>
            {
                valueLabel.Text = input.Value.ToString();
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddIntDictionary(this IInlineInvokeProxy CPH, string description, string columnOneName, string columnTwoName, Dictionary<string, int> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,

            };


            var input = new CustomDataGridView
            {
                Name = saveName,
                Height = 200,
                //Margin = new Padding(10),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = true,
                RowHeadersVisible = true,
                RowHeadersWidth = 25,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = backColour2,
                    ForeColor = forecolour1,
                    SelectionBackColor = backColour1,
                    SelectionForeColor = forecolour1
                },
                EnableHeadersVisualStyles = false,
                GridColor = backColour2,
                AllowUserToDeleteRows = true,
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                AutoSize = false,

            };






            input.Columns.Add("columnOne", columnOneName);

            input.Columns.Add("columnTwo", columnTwoName);
            Dictionary<string, int> rowsToAdd = CPH.SUGetSetting<Dictionary<string, int>>(saveName, defaultValue);
            foreach (KeyValuePair<string, int> entry in rowsToAdd)
            {
                input.Rows.Add(entry.Key, entry.Value); // Add each string directly, assuming input.Rows.Add() accepts individual objects
            }


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 0, 1);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        //BOOLS
        public static List<Control> SUSBAddTrueFalse(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                // Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = CPH.SUGetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = CPH.SUGetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = CPH.SUGetSetting<bool>(saveName, defaultValue) ? "True" : "False",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "True" : "False";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddYesNo(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = CPH.SUGetSetting<bool>(saveName, defaultValue),
                // Padding = new Padding(0),
                // Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = CPH.SUGetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = CPH.SUGetSetting<bool>(saveName, defaultValue) ? "Yes" : "No",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "Yes" : "No";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddOnOff(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,

                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = CPH.SUGetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolFalseColor,
                BoolTrueColor = boolTrueColor,
                BackColor = CPH.SUGetSetting<bool>(saveName, defaultValue) ? boolTrueColor : boolFalseColor,
                Text = CPH.SUGetSetting<bool>(saveName, defaultValue) ? "On" : "Off",
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolTrueColor : boolFalseColor;
                input.Text = input.Checked ? "On" : "Off";
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddCustomBool(this IInlineInvokeProxy CPH, string description, bool defaultValue, string trueName, string trueColor, string falseName, string falseColor, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            Color boolCustomTrueColor = ColorTranslator.FromHtml(trueColor);
            Color boolCustomFalseColor = ColorTranslator.FromHtml(falseColor);
            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                // Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new RoundedCheckBox
            {
                Name = saveName,
                Checked = CPH.SUGetSetting<bool>(saveName, defaultValue),
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                CornerRadius = 10,
                BoolFalseColor = boolCustomFalseColor,
                BoolTrueColor = boolCustomTrueColor,
                BackColor = CPH.SUGetSetting<bool>(saveName, defaultValue) ? boolCustomTrueColor : boolCustomFalseColor,
                Text = CPH.SUGetSetting<bool>(saveName, defaultValue) ? trueName : falseName,
                ForeColor = forecolour3,
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? boolCustomTrueColor : boolCustomFalseColor;
                input.Text = input.Checked ? trueName : falseName;
            };
            input.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(input, "Click to Change");

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddChecklist(this IInlineInvokeProxy CPH, string description, Dictionary<string, bool> checkListItems, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            // Create a new CheckBox control

            var input = new CheckedListBox
            {
                Name = saveName,
                //Padding = new Padding(0),
                //Margin = new Padding(0, 10, 10, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = backColour1,
                ForeColor = forecolour1,
                BorderStyle = BorderStyle.None,

                Width = 280,
                CheckOnClick = true,
                Font = entryFont,

            };



            Dictionary<string, bool> checkedItemsDict = CPH.SUGetSetting<Dictionary<string, bool>>(saveName, checkListItems);


            foreach (KeyValuePair<string, bool> checkItem in checkedItemsDict)
            {

                input.Items.Add(checkItem.Key, checkItem.Value);

            }
            input.Height = checkedItemsDict.Count * 20;





            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;

        }
        //OTHERS
        public static List<Control> SUSBAddColour(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new Label
            {
                Name = saveName + "OBS",
                AutoSize = true,
                Margin = new Padding(0, 6, 10, 0),
                Text = CPH.SUGetSetting<string>(saveName + "OBS", " "),
                ForeColor = forecolour2,
                Font = valueFont,
                TextAlign = ContentAlignment.BottomCenter,

            };
            var defaultColour = CPH.SUGetSetting<string>(saveName + "HTML", defaultValue);
            var input = new RoundedButton
            {
                Name = saveName + "HTML",
                Text = defaultColour,
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                BackColor = ColorTranslator.FromHtml(defaultColour),
                FlatStyle = FlatStyle.Flat,
                Size = new System.Drawing.Size(30, 30),
                Font = buttonFont,
                TextAlign = ContentAlignment.MiddleCenter,
                CornerRadius = 8,
                Cursor = Cursors.Hand
            };
            input.FlatAppearance.BorderSize = 0;

            // Event handler for button click to open color picker
            input.Click += (sender, e) =>
            {
                // Here, you should implement code to open a color picker dialog
                // and update the button text and value label with the selected color
                // For example:
                var colorDialog = new ColorDialog();
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedColor = colorDialog.Color;
                    input.BackColor = selectedColor;
                    input.ForeColor = selectedColor.GetBrightness() < 0.5 ? forecolour1 : forecolour3;
                    long alpha = selectedColor.A;
                    long green = selectedColor.G;
                    long blue = selectedColor.B;
                    long red = selectedColor.R;
                    long obsColour = (alpha * 256 * 256 * 256) + (green * 256 * 256) + (blue * 256) + red;
                    valueLabel.Text = $"{obsColour}";
                    string hexValue = selectedColor.R.ToString("X2") + selectedColor.G.ToString("X2") + selectedColor.B.ToString("X2");
                    input.Text = "#" + hexValue;
                    Console.WriteLine($"Selected color: ARGB({selectedColor.A}, {selectedColor.R}, {selectedColor.G}, {selectedColor.B})");
                }
            };



            valueLabel.Click += (sender, e) =>
            {

                var text = valueLabel.Text;
                Thread thread = new Thread(() => Clipboard.SetText(text));
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end
            };



            toolTip.SetToolTip(input, "Pick a Colour");
            toolTip.SetToolTip(valueLabel, "Click to Copy");

            SUSBSetButtonColor(input, defaultColour);

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddColor(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();


            settings.AddRange(CPH.SUSBAddColour(description, defaultValue, saveName, tabName));


            return settings;
        }
        public static List<Control> SUSBAddFile(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, ""),
                //Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };

            var input = new RoundedButton
            {
                Text = "🗁",
                AutoSize = true,
                //Margin = new Padding(10, 10, 0, 0),
                Size = new System.Drawing.Size(20, 20),
                ForeColor = forecolour2,
                Font = buttonFileFont,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.TopLeft,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK // Set DialogResult
            };
            input.FlatAppearance.BorderSize = 0;
            //input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            //input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            toolTip.SetToolTip(input, "Click to open explorer");

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFileDialogAsync();
                if (filePath != null)
                {
                    valueLabel.Text = filePath;
                    Console.WriteLine(filePath);
                }
            };


            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(valueLabel, 1, 0);
            settingsTable.Controls.Add(input, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddFolder(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName
            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };

            var valueLabel = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.SUGetSetting<string>(saveName, ""),
                //Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = entryFont,
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Height = 23,

            };

            var input = new RoundedButton
            {
                Text = "🗁",
                AutoSize = true,
                //Margin = new Padding(10, 10, 0, 0),
                Size = new System.Drawing.Size(20, 20),
                ForeColor = forecolour2,
                Font = buttonFileFont,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.TopLeft,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK // Set DialogResult
            };
            input.FlatAppearance.BorderSize = 0;
            //input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            //input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            toolTip.SetToolTip(input, "Click to open explorer");

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFolderDialogAsync();
                if (filePath != null)
                {
                    valueLabel.Text = filePath;
                    Console.WriteLine(filePath);
                }
            };


            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(valueLabel, 1, 0);
            settingsTable.Controls.Add(input, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddFont(this IInlineInvokeProxy CPH, string description, string defaultName, string defaultSize, string defaultStyle, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));



            var label = new Label
            {
                Text = description,
                AutoSize = true,
                Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };


            var input = new RoundedButton
            {
                Name = saveName + "Name",
                Text = CPH.SUGetSetting<string>(saveName + "Name", defaultName),
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                BackColor = backColour2,
                ForeColor = forecolour1,
                Font = buttonFont,
                Size = new System.Drawing.Size(280, 30),
                CornerRadius = 8,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            input.FlatAppearance.BorderSize = 0;
            input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            input.FlatAppearance.MouseDownBackColor = backColour1; // Click color

            var size = new Label
            {
                Name = saveName + "Size",
                Text = CPH.SUGetSetting<string>(saveName + "Size", defaultSize),
                AutoSize = true,
                //Margin = new Padding(10),
                Font = valueFont,
                ForeColor = forecolour2,

            };
            var style = new Label
            {
                Name = saveName + "Style",
                Text = CPH.SUGetSetting<string>(saveName + "Style", defaultStyle),
                AutoSize = true,
                //Margin = new Padding(10),
                Font = valueFont,
                ForeColor = forecolour2,

            };

            // Event handler for button click to open color picker
            input.Click += (sender, e) =>
            {
                // Here, you should implement code to open a color picker dialog
                // and update the button text and value label with the selected color
                // For example:
                var fontDialog = new FontDialog();
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {

                    input.Text = fontDialog.Font.Name.ToString();
                    size.Text = fontDialog.Font.Size.ToString();
                    style.Text = fontDialog.Font.Style.ToString();

                }
            };



            toolTip.SetToolTip(input, "Select a font");

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(size, 2, 1);
            settingsTable.Controls.Add(style, 1, 1);


            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SUSBAddRunAction(this IInlineInvokeProxy CPH, string description, string buttonText, string actionName, bool runImmediately, string tabName = "General")
        {


            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                Tag = tabName

            };

            // Define column styles for better control over sizing
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var label = new Label
            {
                Text = description,
                AutoSize = true,
                //Margin = new Padding(10),
                Font = labelFont,
                ForeColor = forecolour1,
            };



            var input = new RoundedButton
            {
                Text = $"{buttonText}",
                //Margin = new Padding(0, 10, 0, 0),
                Size = new System.Drawing.Size(280, 30),
                Font = buttonFont,
                AutoSize = true,
                ForeColor = forecolour1,
                BackColor = backColour2,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                CornerRadius = 8,
                Cursor = Cursors.Hand,
            };
            input.FlatAppearance.BorderSize = 0;
            input.FlatAppearance.MouseOverBackColor = backColour2; // Hover color
            input.FlatAppearance.MouseDownBackColor = backColour1; // Click color
            input.Click += (sender, e) =>
            {
                CPH.SUSBRunActionButton(actionName, runImmediately);
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            toolTip.SetToolTip(input, "Run the Action");
            // Add the table layout to the list of controls
            settings.Add(settingsTable);
            return settings;



        }
        public static List<Control> SUSBAddActionDrop(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<ActionData> actions = CPH.GetActions();
            List<string> actionDropDown = new List<string>();
            foreach (ActionData action in actions)
            {
                actionDropDown.Add(action.Name);
            }

            string[] actionsArray = actionDropDown.ToArray();
            Array.Sort(actionsArray);
            List<Control> settings = new List<Control>();
            settings.AddRange(CPH.SUSBAddDropDown(description, actionsArray, defaultValue, saveName, tabName));


            return settings;
        }

        //Logging
        public static void SUSBSettingsLog(this IInlineInvokeProxy CPH, string message)
        {
            string program_Directory = AppDomain.CurrentDomain.BaseDirectory;
            string formattedDate = DateTime.Now.ToString("yyyy_MM_dd");
            string dir = Path.Combine(program_Directory, "StreamUP", "Settings Files");
            string file_Path = Path.Combine(dir, $"logFile_{formattedDate}.log");

            // Create directory if it doesn't exist
            Directory.CreateDirectory(dir);

            if (!File.Exists(file_Path))
            {
                File.Create(file_Path).Close();


            }
            string timestamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ";
            File.AppendAllText(file_Path, timestamp + message + Environment.NewLine);


        }

        #endregion

        #region Setting Saving

        public static void SUSaveButton_Click(this IInlineInvokeProxy CPH, object sender, EventArgs e, List<Control> layout)
        {
            CPH.SUSBSettingsLog("Pressed Save");

            var numericUpDownsAndTextBoxes = layout
                .OfType<TableLayoutPanel>()
                .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                switch (control)
                {
                    case Label label:
                        CPH.SUSBSettingsLog($"Save Name: {label.Name}, Value: {label.Text}");
                        if (!string.IsNullOrEmpty(label.Name))
                        {
                            CPH.SUSaveSetting(label.Name, label.Text);
                        }
                        break;
                    case NumericUpDown numericUpDown:
                        CPH.SUSBSettingsLog($"Save Name: {numericUpDown.Name}, Value: {numericUpDown.Value}");
                        CPH.SUSaveSetting(numericUpDown.Name, (double)numericUpDown.Value);
                        break;
                    case TextBox textBox:
                        CPH.SUSBSettingsLog($"Save Name: {textBox.Name}, Text: {textBox.Text}");
                        CPH.SUSaveSetting(textBox.Name, textBox.Text);
                        break;
                    case CheckBox checkbox:
                        CPH.SUSBSettingsLog($"Save Name: {checkbox.Name}, Text: {checkbox.Checked}");
                        CPH.SUSaveSetting(checkbox.Name, checkbox.Checked);
                        break;
                    case TrackBar trackbar:
                        CPH.SUSBSettingsLog($"Save Name: {trackbar.Name}, Text: {trackbar.Value}");
                        CPH.SUSaveSetting(trackbar.Name, trackbar.Value);
                        break;
                    case Button button:
                        CPH.SUSBSettingsLog($"Save Name: {button.Name}, Text: {button.Text}");
                        CPH.SUSaveSetting(button.Name, button.Text);
                        break;
                    case ComboBox comboBox:
                        CPH.SUSBSettingsLog($"Save Name: {comboBox.Name}, Text: {comboBox.SelectedItem}");
                        CPH.SUSaveSetting(comboBox.Name, comboBox.SelectedItem);
                        break;
                    case CheckedListBox checkedListBox:
                        var checkedItemsDict = new Dictionary<string, bool>();
                        foreach (var item in checkedListBox.Items)
                        {
                            string itemName = item.ToString();
                            bool isChecked = checkedListBox.GetItemChecked(checkedListBox.Items.IndexOf(item));
                            checkedItemsDict[itemName] = isChecked;
                        }
                        var jsonData = JsonConvert.SerializeObject(checkedItemsDict);
                        CPH.SUSBSettingsLog($"{checkedListBox.Name}, Items: {string.Join(", ", checkedItemsDict.Select(kv => $"[{kv.Key}, {kv.Value}]"))}");
                        CPH.SUSaveSetting(checkedListBox.Name, checkedItemsDict);
                        break;
                    case DataGridView dataGridView:
                        if (dataGridView.Columns.Count == 1)
                        {
                            var dataRows = new List<string>();
                            foreach (DataGridViewRow row in dataGridView.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    string item = row.Cells[0].Value?.ToString();
                                    if (!string.IsNullOrEmpty(item))
                                    {
                                        dataRows.Add(item);
                                    }
                                }
                            }
                            //CPH.SUSBSettingsLog($"Save Name: {dataGridView.Name}, Data: {string.Join(",", dataRows.ToArray())}");
                            CPH.SUSaveSetting(dataGridView.Name, dataRows);
                        }
                        else if (dataGridView.Columns.Count >= 2)
                        {
                            bool isIntDict = true;
                            var dataDictString = new Dictionary<string, string>();
                            var dataDictInt = new Dictionary<string, int>();

                            foreach (DataGridViewRow row in dataGridView.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    string key = row.Cells[0].Value?.ToString();
                                    string value = row.Cells[1].Value?.ToString();

                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        if (int.TryParse(value, out int intValue))
                                        {
                                            dataDictInt[key] = intValue;
                                        }
                                        else
                                        {
                                            isIntDict = false;
                                            dataDictString[key] = value ?? string.Empty;
                                        }
                                    }
                                }
                            }

                            if (isIntDict)
                            {

                                CPH.SUSaveSetting(dataGridView.Name, dataDictInt);
                            }
                            else
                            {

                                CPH.SUSaveSetting(dataGridView.Name, dataDictString);
                            }



                        }
                        break;
                }
            }

            MessageBox.Show("Settings have been saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void SUResetButton_Click(this IInlineInvokeProxy CPH, object sender, EventArgs e, List<Control> layout, Form form)
        {
            CPH.SUSBSettingsLog("Pressed Reset");

            var numericUpDownsAndTextBoxes = layout
         .OfType<TableLayoutPanel>()
         .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                CPH.SUDeleteSetting(control.Name);
            }
            // Implement reset logic here
            MessageBox.Show("Settings have been reset.", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            form.Close();
        }


        public static T SUGetSetting<T>(this IInlineInvokeProxy CPH, string settingName, T defaultValue)
        {
            CPH.TryGetArg("saveFile", out string saveFile);
            
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, $"Product_Settings_{saveFile}.json");
            var database = new SimpleDatabase(filePath);
            var value = database.Get(settingName, defaultValue);
            return value;

        }
        public static void SUSaveSetting(this IInlineInvokeProxy CPH, string settingName, object newValue)
        {
            CPH.TryGetArg("saveFile", out string saveFile);
            
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, $"Product_Settings_{saveFile}.json");
            var database = new SimpleDatabase(filePath);


            database.Update(settingName, newValue);


        }
        public static void SUDeleteSetting(this IInlineInvokeProxy CPH, string settingName)
        {
            CPH.TryGetArg("saveFile", out string saveFile);
            
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, $"Product_Settings_{saveFile}.json");
            var database = new SimpleDatabase(filePath);


            database.Delete(settingName);


        }
    }

    #endregion




}
