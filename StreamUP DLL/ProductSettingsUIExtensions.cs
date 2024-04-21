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

namespace StreamUP {

    // Settings definition
    // Currently support for 11 different settings types:
    // - StreamUpSettingType.Action
    // - StreamUpSettingType.Boolean
    // - StreamUpSettingType.Colour
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
    //
    // Name = The variable name, Description = The UI label, Type = See above, Default = The default value as a string.



    public static class ProductSettingsUIExtensions {
        public static bool? savePressed = false;
        private static string logName = "DLL::ProductSettingsUI";
        public static bool? SUExecuteSettingsMenu(this IInlineInvokeProxy CPH, ProductInfo productInfo, List<StreamUpSetting> streamUpSettings, IDictionary<string, object> sbArgs, string settingsGlobalName = "ProductSettings")
        {
            // Create loading window
            UIResources.closeLoadingWindow = false;            
            CPH.SUUIShowSettingsLoadingMessage("StreamUP Settings Loading...");

            Dictionary<string, object> productSettings = null;
            string productSettingsJson = CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_ProductSettings", true);
            if (!string.IsNullOrEmpty(productSettingsJson))
            {
                productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(productSettingsJson);
            }

            List<string> sbActions = new List<string>();
            for (int i = 0; i < streamUpSettings.Count; i++) {
                StreamUpSetting item = streamUpSettings[i];
                if (item.Type == StreamUpSettingType.Action) {
                    sbActions = CPH.GetSBActions(sbArgs);
                    if (sbActions.Count == 0) {
                        return false;
                    }
                }
            }

            var settingsForm = new Form();
            settingsForm.Text = $"StreamUP | {productInfo.ProductName}";
            settingsForm.Width = 540;
            settingsForm.Height = 720;
            settingsForm.FormBorderStyle = FormBorderStyle.Fixed3D;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;
            byte[] bytes = Convert.FromBase64String(UIResources.supIconString);
            using var ms = new MemoryStream(bytes);
            settingsForm.Icon = new Icon(ms);
            var description = new Label();
            description.Text = $"Please provide your preferred settings for {productInfo.ProductName} using the controls below.";
            description.MaximumSize = new System.Drawing.Size(498, 0);
            description.AutoSize = true;
            description.Dock = DockStyle.Fill;
            description.Padding = new Padding(0, 4, 0, 0);
            var settingsTable = new TableLayoutPanel();
            settingsTable.Dock = DockStyle.Fill;
            settingsTable.ColumnCount = 2;
            settingsTable.AutoScroll = true;
            settingsTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            settingsTable.Controls.Add(description);
            settingsTable.SetColumnSpan(description, 2);

            for (int i = 0; i < streamUpSettings.Count; i++) {
                StreamUpSetting item = streamUpSettings[i];

                if (item.Type == StreamUpSettingType.Action) {
                    CPH.AddActionSetting(toTable: settingsTable, withSetting: item, withActions: sbActions, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Boolean) {
                    CPH.AddBoolSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Colour) {
                    CPH.AddColorSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Double) {
                    CPH.AddDoubleSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Dropdown) {
                    CPH.AddDropdownSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Heading) {
                    CPH.AddHeadingSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Integer) {
                    CPH.AddIntSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Label) {
                    CPH.AddLabelSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Reward) {
                    CPH.AddRewardSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Ruler) {
                    CPH.AddRulerSettings(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Secret) {
                    CPH.AddSecretSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.Spacer) {
                    CPH.AddSpacerSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
                else if (item.Type == StreamUpSettingType.String) {
                    CPH.AddStringSetting(toTable: settingsTable, withSetting: item, atIndex: i, productSettings);
                }
            }

            CPH.AddButtonControls(toTable: settingsTable, withParent: settingsForm, atIndex: streamUpSettings.Count + 1, streamUpSettings, sbArgs, productInfo, settingsGlobalName);

            settingsTable.AutoScroll = false;
            settingsTable.HorizontalScroll.Enabled = false;
            settingsTable.AutoScroll = true;

            settingsForm.Controls.Add(settingsTable);
            var statusBar = new StatusStrip();
            var statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "© StreamUP";
            statusBar.Items.Add(statusLabel);
            settingsForm.Controls.Add(statusBar);
            
            UIResources.closeLoadingWindow = true;

            settingsForm.ShowDialog();
            CPH.SUWriteLog("Settings menu loaded.", logName);

            return savePressed;
        }
        
        private static List<string> GetSBActions(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs)
        {
            ClientWebSocket ws = new ClientWebSocket();
            var sbActions = new List<string>();

            var wsuri = sbArgs["websocketURI"].ToString();
            try {
                ws.ConnectAsync(new Uri(wsuri), CancellationToken.None).Wait();
            }
            catch {
                System.Windows.Forms.MessageBox.Show("An error occurred while fetching Streamer.bot actions.\nPlease check your Streamer.bot websocket settings, make sure the internal websocket server is turned on and try again.\n\nThe websocket URL and port should match your Streamer.bot instance.", "StreamUP Settings UI - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return sbActions;
            }

            string response = string.Empty;
            string json = string.Empty;
            string data = JsonConvert.SerializeObject(
                new {
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

            if (result.EndOfMessage) {
                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }
            else {
                while (!result.EndOfMessage) {
                    result = ws.ReceiveAsync(bufferSegment, CancellationToken.None).Result;
                    response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
                    json = json + response;
                }

                response = Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count);
            }

            JObject actionJson = JObject.Parse(json.ToString());
            JArray actionName = actionJson.Value<JArray>("actions");
            foreach (JObject item in actionName) {
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
            toTable.Controls.Add(label, 0, atIndex + 1);

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

            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void AddRewardSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var twitchRewards = CPH.TwitchGetRewards();
            List<string> titleList = new List<string>();

            foreach (var reward in twitchRewards) {
                // Extract the "Title" value from the TwitchReward object
                string title = reward.Title;

                // Add the name to the names list
                titleList.Add(title);
            }

            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

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
            foreach (var reward in twitchRewards) {
                if (reward.Id == currentValue) {
                    rewardTitle = reward.Title;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = rewardTitle;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void AddDropdownSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var dropdown = new ComboBox();
            dropdown.Name = withSetting.Name;
            dropdown.Tag = withSetting.Type;
            dropdown.Items.AddRange((string[])withSetting.Data);
            SetComboBoxWidth(dropdown);
            dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            var clearButton = new Button();
            clearButton.Text = "Clear";
            clearButton.Click += (sender, e) => {
                dropdown.SelectedItem = null;
            };

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue)) {
                dropdown.SelectedItem = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                dropdown.SelectedItem = withSetting.Default;
            }

            dropdown.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Right) {
                    dropdown.SelectedItem = null;
                }
            };

            toTable.Controls.Add(dropdown, 1, atIndex + 1);
        }

        private static void SetComboBoxWidth(ComboBox cb, int minWidth = 120, int maxWidth = 245)
        {
            int calculatedWidth = 0;
            int temp = 0;
            foreach (var obj in cb.Items) {
                temp = TextRenderer.MeasureText(obj.ToString(), cb.Font).Width;
                if (temp > calculatedWidth) {
                    calculatedWidth = temp;
                }
            }
            calculatedWidth += 20; // Add padding
            cb.Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));

            cb.MouseWheel += (sender, e) => {
                ((HandledMouseEventArgs)e).Handled = true;
            };
        }

        private static void AddSecretSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;
            textbox.UseSystemPasswordChar = true;

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue)) {
                textbox.Text = currentValue;
            }
            else
            if (!string.IsNullOrEmpty(withSetting.Default)) {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex + 1);
        }

        private static void AddSpacerSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            toTable.Controls.Add(new Label(), 0, atIndex + 1);
            toTable.Controls.Add(new Label(), 1, atIndex + 1);
        }

        private static void AddRulerSettings(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            Label rulerLabel = new Label();
            rulerLabel.BorderStyle = BorderStyle.Fixed3D;
            rulerLabel.Height = 2;
            rulerLabel.Width = 496;
            rulerLabel.Margin = new Padding(10, 10, 10, 10);

            toTable.Controls.Add(rulerLabel, 0, atIndex + 1);
            toTable.SetColumnSpan(rulerLabel, 2);
        }

        private static void AddStringSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var textbox = new TextBox();
            textbox.Name = withSetting.Name;
            textbox.Width = 240;

            string currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = settings[withSetting.Name].ToString();
            }

            if (!string.IsNullOrEmpty(currentValue)) {
                textbox.Text = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                textbox.Text = withSetting.Default;
            }

            toTable.Controls.Add(textbox, 1, atIndex + 1);
        }

        private static void AddColorSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 8, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(270, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var button = new Button();
            button.Text = "Pick a colour";
            button.AutoSize = true;
            button.Name = withSetting.Name;

            long currentValue = 0;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = long.Parse(settings[withSetting.Name].ToString());
            }
            
            if (currentValue != 0) {
                byte a = (byte)((currentValue & 0xff000000) >> 24);
                byte b = (byte)((currentValue & 0x00ff0000) >> 16);
                byte g = (byte)((currentValue & 0x0000ff00) >> 8);
                byte r = (byte)(currentValue & 0x000000ff);

                var colour = System.Drawing.Color.FromArgb(a, r, g, b);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var colour = ColorTranslator.FromHtml(withSetting.Default);
                button.ForeColor = (colour.GetBrightness() < 0.5) ? Color.White : Color.Black;
                button.BackColor = colour;
            }

            button.Click += (sender, e) => {
                var colorDialog = new ColorDialog();
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog() == DialogResult.OK) {
                    button.ForeColor = (colorDialog.Color.GetBrightness() < 0.5) ? Color.White : Color.Black;
                    button.BackColor = colorDialog.Color;
                }
            };
            toTable.Controls.Add(button, 1, atIndex + 1);
        }

        private static void AddBoolSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
            var checkbox = new CheckBox();
            checkbox.Name = withSetting.Name;

            bool? currentValue = null;
            if (settings != null && settings.ContainsKey(withSetting.Name))
            {
                currentValue = bool.Parse(settings[withSetting.Name].ToString());
            }
            
            if (currentValue != null) {
                checkbox.Checked = currentValue.Value;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = bool.TryParse(withSetting.Default, out bool defaultValue);
                checkbox.Checked = hasCorrectDefault ? defaultValue : false;
            }

            toTable.Controls.Add(checkbox, 1, atIndex + 1);
        }

        private static void AddIntSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);
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
 
            if (currentValue != 0) {        
                input.Value = currentValue;
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = int.TryParse(withSetting.Default, out int defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex + 1);
        }

        private static void AddLabelSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(498, 0);

            toTable.Controls.Add(label, 0, atIndex + 1);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddHeadingSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(498, 0);
            label.Font = new Font(label.Font.FontFamily, label.Font.Size + 2, System.Drawing.FontStyle.Bold);

            toTable.Controls.Add(label, 0, atIndex + 1);
            toTable.SetColumnSpan(label, 2);
        }

        private static void AddDoubleSetting(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, StreamUpSetting withSetting, int atIndex, Dictionary<string, object> settings)
        {
            var label = new Label();
            label.Text = withSetting.Description;
            label.Padding = new Padding(0, 4, 0, 0);
            label.AutoSize = true;
            label.MaximumSize = new System.Drawing.Size(250, 0);
            toTable.Controls.Add(label, 0, atIndex + 1);

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
            
            if (currentValue != 0) {
                input.Value = Convert.ToDecimal(currentValue);
            }
            else if (!string.IsNullOrEmpty(withSetting.Default)) {
                var hasCorrectDefault = decimal.TryParse(withSetting.Default, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal defaultValue);
                input.Value = hasCorrectDefault ? defaultValue : 0;
            }

            toTable.Controls.Add(input, 1, atIndex + 1);
        }

        private static void AddButtonControls(this IInlineInvokeProxy CPH, TableLayoutPanel toTable, Form withParent, int atIndex, List<StreamUpSetting> streamUpSettings, IDictionary<string, object> sbArgs, ProductInfo productInfo, string settingsGlobalName)
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
            saveButton.Click += (sender, e) => {
                var twitchRewards = CPH.TwitchGetRewards();
                Dictionary<string, object> settingsToSave = new Dictionary<string, object>();
                
                foreach (Control control in toTable.Controls) {
                    if (!string.IsNullOrEmpty(control.Name)) {
                        object value = null;

                        switch (control) {
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
                        }

                        if (value != null) {
                            settingsToSave[control.Name] = value;
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

            TableLayoutPanel innerTableLayoutPanel = new TableLayoutPanel();
            innerTableLayoutPanel.ColumnCount = 2; // Adjust the number of columns as needed
            innerTableLayoutPanel.RowCount = 1;
            innerTableLayoutPanel.Dock = DockStyle.Fill;

            toTable.Controls.Add(innerTableLayoutPanel, 0, atIndex + 1);
            toTable.SetColumnSpan(innerTableLayoutPanel, 2);

            innerTableLayoutPanel.Controls.Add(resetButton, 0, 0);
            innerTableLayoutPanel.Controls.Add(saveButton, 1, 0);
        }
    }
    public class StreamUpSetting
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public StreamUpSettingType Type { get; set; }

        public string Default { get; set; }

        public object Data { get; set; }
    }

    public enum StreamUpSettingType
    {
        Action,
        Boolean,
        Colour,
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
    }


   public static class ProductSettingsBuilder
    {
        public static List<StreamUpSetting> SUSettingsCreateAction(this IInlineInvokeProxy CPH, string name, string description, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Action, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateBoolean(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Boolean, Default = defaultValue }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }    
        public static List<StreamUpSetting> SUSettingsCreateColour(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Colour, Default = defaultValue,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateDouble(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Double, Default = defaultValue,}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateDropdown(this IInlineInvokeProxy CPH, string name, string description, string[] data, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Dropdown, Data = data, Default = defaultValue}
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateHeading(this IInlineInvokeProxy CPH, string description, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Description = description, Type = StreamUpSettingType.Heading,}           
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateInteger(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Integer, Default = defaultValue, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateLabel(this IInlineInvokeProxy CPH, string description, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Description = description, Type = StreamUpSettingType.Label, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateReward(this IInlineInvokeProxy CPH, string name, string description, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Reward, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateRuler(this IInlineInvokeProxy CPH, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting {Type = StreamUpSettingType.Ruler, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateSecret(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.Secret, Default = defaultValue, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateSpacer(this IInlineInvokeProxy CPH)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Type = StreamUpSettingType.Spacer, }
            };

            return settings;
        }
        public static List<StreamUpSetting> SUSettingsCreateString(this IInlineInvokeProxy CPH, string name, string description, string defaultValue, bool addSpacer = false)
        {
            var settings = new List<StreamUpSetting>
            {
                new StreamUpSetting { Name = name, Description = description, Type = StreamUpSettingType.String, Default = defaultValue, }
            };

            if (addSpacer)
            {
                settings.Add(new StreamUpSetting { Type = StreamUpSettingType.Spacer });
            }

            return settings;
        }
        
    }
}