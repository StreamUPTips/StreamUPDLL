# WebView2Settings - Complete Settings Management System

## Overview

A comprehensive, well-organized settings management system for StreamUP products with:
- ✅ **Automatic caching** with transparent hit/miss
- ✅ **30 methods** for complete settings lifecycle
- ✅ **Organized into 7 operation files** for clarity
- ✅ **Backup/restore** functionality
- ✅ **Product discovery** utilities
- ✅ **Consistent logging** (Error/Info/Debug)
- ✅ **Type-safe** generic methods
- ✅ **Atomic file + cache updates**

---

## Folder Structure

```
WebView2Settings/
├── README.md (this file)
├── CHANGELOG.md (what's new in this refactoring)
├── _README_V2_SYSTEM.md (detailed architecture guide)
├── _SYSTEM_OVERVIEW.md (system design overview)
├── _CACHING_CONTROL.md (caching behavior explained)
│
├── Core Files
│   ├── SettingsControllerV2.cs (directory/core utilities)
│   ├── SettingsCacheV2.cs (cache storage)
│   ├── ProductInitializerV2.cs (product validation)
│   └── SettingsMenuV2.cs (WebView2 UI)
│
└── Operation Files
    ├── SettingsLoadV2.cs (Load all types)
    ├── SettingsSaveV2.cs (Save all types)
    ├── SettingsUpdateV2.cs (Update individual settings)
    ├── SettingsObjectV2.cs (In-memory JObject helpers)
    ├── SettingsCacheManagementV2.cs (Cache operations)
    └── SettingsUtilityV2.cs (High-level flows + utilities)
```

---

## Quick Start

### 1. Load Settings Efficiently
```csharp
// These load ONLY what you need (not the full product)
JObject productInfo = SUP.LoadProductInfoV2("sup001");
JObject settings = SUP.LoadSettingsV2("sup001");
int obsConnection = SUP.LoadObsConnectionV2("sup001");

// All automatically cached on first load!
```

### 2. Modify In Memory
```csharp
SUP.SetSettingInObjectV2(settings, "myKey", "myValue");
```

### 3. Save Back (Updates file AND cache)
```csharp
SUP.SaveSettingsV2("sup001", settings);
SUP.SaveProductInfoV2("sup001", productInfo);
SUP.SaveObsConnectionV2("sup001", obsConnection);
```

### 4. Use High-Level Pattern
```csharp
SUP.LoadAndApplyProductSettingsV2("sup001", (settings) =>
{
    int obs = SUP.GetSettingFromObjectV2(settings, "ObsConnection", 0);
    // Apply settings...
    return true;
});
```

### 5. Initialize with User Feedback (Recommended)
```csharp
public bool Execute()
{
    string productNumber = "sup001";
    string settingsActionName = "sup001_Settings";  // Hardcode your settings action name

    // Step 1: Check initialization - prompts user if needed (once per session)
    var initResult = SUP.CheckAndPromptProductInitializationV2(productNumber);

    if (initResult == InitializationPromptResult.NotInitializedUserAccepted)
    {
        // User clicked Yes - run your settings action
        CPH.RunAction(settingsActionName);
        return false;  // Settings will initialize on next run
    }

    if (initResult == InitializationPromptResult.NotInitializedUserDeclined)
    {
        return false;  // User declined or already prompted
    }

    // Step 2: Validate OBS connection - warns if disconnected (once per session)
    SUP.ValidateObsConnectionV2(productNumber);

    // Step 3: Continue with normal product logic
    JObject productData = SUP.LoadProductDataV2(productNumber);
    // ... rest of code
}
```

---

## File-by-File Guide

### **ProductInitializerV2.cs**
Product initialization and validation with user feedback.
```csharp
// Validation
bool IsProductInitializedV2(productNumber)
bool InitializeProductV2(productNumber, out errorMessage)

// Initialization with User Prompts
InitializationPromptResult CheckAndPromptProductInitializationV2(productNumber)
bool ValidateObsConnectionV2(productNumber)

// Result Enum
public enum InitializationPromptResult
{
    AlreadyInitialized,              // Product is already initialized
    NotInitializedUserAccepted,      // User clicked Yes to run settings
    NotInitializedUserDeclined       // User clicked No or already prompted
}
```

- **CheckAndPromptProductInitializationV2()** - Checks if product is initialized. If not, prompts user to run settings (once per session). Returns `InitializationPromptResult` enum indicating the status and user's choice. Product handles running the settings action.
- **ValidateObsConnectionV2()** - Validates OBS connection for initialized product. Shows warning if disconnected (once per session). Non-blocking - doesn't prevent product execution.

### **SettingsLoadV2.cs**
Load operations - pulling data from files with optional caching.
```csharp
JObject LoadProductDataV2(productNumber, useCache=true)
JObject LoadProductInfoV2(productNumber, useCache=true)
JObject LoadSettingsV2(productNumber, useCache=true)
int LoadObsConnectionV2(productNumber, useCache=true)
T LoadSettingV2<T>(productNumber, key, defaultValue)
```

### **SettingsSaveV2.cs**
Save operations - pushing data to files and cache atomically.
```csharp
bool SaveProductDataV2(productNumber, data, useCache=true)
bool SaveProductInfoV2(productNumber, productInfo)
bool SaveSettingsV2(productNumber, settings)
bool SaveObsConnectionV2(productNumber, obsConnection)
```

### **SettingsUpdateV2.cs**
Quick updates for individual settings (load → modify → save).
```csharp
bool UpdateSettingV2(productNumber, key, value)
bool UpdateSettingsV2(productNumber, dictOfSettings)
```

### **SettingsObjectV2.cs**
In-memory operations (no file I/O - use before saving).
```csharp
T GetSettingFromObjectV2<T>(jObject, key, defaultValue)
void SetSettingInObjectV2<T>(jObject, key, value)
```

### **SettingsCacheManagementV2.cs**
Cache inspection and control.
```csharp
void InvalidateCacheV2(productNumber)
void InvalidateAllCacheV2()
bool IsCachedV2(productNumber)
int GetCachedProductCountV2()
```

### **SettingsUtilityV2.cs**
High-level flows and utility operations.
```csharp
// High-level pattern
bool LoadAndApplyProductSettingsV2(productNumber, callback)

// Validation
bool ValidateProductSettingsV2(productNumber)

// Discovery
List<string> GetAllProductsV2()
Dictionary<string, string> GetAllProductNamesV2()
Dictionary<string, JObject> LoadAllProductsV2()
bool ProductExistsV2(productNumber)

// Backup/Restore
bool BackupProductV2(productNumber, filePath)
bool RestoreProductV2(filePath, productNumber)
bool ExportProductV2(productNumber, filePath)
bool ImportProductV2(filePath, productNumber)

// Cleanup
bool DeleteProductV2(productNumber)
```

### **SettingsControllerV2.cs**
Core utilities (directory access, file paths).
```csharp
bool GetStreamerBotDirectory(out string directory)
```

---

## Method Reference by Category

### Initialization (4 methods) - Check & validate products with user feedback
- `IsProductInitializedV2()` - Check if product data exists and is valid
- `InitializeProductV2()` - Initialize product, returns error message
- `CheckAndPromptProductInitializationV2()` - Check & prompt user if not initialized. Returns `InitializationPromptResult` enum. Product handles settings action. (anti-spam)
- `ValidateObsConnectionV2()` - Validate OBS connection, warn if disconnected (anti-spam)

### Load (5 methods) - Pull data from disk
- `LoadProductDataV2()` - Complete product JSON
- `LoadProductInfoV2()` - Just productInfo section
- `LoadSettingsV2()` - Just settings section
- `LoadObsConnectionV2()` - Just OBS connection
- `LoadSettingV2<T>()` - Just one setting

### Save (4 methods) - Push to disk and cache
- `SaveProductDataV2()` - Complete product
- `SaveProductInfoV2()` - Update productInfo
- `SaveSettingsV2()` - Update settings
- `SaveObsConnectionV2()` - Update OBS connection

### Update (2 methods) - Quick modify+save
- `UpdateSettingV2()` - One setting
- `UpdateSettingsV2()` - Multiple settings

### Object (2 methods) - In-memory only
- `GetSettingFromObjectV2<T>()` - Get from JObject
- `SetSettingInObjectV2<T>()` - Set in JObject

### Cache (4 methods) - Manage memory
- `InvalidateCacheV2()` - Clear one
- `InvalidateAllCacheV2()` - Clear all
- `IsCachedV2()` - Check status
- `GetCachedProductCountV2()` - Get count

### Utility (11 methods) - Helpers
- `LoadAndApplyProductSettingsV2()` - Apply pattern
- `ValidateProductSettingsV2()` - Validate
- `GetAllProductsV2()` - Find all
- `GetAllProductNamesV2()` - Names dict
- `LoadAllProductsV2()` - Load all
- `BackupProductV2()` - Backup
- `RestoreProductV2()` - Restore
- `ExportProductV2()` - Export
- `ImportProductV2()` - Import
- `DeleteProductV2()` - Delete
- `ProductExistsV2()` - Exists check

---

## Cache Behavior

### Automatic Population
```csharp
// First load: Reads file, caches result
var data = SUP.LoadSettingsV2("sup001");  // 10ms, cached

// Second load: Returns from cache
var data = SUP.LoadSettingsV2("sup001");  // <1ms

// Force fresh: Reload from file
var data = SUP.LoadSettingsV2("sup001", useCache: false);  // 10ms
```

### Save Syncs Cache
```csharp
// Saves to file AND updates cache atomically
bool success = SUP.SaveSettingsV2("sup001", settings);
```

---

## Logging Levels

```
LogError()  → Errors, validation failures, exceptions
LogInfo()   → Core operations (saves complete, loads complete)
LogDebug()  → Verbose traces (cache hits, memory ops)
```

---

## Common Patterns

### Pattern 1: Initialize Product with User Feedback (RECOMMENDED)
```csharp
public bool Execute()
{
    string productNumber = "sup001";
    string settingsActionName = "sup001_Settings";  // Your hardcoded settings action

    // Step 1: Check initialization - shows prompt once if not initialized
    var initResult = SUP.CheckAndPromptProductInitializationV2(productNumber);

    // Step 2: Handle result
    if (initResult == InitializationPromptResult.NotInitializedUserAccepted)
    {
        // User clicked Yes - run your settings action
        CPH.RunAction(settingsActionName);
        return false;  // Settings will initialize on next run
    }

    if (initResult == InitializationPromptResult.NotInitializedUserDeclined)
    {
        return false;  // User declined or already prompted this session
    }

    // Step 3: Validate OBS - warns once if disconnected
    SUP.ValidateObsConnectionV2(productNumber);

    // Step 4: Continue with product logic
    JObject data = SUP.LoadProductDataV2(productNumber);
    // ... rest of code
}
```

**Features**:
- Prompts user to run settings if product not initialized
- Only prompts once per session (anti-spam via static tracker)
- Product controls when settings action is triggered
- Hardcode your own settings action name for reliability
- Warns if OBS not connected (one warning per session)
- Warnings don't block product execution

### Pattern 2: Load, Modify, Save
```csharp
JObject settings = SUP.LoadSettingsV2("sup001");
SUP.SetSettingInObjectV2(settings, "key", "value");
SUP.SaveSettingsV2("sup001", settings);
```

### Pattern 3: Safe Backup Before Changes
```csharp
SUP.BackupProductV2("sup001", "C:\\backup\\sup001.json");
SUP.UpdateSettingV2("sup001", "key", "value");
// If problem: SUP.RestoreProductV2("C:\\backup\\sup001.json", "sup001");
```

### Pattern 4: Discover & Process All
```csharp
var products = SUP.GetAllProductsV2();
var names = SUP.GetAllProductNamesV2();
foreach (var prod in products)
{
    LogInfo($"Processing {names[prod]}");
    // ...
}
```

### Pattern 5: High-Level Apply
```csharp
SUP.LoadAndApplyProductSettingsV2("sup001", (settings) =>
{
    string theme = SUP.GetSettingFromObjectV2(settings, "theme", "light");
    // Apply theme...
    return true;
});
```

---

## Data Structure

Each product has this JSON structure:

```json
{
  "productInfo": {
    "productNumber": "sup001",
    "productName": "My Product",
    "productVersion": "1.0.0",
    "libraryVersion": "3.0.0.2",
    "sceneName": "Scene Name",
    "sceneVersion": "1.0.0",
    "settingsAction": "Action Name",
    "saveAction": { "actionName": "PostSaveAction" },
    "saveExecuteMethod": { "codeFile": "file.cs", "methodName": "Method" }
  },
  "obsConnection": 0,
  "settings": {
    "setting1": "value1",
    "setting2": 123,
    "setting3": true,
    "CustomKey": "customValue"
  }
}
```

---

## Performance

| Operation | Time | Source |
|-----------|------|--------|
| Load (cache hit) | <1ms | Memory |
| Load (cache miss) | 5-10ms | Disk |
| Save | 10-20ms | File I/O |
| Backup | 10-30ms | File I/O |

---

## Documentation Index

- **README.md** (this file) - Overview and quick start
- **CHANGELOG.md** - What changed in this refactoring
- **_README_V2_SYSTEM.md** - Detailed architecture guide
- **_SYSTEM_OVERVIEW.md** - System design overview
- **_CACHING_CONTROL.md** - Caching behavior deep dive

---

## Key Changes from Previous Version

✅ **Removed**:
- Backward compatibility aliases
- Ambiguous method names
- Monolithic single file

✅ **Added**:
- 10+ utility methods
- Organized into operation files
- Backup/restore functionality
- Product discovery
- Consistent logging levels

✅ **Improved**:
- Cache auto-population (transparent)
- File organization (7 files, clear purpose)
- Method naming (explicit intent)
- API clarity (30 methods, well-organized)

---

## Questions?

Refer to `_README_V2_SYSTEM.md` for architectural details.
