---
name: ui-screens
description: Stream B (screens, flow, Day 3 build) rules for Byte World — UIRoot, dialogue bubble, inspect panel, top bar, every screen, Day 3 build mode (BuildManager, NetworkGraph, DoorRules, TrafficRun), audio hooks, WebGL template and build. Use for any work in Scripts/UI, Scripts/Build, Boot.unity or Build.unity.
---

# ui-screens (Stream B)

Stack: Unity 6000.6.9f1, uGUI, TextMeshPro with a pixel font (Pixelify Sans or Silkscreen), URP 2D, WebGL.

**UI**
- One persistent `Resources/UI/UIRoot.prefab`: Screen Space Overlay canvas, Canvas Scaler Scale With Screen Size 1920 × 1080, match 0.5; EventSystem with `InputSystemUIInputModule`; safe-area panel for phone notches.
- Implement and register `IDialogueService` (bubble, tap to continue, max ~110 characters, `ShowHint` with a pointer and "Try again") and `IInspectPanel` (title, up to four fields, two buttons).
- Screens listen to `GameEvents` and send button presses back with `GameEvents.RaiseUiAction(UiAction.X)`. They never contain gameplay logic and never call job scripts. Top bar: day and progress label, backup nest (greys on `BackupsChanged`), complaints pips (`Complaint`).
- Every string comes from `dialogue.json` (`ui` block for buttons and formats). Colours come from `Palette`.
- Title logo: five taps raise `GameEvents.RaiseDebugMenu()`. Collapse "Start again" raises `RaiseRestartDay()`.

**Day 3 build mode (PRD 4.10)**
- `BuildManager` implements `IBuildMode`, loads puzzles only from `build.json`. `NetworkGraph`, `DoorRules` and `TrafficRun` are plain C# classes (no MonoBehaviour), so the outcome rules can be unit tested; scene objects only display and take taps. Fixed slots, no dragging.
- Packet outcomes: Delivered, Lost (complaint), Blocked (complaint), Leaked → `Services.Backups.ReportSnatch(new SnatchInfo { source = SnatchSource.TestRun, ... })`. Pass with `ComplaintRules.RunPasses`.
- Raise `PuzzleStarted`, `TestRunFinished`, `PuzzlePassed`, `RevealClosed`. GameManager (Stream A) decides Peace.
- The blueprint overlay reads the world map art (sprites from C). It doesn't open or edit `World.unity`.

**WebGL (PRD 6.8):** Gzip with Decompression Fallback, Data Caching on, strip engine code, managed stripping Medium or High, landscape lock template, build under ~20 MB.

Read the Unity Console through MCP after every compile.

**Must not:** edit job logic (`Scripts/Jobs`, `Scripts/World`, GameManager, BackupNest), `World.unity` or job scenes, C's JSON content, or shared contracts outside a contract-change PR.
