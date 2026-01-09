using Microsoft.Web.WebView2.Core;
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
using System.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private CoreWebView2 webView2Core;
        private const int MaxObsConnectionIndex = 20;
        private bool hasLoggedObsCheckError;
        private int preferredObsConnectionIndex = -1;
        private string _currentProductNumber = "UNKNOWN";
        private string _currentProductName = "Unknown Product";
        private static ManualResetEvent _menuClosedEvent = new ManualResetEvent(false);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private JObject _initialJson;

        /// <summary>
        /// Open the WebView2-based settings menu for a product.
        /// </summary>
        /// <param name="productName">Display name of the product</param>
        /// <param name="jsonPulling">Initial product data containing productInfo, settings, etc.</param>
        public void OpenSettingsMenuV2(JObject jsonPulling)
        {
            _initialJson = jsonPulling;
            LoadProductInfo(_initialJson);

            // Extract product number for later use
            JObject productInfo = _initialJson?["productInfo"] as JObject;
            if (productInfo != null)
            {
                _currentProductNumber = productInfo["productNumber"]?.ToString() ?? "UNKNOWN";
                _currentProductName = productInfo["productName"]?.ToString() ?? "Unknown Product";
            }

            Thread thread = new Thread(() =>
            {
                try
                {
                    bool lightTheme = IsLightTheme();
                    var form = new Form
                    {
                        Text = $"{_currentProductName} Settings",
                        StartPosition = FormStartPosition.CenterScreen,
                        ClientSize = new Size(800, 900),
                        MinimumSize = new Size(600, 600),
                        BackColor = lightTheme
                            ? Color.FromArgb(245, 245, 245)
                            : Color.FromArgb(32, 32, 32),
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
                            DwmSetWindowAttribute(
                                form.Handle,
                                DWMWA_USE_IMMERSIVE_DARK_MODE,
                                ref useDark,
                                sizeof(int)
                            );
                        }
                    };
                    var webView = new WebView2 { Dock = DockStyle.Fill };
                    form.Controls.Add(webView);
                    form.Load += async (_, __) =>
                    {
                        try
                        {
                            await webView.EnsureCoreWebView2Async(null);
                            webView2Core = webView.CoreWebView2;
                            string css =
                                $@"
                            const s = document.createElement('style');
                            s.textContent = `
                                html, body {{
                                    background-color: {(lightTheme ? "#FFFFFF" : "#202020")} !important;
                                    color: {(lightTheme ? "#000000" : "#E0E0E0")} !important;
                                }}
                            `;
                            document.head.appendChild(s);
                        ";
                            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                                css
                            );
                            webView.CoreWebView2.WebMessageReceived += WebViewOnWebMessageReceived;
                            webView.CoreWebView2.NavigationStarting += WebViewOnNavigationStarting;
                            webView.CoreWebView2.NewWindowRequested += WebViewOnNewWindowRequested;
                            webView.Source = new Uri($"https://viewer.streamup.tips/");

                            // Save product info and obsConnection to file on menu load
                            SaveInitialProductDataV2(_initialJson);
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
                    _menuClosedEvent.Set();
                }
                catch (Exception ex)
                {
                    _CPH.LogError("UI thread crash: " + ex.Message);
                    _menuClosedEvent.Set();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Wait for the settings menu to close. Used to ensure settings are applied before continuing.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds (default 5 minutes)</param>
        /// <returns>True if menu closed, false if timeout</returns>
        public bool WaitForMenuToClose(int timeoutMs = 300000)
        {
            _menuClosedEvent.Reset();
            return _menuClosedEvent.WaitOne(timeoutMs);
        }

        private void WebViewOnWebMessageReceived(
            object sender,
            CoreWebView2WebMessageReceivedEventArgs evt
        )
        {
            try
            {
                var json = evt.WebMessageAsJson;
                var message = JsonConvert.DeserializeObject<JObject>(json);

                if (message?["action"]?.ToString() == "requestInitialData")
                {
                    SendInitialData(_initialJson);
                    return;
                }

                switch (message?["action"]?.ToString())
                {
                    case "loadSettings":
                        LoadAndSendSettingsV2();
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
                    case "executeMethod":
                        ExecuteMethodFromWebView(message);
                        return;
                    case "requestObsData":
                        RequestObsInputData(message);
                        return;
                    case "requestObsScenes":
                        RequestObsSceneData(message);
                        return;
                }

                if (message?["settings"] != null)
                {
                    SaveSettingsV2(message);
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error handling WebView message: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Save settings received from WebView2 to file and cache.
        /// Uses SettingsControllerV2 for all file I/O operations.
        /// After saving, executes any configured save action or execute method.
        /// </summary>
        private void SaveSettingsV2(JObject message)
        {
            try
            {
                var settings = message["settings"] as JObject;
                var productInfo = message["productInfo"] as JObject;
                var obsConnection = message["obsConnection"] ?? 0;

                if (settings == null || productInfo == null)
                {
                    _CPH.LogError("Missing settings or productInfo in message");
                    return;
                }

                string productNumber = productInfo["productNumber"]?.ToString() ?? "UNKNOWN";

                // Load current data to preserve all sections
                JObject currentData = LoadProductDataV2(productNumber);
                if (currentData == null)
                {
                    currentData = new JObject();
                }

                // Update sections with new data
                currentData["productInfo"] = productInfo;
                currentData["obsConnection"] = obsConnection;
                currentData["settings"] = settings;

                // Save complete data structure using SettingsControllerV2
                if (SaveProductDataV2(productNumber, currentData))
                {
                    _CPH.LogInfo($"Settings saved successfully for product {productNumber}");

                    // After successful save, execute any configured save action
                    ExecuteSaveAction(message);
                }
                else
                {
                    _CPH.LogError($"Failed to save settings for product {productNumber}");
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error saving settings: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Execute the save action or execute method if configured in productInfo.
        /// This is called after settings are successfully saved.
        /// </summary>
        private void ExecuteSaveAction(JObject message)
        {
            try
            {
                var productInfo = message["productInfo"] as JObject;
                if (productInfo == null)
                {
                    return;
                }

                // Check for saveAction (run Streamer.bot action)
                var saveAction = productInfo["saveAction"] as JObject;
                if (saveAction != null)
                {
                    string actionName = saveAction["actionName"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(actionName))
                    {
                        _CPH.LogInfo($"Executing save action: {actionName}");
                        try
                        {
                            _CPH.RunAction(actionName);
                            _CPH.LogInfo($"Save action executed successfully: {actionName}");
                        }
                        catch (Exception ex)
                        {
                            _CPH.LogError(
                                $"Error executing save action '{actionName}': {ex.Message}"
                            );
                        }
                        return;
                    }
                }

                // Check for saveExecuteMethod (execute C# method)
                var saveExecuteMethod = productInfo["saveExecuteMethod"] as JObject;
                if (saveExecuteMethod != null)
                {
                    string codeFile = saveExecuteMethod["codeFile"]?.ToString();
                    string methodName = saveExecuteMethod["methodName"]?.ToString();

                    if (
                        !string.IsNullOrWhiteSpace(codeFile)
                        && !string.IsNullOrWhiteSpace(methodName)
                    )
                    {
                        _CPH.LogInfo($"Executing save method: {codeFile}.{methodName}");
                        try
                        {
                            _CPH.ExecuteMethod(codeFile, methodName);
                            _CPH.LogInfo(
                                $"Save method executed successfully: {codeFile}.{methodName}"
                            );
                        }
                        catch (Exception ex)
                        {
                            _CPH.LogError(
                                $"Error executing save method '{codeFile}.{methodName}': {ex.Message}"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error in ExecuteSaveAction: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Load settings from file/cache and send to WebView2.
        /// Uses SettingsControllerV2 for all file I/O operations.
        /// </summary>
        private void LoadAndSendSettingsV2()
        {
            try
            {
                // Use current product number if available
                string productNumber = _currentProductNumber;
                if (productNumber == "UNKNOWN" && _initialJson != null)
                {
                    JObject productInfo = _initialJson["productInfo"] as JObject;
                    if (productInfo != null)
                    {
                        productNumber = productInfo["productNumber"]?.ToString() ?? "UNKNOWN";
                    }
                }

                // Load product data using SettingsControllerV2 (cache-first approach)
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    _CPH.LogInfo("No saved settings found");
                    return;
                }

                // Extract sections from loaded data
                var productInfo2 = data["productInfo"] as JObject;
                var obsConnection = data["obsConnection"] ?? 0;
                var settings = data["settings"] as JObject;

                if (productInfo2 == null)
                {
                    _CPH.LogError("Loaded settings missing productInfo");
                    return;
                }

                if (settings == null)
                {
                    settings = new JObject();
                    _CPH.LogInfo("No user settings found, using empty settings object");
                }

                // Send complete structure to viewer
                var response = new
                {
                    action = "applySettings",
                    productInfo = productInfo2,
                    obsConnection = obsConnection,
                    settings = settings
                };
                string responseJson = JsonConvert.SerializeObject(response);
                webView2Core?.PostWebMessageAsJson(responseJson);
                _CPH.LogInfo($"Loaded and sent settings to UI for product {productNumber}");
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
                    openFileDialog.Filter =
                        "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|All Files|*.*";
                else if (fileType == "audio")
                    openFileDialog.Filter =
                        "Audio Files|*.mp3;*.wav;*.ogg;*.aac;*.m4a|All Files|*.*";
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

        /// <summary>
        /// Handle executeMethod action from WebView2. Executes a Streamer.bot method.
        /// </summary>
        private void ExecuteMethodFromWebView(JObject message)
        {
            try
            {
                string executeCode = message["executeCode"]?.ToString();
                string method = message["method"]?.ToString();

                if (string.IsNullOrWhiteSpace(executeCode) || string.IsNullOrWhiteSpace(method))
                {
                    _CPH.LogError("ExecuteMethod: Missing executeCode or method parameter");
                    return;
                }

                _CPH.LogInfo(
                    $"Executing method from Settings Viewer: Code='{executeCode}', Method='{method}'"
                );
                _CPH.ExecuteMethod(executeCode, method);
                _CPH.LogInfo($"Method executed successfully");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error executing method from WebView: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Handle requestObsData action from WebView2. Fetches OBS input/source data and sends to JavaScript.
        /// Filters sources by type (browser, image, media, etc.) based on sourceTypeFilter parameter.
        /// </summary>
        private void RequestObsInputData(JObject message)
        {
            try
            {
                int obsConnection =
                    (int?)message["obsConnection"]
                    ?? ResolveObsConnectionIndex(GetObsConnectionStates(out _));
                if (obsConnection < 0)
                {
                    _CPH.LogWarn("No active OBS connection available");
                    var errorResponse = new
                    {
                        action = "obsDataUpdated",
                        data = new
                        {
                            connectionStatus = "disconnected",
                            allSources = new string[0],
                            browserSources = new string[0],
                            imageSources = new string[0],
                            mediaSources = new string[0],
                            cameraSources = new string[0],
                            gameCapturesSources = new string[0],
                            windowCaptureSources = new string[0],
                            displayCaptureSources = new string[0]
                        }
                    };
                    string errorJson = JsonConvert.SerializeObject(errorResponse);
                    webView2Core?.PostWebMessageAsJson(errorJson);
                    return;
                }

                // Fetch input list from OBS
                string inputListResponse = _CPH.ObsSendRaw("GetInputList", "{}", obsConnection);
                if (string.IsNullOrWhiteSpace(inputListResponse) || inputListResponse == "{}")
                {
                    _CPH.LogWarn("Empty response from OBS GetInputList");
                    var emptyResponse = new
                    {
                        action = "obsDataUpdated",
                        data = new
                        {
                            connectionStatus = "disconnected",
                            allSources = new string[0],
                            browserSources = new string[0],
                            imageSources = new string[0],
                            mediaSources = new string[0],
                            cameraSources = new string[0],
                            gameCapturesSources = new string[0],
                            windowCaptureSources = new string[0],
                            displayCaptureSources = new string[0]
                        }
                    };
                    string emptyJson = JsonConvert.SerializeObject(emptyResponse);
                    webView2Core?.PostWebMessageAsJson(emptyJson);
                    return;
                }

                JObject inputListObj = JObject.Parse(inputListResponse);
                var inputs = inputListObj["inputs"]?.ToObject<List<JObject>>();

                if (inputs == null || inputs.Count == 0)
                {
                    _CPH.LogWarn("No inputs found in OBS");
                    var noInputsResponse = new
                    {
                        action = "obsDataUpdated",
                        data = new
                        {
                            connectionStatus = "connected",
                            allSources = new string[0],
                            browserSources = new string[0],
                            imageSources = new string[0],
                            mediaSources = new string[0],
                            cameraSources = new string[0],
                            gameCapturesSources = new string[0],
                            windowCaptureSources = new string[0],
                            displayCaptureSources = new string[0]
                        }
                    };
                    string noInputsJson = JsonConvert.SerializeObject(noInputsResponse);
                    webView2Core?.PostWebMessageAsJson(noInputsJson);
                    return;
                }

                // Extract all source names
                var allSources = inputs
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                // Filter by input kind
                var browserSources = inputs
                    .Where(i => i["inputKind"]?.ToString() == "browser_source")
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var imageSources = inputs
                    .Where(i => i["inputKind"]?.ToString() == "image_source")
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var mediaSources = inputs
                    .Where(
                        i =>
                            new[] { "ffmpeg_source", "vlc_source" }.Contains(
                                i["inputKind"]?.ToString()
                            )
                    )
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var cameraSources = inputs
                    .Where(
                        i =>
                            new[] { "dshow_input", "av_capture_input", "v4l2_input" }.Contains(
                                i["inputKind"]?.ToString()
                            )
                    )
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var gameCapturesSources = inputs
                    .Where(i => i["inputKind"]?.ToString() == "game_capture")
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var windowCaptureSources = inputs
                    .Where(i => i["inputKind"]?.ToString() == "window_capture")
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                var displayCaptureSources = inputs
                    .Where(
                        i =>
                            new[] { "monitor_capture", "xcomposite_input" }.Contains(
                                i["inputKind"]?.ToString()
                            )
                    )
                    .Select(i => i["inputName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                // Build response
                var responseData = new
                {
                    action = "obsDataUpdated",
                    data = new
                    {
                        connectionStatus = "connected",
                        allSources = allSources,
                        browserSources = browserSources,
                        imageSources = imageSources,
                        mediaSources = mediaSources,
                        cameraSources = cameraSources,
                        gameCapturesSources = gameCapturesSources,
                        windowCaptureSources = windowCaptureSources,
                        displayCaptureSources = displayCaptureSources
                    }
                };

                string responseJson = JsonConvert.SerializeObject(responseData);
                webView2Core?.PostWebMessageAsJson(responseJson);
                _CPH.LogInfo("Sent OBS input data to WebView");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error requesting OBS input data: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Handle requestObsScenes action from WebView2. Fetches OBS scene list and sends to JavaScript.
        /// Filters out groups to return only actual scenes.
        /// </summary>
        private void RequestObsSceneData(JObject message)
        {
            try
            {
                _CPH.LogInfo("RequestObsSceneData called");
                int obsConnection =
                    (int?)message["obsConnection"]
                    ?? ResolveObsConnectionIndex(GetObsConnectionStates(out _));

                _CPH.LogInfo($"Resolved OBS connection: {obsConnection}");

                if (obsConnection < 0)
                {
                    _CPH.LogWarn("No active OBS connection available");
                    var errorResponse = new
                    {
                        action = "obsScenesUpdated",
                        data = new
                        {
                            connectionStatus = "disconnected",
                            scenes = new string[0]
                        }
                    };
                    string errorJson = JsonConvert.SerializeObject(errorResponse);
                    webView2Core?.PostWebMessageAsJson(errorJson);
                    return;
                }

                // Use existing GetObsSceneList method
                if (!GetObsSceneList(obsConnection, out JObject sceneList))
                {
                    _CPH.LogWarn("Failed to retrieve scene list from OBS");
                    var emptyResponse = new
                    {
                        action = "obsScenesUpdated",
                        data = new
                        {
                            connectionStatus = "connected",
                            scenes = new string[0]
                        }
                    };
                    string emptyJson = JsonConvert.SerializeObject(emptyResponse);
                    webView2Core?.PostWebMessageAsJson(emptyJson);
                    return;
                }

                var scenes = sceneList["scenes"]?.ToObject<List<JObject>>();
                if (scenes == null || scenes.Count == 0)
                {
                    _CPH.LogInfo("No scenes found in OBS");
                    var noScenesResponse = new
                    {
                        action = "obsScenesUpdated",
                        data = new
                        {
                            connectionStatus = "connected",
                            scenes = new string[0]
                        }
                    };
                    string noScenesJson = JsonConvert.SerializeObject(noScenesResponse);
                    webView2Core?.PostWebMessageAsJson(noScenesJson);
                    return;
                }

                // Filter out groups (isGroup: true) - user wants scenes only
                var nonGroupScenes = scenes
                    .Where(s =>
                    {
                        bool? isGroup = (bool?)s["isGroup"];
                        return !isGroup.HasValue || !isGroup.Value;
                    })
                    .Select(s => s["sceneName"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .OrderBy(n => n) // Sort alphabetically
                    .ToList();

                // Build and send response
                _CPH.LogInfo($"Sending {nonGroupScenes.Count} scenes to WebView: {string.Join(", ", nonGroupScenes)}");
                var responseData = new
                {
                    action = "obsScenesUpdated",
                    data = new
                    {
                        connectionStatus = "connected",
                        scenes = nonGroupScenes
                    }
                };

                string responseJson = JsonConvert.SerializeObject(responseData);
                _CPH.LogInfo($"Response JSON: {responseJson}");
                webView2Core?.PostWebMessageAsJson(responseJson);
                _CPH.LogInfo($"Message posted to WebView");
            }
            catch (Exception ex)
            {
                _CPH.LogError("Error requesting OBS scene data: " + ex.Message);
                _CPH.LogError("Stack trace: " + ex.StackTrace);
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
            if (
                preferredObsConnectionIndex >= 0
                && states.TryGetValue(preferredObsConnectionIndex, out bool preferredConnected)
                && preferredConnected
            )
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
                var token =
                    message?["preferredIndex"] ?? message?["index"] ?? message?["connectionId"];
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

                // Save the OBS connection to file immediately
                SaveObsConnectionV2(_currentProductNumber, preferredObsConnectionIndex);

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

                var productInfoObj = jsonData["productInfo"] as JObject;
                if (productInfoObj == null)
                {
                    _CPH.LogError("JSON data missing productInfo object");
                    return false;
                }

                var productInfo = new ProductInfo
                {
                    ProductName = productInfoObj["productName"]?.ToString() ?? "Unknown",
                    ProductNumber = productInfoObj["productNumber"]?.ToString() ?? "UNKNOWN",
                    ProductVersionNumber = ParseVersionString(
                        productInfoObj["productVersion"]?.ToString()
                    ),
                    RequiredLibraryVersion = ParseVersionString(
                        productInfoObj["libraryVersion"]?.ToString()
                    ),
                    SceneName = productInfoObj["sceneName"]?.ToString(),
                    SourceNameVersionNumber = ParseVersionString(
                        productInfoObj["sceneVersion"]?.ToString()
                    ),
                    SettingsAction = productInfoObj["settingsAction"]?.ToString(),
                };

                _CPH.SetGlobalVar($"{productInfo.ProductNumber}_ProductInfo", productInfo, false);
                _CPH.LogInfo(
                    $"ProductInfo loaded: {productInfo.ProductName} ({productInfo.ProductNumber})"
                );
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

        /// <summary>
        /// Handle navigation in WebView2. External links are opened in the default browser instead.
        /// Only allow navigation to the viewer.streamup.tips domain within the WebView.
        /// </summary>
        private void WebViewOnNavigationStarting(
            object sender,
            CoreWebView2NavigationStartingEventArgs e
        )
        {
            try
            {
                if (string.IsNullOrEmpty(e.Uri))
                {
                    return;
                }

                Uri navigationUri = new Uri(e.Uri);

                // Allow navigation only to viewer.streamup.tips domain
                if (navigationUri.Host != "viewer.streamup.tips")
                {
                    // Cancel navigation and open in default browser instead
                    e.Cancel = true;

                    try
                    {
                        _CPH.LogDebug($"Opening external link in default browser: {e.Uri}");
                        System.Diagnostics.Process.Start(
                            new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = e.Uri,
                                UseShellExecute = true
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        _CPH.LogError($"Failed to open link in default browser: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError($"Error handling WebView navigation: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle new window requests (links with target="_blank"). Open external links in default browser.
        /// </summary>
        private void WebViewOnNewWindowRequested(
            object sender,
            CoreWebView2NewWindowRequestedEventArgs e
        )
        {
            try
            {
                if (string.IsNullOrEmpty(e.Uri))
                {
                    return;
                }

                Uri navigationUri = new Uri(e.Uri);

                // Only allow new windows for viewer.streamup.tips domain
                if (navigationUri.Host != "viewer.streamup.tips")
                {
                    // Prevent new window and open in default browser instead
                    e.Handled = true;

                    try
                    {
                        _CPH.LogDebug($"Opening external link in default browser: {e.Uri}");
                        System.Diagnostics.Process.Start(
                            new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = e.Uri,
                                UseShellExecute = true
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        _CPH.LogError($"Failed to open link in default browser: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError($"Error handling new window request: {ex.Message}");
            }
        }

        /// <summary>
        /// Save initial product info and OBS connection when settings menu loads.
        /// This ensures the current state is saved to file before user makes any changes.
        /// </summary>
        private void SaveInitialProductDataV2(JObject initialData)
        {
            try
            {
                if (initialData == null)
                {
                    _CPH.LogWarn("Initial data is null, cannot save");
                    return;
                }

                var productInfo = initialData["productInfo"] as JObject;
                if (productInfo == null)
                {
                    _CPH.LogWarn("Product info not found in initial data");
                    return;
                }

                string productNumber = productInfo["productNumber"]?.ToString();
                if (string.IsNullOrEmpty(productNumber))
                {
                    _CPH.LogWarn("Product number not found in initial data");
                    return;
                }

                // Get current data to preserve settings
                JObject currentData = LoadProductDataV2(productNumber);
                if (currentData == null)
                {
                    currentData = new JObject();
                }

                // Update with initial product info and OBS connection
                currentData["productInfo"] = productInfo;

                // Extract obsConnection from initial data (default to 0 if not present)
                int obsConnection = (int?)initialData["obsConnection"] ?? 0;
                currentData["obsConnection"] = obsConnection;

                // Preserve existing settings if they exist
                if (currentData["settings"] == null)
                {
                    currentData["settings"] = new JObject();
                }

                // Save to file
                if (SaveProductDataV2(productNumber, currentData))
                {
                    _CPH.LogInfo($"Initial product data saved on menu load for {productNumber}");
                }
                else
                {
                    _CPH.LogWarn($"Failed to save initial product data for {productNumber}");
                }
            }
            catch (Exception ex)
            {
                _CPH.LogError($"Error saving initial product data: {ex.Message}");
                _CPH.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
