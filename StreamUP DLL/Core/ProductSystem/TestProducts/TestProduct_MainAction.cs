// ============================================================================
// TEST PRODUCT - MAIN ACTION
// ============================================================================
// This demonstrates the SIMPLIFIED workflow for a Main action.
//
// OLD WAY: Had to pass the ENTIRE config JSON again, manually check results
// NEW WAY: Just call IsProductReady() - NO CONFIG NEEDED!
//
// What IsProductReady() does automatically:
// 1. Uses cached validation if available (INSTANT return on subsequent calls!)
// 2. Loads registered product config (from when Settings action ran)
// 3. Validates library version
// 4. Checks settings exist (prompts user if not)
// 5. For OBS products: validates OBS connection and scene
// 6. Shows user-friendly dialogs if issues found
// 7. Caches successful result for future calls
// ============================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;
using StreamUP;

#if EXTERNAL_EDITOR
public class TEST001_MainAction : CPHInlineBase
#else
public class CPHInline
#endif
{
    // Product number - MUST match the Settings action
    private readonly string productNumber = "TEST001";

    // StreamUP library instance
    public StreamUpLib SUP;

    /// <summary>
    /// Initialize the StreamUP library
    /// </summary>
    public void Init()
    {
        SUP = new StreamUpLib(CPH, productNumber);
    }

    /// <summary>
    /// Main execution method
    /// </summary>
    public bool Execute()
    {
        // Initialize StreamUP library
        Init();

        SUP.LogInfo("=== TEST PRODUCT: Main Action Starting ===");

        // =====================================================================
        // SIMPLIFIED WORKFLOW - ONE LINE, NO CONFIG!
        // =====================================================================
        // This single call does EVERYTHING:
        // - Loads config that was registered when Settings action ran
        // - Validates library version
        // - Checks if settings have been configured
        // - Caches result for instant subsequent calls
        //
        // On FIRST call: Full validation (a few ms)
        // On SUBSEQUENT calls: Instant return from cache!
        // =====================================================================

        if (!SUP.IsProductReady())
        {
            // User was shown a dialog explaining the issue
            // Just return - nothing more to do
            SUP.LogInfo("Product not ready, exiting");
            return true;
        }

        SUP.LogInfo("Product is ready! Proceeding with main logic...");

        // =====================================================================
        // OPTION 1: Get individual settings (simple approach)
        // =====================================================================

        bool productEnabled = SUP.GetProductSetting("productEnabled", true);
        string displayName = SUP.GetProductSetting("displayName", "Guest");
        int volumeLevel = SUP.GetProductSetting("volumeLevel", 75);
        string themeColor = SUP.GetProductSetting("themeColor", "#6af4ff");
        bool debugMode = SUP.GetProductSetting("debugMode", false);
        string logLevel = SUP.GetProductSetting("logLevel", "info");

        SUP.LogInfo($"Settings loaded:");
        SUP.LogInfo($"  - Product Enabled: {productEnabled}");
        SUP.LogInfo($"  - Display Name: {displayName}");
        SUP.LogInfo($"  - Volume Level: {volumeLevel}%");
        SUP.LogInfo($"  - Theme Color: {themeColor}");
        SUP.LogInfo($"  - Debug Mode: {debugMode}");
        SUP.LogInfo($"  - Log Level: {logLevel}");

        // Check if product is enabled
        if (!productEnabled)
        {
            SUP.LogInfo("Product is disabled, exiting");
            return true;
        }

        // =====================================================================
        // YOUR MAIN PRODUCT LOGIC GOES HERE
        // =====================================================================

        SUP.LogInfo($"Hello, {displayName}! Welcome to the test product.");
        SUP.LogInfo($"Playing sound at {volumeLevel}% volume...");
        SUP.LogInfo($"Using theme color: {themeColor}");

        if (debugMode)
        {
            SUP.LogDebug($"Debug logging enabled at level: {logLevel}");
        }

        // Example: Set a global variable for other actions to use
        CPH.SetGlobalVar("TEST001_LastRun", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        CPH.SetGlobalVar("TEST001_DisplayName", displayName);

        SUP.LogInfo("=== TEST PRODUCT: Main Action Complete ===");

        return true;
    }
}

// ============================================================================
// ALTERNATIVE: Using Typed Settings (full IntelliSense support!)
// ============================================================================
//
// Add this class to your code (or use the exported typed class):
//
// public class TestProductSettings
// {
//     [JsonProperty("productEnabled")]
//     public bool ProductEnabled { get; set; } = true;
//
//     [JsonProperty("displayName")]
//     public string DisplayName { get; set; } = "StreamUser";
//
//     [JsonProperty("volumeLevel")]
//     public int VolumeLevel { get; set; } = 75;
//
//     [JsonProperty("themeColor")]
//     public string ThemeColor { get; set; } = "#6af4ff";
//
//     [JsonProperty("debugMode")]
//     public bool DebugMode { get; set; } = false;
//
//     [JsonProperty("logLevel")]
//     public string LogLevel { get; set; } = "info";
// }
//
// Then use it like this:
//
// public bool Execute()
// {
//     Init();
//
//     if (!SUP.IsProductReady())
//         return true;
//
//     // Get all settings as a typed object - full IntelliSense!
//     var settings = SUP.GetTypedSettings<TestProductSettings>();
//
//     // Now you have full IntelliSense:
//     if (!settings.ProductEnabled)
//         return true;
//
//     SUP.LogInfo($"Hello, {settings.DisplayName}!");
//     SUP.LogInfo($"Volume: {settings.VolumeLevel}%");
//     SUP.LogInfo($"Theme: {settings.ThemeColor}");
//
//     // You can also modify and save:
//     // settings.DisplayName = "NewName";
//     // SUP.SaveTypedSettings(settings);
//
//     return true;
// }
// ============================================================================


// ============================================================================
// ALTERNATIVE: Get detailed status instead of just bool
// ============================================================================
//
// public bool Execute()
// {
//     Init();
//
//     var status = SUP.CheckProductReady();
//
//     switch (status)
//     {
//         case ProductReadyStatus.Ready:
//             // Proceed with normal operation
//             break;
//
//         case ProductReadyStatus.ConfigNotRegistered:
//             SUP.LogError("Settings action needs to run first!");
//             // Maybe run the settings action?
//             CPH.RunAction("TEST001 - Settings");
//             break;
//
//         case ProductReadyStatus.SettingsNotConfigured:
//             SUP.LogInfo("User needs to configure settings");
//             break;
//
//         case ProductReadyStatus.ObsNotConnected:
//             SUP.LogWarning("OBS is not connected");
//             break;
//
//         case ProductReadyStatus.AlreadyPrompted:
//             // User was already shown a prompt this session
//             // Silent failure - don't spam dialogs
//             break;
//
//         default:
//             SUP.LogInfo($"Product not ready: {status}");
//             break;
//     }
//
//     return true;
// }
// ============================================================================
