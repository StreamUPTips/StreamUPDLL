// ============================================================================
// TEST PRODUCT - OBS PRODUCT EXAMPLE
// ============================================================================
// This demonstrates the simplified workflow for an OBS-type product.
//
// For OBS products, IsProductReady() ALSO validates:
// - OBS WebSocket connection is active
// - Required scene exists
// - Scene version is correct (if specified)
//
// All handled automatically - no extra code needed!
// ============================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;
using StreamUP;

// ============================================================================
// SETTINGS ACTION
// ============================================================================

#if EXTERNAL_EDITOR
public class OBSTEST001_SettingsAction : CPHInlineBase
#else
public class CPHInline
#endif
{
    // Product number
    private readonly string productNumber = "OBSTEST001";

    // StreamUP library instance
    public StreamUpLib SUP;

    // OBS Product Configuration
    // Note the key differences from Streamer.Bot Only:
    // - productType: "OBS"
    // - sceneName: Required OBS scene
    // - sourceName: Source to check for version
    // - sceneVersion: Minimum scene version required
    private static readonly string ProductConfig = @"
{
    ""productName"": ""OBS Test Product"",
    ""productVersion"": ""2.0.0.0"",
    ""productNumber"": ""OBSTEST001"",
    ""productType"": ""OBS"",
    ""author"": ""StreamUP"",
    ""libraryVersion"": ""3.0.2.0"",
    ""settingsAction"": ""OBS Test - Settings Saved"",
    ""sceneName"": ""[SUP] OBS Test Scene"",
    ""sourceName"": ""[SUP] OBS Test Info"",
    ""sceneVersion"": ""2.0.0.0"",
    ""menuName"": ""OBS Test Settings"",
    ""menuDescription"": ""Test product demonstrating OBS integration"",
    ""Pages"": [
        {
            ""name"": ""General"",
            ""Settings"": [
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""markdown"",
                    ""content"": ""# OBS Test Product\n\nThis product requires:\n- OBS connected via WebSocket\n- Scene: **[SUP] OBS Test Scene**\n- Scene version: **2.0.0.0** or higher"",
                    ""variant"": ""info""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Auto-Switch Scene"",
                    ""description"": ""Automatically switch to product scene"",
                    ""type"": ""boolean"",
                    ""defaultValue"": true,
                    ""backendName"": ""autoSwitchScene""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Transition Duration"",
                    ""description"": ""Scene transition time in milliseconds"",
                    ""type"": ""number"",
                    ""defaultValue"": 300,
                    ""backendName"": ""transitionDuration"",
                    ""config"": {
                        ""numberFormat"": ""integer"",
                        ""min"": 0,
                        ""max"": 5000,
                        ""step"": 100
                    }
                }
            ]
        },
        {
            ""name"": ""OBS Settings"",
            ""Settings"": [
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""label"",
                    ""text"": ""OBS Scene Selection"",
                    ""style"": ""heading""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Target Scene"",
                    ""description"": ""Scene to switch to when triggered"",
                    ""type"": ""obs-scene"",
                    ""defaultValue"": """",
                    ""backendName"": ""targetScene"",
                    ""config"": {
                        ""placeholder"": ""Select Scene..."",
                        ""allowEmpty"": true
                    }
                },
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""separator"",
                    ""style"": ""line""
                },
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""label"",
                    ""text"": ""OBS Source Selection"",
                    ""style"": ""heading""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Media Source"",
                    ""description"": ""Source to control"",
                    ""type"": ""obs-source"",
                    ""defaultValue"": """",
                    ""backendName"": ""mediaSource"",
                    ""config"": {
                        ""placeholder"": ""Select Source..."",
                        ""allowEmpty"": true
                    }
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Apply Filter"",
                    ""description"": ""Filter to apply when activated"",
                    ""type"": ""obs-filter"",
                    ""defaultValue"": """",
                    ""backendName"": ""applyFilter"",
                    ""config"": {
                        ""placeholder"": ""Select Filter..."",
                        ""allowEmpty"": true
                    }
                }
            ]
        }
    ]
}";

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
        Init();

        SUP.LogInfo("=== OBS TEST PRODUCT: Settings Action ===");

        var config = JObject.Parse(ProductConfig);
        SUP.OpenProductSettings(config);

        return true;
    }
#if EXTERNAL_EDITOR
}
#endif


// ============================================================================
// MAIN ACTION
// ============================================================================
// The Main Action is EXACTLY THE SAME as a Streamer.Bot Only product!
// IsProductReady() automatically handles all OBS checks.
// ============================================================================

#if EXTERNAL_EDITOR
public class OBSTEST001_MainAction : CPHInlineBase
{
#else
// Note: In Streamer.bot inline, this would be in a separate action file
// public class CPHInline
#endif
    // Product number - MUST match Settings action

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
        Init();

        SUP.LogInfo("=== OBS TEST PRODUCT: Main Action Starting ===");

        // =====================================================================
        // SAME ONE-LINER - OBS CHECKS ARE AUTOMATIC!
        // =====================================================================
        // For OBS products, IsProductReady() ALSO checks:
        // - OBS is connected on the user's selected connection
        // - The required scene exists
        // - The scene version is up-to-date
        //
        // If any check fails, the user sees a friendly dialog.
        // =====================================================================

        if (!SUP.IsProductReady())
        {
            SUP.LogInfo("Product not ready (OBS not connected or scene missing?)");
            return true;
        }

        SUP.LogInfo("Product ready! OBS is connected and scene exists.");

        // Get the OBS connection index the user selected
        int obsConnection = SUP.GetProductObsConnection();
        SUP.LogInfo($"Using OBS connection: {obsConnection}");

        // Get settings
        bool autoSwitch = SUP.GetProductSetting("autoSwitchScene", true);
        int transitionMs = SUP.GetProductSetting("transitionDuration", 300);
        string targetScene = SUP.GetProductSetting("targetScene", "");
        string mediaSource = SUP.GetProductSetting("mediaSource", "");
        string applyFilter = SUP.GetProductSetting("applyFilter", "");

        SUP.LogInfo($"Settings:");
        SUP.LogInfo($"  - Auto-Switch: {autoSwitch}");
        SUP.LogInfo($"  - Transition: {transitionMs}ms");
        SUP.LogInfo($"  - Target Scene: {targetScene}");
        SUP.LogInfo($"  - Media Source: {mediaSource}");
        SUP.LogInfo($"  - Apply Filter: {applyFilter}");

        // =====================================================================
        // YOUR OBS PRODUCT LOGIC HERE
        // =====================================================================

        if (autoSwitch && !string.IsNullOrEmpty(targetScene))
        {
            // Switch to the selected scene
            CPH.ObsSetScene(targetScene, obsConnection);
            SUP.LogInfo($"Switched to scene: {targetScene}");
        }

        if (!string.IsNullOrEmpty(mediaSource))
        {
            // Show the media source
            CPH.ObsSetSourceVisibility(targetScene, mediaSource, true, obsConnection);
            SUP.LogInfo($"Showed source: {mediaSource}");
        }

        SUP.LogInfo("=== OBS TEST PRODUCT: Main Action Complete ===");

        return true;
    }
}


// ============================================================================
// UNDERSTANDING OBS VALIDATION
// ============================================================================
//
// When productType is "OBS", IsProductReady() checks:
//
// 1. LIBRARY VERSION
//    - Checks if installed DLL meets minimum version requirement
//    - Shows error dialog if outdated
//
// 2. SETTINGS EXIST
//    - Checks if user has saved settings
//    - Shows prompt dialog if not configured
//
// 3. OBS CONNECTION (OBS products only)
//    - Gets the user's selected OBS connection (0-4)
//    - Checks if that OBS instance is connected
//    - Shows warning dialog if not connected
//
// 4. SCENE EXISTS (if sceneName specified)
//    - Checks if the required scene exists in OBS
//    - Shows warning dialog if scene is missing
//
// 5. SCENE VERSION (if sceneName + sourceName + sceneVersion specified)
//    - Gets the source's "product_version" property
//    - Compares against required version
//    - Shows warning dialog if scene is outdated
//
// All of this happens in ONE CALL: IsProductReady()
// ============================================================================
