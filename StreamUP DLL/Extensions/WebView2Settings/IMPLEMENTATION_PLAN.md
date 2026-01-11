# WebView2 Settings Implementation Plan

This document outlines the complete implementation plan for integrating WebView2-based settings into the StreamUP DLL.

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Data Flow](#2-data-flow)
3. [DLL Implementation](#3-dll-implementation)
4. [Viewer Updates](#4-viewer-updates)
5. [Helper Methods API](#5-helper-methods-api)
6. [OBS Connection Selector](#6-obs-connection-selector)
7. [Caching Strategy](#7-caching-strategy)
8. [Window Behavior](#8-window-behavior)
9. [File Structure](#9-file-structure)
10. [Implementation Order](#10-implementation-order)

---

## 1. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  STREAMER.BOT PRODUCT (C# Execute Code)                                     │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  var sup = new StreamUpLib(CPH, "sup001");                           │   │
│  │  sup.OpenSettingsMenu(settingsJsonObject);                           │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────┬───────────────────────────────────────────────────┘
                          │ JObject (settings menu definition)
                          ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  STREAMUP DLL                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  WebView2Settings Extension                                          │   │
│  │                                                                      │   │
│  │  1. Fetch live data from Streamer.bot via _CPH:                     │   │
│  │     - Actions, Commands, User Groups, Channel Point Rewards          │   │
│  │                                                                      │   │
│  │  2. Fetch live data from OBS via existing ObsExtensions:            │   │
│  │     - Available OBS connections                                      │   │
│  │     - Scenes, Sources, Filters (per selected connection)            │   │
│  │                                                                      │   │
│  │  3. Load saved settings from JSON file                              │   │
│  │                                                                      │   │
│  │  4. Open WebView2 window → viewer.streamup.tips                     │   │
│  │                                                                      │   │
│  │  5. Inject all data into viewer                                     │   │
│  │                                                                      │   │
│  │  6. Handle messages from viewer (save, file dialogs, close)         │   │
│  │                                                                      │   │
│  │  7. Save to JSON file + update static cache                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  Helper Methods (callable from any product action):                        │
│  - GetProductSetting<T>(key, default) → reads from cache/file              │
│  - SetProductSetting(key, value) → writes to cache/file                    │
│  - GetAllProductSettings() → returns full settings JObject                  │
└─────────────────────────┬───────────────────────────────────────────────────┘
                          │ Injects via ExecuteScriptAsync
                          ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  SETTINGS VIEWER (viewer.streamup.tips)                                     │
│                                                                             │
│  Receives via window globals:                                              │
│  - STREAMUP_CONFIG (settings menu JSON)                                    │
│  - STREAMUP_SAVED_SETTINGS (previously saved values)                       │
│  - STREAMUP_PRODUCT_DATA (OBS instance, etc.)                              │
│  - STREAMUP_LIVE_DATA (OBS scenes/sources, SB actions/commands, etc.)      │
│                                                                             │
│  Sends via chrome.webview.postMessage():                                   │
│  - save: { data: {...} }                                                   │
│  - close: {}                                                               │
│  - openFileDialog: { callbackId, filter, title }                          │
│  - requestObsData: { connectionIndex } (when user changes OBS connection)  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Data Flow

### 2.1 Opening Settings

```
Product calls sup.OpenSettingsMenu(json)
         │
         ▼
┌─────────────────────────────┐
│ DLL: Fetch Streamer.bot data │
│ - _CPH.GetActions()          │
│ - _CPH.GetCommands()         │
│ - TwitchGetRewards()         │
│ - GetGroups()                │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Get OBS connections     │
│ - Check which are connected  │
│ - List available connections │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Load saved settings     │
│ - Read from cache (if valid) │
│ - Or read from JSON file     │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Open WebView2 window    │
│ - Navigate to viewer URL     │
│ - Wait for navigation done   │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Inject data into viewer │
│ - STREAMUP_CONFIG            │
│ - STREAMUP_SAVED_SETTINGS    │
│ - STREAMUP_LIVE_DATA         │
│ - STREAMUP_PRODUCT_DATA      │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ Viewer: Render settings UI   │
│ - Apply saved values         │
│ - Populate dropdowns         │
└─────────────────────────────┘
```

### 2.2 Saving Settings

```
User clicks Save in viewer
         │
         ▼
┌─────────────────────────────┐
│ Viewer: Collect all values   │
│ - Build settings object      │
│ - Add meta information       │
└──────────────┬──────────────┘
               │
               ▼
chrome.webview.postMessage({
  action: "save",
  data: { meta: {...}, settings: {...} }
})
               │
               ▼
┌─────────────────────────────┐
│ DLL: OnWebMessageReceived    │
│ - Parse message              │
│ - Extract settings           │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Save to file            │
│ - Write JSON to disk         │
│ - Update static cache        │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Send success to viewer  │
│ - Show toast notification    │
└─────────────────────────────┘
```

### 2.3 OBS Connection Change

When user selects a different OBS connection in the viewer:

```
User selects OBS connection #2
         │
         ▼
chrome.webview.postMessage({
  action: "requestObsData",
  connectionIndex: 2
})
         │
         ▼
┌─────────────────────────────┐
│ DLL: Fetch OBS data          │
│ - GetObsSceneList(2)         │
│ - Get sources per scene      │
│ - Get filters                │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ DLL: Send to viewer          │
│ - ExecuteScriptAsync         │
│ - window.updateObsData({...})│
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│ Viewer: Update dropdowns     │
│ - Refresh scene list         │
│ - Refresh source list        │
│ - Refresh filter list        │
└─────────────────────────────┘
```

---

## 3. DLL Implementation

### 3.1 File Structure

```
Extensions/WebView2Settings/
├── WebView2SettingsWindow.cs      # Main WebView2 Form window
├── WebView2SettingsMethods.cs     # Public methods (OpenSettingsMenu, etc.)
├── WebView2SettingsHelpers.cs     # Helper methods (GetProductSetting, etc.)
├── WebView2SettingsCache.cs       # Static caching logic
├── WebView2SettingsDataFetcher.cs # Fetch SB and OBS data
├── WebView2SettingsMessages.cs    # Message handling (save, dialogs, etc.)
└── WebView2SettingsModels.cs      # Data models/classes
```

### 3.2 WebView2SettingsWindow.cs

```csharp
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private static Form _settingsWindow;
        private static WebView2 _webView;
        private static bool _hasUnsavedChanges = false;
        private static JObject _currentConfig;
        private static JObject _currentSavedSettings;
        private static string _currentProductNumber;

        // Window position/size persistence
        private static Point? _lastWindowLocation;
        private static Size? _lastWindowSize;

        private const string VIEWER_URL = "https://viewer.streamup.tips/";
        // For dev: private const string VIEWER_URL = "http://localhost:8080/";

        /// <summary>
        /// Opens the WebView2 settings window for a product
        /// </summary>
        public async Task OpenSettingsMenu(JObject settingsConfig)
        {
            LogInfo("Opening WebView2 settings menu");

            try
            {
                // Extract product info
                _currentProductNumber = settingsConfig["productNumber"]?.ToString() ?? _ProductIdentifier;
                _currentConfig = settingsConfig;

                // Load saved settings
                _currentSavedSettings = LoadProductSettings(_currentProductNumber);

                // Fetch live data
                var liveData = await FetchLiveDataAsync();

                // Create or reuse window
                if (_settingsWindow == null || _settingsWindow.IsDisposed)
                {
                    await CreateSettingsWindow();
                }

                // Update window title
                var productName = settingsConfig["productName"]?.ToString() ?? "Settings";
                _settingsWindow.Text = $"{productName} - Settings";

                // Navigate and inject data
                await NavigateAndInjectData(liveData);

                // Show window
                _settingsWindow.Show();
                _settingsWindow.Activate();
                _settingsWindow.BringToFront();
            }
            catch (Exception ex)
            {
                LogError($"Failed to open settings menu: {ex.Message}");
                throw;
            }
        }

        private async Task CreateSettingsWindow()
        {
            LogInfo("Creating WebView2 settings window");

            _settingsWindow = new Form
            {
                Text = "StreamUP Settings",
                Width = _lastWindowSize?.Width ?? 900,
                Height = _lastWindowSize?.Height ?? 700,
                MinimumSize = new Size(600, 400),
                StartPosition = _lastWindowLocation.HasValue
                    ? FormStartPosition.Manual
                    : FormStartPosition.CenterScreen,
                Icon = GetStreamUpIcon(), // Load from resources
                BackColor = Color.FromArgb(26, 31, 53) // StreamUP midnight
            };

            if (_lastWindowLocation.HasValue)
            {
                _settingsWindow.Location = _lastWindowLocation.Value;
            }

            // Set dark title bar
            SetDarkTitleBar(_settingsWindow.Handle);

            // Handle window events
            _settingsWindow.FormClosing += OnSettingsWindowClosing;
            _settingsWindow.Move += (s, e) => _lastWindowLocation = _settingsWindow.Location;
            _settingsWindow.Resize += (s, e) => {
                if (_settingsWindow.WindowState == FormWindowState.Normal)
                    _lastWindowSize = _settingsWindow.Size;
            };

            // Create WebView2
            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            _settingsWindow.Controls.Add(_webView);

            // Initialize WebView2
            var dataPath = Path.Combine(GetStreamerBotFolder(), "StreamUP", "Data");
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(dataPath, "WebView2Cache")
            );

            await _webView.EnsureCoreWebView2Async(env);

            // Configure WebView2 settings
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            // Set up message handler
            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        }

        private async Task NavigateAndInjectData(JObject liveData)
        {
            LogInfo("Navigating to viewer and injecting data");

            var tcs = new TaskCompletionSource<bool>();

            void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                _webView.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
                tcs.SetResult(e.IsSuccess);
            }

            _webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            _webView.CoreWebView2.Navigate(VIEWER_URL);

            var success = await tcs.Task;
            if (!success)
            {
                LogError("Navigation to viewer failed");
                throw new Exception("Failed to load settings viewer");
            }

            // Build product data (includes OBS instance number from saved settings)
            var productData = new JObject
            {
                ["obsInstanceNumber"] = _currentSavedSettings?["productData"]?["obsInstanceNumber"] ?? 0
            };

            // Inject all data
            var initScript = $@"
                window.STREAMUP_MODE = 'production';
                window.STREAMUP_CONFIG = {_currentConfig.ToString(Formatting.None)};
                window.STREAMUP_SAVED_SETTINGS = {_currentSavedSettings?.ToString(Formatting.None) ?? "null"};
                window.STREAMUP_PRODUCT_DATA = {productData.ToString(Formatting.None)};
                window.STREAMUP_LIVE_DATA = {liveData.ToString(Formatting.None)};
                window.dispatchEvent(new Event('streamup-init'));
            ";

            await _webView.CoreWebView2.ExecuteScriptAsync(initScript);
            _hasUnsavedChanges = false;

            LogInfo("Data injected successfully");
        }

        private void OnSettingsWindowClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            LogInfo("Settings window closed");
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private void SetDarkTitleBar(IntPtr handle)
        {
            try
            {
                var attribute = 20; // DWMWA_USE_IMMERSIVE_DARK_MODE
                var useImmersiveDarkMode = 1;
                DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int));
            }
            catch (Exception ex)
            {
                LogDebug($"Could not set dark title bar: {ex.Message}");
            }
        }

        private Icon GetStreamUpIcon()
        {
            // TODO: Load from embedded resources
            return null;
        }
    }
}
```

### 3.3 WebView2SettingsDataFetcher.cs

```csharp
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Fetches all live data from Streamer.bot and OBS
        /// </summary>
        private async Task<JObject> FetchLiveDataAsync()
        {
            LogInfo("Fetching live data from Streamer.bot and OBS");

            var liveData = new JObject
            {
                ["streamerBot"] = FetchStreamerBotData(),
                ["obs"] = FetchObsData()
            };

            return liveData;
        }

        /// <summary>
        /// Fetch data from Streamer.bot
        /// </summary>
        private JObject FetchStreamerBotData()
        {
            LogInfo("Fetching Streamer.bot data");

            var data = new JObject();

            try
            {
                // Get actions
                var actions = new JArray();
                // _CPH.GetActions() returns list of action info
                // We need to format this appropriately
                // TODO: Verify exact return type and format
                data["actions"] = actions;

                // Get commands
                var commands = new JArray();
                // TODO: _CPH.GetCommands()
                data["commands"] = commands;

                // Get user groups
                var groups = new JArray();
                // TODO: _CPH.GetGroups()
                data["userGroups"] = groups;

                // Get Twitch channel point rewards
                var rewards = new JArray();
                // TODO: _CPH.TwitchGetRewards()
                data["channelPointRewards"] = rewards;

                LogInfo("Streamer.bot data fetched successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch Streamer.bot data: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// Fetch data from all OBS connections
        /// </summary>
        private JObject FetchObsData()
        {
            LogInfo("Fetching OBS data");

            var data = new JObject();

            try
            {
                // Get available OBS connections
                var connections = GetObsConnections();
                data["connections"] = connections;

                // For each connected OBS, get scenes/sources/filters
                var connectionsData = new JArray();

                for (int i = 0; i < 5; i++) // Streamer.bot supports up to 5 OBS connections
                {
                    if (_CPH.ObsIsConnected(i))
                    {
                        var connectionData = new JObject
                        {
                            ["index"] = i,
                            ["connected"] = true,
                            ["scenes"] = FetchObsScenes(i),
                            ["sources"] = FetchObsSources(i),
                            ["filters"] = FetchObsFilters(i)
                        };
                        connectionsData.Add(connectionData);
                    }
                    else
                    {
                        connectionsData.Add(new JObject
                        {
                            ["index"] = i,
                            ["connected"] = false
                        });
                    }
                }

                data["connectionsData"] = connectionsData;

                LogInfo("OBS data fetched successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch OBS data: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// Get list of OBS connections with their status
        /// </summary>
        private JArray GetObsConnections()
        {
            var connections = new JArray();

            for (int i = 0; i < 5; i++)
            {
                var isConnected = _CPH.ObsIsConnected(i);
                var connection = new JObject
                {
                    ["index"] = i,
                    ["name"] = $"OBS Connection {i}",
                    ["connected"] = isConnected
                };
                connections.Add(connection);
            }

            return connections;
        }

        /// <summary>
        /// Fetch all scenes from an OBS connection
        /// </summary>
        private JArray FetchObsScenes(int obsConnection)
        {
            var scenes = new JArray();

            if (GetObsSceneList(obsConnection, out JObject sceneList))
            {
                if (sceneList["scenes"] is JArray sceneArray)
                {
                    foreach (var scene in sceneArray)
                    {
                        scenes.Add(new JObject
                        {
                            ["name"] = scene["sceneName"]?.ToString(),
                            ["uuid"] = scene["sceneUuid"]?.ToString()
                        });
                    }
                }
            }

            return scenes;
        }

        /// <summary>
        /// Fetch all sources from an OBS connection (from all scenes)
        /// </summary>
        private JArray FetchObsSources(int obsConnection)
        {
            var sources = new JArray();
            var seenSources = new HashSet<string>();

            // Get scenes first
            if (GetObsSceneList(obsConnection, out JObject sceneList))
            {
                if (sceneList["scenes"] is JArray sceneArray)
                {
                    foreach (var scene in sceneArray)
                    {
                        var sceneName = scene["sceneName"]?.ToString();
                        if (GetObsSceneItemsArray(sceneName, OBSSceneType.Scene, obsConnection, out JArray items))
                        {
                            foreach (JObject item in items)
                            {
                                var sourceName = item["sourceName"]?.ToString();
                                if (!string.IsNullOrEmpty(sourceName) && !seenSources.Contains(sourceName))
                                {
                                    seenSources.Add(sourceName);
                                    sources.Add(new JObject
                                    {
                                        ["name"] = sourceName,
                                        ["type"] = item["inputKind"]?.ToString(),
                                        ["isGroup"] = item["isGroup"]
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return sources;
        }

        /// <summary>
        /// Fetch all filters from an OBS connection (from all sources)
        /// </summary>
        private JArray FetchObsFilters(int obsConnection)
        {
            var filters = new JArray();

            // Get all sources and their filters
            if (GetObsSceneList(obsConnection, out JObject sceneList))
            {
                var processedSources = new HashSet<string>();

                if (sceneList["scenes"] is JArray sceneArray)
                {
                    foreach (var scene in sceneArray)
                    {
                        var sceneName = scene["sceneName"]?.ToString();
                        if (GetObsSceneItemsArray(sceneName, OBSSceneType.Scene, obsConnection, out JArray items))
                        {
                            foreach (JObject item in items)
                            {
                                var sourceName = item["sourceName"]?.ToString();
                                if (!string.IsNullOrEmpty(sourceName) && !processedSources.Contains(sourceName))
                                {
                                    processedSources.Add(sourceName);

                                    if (GetObsSourceFilterList(sourceName, obsConnection, out JArray sourceFilters))
                                    {
                                        foreach (JObject filter in sourceFilters)
                                        {
                                            filters.Add(new JObject
                                            {
                                                ["sourceName"] = sourceName,
                                                ["filterName"] = filter["filterName"]?.ToString(),
                                                ["filterKind"] = filter["filterKind"]?.ToString(),
                                                ["filterEnabled"] = filter["filterEnabled"]
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return filters;
        }

        /// <summary>
        /// Refresh OBS data for a specific connection (called when user changes selection)
        /// </summary>
        private async Task RefreshObsDataForConnection(int connectionIndex)
        {
            LogInfo($"Refreshing OBS data for connection {connectionIndex}");

            var obsData = new JObject
            {
                ["scenes"] = FetchObsScenes(connectionIndex),
                ["sources"] = FetchObsSources(connectionIndex),
                ["filters"] = FetchObsFilters(connectionIndex)
            };

            // Send to viewer
            var script = $"window.updateObsData({obsData.ToString(Formatting.None)});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
```

### 3.4 WebView2SettingsMessages.cs

```csharp
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = JObject.Parse(e.WebMessageAsJson);
                var action = message["action"]?.ToString();

                LogDebug($"Received message: {action}");

                switch (action)
                {
                    case "save":
                        HandleSaveMessage(message);
                        break;

                    case "close":
                        HandleCloseMessage(message);
                        break;

                    case "openFileDialog":
                        HandleFileDialogMessage(message);
                        break;

                    case "openFolderDialog":
                        HandleFolderDialogMessage(message);
                        break;

                    case "requestObsData":
                        HandleRequestObsDataMessage(message);
                        break;

                    case "settingsChanged":
                        _hasUnsavedChanges = true;
                        break;

                    case "reset":
                        HandleResetMessage(message);
                        break;

                    default:
                        LogDebug($"Unknown message action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling web message: {ex.Message}");
            }
        }

        private void HandleSaveMessage(JObject message)
        {
            LogInfo("Handling save message");

            try
            {
                var data = message["data"] as JObject;
                if (data == null)
                {
                    SendToViewer(new { action = "saveComplete", success = false, error = "No data provided" });
                    return;
                }

                // Save to file
                var filePath = GetProductSettingsFilePath(_currentProductNumber);
                var json = data.ToString(Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                // Update cache
                UpdateSettingsCache(_currentProductNumber, data);

                // Update current saved settings
                _currentSavedSettings = data;
                _hasUnsavedChanges = false;

                // Notify viewer
                SendToViewer(new { action = "saveComplete", success = true });

                // Trigger save action if configured
                TriggerSaveAction();

                LogInfo("Settings saved successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to save settings: {ex.Message}");
                SendToViewer(new { action = "saveComplete", success = false, error = ex.Message });
            }
        }

        private void HandleCloseMessage(JObject message)
        {
            LogInfo("Handling close message");
            _settingsWindow?.Close();
        }

        private void HandleFileDialogMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var filter = message["filter"]?.ToString() ?? "All files (*.*)|*.*";
            var title = message["title"]?.ToString() ?? "Select File";

            LogDebug($"Opening file dialog: {title}");

            // Must run on UI thread
            _settingsWindow.Invoke((MethodInvoker)delegate
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = filter;
                    dialog.Title = title;

                    string path = null;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.FileName;
                    }

                    SendToViewer(new { action = "fileDialogResult", callbackId, path });
                }
            });
        }

        private void HandleFolderDialogMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var title = message["title"]?.ToString() ?? "Select Folder";

            LogDebug($"Opening folder dialog: {title}");

            // Must run on UI thread
            _settingsWindow.Invoke((MethodInvoker)delegate
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = title;

                    string path = null;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        path = dialog.SelectedPath;
                    }

                    SendToViewer(new { action = "fileDialogResult", callbackId, path });
                }
            });
        }

        private async void HandleRequestObsDataMessage(JObject message)
        {
            var connectionIndex = message["connectionIndex"]?.Value<int>() ?? 0;
            LogInfo($"Handling OBS data request for connection {connectionIndex}");

            await RefreshObsDataForConnection(connectionIndex);
        }

        private void HandleResetMessage(JObject message)
        {
            LogInfo("Handling reset to defaults");

            // The viewer handles the UI reset, we just need to confirm
            _hasUnsavedChanges = true;
            SendToViewer(new { action = "resetComplete", success = true });
        }

        private void SendToViewer(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            _webView.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void TriggerSaveAction()
        {
            var settingsAction = _currentConfig?["settingsAction"]?.ToString();
            if (!string.IsNullOrEmpty(settingsAction))
            {
                try
                {
                    LogInfo($"Triggering save action: {settingsAction}");
                    _CPH.RunAction(settingsAction, false);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to trigger save action: {ex.Message}");
                }
            }
        }
    }
}
```

### 3.5 WebView2SettingsCache.cs

```csharp
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Static cache persists across all product executions while Streamer.bot is running
        private static Dictionary<string, CachedProductSettings> _productSettingsCache
            = new Dictionary<string, CachedProductSettings>();

        private class CachedProductSettings
        {
            public JObject Data { get; set; }
            public DateTime FileTimestamp { get; set; }
        }

        /// <summary>
        /// Get the file path for a product's settings
        /// </summary>
        private string GetProductSettingsFilePath(string productNumber)
        {
            var dataPath = Path.Combine(GetStreamerBotFolder(), "StreamUP", "Data");
            Directory.CreateDirectory(dataPath);
            return Path.Combine(dataPath, $"{productNumber}_Data.json");
        }

        /// <summary>
        /// Load product settings from cache or file
        /// </summary>
        private JObject LoadProductSettings(string productNumber)
        {
            LogInfo($"Loading settings for product {productNumber}");

            var filePath = GetProductSettingsFilePath(productNumber);

            // Check cache first
            if (_productSettingsCache.TryGetValue(productNumber, out var cached))
            {
                // Validate cache - check if file was modified
                if (File.Exists(filePath))
                {
                    var fileTime = File.GetLastWriteTime(filePath);
                    if (fileTime == cached.FileTimestamp)
                    {
                        LogDebug("Using cached settings");
                        return cached.Data;
                    }
                    LogDebug("Cache invalidated - file was modified");
                }
            }

            // Load from file
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var data = JObject.Parse(json);

                    // Update cache
                    _productSettingsCache[productNumber] = new CachedProductSettings
                    {
                        Data = data,
                        FileTimestamp = File.GetLastWriteTime(filePath)
                    };

                    LogInfo("Settings loaded from file");
                    return data;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load settings file: {ex.Message}");
                }
            }

            LogInfo("No saved settings found");
            return null;
        }

        /// <summary>
        /// Update the settings cache
        /// </summary>
        private void UpdateSettingsCache(string productNumber, JObject data)
        {
            var filePath = GetProductSettingsFilePath(productNumber);

            _productSettingsCache[productNumber] = new CachedProductSettings
            {
                Data = data,
                FileTimestamp = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : DateTime.Now
            };
        }
    }
}
```

### 3.6 WebView2SettingsHelpers.cs

```csharp
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Get a single setting value for the current product
        /// </summary>
        public T GetProductSetting<T>(string key, T defaultValue)
        {
            LogDebug($"Getting setting: {key}");

            var settings = LoadProductSettings(_ProductIdentifier);
            if (settings == null)
            {
                return defaultValue;
            }

            var settingsObj = settings["settings"] as JObject;
            if (settingsObj == null || !settingsObj.ContainsKey(key))
            {
                return defaultValue;
            }

            try
            {
                return settingsObj[key].ToObject<T>();
            }
            catch (Exception ex)
            {
                LogError($"Failed to convert setting {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Set a single setting value for the current product
        /// </summary>
        public void SetProductSetting(string key, object value)
        {
            LogInfo($"Setting: {key}");

            var settings = LoadProductSettings(_ProductIdentifier);
            if (settings == null)
            {
                settings = new JObject
                {
                    ["meta"] = new JObject
                    {
                        ["productNumber"] = _ProductIdentifier,
                        ["savedAt"] = DateTime.UtcNow.ToString("O")
                    },
                    ["settings"] = new JObject()
                };
            }

            var settingsObj = settings["settings"] as JObject ?? new JObject();
            settingsObj[key] = JToken.FromObject(value);
            settings["settings"] = settingsObj;
            settings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

            // Save to file
            var filePath = GetProductSettingsFilePath(_ProductIdentifier);
            File.WriteAllText(filePath, settings.ToString(Formatting.Indented), Encoding.UTF8);

            // Update cache
            UpdateSettingsCache(_ProductIdentifier, settings);
        }

        /// <summary>
        /// Get all settings for the current product
        /// </summary>
        public JObject GetAllProductSettings()
        {
            var settings = LoadProductSettings(_ProductIdentifier);
            return settings?["settings"] as JObject ?? new JObject();
        }

        /// <summary>
        /// Check if settings exist for the current product
        /// </summary>
        public bool ProductSettingsExist()
        {
            var filePath = GetProductSettingsFilePath(_ProductIdentifier);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete all settings for the current product
        /// </summary>
        public void DeleteProductSettings()
        {
            LogInfo("Deleting product settings");

            var filePath = GetProductSettingsFilePath(_ProductIdentifier);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Remove from cache
            _productSettingsCache.Remove(_ProductIdentifier);
        }

        /// <summary>
        /// Get the OBS connection index for the current product
        /// </summary>
        public int GetProductObsConnection()
        {
            var settings = LoadProductSettings(_ProductIdentifier);
            return settings?["productData"]?["obsInstanceNumber"]?.Value<int>() ?? 0;
        }
    }
}
```

---

## 4. Viewer Updates

### 4.1 Changes Needed

The Settings Viewer needs updates to support:

1. **Receive injected data** - Already supports `window.STREAMUP_*` globals
2. **OBS connection selector** - New component type
3. **Streamer.bot data dropdowns** - Actions, commands, user groups, rewards
4. **OBS data dropdowns** - Scenes, sources, filters
5. **Send data change notifications** - `settingsChanged` message
6. **Handle OBS connection changes** - Request new data when user changes connection

### 4.2 New Component Types

Add to `componentRegistry.js`:

```javascript
// Integration components (populated by DLL)
'streamerbot-action': { group: 'integration', label: 'Streamer.bot Action' },
'streamerbot-command': { group: 'integration', label: 'Streamer.bot Command' },
'streamerbot-usergroup': { group: 'integration', label: 'User Group' },
'twitch-reward': { group: 'integration', label: 'Channel Point Reward' },
'obs-connection': { group: 'integration', label: 'OBS Connection' },
'obs-scene': { group: 'integration', label: 'OBS Scene' },
'obs-source': { group: 'integration', label: 'OBS Source' },
'obs-filter': { group: 'integration', label: 'OBS Filter' },
```

### 4.3 New Control Renderer

Create `js/rendering/controls/integrationControl.js`:

```javascript
/**
 * Renders integration controls (Streamer.bot and OBS dropdowns)
 * These are populated with live data from the DLL
 */

export function createIntegrationControl(setting, container) {
    const type = setting.type;
    const liveData = window.STREAMUP_LIVE_DATA || {};

    switch (type) {
        case 'obs-connection':
            return createObsConnectionSelector(setting, container, liveData);
        case 'obs-scene':
            return createObsSceneSelector(setting, container, liveData);
        case 'obs-source':
            return createObsSourceSelector(setting, container, liveData);
        case 'obs-filter':
            return createObsFilterSelector(setting, container, liveData);
        case 'streamerbot-action':
            return createStreamerBotActionSelector(setting, container, liveData);
        case 'streamerbot-command':
            return createStreamerBotCommandSelector(setting, container, liveData);
        case 'streamerbot-usergroup':
            return createStreamerBotUserGroupSelector(setting, container, liveData);
        case 'twitch-reward':
            return createTwitchRewardSelector(setting, container, liveData);
        default:
            return null;
    }
}

function createObsConnectionSelector(setting, container, liveData) {
    const connections = liveData.obs?.connections || [];

    const select = document.createElement('select');
    select.id = setting.backendName;
    select.className = 'settings-select';

    connections.forEach(conn => {
        const option = document.createElement('option');
        option.value = conn.index;
        option.textContent = `${conn.name} ${conn.connected ? '(Connected)' : '(Disconnected)'}`;
        option.disabled = !conn.connected;
        select.appendChild(option);
    });

    // When connection changes, request new OBS data
    select.addEventListener('change', () => {
        if (window.chrome?.webview) {
            window.chrome.webview.postMessage(JSON.stringify({
                action: 'requestObsData',
                connectionIndex: parseInt(select.value)
            }));
        }
    });

    container.appendChild(select);
    return select;
}

// ... similar implementations for other selectors
```

### 4.4 Global Function for OBS Data Updates

Add to `js/main.js`:

```javascript
/**
 * Called by DLL when OBS data needs to be refreshed
 */
window.updateObsData = function(obsData) {
    console.log('[StreamUP Viewer] Updating OBS data');

    // Update the live data
    if (!window.STREAMUP_LIVE_DATA) {
        window.STREAMUP_LIVE_DATA = {};
    }
    if (!window.STREAMUP_LIVE_DATA.obs) {
        window.STREAMUP_LIVE_DATA.obs = {};
    }

    window.STREAMUP_LIVE_DATA.obs.scenes = obsData.scenes;
    window.STREAMUP_LIVE_DATA.obs.sources = obsData.sources;
    window.STREAMUP_LIVE_DATA.obs.filters = obsData.filters;

    // Dispatch event for controls to update
    window.dispatchEvent(new CustomEvent('obs-data-updated', { detail: obsData }));
};
```

---

## 5. Helper Methods API

### 5.1 Public Methods for Products

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `OpenSettingsMenu` | `JObject config` | `Task` | Opens the settings WebView2 window |
| `GetProductSetting<T>` | `string key, T default` | `T` | Gets a single setting value |
| `SetProductSetting` | `string key, object value` | `void` | Sets a single setting value |
| `GetAllProductSettings` | none | `JObject` | Gets all settings as JObject |
| `ProductSettingsExist` | none | `bool` | Checks if settings file exists |
| `DeleteProductSettings` | none | `void` | Deletes settings file and cache |
| `GetProductObsConnection` | none | `int` | Gets the selected OBS connection index |

### 5.2 Usage Examples

```csharp
// In a Streamer.bot product action

// Initialize StreamUP library
var sup = new StreamUpLib(CPH, "sup001");

// Open settings menu
var settingsJson = JObject.Parse(@"{
    ""productName"": ""My Product"",
    ""productNumber"": ""sup001"",
    ""productVersion"": ""1.0.0"",
    ""Pages"": [...]
}");
await sup.OpenSettingsMenu(settingsJson);

// Read settings (in another action)
var username = sup.GetProductSetting<string>("userName", "Guest");
var volume = sup.GetProductSetting<int>("masterVolume", 100);
var enabled = sup.GetProductSetting<bool>("alertsEnabled", true);

// Get OBS connection for this product
var obsConnection = sup.GetProductObsConnection();
sup.SetObsSceneVisibility("MyScene", "MySource", true, obsConnection);

// Update a setting programmatically
sup.SetProductSetting("lastUsed", DateTime.Now.ToString());
```

---

## 6. OBS Connection Selector

### 6.1 How It Works

1. **On Open**: DLL checks all 5 possible OBS connections for connectivity
2. **Display**: Viewer shows dropdown with connected/disconnected status
3. **Selection**: User picks which OBS to use for this product
4. **Storage**: Selected connection index saved in `productData.obsInstanceNumber`
5. **Refresh**: When selection changes, DLL fetches scenes/sources/filters for that connection
6. **Usage**: Products call `GetProductObsConnection()` to get the index

### 6.2 Data Structure

```json
{
  "meta": { ... },
  "settings": { ... },
  "productData": {
    "obsInstanceNumber": 0
  }
}
```

### 6.3 Connection Status

```javascript
window.STREAMUP_LIVE_DATA = {
  obs: {
    connections: [
      { index: 0, name: "OBS Connection 0", connected: true },
      { index: 1, name: "OBS Connection 1", connected: false },
      { index: 2, name: "OBS Connection 2", connected: true },
      { index: 3, name: "OBS Connection 3", connected: false },
      { index: 4, name: "OBS Connection 4", connected: false }
    ],
    connectionsData: [
      {
        index: 0,
        connected: true,
        scenes: [...],
        sources: [...],
        filters: [...]
      },
      ...
    ]
  }
}
```

---

## 7. Caching Strategy

### 7.1 Cache Levels

1. **Static Dictionary**: `_productSettingsCache` persists across all product executions
2. **File Timestamp Validation**: Cache is invalidated if file was modified externally
3. **Immediate Update**: Cache is updated immediately when settings are saved

### 7.2 Cache Flow

```
GetProductSetting("key", default)
         │
         ▼
    ┌─────────────────┐
    │ Check cache     │
    │ exists?         │
    └────────┬────────┘
             │
    ┌────────┴────────┐
    │                 │
   Yes               No
    │                 │
    ▼                 ▼
┌─────────────┐   ┌─────────────┐
│ Check file  │   │ Load from   │
│ timestamp   │   │ file        │
└──────┬──────┘   └──────┬──────┘
       │                 │
   ┌───┴───┐             │
   │       │             │
 Same   Different        │
   │       │             │
   ▼       ▼             ▼
┌─────┐ ┌─────────────────────┐
│Use  │ │ Load file, update   │
│cache│ │ cache               │
└─────┘ └─────────────────────┘
```

### 7.3 Benefits

- Fast reads (no file I/O if cache is valid)
- Consistent across actions (shared static cache)
- Auto-invalidates if file edited externally
- Low memory footprint (only loaded products are cached)

---

## 8. Window Behavior

### 8.1 Non-Modal

The settings window does NOT block Streamer.bot:
- User can interact with Streamer.bot while settings are open
- Multiple products could potentially open settings (though we may want to prevent this)

### 8.2 Window Persistence

- **Size**: Remembered across sessions (stored in static variables)
- **Position**: Remembered across sessions
- **State**: Window state (maximized, etc.) could be added

### 8.3 Close Behavior

When user tries to close:
1. Check `_hasUnsavedChanges` flag
2. If true, show confirmation dialog
3. If user confirms, close window
4. If user cancels, keep window open

### 8.4 Reset to Defaults

- Viewer handles the UI reset (reverts all fields to `defaultValue`)
- Sends `reset` message to DLL
- DLL sets `_hasUnsavedChanges = true`
- User must click Save to persist the reset

---

## 9. File Structure

### 9.1 DLL Files to Create

```
StreamUP DLL/Extensions/WebView2Settings/
├── WebView2SettingsWindow.cs      # Window creation, navigation, injection
├── WebView2SettingsMethods.cs     # OpenSettingsMenu entry point
├── WebView2SettingsHelpers.cs     # GetProductSetting, SetProductSetting, etc.
├── WebView2SettingsCache.cs       # Static caching logic
├── WebView2SettingsDataFetcher.cs # Fetch Streamer.bot and OBS data
├── WebView2SettingsMessages.cs    # Handle messages from viewer
└── IMPLEMENTATION_PLAN.md         # This document
```

### 9.2 Viewer Files to Create/Update

```
Settings-Viewer/js/
├── config/
│   └── componentRegistry.js       # UPDATE: Add integration types
├── rendering/
│   └── controls/
│       ├── integrationControl.js  # NEW: Streamer.bot/OBS selectors
│       └── index.js               # UPDATE: Export new control
├── main.js                        # UPDATE: Add window.updateObsData
└── core/
    └── bridge.js                  # UPDATE: Add requestObsData message
```

---

## 10. Implementation Order

### Phase 1: DLL Core (Week 1)

1. [ ] Create `WebView2SettingsWindow.cs` - Basic window with WebView2
2. [ ] Create `WebView2SettingsMethods.cs` - `OpenSettingsMenu` entry point
3. [ ] Create `WebView2SettingsCache.cs` - File loading and caching
4. [ ] Create `WebView2SettingsMessages.cs` - Basic save/close handling
5. [ ] Test with existing viewer (should work for basic types)

### Phase 2: Data Fetching (Week 1-2)

6. [ ] Create `WebView2SettingsDataFetcher.cs` - Streamer.bot data
7. [ ] Add OBS data fetching (scenes, sources, filters)
8. [ ] Add OBS connection enumeration
9. [ ] Test data injection into viewer

### Phase 3: Viewer Integration Components (Week 2)

10. [ ] Create `integrationControl.js` in viewer
11. [ ] Add OBS connection selector
12. [ ] Add OBS scene/source/filter selectors
13. [ ] Add Streamer.bot action/command/usergroup/reward selectors
14. [ ] Add `window.updateObsData` function
15. [ ] Test integration components end-to-end

### Phase 4: Helper Methods (Week 2-3)

16. [ ] Create `WebView2SettingsHelpers.cs`
17. [ ] Implement `GetProductSetting<T>`
18. [ ] Implement `SetProductSetting`
19. [ ] Implement `GetAllProductSettings`
20. [ ] Implement `GetProductObsConnection`
21. [ ] Test helper methods from sample product

### Phase 5: Polish (Week 3)

22. [ ] Add unsaved changes detection and confirmation
23. [ ] Add window size/position persistence
24. [ ] Add dark title bar
25. [ ] Add StreamUP icon
26. [ ] Add file/folder dialog handling
27. [ ] Add reset to defaults handling
28. [ ] Comprehensive testing

### Phase 6: Documentation & Deployment

29. [ ] Update `DLL_INTEGRATION.md` in viewer
30. [ ] Create sample product demonstrating usage
31. [ ] Deploy viewer to viewer.streamup.tips
32. [ ] Test full end-to-end flow

---

## Appendix A: Verified Data Structures

### A.1 CPH Method Return Types (Verified 2026-01-11)

#### `_CPH.GetActions()` → `List<ActionData>`
```json
{
  "Id": "af5b4b42-2b36-479d-b174-b1918a4d0655",
  "Name": "Horizontal Chat • Main Action",
  "Enabled": true,
  "Group": "StreamUP Widgets • Horizontal Chat",
  "Queue": null,
  "QueueId": "00000000-0000-0000-0000-000000000000"
}
```

#### `_CPH.GetCommands()` → `List<CommandData>`
```json
{
  "Id": "0154a7c4-75c3-4e14-9531-4e6086e269bf",
  "Name": "Xmas Overlay • Get Snowball Stats",
  "Enabled": true,
  "Group": "Xmas Overlay",
  "Mode": 0,
  "Commands": ["!snowballstats", "!sbstats"],
  "RegexCommand": null,
  "CaseSensitive": false,
  "Sources": ["None", "Twitch Message"]
}
```

#### `_CPH.GetGroups()` → `List<string>`
```json
["Giveaway", "Moderators", "VIPs"]
```

#### `_CPH.TwitchGetRewards()` → `List<TwitchReward>`
```json
{
  "Id": "d1f83b1c-8bb5-4ec1-95fd-90feb8a58f93",
  "Title": "On Screen Message",
  "Group": "Dynamic Stream-Island",
  "Prompt": "Show a message on the Dynamic Stream-Island...",
  "Cost": 100,
  "InputRequired": true,
  "BackgroundColor": "#9A0769",
  "Paused": false,
  "Enabled": true,
  "IsOurs": true
}
```

### A.2 OBS Connection Names

**Status**: No CPH method exists to list connection names. `ObsGetConnectionByName` exists (connections have names), but we cannot enumerate them.

**Solution**: Use generic labels "OBS Connection 0" through "OBS Connection 4" for now. Can be enhanced later if a method becomes available.

### A.3 Window Icon

**Location**:
- DLL: `Resources/StreamUp-icon.png` (copied from branding)
- Viewer: `assets/streamup-icon.png` (copied from branding)

**Source**: `StreamUp-3color-cyanmidnightpink-button@4x.png`

### A.4 Live Data Structure for Viewer

```javascript
window.STREAMUP_LIVE_DATA = {
  streamerBot: {
    actions: [
      { id: "guid", name: "Action Name", enabled: true, group: "Group Name" }
    ],
    commands: [
      { id: "guid", name: "Command Name", enabled: true, group: "Group", commands: ["!cmd"] }
    ],
    userGroups: ["Group1", "Group2"],
    channelPointRewards: [
      { id: "guid", title: "Reward Title", cost: 100, enabled: true, group: "Group" }
    ]
  },
  obs: {
    connections: [
      { index: 0, name: "OBS Connection 0", connected: true },
      { index: 1, name: "OBS Connection 1", connected: false }
    ],
    connectionsData: [
      {
        index: 0,
        connected: true,
        scenes: [{ name: "Scene 1", uuid: "guid" }],
        sources: [{ name: "Source 1", type: "browser_source", isGroup: false }],
        filters: [{ sourceName: "Source 1", filterName: "Filter 1", filterKind: "color_filter" }]
      }
    ]
  }
};
```

---

## Appendix B: Resolved Questions

| Question | Answer |
|----------|--------|
| CPH Method Return Types | Verified - see Appendix A.1 |
| OBS Connection Names | Not available via API - use generic labels |
| Multiple Windows | Prevent - only one settings window at a time |
| Viewer URL | Confirmed: `https://viewer.streamup.tips/` |
| Icon | Copied to `Resources/StreamUp-icon.png` in DLL |

---

*Document Version: 1.1.0*
*Last Updated: 2026-01-11*
