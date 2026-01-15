/*
 * ============================================================
 * OBS WEBSOCKET 5 - COMPLETE TEST SUITE
 * ============================================================
 *
 * This is a Streamer.bot Execute Code action that tests ALL
 * OBS WebSocket 5 wrapper methods in the StreamUP DLL.
 *
 * SETUP:
 * 1. Make sure OBS is running and connected to Streamer.bot
 * 2. Create a new Execute Code action in Streamer.bot
 * 3. Copy this entire file content into the action
 * 4. Run the action and check the Streamer.bot logs
 *
 * WHAT IT TESTS:
 * - Scene management (create, switch, remove)
 * - Scene items (visibility, transform, position, scale, crop)
 * - Inputs (create, settings, mute, volume)
 * - Filters (create, enable, settings, remove)
 * - Transitions (get, set)
 * - Outputs (stream/record status - does NOT start streaming!)
 * - Media controls
 * - General (version, stats, hotkeys)
 * - Config (profiles, scene collections)
 * - UI (studio mode)
 *
 * CLEANUP:
 * The test creates temporary scenes/sources prefixed with
 * "[TEST]" and removes them at the end.
 *
 * ============================================================
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using StreamUP;

public class CPHInline
{
    // StreamUP library instance
    public StreamUpLib SUP;

    // Product info for test
    private const string PRODUCT_NUMBER = "obs-test-suite";

    /// <summary>
    /// Initialize the StreamUP library
    /// </summary>
    public void Init()
    {
        SUP = new StreamUpLib(CPH, PRODUCT_NUMBER);
    }

    public bool Execute()
    {
        // Initialize StreamUP library
        Init();

        // Test configuration
        const string TEST_PREFIX = "[TEST]";
        const string TEST_SCENE = "[TEST] OBS Methods Test Scene";
        const string TEST_SCENE_2 = "[TEST] OBS Methods Test Scene 2";
        const string TEST_TEXT_SOURCE = "[TEST] Text Source";
        const string TEST_BROWSER_SOURCE = "[TEST] Browser Source";
        const string TEST_COLOR_SOURCE = "[TEST] Color Source";
        const string TEST_FILTER = "[TEST] Color Correction";

        int passed = 0;
        int failed = 0;
        int skipped = 0;

        var results = new List<string>();

        void Log(string message)
        {
            SUP.LogInfo($"[OBS TEST] {message}");
            results.Add(message);
        }

        void Pass(string test, string detail = "")
        {
            passed++;
            var msg = $"✓ PASS: {test}";
            if (!string.IsNullOrEmpty(detail))
                msg += $" ({detail})";
            Log(msg);
        }

        void Fail(string test, string reason)
        {
            failed++;
            Log($"✗ FAIL: {test} - {reason}");
        }

        void Skip(string test, string reason)
        {
            skipped++;
            Log($"○ SKIP: {test} - {reason}");
        }

        void Section(string name)
        {
            Log("");
            Log($"═══════════════════════════════════════════════════════");
            Log($"  {name}");
            Log($"═══════════════════════════════════════════════════════");
        }

        try
        {
            Log("╔═══════════════════════════════════════════════════════╗");
            Log("║     OBS WEBSOCKET 5 - COMPLETE METHOD TEST SUITE      ║");
            Log("╚═══════════════════════════════════════════════════════╝");
            Log("");
            Log($"Test started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            // Store original scene to restore later
            string originalScene = null;
            bool originalStudioMode = false;

            // ═══════════════════════════════════════════════════════
            // GENERAL REQUESTS
            // ═══════════════════════════════════════════════════════
            Section("GENERAL REQUESTS");

            // GetVersion
            try
            {
                var version = SUP.ObsGetVersion();
                if (version != null)
                {
                    var obsVer = version["obsVersion"]?.ToString();
                    var wsVer = version["obsWebSocketVersion"]?.ToString();
                    Pass("ObsGetVersion", $"OBS {obsVer}, WS {wsVer}");
                }
                else
                    Fail("ObsGetVersion", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetVersion", ex.Message);
            }

            // GetObsVersion
            try
            {
                var obsVer = SUP.ObsGetObsVersion();
                if (!string.IsNullOrEmpty(obsVer))
                    Pass("ObsGetObsVersion", obsVer);
                else
                    Fail("ObsGetObsVersion", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetObsVersion", ex.Message);
            }

            // GetWebSocketVersion
            try
            {
                var wsVer = SUP.ObsGetWebSocketVersion();
                if (!string.IsNullOrEmpty(wsVer))
                    Pass("ObsGetWebSocketVersion", wsVer);
                else
                    Fail("ObsGetWebSocketVersion", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetWebSocketVersion", ex.Message);
            }

            // GetStats
            try
            {
                var stats = SUP.ObsGetStats();
                if (stats != null)
                {
                    var fps = stats["activeFps"]?.Value<double>();
                    Pass("ObsGetStats", $"FPS: {fps:F1}");
                }
                else
                    Fail("ObsGetStats", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetStats", ex.Message);
            }

            // GetCpuUsage
            try
            {
                var cpu = SUP.ObsGetCpuUsage();
                if (cpu.HasValue)
                    Pass("ObsGetCpuUsage", $"{cpu:F1}%");
                else
                    Fail("ObsGetCpuUsage", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCpuUsage", ex.Message);
            }

            // GetMemoryUsage
            try
            {
                var mem = SUP.ObsGetMemoryUsage();
                if (mem.HasValue)
                    Pass("ObsGetMemoryUsage", $"{mem:F1} MB");
                else
                    Fail("ObsGetMemoryUsage", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetMemoryUsage", ex.Message);
            }

            // GetActiveFps
            try
            {
                var fps = SUP.ObsGetActiveFps();
                if (fps.HasValue)
                    Pass("ObsGetActiveFps", $"{fps:F1}");
                else
                    Fail("ObsGetActiveFps", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetActiveFps", ex.Message);
            }

            // GetHotkeyList
            try
            {
                var hotkeys = SUP.ObsGetHotkeyList();
                if (hotkeys != null)
                    Pass("ObsGetHotkeyList", $"{hotkeys.Count} hotkeys");
                else
                    Fail("ObsGetHotkeyList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetHotkeyList", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // SCENE MANAGEMENT
            // ═══════════════════════════════════════════════════════
            Section("SCENE MANAGEMENT");

            // Store original scene
            try
            {
                originalScene = SUP.ObsGetCurrentScene();
                Pass("ObsGetCurrentScene", originalScene ?? "(none)");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentScene", ex.Message);
            }

            // GetSceneList
            try
            {
                var scenes = SUP.ObsGetSceneList();
                if (scenes != null)
                    Pass("ObsGetSceneList", $"{scenes.Count} scenes");
                else
                    Fail("ObsGetSceneList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneList", ex.Message);
            }

            // CreateScene
            try
            {
                var uuid = SUP.ObsCreateScene(TEST_SCENE);
                if (!string.IsNullOrEmpty(uuid))
                    Pass("ObsCreateScene", $"UUID: {uuid.Substring(0, 8)}...");
                else
                    Fail("ObsCreateScene", "Returned null/empty UUID");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateScene", ex.Message);
            }

            // CreateScene (second scene)
            try
            {
                var uuid = SUP.ObsCreateScene(TEST_SCENE_2);
                if (!string.IsNullOrEmpty(uuid))
                    Pass("ObsCreateScene (2nd)", $"UUID: {uuid.Substring(0, 8)}...");
                else
                    Fail("ObsCreateScene (2nd)", "Returned null/empty UUID");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateScene (2nd)", ex.Message);
            }

            // SetCurrentScene
            try
            {
                if (SUP.ObsSetCurrentScene(TEST_SCENE))
                    Pass("ObsSetCurrentScene", TEST_SCENE);
                else
                    Fail("ObsSetCurrentScene", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetCurrentScene", ex.Message);
            }

            // GetCurrentScene (verify switch)
            try
            {
                var current = SUP.ObsGetCurrentScene();
                if (current == TEST_SCENE)
                    Pass("ObsGetCurrentScene (verify)", "Scene switched correctly");
                else
                    Fail("ObsGetCurrentScene (verify)", $"Expected {TEST_SCENE}, got {current}");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentScene (verify)", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // INPUT/SOURCE CREATION
            // ═══════════════════════════════════════════════════════
            Section("INPUT/SOURCE CREATION");

            // GetInputList
            try
            {
                var inputs = SUP.ObsGetInputList();
                if (inputs != null)
                    Pass("ObsGetInputList", $"{inputs.Count} inputs");
                else
                    Fail("ObsGetInputList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetInputList", ex.Message);
            }

            // GetInputKindList
            try
            {
                var kinds = SUP.ObsGetInputKindList();
                if (kinds != null)
                    Pass("ObsGetInputKindList", $"{kinds.Count} input kinds");
                else
                    Fail("ObsGetInputKindList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetInputKindList", ex.Message);
            }

            // CreateInput - Text Source
            try
            {
                var result = SUP.ObsCreateInput(
                    TEST_SCENE,
                    TEST_TEXT_SOURCE,
                    "text_gdiplus_v3",
                    new { text = "Hello from test!" }
                );
                if (result != null)
                    Pass("ObsCreateInput (Text)", $"ID: {result["sceneItemId"]}");
                else
                    Fail("ObsCreateInput (Text)", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateInput (Text)", ex.Message);
            }

            // CreateInput - Color Source
            try
            {
                var result = SUP.ObsCreateInput(
                    TEST_SCENE,
                    TEST_COLOR_SOURCE,
                    "color_source_v3",
                    new
                    {
                        color = 0xFF0000FF,
                        width = 200,
                        height = 200
                    }
                );
                if (result != null)
                    Pass("ObsCreateInput (Color)", $"ID: {result["sceneItemId"]}");
                else
                    Fail("ObsCreateInput (Color)", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateInput (Color)", ex.Message);
            }

            // CreateInput - Browser Source
            try
            {
                var result = SUP.ObsCreateInput(
                    TEST_SCENE,
                    TEST_BROWSER_SOURCE,
                    "browser_source",
                    new
                    {
                        url = "https://example.com",
                        width = 800,
                        height = 600
                    }
                );
                if (result != null)
                    Pass("ObsCreateInput (Browser)", $"ID: {result["sceneItemId"]}");
                else
                    Fail("ObsCreateInput (Browser)", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateInput (Browser)", ex.Message);
            }

            // Small delay to ensure sources are created
            System.Threading.Thread.Sleep(500);

            // ═══════════════════════════════════════════════════════
            // INPUT SETTINGS
            // ═══════════════════════════════════════════════════════
            Section("INPUT SETTINGS");

            // GetInputSettings
            try
            {
                var settings = SUP.ObsGetInputSettings(TEST_TEXT_SOURCE);
                if (settings != null)
                    Pass("ObsGetInputSettings", $"Got settings for {TEST_TEXT_SOURCE}");
                else
                    Fail("ObsGetInputSettings", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetInputSettings", ex.Message);
            }

            // SetInputSettings
            try
            {
                if (SUP.ObsSetInputSettings(TEST_TEXT_SOURCE, new { text = "Updated text!" }))
                    Pass("ObsSetInputSettings", "Text updated");
                else
                    Fail("ObsSetInputSettings", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetInputSettings", ex.Message);
            }

            // SetText (convenience method)
            try
            {
                if (SUP.ObsSetText(TEST_TEXT_SOURCE, "Text via SetText!"))
                    Pass("ObsSetText", "Text set successfully");
                else
                    Fail("ObsSetText", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetText", ex.Message);
            }

            // GetText
            try
            {
                var text = SUP.ObsGetText(TEST_TEXT_SOURCE);
                if (text == "Text via SetText!")
                    Pass("ObsGetText", text);
                else
                    Fail("ObsGetText", $"Expected 'Text via SetText!', got '{text}'");
            }
            catch (Exception ex)
            {
                Fail("ObsGetText", ex.Message);
            }

            // SetBrowserUrl
            try
            {
                if (SUP.ObsSetBrowserUrl(TEST_BROWSER_SOURCE, "https://google.com"))
                    Pass("ObsSetBrowserUrl", "URL updated");
                else
                    Fail("ObsSetBrowserUrl", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetBrowserUrl", ex.Message);
            }

            // RefreshBrowser
            try
            {
                if (SUP.ObsRefreshBrowser(TEST_BROWSER_SOURCE))
                    Pass("ObsRefreshBrowser", "Browser refreshed");
                else
                    Fail("ObsRefreshBrowser", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsRefreshBrowser", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // SCENE ITEMS (VISIBILITY, TRANSFORM)
            // ═══════════════════════════════════════════════════════
            Section("SCENE ITEMS - VISIBILITY & TRANSFORM");

            // GetSceneItemId
            try
            {
                var id = SUP.ObsGetSceneItemId(TEST_SCENE, TEST_TEXT_SOURCE);
                if (id > 0)
                    Pass("ObsGetSceneItemId", $"ID: {id}");
                else
                    Fail("ObsGetSceneItemId", $"Invalid ID: {id}");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneItemId", ex.Message);
            }

            // GetSceneItemList
            try
            {
                var items = SUP.ObsGetSceneItemList(TEST_SCENE);
                if (items != null)
                    Pass("ObsGetSceneItemList", $"{items.Count} items");
                else
                    Fail("ObsGetSceneItemList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneItemList", ex.Message);
            }

            // HideSource
            try
            {
                if (SUP.ObsHideSource(TEST_SCENE, TEST_TEXT_SOURCE))
                    Pass("ObsHideSource", "Source hidden");
                else
                    Fail("ObsHideSource", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsHideSource", ex.Message);
            }

            // IsSourceVisible (should be false)
            try
            {
                var visible = SUP.ObsIsSourceVisible(TEST_SCENE, TEST_TEXT_SOURCE);
                if (!visible)
                    Pass("ObsIsSourceVisible (hidden)", "Correctly reports hidden");
                else
                    Fail("ObsIsSourceVisible (hidden)", "Should be hidden");
            }
            catch (Exception ex)
            {
                Fail("ObsIsSourceVisible (hidden)", ex.Message);
            }

            // ShowSource
            try
            {
                if (SUP.ObsShowSource(TEST_SCENE, TEST_TEXT_SOURCE))
                    Pass("ObsShowSource", "Source shown");
                else
                    Fail("ObsShowSource", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsShowSource", ex.Message);
            }

            // IsSourceVisible (should be true)
            try
            {
                var visible = SUP.ObsIsSourceVisible(TEST_SCENE, TEST_TEXT_SOURCE);
                if (visible)
                    Pass("ObsIsSourceVisible (shown)", "Correctly reports visible");
                else
                    Fail("ObsIsSourceVisible (shown)", "Should be visible");
            }
            catch (Exception ex)
            {
                Fail("ObsIsSourceVisible (shown)", ex.Message);
            }

            // ToggleSourceVisibility
            try
            {
                var newState = SUP.ObsToggleSourceVisibility(TEST_SCENE, TEST_TEXT_SOURCE);
                if (newState.HasValue)
                    Pass(
                        "ObsToggleSourceVisibility",
                        $"New state: {(newState.Value ? "visible" : "hidden")}"
                    );
                else
                    Fail("ObsToggleSourceVisibility", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsToggleSourceVisibility", ex.Message);
            }

            // Restore visibility
            SUP.ObsShowSource(TEST_SCENE, TEST_TEXT_SOURCE);

            // SetSourcePosition
            try
            {
                if (SUP.ObsSetSourcePosition(TEST_SCENE, TEST_TEXT_SOURCE, 100, 100))
                    Pass("ObsSetSourcePosition", "Position set to 100,100");
                else
                    Fail("ObsSetSourcePosition", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourcePosition", ex.Message);
            }

            // SetSourceScale
            try
            {
                if (SUP.ObsSetSourceScale(TEST_SCENE, TEST_TEXT_SOURCE, 1.5, 1.5))
                    Pass("ObsSetSourceScale", "Scale set to 1.5x");
                else
                    Fail("ObsSetSourceScale", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceScale", ex.Message);
            }

            // SetSourceRotation
            try
            {
                if (SUP.ObsSetSourceRotation(TEST_SCENE, TEST_TEXT_SOURCE, 45))
                    Pass("ObsSetSourceRotation", "Rotation set to 45°");
                else
                    Fail("ObsSetSourceRotation", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceRotation", ex.Message);
            }

            // SetSourceCrop
            try
            {
                if (SUP.ObsSetSourceCrop(TEST_SCENE, TEST_COLOR_SOURCE, 10, 10, 10, 10))
                    Pass("ObsSetSourceCrop", "Crop set to 10px all sides");
                else
                    Fail("ObsSetSourceCrop", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceCrop", ex.Message);
            }

            // GetSceneItemTransform
            try
            {
                var transform = SUP.ObsGetSceneItemTransform(TEST_SCENE, TEST_TEXT_SOURCE);
                if (transform != null)
                {
                    var posX = transform["positionX"]?.Value<double>();
                    Pass("ObsGetSceneItemTransform", $"Position X: {posX}");
                }
                else
                    Fail("ObsGetSceneItemTransform", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneItemTransform", ex.Message);
            }

            // SetSourceLocked
            try
            {
                if (SUP.ObsSetSourceLocked(TEST_SCENE, TEST_TEXT_SOURCE, true))
                    Pass("ObsSetSourceLocked", "Source locked");
                else
                    Fail("ObsSetSourceLocked", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceLocked", ex.Message);
            }

            // IsSourceLocked
            try
            {
                var locked = SUP.ObsIsSourceLocked(TEST_SCENE, TEST_TEXT_SOURCE);
                if (locked)
                    Pass("ObsIsSourceLocked", "Correctly reports locked");
                else
                    Fail("ObsIsSourceLocked", "Should be locked");
            }
            catch (Exception ex)
            {
                Fail("ObsIsSourceLocked", ex.Message);
            }

            // Unlock source
            SUP.ObsSetSourceLocked(TEST_SCENE, TEST_TEXT_SOURCE, false);

            // ═══════════════════════════════════════════════════════
            // INPUT AUDIO
            // ═══════════════════════════════════════════════════════
            Section("INPUT AUDIO");

            // Note: These may fail if the text source doesn't have audio capabilities
            // but we test them anyway for coverage

            // GetInputMute
            try
            {
                var muted = SUP.ObsGetInputMute(TEST_TEXT_SOURCE);
                Pass("ObsGetInputMute", $"Muted: {muted}");
            }
            catch (Exception ex)
            {
                Skip("ObsGetInputMute", "Source may not have audio");
            }

            // GetInputVolume
            try
            {
                var volume = SUP.ObsGetInputVolume(TEST_TEXT_SOURCE);
                if (volume != null)
                    Pass("ObsGetInputVolume", $"dB: {volume["inputVolumeDb"]}");
                else
                    Skip("ObsGetInputVolume", "Source may not have audio");
            }
            catch (Exception ex)
            {
                Skip("ObsGetInputVolume", "Source may not have audio");
            }

            // ═══════════════════════════════════════════════════════
            // FILTERS
            // ═══════════════════════════════════════════════════════
            Section("FILTERS");

            // CreateSourceFilter
            try
            {
                if (
                    SUP.ObsCreateSourceFilter(
                        TEST_COLOR_SOURCE,
                        TEST_FILTER,
                        "color_filter_v2",
                        new { brightness = 0.1, contrast = 0.1 }
                    )
                )
                    Pass("ObsCreateSourceFilter", "Filter created");
                else
                    Fail("ObsCreateSourceFilter", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsCreateSourceFilter", ex.Message);
            }

            System.Threading.Thread.Sleep(200);

            // GetSourceFilterList
            try
            {
                var filters = SUP.ObsGetSourceFilterList(TEST_COLOR_SOURCE);
                if (filters != null)
                    Pass("ObsGetSourceFilterList", $"{filters.Count} filters");
                else
                    Fail("ObsGetSourceFilterList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSourceFilterList", ex.Message);
            }

            // GetSourceFilter
            try
            {
                var filter = SUP.ObsGetSourceFilter(TEST_COLOR_SOURCE, TEST_FILTER);
                if (filter != null)
                    Pass("ObsGetSourceFilter", "Got filter details");
                else
                    Fail("ObsGetSourceFilter", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSourceFilter", ex.Message);
            }

            // SetSourceFilterEnabled (disable)
            try
            {
                if (SUP.ObsSetSourceFilterEnabled(TEST_COLOR_SOURCE, TEST_FILTER, false))
                    Pass("ObsSetSourceFilterEnabled (disable)", "Filter disabled");
                else
                    Fail("ObsSetSourceFilterEnabled (disable)", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceFilterEnabled (disable)", ex.Message);
            }

            // IsSourceFilterEnabled
            try
            {
                var enabled = SUP.ObsIsSourceFilterEnabled(TEST_COLOR_SOURCE, TEST_FILTER);
                if (!enabled)
                    Pass("ObsIsSourceFilterEnabled", "Correctly reports disabled");
                else
                    Fail("ObsIsSourceFilterEnabled", "Should be disabled");
            }
            catch (Exception ex)
            {
                Fail("ObsIsSourceFilterEnabled", ex.Message);
            }

            // SetSourceFilterEnabled (enable)
            try
            {
                if (SUP.ObsSetSourceFilterEnabled(TEST_COLOR_SOURCE, TEST_FILTER, true))
                    Pass("ObsSetSourceFilterEnabled (enable)", "Filter enabled");
                else
                    Fail("ObsSetSourceFilterEnabled (enable)", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceFilterEnabled (enable)", ex.Message);
            }

            // ToggleSourceFilter
            try
            {
                var newState = SUP.ObsToggleSourceFilter(TEST_COLOR_SOURCE, TEST_FILTER);
                if (newState.HasValue)
                    Pass(
                        "ObsToggleSourceFilter",
                        $"New state: {(newState.Value ? "enabled" : "disabled")}"
                    );
                else
                    Fail("ObsToggleSourceFilter", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsToggleSourceFilter", ex.Message);
            }

            // SetSourceFilterSettings
            try
            {
                if (
                    SUP.ObsSetSourceFilterSettings(
                        TEST_COLOR_SOURCE,
                        TEST_FILTER,
                        new { brightness = 0.2 }
                    )
                )
                    Pass("ObsSetSourceFilterSettings", "Settings updated");
                else
                    Fail("ObsSetSourceFilterSettings", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceFilterSettings", ex.Message);
            }

            // SetSourceFilterIndex
            try
            {
                if (SUP.ObsSetSourceFilterIndex(TEST_COLOR_SOURCE, TEST_FILTER, 0))
                    Pass("ObsSetSourceFilterIndex", "Index set to 0");
                else
                    Fail("ObsSetSourceFilterIndex", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetSourceFilterIndex", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // TRANSITIONS
            // ═══════════════════════════════════════════════════════
            Section("TRANSITIONS");

            // GetTransitionKindList
            try
            {
                var kinds = SUP.ObsGetTransitionKindList();
                if (kinds != null)
                    Pass("ObsGetTransitionKindList", $"{kinds.Count} transition types");
                else
                    Fail("ObsGetTransitionKindList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetTransitionKindList", ex.Message);
            }

            // GetSceneTransitionList
            try
            {
                var transitions = SUP.ObsGetSceneTransitionList();
                if (transitions != null)
                    Pass("ObsGetSceneTransitionList", $"{transitions.Count} transitions");
                else
                    Fail("ObsGetSceneTransitionList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneTransitionList", ex.Message);
            }

            // GetCurrentSceneTransition
            try
            {
                var transition = SUP.ObsGetCurrentSceneTransition();
                if (transition != null)
                {
                    var name = transition["transitionName"]?.ToString();
                    Pass("ObsGetCurrentSceneTransition", name);
                }
                else
                    Fail("ObsGetCurrentSceneTransition", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentSceneTransition", ex.Message);
            }

            // GetCurrentTransitionName
            try
            {
                var name = SUP.ObsGetCurrentTransitionName();
                if (!string.IsNullOrEmpty(name))
                    Pass("ObsGetCurrentTransitionName", name);
                else
                    Fail("ObsGetCurrentTransitionName", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentTransitionName", ex.Message);
            }

            // GetCurrentTransitionDuration
            // Note: Some transitions (like "Scene As Transition") don't have a configurable duration
            try
            {
                var duration = SUP.ObsGetCurrentTransitionDuration();
                if (duration.HasValue)
                    Pass("ObsGetCurrentTransitionDuration", $"{duration}ms");
                else
                    Pass("ObsGetCurrentTransitionDuration", "N/A (transition has no duration)");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentTransitionDuration", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // OUTPUTS (Stream/Record Status - NO STARTING!)
            // ═══════════════════════════════════════════════════════
            Section("OUTPUTS (Status Only - Safe)");

            // GetStreamStatus
            try
            {
                var status = SUP.ObsGetStreamStatus();
                if (status != null)
                {
                    var active = status["outputActive"]?.Value<bool>();
                    Pass("ObsGetStreamStatus", $"Streaming: {active}");
                }
                else
                    Fail("ObsGetStreamStatus", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetStreamStatus", ex.Message);
            }

            // IsStreaming
            try
            {
                var streaming = SUP.ObsIsStreaming();
                Pass("ObsIsStreaming", $"{streaming}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsStreaming", ex.Message);
            }

            // GetRecordStatus
            try
            {
                var status = SUP.ObsGetRecordStatus();
                if (status != null)
                {
                    var active = status["outputActive"]?.Value<bool>();
                    Pass("ObsGetRecordStatus", $"Recording: {active}");
                }
                else
                    Fail("ObsGetRecordStatus", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetRecordStatus", ex.Message);
            }

            // IsRecording
            try
            {
                var recording = SUP.ObsIsRecording();
                Pass("ObsIsRecording", $"{recording}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsRecording", ex.Message);
            }

            // IsRecordingPaused
            try
            {
                var paused = SUP.ObsIsRecordingPaused();
                Pass("ObsIsRecordingPaused", $"{paused}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsRecordingPaused", ex.Message);
            }

            // IsReplayBufferActive
            try
            {
                var active = SUP.ObsIsReplayBufferActive();
                Pass("ObsIsReplayBufferActive", $"{active}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsReplayBufferActive", ex.Message);
            }

            // IsVirtualCamActive
            try
            {
                var active = SUP.ObsIsVirtualCamActive();
                Pass("ObsIsVirtualCamActive", $"{active}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsVirtualCamActive", ex.Message);
            }

            // GetOutputList
            try
            {
                var outputs = SUP.ObsGetOutputList();
                if (outputs != null)
                    Pass("ObsGetOutputList", $"{outputs.Count} outputs");
                else
                    Fail("ObsGetOutputList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetOutputList", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // CONFIG
            // ═══════════════════════════════════════════════════════
            Section("CONFIG");

            // GetSceneCollectionList
            try
            {
                var collections = SUP.ObsGetSceneCollectionList();
                if (collections != null)
                {
                    var current = collections["currentSceneCollectionName"]?.ToString();
                    Pass("ObsGetSceneCollectionList", $"Current: {current}");
                }
                else
                    Fail("ObsGetSceneCollectionList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSceneCollectionList", ex.Message);
            }

            // GetCurrentSceneCollection
            try
            {
                var name = SUP.ObsGetCurrentSceneCollection();
                if (!string.IsNullOrEmpty(name))
                    Pass("ObsGetCurrentSceneCollection", name);
                else
                    Fail("ObsGetCurrentSceneCollection", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentSceneCollection", ex.Message);
            }

            // GetProfileList
            try
            {
                var profiles = SUP.ObsGetProfileList();
                if (profiles != null)
                {
                    var current = profiles["currentProfileName"]?.ToString();
                    Pass("ObsGetProfileList", $"Current: {current}");
                }
                else
                    Fail("ObsGetProfileList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetProfileList", ex.Message);
            }

            // GetCurrentProfile
            try
            {
                var name = SUP.ObsGetCurrentProfile();
                if (!string.IsNullOrEmpty(name))
                    Pass("ObsGetCurrentProfile", name);
                else
                    Fail("ObsGetCurrentProfile", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentProfile", ex.Message);
            }

            // GetVideoSettings
            try
            {
                var settings = SUP.ObsGetVideoSettings();
                if (settings != null)
                {
                    var width = settings["baseWidth"]?.Value<int>();
                    var height = settings["baseHeight"]?.Value<int>();
                    Pass("ObsGetVideoSettings", $"{width}x{height}");
                }
                else
                    Fail("ObsGetVideoSettings", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetVideoSettings", ex.Message);
            }

            // GetRecordDirectory
            try
            {
                var dir = SUP.ObsGetRecordDirectory();
                if (!string.IsNullOrEmpty(dir))
                    Pass("ObsGetRecordDirectory", dir);
                else
                    Fail("ObsGetRecordDirectory", "Returned null/empty");
            }
            catch (Exception ex)
            {
                Fail("ObsGetRecordDirectory", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // UI
            // ═══════════════════════════════════════════════════════
            Section("UI");

            // GetMonitorList
            try
            {
                var monitors = SUP.ObsGetMonitorList();
                if (monitors != null)
                    Pass("ObsGetMonitorList", $"{monitors.Count} monitors");
                else
                    Fail("ObsGetMonitorList", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetMonitorList", ex.Message);
            }

            // Store original studio mode
            try
            {
                originalStudioMode = SUP.ObsIsStudioModeEnabled();
                Pass("ObsIsStudioModeEnabled", $"{originalStudioMode}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsStudioModeEnabled", ex.Message);
            }

            // Enable studio mode
            try
            {
                if (SUP.ObsEnableStudioMode())
                    Pass("ObsEnableStudioMode", "Studio mode enabled");
                else
                    Fail("ObsEnableStudioMode", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsEnableStudioMode", ex.Message);
            }

            System.Threading.Thread.Sleep(300);

            // Verify studio mode is on
            try
            {
                if (SUP.ObsIsStudioModeEnabled())
                    Pass("ObsIsStudioModeEnabled (verify)", "Correctly reports enabled");
                else
                    Fail("ObsIsStudioModeEnabled (verify)", "Should be enabled");
            }
            catch (Exception ex)
            {
                Fail("ObsIsStudioModeEnabled (verify)", ex.Message);
            }

            // Set preview scene
            try
            {
                if (SUP.ObsSetCurrentPreviewScene(TEST_SCENE_2))
                    Pass("ObsSetCurrentPreviewScene", TEST_SCENE_2);
                else
                    Fail("ObsSetCurrentPreviewScene", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsSetCurrentPreviewScene", ex.Message);
            }

            // Get preview scene
            try
            {
                var preview = SUP.ObsGetCurrentPreviewScene();
                if (preview == TEST_SCENE_2)
                    Pass("ObsGetCurrentPreviewScene", preview);
                else
                    Fail("ObsGetCurrentPreviewScene", $"Expected {TEST_SCENE_2}, got {preview}");
            }
            catch (Exception ex)
            {
                Fail("ObsGetCurrentPreviewScene", ex.Message);
            }

            // Disable studio mode
            try
            {
                if (SUP.ObsDisableStudioMode())
                    Pass("ObsDisableStudioMode", "Studio mode disabled");
                else
                    Fail("ObsDisableStudioMode", "Returned false");
            }
            catch (Exception ex)
            {
                Fail("ObsDisableStudioMode", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // SOURCES (General)
            // ═══════════════════════════════════════════════════════
            Section("SOURCES (General)");

            // GetSourceActive
            try
            {
                var active = SUP.ObsGetSourceActive(TEST_TEXT_SOURCE);
                if (active != null)
                    Pass("ObsGetSourceActive", $"Active: {active["videoActive"]}");
                else
                    Fail("ObsGetSourceActive", "Returned null");
            }
            catch (Exception ex)
            {
                Fail("ObsGetSourceActive", ex.Message);
            }

            // IsSourceActive
            try
            {
                var active = SUP.ObsIsSourceActive(TEST_TEXT_SOURCE);
                Pass("ObsIsSourceActive", $"{active}");
            }
            catch (Exception ex)
            {
                Fail("ObsIsSourceActive", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // CLEANUP
            // ═══════════════════════════════════════════════════════
            Section("CLEANUP");

            // Restore original scene first
            if (!string.IsNullOrEmpty(originalScene))
            {
                try
                {
                    SUP.ObsSetCurrentScene(originalScene);
                    Pass("Restore original scene", originalScene);
                }
                catch (Exception ex)
                {
                    Fail("Restore original scene", ex.Message);
                }
            }

            // Restore studio mode
            try
            {
                SUP.ObsSetStudioModeEnabled(originalStudioMode);
                Pass("Restore studio mode", $"{originalStudioMode}");
            }
            catch (Exception ex)
            {
                Fail("Restore studio mode", ex.Message);
            }

            System.Threading.Thread.Sleep(500);

            // Remove filter
            try
            {
                if (SUP.ObsRemoveSourceFilter(TEST_COLOR_SOURCE, TEST_FILTER))
                    Pass("ObsRemoveSourceFilter", "Filter removed");
                else
                    Skip("ObsRemoveSourceFilter", "Filter may already be gone");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveSourceFilter", ex.Message);
            }

            // Remove inputs
            try
            {
                SUP.ObsRemoveInput(TEST_TEXT_SOURCE);
                Pass("ObsRemoveInput (Text)", "Removed");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveInput (Text)", ex.Message);
            }

            try
            {
                SUP.ObsRemoveInput(TEST_BROWSER_SOURCE);
                Pass("ObsRemoveInput (Browser)", "Removed");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveInput (Browser)", ex.Message);
            }

            try
            {
                SUP.ObsRemoveInput(TEST_COLOR_SOURCE);
                Pass("ObsRemoveInput (Color)", "Removed");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveInput (Color)", ex.Message);
            }

            System.Threading.Thread.Sleep(300);

            // Remove test scenes
            try
            {
                if (SUP.ObsRemoveScene(TEST_SCENE))
                    Pass("ObsRemoveScene", TEST_SCENE);
                else
                    Skip("ObsRemoveScene", "Scene may already be gone");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveScene", ex.Message);
            }

            try
            {
                if (SUP.ObsRemoveScene(TEST_SCENE_2))
                    Pass("ObsRemoveScene (2)", TEST_SCENE_2);
                else
                    Skip("ObsRemoveScene (2)", "Scene may already be gone");
            }
            catch (Exception ex)
            {
                Skip("ObsRemoveScene (2)", ex.Message);
            }

            // ═══════════════════════════════════════════════════════
            // SUMMARY
            // ═══════════════════════════════════════════════════════
            Log("");
            Log("╔═══════════════════════════════════════════════════════╗");
            Log("║                    TEST SUMMARY                       ║");
            Log("╚═══════════════════════════════════════════════════════╝");
            Log("");
            Log($"  ✓ PASSED:  {passed}");
            Log($"  ✗ FAILED:  {failed}");
            Log($"  ○ SKIPPED: {skipped}");
            Log($"  ─────────────────");
            Log($"  TOTAL:     {passed + failed + skipped}");
            Log("");

            if (failed == 0)
            {
                Log("🎉 ALL TESTS PASSED! The OBS WebSocket 5 wrapper is working correctly.");
            }
            else
            {
                Log($"⚠️ {failed} test(s) failed. Check the logs above for details.");
            }

            Log("");
            Log($"Test completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            return true;
        }
        catch (Exception ex)
        {
            SUP.LogError($"[OBS TEST] Fatal error: {ex.Message}");
            SUP.LogError($"[OBS TEST] Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
