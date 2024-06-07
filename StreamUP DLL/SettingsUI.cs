using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LiteDB;
using System.Windows;
using Streamer.bot.Plugin.Interface;
using System.Net;

namespace StreamUP
{
    #region CustomElements
    public class CustomNumericUpDown : NumericUpDown
    {
        private const int EM_SETMARGINS = 0xD3;

        public CustomNumericUpDown()
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
            const int margin = 5; // Adjust this value as needed for the desired indentation
            SendMessage(this.Controls[1].Handle, EM_SETMARGINS, 1, (IntPtr)((margin << 16) | margin));
        }


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
    }

    public class CustomTextBox : TextBox
    {
        private const int EM_SETMARGINS = 0xD3;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetTextBoxMargins();
        }

        private void SetTextBoxMargins()
        {
            const int margin = 5; // Adjust this value as needed for the desired indentation
            SendMessage(this.Handle, EM_SETMARGINS, 1, (IntPtr)((margin << 16) | margin));
            SendMessage(this.Handle, EM_SETMARGINS, 3, (IntPtr)((margin << 16) | margin));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
    }

    public class BorderlessTabControl : TabControl
    {
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
            }
        }




        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }
    }

    public partial class CustomSplashScreen : Form
    {
        public CustomSplashScreen()
        {



            Label loadingLabel = new Label
            {
                Text = "Loading Settings UI...",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                Dock = DockStyle.Bottom,
                Height = 30
            };

            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            //this.BackgroundImage = SettingsUI.Properties.Resources.Splash;
            this.Controls.Add(loadingLabel);




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
        public static void RunActionButton(this IInlineInvokeProxy CPH, string actionName, bool runImmediately)
        {
            CPH.RunAction(actionName, runImmediately);
        }
    }

    public static class SettingsBuilder
    {
        public static TableLayoutPanel settingsTable;
        private static FlowLayoutPanel buttonPanel;
        private static Button saveButton;
        private static Button resetButton;
        //private static SplashScreen splashScreen;

        // List to hold controls
        private static readonly List<Control> controls = new List<Control>();
        public static void BuildForm(this IInlineInvokeProxy CPH, string title, List<Control> layout, string imageFilePath = null)
        {

            //CustomSplashScreen splashScreen = new CustomSplashScreen();
           // splashScreen.Show();


            Thread.Sleep(3000);

            //splashScreen.Close();

            CPH.CreateMainForm(title, layout, imageFilePath);


        }
        public static bool CreateMainForm(this IInlineInvokeProxy CPH, string title, List<Control> layout, string imageFilePath = null)
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

            TabControl tabControl = new BorderlessTabControl
            {
                Dock = DockStyle.Fill,
                //BackColor = ColorTranslator.FromHtml("#1F1F23"),
                //ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),


            };


            // Create a form
            var form = new Form
            {
                Text = title,
                Width = 600,
                Height = 800,
                FormBorderStyle = FormBorderStyle.Sizable,
                Icon = GetIconOrDefault(imageFilePath),
                BackColor = ColorTranslator.FromHtml("#18181B")
            };

            // Create and add tab pages
            foreach (var tab in tabControls)
            {
                TabPage tabPage = new TabPage(tab.Key)
                {
                    BackColor = ColorTranslator.FromHtml("#18181B")
                };

                TableLayoutPanel tableLayout = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    GrowStyle = TableLayoutPanelGrowStyle.AddRows,
                    AutoScroll = true
                };

                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

                foreach (Control control in tab.Value)
                {
                    int rowIndex = tableLayout.RowCount++;
                    tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    tableLayout.Controls.Add(control, 0, rowIndex);
                    tableLayout.SetColumnSpan(control, 2);
                }

                tabPage.Controls.Add(tableLayout);
                tabControl.TabPages.Add(tabPage);
            }

            // Create "About" tab page
           // TabPage aboutTabPage = new TabPage("About")
            //{
           //     BackColor = ColorTranslator.FromHtml("#18181B")
          //  };

            // Add your specific controls to the About tab
           // aboutTabPage.Controls.Add(CreateAboutPanel());

           // tabControl.TabPages.Add(aboutTabPage);

            buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5),
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,

                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular)
            };
            saveButton = new Button
            {
                Text = "💾 Save",
                Padding = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
            };

            saveButton.Click += (sender, e) => CPH.SaveButton_Click(sender, e, layout);

            resetButton = new Button
            {
                Text = "🔄 Reset",
                Padding = new Padding(10),
                AutoSize = true,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.WhiteSmoke,
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular)
            };
            resetButton.Click += (sender, e) => CPH.ResetButton_Click(sender, e, layout, form);

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
            form.ShowDialog();



            return true;
        }
        private static Panel CreateAboutPanel()
        {
            Panel aboutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#18181B"),
                AutoScroll = true
            };

            FlowLayoutPanel flowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                AutoSize = true,
                WrapContents = false
            };

            Label outtro = new Label
            {
                Text = "This UI Settings is brought to you by TerrierDarts. \n You can support TerrierDarts through the following methods:",
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Italic)
            };

            Label creditsHeading = new Label
            {
                Text = "Special Thanks",
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 18.0F, FontStyle.Underline)
            };

            Label creditsContent = new Label
            {
                Text = "This UI would not be possibile with out the help and support of the following people. \n\n ConfuzzedCat, BitGamey, GoWMan, Web_Mage, tawmae, Mustached_Maniac, Rondhi, Lyfesaver74 and Geocym. ",
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Regular),
                Padding = new Padding(0, 0, 0, 25)
            };

            Label disclaimer = new Label
            {
                Text = "You are welcome and encouraged to distrubute this as part of your own extensions even if paid. However please do not edit or copy any of the source code and please do not sell this as a standalone product. \n If you have bought this please reach out to TerrierDarts on Twitter",
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.Bottom,
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font(FontFamily.GenericSansSerif, 8.0F, FontStyle.Regular)
            };

            LinkLabel forthwall = CreateLinkLabel("FourthWall", "https://streamware.co.uk", "fourthwall");
            LinkLabel kofi = CreateLinkLabel("Ko-fi", "https://www.ko-fi.com/TerrierDarts", "kofi");
            LinkLabel patreon = CreateLinkLabel("Patreon", "https://www.patreon.com/TerrierDarts", "patreon");
            LinkLabel youtube = CreateLinkLabel("YouTube", "https://youtube.com/@TerrierDarts", "youtube");
            LinkLabel twitch = CreateLinkLabel("Twitch", "https://twitch.tv/TerrierDarts", "twitch");
            //LinkLabel doras = CreateLinkLabel("Social Links", "https://doras.to/terrierdarts");
            //LinkLabel itch = CreateLinkLabel("Itch.io - Get it here", "https://terrierdarts.itch.io/", "itch");

            flowLayout.Controls.Add(outtro);
            //flowLayout.Controls.Add(itch);
            flowLayout.Controls.Add(forthwall);
            flowLayout.Controls.Add(kofi);
            flowLayout.Controls.Add(patreon);
            flowLayout.Controls.Add(youtube);
            flowLayout.Controls.Add(twitch);
            //flowLayout.Controls.Add(doras);
            flowLayout.Controls.Add(creditsHeading);
            flowLayout.Controls.Add(creditsContent);
            //flowLayout.Controls.Add(disclaimer);


            aboutPanel.Controls.Add(flowLayout);
            return aboutPanel;
        }
        private static LinkLabel CreateLinkLabel(string text, string url, string iconName)
        {
            LinkLabel linkLabel = new LinkLabel
            {
                Text = text, // Add a space before the text for better spacing
                AutoSize = true,
                LinkColor = Color.SkyBlue,
                Anchor = AnchorStyles.None,
                Padding = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Regular),
            };

            // Load the image from resources
           // System.Drawing.Image iconImage = (System.Drawing.Image)SettingsUI.Properties.Resources.ResourceManager.GetObject(iconName);

           /* if (iconImage != null)
            {
                int textHeight = TextRenderer.MeasureText(text, linkLabel.Font).Height;
                System.Drawing.Image resizedImage = new Bitmap(iconImage, new System.Drawing.Size(textHeight, textHeight));
                linkLabel.Image = resizedImage;
                linkLabel.ImageAlign = ContentAlignment.MiddleLeft; // Align the image to the left
                linkLabel.TextAlign = ContentAlignment.MiddleRight; // Align the text to the right
                linkLabel.Padding = new Padding(resizedImage.Width + 5, 0, 10, 10);
            }*/

            linkLabel.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(url);
            return linkLabel;
        }
        private static Icon GetIconOrDefault(string imageFilePath)
        {
            Icon icon = null;
            if (!string.IsNullOrEmpty(imageFilePath))
            {
                if (File.Exists(imageFilePath))
                {
                    icon = ConvertToIcon(imageFilePath);
                }
                else
                {
                    DownloadImage(imageFilePath);
                    string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string filePath = Path.Combine(programDirectory, "Extensions", "Data", "iconImage.png");
                    icon = ConvertToIcon(filePath);
                }
                //
            }



            if (icon == null)
            {
                // Set a default icon if conversion fails or both image files are missing
                icon = SystemIcons.Application;//SettingsUI.Properties.Resources.sbextlogo;//SystemIcons.Application;
            }

            return icon;
        }
        private static Icon ConvertToIcon(string imagePath)
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
        public static void SetButtonColor(Button button, string defaultValue)
        {
            // Convert the default value (assumed to be a hex color string) to a Color object
            if (ColorTranslator.FromHtml(defaultValue) is Color defaultColor)
            {
                button.BackColor = defaultColor;
                // Set the button's text color based on the brightness of the background color
                button.ForeColor = defaultColor.GetBrightness() < 0.5 ? Color.White : Color.Black;
            }
        }

        #region Methods

        //HEADINGS AND LABELS
        public static List<Control> SettingsBuilderAddHeading(this IInlineInvokeProxy CPH, string headingText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var heading = new Label
            {
                Text = headingText,
                Font = new Font(FontFamily.GenericSansSerif, 14.0F, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(10),
                Dock = DockStyle.Fill,
                ForeColor = Color.WhiteSmoke,
                Tag = tabName
            };

            settings.Add(heading); // Add the label directly to the list
            return settings;
        }
        public static List<Control> SettingsBuilderAddLabel(this IInlineInvokeProxy CPH, string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                Tag = tabName

            };

            settings.Add(label);
            return settings;
        }
        public static List<Control> SettingsBuilderAddLink(this IInlineInvokeProxy CPH, string labelText, string url, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            LinkLabel linkLabel = new LinkLabel
            {
                Text = labelText,
                AutoSize = true,
                LinkColor = Color.SkyBlue,
                Padding = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                Tag = tabName
            };

            linkLabel.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(url);

            settings.Add(linkLabel);
            return settings;
        }
        public static List<Control> SettingsBuilderAddLine(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var rule = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ForeColor = Color.WhiteSmoke,
                Tag = tabName
            };

            settings.Add(rule);
            return settings;

        }
        public static List<Control> SettingsBuilderAddDescription(this IInlineInvokeProxy CPH, string labelText, string tabName = "General")
        {
            List<Control> settings = new List<Control>();


            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ForeColor = Color.WhiteSmoke,
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Italic),
                Tag = tabName
            };

            settings.Add(label); // Add control to the list
            return settings;
        }
        public static List<Control> SettingsBuilderAddSpace(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            var space = new Label
            {
                Text = Environment.NewLine,
                Font = new Font(FontFamily.GenericSansSerif, 20.0F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ForeColor = Color.WhiteSmoke,
                Tag = tabName
            };

            settings.Add(space);
            return settings;

        }
        //TEXT
        public static List<Control> SettingsBuilderAddTextBox(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.GetSettingsValue<string>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None,



            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddMultiLineTextBox(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.GetSettingsValue<string>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None,
                Multiline = true,
                Height = 100



            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddLimitText(this IInlineInvokeProxy CPH, string description, string defaultValue, int textLimit, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                Margin = new Padding(10),

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.GetSettingsValue<string>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None,
                MaxLength = textLimit



            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddList(this IInlineInvokeProxy CPH, string description, List<string> defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,

            };


            var input = new DataGridView
            {
                Name = saveName,
                Height = 200,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None,
                ColumnHeadersVisible = false,
                RowHeadersVisible = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ColorTranslator.FromHtml("#1F1F23"),
                    ForeColor = Color.White,
                    SelectionBackColor = ColorTranslator.FromHtml("#3E3E42"),
                    SelectionForeColor = Color.White
                },
                EnableHeadersVisualStyles = false,
                GridColor = ColorTranslator.FromHtml("#1F1F23"),
                AllowUserToDeleteRows = true
            };






            input.Columns.Add("Items", "Items");
            List<string> rowsToAdd = CPH.GetSettingsValue<List<string>>(saveName, defaultValue);
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
        public static List<Control> SettingsBuilderAddPassword(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,

            };

            var input = new CustomTextBox
            {
                Name = saveName,
                Text = CPH.GetSettingsValue<string>(saveName, ""),
                UseSystemPasswordChar = true,
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddDropDown(this IInlineInvokeProxy CPH, string description, string[] dropdown, string defaultValue, string saveName, string tabName = "General")
        {

            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
                Margin = new Padding(0),
                ForeColor = Color.WhiteSmoke,
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,

            };

            var input = new ComboBox
            {
                Name = saveName,
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),


            };

            input.Items.AddRange(dropdown);
            input.SelectedItem = CPH.GetSettingsValue<string>(saveName, defaultValue);

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;



        }
        //NUMBERS
        public static List<Control> SettingsBuilderAddInt(this IInlineInvokeProxy CPH, string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = CPH.GetSettingsValue<int>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
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
        public static List<Control> SettingsBuilderAddDecimal(this IInlineInvokeProxy CPH, string description, decimal defaultValue, int decimalPlaces, decimal increments, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke
            };

            var input = new CustomNumericUpDown
            {
                Name = saveName,
                DecimalPlaces = decimalPlaces,
                Increment = increments,
                Minimum = int.MinValue,
                Maximum = int.MaxValue,
                Value = CPH.GetSettingsValue<decimal>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                BorderStyle = BorderStyle.None

            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddTrackbar(this IInlineInvokeProxy CPH, string description, int defaultValue, int min, int max, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            var input = new TrackBar
            {

                Name = saveName,
                Minimum = min,
                Maximum = max,
                Value = CPH.GetSettingsValue<int>(saveName, defaultValue),
                Padding = new Padding(10),
                Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TickFrequency = (max - min) / 2, // Adjust according to your preference
                TickStyle = TickStyle.TopLeft

            };


            var valueLabel = new Label
            {
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                Text = input.Value.ToString(),
                ForeColor = Color.SkyBlue, // Change text color
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
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
        public static List<Control> SettingsBuilderAddTrueFalse(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            // Create a new CheckBox control

            var input = new CheckBox
            {
                Name = saveName,
                Checked = CPH.GetSettingsValue<bool>(saveName, defaultValue),
                Padding = new Padding(0),
                Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                BackColor = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? Color.SeaGreen : Color.IndianRed,
                Text = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? "True" : "False",
                ForeColor = Color.Black,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? Color.SeaGreen : Color.IndianRed;
                input.Text = input.Checked ? "True" : "False";
            };


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddYesNo(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            // Create a new CheckBox control

            var input = new CheckBox
            {
                Name = saveName,
                Checked = CPH.GetSettingsValue<bool>(saveName, defaultValue),
                Padding = new Padding(0),
                Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                BackColor = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? Color.SeaGreen : Color.IndianRed,
                Text = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? "Yes" : "No",
                ForeColor = Color.Black,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? Color.SeaGreen : Color.IndianRed;
                input.Text = input.Checked ? "Yes" : "No";
            };


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddOnOff(this IInlineInvokeProxy CPH, string description, bool defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            // Create a new CheckBox control

            var input = new CheckBox
            {
                Name = saveName,
                Checked = CPH.GetSettingsValue<bool>(saveName, defaultValue),
                Padding = new Padding(0),
                Margin = new Padding(0, 10, 10, 0),
                Height = 30,
                Width = 280,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Appearance = Appearance.Button,
                BackColor = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? Color.SeaGreen : Color.IndianRed,
                Text = CPH.GetSettingsValue<bool>(saveName, defaultValue) ? "On" : "Off",
                ForeColor = Color.Black,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            input.CheckedChanged += (sender, e) =>
            {
                // Change the background color of the checkbox button based on its checked state
                input.BackColor = input.Checked ? Color.SeaGreen : Color.IndianRed;
                input.Text = input.Checked ? "On" : "Off";
            };


            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddChecklist(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            // Create a new CheckBox control

            var input = new CheckedListBox
            {
                Name = saveName,
                // Checked = CPH.GetSettingsValue<bool>(saveName, defaultValue),
                Padding = new Padding(0),
                Margin = new Padding(0, 10, 10, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Height = 250,
                Width = 280,
                CheckOnClick = true,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),

            };

            input.Items.Add("Item 1", false);  // Add unchecked item
            input.Items.Add("Item 2", true);   // Add checked item
            input.Items.Add("Item 3", false);  // Add unchecked item
            input.Items.Add("Item 4", true);   // Add checked item
            input.Items.Add("Item 5", false);




            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);

            return settings;

        }
        //OTHERS
        public static List<Control> SettingsBuilderAddColour(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            var valueLabel = new Label
            {
                Name = saveName + "OBS",
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                Text = CPH.GetSettingsValue<string>(saveName + "OBS", " "),
                ForeColor = Color.SkyBlue,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,

            };
            var defaultColour = CPH.GetSettingsValue<string>(saveName + "HTML", defaultValue);
            var input = new Button
            {
                Name = saveName + "HTML",
                Text = defaultColour,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = ColorTranslator.FromHtml(defaultColour),
                Size = new System.Drawing.Size(30, 30),
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };

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
                    input.ForeColor = selectedColor.GetBrightness() < 0.5 ? Color.White : Color.Black;
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



            SetButtonColor(input, defaultColour);

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddColor(this IInlineInvokeProxy CPH, string description, string defaultValue, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            var valueLabel = new Label
            {
                Name = saveName + "OBS",
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                Text = CPH.GetSettingsValue<string>(saveName + "OBS", " "),
                ForeColor = Color.SkyBlue,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
            };
            var defaultColour = CPH.GetSettingsValue<string>(saveName + "HTML", defaultValue);
            var input = new Button
            {
                Name = saveName + "HTML",
                Text = defaultColour,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = ColorTranslator.FromHtml(defaultColour),
                Size = new System.Drawing.Size(30, 30),
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

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
                    input.ForeColor = selectedColor.GetBrightness() < 0.5 ? Color.White : Color.Black;
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



            SetButtonColor(input, defaultColour);

            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddFile(this IInlineInvokeProxy CPH, string description, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };

            var valueLabel = new TextBox
            {
                Name = saveName,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                Text = CPH.GetSettingsValue<string>(saveName, " "),
                ForeColor = Color.SkyBlue,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                Multiline = false, // Set Multiline property
            };

            var input = new Button
            {
                Text = "🗁",
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                Size = new System.Drawing.Size(30, 30),
                ForeColor = Color.SkyBlue,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                DialogResult = DialogResult.OK // Set DialogResult
            };
            var fileContent = string.Empty;
            var filePath = string.Empty;
            // Event handler for button click to open file dialog
            input.Click += (sender, e) =>
            {

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Console.WriteLine(openFileDialog.FileName);
                        valueLabel.Text = openFileDialog.FileName;
                        Console.WriteLine(openFileDialog.FileName);
                    }



                }
            };



            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(valueLabel, 2, 0);

            settings.Add(settingsTable);

            return settings;
        }

        public static List<Control> SettingsBuilderAddFont(this IInlineInvokeProxy CPH, string description, string defaultName, string defaultSize, string defaultStyle, string saveName, string tabName = "General")
        {
            List<Control> settings = new List<Control>();

            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                AutoSize = true,
                Padding = new Padding(0),
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
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };


            var input = new Button
            {
                Name = saveName + "Name",
                Text = CPH.GetSettingsValue<string>(saveName + "Name", defaultName),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                Size = new System.Drawing.Size(280, 30),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var size = new Label
            {
                Name = saveName + "Size",
                Text = CPH.GetSettingsValue<string>(saveName + "Size", defaultSize),
                AutoSize = true,
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Bold),
                ForeColor = Color.SkyBlue,

            };
            var style = new Label
            {
                Name = saveName + "Style",
                Text = CPH.GetSettingsValue<string>(saveName + "Style", defaultStyle),
                AutoSize = true,
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Bold),
                ForeColor = Color.SkyBlue,

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





            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);
            settingsTable.Controls.Add(size, 2, 1);
            settingsTable.Controls.Add(style, 1, 1);


            settings.Add(settingsTable);

            return settings;
        }
        public static List<Control> SettingsBuilderAddRunAction(this IInlineInvokeProxy CPH, string description, string buttonText, string actionName, bool runImmediately, string tabName = "General")
        {


            List<Control> settings = new List<Control>();

            // Create a TableLayoutPanel to hold the label and NumericUpDown
            TableLayoutPanel settingsTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0),
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
                Margin = new Padding(10),
                Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
            };



            var input = new Button
            {
                Text = $"{buttonText}",
                Margin = new Padding(0, 10, 0, 0),
                Size = new System.Drawing.Size(280, 30),
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                BackColor = ColorTranslator.FromHtml("#1F1F23"),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            input.Click += (sender, e) =>
            {
                CPH.RunActionButton(actionName, runImmediately);
            };

            // Add the label and input to the table
            settingsTable.Controls.Add(label, 0, 0);
            settingsTable.Controls.Add(input, 1, 0);

            // Add the table layout to the list of controls
            settings.Add(settingsTable);
            return settings;



        }
               

        #endregion

        #region Setting Saving

        public static void SaveButton_Click(this IInlineInvokeProxy CPH, object sender, EventArgs e, List<Control> layout)
        {
            CPH.SUWriteLog("Pressed Save");

            var numericUpDownsAndTextBoxes = layout
         .OfType<TableLayoutPanel>()
         .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                switch (control)
                {
                    case Label label:
                        CPH.SUWriteLog($"Save Name: {label.Name}, Value: {label.Text}");
                        CPH.SaveSettingsValue(label.Name, label.Text);
                        break;
                    case NumericUpDown numericUpDown:
                        CPH.SUWriteLog($"Save Name: {numericUpDown.Name}, Value: {numericUpDown.Value}");
                        CPH.SaveSettingsValue(numericUpDown.Name, numericUpDown.Value);
                        break;
                    case TextBox textBox:
                        CPH.SUWriteLog($"Save Name: {textBox.Name}, Text: {textBox.Text}");
                        CPH.SaveSettingsValue(textBox.Name, textBox.Text);
                        break;
                    case CheckBox checkbox:
                        CPH.SUWriteLog($"Save Name: {checkbox.Name}, Text: {checkbox.Checked}");
                        CPH.SaveSettingsValue(checkbox.Name, checkbox.Checked);
                        break;
                    case TrackBar trackbar:
                        CPH.SUWriteLog($"Save Name: {trackbar.Name}, Text: {trackbar.Value}");
                        CPH.SaveSettingsValue(trackbar.Name, trackbar.Value);
                        break;
                    case Button button:
                        CPH.SUWriteLog($"Save Name: {button.Name}, Text: {button.Text}");
                        CPH.SaveSettingsValue(button.Name, button.Text);
                        break;
                    case ComboBox comboBox:
                        CPH.SUWriteLog($"Save Name: {comboBox.Name}, Text: {comboBox.SelectedItem}");
                        CPH.SaveSettingsValue(comboBox.Name, comboBox.SelectedItem);
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
                        CPH.SUWriteLog($"Save Name: {dataGridView.Name}, Text: {string.Join(",", dataRows.ToArray())}");
                        CPH.SaveSettingsValue(dataGridView.Name, string.Join(",", dataRows.ToArray()));
                        break;


                        // Add other cases for different types of controls if needed
                }
            }

            MessageBox.Show("Settings have been saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void ResetButton_Click(this IInlineInvokeProxy CPH, object sender, EventArgs e, List<Control> layout, Form form)
        {
            CPH.SUWriteLog("Pressed Reset");

            var numericUpDownsAndTextBoxes = layout
         .OfType<TableLayoutPanel>()
         .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                CPH.DeleteSettingsValue(control.Name);
            }
            // Implement reset logic here
            MessageBox.Show("Settings have been reset.", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            form.Hide();
        }
        public static void SaveSettingsValue(this IInlineInvokeProxy CPH, string settingName, object settingValue)
        {
            if (settingName != null)
            {


                string program_Directory = AppDomain.CurrentDomain.BaseDirectory;
                string dir = Path.Combine(program_Directory, "StreamUP", "Data");
                Directory.CreateDirectory(dir);
                string file_Path = Path.Combine(dir, "Settings_Database.db");
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
        public static T GetSettingsValue<T>(this IInlineInvokeProxy CPH, string settingName, T defaultValue)
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, "Settings_Database.db");

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
        public static void DeleteSettingsValue(this IInlineInvokeProxy CPH, string settingName)
        {

            string program_Directory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(program_Directory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            string file_Path = Path.Combine(dir, "Settings_Database.db");

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


}
