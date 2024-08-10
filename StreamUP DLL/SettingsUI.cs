using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LiteDB;
using Streamer.bot.Plugin.Interface;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Reflection.Emit;
using Label = System.Windows.Forms.Label;

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
        public ObjectId Id { get; set; }
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


        /*
        private static Icon ConvertToIcon(Bitmap imagePath)
        {
            try
            {
                using (System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath))
                {
                    // Convert the image to an icon
                    return Icon.FromHandle(((Bitmap)img).GetHicon());
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., file not found, invalid image format)
                Console.WriteLine("Error converting image to icon: " + ex.Message);
                return null;
            }
        }
        private static void DownloadImage(string imageUrl)
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(programDirectory, "Extensions", "Data", "iconImage.png");
            using (WebClient client = new WebClient())
            {
                try
                {
                    // Download the image data
                    byte[] imageData = client.DownloadData(imageUrl);

                    // Write the byte array to a file
                    File.WriteAllBytes(filePath, imageData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to retrieve the image. Exception: " + ex.Message);
                }
            }
        }
        */
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

        private static Task<string> OpenFileDialogAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            Thread thread = new Thread(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    DialogResult result = openFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        tcs.SetResult(openFileDialog.FileName);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA); // Set the thread to STA
            thread.Start();

            return tcs.Task;
        }
        private static Task<string> OpenFolderDialogAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            Thread thread = new Thread(() =>
            {
                using (FolderBrowserDialog openFolderDialog = new FolderBrowserDialog())
                {
                    DialogResult result = openFolderDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        tcs.SetResult(openFolderDialog.SelectedPath);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA); // Set the thread to STA
            thread.Start();

            return tcs.Task;
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
        //Streamerbot Base Stuff
        /* public static List<Control> SUSBAddActionDrop(this IInlineInvokeProxy CPH, string description,string defaultValue, string saveName, string tabName = "General")
         {
             List<ActionData> actions = CPH.GetActions();
             List<string> actionDropDown = new List<string>();
             foreach (ActionData action in actions)
             {
                 actionDropDown.Add(action.Name);
             }
             List<Control> settings = new List<Control>();

             string[] actionsArray = actionDropDown.ToArray();
             actionsArray.Sort();
             settings.AddRange(CPH.SUSBAddDropDown(description, actionsArray, defaultValue, saveName, tabName));


             return settings;
         }
         */
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
                        CPH.SUSaveSetting(label.Name, label.Text);
                        break;
                    case NumericUpDown numericUpDown:
                        CPH.SUSBSettingsLog($"Save Name: {numericUpDown.Name}, Value: {numericUpDown.Value}");
                        CPH.SUSaveSetting(numericUpDown.Name, numericUpDown.Value);
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

                        Dictionary<string, bool> checkedItemsDict = new Dictionary<string, bool>();

                        foreach (var item in checkedListBox.Items)
                        {
                            string itemName = item.ToString();
                            bool isChecked = checkedListBox.GetItemChecked(checkedListBox.Items.IndexOf(item));
                            checkedItemsDict[itemName] = isChecked;
                        }
                        CPH.SUSBSettingsLog($"{checkedListBox.Name}, Items: {string.Join(", ", checkedItemsDict.Select(kv => $"[{kv.Key}, {kv.Value}]"))}");
                        // Serialize the dictionary to a JSON string
                        string jsonData = JsonConvert.SerializeObject(checkedItemsDict);

                        // Save the JSON string
                        CPH.SUSaveSetting(checkedListBox.Name, jsonData);
                        break;
                    case DataGridView dataGridView:

                        List<string> dataRows = new List<string>();

                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            if (!row.IsNewRow) // Skip the new row at the end
                            {
                                string item = row.Cells[0].Value?.ToString(); // Assuming there's only one column
                                dataRows.Add(item);

                            }

                        }
                        CPH.SUSBSettingsLog($"Save Name: {dataGridView.Name}, Text: {string.Join(",", dataRows.ToArray())}");
                        CPH.SUSaveSetting(dataGridView.Name, string.Join(",", dataRows.ToArray()));
                        break;


                        // Add other cases for different types of controls if needed
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
            form.Hide();
        }
        public static void SUSaveSetting(this IInlineInvokeProxy CPH, string settingName, object settingValue)
        {
            if (!string.IsNullOrEmpty(settingName))
            {


                string program_Directory = AppDomain.CurrentDomain.BaseDirectory;
                string dir = Path.Combine(program_Directory, "StreamUP", "Data");
                Directory.CreateDirectory(dir);
                string file_Path = Path.Combine(dir, "Product_Settings.db");
                using (var db = new LiteDatabase($"Filename={file_Path}; Connection=shared"))
                {
                    var col = db.GetCollection<SaveSettings>("settings");

                    // Check if the setting already exists
                    var existingSetting = col.FindOne(Query.EQ("SettingName", settingName));
                    if (existingSetting != null)
                    {
                        // Update the existing setting
                        existingSetting.SettingValue = settingValue;
                        col.Update(existingSetting);
                    }
                    else
                    {
                        // Insert a new setting
                        col.Insert(new SaveSettings { SettingName = settingName, SettingValue = settingValue });
                    }
                }
            }
        }
        public static T SUGetSetting<T>(this IInlineInvokeProxy CPH, string settingName, T defaultValue)
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, "Product_Settings.db");

            using (var db = new LiteDatabase($"Filename={filePath}; Connection=shared"))
            {
                var col = db.GetCollection<SaveSettings>("settings");
                var settings = col.FindOne(Query.EQ("SettingName", settingName));
                if (settings != null)
                {
                    if (typeof(T) == typeof(List<string>))
                    {
                        // Check if the stored value is actually an array
                        if (settings.SettingValue != null)
                        {
                            // Split the string and convert it to a list of strings
                            List<string> listValue = settings.SettingValue.ToString()
                                .Trim('[', ']') // Remove brackets from the string
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim(' ', '"')) // Trim whitespace and quotes from each element
                                .ToList();
                            return (T)Convert.ChangeType(listValue, typeof(T));
                        }
                        else
                        {
                            // Handle case where the stored value is not an array
                            return defaultValue;
                        }
                    }
                    else if (typeof(T) == typeof(Dictionary<string, bool>))
                    {
                        if (settings.SettingValue != null)
                        {
                            // Parse the JSON string and convert it to a dictionary
                            Dictionary<string, bool> dictValue = JsonConvert.DeserializeObject<Dictionary<string, bool>>(settings.SettingValue.ToString());
                            return (T)Convert.ChangeType(dictValue, typeof(T));
                        }
                        else
                        {
                            // Handle case where the stored value is not a dictionary
                            return defaultValue;
                        }
                    }
                    else
                    {
                        // Handle other types
                        return (T)Convert.ChangeType(settings.SettingValue, typeof(T));
                    }
                }
            }

            // Return default value if settings not found
            return defaultValue;
        }
        public static void SUDeleteSetting(this IInlineInvokeProxy CPH, string settingName)
        {

            string program_Directory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(program_Directory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string file_Path = Path.Combine(dir, "Product_Settings.db");

            using (var db = new LiteDatabase($"Filename={file_Path}; Connection=shared"))
            {

                var col = db.GetCollection<SaveSettings>("settings");
                var Settings = col.FindOne(Query.EQ("SettingName", settingName));
                if (Settings != null)
                {
                    col.Delete(Settings.Id);
                }

            }


        }
    }

    #endregion

    public static class UIResources
    {
        public static bool closeLoadingWindow = false;
        public static int streamUpSettingsCount = 0;
        public static int streamUpSettingsProgress = 0;
        public static readonly string cyanStacked = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAAEosAABKLAAAAAAAAAAAAAAAAAAAvYb/AL2G/wC9hv8pvYb/ib2G/9G9hv/zvYb//L2G//e9hv/fvYb/pL2G/0S9hv8FvYb/AAAAAAC9hv8AvYb/AL2G/2a9hv+9vYb/ur2G/7q9hv+8vYb/V72G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/wC9hv8BvYb/U72G/9e9hv/+vYb//72G//+9hv//vYb//72G//+9hv//vYb/7L2G/3+9hv8LvYb/AL2G/wC9hv8AvYb/jL2G//+9hv//vYb//72G//+9hv93vYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAvYb/AL2G/0a9hv/ovYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/+r2G/3q9hv8CvYb/AL2G/wC9hv+MvYb//72G//+9hv//vYb//72G/3e9hv8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC9hv8UvYb/vr2G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/572G/zO9hv8AvYb/AL2G/4y9hv//vYb//72G//+9hv//vYb/d72G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/1m9hv/2vYb//72G//+9hv//vYb/+b2G/7a9hv+FvYb/nr2G/+u9hv//vYb//72G//+9hv//vYb/hr2G/wC9hv8AvYb/jL2G//+9hv//vYb//72G//+9hv93vYb/AL2G/wC9hv8NvYb/Eb2G/wO9hv8AvYb/AAAAAAAAAAAAvYb/or2G//+9hv//vYb//72G//+9hv+ovYb/Db2G/wC9hv8BvYb/Zb2G//m9hv//vYb//72G//+9hv/DvYb/DL2G/wC9hv+MvYb//72G//+9hv//vYb//72G/3e9hv8AvYb/AL2G/269hv/QvYb/pb2G/1q9hv8QvYb/AL2G/wC9hv/TvYb//72G//+9hv//vYb/+r2G/1G9hv8AvYb/AL2G/wC9hv8XvYb/072G//+9hv//vYb//72G/+K9hv8hvYb/AL2G/4y9hv//vYb//72G//+9hv//vYb/d72G/wC9hv8AvYb/f72G//+9hv//vYb/+r2G/7K9hv8kvYb/AL2G/+29hv//vYb//72G//+9hv/tvYb/ML2G/wAAAAAAvYb/AL2G/wW9hv+zvYb//72G//+9hv//vYb/8L2G/zO9hv8AvYb/jL2G//+9hv//vYb//72G//+9hv93vYb/AL2G/wC9hv9/vYb//72G//+9hv//vYb//72G/7G9hv8SvYb/+L2G//+9hv//vYb//72G/+e9hv8mvYb/AAAAAAC9hv8AvYb/Ar2G/6e9hv//vYb//72G//+9hv/1vYb/Pr2G/wC9hv+MvYb//72G//+9hv//vYb//72G/3e9hv8AvYb/AL2G/3+9hv//vYb//72G//+9hv//vYb/+L2G/2e9hv/6vYb//72G//+9hv//vYb/5r2G/yW9hv8AAAAAAL2G/wC9hv8BvYb/pb2G//+9hv//vYb//72G//a9hv9BvYb/AL2G/4y9hv//vYb//b2G/7u9hv+cvYb/Sb2G/wC9hv8AvYb/Tr2G/5y9hv++vYb//r2G//+9hv//vYb/ub2G//q9hv//vYb//72G//+9hv/mvYb/Jb2G/wAAAAAAvYb/AL2G/wG9hv+lvYb//72G//+9hv//vYb/9r2G/0G9hv8AvYb/jL2G//+9hv//vYb/mr2G/wW9hv8AAAAAAAAAAAC9hv8AvYb/B72G/6G9hv//vYb//72G//+9hv/mvYb/+r2G//+9hv//vYb//72G/+a9hv8lvYb/AAAAAAC9hv8AvYb/Ab2G/6W9hv//vYb//72G//+9hv/2vYb/Qb2G/wC9hv+MvYb//72G//+9hv/vvYb/Rr2G/wC9hv8AvYb/AL2G/wC9hv9MvYb/8r2G//+9hv//vYb//72G//i9hv/6vYb//72G//+9hv//vYb/5r2G/yW9hv8AAAAAAL2G/wC9hv8BvYb/pb2G//+9hv//vYb//72G//a9hv9BvYb/AL2G/4y9hv//vYb//72G//+9hv+2vYb/Dr2G/wC9hv8AvYb/Eb2G/7y9hv//vYb//72G//+9hv//vYb/+r2G//q9hv//vYb//72G//+9hv/mvYb/Jb2G/wAAAAAAvYb/AL2G/wG9hv+lvYb//72G//+9hv//vYb/9r2G/0G9hv8AvYb/jL2G//+9hv//vYb//72G//m9hv9hvYb/AL2G/wC9hv9ovYb/+72G//+9hv//vYb//72G//+9hv/pvYb/+r2G//+9hv//vYb//72G/+a9hv8lvYb/AAAAAAC9hv8AvYb/Ab2G/6W9hv//vYb//72G//+9hv/2vYb/Qb2G/wC9hv+MvYb//72G//+9hv//vYb//72G/8+9hv8avYb/H72G/9O9hv//vYb//72G//+9hv//vYb//72G/7e9hv/6vYb//72G//+9hv//vYb/5r2G/yW9hv8AAAAAAL2G/wC9hv8BvYb/pb2G//+9hv//vYb//72G//a9hv9BvYb/AL2G/4y9hv//vYb//72G//+9hv//vYb//72G/4C9hv+GvYb//72G//+9hv//vYb//72G//+9hv/0vYb/XL2G//q9hv//vYb//72G//+9hv/mvYb/Jb2G/wAAAAAAvYb/AL2G/wG9hv+lvYb//72G//+9hv//vYb/9r2G/0G9hv8AvYb/jL2G//+9hv//vYb//72G//+9hv//vYb/772G//C9hv//vYb//72G//+9hv//vYb//72G/6C9hv8LvYb/+r2G//+9hv//vYb//72G/+a9hv8lvYb/AAAAAAC9hv8AvYb/Ab2G/6W9hv//vYb//72G//+9hv/2vYb/Qb2G/wC9hv+MvYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//u9hv+ovYb/Gr2G/wC9hv/4vYb//b2G//29hv//vYb/5L2G/yW9hv8AAAAAAL2G/wC9hv8BvYb/pL2G//+9hv/9vYb//r2G//S9hv9AvYb/AL2G/4u9hv//vYb//b2G//29hv/9vYb//b2G//29hv/9vYb/+b2G/+u9hv/AvYb/ZL2G/w69hv8AvYb/AL2G/1S9hv9WvYb/Vr2G/1e9hv9OvYb/Db2G/wAAAAAAvYb/AL2G/wC9hv84vYb/V72G/1a9hv9WvYb/U72G/xa9hv8AvYb/L72G/1e9hv9WvYb/Vr2G/1a9hv9WvYb/Vr2G/1W9hv9KvYb/Lb2G/w29hv8AvYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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";
        public static readonly string midnightStacked = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAAEosAABKLAAAAAAAAAAAAAAAAAAAvYb/AL2G/wC9hv8pvYb/ib2G/9G9hv/yvYb//L2G//i9hv/fvYb/pL2G/0W9hv8FvYb/AAAAAAC9hv8AvYb/AL2G/2W9hv+9vYb/ur2G/7q9hv+8vYb/WL2G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/wC9hv8BvYb/Ur2G/9a9hv/+vYb//72G//+9hv//vYb//72G//+9hv//vYb/7L2G/4C9hv8LvYb/AL2G/wC9hv8AvYb/ir2G//+9hv//vYb//72G//+9hv95vYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAvYb/AL2G/0W9hv/nvYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/+r2G/3u9hv8CvYb/AL2G/wC9hv+KvYb//72G//+9hv//vYb//72G/3m9hv8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC9hv8TvYb/vb2G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/572G/zS9hv8AvYb/AL2G/4q9hv//vYb//72G//+9hv//vYb/eb2G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/1e9hv/1vYb//72G//+9hv//vYb/+b2G/7e9hv+FvYb/nr2G/+q9hv//vYb//72G//+9hv//vYb/iL2G/wC9hv8AvYb/ir2G//+9hv//vYb//72G//+9hv95vYb/AL2G/wC9hv8NvYb/Eb2G/wO9hv8AvYb/AAAAAAAAAAAAvYb/oL2G//+9hv//vYb//72G//+9hv+pvYb/Dr2G/wC9hv8BvYb/Y72G//i9hv//vYb//72G//+9hv/EvYb/Db2G/wC9hv+KvYb//72G//+9hv//vYb//72G/3m9hv8AvYb/AL2G/2y9hv/RvYb/pb2G/1u9hv8QvYb/AL2G/wC9hv/RvYb//72G//+9hv//vYb/+72G/1O9hv8AvYb/AL2G/wC9hv8WvYb/0r2G//+9hv//vYb//72G/+O9hv8jvYb/AL2G/4q9hv//vYb//72G//+9hv//vYb/eb2G/wC9hv8AvYb/fb2G//+9hv//vYb/+r2G/7O9hv8lvYb/AL2G/+u9hv//vYb//72G//+9hv/uvYb/Mr2G/wAAAAAAvYb/AL2G/wW9hv+yvYb//72G//+9hv//vYb/8L2G/zW9hv8AvYb/ir2G//+9hv//vYb//72G//+9hv95vYb/AL2G/wC9hv99vYb//72G//+9hv//vYb//72G/7K9hv8TvYb/9r2G//+9hv//vYb//72G/+i9hv8ovYb/AAAAAAC9hv8AvYb/Ab2G/6W9hv//vYb//72G//+9hv/2vYb/P72G/wC9hv+KvYb//72G//+9hv//vYb//72G/3m9hv8AvYb/AL2G/329hv//vYb//72G//+9hv//vYb/+L2G/2m9hv/5vYb//72G//+9hv//vYb/572G/ye9hv8AAAAAAL2G/wC9hv8BvYb/o72G//+9hv//vYb//72G//e9hv9CvYb/AL2G/4q9hv//vYb//b2G/7y9hv+cvYb/Sr2G/wC9hv8AvYb/Tb2G/5y9hv+9vYb//r2G//+9hv//vYb/u72G//m9hv//vYb//72G//+9hv/nvYb/J72G/wAAAAAAvYb/AL2G/wG9hv+jvYb//72G//+9hv//vYb/972G/0O9hv8AvYb/ir2G//+9hv//vYb/nL2G/wW9hv8AAAAAAAAAAAC9hv8AvYb/Br2G/5+9hv//vYb//72G//+9hv/nvYb/+b2G//+9hv//vYb//72G/+e9hv8nvYb/AAAAAAC9hv8AvYb/Ab2G/6O9hv//vYb//72G//+9hv/3vYb/Q72G/wC9hv+KvYb//72G//+9hv/wvYb/R72G/wC9hv8AvYb/AL2G/wC9hv9KvYb/8b2G//+9hv//vYb//72G//m9hv/5vYb//72G//+9hv//vYb/572G/ye9hv8AAAAAAL2G/wC9hv8BvYb/o72G//+9hv//vYb//72G//e9hv9DvYb/AL2G/4q9hv//vYb//72G//+9hv+4vYb/D72G/wC9hv8AvYb/EL2G/7u9hv//vYb//72G//+9hv//vYb/+72G//m9hv//vYb//72G//+9hv/nvYb/J72G/wAAAAAAvYb/AL2G/wG9hv+jvYb//72G//+9hv//vYb/972G/0O9hv8AvYb/ir2G//+9hv//vYb//72G//q9hv9jvYb/AL2G/wC9hv9mvYb/+r2G//+9hv//vYb//72G//+9hv/qvYb/+b2G//+9hv//vYb//72G/+e9hv8nvYb/AAAAAAC9hv8AvYb/Ab2G/6O9hv//vYb//72G//+9hv/3vYb/Q72G/wC9hv+KvYb//72G//+9hv//vYb//72G/9C9hv8cvYb/Hr2G/9K9hv//vYb//72G//+9hv//vYb//72G/7m9hv/5vYb//72G//+9hv//vYb/572G/ye9hv8AAAAAAL2G/wC9hv8BvYb/o72G//+9hv//vYb//72G//e9hv9DvYb/AL2G/4q9hv//vYb//72G//+9hv//vYb//72G/4G9hv+EvYb//72G//+9hv//vYb//72G//+9hv/1vYb/Xr2G//m9hv//vYb//72G//+9hv/nvYb/J72G/wAAAAAAvYb/AL2G/wG9hv+jvYb//72G//+9hv//vYb/972G/0O9hv8AvYb/ir2G//+9hv//vYb//72G//+9hv//vYb/772G//C9hv//vYb//72G//+9hv//vYb//72G/6G9hv8MvYb/+b2G//+9hv//vYb//72G/+e9hv8nvYb/AAAAAAC9hv8AvYb/Ab2G/6O9hv//vYb//72G//+9hv/3vYb/Q72G/wC9hv+KvYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//u9hv+pvYb/G72G/wC9hv/3vYb//b2G//29hv//vYb/5r2G/ye9hv8AAAAAAL2G/wC9hv8BvYb/ob2G//+9hv/9vYb//b2G//W9hv9CvYb/AL2G/4m9hv//vYb//b2G//29hv/9vYb//b2G//29hv/9vYb/+b2G/+u9hv/AvYb/Zb2G/w+9hv8AvYb/AL2G/1S9hv9WvYb/Vr2G/1e9hv9OvYb/Db2G/wAAAAAAvYb/AL2G/wC9hv83vYb/V72G/1a9hv9WvYb/U72G/xa9hv8AvYb/L72G/1e9hv9WvYb/Vr2G/1a9hv9WvYb/Vr2G/1W9hv9KvYb/Lr2G/w29hv8AvYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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";
        public static readonly string supSettingsLoadingBGString = "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";
        public static readonly string supIconString = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAABILAAASCwAAAAAAAAAAAAAAAAAAvYb/AL2G/wC9hv8lvYb/gr2G/8u9hv/tvYb/+L2G//O9hv/bvYb/oL2G/0K9hv8EvYb/AAAAAAC9hv8AvYb/AL2G/2u9hv+7vYb/uL2G/7i9hv+6vYb/Ur2G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/wC9hv8BvYb/TL2G/9K9hv/9vYb//72G//+9hv//vYb//72G//+9hv//vYb/6r2G/3y9hv8KvYb/AL2G/wC9hv8AvYb/lL2G//+9hv//vYb//72G//+9hv9yvYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAvYb/AL2G/0C9hv/kvYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/+b2G/3m9hv8CvYb/AL2G/wC9hv+UvYb//72G//+9hv//vYb//72G/3K9hv8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC9hv8QvYb/t72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb/5r2G/zS9hv8AvYb/AL2G/5S9hv//vYb//72G//+9hv//vYb/cr2G/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAL2G/1K9hv/zvYb//72G//+9hv//vYb/+72G/8G9hv+QvYb/qL2G/+69hv//vYb//72G//+9hv//vYb/h72G/wC9hv8AvYb/lL2G//+9hv//vYb//72G//+9hv9yvYb/AL2G/wC9hv8NvYb/D72G/wK9hv8AAAAAAAAAAAAAAAAAvYb/mr2G//+9hv//vYb//72G//+9hv+0vYb/FL2G/wC9hv8DvYb/a72G//q9hv//vYb//72G//+9hv/DvYb/Db2G/wC9hv+UvYb//72G//+9hv//vYb//72G/3K9hv8AvYb/AL2G/3a9hv/NvYb/n72G/1O9hv8NvYb/AL2G/wC9hv/LvYb//72G//+9hv//vYb//b2G/129hv8AvYb/AL2G/wC9hv8ZvYb/1r2G//+9hv//vYb//72G/+K9hv8ivYb/AL2G/5S9hv//vYb//72G//+9hv//vYb/cr2G/wC9hv8AvYb/i72G//+9hv//vYb/+L2G/6m9hv8fvYb/AL2G/+a9hv//vYb//72G//+9hv/yvYb/OL2G/wAAAAAAvYb/AL2G/wa9hv+1vYb//72G//+9hv//vYb/8L2G/zW9hv8AvYb/lL2G//+9hv//vYb//72G//+9hv9yvYb/AL2G/wC9hv+LvYb//72G//+9hv//vYb//72G/6a9hv8NvYb/8b2G//+9hv//vYb//72G/+y9hv8vvYb/AAAAAAC9hv8AvYb/Ar2G/6e9hv//vYb//72G//+9hv/1vYb/P72G/wC9hv+UvYb//72G//+9hv//vYb//72G/3K9hv8AvYb/AL2G/4u9hv//vYb//72G//+9hv//vYb/9b2G/1q9hv/xvYb//72G//+9hv//vYb/7L2G/y+9hv8AAAAAAL2G/wC9hv8CvYb/pr2G//+9hv//vYb//72G//W9hv9AvYb/AL2G/5S9hv//vYb//b2G/7+9hv+ivYb/SL2G/wC9hv8AvYb/Wb2G/6O9hv/HvYb//72G//+9hv//vYb/rL2G//G9hv//vYb//72G//+9hv/svYb/L72G/wAAAAAAvYb/AL2G/wK9hv+mvYb//72G//+9hv//vYb/9b2G/0C9hv8AvYb/lL2G//+9hv//vYb/l72G/wW9hv8AvYb/AL2G/wC9hv8AvYb/C72G/629hv//vYb//72G//+9hv/avYb/8b2G//+9hv//vYb//72G/+y9hv8vvYb/AAAAAAC9hv8AvYb/Ar2G/6a9hv//vYb//72G//+9hv/1vYb/QL2G/wC9hv+UvYb//72G//+9hv/tvYb/Qr2G/wC9hv8AvYb/AL2G/wC9hv9XvYb/9r2G//+9hv//vYb//72G/+69hv/xvYb//72G//+9hv//vYb/7L2G/y+9hv8AAAAAAL2G/wC9hv8CvYb/pr2G//+9hv//vYb//72G//W9hv9AvYb/AL2G/5S9hv//vYb//72G//+9hv+yvYb/Db2G/wC9hv8AvYb/F72G/8a9hv//vYb//72G//+9hv//vYb/8L2G//G9hv//vYb//72G//+9hv/svYb/L72G/wAAAAAAvYb/AL2G/wK9hv+mvYb//72G//+9hv//vYb/9b2G/0C9hv8AvYb/lL2G//+9hv//vYb//72G//i9hv9dvYb/AL2G/wC9hv9zvYb//b2G//+9hv//vYb//72G//+9hv/fvYb/8b2G//+9hv//vYb//72G/+y9hv8vvYb/AAAAAAC9hv8AvYb/Ar2G/6a9hv//vYb//72G//+9hv/1vYb/QL2G/wC9hv+UvYb//72G//+9hv//vYb//72G/8u9hv8YvYb/Jr2G/9u9hv//vYb//72G//+9hv//vYb//72G/629hv/xvYb//72G//+9hv//vYb/7L2G/y+9hv8AAAAAAL2G/wC9hv8CvYb/pr2G//+9hv//vYb//72G//W9hv9AvYb/AL2G/5S9hv//vYb//72G//+9hv//vYb//r2G/329hv+RvYb//72G//+9hv//vYb//72G//+9hv/yvYb/VL2G//G9hv//vYb//72G//+9hv/svYb/L72G/wAAAAAAvYb/AL2G/wK9hv+mvYb//72G//+9hv//vYb/9b2G/0C9hv8AvYb/lL2G//+9hv//vYb//72G//+9hv//vYb/8L2G//K9hv//vYb//72G//+9hv//vYb//72G/5u9hv8KvYb/8b2G//+9hv//vYb//72G/+y9hv8vvYb/AAAAAAC9hv8AvYb/Ar2G/6a9hv//vYb//72G//+9hv/1vYb/QL2G/wC9hv+UvYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//q9hv+mvYb/Gb2G/wC9hv/xvYb//72G//69hv//vYb/672G/y+9hv8AAAAAAL2G/wC9hv8CvYb/pr2G//+9hv/+vYb//72G//S9hv9AvYb/AL2G/5O9hv//vYb//r2G//69hv/+vYb//r2G//69hv/+vYb/+r2G/+29hv/DvYb/Z72G/w+9hv8AAAAAAL2G/1i9hv9dvYb/Xb2G/129hv9WvYb/Eb2G/wAAAAAAvYb/AL2G/wG9hv89vYb/X72G/129hv9dvYb/Wr2G/xe9hv8AvYb/Nr2G/1+9hv9dvYb/Xb2G/129hv9dvYb/Xb2G/1y9hv9PvYb/Mb2G/w69hv8AvYb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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";
        public static readonly string midnightWhite = "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";
        public static readonly string pinkMidnight = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD////////////////////////////////////////////////+/f//9Ov//+LK///Rq///xpb//8CL//+9hv//vYb//8CL///Glv//0av//+LJ///z6v///v3////////////////////////////////////////////////////////////////////////////////////////+/f//8OT//9e2///FlP//vof//7yF//+9hf//vYb//72G//+9hv//vYb//72F//+8hf//vof//8WU///Xtf//8OP///79////////////////////////////////////////////////////////////////////////+PL//9u9///Cj///vIT//72F//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYX//7yE///Cj///2rz///jy//////////////////////////////////////////////////////////////Hl///Mov//vYX//72F//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72F//+8hf//zKH///Dk///////////////////////////////////////////////////u3///xpf//7yE//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+8hP//xpb//+3e////////////////////////////////////////8eX//8aX//+8hP//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+8hf//xpb///Dk//////////////////////////////jy///Mov//vIT//72G//+9hv//vYb//72G//++h///vof//7uE+/+8hfz/vof//76H//+9hv//vYb//76H//++h///vof//72G//+9hv//vYb//72G//+9hv//vYb//72G//+8hP//zKH///jy///////////////////+/v//273//72F//+9hv//vYb//72G//+9hv//vof//6p12f+DUIr/cT9k/3E/Zf+FUo3/rXfd/76H//+6g/n/jVic/31JfP+OWZ//u4T6/72G//+9hv//vYb//72G//+9hv//vYb//72G//+8hf//2rv///7+//////////////Dk///Cj///vYX//72G//+9hv//vYb//76H//+octT/YTBE/00dG/9LHBj/SxwY/00dG/9kM0v/rHbd/7iC9f9lNE3/ShoV/2g2Uv+5gvb/vof//76H//+9hv//vYb//72G//+9hv//vYb//72F///Cj///8OP////////+/v//17f//7yE//+9hv//vYb//72G//+9hv//vIX9/3pHdv9NHRr/UiIl/3A+Yv9vPWD/USEk/00dG/+BToX/uIH0/2c1UP9MHBn/ajhV/7mC9v+zfOr/snzp/72G/f+9hv//vYb//72G//+9hv//vYb//7yE///Xtf///v3///Tr///Flf//vYX//72G//+9hv//vYb//76H//+0fez/YTBE/0wcGf9sOln/t4H0/7aA8f9pOFX/TBwZ/2g3Uv+yfOn/ZzZQ/0wcGf9qOFX/uYP4/4RQiv9fLkD/iVWU/7mC9f+9hv//vYb//72G//+9hv//vYX//8WU///z6v//48v//76H//+9hv//vYb//72G//+9hv//vof//6133/9aKTX/TB0a/3xJef++h///vYb//3pHdf9MHBn/YC9C/6133/9oNlH/TR0b/2s5V/+6g/j/gE2C/00dG/9QICH/kV2m/76H//+9hv//vYb//72G//+9hv//vof//+LJ///Srf//vIX//72G//+9hv//vYb//72G//++h///rHbc/1koM/9MHRr/fUp9/76H//+9hv//fEl5/0wcGf9fLj//rHbc/2c1T/9gL0H/nmnA/7yF/f+pc9b/bz1g/0wcGf9wPmP/u4T7/72G//+9hv//vYb//72G//+8hf//0av//8eX//+9hf//vYb//72G//+9hv//vYb//76H//+sdtz/WSgz/0wdGv99Sn3/vof//72G//97SXr/TBwZ/18uP/+sdtz/ZzZR/1MjJ/+cZ7v/wIn//6x33P9fLj//TR0a/2c2Uf+4gfT/vYb//72G//+9hv//vYb//72F///Glv//wIz//72G//+9hv//vYb//72G//+9hv//vof//6x23P9ZKDP/TB0a/31Kff++h///vYb//3tJev9MHBn/Xy4//6x23P9oNlH/TBwZ/2k4Vf+0fer/fUp8/04eHf9NHRv/bjxf/7qD+f+9hv//vYb//72G//+9hv//vYb//7+L//+9h///vYb//72G//+9hv//vYb//72G//++h///rHbc/1koM/9MHRr/fUp9/76H//+9hv//e0l6/0wcGf9fLj//rHbc/2g2Uf9NHhz/USEj/3VDbP9YJzD/Th4c/1AgIP+QXKL/vof//72G//+9hv//vYb//72G//+9hv//vYb//72H//+9hv//vYb//72G//+9hv//vYb//76H//+sdtz/WSgy/0wdGf99Sn3/vof//72G//97SXn/TBwY/18uPv+sdtz/aDZQ/04eG/9PHx7/Th4d/08fH/9XJzD/gk6G/7iB9P+9hv//vYb//72G//+9hv//vYb//72G//+9hv//wIz//72G//+9hv//vYb//72G//+9hv//vYb//7aA8f+WYa//kV2m/6RuzP+/iP//vof//6Ruy/+QW6P/mGO0/7iB8v+bZrn/kV2m/5Jep/+RXab/lmKw/6lz1v+8hfv/v4j//72G//+9hv//vYb//72G//+9hv//vYb//8CL///HmP//vYX//72G//+9hv//vYb//72G//+9hv//vIX9/5xovf+QXKT/tX7t/55ov/+aZrj/oGvE/7yF+f+ga8P/j1uh/6543/+VYa7/l2Oy/6Zxzv+kb8v/pG/L/6hz1f+ga8T/u4X8/72G//+9hv//vYb//72G//+9hf//xpb//9Kt//+8hf//vYb//72G//+9hv//vYb//76H//+veeP/dkRu/3ZEb/+MWJr/dUNs/5Neqf93RHD/rXfe/3NBaf+DT4j/hVKO/3A+Yv9wPmP/g1CK/35Lf/9/TIH/jFic/3lGdP+5g/j/vYb//72G//+9hv//vYb//7yF///Rq///48v//76I//+9hv//vYb//72G//+9hv//vYb//7mC9/+pc9b/eUZ0/5Bbov95RnP/o27I/3dFcf+rddf/azlX/31KfP+UYKz/iFST/2k4Vf+FUo3/f0yA/4BNgv+OWp3/eUd1/7mD+P+9hv//vYb//72G//+9hv//vof//+LK///06///xZX//7yF//+9hv//vYb//72G//+9hv//u4T7/4RQiv+ATYL/qnXZ/207Xf+WYrH/azlY/5Neqf9vPWH/cj9m/4ZSjv+IVJP/bz1g/4hUk/9yQGb/cT9l/3tIeP94RXL/uoP5/72G//+9hv//vYb//72F///FlP//9Or///7+///Yt///vIT//72G//+9hv//vYb//76H//+td9//cD5i/5plt/+SXqf/ZjVP/5JeqP+SXqj/lmKw/5djsv+JVZT/rXjf/5JeqP+NWZ7/qnTZ/45aoP+KVpf/jFic/5xnu/+9hv//vYb//72G//+9hv//vIT//9e2///+/v////////Hl///CkP//vYX//72G//+9hv//vYb//7V/7/95RnX/bz1g/59qwf+bZrn/tn/v/76H//++h///vof//76H//++h///vof//76H//++h///vof//76H//++h///vof//72G//+9hv//vYb//72F///Cj///8OT//////////////v7//9u+//+9hf//vYb//72G//+9hv//vYb//7V/7/+weuX/vIX9/76H//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYX//9u8///+/v//////////////////+PP//82j//+8hP//vYb//72G//+9hv//vYb//76H//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//7yE///Mov//+PL/////////////////////////////8eb//8eY//+8hP//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+8hP//xpf///Hl////////////////////////////////////////7uD//8eY//+8hP//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vIT//8aX///u3///////////////////////////////////////////////////8eb//82j//+9hf//vYX//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYX//72F///Mov//8eX/////////////////////////////////////////////////////////////+PP//9y+///CkP//vIT//72F//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYb//72G//+9hv//vYX//7yE///Cj///273///jy/////////////////////////////////////////////////////////////////////////v3///Hl///Yt///xZX//76I//+8hf//vYX//72G//+9hv//vYb//72G//+9hf//vIX//76H///Flf//17f///Hk///+/f////////////////////////////////////////////////////////////////////////////////////////79///07P//48z//9Kt///HmP//wIz//72H//+9h///wIz//8eX///Srf//48v///Tr///+/f//////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string whiteBlack = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////v7+//7+/v//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////9PT0/95eXn/T09P/1BQUP9+fn7/2dnZ///////4+Pj/jo6O/2pqav+RkZH/+fn5///////////////////////////////////////////////////////////////////////////////////////////////////////Ozs7/Kioq/wAAAP8AAAD/AAAA/wAAAP8yMjL/2NjY//T09P83Nzf/AAAA/z09Pf/19fX//////////////////////////////////////////////////////////////////////////////////////////////////f39/2NjY/8AAAD/BwcH/01NTf9KSkr/BgYG/wAAAP91dXX/8/Pz/zg4OP8AAAD/Pj4+//X19f/n5+f/5+fn//39/f/////////////////////////////////////////////////////////////////////////////////p6en/LCws/wAAAP9CQkL/8vLy/+/v7/89PT3/AAAA/zs7O//l5eX/ODg4/wAAAP8+Pj7/9/f3/3p6ev8lJSX/hYWF//T09P///////////////////////////////////////////////////////////////////////////9vb2/8aGhr/AAAA/2dnZ////////////2JiYv8AAAD/KCgo/9ra2v85OTn/AAAA/0BAQP/39/f/cHBw/wAAAP8FBQX/mpqa////////////////////////////////////////////////////////////////////////////19fX/xcXF/8AAAD/a2tr////////////Z2dn/wAAAP8kJCT/2NjY/zc3N/8mJib/uLi4//39/f/R0dH/SkpK/wAAAP9PT0//+vr6///////////////////////////////////////////////////////////////////////X19f/FxcX/wAAAP9ra2v///////////9nZ2f/AAAA/yQkJP/Y2Nj/ODg4/wkJCf+xsbH//////9fX1/8kJCT/AAAA/zk5Of/z8/P//////////////////////////////////////////////////////////////////////9fX1/8XFxf/AAAA/2tra////////////2dnZ/8AAAD/JCQk/9jY2P85OTn/AAAA/z4+Pv/n5+f/a2tr/wAAAP8AAAD/SkpK//j4+P//////////////////////////////////////////////////////////////////////19fX/xcXF/8AAAD/a2tr////////////Z2dn/wAAAP8kJCT/2NjY/zk5Of8AAAD/BgYG/1hYWP8VFRX/AAAA/wQEBP+VlZX////////////////////////////////////////////////////////////////////////////X19f/FxcX/wAAAP9ra2v///////////9nZ2f/AAAA/yQkJP/Y2Nj/OTk5/wAAAP8AAAD/AQEB/wEBAf8UFBT/dXV1//Ly8v///////////////////////////////////////////////////////////////////////////+/v7/+kpKT/mpqa/8XFxf///////////8PDw/+Wlpb/qamp//Dw8P+wsLD/mpqa/5ubm/+ampr/paWl/9HR0f/7+/v//////////////////////////////////////////////////////////////////////////////////Pz8/7Ozs/+YmJj/6urq/7a2tv+vr6//u7u7//j4+P+6urr/lJSU/9vb2/+jo6P/p6en/8fHx//ExMT/xMTE/9DQ0P+8vLz//Pz8///////////////////////////////////////////////////////////////////////f39//W1tb/1tbW/+MjIz/WVlZ/52dnf9eXl7/2dnZ/1RUVP93d3f/fn5+/05OTv9PT0//e3t7/25ubv9xcXH/j4+P/2JiYv/39/f///////////////////////////////////////////////////////////////////////b29v/R0dH/YmJi/5WVlf9gYGD/wcHB/11dXf/S0tL/QUFB/2pqav+hoaH/hISE/z4+Pv9+fn7/bm5u/3Fxcf+Pj4//YmJi//f39///////////////////////////////////////////////////////////////////////+vr6/3p6ev9xcXH/1NTU/0ZGRv+mpqb/QkJC/52dnf9MTEz/UlJS/39/f/+EhIT/S0tL/4WFhf9TU1P/UVFR/2ZmZv9fX1//+fn5///////////////////////////////////////////////////////////////////////b29v/T09P/62trf+bm5v/OTk5/52dnf+cnJz/paWl/6enp/+FhYX/29vb/5ycnP+RkZH/09PT/5OTk/+JiYn/jo6O/7Kysv///////////////////////////////////////////////////////////////////////////+3t7f9iYmL/SkpK/7m5uf+vr6//7e3t/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////+3t7f/i4uL//Pz8////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string creamMidnight = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD////////////////////////////////////////////////+////+f3//+77///l+f//3vf//9r2///Z9v//2fb//9r2///e9///5fn//+77///4/f///v/////////////////////////////////////////////////////////////////////////////////////////+////9/3//+j6///e9///2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//933///o+v//9v3///7/////////////////////////////////////////////////////////////////////////+/7//+r6///c9///2Pb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9j2///c9///6vr///v+//////////////////////////////////////////////////////////////f9///i+P//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//4fj///f9///////////////////////////////////////////////////1/f//3vf//9j2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Y9v//3vf///X9////////////////////////////////////////9/3//973///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//3vf///b9//////////////////////////////v+///i+P//2Pb//9n2///Z9v//2fb//9n2///Z9///2ff//9j0///Z9P//2ff//9n3///Z9v//2fb//9n3///Z9///2ff//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Y9v//4fj///v+////////////////////////6vr//9n2///Z9v//2fb//9n2///Z9v//2ff//9Tj///Ku///xqn//8ap///Lvf//1eX//9n3///Y8///zMT//8m0///Nxf//2PP//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//6vr///////////////////f9///c9///2fb//9n2///Z9v//2fb//9n3///U4f//wpj//72E//+8gv//vIL//72E///CnP//1eX//9jx///Dnf//vIH//8Sg///Y8f//2ff//9n3///Z9v//2fb//9n2///Z9v//2fb//9n2///c9///9v3/////////////6Pr//9j2///Z9v//2fb//9n2///Z9v//2fX//8ix//+9hP//von//8Wo///Fp///voj//72E///Kuf//2PH//8Oe//+8g///xKH//9jy///W6///1uv//9n2///Z9v//2fb//9n2///Z9v//2fb//9j2///o+v///v////n+///e9///2fb//9n2///Z9v//2fb//9n3///X7f//wpn//7yD///Eo///2PD//9fv///Eof//vIP//8Og///W6///w5///7yD///Eof//2PL//8q8///Blv//zMH//9jx///Z9v//2fb//9n2///Z9v//2fb//933///4/f//7/v//9n2///Z9v//2fb//9n2///Z9v//2ff//9Xm///Akf//vIP//8i0///Z9///2fb//8ix//+8g///wZf//9Xm///Dn///vIT//8Si///Y8v//ybf//72E//++h///zsr//9n3///Z9v//2fb//9n2///Z9v//2fb//+77///l+f//2fb//9n2///Z9v//2fb//9n2///Z9///1eT//8CQ//+8g///ybX//9n3///Z9///yLP//7yD///Blv//1eX//8Oe///Bl///0df//9n1///U4v//xab//7yD///GqP//2PT//9n2///Z9v//2fb//9n2///Z9v//5fn//973///Z9v//2fb//9n2///Z9v//2fb//9n3///V5P//wJD//7yD///Jtf//2ff//9n2///Is///vIP//8GW///V5f//w5///76K///Q1P//2vn//9Xl///Blv//vIT//8Of///Y8f//2fb//9n2///Z9v//2fb//9n2///e9///2/b//9n2///Z9v//2fb//9n2///Z9v//2ff//9Xk///AkP//vIP//8m1///Z9///2fb//8iz//+8g///wZb//9Xl///Dn///vIP//8Sh///W7P//ybX//72F//+9hP//xab//9jz///Z9v//2fb//9n2///Z9v//2fb//9r2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9///1eT//8CQ//+8g///ybX//9n3///Z9v//yLP//7yD///Blv//1eX//8Of//+9hP//voj//8et//+/j///vYX//72H///NyP//2ff//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n3///V5P//wJD//7yD///Jtf//2ff//9n2///Is///vIP//8GW///V5f//w57//72D//+9hf//vYX//72G//+/j///yrr//9jx///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2/b//9n2///Z9v//2fb//9n2///Z9v//2fb//9fv///R0P//0Mv//9Pd///b+f//2vj//9Td///OyP//0dL//9ny///R1f//z8v//9DM///Pyv//0M///9Xj///Z9v//2vn//9n2///Z9v//2fb//9n2///Z9v//2fb//9r2///f9///2fb//9n2///Z9v//2fb//9n2///Z9v//1/T8/6+1uf+fnaD/zeTr/7G4v/+tsrj/tL3C/9Xy+P+zu8D/n5yg/8XX3f+mp6v/qKuu/7rHzP+4xMn/uMTK/7/O1f+1vsT/1/P8/9n2///Z9v//2fb//9n2///Z9v//3vf//+X5///Z9v//2fb//9n2///Z9v//2fb//9r4///I2+P/gGtu/4Brb/+blZr/f2ls/6Sjqf+BbXD/xNbe/3xmaf+Pg4j/k4mO/3hfYv95YGP/kYWK/4p7f/+MfYH/nJec/4RxdP/V7/j/2fb//9n2///Z9v//2fb//9n2///l+f//7/v//9r2///Z9v//2fb//9n2///Z9v//2fb//9Tu9//Az9b/hHF0/6Cdov+EcXP/ucTI/4Jucf/C0tf/clVX/4l4fP+mp6z/lo6T/3BTVf+TiY3/i32A/42Agv+dmZ3/hHJ1/9Xv+P/Z9v//2fb//9n2///Z9v//2fb//+77///5/v//3vf//9n2///Z9v//2fb//9n2///Z9v//1/L7/5GGiv+MfoL/wtLZ/3VaXf+oq7H/clVY/6Sjqf94XmH/emNm/5OKjv+WjpP/d15g/5aOk/97ZGb/emJl/4Z1eP+CbnL/1vH5/9n2///Z9v//2fb//9n2///d9///+P3////////o+v//2Pb//9n2///Z9v//2fb//9r4///F19//eF9i/62xt/+joqf/bU1P/6SjqP+joqj/qKqw/6mssv+XkJT/xtjf/6SjqP+dmZ7/wdHZ/56boP+Zkpf/nJec/6+1u//Z9v//2fb//9n2///Z9v//2Pb//+j6//////////////f9///c9///2fb//9n2///Z9v//2vf//8/n7/+EcXX/d11g/7O7wf+us7n/0Ojv/9v5///a+P//2vj//9r4///a+P//2vj//9r4///a9///2/n//9v4///b+f//2vj//9n2///Z9v//2fb//9n2///c9///9v3//////////////////+v6///Z9v//2fb//9n2///Z9v//2ff//8/n7//J3eX/2PT9/9r3///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//+r6////////////////////////+/7//+L4///Y9v//2fb//9n2///Z9v//2vf//9r4///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9j2///i+P//+/7/////////////////////////////9/3//9/3///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//3vf///f9////////////////////////////////////////9f3//973///Y9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2Pb//973///1/f//////////////////////////////////////////////////9/3//+L4///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///i+P//9/3/////////////////////////////////////////////////////////////+/7//+v6///c9///2Pb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9j2///c9///6vr///v+//////////////////////////////////////////////////////////////////////////////f9///o+v//3vf//9r2///Z9v//2fb//9n2///Z9v//2fb//9n2///Z9v//2fb//9n2///e9///6Pr///f9///+//////////////////////////////////////////////////////////////////////////////////////////7////5/v//7/v//+X5///f9///2/b//9n2///Z9v//2/b//973///l+f//7/v///n+///+////////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string cyanMidnight = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD///////////////////////////////////////////////////3///3m///6vv//95j///Z+///0cP//9Gr///Rq///0cP//9X7///eX///6vf///eX////8//////////////////////////////////////////////////////////////////////////////////////////3///3e///4pf//9Xz///Rs///0af//9Gn///Rq///0av//9Gr///Rq///0af//9Gn///Rr///1e///+KT///3d/////f////////////////////////////////////////////////////////////////////////7v///5rf//9XX///Ro///0af//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rp///0aP//9XX///ms///+7//////////////////////////////////////////////////////////////93///94z///Rp///0af//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0af//9Gn///aL///93v///////////////////////////////////////////////////Nj///Z////0aP//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gj///V+///81/////////////////////////////////////////3f///2f///9Gj///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gj///Z+///93v/////////////////////////////+7///943///Ro///0av//9Gr///Rq///0av//9Wn///Vp//7ybP/+82z///Vp///1af//9Gr///Rq///1af//9Wn///Vp///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gj///aL///+7v////////////////////7///mu///0af//9Gr///Rq///0av//9Gr///Vp//Thg//curj/0ajR/9Ko0P/dvLb/9eSA///1af/98W7/4sOs/9m0wf/jxar//vJt///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gn///ms/////f/////////////93v//9Xb///Rp///0av//9Gr///Rq///1af/y34f/yJjn/7yE//+7gv//u4L//7yE///Km+L/9eOB//zvcP/LnN//uoH//8yf2//88HD///Vp///1af//9Gr///Rq///0av//9Gr///Rq///0af//9XX///zc//////////3///im///0aP//9Gr///Rq///0av//9Gr//vNr/9axxf+8hP//v4n7/9Gn0v/QptT/voj7/7yE///buLv//O9x/8ue3/+7g///zaHb//zwcP/56nj/+el4///0av//9Gn///Rq///0av//9Gr///Rq///0aP//+KT////9///95v//9Xz///Rp///0av//9Gr///Rq///1af/563f/yJjm/7uD///Oo9j//O5x//vtc//NoNv/u4P//8yf3P/46Xn/zJ7e/7uD///Nodv//fBv/927uP/Hlun/38Cx//zwcP//9Gr///Rq///0av//9Gr///Rp///1e////eX///q////0bP//9Gr///Rq///0av//9Gr///Vp//bkf//EkfD/u4P//9izwv//9Wn///Rq/9axxf+7g///x5fo//Xkf//Mn97/vIT//86i2f/98W//2re9/7yE//++h/z/5cil///1aP//9Gr///Rq///0av//9Gr///Rr///6vf//+Jn///Rp///0av//9Gr///Rq///0av//9Wn/9eOB/8OQ8v+7g///2bTA///1af//9Gn/2LPD/7uD///Glur/9eOB/8ue3//Hl+n/7dWU//7za//z4IX/0KbU/7uD///Rp9H//vJt///0av//9Gr///Rq///0av//9Gn///eX///2gP//9Gn///Rq///0av//9Gr///Rq///1af/144H/w5Dy/7uD///ZtMD///Vp///0af/YssP/u4P//8aW6v/144H/zJ7e/7+K+v/r05f///dm//XjgP/Glur/vIT//8ye3f/873H///Rq///0av//9Gr///Rq///0af//9X7///Vx///0av//9Gr///Rq///0av//9Gr///Vp//Xjgf/DkPL/u4P//9m0wP//9Wn///Rp/9iyw/+7g///xpbq//Xjgf/Mn97/u4P//82g2//563f/2bTA/7yF//+8hP//0KXU//3xbv//9Gr///Rq///0av//9Gr///Rq///0cP//9Gv///Rq///0av//9Gr///Rq///0av//9Wn/9eOB/8OQ8v+7g///2bTA///1af//9Gn/2LLD/7uD///Glur/9eOB/8yf3v+8hP//voj8/9Ssy//Cj/P/vIX//72H/f/kx6j///Vo///0av//9Gr///Rq///0av//9Gr///Rq///0a///9Gr///Rq///0av//9Gr///Rq///1af/144H/w5Dy/7uD///ZtMD///Vp///0af/YssP/u4P//8aW6v/144H/zJ/e/7yF//+9hv//vYX+/72G/v/CjvP/27m6//zvcf//9Gn///Rq///0av//9Gr///Rq///0av//9Gr///Vx///0av//9Gr///Rq///0av//9Gr///Rp//vtc//pzqH/5sqo//Dbjf//92r///Zq//Dcjv/kx6j/6tCe//vwdP/r05n/5sqo/+fKqP/myaf/6c6h//Thh//+9G3///dq///0av//9Gr///Rq///0av//9Gr///Rq///0cP//9oD///Rp///0av//9Gr///Rq///0av//9Gr//fJp/8m0UP+2nEj/8ONi/823VP/HsVL/0LtU//rwZf/OulP/tZpK/+XWXf++pkv/walM/9jFVv/VwlX/1sNX/97MXP/RvFb//fFp///0av//9Gr///Rq///0av//9Gn///V+///4mv//9Gn///Rq///0av//9Gr///Rq///2a//p2mH/jWs6/45rOv+wlEj/jGk5/7uiTf+PbDr/5dRf/4llOP+hg0L/pohE/4NeNf+FYDb/o4VD/5p6P/+dfED/sZZJ/5JwO//57Wj///Rq///0av//9Gr///Rq///0af//95j///q////0bP//9Gr///Rq///0av//9Gr///Rq//nsZ//fzVz/knA8/7acS/+RcTz/1MNZ/49tO//g0F3/e1Uy/5h4Pv++pk7/qo1G/3lSMf+miET/m3xA/51/Qf+ymEr/k3E8//ntaP//9Gr///Rq///0av//9Gr///Rs///6vf///ef///V9///0af//9Gr///Rq///0av//9Gr//PBp/6OFQ/+dfUD/4dBd/39ZNP/BqVD/fFUy/7uiTf+DXjX/hmI3/6aJRP+qjUb/gl01/6qNRv+HYzf/hmI3/5V0Pf+Qbjv/++9o///0av//9Gr///Rq///0af//9Xv///3l/////v//+af///Ro///0av//9Gr///Rq///2a//m1l//g141/8awUv+6oU3/dUwv/7uiTf+7oU3/wahP/8KrUP+rj0b/5tZf/7uiTf+zmEn/4dBd/7SZSv+tkUf/sZZJ/8q0U///9Gr///Rq///0av//9Gr///Ro///4pf////3////////93///9Xb///Rp///0av//9Gr///Vq//LlZf+ScTz/gl01/866Vv/IslP/8+Zl///3a///9mv///Zr///2a///9mr///Zr///2a///9Wr///dr///2a///9mv///Zr///0av//9Gr///Rq///0af//9XX///3d///////////////+///5r///9Gn///Rq///0av//9Gr///Vq//LlZf/r3GH//fJp///1a///9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rp///5rf////3///////////////////7w///3jv//9Gj///Rq///0av//9Gr///Vq///2a///9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0aP//94z///7v//////////////////////////////3g///2gP//9Gj///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gj///Z////93/////////////////////////////////////////zZ///2gP//9Gj///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Ro///2f////Nj///////////////////////////////////////////////////3g///3jv//9Gn///Rp///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rp///0af//943///3f//////////////////////////////////////////////////////////////7w///5r///9Xb///Ro///0af//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rq///0av//9Gr///Rp///0aP//9Xb///mu///+7//////////////////////////////////////////////////////////////////////////9///93///+Kf///V9///0bP//9Gn///Rp///0av//9Gr///Rq///0av//9Gn///Rp///0bP//9Xz///im///93v////3//////////////////////////////////////////////////////////////////////////////////////////f///ef///rA///4mv//9oD///Vx///0a///9Gv///Vx///2f///95n///q////95v////3/////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string greyMidnight = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD//////////////////////////////////////////////////////Pz8//f39//y8vL/7+/v/+3t7f/s7Oz/7Ozs/+3t7f/v7+//8vLy//f39//8/Pz/////////////////////////////////////////////////////////////////////////////////////////////////+/v7//T09P/u7u7/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+7u7v/z8/P/+/v7/////////////////////////////////////////////////////////////////////////////f39//X19f/t7e3/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/t7e3/9PT0//39/f////////////////////////////////////////////////////////////v7+//w8PD/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/8PDw//v7+//////////////////////////////////////////////////6+vr/7+/v/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7+/v//r6+v//////////////////////////////////////+/v7/+/v7//s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7+/v//v7+/////////////////////////////39/f/w8PD/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7ez/7O3s/+vq7P/r6+z/7e3s/+zt7P/s7Oz/7Ozs/+zt7P/s7ez/7O3s/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/8PDw//39/f//////////////////////9fX1/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7O3s/+Tb7//Tt/b/zKX5/8ym+f/UuPb/5d3v/+zt7P/r6e3/17/1/9Cw9//YwPT/6+rs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/9PT0//////////////////v7+//u7u7/7Ozs/+zs7P/s7Oz/7Ozs/+zt7P/j2PD/xZf8/7yE//+7g///u4P//7yE///Gmvv/5d3v/+ro7f/Gm/v/u4H//8id+v/q6O3/7O3s/+zt7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/t7e3/+/v7////////////9PT0/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ovs/8+u+P+8hP//von+/8ul+f/LpPn/voj//7yE///Stfb/6uft/8ec+/+8g///yJ/6/+ro7f/o4u7/6OLu/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/z8/P///////z8/P/u7u7/7Ozs/+zs7P/s7Oz/7Ozs/+zt7P/o4+7/xZf8/7yD///JoPr/6uft/+nm7f/In/r/vIP//8id+//n4u7/x537/7yD///In/r/6unt/9S39v/Elfz/1rv1/+ro7f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+7u7v/8/Pz/9/f3/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7O3s/+Xd7//CkP3/vIT//9Cv9//s7ez/7Ozs/8+u+P+8g///xJb8/+Xd7//Infv/vIT//8mg+v/r6e3/0rP3/7yE//+9h///2cTz/+3t7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs//f39//y8vL/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7ez/5dzv/8GP/f+8hP//0bH3/+zt7P/s7Oz/0K/3/7yD///Elfz/5dzv/8ec+//Elfz/38/x/+zr7P/j2e//y6P5/7yD///Lpfn/6+rs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/8vLy/+/v7//s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zt7P/l3O//wY/9/7yE///Rsff/7O3s/+zs7P/Qr/f/vIP//8SV/P/l3O//x537/7+K/v/ezfL/7e/s/+Xd7//Elfz/vIT//8ed+//q5+3/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/v7+//7e3t/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7O3s/+Xc7//Bj/3/vIT//9Gx9//s7ez/7Ozs/9Cv9/+8g///xJX8/+Xc7//Infv/vIP//8ie+v/o4+7/0bH3/7yF//+8hP//yqP5/+vp7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+3t7f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7ez/5dzv/8GP/f+8hP//0bH3/+zt7P/s7Oz/0K/3/7yD///Elfz/5dzv/8id+/+8hf//voj//82p+P/Bjv3/vYX//72H///ZwvT/7O3s/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zt7P/l3O//wY/9/7yE///Rsff/7O3s/+zs7P/Qr/f/vIP//8SV/P/l3O//yJ37/7yF//+9hv//vYX//72G///Bjv7/07X2/+rn7f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7e3t/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+nm7f/dyfX/28X3/+LV8f/u7+7/7u7u/+PV8//ZwvT/3sv1/+vp8P/ezvT/28X3/9vG9//axPb/3cn1/+Tb8v/t7O7/7u7u/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+3t7f/v7+//7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/6urp/7yurP+rl5X/39va/7+xsf+7q6v/wrW0/+jo5v/BtLL/qpaV/9XPzf+yoZ//tKSi/8m/vf/HvLr/x727/8/Gxf/Dtrb/6unp/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7+/v//Ly8v/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+3u7v/Z09P/h2ho/4doaP+lkJD/hWZm/6+dnf+IaWn/1c3N/4NiYv+Yf3//nYSE/35cXP9/XV3/moGB/5J3d/+UeXn/p5KS/4ttbf/n5eX/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/y8vL/9/f3/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+bl5f/Qx8f/i21t/6uXl/+Lbm7/yL29/4lqav/Sysr/d1NT/5B0dP+yoKD/oImJ/3VQUP+chIT/k3l5/5Z7e/+olJT/jG5u/+fl5f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs//f39//8/Pz/7u7u/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/6ejo/5qBgf+Uenr/0cnJ/3pXV/+1pKT/d1NT/7Cdnf99W1v/gGBg/52Fhf+giYn/fVtb/6CJif+BYGD/gF9f/45xcf+Ja2v/6Ofn/+zs7P/s7Oz/7Ozs/+zs7P/u7u7//Pz8///////09PT/7Ozs/+zs7P/s7Oz/7Ozs/+3u7v/Wz8//flxc/7qqqv+vnJz/cEtL/6+dnf+vnJz/tKOj/7alpf+hior/1s/P/6+dnf+ok5P/0cnJ/6qVlf+jjY3/p5GR/72urv/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs//Pz8/////////////v7+//u7u7/7Ozs/+zs7P/s7Oz/7e3t/+Hd3f+LbW3/fVpa/8G0tP+8ra3/4t/f/+7v7//t7u7/7u7u/+7u7v/t7e3/7u7u/+7u7v/t7e3/7u/v/+7u7v/u7u7/7e7u/+zs7P/s7Oz/7Ozs/+zs7P/u7u7/+/v7//////////////////X19f/s7Oz/7Ozs/+zs7P/s7Oz/7O3t/+He3v/a1NT/6+rq/+3t7f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs//X19f///////////////////////f39//Hx8f/s7Oz/7Ozs/+zs7P/s7Oz/7e3t/+3t7f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/w8PD//f39////////////////////////////+/v7/+/v7//s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7+/v//v7+///////////////////////////////////////+vr6/+/v7//s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+/v7//6+vr/////////////////////////////////////////////////+/v7//Hx8f/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/w8PD/+/v7/////////////////////////////////////////////////////////////f39//X19f/u7u7/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/u7u7/9fX1//39/f////////////////////////////////////////////////////////////////////////////v7+//09PT/7u7u/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/s7Oz/7Ozs/+zs7P/u7u7/9PT0//v7+//////////////////////////////////////////////////////////////////////////////////////////////////8/Pz/9/f3//Ly8v/v7+//7e3t/+zs7P/s7Oz/7e3t/+/v7//y8vL/9/f3//z8/P//////////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string midnightCyan = "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIAAAAAAAABAAACMuAAAjLgAAAAAAAAAAAAD////////////////////////////////////////////////8+/v/4dnZ/7Kdnf+FZGT/Zz09/1YoKP9PHx//Tx8f/1YnJ/9mPT3/hWNj/7GcnP/g2Nj//Pv7///////////////////////////////////////////////////////////////////////////////////////8/Pz/2M3N/5V4eP9kOTn/USEh/00dHf9OHh7/Tx8f/08fH/9PHx//Tx8f/04eHv9NHR3/USEh/2M5Of+Udnb/18zM//z7+///////////////////////////////////////////////////////////////////////7Ofn/56EhP9cMDD/TBwc/04dHf9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Th0d/0wcHP9cLy//nYKC/+vm5v///////////////////////////////////////////////////////////9nPz/94U1P/Th0d/04eHv9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/04eHv9OHR3/dlFR/9jNzf/////////////////////////////////////////////////RxMT/aD8//00cHP9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9NHBz/Zz09/8/Cwv//////////////////////////////////////2c/P/2g/P/9NHBz/Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9NHBz/Zz09/9fNzf///////////////////////////+zn5/94U1P/TRwc/08fH/9PHx//Tx8f/08fH/9OHh3/Th4d/1EhI/9QICL/Th4d/04eHf9PHx//Tx8f/04eHf9OHh3/Th4d/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9NHBz/dlFR/+vm5v/////////////////9/f3/n4WF/04dHf9PHx//Tx8f/08fH/9PHx//Th4e/2IwRf+JVZT/m2a6/5tmuf+HU5H/Xy5A/04eHf9SIiX/f02C/49cov9+TH//USEk/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9OHR3/nYKC//38/P///////////9jNzf9dMDD/Th4e/08fH/9PHx//Tx8f/04eHf9kM0r/q3Xa/7+I///Bif//wYn//7+I//+octP/YC9B/1QjKP+ncc7/wov//6Rvyf9TIyj/Th4d/04eHv9PHx//Tx8f/08fH/9PHx//Tx8f/04eHv9cLy//1svL///////9/f3/lnl5/0wcHP9PHx//Tx8f/08fH/9PHx//UCAh/5JeqP+/iP//uoP5/5xnvP+daL7/u4T6/7+I//+LV5j/VCQq/6Vwzv/Aif//om3J/1MjKP9ZKTT/Wik1/08fH/9PHx7/Tx8f/08fH/9PHx//Tx8f/0wcHP+Tdnb//fz8/+La2v9kOjr/Th0d/08fH/9PHx//Tx8f/04eHv9YKDL/q3XZ/8CJ//+ga8X/VSQq/1YlLf+jbcn/wIn//6Ruy/9aKTX/pW/O/8CJ//+ibcn/UyIm/4hVlP+td97/g1CK/1MjKP9PHx7/Tx8f/08fH/9PHx//Th0d/2M5Of/g19f/s5+f/1EiIv9PHx//Tx8f/08fH/9PHx//Th4d/18uP/+yfOj/wIj//5BcpP9OHh7/Tx8e/5JeqP/Aif//rHbc/18uP/+kb83/v4j//6Fsx/9SIib/jFid/7+I//+8hfr/e0h4/04eHP9PHx//Tx8f/08fH/9PHx//USEh/7GcnP+HZmb/TR0d/08fH/9PHx//Tx8f/08fH/9OHh3/YC9C/7N96//AiP//j1uh/04eHf9PHx7/kFyk/8CJ//+td9//YC9C/6Vwz/+sdt3/bjxe/1AgIf9jMkj/nWi+/8CJ//+cZ7r/USEj/08fH/9PHx//Tx8f/08fH/9NHR3/hGNj/2g/P/9OHR3/Tx8f/08fH/9PHx//Tx8f/04eHf9gL0L/s33r/8CI//+PW6H/Th4e/08fHv+RXKT/wIn//6133/9gL0L/pW/N/7mC9/9wPmL/TBwZ/2AuQf+td9//v4j//6Vvzf9UJCr/Tx8e/08fH/9PHx//Tx8f/04eHv9mPT3/Vyoq/08eHv9PHx//Tx8f/08fH/9PHx//Th4d/2AvQv+zfev/wIj//49bof9OHh7/Tx8e/5FcpP/Aif//rXff/2AvQv+kb83/wIn//6NtyP9YKDP/j1uh/76H//+/iP//nmm+/1IiJf9PHx//Tx8f/08fH/9PHx//Tx8f/1YoKP9QISH/Tx8f/08fH/9PHx//Tx8f/08fH/9OHh3/YC9C/7N96//AiP//j1uh/04eHv9PHx7/kVyk/8CJ//+td9//YC9C/6Rvzf+/h///u4T6/5disf+0fu3/vof//7yF/P98SXz/Th4d/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/1AhIf9PHx//Tx8f/08fH/9PHx//Tx8f/04eHf9gL0L/s33r/8CI//+PW6H/Th4e/08fHv+RXKX/wIn//6133/9gL0L/pG/N/76H//+9hv//vof+/72G/v+1fu7/ileY/1QkKv9PHx7/Tx8f/08fH/9PHx//Tx8f/08fH/9PICD/WCoq/08eHv9PHx//Tx8f/08fH/9PHx//Tx8e/1YlLf91QnD/ekZ6/2g2U/9NHB7/TR0e/2c1VP98Snv/c0Bs/1MiLP9wPmX/ekd6/3lGef96R3n/dUJv/2IxSf9PHyL/TRwd/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/1YoKP9pQED/Th0d/08fH/9PHx//Tx8f/08fH/9PHx//USEf/4RgM/+Xdzv/XTAj/4FcNf+HYjf/flgx/1IjHf9/WTH/mXk+/2g9J/+PbTf/jWo2/3VOLP94US3/eFAu/3BHLf99VzP/USIg/08fH/9PHx//Tx8f/08fH/9OHh7/Zz09/4hnZ/9NHR3/Tx8f/08fH/9PHx//Tx8f/04dHv9lOSj/wahP/8CoT/+ef0H/wqpQ/5JxPP++p0//aT8q/8WuUf+tkEf/qItF/8m1VP/Js1P/qo5G/7OZSv+xl0n/nH1A/7ujTv9VJiH/Tx8f/08fH/9PHx//Tx8f/00dHf+FZGT/tKCg/1EiIv9PHx//Tx8f/08fH/9PHx//Tx8f/1UnIv9vRi3/vKNN/5d3Pv+7ok3/eFAw/76mTv9sQyz/0r5X/7abS/+QbTv/pIZD/9TBWP+oi0X/spdJ/7CUSP+bez//u6JN/1UmIf9PHx//Tx8f/08fH/9PHx//USEh/7KcnP/i2tr/ZTs7/04dHf9PHx//Tx8f/08fH/9PHx//UiMg/6uORv+xlkn/bUMs/8+6Vf+Majn/0b5X/5NxPP/LtVT/xrFS/6eKRf+khkP/y7ZU/6OGQ//GsFL/x7FS/7ifTP+9pU7/UyQh/08fH/9PHx//Tx8f/04dHf9kOTn/4NjY//39/f+Wenr/TBwc/08fH/9PHx//Tx8f/04dHv9oPSr/ybVU/4djN/+Ucjz/2Mda/5NxPP+Tcjz/jWs6/4xoOf+jhEP/aD0q/5NxPP+be0D/bUMs/5p6P/+hgkL/nX1A/4RfNv9PHx//Tx8f/08fH/9PHx//TBwc/5R3d//9/Pz//////9nOzv9dMTH/Th4e/08fH/9PHx//Th4f/1wuJP+8ok3/zLZU/39ZM/+FYTb/Wi0k/0wcHv9OHR7/TR0e/00dHv9OHR//TR0e/00dHv9OHh//TRwe/00dHv9NHR7/Th0e/08fH/9PHx//Tx8f/04eHv9cMDD/18zM/////////////f39/6CGhv9OHh7/Tx8f/08fH/9PHx//Tx4f/1suJP9jNyj/UCEg/04eHv9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Th0d/56EhP/9/f3/////////////////7ejo/3lVVf9MHBz/Tx8f/08fH/9PHx//Th4f/04dHv9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/00cHP93UlL/7Ofn////////////////////////////2tDQ/2lAQP9NHBz/Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9NHBz/aD4+/9nOzv//////////////////////////////////////0sbG/2lAQP9NHBz/Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//TRwc/2g+Pv/RxMT/////////////////////////////////////////////////2tDQ/3lVVf9OHh7/Th4e/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Th4e/04dHf94U1P/2c/P////////////////////////////////////////////////////////////7enp/6GHh/9dMTH/TBwc/04dHf9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Tx8f/08fH/9PHx//Th0d/0wcHP9dMDD/n4WF/+3o6P///////////////////////////////////////////////////////////////////////fz8/9nPz/+Xenr/ZTs7/1EiIv9NHR3/Th0d/08eHv9PHx//Tx8f/08eHv9OHR3/TR0d/1EiIv9kOjr/lnl5/9nOzv/9/Pz///////////////////////////////////////////////////////////////////////////////////////z8/P/j29v/taCg/4hnZ/9pQED/WCoq/1AhIf9QISH/Vyoq/2g/P/+HZmb/tJ+f/+La2v/8+/v/////////////////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public static readonly string midnightWhiteSplash = "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";
        public static readonly string midnightCyanSplash = "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";
        public static readonly string creamMidnightSplash = "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";
        public static readonly string cyanMidnightSplash = "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";
        public static readonly string whiteBlackSplash = "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";
        public static readonly string pinkMidnightSplash = "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";
        public static readonly string grayMidightSplash = "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";

    }


}
