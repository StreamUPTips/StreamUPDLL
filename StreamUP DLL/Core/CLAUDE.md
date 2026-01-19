# StreamUP DLL - CLAUDE.md

## Overview

C# library that hosts the Settings Viewer in a WebView2 control and bridges it to Streamer.bot and OBS. Part of a 3-repo system:

| Repo | Path | Purpose |
|------|------|---------|
| Builder | `D:\StreamUP\GitHub\Streamerbot-Settings\Settings-Builder` | Visual UI builder |
| Viewer | `D:\StreamUP\GitHub\Streamerbot-Settings\Settings-Viewer` | Runtime renderer |
| **DLL (this)** | `D:\StreamUP\GitHub\Streamerbot-DLL\StreamUP DLL\Core` | C# WebView2 host |

---

## Directory Structure

```
Core/
├── Settings/                   # WebView2 settings window system
│   ├── WebView2SettingsWindow.cs    # Window creation, data injection, lifecycle
│   ├── WebView2SettingsMessages.cs  # Handles messages FROM Viewer
│   ├── WebView2SettingsMethods.cs   # Public API: OpenSettingsMenu(), etc.
│   ├── WebView2SettingsHelpers.cs   # Helper methods for products
│   ├── WebView2SettingsCache.cs     # File I/O and caching
│   ├── WebView2SettingsDataFetcher.cs # Fetches SB and OBS data
│   └── BorderlessForm.cs            # Custom window with resize support
├── ProductSystem/              # Product initialization and validation
│   ├── ProductEntryPoints.cs        # Main entry points
│   ├── Initialization/              # Init logic
│   │   ├── ProductInitializerV2.cs
│   │   ├── ProductConfigRegistry.cs
│   │   ├── ProductValidationCache.cs
│   │   ├── InitializationResult.cs
│   │   └── PromptManager.cs
│   ├── Validation/
│   │   ├── ObsValidator.cs
│   │   └── LibraryVersionValidator.cs
│   └── UI/
│       ├── DialogType.cs
│       └── ModernDialog.cs
├── Obs/                        # OBS WebSocket wrapper
│   ├── WebSocket/
│   │   ├── ObsCore.cs               # Core OBS connection
│   │   ├── ObsScenes.cs             # Scene operations
│   │   ├── ObsSceneItems.cs         # Scene item operations
│   │   ├── ObsSources.cs            # Source operations
│   │   ├── ObsFilters.cs            # Filter operations
│   │   ├── ObsInputs.cs             # Input operations
│   │   ├── ObsTransitions.cs        # Transition operations
│   │   ├── ObsOutputs.cs            # Output operations
│   │   ├── ObsMedia.cs              # Media operations
│   │   ├── ObsUi.cs                 # UI operations
│   │   ├── ObsGeneral.cs            # General operations
│   │   └── ObsConfig.cs             # Configuration
│   └── Helpers/
│       ├── GetObsScaleFactor.cs
│       └── ObsTextFitting.cs
└── Utils/
    └── LZString.cs                  # LZ-String compression
```

---

## Message Contract: Viewer → C#

All messages sent via `window.chrome.webview.postMessage({action, ...})`

| Action | Parameters | Handler | Description |
|--------|------------|---------|-------------|
| `save` | `data` (JObject) | `HandleSaveMessage()` | Save settings to file |
| `close` | none | `HandleCloseMessage()` | Close settings window |
| `openFileDialog` | `callbackId`, `filter`, `title`, `initialDirectory` | `HandleOpenFileDialogMessage()` | Open file picker |
| `openFolderDialog` | `callbackId`, `title`, `initialDirectory` | `HandleOpenFolderDialogMessage()` | Open folder picker |
| `requestObsData` | `connectionIndex` | `HandleRequestObsDataMessage()` | Refresh OBS data for connection |
| `settingsChanged` | none | (inline) | Mark unsaved changes |
| `reset` | none | `HandleResetMessage()` | Reset to defaults |
| `log` | `level`, `message` | `HandleLogMessage()` | Log to Streamer.bot |
| `minimize` | none | `MinimizeWindow()` | Minimize window |
| `maximize` | none | `MaximizeWindow()` | Toggle maximize |
| `startDrag` | none | `StartWindowDrag()` | Enable window dragging |
| `runAction` | `callbackId`, `actionId` or `actionName` | `HandleRunActionMessage()` | Run a Streamer.bot action |
| `executeMethod` | `callbackId`, `codeId`, `methodName` | `HandleExecuteMethodMessage()` | Execute a method in SB |
| `openExternalUrl` | `url` | `HandleOpenExternalUrlMessage()` | Open URL in browser |

---

## Message Contract: C# → Viewer

Messages sent via `SendToViewer({action, ...})` → `PostWebMessageAsJson()`

| Action | Parameters | Purpose |
|--------|------------|---------|
| `saveComplete` | `success`, `error?` | Confirm save completed |
| `fileDialogResult` | `callbackId`, `path`, `error?` | File/folder dialog result |
| `resetComplete` | `success` | Confirm reset completed |
| `runActionResult` | `callbackId`, `success`, `error?` | Action execution result |
| `executeMethodResult` | `callbackId`, `success`, `error?` | Method execution result |

---

## Data Injection on Init

When window opens, these globals are injected before the Viewer loads:

```javascript
window.STREAMUP_MODE = 'production';
window.STREAMUP_CONFIG = {...};           // Settings menu structure (from product JSON)
window.STREAMUP_SAVED_SETTINGS = {...};   // User's saved values (from _Data.json)
window.STREAMUP_PRODUCT_DATA = {...};     // OBS instance number, etc.
window.STREAMUP_LIVE_DATA = {...};        // Actions, commands, scenes, filters, etc.

// Viewer listens for this event to know data is ready:
window.dispatchEvent(new Event('streamup-init'));
```

### Live Data Structure

```json
{
  "streamerBot": {
    "actions": [{ "id": "guid", "name": "Action Name", "enabled": true, "group": "" }],
    "commands": [{ "id": "guid", "name": "Command Name", "enabled": true, "group": "", "commands": ["!cmd"] }],
    "userGroups": ["group1", "group2"],
    "channelPointRewards": [{ "id": "guid", "title": "Reward", "cost": 100, "enabled": true }]
  },
  "obs": {
    "connections": [{ "index": 0, "name": "OBS Connection 0", "connected": true }],
    "connectionsData": [{
      "index": 0,
      "connected": true,
      "scenes": [{ "name": "Scene", "uuid": "..." }],
      "sources": [{ "name": "Source", "type": "browser_source", "isGroup": false }],
      "filters": [{ "sourceName": "Source", "filterName": "Filter", "filterKind": "...", "filterEnabled": true }]
    }]
  }
}
```

---

## Settings Storage

**File Location:** `Streamer.bot/StreamUP/Data/{productNumber}_Data.json`

**Structure:**
```json
{
  "meta": {
    "productNumber": "PRD001",
    "productName": "My Product",
    "savedAt": "2024-01-19T12:34:56.789Z"
  },
  "settings": {
    "userName": "JohnDoe",
    "masterVolume": 75
  },
  "productData": {
    "obsInstanceNumber": 0
  }
}
```

---

## Public API Reference

### Window Management

```csharp
// Open settings window with config
bool OpenSettingsMenu(JObject settingsConfig)
bool OpenSettingsMenu(string settingsConfigJson)  // Supports LZ-String compressed

// Check/close window
bool IsSettingsWindowOpen()
bool CloseSettingsWindow(bool force = false)
```

### Setting Access (use in Main actions)

```csharp
// Get single setting
T GetProductSetting<T>(string key, T defaultValue)
// Examples:
string name = sup.GetProductSetting<string>("userName", "");
int volume = sup.GetProductSetting<int>("masterVolume", 50);
bool enabled = sup.GetProductSetting<bool>("isEnabled", true);
List<string> items = sup.GetProductSetting<List<string>>("selectedItems", new List<string>());

// Set settings
bool SetProductSetting(string key, object value)
bool SetProductSettings(Dictionary<string, object> settingsToUpdate)

// Other methods
JObject GetAllProductSettings()
JObject GetFullProductData()
bool ProductSettingsExist()
bool DeleteProductSettings()
bool HasProductSetting(string key)
bool RemoveProductSetting(string key)
List<string> GetProductSettingKeys()
```

### Typed Settings (recommended)

```csharp
// Get all settings as typed class (generated by Builder)
T GetTypedSettings<T>() where T : class, new()

// Get product info as typed class
T GetProductInfo<T>() where T : class, new()

// Save all settings from typed class
bool SaveTypedSettings<T>(T settings) where T : class

// Example usage:
var settings = sup.GetTypedSettings<MyProductSettings>();
string userName = settings.UserName;  // IntelliSense + type safety!
settings.MasterVolume = 75;
sup.SaveTypedSettings(settings);
```

### OBS Connection

```csharp
// Get/set which OBS connection the product uses
int GetProductObsConnection()  // Returns 0-4
bool SetProductObsConnection(int connectionIndex)
```

### Import/Export

```csharp
string ExportProductSettings()
bool ImportProductSettings(string json, bool mergeWithExisting = false)
```

---

## Adding a New Message Handler

### 1. Add case in `WebView2SettingsMessages.cs`:

```csharp
case "myNewAction":
    HandleMyNewActionMessage(message);
    break;
```

### 2. Create handler method:

```csharp
private void HandleMyNewActionMessage(JObject message)
{
    var callbackId = message["callbackId"]?.ToString();
    var param = message["param"]?.ToString();

    try
    {
        // Process...

        SendToViewer(new {
            action = "myNewActionResult",
            callbackId,
            success = true
        });
    }
    catch (Exception ex)
    {
        SendToViewer(new {
            action = "myNewActionResult",
            callbackId,
            success = false,
            error = ex.Message
        });
    }
}
```

### 3. Update Viewer to send/receive

In `Viewer/js/core/bridge.js`:
```javascript
export function sendMyNewAction(param) {
  return sendMessageWithCallback('myNewAction', { param });
}
```

---

## Save Action Trigger

When user saves settings, the DLL can trigger a Streamer.bot action. Configure in product JSON:

```json
{
  "settingsAction": "My Settings Saved Action"
}
```

Or use execute method:
```json
{
  "onSaveExecuteMethod": {
    "codeId": "my-execute-code-id",
    "methodName": "OnSettingsSaved"
  }
}
```

---

## Key Dependencies

- **Microsoft.Web.WebView2** - WebView2 control
- **Newtonsoft.Json** - JSON parsing (JObject, JArray)
- **Streamer.bot CPH** - `_CPH` instance for actions, commands, OBS, etc.

---

## Viewer URL

Production: `https://viewer.streamup.tips/`

For local development, uncomment in `WebView2SettingsWindow.cs`:
```csharp
//private const string VIEWER_URL = "http://localhost:8080/";
```

---

## Cross-Repo Coordination

When changing messages or data contracts, update:
1. **DLL** - This file + handler code
2. **Viewer** - `js/core/bridge.js` + relevant modules
3. **Root docs** - `CLAUDE.md` JSON contract section

See root `CLAUDE.md` for full cross-repo documentation.
