// ============================================================================
// TEST PRODUCT - TYPED SETTINGS EXAMPLE
// ============================================================================
// This demonstrates using typed settings classes for full IntelliSense support.
//
// Benefits of typed settings:
// - Full IntelliSense/autocomplete in your IDE
// - Compile-time type checking
// - Cleaner code with properties instead of string keys
// - Easy to modify and save back
// ============================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;
using StreamUP;

/// <summary>
/// Typed settings class - matches the settings in the product config.
/// Export this from Settings Builder using "Export Typed C#" option.
/// </summary>
public class TestProductSettings
{
    // Quick Start page
    [JsonProperty("productEnabled")]
    public bool ProductEnabled { get; set; } = true;

    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = "StreamUser";

    [JsonProperty("volumeLevel")]
    public int VolumeLevel { get; set; } = 75;

    [JsonProperty("themeColor")]
    public string ThemeColor { get; set; } = "#6af4ff";

    // Advanced page - Debug group
    [JsonProperty("debugMode")]
    public bool DebugMode { get; set; } = false;

    [JsonProperty("logLevel")]
    public string LogLevel { get; set; } = "info";
}


#if EXTERNAL_EDITOR
public class TEST001_TypedSettingsDemo : CPHInlineBase
#else
public class CPHInline
#endif
{
    // Product number
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
        Init();

        SUP.LogInfo("=== TEST PRODUCT: Typed Settings Demo ===");

        // Check if product is ready
        if (!SUP.IsProductReady())
        {
            return true;
        }

        // =====================================================================
        // GET TYPED SETTINGS - Full IntelliSense!
        // =====================================================================

        var settings = SUP.GetTypedSettings<TestProductSettings>();

        // Now you have full IntelliSense/autocomplete:
        SUP.LogInfo($"Product Enabled: {settings.ProductEnabled}");
        SUP.LogInfo($"Display Name: {settings.DisplayName}");
        SUP.LogInfo($"Volume Level: {settings.VolumeLevel}%");
        SUP.LogInfo($"Theme Color: {settings.ThemeColor}");
        SUP.LogInfo($"Debug Mode: {settings.DebugMode}");
        SUP.LogInfo($"Log Level: {settings.LogLevel}");

        // Use settings in your logic
        if (!settings.ProductEnabled)
        {
            SUP.LogInfo("Product is disabled");
            return true;
        }

        // Conditional logic based on settings
        if (settings.VolumeLevel > 0)
        {
            SUP.LogInfo($"Playing sound at {settings.VolumeLevel}% volume");
            // CPH.PlaySound(..., settings.VolumeLevel / 100.0f);
        }

        if (settings.DebugMode)
        {
            SUP.LogDebug($"Debug mode active - Log level: {settings.LogLevel}");
        }

        // =====================================================================
        // MODIFY AND SAVE SETTINGS
        // =====================================================================

        // You can modify settings and save them back:
        settings.DisplayName = $"{settings.DisplayName} (Last run: {DateTime.Now:HH:mm})";

        // Save the modified settings
        SUP.SaveTypedSettings(settings);
        SUP.LogInfo("Settings updated and saved");

        SUP.LogInfo("=== Typed Settings Demo Complete ===");

        return true;
    }
}


// ============================================================================
// FULL MASTER COMPONENT TEST SUITE TYPED SETTINGS
// ============================================================================
// This is the complete typed class for the full test suite.
// Copy this from the Settings Builder export.
// ============================================================================

/// <summary>
/// Full typed settings class for Master Component Test Suite
/// Includes all 60+ settings from the comprehensive test suite
/// </summary>
public class MasterComponentTestSuiteSettings
{
    #region Text Inputs

    [JsonProperty("textSingleLine")]
    public string TextSingleLine { get; set; } = "Default text value";

    [JsonProperty("textPassword")]
    public string TextPassword { get; set; } = "";

    [JsonProperty("textMultiline")]
    public string TextMultiline { get; set; } = "Line 1\nLine 2\nLine 3";

    [JsonProperty("textUrl")]
    public string TextUrl { get; set; } = "https://example.com";

    [JsonProperty("textEmail")]
    public string TextEmail { get; set; } = "user@example.com";

    [JsonProperty("textIpAddress")]
    public string TextIpAddress { get; set; } = "127.0.0.1";

    [JsonProperty("textJson")]
    public string TextJson { get; set; } = "{\n  \"key\": \"value\",\n  \"number\": 42\n}";

    #endregion

    #region Numbers

    [JsonProperty("numInteger")]
    public int NumInteger { get; set; } = 42;

    [JsonProperty("numDecimal")]
    public double NumDecimal { get; set; } = 3.14159;

    [JsonProperty("numSlider")]
    public int NumSlider { get; set; } = 50;

    [JsonProperty("numPercentage")]
    public double NumPercentage { get; set; } = 75.0;

    [JsonProperty("numCurrencyUsd")]
    public double NumCurrencyUsd { get; set; } = 99.99;

    [JsonProperty("numCurrencyEur")]
    public double NumCurrencyEur { get; set; } = 49.95;

    [JsonProperty("numCurrencyGbp")]
    public double NumCurrencyGbp { get; set; } = 29.99;

    [JsonProperty("numLabeledSliderQuality")]
    public int NumLabeledSliderQuality { get; set; } = 2;

    [JsonProperty("numLabeledSliderSpeed")]
    public int NumLabeledSliderSpeed { get; set; } = 1;

    [JsonProperty("numLabeledSliderPriority")]
    public int NumLabeledSliderPriority { get; set; } = 2;

    #endregion

    #region Selection

    [JsonProperty("boolEnabledOn")]
    public bool BoolEnabledOn { get; set; } = true;

    [JsonProperty("boolEnabledOff")]
    public bool BoolEnabledOff { get; set; } = false;

    [JsonProperty("selectDropdown")]
    public string SelectDropdown { get; set; } = "option2";

    [JsonProperty("selectRadio")]
    public string SelectRadio { get; set; } = "medium";

    [JsonProperty("selectButtons")]
    public string SelectButtons { get; set; } = "center";

    [JsonProperty("multiselectCheckboxes")]
    public List<string> MultiselectCheckboxes { get; set; } = new List<string> { "feature1", "feature3" };

    [JsonProperty("multiselectTags")]
    public List<string> MultiselectTags { get; set; } = new List<string> { "red", "blue" };

    #endregion

    #region Colors

    [JsonProperty("colorHex")]
    public string ColorHex { get; set; } = "#6af4ff";

    [JsonProperty("colorHexAlpha")]
    public string ColorHexAlpha { get; set; } = "#ff6b6b80";

    [JsonProperty("colorRgb")]
    public string ColorRgb { get; set; } = "#4ecdc4";

    [JsonProperty("colorRgba")]
    public string ColorRgba { get; set; } = "#95e1d3";

    [JsonProperty("colorObs")]
    public long ColorObs { get; set; } = 0L;

    [JsonProperty("colorObsAlpha")]
    public long ColorObsAlpha { get; set; } = 0L;

    #endregion

    #region Date & Time

    [JsonProperty("dateOnly")]
    public string DateOnly { get; set; } = "2025-12-25";

    [JsonProperty("timeOnly")]
    public string TimeOnly { get; set; } = "14:30";

    [JsonProperty("dateTime")]
    public string DateTime { get; set; } = "2025-12-31T23:59";

    [JsonProperty("duration")]
    public string Duration { get; set; } = "01:30:00";

    #endregion

    #region Files

    [JsonProperty("fileAny")]
    public string FileAny { get; set; } = "";

    [JsonProperty("fileImage")]
    public string FileImage { get; set; } = "";

    [JsonProperty("fileAudio")]
    public string FileAudio { get; set; } = "";

    [JsonProperty("fileVideo")]
    public string FileVideo { get; set; } = "";

    [JsonProperty("fileFont")]
    public string FileFont { get; set; } = "";

    [JsonProperty("fileFolder")]
    public string FileFolder { get; set; } = "";

    [JsonProperty("keybindWithDefault")]
    public string KeybindWithDefault { get; set; } = "Ctrl+Shift+H";

    [JsonProperty("keybindNoDefault")]
    public string KeybindNoDefault { get; set; } = "";

    #endregion

    #region Data Structures

    [JsonProperty("tableSimple")]
    public List<Dictionary<string, object>> TableSimple { get; set; } = new List<Dictionary<string, object>>();

    [JsonProperty("tableMultiCol")]
    public List<Dictionary<string, object>> TableMultiCol { get; set; } = new List<Dictionary<string, object>>();

    [JsonProperty("tableSceneLinked")]
    public List<Dictionary<string, object>> TableSceneLinked { get; set; } = new List<Dictionary<string, object>>();

    [JsonProperty("listText")]
    public List<string> ListText { get; set; } = new List<string> { "First item", "Second item", "Third item" };

    [JsonProperty("listNumber")]
    public List<string> ListNumber { get; set; } = new List<string> { "100", "200", "300", "500" };

    [JsonProperty("keyValuePairs")]
    public Dictionary<string, string> KeyValuePairs { get; set; } = new Dictionary<string, string>();

    #endregion

    #region Groups

    [JsonProperty("groupUsername")]
    public string GroupUsername { get; set; } = "StreamUser";

    [JsonProperty("groupNotifications")]
    public bool GroupNotifications { get; set; } = true;

    [JsonProperty("groupDebugMode")]
    public bool GroupDebugMode { get; set; } = false;

    [JsonProperty("groupLogLevel")]
    public string GroupLogLevel { get; set; } = "info";

    [JsonProperty("groupMaxLogSize")]
    public int GroupMaxLogSize { get; set; } = 10;

    [JsonProperty("groupPrimaryColor")]
    public string GroupPrimaryColor { get; set; } = "#3498db";

    [JsonProperty("groupSecondaryColor")]
    public string GroupSecondaryColor { get; set; } = "#2ecc71";

    #endregion

    #region Validation

    [JsonProperty("valRequiredText")]
    public string ValRequiredText { get; set; } = "";

    [JsonProperty("valRequiredNumber")]
    public int ValRequiredNumber { get; set; } = 0;

    [JsonProperty("valRequiredSelect")]
    public string ValRequiredSelect { get; set; } = "";

    [JsonProperty("valBoundedInt")]
    public int ValBoundedInt { get; set; } = 5;

    [JsonProperty("valStepValue")]
    public double ValStepValue { get; set; } = 1.0;

    [JsonProperty("valNegativeRange")]
    public int ValNegativeRange { get; set; } = 0;

    [JsonProperty("valShortText")]
    public string ValShortText { get; set; } = "";

    [JsonProperty("valLongText")]
    public string ValLongText { get; set; } = "";

    #endregion

    #region Integrations

    [JsonProperty("obsSceneTarget")]
    public string ObsSceneTarget { get; set; } = "";

    [JsonProperty("obsSceneOptional")]
    public string ObsSceneOptional { get; set; } = "";

    [JsonProperty("obsSourceMedia")]
    public string ObsSourceMedia { get; set; } = "";

    [JsonProperty("obsSourceOptional")]
    public string ObsSourceOptional { get; set; } = "";

    [JsonProperty("obsFilterColor")]
    public string ObsFilterColor { get; set; } = "";

    [JsonProperty("obsFilterOptional")]
    public string ObsFilterOptional { get; set; } = "";

    [JsonProperty("sbActionTrigger")]
    public string SbActionTrigger { get; set; } = "";

    [JsonProperty("sbActionEnabled")]
    public string SbActionEnabled { get; set; } = "";

    [JsonProperty("sbActionGroup")]
    public string SbActionGroup { get; set; } = "";

    [JsonProperty("sbCommand")]
    public string SbCommand { get; set; } = "";

    [JsonProperty("sbCommandEnabled")]
    public string SbCommandEnabled { get; set; } = "";

    [JsonProperty("sbUserGroup")]
    public string SbUserGroup { get; set; } = "";

    [JsonProperty("sbUserGroupOptional")]
    public string SbUserGroupOptional { get; set; } = "";

    [JsonProperty("twitchReward")]
    public string TwitchReward { get; set; } = "";

    [JsonProperty("twitchRewardEnabled")]
    public string TwitchRewardEnabled { get; set; } = "";

    [JsonProperty("groupedObsScene")]
    public string GroupedObsScene { get; set; } = "";

    [JsonProperty("groupedSbAction")]
    public string GroupedSbAction { get; set; } = "";

    [JsonProperty("groupedTwitchReward")]
    public string GroupedTwitchReward { get; set; } = "";

    #endregion
}
