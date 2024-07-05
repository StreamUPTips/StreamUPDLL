using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Label = System.Windows.Forms.Label;
using Streamer.bot.Plugin.Interface;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace StreamUP
{

    // Settings definition
    // Currently support for 11 different settings types:
    // - StreamUpSettingType.Action
    // - StreamUpSettingType.Boolean
    // - StreamUpSettingType.Colour
    // - StreamUpSettingType.ColourHex
    // - StreamUpSettingType.Double
    // - StreamUpSettingType.Dropdown
    // - StreamUpSettingType.Heading
    // - StreamUpSettingType.Integer
    // - StreamUpSettingType.Label
    // - StreamUpSettingType.Reward
    // - StreamUpSettingType.Ruler
    // - StreamUpSettingType.Secret
    // - StreamUpSettingType.Spacer
    // - StreamUpSettingType.String
    // - StreamUpSettingType.Link 
    //
    // Name = The variable name, Description = The UI label, Type = See above, Default = The default value as a string.
    public static class ProductSettingsUIExtensions
    {
        public static bool? savePressed = false;
        private static string logName = "DLL::ProductSettingsUI";
     private static readonly Color backColour1 = ColorTranslator.FromHtml("#121212");
        private static readonly Color backColour2 = ColorTranslator.FromHtml("#676767");
        private static readonly Color backColour3 = ColorTranslator.FromHtml("#212121");
        private static Color forecolour1 = Color.WhiteSmoke;
        private static Color forecolour2 = Color.SkyBlue;
        private static Color linkColour = ColorTranslator.FromHtml("#FF86BD");
        private static Color boolTrueColor = Color.SeaGreen;
        private static Color boolFalseColor =  Color.IndianRed;

        public static bool? SUExecuteSettingsMenu(this IInlineInvokeProxy CPH, ProductInfo productInfo, List<StreamUpSetting> streamUpSettings, IDictionary<string, object> sbArgs, string settingsGlobalName = "ProductSettings")
        {
            // Create loading window
            UIResources.closeLoadingWindow = false;
            UIResources.streamUpSettingsCount = streamUpSettings.Count;
            CPH.SUUIShowSettingsLoadingMessage("StreamUP Settings Loading...");

            Dictionary<string, object> productSettings = null;
            string productSettingsJson = CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_{settingsGlobalName}", true);
            if (!string.IsNullOrEmpty(productSettingsJson))
            {
                productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(productSettingsJson);
            }

            List<string> sbActions = new List<string>();
            for (int i = 0; i < streamUpSettings.Count; i++)
            {
                StreamUpSetting item = streamUpSettings[i];
                if (item.Type == StreamUpSettingType.Action)
                {
                    sbActions = CPH.GetSBActions(sbArgs);
                    if (sbActions.Count == 0)
                    {
                        return false;
                    }
                }
            }

            var settingsForm = new Form
            {
                Text = $"StreamUP | {productInfo.ProductName}",
                Width = 540,
                Height = 720,
                FormBorderStyle = FormBorderStyle.Fixed3D,
                //MaximizeBox = false,
                //MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = backColour1
            };

            byte[] bytes = Convert.FromBase64String(UIResources.supIconString);
            using var ms = new MemoryStream(bytes);
            settingsForm.Icon = new Icon(ms);

            TabControl tabControl = new BorderlessTabControl
            {
                Dock = DockStyle.Fill,
                ActiveText = forecolour1,
                ActiveBackground = backColour1,
                InactiveText = forecolour2,
                InactiveBackground = backColour3,
                TabBarBackColor = backColour3,
            };
            settingsForm.Controls.Add(tabControl);
            Dictionary<string, TableLayoutPanel> tabPages = new Dictionary<string, TableLayoutPanel>();

            var description = new Label();
            description.Text = $"Please provide your preferred settings for {productInfo.ProductName} using the controls below.";
            description.MaximumSize = new System.Drawing.Size(498, 0);
            description.AutoSize = true;
            description.Dock = DockStyle.Fill;
            description.Padding = new Padding(0, 4, 0, 0);
            settingsForm.Controls.Add(description);

            int rowIndex = 0;
            foreach (var setting in streamUpSettings)
            {
                string tabName = setting.TabName ?? "General";

                if (!tabPages.ContainsKey(tabName))
                {
                    var tabPage = new TabPage(tabName);
                    var settingsTable = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 3,
                        AutoScroll = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        BackColor = backColour1
                    };

                    settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                    tabPage.BackColor= backColour1;
                    tabPage.Controls.Add(settingsTable);
                    tabControl.Controls.Add(tabPage);
                    tabPages[tabName] = settingsTable;
                }

                // Now add settings to the table
                CPH.AddSettingToTable(tabPages[tabName], setting, sbActions, productSettings, ref rowIndex);
                UIResources.streamUpSettingsProgress += 1;
            }

            // Add Save, Reset, Cancel buttons
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            CPH.AddButtonControls(buttonPanel, withParent: settingsForm, atIndex: streamUpSettings.Count + 1, streamUpSettings, sbArgs, productInfo, settingsGlobalName, tabControl);

            var statusBar = new StatusStrip
            {
                SizingGrip = false,
                BackColor = backColour2
            };
            var statusLabel = new ToolStripStatusLabel
            {
                Text = $"© StreamUP {DateTime.UtcNow.Year}",
                ForeColor = forecolour1
            };
            statusBar.Items.Add(statusLabel);
            settingsForm.Controls.Add(buttonPanel);
            settingsForm.Controls.Add(statusBar);

            UIResources.closeLoadingWindow = true;
            settingsForm.Focus();
            settingsForm.ShowDialog();
            UIResources.streamUpSettingsProgress = 0;
            UIResources.streamUpSettingsCount = 0;
            CPH.SUWriteLog("Settings menu loaded.", logName);

            return savePressed;
        }

        private static void AddSettingToTable(this IInlineInvokeProxy CPH, TableLayoutPanel table, StreamUpSetting setting, List<string> actions, Dictionary<string, object> settings, ref int rowIndex)
        {
            switch (setting.Type)
            {
                case StreamUpSettingType.Action:
                    CPH.AddActionSetting(toTable: table, withSetting: setting, withActions: actions, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Boolean:
                    CPH.AddBoolSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Colour:
                    CPH.AddColorSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.ColourHex:
                    CPH.AddColorSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Double:
                    CPH.AddDoubleSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Dropdown:
                    CPH.AddDropdownSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Heading:
                    CPH.AddHeadingSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Integer:
                    CPH.AddIntSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Label:
                    CPH.AddLabelSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Reward:
                    CPH.AddRewardSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Ruler:
                    CPH.AddRulerSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Secret:
                    CPH.AddSecretSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Spacer:
                    CPH.AddSpacerSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.String:
                    CPH.AddStringSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Link:
                    CPH.AddLinkSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.Multiline:
                    CPH.AddMultiStringSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.TrackBar:
                    CPH.AddTrackbarSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.FileDialog:
                    CPH.AddFileSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
                case StreamUpSettingType.FolderBrowser:
                    CPH.AddFolderSetting(toTable: table, withSetting: setting, atIndex: rowIndex, settings);
                    break;
            }
            rowIndex++;
        }


        private static List<string> GetSBActions(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs)
        {
            ClientWebSocket ws = new ClientWebSocket();
            var sbActions = new List<string>();

            var wsuri = sbArgs["websocketURI"].ToString();
            try
            {
                ws.ConnectAsync(new Uri(wsuri), CancellationToken.None).Wait();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("An error occurred while fetching Streamer.bot actions.\nPlease check your Streamer.bot websocket settings, make sure the internal websocket server is turned on and try again.\n\nThe websocket URL and port should match your Streamer.bot instance.", "StreamUP Settings UI - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sbActions;
            }

            string response = string.Empty;
            string json = string.Empty;
            string data = JsonConvert.SerializeObject(
                new
                {
                    request = "GetActions",
                    id = "123"
                }
            );

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var dataSegment = new ArraySegment<byte>(dataBytes);
            ws.SendAsync(dataSegment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            var buffer = new byte[8192];
            var bufferSegment = new ArraySegment<byte>(buffer);
            WebSocketReceiveResult result = ws.ReceiveAsync(bufferSegment, CancellationToken.None).Result;
            response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            json += response;

            if (result.EndOfMessage)
            {
                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }
            else
            {
                while (!result.EndOfMessage)
                {
                    result = ws.ReceiveAsync(bufferSegment, CancellationToken.None).Result;
                    response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
                    json = json + response;
                }

                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }

            JObject actionJson = JObject.Parse(json.ToString());
            JArray actionName = actionJson.Value<JArray>("actions");
            foreach (JObject item in actionName)
            {
                object name = item["name"].ToString();
                sbActions.Add(name.ToString());
            }

            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "No longer needed", CancellationToken.None).Wait();
            return sbActions;
        }

        private static void AddActionSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, List<string> withActions, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            label.ForeColor = forecolour1;
            toTable.Controls.Add(label, 0, atIndex);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange(withActions.ToArray());
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;


            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex);
        }

        private static void AddRewardSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var twitchRewards = CPH.TwitchGetRewards();
            List<string> titleList = new List<string>();

            foreach (var reward in twitchRewards)
            {
                // Extract the "Title" value from the TwitchReward object
                string title = reward.Title;

                // Add the name to the names list
                titleList.Add(title);
            }

            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange(titleList.ToArray());
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            string rewardTitle = null;
            foreach (var reward in twitchRewards)
            {
                if (reward.Id == currentValue)
                {
                    rewardTitle = reward.Title;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                dropdown.SelectedItem = rewardTitle;
            }

            dropdown.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex);
        }

        private static void AddDropdownSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange((string[])withSetting.Data);
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            var clearButton = new Button();
            clearButton.Text = "Clear";
            clearButton.Click += (sender, e) =>
            {
                dropdown.SelectedItem = null;
            };

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex);
        }

        private static void SetComboBoxWidth(ComboBox cb, int minWidth = 120, int maxWidth = 245)
        {
            int calculatedWidth = 0;
            int temp = 0;
            foreach (var obj in cb.Items)
            {
                temp = TextRenderer.MeasureText(obj.ToString(), cb.Font).Width;
                if (temp > calculatedWidth)
                {
                    calculatedWidth = temp;
                }
            }
            calculatedWidth += 20; // Add padding
            cb.Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));

            cb.MouseWheel += (sender, e) =>
            {
                ((HandledMouseEventArgs)e).Handled = true;
            };
        }

        private static void AddSecretSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);

            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;
            textbox.UseSystemPasswordChar = true;

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                textbox.Text = currentValue;
            }
            else
            if (!string.IsNullOrEmpty(withSetting.Default))
            {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex);
        }

        private static void AddSpacerSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            toTable.Controls.Add(new Label(), 0, atIndex);
            toTable.Controls.Add(new Label(), 1, atIndex);
        }

        private static void AddRulerSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            Label rulerLabel = new Label();
            rulerLabel.BorderStyle = BorderStyle.Fixed3D;
            rulerLabel.Height = 2;
            rulerLabel.Width = 496;
            rulerLabel.ForeColor = forecolour1;
            rulerLabel.Margin = new Padding(10, 10, 10, 10);

            toTable.Controls.Add(rulerLabel, 0, atIndex);
            toTable.SetColumnSpan(rulerLabel, 3);
        }

        private static void AddStringSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);
            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex);
        }

        private static void AddMultiStringSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label
            {
                Text = withSetting.Description,
                Padding = new Padding(0, 4, 0, 0),
                AutoSize = true,
                MaximumSize = new System.Drawing.Size(250, 0),
                ForeColor = forecolour1
            };
            toTable.Controls.Add(label, 0, atIndex);
            var textbox = new TextBox
            {
                Name = withSetting.Name,
                Multiline = true,
                Height = 100,
                Width = 240
            };

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex);
        }


        private static void AddColorSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 8, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(270, 0);
            toTable.Controls.Add(label, 0, atIndex);
            var button = new Button();
            button.Text = "Pick a colour";
            button.AutoSize = true;
            button.Name = withSetting.Name;
            button.Tag = withSetting.Type;

            long currentValue = 0;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                var colorSetting = settings[withSetting.Name].ToString();
                if (colorSetting.Contains("#"))
                {
                    Color color = ColorTranslator.FromHtml(colorSetting);
                    currentValue = ((long)color.A << 24) | ((long)color.B << 16) | ((long)color.G << 8) | color.R;
                }
                else
                {
                    currentValue = long.Parse(settings[withSetting.Name].ToString());
                }
            }

            if (currentValue != 0)
            {
                byte a = (byte)((currentValue & 0xff000000) >> 24);
                byte b = (byte)((currentValue & 0x00ff0000) >> 16);
                byte g = (byte)((currentValue & 0x0000ff00) >> 8);
                byte r = (byte)(currentValue & 0x000000ff);

                var colour = System.Drawing.Color.FromArgb(a, r, g, b);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                var colour = ColorTranslator.FromHtml(withSetting.Default);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }

            button.Click += (sender, e) =>
            {
                var colorDialog = new ColorDialog();
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    button.ForeColor = (colorDialog.Color.GetBrightness() < 0.5) ? Color.White : Color.Black;
                    button.BackColor = colorDialog.Color;
                    var selectedColor = colorDialog.Color;
                    string hexValue = selectedColor.R.ToString("X2") + selectedColor.G.ToString("X2") + selectedColor.B.ToString("X2");
                    button.Text = "#" + hexValue;
                }
            };
            toTable.Controls.Add(button, 1, atIndex);
        }


        private static void AddBoolSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);
            var checkbox = new CheckBox();
            checkbox.Name = withSetting.Name;

            bool? currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = bool.Parse(settings[withSetting.Name].ToString());
            }

            if (currentValue != null)
            {
                checkbox.Checked = currentValue.Value;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                var hasCorrectDefault = bool.TryParse(withSetting.Default, out bool defaultValue);
                checkbox.Checked = hasCorrectDefault ? defaultValue : false;
            }

            toTable.Controls.Add(checkbox, 1, atIndex);
        }

        private static void AddIntSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);
            var input = new NumericUpDown();
            input.Minimum = int.MinValue;
            input.Maximum = int.MaxValue;
            input.Name = withSetting.Name;
            input.Tag = withSetting.Type;

            int currentValue = 0;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = int.Parse(settings[withSetting.Name].ToString());
            }

            if (currentValue != 0)
            {
                input.Value = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                var hasCorrectDefault = int.TryParse(withSetting.Default, out int defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex);
        }

        private static void AddTrackbarSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label
            {
                Text = withSetting.Description,
                AutoSize = true,
                Margin = new Padding(4),
                ForeColor = forecolour1
               
            };
            var input = new TrackBar
            {

                Name = withSetting.Name,
                Minimum = withSetting.Min,
                Maximum = withSetting.Max,
                Padding = new Padding(4),
                //Margin = new Padding(0, 10, 10, 0),
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                TickFrequency = (withSetting.Max - withSetting.Min) / 2, // Adjust according to your preference
                TickStyle = TickStyle.TopLeft,
                //Width = 150

            };

            int currentValue = 0;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = int.Parse(settings[withSetting.Name].ToString());
            }

            if (currentValue != 0)
            {
                input.Value = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                var hasCorrectDefault = int.TryParse(withSetting.Default, out int defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            var valueLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Right,
                //Margin = new Padding(0, 10, 0, 0),
                Text = input.Value.ToString(),
                ForeColor = forecolour2, // Change text color
                Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                //Width = 60

            };



            input.ValueChanged += (sender, e) =>
            {
                valueLabel.Text = input.Value.ToString();
            };


            toTable.Controls.Add(label, 0, atIndex);
            toTable.Controls.Add(input, 1, atIndex);
            toTable.Controls.Add(valueLabel, 2, atIndex);



        }

        private static void AddLabelSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(498, 0);

            toTable.Controls.Add(label, 0, atIndex);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddLinkSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new LinkLabel();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(498, 0);
            label.LinkColor = linkColour;
            label.Font = new Font(FontFamily.GenericSansSerif, 10.0F, FontStyle.Regular);
            label.LinkClicked += (sender, e) => System.Diagnostics.Process.Start(withSetting.Url);
            toTable.Controls.Add(label, 0, atIndex);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddHeadingSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour2;
            label.MaximumSize = new System.Drawing.Size(498, 0);
            label.Font = new Font(label.Font.FontFamily, label.Font.Size + 2, System.Drawing.FontStyle.Bold);

            toTable.Controls.Add(label, 0, atIndex);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddDoubleSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.ForeColor = forecolour1;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex);

            var input = new NumericUpDown();
            input.Minimum = decimal.MinValue;
            input.Maximum = decimal.MaxValue;
            input.DecimalPlaces = 2;
            input.Increment = 0.01m;
            input.Name = withSetting.Name;
            input.Tag = withSetting.Type;

            double currentValue = 0;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = double.Parse(settings[withSetting.Name].ToString());
            }

            if (currentValue != 0)
            {
                input.Value = Convert.ToDecimal(currentValue);
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                var hasCorrectDefault = decimal.TryParse(withSetting.Default, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex);
        }

        public static void AddFileSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {

            var label = new Label
            {
                Text = withSetting.Description,
                //AutoSize = true,
                Width = 170,
                Margin = new Padding(10),
                ForeColor = forecolour1,
            };

            var textbox = new TextBox
            {
                Name = withSetting.Name,
                Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
                //Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                //BackColor = ColorTranslator.FromHtml("#1F1F23"),
                //ForeColor = Color.White,
                //Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                //BorderStyle = BorderStyle.None,
                Width = 170
            };
            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                textbox.Text = withSetting.Default;
            }

            var input = new Button
            {
                Text = "...",
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                //Size = new System.Drawing.Size(40, 40),
                ForeColor = forecolour2,
                //Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                //DialogResult = DialogResult.OK // Set DialogResult
            };

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFileDialogAsync();
                if (filePath != null)
                {
                    textbox.Text = filePath;
                    //Console.WriteLine(filePath);
                }
            };

            toTable.Controls.Add(label, 0, atIndex);
            toTable.Controls.Add(textbox, 1, atIndex);
            toTable.Controls.Add(input, 2, atIndex);


        }

         public static void AddFolderSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {

            var label = new Label
            {
                Text = withSetting.Description,
                //AutoSize = true,
                Width = 170,
                Margin = new Padding(10),
                ForeColor = forecolour1,
            };

            var textbox = new TextBox
            {
                Name = withSetting.Name,
                Padding = new Padding(10),
                //Margin = new Padding(0, 10, 10, 0),
               // Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                //BackColor = ColorTranslator.FromHtml("#1F1F23"),
                //ForeColor = Color.White,
                //Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                //BorderStyle = BorderStyle.None,
                Width = 170
                //AutoSize = true
                
            };
            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default))
            {
                textbox.Text = withSetting.Default;
            }

            var input = new Button
            {
                Text = "...",
                AutoSize = true,
                //Margin = new Padding(0, 10, 0, 0),
                //Size = new System.Drawing.Size(40, 40),
                ForeColor = forecolour2,
                //Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                //DialogResult = DialogResult.OK // Set DialogResult
            };

            // Event handler for button click to open file dialog
            input.Click += async (sender, e) =>
            {
                string filePath = await OpenFolderDialogAsync();
                if (filePath != null)
                {
                    textbox.Text = filePath;
                    //Console.WriteLine(filePath);
                }
            };

            toTable.Controls.Add(label, 0, atIndex);
            toTable.Controls.Add(textbox, 1, atIndex);
            toTable.Controls.Add(input, 2, atIndex);


        }

        private static Task<string> OpenFileDialogAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            Thread thread = new Thread(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    //openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
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
        private static void AddButtonControls(this IInlineInvokeProxy CPH, Panel buttonPanel, Form withParent, int atIndex, List<StreamUpSetting> streamUpSettings, IDictionary<string, object> sbArgs, ProductInfo productInfo, string settingsGlobalName, TabControl tabControl)
        {
            var resetButton = new Button
            {
                Font = new Font("Segoe UI Emoji", 10),
                Text = "🔄 Reset",
                BackColor = Color.LightCoral
            };

            resetButton.Click += (sender, e) =>
            {
                DialogResult result = MessageBox.Show("Are you sure you want to reset all settings?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    // Dictionary to hold the settings for serialization
                    var settingsDictionary = new Dictionary<string, object>();

                    foreach (var setting in streamUpSettings)
                    {
                        if (!string.IsNullOrEmpty(setting.Name))
                        {
                            object value = setting.Default;
                            if (!string.IsNullOrEmpty(setting.Default))
                            {
                                switch (setting.Type)
                                {
                                    case StreamUpSettingType.Boolean:
                                        value = bool.Parse(setting.Default);
                                        break;
                                    case StreamUpSettingType.Double:
                                        value = double.Parse(setting.Default);
                                        break;
                                    case StreamUpSettingType.Integer:
                                        value = int.Parse(setting.Default);
                                        break;
                                    case StreamUpSettingType.Colour:
                                        var colour = ColorTranslator.FromHtml(setting.Default);
                                        long colourValue = ((long)colour.A << 24) | ((long)colour.B << 16) | ((long)colour.G << 8) | (long)colour.R;
                                        value = colourValue;
                                        break;
                                    case StreamUpSettingType.TrackBar:

                                        value = int.Parse(setting.Default);
                                        break;
                                }
                            }
                            settingsDictionary[setting.Name] = value;
                        }
                    }

                    string settingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(settingsDictionary);
                    CPH.SetGlobalVar($"{productInfo.ProductNumber}_{settingsGlobalName}", settingsJson, true);

                    savePressed = null;
                    withParent.Close();
                    CPH.SUWriteLog("Reset button pressed. Resetting settings and loading settings menu again.", logName);
                    CPH.RunAction(sbArgs["actionName"].ToString(), false);
                }
            };

            var saveButton = new Button();
            saveButton.Font = new Font("Segoe UI Emoji", 10);
            saveButton.Text = "💾 Save";
            saveButton.BackColor = Color.LightGreen;
            saveButton.Click += (sender, e) =>
            {
                var twitchRewards = CPH.TwitchGetRewards();
                Dictionary<string, object> settingsToSave = new Dictionary<string, object>();

                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    var tableLayout = tabPage.Controls.OfType<TableLayoutPanel>().FirstOrDefault();

                    if (tableLayout != null)
                    {
                        foreach (Control control in tableLayout.Controls)
                        {
                            object value = null;

                            switch (control)
                            {
                                case ComboBox comboBox when control.Tag is StreamUpSettingType.Action:
                                    value = comboBox.SelectedItem?.ToString() ?? string.Empty;
                                    break;
                                case ComboBox comboBox when control.Tag is StreamUpSettingType.Reward:
                                    var rewardTitle = comboBox.SelectedItem?.ToString();
                                    var rewardId = twitchRewards.FirstOrDefault(reward => reward.Title == rewardTitle)?.Id;
                                    value = rewardId ?? string.Empty;
                                    break;
                                case ComboBox comboBox when control.Tag is StreamUpSettingType.Dropdown:
                                    value = comboBox.SelectedItem?.ToString() ?? string.Empty;
                                    break;
                                case CheckBox checkBox:
                                    value = checkBox.Checked;
                                    break;
                                case Button button when control.Tag is StreamUpSettingType.ColourHex:
                                    Color buttonColor = button.BackColor;
                                    string colorHex = $"#{buttonColor.R.ToString("X2")}{buttonColor.G.ToString("X2")}{buttonColor.B.ToString("X2")}";
                                    value = colorHex;
                                    break;
                                case Button button:
                                    long colourValue = ((long)button.BackColor.A << 24) | ((long)button.BackColor.B << 16) | ((long)button.BackColor.G << 8) | button.BackColor.R;
                                    value = colourValue;
                                    break;
                                case NumericUpDown numericUpDown when control.Tag is StreamUpSettingType.Integer:
                                    value = Convert.ToInt32(numericUpDown.Value);
                                    break;
                                case NumericUpDown numericUpDown when control.Tag is StreamUpSettingType.Double:
                                    value = Convert.ToDouble(numericUpDown.Value);
                                    break;
                                case TextBox textBox:
                                    value = textBox.Text;
                                    break;
                                case TrackBar trackBar:
                                    value = trackBar.Value;
                                    break;
                            }

                            if (value != null)
                            {
                                settingsToSave[control.Name] = value;
                            }
                        }
                    }
                }

                if (settingsToSave.ContainsKey("ObsConnection"))
                {
                    if (!CPH.SUValObsIsConnected(productInfo, (int)settingsToSave["ObsConnection"]))
                    {
                        withParent.Close();
                        CPH.SUWriteLog("METHOD FAILED", logName);
                        return;
                    }
                    settingsToSave["ScaleFactor"] = CPH.SUObsGetCanvasScaleFactor(productInfo.ProductNumber, (int)settingsToSave["ObsConnection"]);
                }

                string settingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(settingsToSave);
                CPH.SetGlobalVar($"{productInfo.ProductNumber}_{settingsGlobalName}", settingsJson, true);

                savePressed = true;
                withParent.Close();
            };

            var cancelButton = new Button();
            cancelButton.Font = new Font("Segoe UI Emoji", 10);
            cancelButton.Text = "❌ Cancel";
            cancelButton.BackColor = Color.LightGray;
            cancelButton.Click += (sender, e) =>
            {
                savePressed = false;
                withParent.Close();
            };

            cancelButton.Width = 100;
            resetButton.Width = 100;
            saveButton.Width = 100;

            resetButton.Dock = DockStyle.Left;

            saveButton.Dock = DockStyle.Right;
            buttonPanel.Controls.Add(saveButton);

            cancelButton.Dock = DockStyle.Right;
            buttonPanel.Controls.Add(cancelButton);

            buttonPanel.Controls.Add(resetButton);
        }
    }
    
    public class StreamUpSetting
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public StreamUpSettingType Type { get; set; }

        public string Default { get; set; }

        public object Data { get; set; }
        public string TabName { get; set; }

        public string Url { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }
    }

    public enum StreamUpSettingType
    {
        Action,
        Boolean,
        Colour,
        ColourHex,
        Double,
        Dropdown,
        Heading,
        Integer,
        Label,
        Reward,
        Ruler,
        Secret,
        Spacer,
        String,
        Link,
        TrackBar,
        Multiline,
        FileDialog,
        FolderBrowser

    }


    public static class ProductSettingsBuilder
    {
        public static List<StreamUpSetting> SUSettingsCreateAction(this IInlineInvokeProxy CPH, string name, string description, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Action, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateBoolean(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Boolean, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateColour(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false, bool returnHex = false)
        {
            var type = StreamUpSettingType.Colour;
            if (returnHex) type = StreamUpSettingType.ColourHex;
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = type, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateDouble(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Double, Default = defaultValue, TabName = tabName,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateDropdown(this IInlineInvokeProxy CPH, string name, string description, string[] data, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Dropdown, Data = data, Default = defaultValue, TabName = tabName,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateHeading(this IInlineInvokeProxy CPH, string description, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Description = description, Type = StreamUpSettingType.Heading, TabName = tabName,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateInteger(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Integer, Default = defaultValue, TabName = tabName,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateLabel(this IInlineInvokeProxy CPH, string description, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Description = description, Type = StreamUpSettingType.Label, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

        public static List<StreamUpSetting> SUSettingsCreateLink(this IInlineInvokeProxy CPH, string description, string url, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Description = description, Type = StreamUpSettingType.Link, Url= url, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

        public static List<StreamUpSetting> SUSettingsCreateMultiline(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Name=name, Description = description, Type = StreamUpSettingType.Multiline, Default=defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

        public static List<StreamUpSetting> SUSettingsCreateTrackbar(this IInlineInvokeProxy CPH, string name, string description, int min, int max, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Name=name, Description = description, Type = StreamUpSettingType.TrackBar, Min = min, Max = max, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

        public static List<StreamUpSetting> SUSettingsCreateFile(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Name=name, Description = description, Type = StreamUpSettingType.FileDialog, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

           public static List<StreamUpSetting> SUSettingsCreateFolder(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Name=name, Description = description, Type = StreamUpSettingType.FolderBrowser, Default = defaultValue, TabName = tabName}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

        public static List<StreamUpSetting> SUSettingsCreateReward(this IInlineInvokeProxy CPH, string name, string description, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Reward, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateRuler(this IInlineInvokeProxy CPH, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Type = StreamUpSettingType.Ruler, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateSecret(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Secret, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateSpacer(this IInlineInvokeProxy CPH, string tabName = "General")
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, }
            };

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateString(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, string tabName = "General", bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.String, Default = defaultValue, TabName = tabName, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer, TabName = tabName, });
            }

            return settings;
        }

    }
}