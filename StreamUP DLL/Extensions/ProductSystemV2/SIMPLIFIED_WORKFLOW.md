# Simplified Product Workflow

This document describes the simplified developer workflow for creating StreamUP products. The new system dramatically reduces boilerplate code and handles all validation/checking automatically.

## Overview

The new system provides:
- **One-liner methods** for common tasks
- **Automatic config registration** so Main actions don't need JSON
- **Validation caching** for instant subsequent runs
- **User-friendly dialogs** when issues are found
- **Prompt deduplication** to prevent spam

## Developer Workflow

### BEFORE (Old Way)

Developers had to:
1. Store the full product config JSON in every action
2. Manually call `InitializeProductV2(config)` with the full config
3. Check the result and handle each case
4. Repeat all this in every action

```csharp
// OLD WAY - Settings Action
var configJson = @"{ huge JSON blob here }";
var config = JObject.Parse(configJson);
sup.OpenSettingsMenu(config);

// OLD WAY - Main Action
var configJson = @"{ same huge JSON blob repeated }";
var config = JObject.Parse(configJson);
var result = sup.InitializeProductV2(config);
if (result != InitializationResult.Success)
    return;
// ... main action code
```

### AFTER (New Way)

Now developers use simple one-liners:

```csharp
// NEW WAY - Settings Action
// Config is only needed here - it gets auto-registered for future use
var config = JObject.Parse(@"{ product config }");
sup.OpenProductSettings(config);
// That's it! Config is now registered for Main action to use

// NEW WAY - Main Action (NO JSON NEEDED!)
if (!sup.IsProductReady())
    return;
// ... main action code
// The system automatically:
// - Loads the registered config
// - Uses cached validation if available (instant!)
// - Validates library version
// - Checks settings exist
// - For OBS products: validates OBS connection and scene
// - Shows user-friendly dialogs if issues found
```

## API Reference

### Settings Action Methods

#### `OpenProductSettings(JObject config)` / `OpenProductSettings(string configJson)`
The recommended method for opening settings. It:
1. Validates library version
2. Registers the config for future use
3. Invalidates validation cache (user might change settings)
4. Opens the settings window

```csharp
var config = JObject.Parse(@"{ ... }");
sup.OpenProductSettings(config);
```

#### `IsSettingsReady(JObject config)` / `IsSettingsReady(string configJson)`
Check if OK to open settings (validates library version only).
Use this if you want to do something before opening the window.

```csharp
if (!sup.IsSettingsReady(config))
    return;
// Do pre-settings stuff
sup.OpenSettingsMenu(config);
```

### Main Action Methods

#### `IsProductReady()` - **THE ONE METHOD YOU NEED**
Returns `true` if product is ready to run. This handles EVERYTHING:
- Uses cached validation if available (instant return on subsequent calls!)
- Loads registered product config
- Validates library version
- Checks settings exist
- For OBS products: validates OBS connection and scene
- Shows user-friendly dialogs if issues found
- Caches successful result

```csharp
if (!sup.IsProductReady())
    return;
// Your main action code here
```

#### `CheckProductReady()`
Same as `IsProductReady()` but returns detailed status instead of bool.

```csharp
var status = sup.CheckProductReady();
if (status != ProductReadyStatus.Ready)
{
    sup.LogInfo($"Product not ready: {status}");
    return;
}
```

**ProductReadyStatus values:**
- `Ready` - Product is ready to use
- `SettingsNotConfigured` - User hasn't configured settings yet
- `LibraryOutdated` - StreamUP DLL needs updating
- `ObsNotConnected` - OBS is not connected
- `ObsSceneMissing` - Required OBS scene is missing
- `ObsSceneOutdated` - OBS scene version is too old
- `ConfigNotRegistered` - Settings action hasn't run yet
- `AlreadyPrompted` - User was already prompted this session (silent)

### Helper Methods

#### `HasProductBeenConfigured()`
Quick check if product has saved settings (no dialogs).

```csharp
if (!sup.HasProductBeenConfigured())
{
    sup.LogInfo("Product not configured yet");
}
```

#### `IsProductConfigRegistered()`
Check if product config is available (Settings action has run).

```csharp
if (!sup.IsProductConfigRegistered())
{
    sup.LogInfo("Settings action needs to run first");
}
```

#### `InvalidateProductValidation()`
Force re-validation on next `IsProductReady()` call.
Use when you know something changed (e.g., OBS reconnected).

```csharp
sup.InvalidateProductValidation();
```

#### `InvalidateAllObsValidations()` (static)
Invalidate validation cache for all OBS products.
Call when OBS connection state changes globally.

```csharp
StreamUpLib.InvalidateAllObsValidations();
```

## How Config Registration Works

When `OpenSettingsMenu()` or `OpenProductSettings()` is called:
1. The config JSON is saved to `StreamUP/ProductConfigs/{productNumber}_Config.json`
2. This config is loaded by `IsProductReady()` in Main actions
3. No need to include the JSON in Main action code

The config persists across Streamer.bot restarts, so the Main action will work as long as Settings was opened at least once.

## How Validation Caching Works

After a successful `IsProductReady()` call:
1. The result is cached in memory
2. Subsequent calls return instantly (no re-validation)
3. Cache is keyed by: product number, OBS connection, settings file hash

The cache automatically invalidates when:
- Settings file changes (detected via file timestamp)
- OBS connection number changes
- User opens Settings window
- Streamer.bot restarts (session-scoped)

## Complete Example

### Settings Action (Only place you need the JSON)

```csharp
using Newtonsoft.Json.Linq;

public class CPHInline : CPHInlineBase
{
    public bool Execute()
    {
        var sup = new StreamUpLib(CPH, "myproduct001");

        var config = JObject.Parse(@"{
            ""productName"": ""My Awesome Product"",
            ""productNumber"": ""myproduct001"",
            ""productVersion"": ""1.0.0.0"",
            ""productType"": ""OBS"",
            ""author"": ""StreamUP"",
            ""libraryVersion"": ""4.0.0.0"",
            ""sceneName"": ""[SUP] My Product"",
            ""sourceName"": ""[SUP] My Product Info"",
            ""sceneVersion"": ""1.0.0.0"",
            ""settingsAction"": ""My Product - Settings"",
            ""menuName"": ""Settings"",
            ""menuDescription"": ""Configure your product"",
            ""Pages"": []
        }");

        sup.OpenProductSettings(config);
        return true;
    }
}
```

### Main Action (No JSON needed!)

```csharp
public class CPHInline : CPHInlineBase
{
    public bool Execute()
    {
        var sup = new StreamUpLib(CPH, "myproduct001");

        // ONE LINE - handles everything!
        if (!sup.IsProductReady())
            return true;

        // Your product code here
        sup.LogInfo("Product is ready, doing the thing!");

        // Get settings
        var userName = sup.GetProductSetting("userName", "Guest");
        var volume = sup.GetProductSetting("volume", 50);

        // Do stuff...

        return true;
    }
}
```

## Migration Guide

### Migrating from InitializeProductV2

**Before:**
```csharp
var config = JObject.Parse(@"{ ... }");
var result = sup.InitializeProductV2(config);
if (result != InitializationResult.Success)
    return true;
```

**After:**
```csharp
if (!sup.IsProductReady())
    return true;
```

### Migrating Settings Actions

**Before:**
```csharp
var config = JObject.Parse(@"{ ... }");
// Manual library check
if (!sup.ValidateLibraryVersionV2("4.0.0.0"))
{
    // Show dialog manually
    return true;
}
sup.OpenSettingsMenu(config);
```

**After:**
```csharp
var config = JObject.Parse(@"{ ... }");
sup.OpenProductSettings(config);
// Everything is handled automatically!
```

## Architecture

### Files

- `SimplifiedProductMethods.cs` - The new one-liner methods
- `ProductConfigRegistry.cs` - Stores/retrieves product configs
- `ProductValidationCache.cs` - Caches validation results
- `ProductInitializerV2.cs` - Core validation logic (unchanged)
- `PromptManager.cs` - Prevents dialog spam (unchanged)

### Flow Diagram

```
Settings Action:
    OpenProductSettings(config)
        ↓
    Validate library version
        ↓
    Register config to ProductConfigRegistry
        ↓
    Invalidate validation cache
        ↓
    Open settings window

Main Action:
    IsProductReady()
        ↓
    Check ProductValidationCache (fast path)
        ↓ (if not cached)
    Load config from ProductConfigRegistry
        ↓
    Run full validation (InitializeProductV2)
        ↓ (if success)
    Cache result in ProductValidationCache
        ↓
    Return true
```

## Troubleshooting

### "ConfigNotRegistered" Error
The Main action can't find the product config. Solutions:
1. Run the Settings action at least once
2. Check the product number matches in both actions
3. Verify `StreamUP/ProductConfigs/{productNumber}_Config.json` exists

### Validation Not Caching
The cache invalidates when:
- Settings file is modified
- OBS connection changes
- Settings window is opened

This is intentional - re-validation ensures fresh state.

### User Gets Multiple Prompts
The PromptManager prevents duplicate prompts per session, but:
- Different issues have different prompts
- Restarting Streamer.bot clears the prompt memory

This is intentional - users should see prompts after restart.
