// ============================================================================
// TEST PRODUCT - SETTINGS ACTION
// ============================================================================
// This demonstrates the SIMPLIFIED workflow for a Settings action.
//
// OLD WAY: Had to manually check library version, handle prompts, etc.
// NEW WAY: Just call OpenProductSettings() - everything is handled automatically!
//
// What OpenProductSettings() does automatically:
// 1. Validates library version (shows error dialog if outdated)
// 2. Registers config for future use by Main action
// 3. Invalidates validation cache (user might change settings)
// 4. Opens the settings window
// ============================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;
using StreamUP;

#if EXTERNAL_EDITOR
public class TEST001_SettingsAction : CPHInlineBase
#else
public class CPHInline
#endif
{
    // Product number - MUST match between Settings and Main actions
    private readonly string productNumber = "TEST001";

    // StreamUP library instance
    public StreamUpLib SUP;

    // Product configuration - this JSON comes from the Settings Builder export
    // In a real product, this would typically be stored in a separate file or resource
    private static readonly string ProductConfig = @"
{
    ""productName"": ""Master Component Test Suite"",
    ""productVersion"": ""1.0.0.0"",
    ""productNumber"": ""TEST001"",
    ""productType"": ""Streamer.Bot Only"",
    ""author"": ""StreamUP"",
    ""libraryVersion"": ""3.0.2.0"",
    ""settingsAction"": ""Test Settings Action"",
    ""menuName"": ""Component Test Settings"",
    ""menuDescription"": ""Comprehensive test suite for all component types and configurations"",
    ""Pages"": [
        {
            ""name"": ""Quick Start"",
            ""Settings"": [
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""markdown"",
                    ""content"": ""# Welcome to Master Component Test Suite!\n\nThis is a comprehensive test of all StreamUP Settings components.\n\n**Features:**\n- All text input formats\n- All number formats\n- Selection controls\n- Color pickers\n- Date/time pickers\n- File selectors\n- Integration controls\n- Layout components"",
                    ""variant"": ""info""
                },
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""separator"",
                    ""style"": ""line""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Product Enabled"",
                    ""description"": ""Enable or disable this product"",
                    ""type"": ""boolean"",
                    ""defaultValue"": true,
                    ""backendName"": ""productEnabled""
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Display Name"",
                    ""description"": ""Your display name shown in outputs"",
                    ""type"": ""text"",
                    ""defaultValue"": ""StreamUser"",
                    ""backendName"": ""displayName"",
                    ""required"": true,
                    ""config"": {
                        ""textFormat"": ""text"",
                        ""placeholder"": ""Enter your name..."",
                        ""maxLength"": 50
                    }
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Volume Level"",
                    ""description"": ""Master volume for sounds"",
                    ""type"": ""number"",
                    ""defaultValue"": 75,
                    ""backendName"": ""volumeLevel"",
                    ""config"": {
                        ""numberFormat"": ""percentage"",
                        ""min"": 0,
                        ""max"": 100,
                        ""step"": 5
                    }
                },
                {
                    ""componentCategory"": ""setting"",
                    ""friendlyName"": ""Theme Color"",
                    ""description"": ""Primary color for UI elements"",
                    ""type"": ""color"",
                    ""defaultValue"": ""#6af4ff"",
                    ""backendName"": ""themeColor"",
                    ""config"": {
                        ""format"": ""hex"",
                        ""showAlpha"": false
                    }
                }
            ]
        },
        {
            ""name"": ""Advanced"",
            ""Settings"": [
                {
                    ""componentCategory"": ""layout"",
                    ""type"": ""group"",
                    ""group"": ""Debug Options"",
                    ""collapsed"": true,
                    ""settings"": [
                        {
                            ""componentCategory"": ""setting"",
                            ""friendlyName"": ""Debug Mode"",
                            ""description"": ""Enable verbose logging"",
                            ""type"": ""boolean"",
                            ""defaultValue"": false,
                            ""backendName"": ""debugMode""
                        },
                        {
                            ""componentCategory"": ""setting"",
                            ""friendlyName"": ""Log Level"",
                            ""description"": ""Select logging verbosity"",
                            ""type"": ""select"",
                            ""defaultValue"": ""info"",
                            ""backendName"": ""logLevel"",
                            ""config"": {
                                ""selectStyle"": ""dropdown"",
                                ""options"": [
                                    { ""value"": ""error"", ""label"": ""Error Only"" },
                                    { ""value"": ""warn"", ""label"": ""Warnings"" },
                                    { ""value"": ""info"", ""label"": ""Info"" },
                                    { ""value"": ""debug"", ""label"": ""Debug"" }
                                ]
                            }
                        }
                    ]
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
        // Initialize StreamUP library
        Init();

        SUP.LogInfo("=== TEST PRODUCT: Settings Action Starting ===");

        // =====================================================================
        // SIMPLIFIED WORKFLOW - ONE LINE!
        // =====================================================================
        // This single call does EVERYTHING:
        // - Validates library version
        // - Registers config for Main action to use later
        // - Opens the settings window
        //
        // If library version check fails, it shows a user-friendly dialog
        // and returns false - no settings window opens.
        // =====================================================================

        var config = JObject.Parse(ProductConfig);
        bool success = SUP.OpenProductSettings(config);

        if (success)
        {
            SUP.LogInfo("Settings window opened successfully");
        }
        else
        {
            SUP.LogInfo("Settings window could not be opened (library check failed?)");
        }

        return true;
    }
}

// ============================================================================
// ALTERNATIVE: If you need to do something BEFORE opening settings
// ============================================================================
//
// public bool Execute()
// {
//     Init();
//     var config = JObject.Parse(ProductConfig);
//
//     // Check if ready (validates library version)
//     if (!SUP.IsSettingsReady(config))
//     {
//         SUP.LogInfo("Library check failed");
//         return true;
//     }
//
//     // Do some pre-settings work here...
//     SUP.LogInfo("Doing pre-settings initialization...");
//
//     // Now open settings
//     SUP.OpenSettingsMenu(config);
//
//     return true;
// }
// ============================================================================
