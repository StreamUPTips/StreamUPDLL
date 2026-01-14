# StreamUP Test Products

This folder contains example products demonstrating the **simplified developer workflow** introduced in StreamUP DLL v3.0.

## Quick Reference

### Class Structure (for External Editor + Streamer.bot compatibility)
```csharp
using StreamUP;

#if EXTERNAL_EDITOR
public class PRODUCTID_MainAction : CPHInlineBase
#else
public class CPHInline
#endif
{
    private readonly string productNumber = "PRODUCTID";
    public StreamUpLib SUP;

    public void Init()
    {
        SUP = new StreamUpLib(CPH, productNumber);
    }

    public bool Execute()
    {
        Init();
        // Your code here
        return true;
    }
}
```

### Settings Action (ONE LINE!)
```csharp
public bool Execute()
{
    Init();
    var config = JObject.Parse(@"{ /* from Settings Builder */ }");
    SUP.OpenProductSettings(config);  // That's it!
    return true;
}
```

### Main Action (ONE LINE, NO CONFIG!)
```csharp
public bool Execute()
{
    Init();
    if (!SUP.IsProductReady())  // Uses cached config!
        return true;
    // Your code here...
    return true;
}
```

---

## Test Files

| File | Purpose |
|------|---------|
| `TestProduct_SettingsAction.cs` | Settings action using `OpenProductSettings()` |
| `TestProduct_MainAction.cs` | Main action using `IsProductReady()` |
| `TestProduct_OBS_Example.cs` | OBS product with scene validation |
| `TestProduct_TypedSettings.cs` | Using typed settings classes |

---

## The Simplified Workflow

### Before (Old Way)
```csharp
// Settings Action - had to manually check library
if (!sup.ValidateLibraryVersionV2("3.0.0.0"))
{
    // Show dialog manually
    return true;
}
sup.OpenSettingsMenu(config);

// Main Action - had to pass ENTIRE config again!
var result = sup.InitializeProductV2(config);  // Same huge JSON!
if (result != InitializationResult.Success)
    return true;
```

### After (New Way)
```csharp
// Settings Action - one line, auto-registers config
sup.OpenProductSettings(config);

// Main Action - no config needed at all!
if (!sup.IsProductReady())
    return true;
```

---

## What Happens Automatically

### `OpenProductSettings(config)`
1. ✅ Validates library version (shows error if outdated)
2. ✅ Registers config for future Main action use
3. ✅ Invalidates validation cache (user might change settings)
4. ✅ Opens the settings window

### `IsProductReady()`
1. ✅ Uses cached validation if available (INSTANT!)
2. ✅ Loads registered product config
3. ✅ Validates library version
4. ✅ Checks settings exist (shows prompt if not)
5. ✅ For OBS products: validates OBS connection
6. ✅ For OBS products: validates scene exists
7. ✅ For OBS products: validates scene version
8. ✅ Caches successful result for future calls

---

## Product Types

### Streamer.Bot Only
```json
{
    "productType": "Streamer.Bot Only",
    "libraryVersion": "3.0.0.0"
}
```

Validates:
- Library version
- Settings exist

### OBS Product
```json
{
    "productType": "OBS",
    "libraryVersion": "3.0.0.0",
    "sceneName": "[SUP] My Scene",
    "sourceName": "[SUP] My Source",
    "sceneVersion": "1.0.0.0"
}
```

Validates (in addition):
- OBS WebSocket connected
- Scene exists
- Scene version ≥ required

---

## Getting Settings

### Option 1: Individual Values
```csharp
string name = sup.GetProductSetting("displayName", "Guest");
int volume = sup.GetProductSetting("volumeLevel", 50);
bool enabled = sup.GetProductSetting("isEnabled", true);
```

### Option 2: Typed Class (Full IntelliSense!)
```csharp
var settings = sup.GetTypedSettings<MyProductSettings>();

// Now you have full IntelliSense:
if (settings.IsEnabled)
{
    sup.LogInfo($"Hello, {settings.DisplayName}!");
}
```

---

## Creating a New Product

### Step 1: Build in Settings Builder
1. Open Settings Builder
2. Create your settings pages
3. Set product info (name, number, version, type)
4. Export JSON using "Export for Viewer"

### Step 2: Create Settings Action
```csharp
public class CPHInline : CPHInlineBase
{
    private static readonly string Config = @"{ /* paste exported JSON */ }";

    public bool Execute()
    {
        var sup = new StreamUpLib(CPH, "YOURPRODUCT001");
        sup.OpenProductSettings(JObject.Parse(Config));
        return true;
    }
}
```

### Step 3: Create Main Action
```csharp
public class CPHInline : CPHInlineBase
{
    public bool Execute()
    {
        var sup = new StreamUpLib(CPH, "YOURPRODUCT001");

        if (!sup.IsProductReady())
            return true;

        // Your product code here!
        var name = sup.GetProductSetting("userName", "Guest");
        sup.LogInfo($"Hello, {name}!");

        return true;
    }
}
```

That's it! The DLL handles everything else automatically.

---

## Validation Caching

After successful `IsProductReady()`:
- Result is cached in memory
- Subsequent calls return **instantly**
- Cache invalidates when:
  - Settings file changes
  - OBS connection changes
  - User opens Settings window
  - Streamer.bot restarts

---

## Error Handling

### Detailed Status
```csharp
var status = sup.CheckProductReady();

switch (status)
{
    case ProductReadyStatus.Ready:
        // Proceed
        break;
    case ProductReadyStatus.ConfigNotRegistered:
        // Settings action never ran
        break;
    case ProductReadyStatus.SettingsNotConfigured:
        // User hasn't configured yet
        break;
    case ProductReadyStatus.ObsNotConnected:
        // OBS WebSocket offline
        break;
    case ProductReadyStatus.ObsSceneMissing:
        // Required scene not found
        break;
    case ProductReadyStatus.ObsSceneOutdated:
        // Scene version too old
        break;
    case ProductReadyStatus.LibraryOutdated:
        // DLL needs updating
        break;
    case ProductReadyStatus.AlreadyPrompted:
        // Silent - user already saw dialog
        break;
}
```

---

## File Locations

```
%AppData%\Streamer.bot\StreamUP\
├── ProductConfigs/           # Auto-registered configs
│   └── YOURPRODUCT001_Config.json
├── Data/                     # User saved settings
│   └── YOURPRODUCT001_Data.json
└── WebView2Cache/            # Browser cache
```

---

## Migration from v2

### Before (v2)
```csharp
// Had to pass config everywhere
var result = sup.InitializeProductV2(config);
if (result != InitializationResult.Success) return true;
```

### After (v3)
```csharp
// No config needed in Main action!
if (!sup.IsProductReady()) return true;
```

The config is automatically loaded from when `OpenProductSettings()` was called.

---

## Best Practices

1. **Use the same product number everywhere**
   - Settings Action: `new StreamUpLib(CPH, "PROD001")`
   - Main Action: `new StreamUpLib(CPH, "PROD001")`

2. **Export typed settings class from Builder**
   - Gives you full IntelliSense
   - Catches typos at compile time

3. **Let the DLL handle errors**
   - Don't manually check OBS connection
   - Don't manually show dialogs
   - `IsProductReady()` does it all

4. **Trust the cache**
   - First call: full validation
   - Subsequent calls: instant
   - Cache auto-invalidates when needed
