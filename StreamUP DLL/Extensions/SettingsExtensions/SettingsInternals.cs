using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private readonly List<Control> controls = new List<Control>();

        public void RunActionButton(string actionName, bool runImmediately)
        {
            _CPH.RunAction(actionName, runImmediately);
        }

        public Form BuildForm(string title, List<Control> layout, ProductInfo productInfo, int imageFilePath = -1)
        {
            // Reset global state before launching
            UIResources.streamUpSettingsProgress = 0;
            UIResources.closeLoadingWindow = false;

            // Launch the progress bar window
            _CPH.SUUIShowSettingsLoadingMessage("Loading Settings...");

            // Create the main form
            Form form = CreateMainForm(title, layout, productInfo, imageFilePath);
            form.Load += (sender, e) => SaveButton_Click(sender, e, layout);
            // Close the progress bar
            UIResources.closeLoadingWindow = true;

            return form;
        }

        public Form CreateMainForm(string title, List<Control> layout, ProductInfo productInfo, int imageFilePath = -1)
        {
            // Initialise the progress
            UIResources.streamUpSettingsCount = layout.Count + 7;

            InitializeDatabase(out string filePath);
            UIResources.streamUpSettingsProgress++;

            // Dictionary to hold controls for each tab
            Dictionary<string, List<Control>> tabControls = new();

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
            UIResources.streamUpSettingsProgress++;

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

            var form = new Form
            {
                Text = title,
                Width = 850,
                Height = 800,
                FormBorderStyle = FormBorderStyle.Sizable,
                Icon = GetIconOrDefault(imageFilePath),
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
                    AutoScroll = true,
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

                    // Increment progress as each control is added
                    UIResources.streamUpSettingsProgress++;
                }

                tabPage.Controls.Add(tableLayout);
                tabControl.TabPages.Add(tabPage);
            }

            // Create "About" tab page
            TabPage aboutTabPage = new TabPage("About")
            {
                BackColor = backColour1
            };
            aboutTabPage.Controls.Add(CreateAboutPanel(productInfo));
            tabControl.TabPages.Add(aboutTabPage);
            UIResources.streamUpSettingsProgress++;


            // Add buttons to the button panel
            buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5),
                AutoSize = true,
                ForeColor = forecolour1,
                Font = new Font("Segoe UI Emoji", 10.0F, FontStyle.Regular)
            };

            // Save and Reset buttons
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
            saveButton.Click += (sender, e) => SaveButton_Click(sender, e, layout);

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
            resetButton.Click += (sender, e) => ResetButton_Click(sender, e, layout, form);

            saveButton.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(saveButton, "Save the Data");
            resetButton.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(resetButton, "Reset the Data to Defaults");

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(resetButton);
            UIResources.streamUpSettingsProgress++;

            // Status bar
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
            UIResources.streamUpSettingsProgress++;

            return form;
        }

        private Panel CreateAboutPanel(ProductInfo productInfo)
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
                Text = GetNamesForCredits(),
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



            Label lblProductName = CreateInfoLabel("Product Name:", productInfo.ProductName);
            Label lblProductVersionNumber = CreateInfoLabel("Product Version:", productInfo.ProductVersionNumber.ToString());
            Label lblProductNumber = CreateInfoLabel("Product Number:", productInfo.ProductNumber);
            // Label lblRequiredLibraryVersion = CreateInfoLabel("Required Library Version:", productInfo.RequiredLibraryVersion.ToString());
            // Label lblSceneName = CreateInfoLabel("Scene Name:", productInfo.SceneName);
            // Label lblSettingsAction = CreateInfoLabel("Settings Action:", productInfo.SettingsAction);
            // Label lblSourceNameVersionCheck = CreateInfoLabel("Source Name Version Check:", productInfo.SourceNameVersionCheck);
            // Label lblSourceNameVersionNumber = CreateInfoLabel("Source Name Version Number:", productInfo.SourceNameVersionNumber.ToString());

            Label line1 = CreateLine();
            Label line2 = CreateLine();
            Label line3 = CreateLine();
            Label line4 = CreateLine();



            LinkLabel streamUp = CreateLinkLabel("StreamUP", "https://doras.to/streamup");
            LinkLabel andi = CreateLinkLabel("Andilippi", "https://doras.to/andi", "Founder");
            LinkLabel waldo = CreateLinkLabel("WaldoAndFriends", "https://doras.to/waldo", "Founder");
            LinkLabel silver = CreateLinkLabel("Silverlink", "https://doras.to/silverlink", "Founder");
            LinkLabel terrierdarts = CreateLinkLabel("CodeWithTD", "https://doras.to/td", "Developer");


            flowLayout.Controls.Add(productInfoLabel);
            flowLayout.Controls.Add(lblProductName);
            flowLayout.Controls.Add(lblProductVersionNumber);
            flowLayout.Controls.Add(lblProductNumber);
            // flowLayout.Controls.Add(lblRequiredLibraryVersion);
            // flowLayout.Controls.Add(lblSceneName);
            // flowLayout.Controls.Add(lblSettingsAction);
            // flowLayout.Controls.Add(lblSourceNameVersionCheck);
            // flowLayout.Controls.Add(lblSourceNameVersionNumber);
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

        private string GetNamesForCredits()
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

        private LinkLabel CreateLinkLabel(string text, string url, string subhead = "")
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

        private Label CreateLine()
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

        private Label CreateInfoLabel(string text, string value)
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

        private Icon GetIconOrDefault(int imageNumber = -1)
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

        public void SetButtonColor(Button button, string defaultValue)
        {
            // Convert the default value (assumed to be a hex color string) to a Color object
            if (ColorTranslator.FromHtml(defaultValue) is Color defaultColor)
            {
                button.BackColor = defaultColor;
                // Set the button's text color based on the brightness of the background color
                button.ForeColor = defaultColor.GetBrightness() < 0.5 ? Color.White : Color.Black;
            }
        }

        private Image GetRandomImageIcon()
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

        private Task<string> OpenFileDialogAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            Thread thread = new Thread(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    openFileDialog.Filter = "All files (*.*)|*.*";
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

        private Task<string> OpenFolderDialogAsync()
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

        //Logging
        public void SaveButton_Click(object sender, EventArgs e, List<Control> layout)
        {
            LogInfo("Pressed Save");
            _CPH.SetArgument("settingResetArgument", false);
            var numericUpDownsAndTextBoxes = layout
                .OfType<TableLayoutPanel>()
                .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                switch (control)
                {
                    case Label label:
                        
                        if (!string.IsNullOrEmpty(label.Name))
                        {
                            SaveSetting(label.Name, label.Text);
                        }
                        break;
                    case NumericUpDown numericUpDown:
                        
                        SaveSetting(numericUpDown.Name, numericUpDown.Value.ToString());
                        break;
                    case TextBox textBox:
                        
                        SaveSetting(textBox.Name, textBox.Text);
                        break;
                    case CheckBox checkbox:
                        
                        SaveSetting(checkbox.Name, checkbox.Checked);
                        break;
                    case TrackBar trackbar:
                        
                        SaveSetting(trackbar.Name, trackbar.Value);
                        break;
                    case Button button:
                        
                        SaveSetting(button.Name, button.Text);
                        break;
                    case ComboBox comboBox:
                       
                        SaveSetting(comboBox.Name, comboBox.SelectedItem);
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
                       
                        SaveSetting(checkedListBox.Name, checkedItemsDict);
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
                            //LogInfo($"Save Name: {dataGridView.Name}, Data: {string.Join(",", dataRows.ToArray())}");
                            SaveSetting(dataGridView.Name, dataRows);
                        }
                        else if (dataGridView.Columns.Count == 2)
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

                                SaveSetting(dataGridView.Name, dataDictInt);
                            }
                            else
                            {

                                SaveSetting(dataGridView.Name, dataDictString);
                            }



                        }
                        else if (dataGridView.Columns.Count == 3)
                        {

                            if (dataGridView.Name.Contains("slot"))
                            {
                                var dataTuple = new List<(string Emote, int Payout, int Percentage)>();


                                foreach (DataGridViewRow row in dataGridView.Rows)
                                {

                                    if (!row.IsNewRow)
                                    {
                                        string key = row.Cells[0].Value?.ToString();
                                        int value1 = int.Parse(row.Cells[1].Value?.ToString());
                                        int value2 = int.Parse(row.Cells[2].Value?.ToString());

                                        // Add the tuple to the list
                                        dataTuple.Add((key, value1, value2));
                                    }
                                }


                                SaveSetting(dataGridView.Name, dataTuple);
                            }




                        }

                        break;
                }
            }
            if (sender == saveButton)
            {
                MessageBox.Show("Settings have been saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void ResetButton_Click(object sender, EventArgs e, List<Control> layout, Form form)
        {
            LogInfo("Pressed Reset");

            var numericUpDownsAndTextBoxes = layout
            .OfType<TableLayoutPanel>()
            .SelectMany(tableLayoutPanel => tableLayoutPanel.Controls.OfType<Control>());

            foreach (var control in numericUpDownsAndTextBoxes)
            {
                DeleteSetting(control.Name);
            }
            // Implement reset logic here
            MessageBox.Show("Settings have been reset back to defaults.\nThis UI will now close and attempt to reload so you will need to save your desired settings again.", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            form.Close();
            //todo Handle Returning False for reset
            _CPH.SetArgument("settingResetArgument", true);
            _CPH.TryGetArg("actionName", out string actionName);
            _CPH.RunAction(actionName, false);

        }

        public void InitializeDatabase(out string filePath)
        {
            _CPH.TryGetArg("saveFile", out string saveFile);

            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(programDirectory, "StreamUP", "Data");
            Directory.CreateDirectory(dir);
            filePath = Path.Combine(dir, $"{saveFile}_ProductSettings.json");
            Initialize(filePath);
        }

        public T GetSetting<T>(string settingName, T defaultValue)
        {
            if (!_initialized)
            {
                InitializeDatabase(out string filePath);
            }
            return StreamUpInternalGet(settingName, defaultValue);
        }

        public void SaveSetting(string settingName, object newValue)
        {
            LogInfo($"Saving - {settingName} with {newValue}");
            if (!_initialized)
            {
                InitializeDatabase(out string filePath);
            }
            StreamUpInternalUpdate(settingName, newValue);
        }

        public void DeleteSetting(string settingName)
        {
            if (!_initialized)
            {
                InitializeDatabase(out string filePath);
            }
            StreamUpInternalDelete(settingName);
        }

        public void SettingsDispose()
        {
            _initialized = false;
        }

    }
}