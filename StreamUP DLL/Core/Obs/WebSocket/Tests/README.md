# OBS WebSocket 5 Test Suite

This folder contains test code for the OBS WebSocket 5 wrapper methods. These files are **NOT** compiled as part of the DLL - they are meant to be copied into Streamer.bot Execute Code actions.

## Files

### ObsMethodsTestSuite.cs

A comprehensive test suite that tests ALL OBS WebSocket 5 methods.

**How to use:**
1. Open Streamer.bot
2. Create a new Action
3. Add an "Execute Code" sub-action
4. Copy the entire contents of `ObsMethodsTestSuite.cs` into the code editor
5. Add reference to `StreamUP.dll` in the Execute Code references
6. Make sure OBS is running and connected
7. Run the action
8. Check the Streamer.bot logs for results (View > Logs or Ctrl+L)

**Code Structure:**
```csharp
using StreamUP;

public class CPHInline
{
    public StreamUpLib SUP;

    public void Init()
    {
        SUP = new StreamUpLib(CPH, "obs-test-suite");
    }

    public bool Execute()
    {
        Init();
        // Tests use SUP.ObsXxx() methods and SUP.LogInfo()
        return true;
    }
}
```

**What it tests:**
- ✅ General (version, stats, hotkeys)
- ✅ Scenes (create, switch, remove)
- ✅ Inputs (create, settings, text, browser)
- ✅ Scene Items (visibility, transform, position, scale, crop, lock)
- ✅ Filters (create, enable, settings, remove)
- ✅ Transitions (get current, list)
- ✅ Outputs (stream/record/replay/virtualcam STATUS - does NOT start them!)
- ✅ Config (profiles, scene collections, video settings)
- ✅ UI (studio mode, monitors)

**What it creates (and cleans up):**
- `[TEST] OBS Methods Test Scene` - temporary scene
- `[TEST] OBS Methods Test Scene 2` - temporary scene
- `[TEST] Text Source` - temporary text source
- `[TEST] Browser Source` - temporary browser source
- `[TEST] Color Source` - temporary color source
- `[TEST] Color Correction` - temporary filter

All test objects are prefixed with `[TEST]` and are removed at the end of the test.

**Expected output:**
```
[OBS TEST] ╔═══════════════════════════════════════════════════════╗
[OBS TEST] ║     OBS WEBSOCKET 5 - COMPLETE METHOD TEST SUITE      ║
[OBS TEST] ╚═══════════════════════════════════════════════════════╝
[OBS TEST]
[OBS TEST] ═══════════════════════════════════════════════════════
[OBS TEST]   GENERAL REQUESTS
[OBS TEST] ═══════════════════════════════════════════════════════
[OBS TEST] ✓ PASS: ObsGetVersion (OBS 30.2.2, WS 5.5.2)
[OBS TEST] ✓ PASS: ObsGetObsVersion (30.2.2)
...
[OBS TEST] ╔═══════════════════════════════════════════════════════╗
[OBS TEST] ║                    TEST SUMMARY                       ║
[OBS TEST] ╚═══════════════════════════════════════════════════════╝
[OBS TEST]
[OBS TEST]   ✓ PASSED:  75
[OBS TEST]   ✗ FAILED:  0
[OBS TEST]   ○ SKIPPED: 5
[OBS TEST]   ─────────────────
[OBS TEST]   TOTAL:     80
[OBS TEST]
[OBS TEST] 🎉 ALL TESTS PASSED! The OBS WebSocket 5 wrapper is working correctly.
```

## Safety Notes

- The test does **NOT** start streaming or recording
- The test only checks status of outputs (IsStreaming, IsRecording, etc.)
- All created objects are cleaned up at the end
- Your original scene is restored after the test
- Your studio mode state is restored after the test
