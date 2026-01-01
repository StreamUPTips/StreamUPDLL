using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Streamer.bot.Plugin.Interface.Model;
using Newtonsoft.Json.Serialization;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private CoreWebView2 webView2Core;
        private const int MaxObsConnectionIndex = 20;
        private bool hasLoggedObsCheckError;
        private int preferredObsConnectionIndex = -1;
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private JObject _initialJson;


        public void OpenSettingsMenu(string productName, JObject jsonPulling)
        {
            _initialJson = jsonPulling;
            LoadProductInfo(_initialJson);
            Thread thread = new Thread(() =>
            {
                try
                {
                    bool lightTheme = IsLightTheme();
                    var form = new Form
                    {
                        Text = $"{productName} Settings",
                        StartPosition = FormStartPosition.CenterScreen,
                        ClientSize = new Size(800, 900),
                        MinimumSize = new Size(600, 600),
                        BackColor = lightTheme ? Color.FromArgb(245, 245, 245) : Color.FromArgb(32, 32, 32),
                        ForeColor = lightTheme ? Color.Black : Color.FromArgb(224, 224, 224),
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
                    };
                    var appIcon = TryLoadStreamerBotIcon();
                    if (appIcon != null)
                    {
                        form.Icon = appIcon;
                    }

                    form.HandleCreated += (_, __) =>
                    {
                        if (!lightTheme)
                        {
                            int useDark = 1;
                            DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                        }
                    };
                    var webView = new WebView2
                    {
                        Dock = DockStyle.Fill
                    };
                    form.Controls.Add(webView);
                    form.Load += async (_, __) =>
                    {
                        try
                        {
                            await webView.EnsureCoreWebView2Async(null);
                            webView2Core = webView.CoreWebView2;
                            string css = $@"
                            const s = document.createElement('style');
                            s.textContent = `
                                html, body {{
                                    background-color: {(lightTheme ? "#FFFFFF" : "#202020")} !important;
                                    color: {(lightTheme ? "#000000" : "#E0E0E0")} !important;
                                }}
                            `;
                            document.head.appendChild(s);
                        ";
                            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(css);
                            webView.CoreWebView2.WebMessageReceived += WebViewOnWebMessageReceived;
                            webView.Source = new Uri($"https://viewer.streamup.tips/"); //! THIS IS THE WEBSITE URL
                        }
                        catch (Exception ex)
                        {
                            _CPH.LogError("WebView init exception: " + ex.Message);
                            if (!form.IsDisposed)
                            {
                                form.Close();
                            }
                        }
                    };
                    Application.Run(form);
                }
                catch (Exception ex)
                {
                    _CPH.LogError("UI thread crash: " + ex.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WebViewOnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs evt)
        {
            try
            {
                var json = evt.WebMessageAsJson;
                var message = JsonConvert.DeserializeObject<JObject>(json);
                // ======================================================
                // 🔥 OPTION 3 — Webpage Requests Initial Actions/Groups/Rewards
                // ======================================================
                if (message?["action"]?.ToString() == "requestInitialData")
                {
                    SendInitialData(_initialJson);
                    return;
                }

                // ======================================================
                switch (message?["action"]?.ToString())
                {
                    case "loadSettings":
                        LoadAndSendSettings();
                        return;
                    case "selectFile":
                        SelectFile(message);
                        return;
                    case "requestConnectionStatus":
                        SendConnectionStatus();
                        return;
                    case "retryConnections":
                        HandleRetryConnections();
                        return;
                    case "setObsConnectionIndex":
                        UpdatePreferredObsConnection(message);
                        return;
                }

                if (message?["settings"] != null)
                {
                    SaveSettings(message);
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error handling WebView message: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        private void SaveSettings(JObject message)
        {
            try
            {
                var settings = message["settings"] as JObject;
                string productNumber = message["productNumber"]?.ToString() ?? "UNKNOWN";
                if (settings == null)
                {
                    _CPH.LogError("No settings found in message");
                    return;
                }

                var cleanedValues = new Dictionary<string, object>();
                foreach (var kvp in settings)
                {
                    object valueToSave = ConvertJTokenToNative(kvp.Value);
                    cleanedValues[kvp.Key] = valueToSave;
                }

                string streamerBotFolder = AppDomain.CurrentDomain.BaseDirectory;
                string streamUpFolder = Path.Combine(streamerBotFolder, "StreamUP", "Data");
                Directory.CreateDirectory(streamUpFolder);
                string fileName = $"{productNumber}_ProductSettings.json";
                string filePath = Path.Combine(streamUpFolder, fileName);
                string jsonOutput = JsonConvert.SerializeObject(cleanedValues, Formatting.Indented);
                File.WriteAllText(filePath, jsonOutput);
                _CPH.LogInfo($"Settings saved successfully to: {filePath}");
                _CPH.LogInfo($"Saved {cleanedValues.Count} settings for product {productNumber}");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error saving settings: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        private void LoadAndSendSettings()
        {
            try
            {
                string filePath = ResolveSettingsFilePath();
                if (filePath == null)
                {
                    _CPH.LogInfo("No saved settings files found");
                    return;
                }

                _CPH.LogInfo($"Loading settings from: {filePath}");
                string jsonContent = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
                var response = new
                {
                    action = "applySettings",
                    settings = settings
                };
                string responseJson = JsonConvert.SerializeObject(response);
                webView2Core?.PostWebMessageAsJson(responseJson);
                _CPH.LogInfo($"Loaded and sent {settings.Count} settings to UI");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error loading settings: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        private void SelectFile(JObject message)
        {
            try
            {
                string uniqueId = message["uniqueId"]?.ToString();
                string fileType = message["fileType"]?.ToString();
                var openFileDialog = new System.Windows.Forms.OpenFileDialog();
                if (fileType == "image")
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|All Files|*.*";
                else if (fileType == "audio")
                    openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.ogg;*.aac;*.m4a|All Files|*.*";
                else
                    openFileDialog.Filter = "All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var response = new
                    {
                        action = "filePathSelected",
                        inputId = uniqueId,
                        path = openFileDialog.FileName
                    };
                    string responseJson = JsonConvert.SerializeObject(response);
                    webView2Core?.PostWebMessageAsJson(responseJson);
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error selecting file: " + ex.Message);
            }
        }

        private object ConvertJTokenToNative(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            switch (token.Type)
            {
                case JTokenType.Integer:
                    return (long)token;
                case JTokenType.Float:
                    return (double)token;
                case JTokenType.Boolean:
                    return (bool)token;
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in ((JObject)token).Properties())
                    {
                        dict[prop.Name] = ConvertJTokenToNative(prop.Value);
                    }

                    return dict;
                case JTokenType.Array:
                    var list = new List<object>();
                    foreach (var item in (JArray)token)
                    {
                        list.Add(ConvertJTokenToNative(item));
                    }

                    return list;
                default:
                    return token.ToString();
            }
        }

        private bool IsLightTheme()
        {
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            try
            {
                using var subKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);
                return (int?)subKey?.GetValue("AppsUseLightTheme", 1) == 1;
            }
            catch
            {
                return true;
            }
        }

        private Icon TryLoadStreamerBotIcon()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDir, "Streamer.bot.exe");
                if (File.Exists(exePath))
                {
                    return Icon.ExtractAssociatedIcon(exePath);
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Unable to load Streamer.bot icon: " + ex.Message);
            }

            return null;
        }

        private string ResolveSettingsFilePath()
        {
            string streamerBotFolder = AppDomain.CurrentDomain.BaseDirectory;
            string streamUpFolder = Path.Combine(streamerBotFolder, "StreamUP", "Data");
            if (!Directory.Exists(streamUpFolder))
            {
                return null;
            }

            var settingsFiles = Directory.GetFiles(streamUpFolder, "*_ProductSettings.json");
            return settingsFiles.Length > 0 ? settingsFiles[0] : null;
        }

        private string TryLoadProductName()
        {
            try
            {
                string filePath = ResolveSettingsFilePath();
                if (filePath == null)
                {
                    return null;
                }

                string jsonContent = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<JObject>(jsonContent);
                return settings?["productName"]?.ToString() ?? settings?["ProductName"]?.ToString();
            }
            catch (Exception ex)
            {
                _CPH.LogError("Unable to read product name from settings: " + ex.Message);
                return null;
            }
        }

        private void SendInitialData(JObject jsonString)
        {
            try
            {
                var data = new
                {
                    action = "initialData",
                    actions = _CPH.GetActions(),
                    commands = _CPH.GetCommands(),
                    groups = _CPH.GetGroups(),
                    rewards = _CPH.TwitchGetRewards(),
                    connectionStatus = BuildConnectionStatusPayload(),
                    json = jsonString.ToString()
                };
                string json = JsonConvert.SerializeObject(data);
                webView2Core?.PostWebMessageAsJson(json);
                _CPH.LogInfo("Sent actions/groups/rewards to WebView");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error sending initial data: " + ex.Message);
            }
        }

        private void SendConnectionStatus()
        {
            try
            {
                var payload = new
                {
                    action = "connectionStatus",
                    status = BuildConnectionStatusPayload()
                };
                string json = JsonConvert.SerializeObject(payload);
                webView2Core?.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error sending connection status: " + ex.Message);
            }
        }

        private void HandleRetryConnections()
        {
            _CPH.LogInfo("Manual connection retry requested from UI");
            SendConnectionStatus();
        }

        private object BuildConnectionStatusPayload()
        {
            List<object> serializedConnections;
            var obsStates = GetObsConnectionStates(out serializedConnections);
            int resolvedIndex = ResolveObsConnectionIndex(obsStates);
            bool streamerBotConnected = webView2Core != null;
            return new
            {
                streamerBotConnected,
                obsConnected = resolvedIndex >= 0,
                obsConnectionId = resolvedIndex,
                preferredObsConnectionId = preferredObsConnectionIndex,
                obsConnections = serializedConnections
            };
        }

        private Dictionary<int, bool> GetObsConnectionStates(out List<object> serializedConnections)
        {
            var states = new Dictionary<int, bool>();
            serializedConnections = new List<object>();
            for (int i = 0; i <= MaxObsConnectionIndex; i++)
            {
                bool connected = IsObsConnectionSlotActive(i);
                states[i] = connected;
                serializedConnections.Add(new { id = i, connected });
            }

            return states;
        }

        private int ResolveObsConnectionIndex(Dictionary<int, bool> states)
        {
            if (preferredObsConnectionIndex >= 0 && states.TryGetValue(preferredObsConnectionIndex, out bool preferredConnected) && preferredConnected)
            {
                return preferredObsConnectionIndex;
            }

            foreach (var kvp in states)
            {
                if (kvp.Value)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        private void UpdatePreferredObsConnection(JObject message)
        {
            try
            {
                var token = message?["preferredIndex"] ?? message?["index"] ?? message?["connectionId"];
                if (token == null)
                {
                    return;
                }

                if (!int.TryParse(token.ToString(), out int parsed))
                {
                    return;
                }

                if (parsed < 0)
                {
                    preferredObsConnectionIndex = -1;
                }
                else if (parsed <= MaxObsConnectionIndex)
                {
                    preferredObsConnectionIndex = parsed;
                }
                else
                {
                    preferredObsConnectionIndex = -1;
                }

                _CPH.LogInfo($"Preferred OBS connection set to {preferredObsConnectionIndex}");
                SendConnectionStatus();
            }
            catch (Exception ex)
            {
                _CPH.LogError("Unable to update preferred OBS connection: " + ex.Message);
            }
        }

        private bool IsObsConnectionSlotActive(int connectionNumber)
        {
            try
            {
                return _CPH.ObsIsConnected(connectionNumber);
            }
            catch (Exception ex)
            {
                if (!hasLoggedObsCheckError)
                {
                    _CPH.LogError($"ObsIsConnected({connectionNumber}) failed: {ex.Message}");
                    hasLoggedObsCheckError = true;
                }

                return false;
            }
        }

        private bool LoadProductInfo(JObject jsonData)
        {
            try
            {
                if (jsonData == null)
                {
                    _CPH.LogError("JSON data is null, cannot load ProductInfo");
                    return false;
                }

                var productInfo = new ProductInfo
                {
                    ProductName = jsonData["productName"]?.ToString() ?? "Unknown",
                    ProductNumber = jsonData["productNumber"]?.ToString() ?? "UNKNOWN",
                    ProductVersionNumber = ParseVersionString(jsonData["productVersion"]?.ToString()),
                    RequiredLibraryVersion = ParseVersionString(jsonData["libraryVersion"]?.ToString()),
                    SceneName = jsonData["sceneName"]?.ToString(),
                    SourceNameVersionNumber = ParseVersionString(jsonData["sceneVersion"]?.ToString()),
                    SettingsAction = jsonData["settingsAction"]?.ToString(),
                };

                _CPH.SetGlobalVar($"{productInfo.ProductNumber}_ProductInfo", productInfo, false);
                _CPH.LogInfo($"ProductInfo loaded: {productInfo.ProductName} ({productInfo.ProductNumber})");
                return true;
            }
            catch (Exception ex)
            {
                _CPH.LogError($"Error loading ProductInfo: {ex.Message}");
                return false;
            }
        }

        private Version ParseVersionString(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return new Version(0, 0, 0, 0);

            if (Version.TryParse(versionString, out var version))
                return version;

            return new Version(0, 0, 0, 0);
        }

    }
}
