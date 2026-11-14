# Byte World: rules for every agent session

Byte World is a short, landscape, bright pixel-art story game for teens (13–17) about cybersecurity, built for The Big Hack 2026 (Consorzio CLARA / Cisco). A tiny town lives inside your Wi-Fi. Four job Bytes (Scout checks boats, Gigi keeps the gate, Uno and Due guard the vault, Patch fixes the wall) handle incoming data. Mishandle it and an attacker snatches the Byte; one of three shared backups takes over; run out and Byte World collapses. Day 1 is a tutorial, Day 2 has double alerts, Day 3 is a tap-only network build with a final test run. The full spec is the PRD (v1.3); `STREAMS.md` says who owns what.

## Pillars (PRD 4.1)

1. **Jobs, not lessons.** No security or networking words on Days 1 and 2. Real terms appear only on Day 3 reveal cards.
2. **Notice what's off.** Most incoming items are fine; a few are slightly wrong.
3. **Loss you can feel**, never scary or gory.
4. **Short and readable.** One bubble per line (max ~110 characters). Shifts 30–60 s, puzzles 1–2 min.
5. **Safe isn't blocking everything.** Blocking safe items fills the complaints meter.

## Stack (state this in every prompt)

Unity **6000.6.9f1** (everyone on exactly this version), URP 2D, uGUI + TextMeshPro, **WebGL** target, landscape, **tap input only** via the uGUI EventSystem (`IPointerClickHandler`). The project uses the Input System package only, so the EventSystem needs `InputSystemUIInputModule`. Pixel Perfect Camera 480 × 270, 16 PPU, Point filter, no compression. Canvas Scaler: Scale With Screen Size, 1920 × 1080, match 0.5. WebGL rules: no threads, no file system, no `System.IO` at runtime, keep allocations low.

## Folder structure

```
Assets/_Project/
  Scripts/
    Core/         GameState.cs, Events.cs, Services.cs, Palette.cs, Bootstrap.cs   (shared contracts)
    Core/Contracts/  interfaces + ComplaintRules                                     (shared contracts)
    Core/Data/    JSON data classes                                                  (shared contracts)
    Core/Stubs/   stub implementations                                               (shared contracts)
    Core/         GameManager.cs, DayManager.cs, BackupNest.cs, DataLoader.cs        (Stream A)
    World/        WorldMap, RoamingByte, JobByte, AlertController                    (Stream A)
    Jobs/         JobShift, ItemQueue, ScoutJob, GigiJob, TwinsJob, PatchJob         (Stream A)
    Debug/        DebugStart, debug menu  (namespace ByteWorld.DebugTools)            (Stream A)
    UI/           DialogueBubble, InspectPanel, TopBar, DayCard, RevealCard, Screens/ (Stream B)
    Build/        BuildManager, NetworkGraph, BuildSlot, CableTool, DoorRules, TrafficRun (Stream B)
    Editor/       editor tools (FoundationSetup, FakeEventPanel, PixelArtImporter)
  Scenes/  Prefabs/  Art/Sprites/  Art/Palettes/  Audio/
  Resources/Data/   days.json, dialogue.json, roaming.json, build.json (Content C), flags.json (Stream A)
  Resources/Core/   GameRoot.prefab (Stream A)
  Resources/UI/     UIRoot.prefab   (Stream B)
Tools/sprites/      grids/*.txt, make_sprites.py, quantize.py (Content C)
```

Namespaces: `ByteWorld`, `ByteWorld.Data`, `ByteWorld.Stubs`, `ByteWorld.EditorTools`, `ByteWorld.DebugTools`. **Never name a namespace `ByteWorld.Debug`**: it hides `UnityEngine.Debug` everywhere.

## Architecture (PRD 6.4)

- **Shared contracts live in `Scripts/Core/` (GameState, Events, Services, Palette, Bootstrap, Contracts/, Data/, Stubs/).** They compile with no gameplay logic. See the contract-change rule below.
- **Startup:** `Bootstrap` instantiates `Resources/Core/GameRoot.prefab` (A) and `Resources/UI/UIRoot.prefab` (B) before the first scene loads. Each root registers its implementations with `Services.Register<T>(this)` in `Awake`. Anything not registered falls back to a stub, so each stream runs alone.
- **Services:** `Services.Backups` (IBackupNest, A), `Services.Data` (IGameData, A), `Services.Flow` (IGameFlow, A), `Services.Dialogue` (IDialogueService, B), `Services.Inspect` (IInspectPanel, B), `Services.BuildMode` (IBuildMode, B).
- **Events:** `GameEvents` in `Events.cs`. Gameplay raises, UI listens. **Gameplay never references UI types**; it calls the interfaces. UI talks back only through `GameEvents.RaiseUiAction(UiAction)`, `RaiseRestartDay()` and `RaiseDebugMenu()`.
- **Backups:** every snatch (shift failure, complaints failure, Day 3 leak) goes through `Services.Backups.ReportSnatch(...)`. Only the nest raises `Snatched`, `BackupsChanged` and `Collapsed`. Day 1 tutorial mistakes never call it.
- **Complaints:** use `ComplaintRules.ShiftFailed` (shifts) and `ComplaintRules.RunPasses` (test runs). Never re-implement the rule.
- **Day 3:** GameManager calls `Services.BuildMode.Begin(startPuzzle)`; build mode raises `PuzzleStarted`, `TestRunFinished`, `PuzzlePassed`, `RevealClosed`; after the last `RevealClosed`, GameManager moves to Peace.
- **Fallback:** `flags.json` `"day3Mode": "fallback"` makes Day 3 the final attack (Stream A, PRD 4.7). Flipped at the Sunday 08:00 cut-off.
- **No saving.** Reloading the page restarts the game.
- **Testing without the other stream:** open **Byte World > Fake Event Panel** in Play mode to fire any event.

## Data (PRD 6.5)

All content lives in `Resources/Data/*.json`, loaded with `Resources.Load<TextAsset>` + `JsonUtility`. The data classes in `Scripts/Core/Data/` are the schema. JsonUtility rules: public fields only, no dictionaries, **no nested arrays** (that's why `alertGroups` is `[{ "jobs": [...] }]`), missing fields get default values. Changing a data class is a contract change. Check content with **Byte World > Validate Data**.

**Never put game text in code.** Every player-facing string, including button labels and HUD formats, comes from `dialogue.json` (`ui` block, `jobs[].primaryAction`, and so on). The only exception is the developer-only IMGUI text in the stubs and the editor tools.

## Palette (PRD 6.6)

Use `ByteWorld.Palette` (`Palette.Ink`, `Palette.AlertPink`, `Palette.GridNormal`...). Never type hex colours anywhere else in code. Sprite grids use indices 1–5: ink, blue, cyan, cream, pink.

## Ownership (PRD 6.2)

| Area | Owner |
|---|---|
| `World.unity`, `Job_Scout/Gigi/Twins/Patch.unity`, `Resources/Core/GameRoot.prefab`, `Scripts/Core/` (non-contract files), `World/`, `Jobs/`, `Debug/`, `flags.json` | **Stream A** (person A) |
| `Boot.unity`, `Build.unity`, `Resources/UI/UIRoot.prefab`, every UI prefab, `Scripts/UI/`, `Scripts/Build/`, WebGL template and build settings | **Stream B** (person B) |
| `days.json`, `dialogue.json`, `roaming.json`, `build.json`, `Art/`, `Audio/`, `Tools/sprites/` | **Content C** (through `c/*` branches) |
| `Scripts/Core/` contract files (`GameState.cs`, `Events.cs`, `Services.cs`, `Palette.cs`, `Bootstrap.cs`, `Contracts/`, `Data/`, `Stubs/`) | **Shared**, contract-change rule |

**Nobody edits a scene, prefab or folder they don't own.** Need something from the other stream? Open an issue for its owner, or put it in your own prefab and hand it over.

## Rules for agents

- **Never hand-edit `.unity` or `.prefab` YAML.** Scene and prefab changes go through the Unity MCP (the Editor makes the change) or a human, and only in scenes and prefabs your stream owns. Save the scene and commit after each working MCP change.
- **After every compile, read the Unity Console through MCP** (`Unity_ReadConsole`) and fix every new error before moving on. Don't report a task done with Console errors.
- Don't use the MCP for Project Settings changes (agree them as a team), other people's scenes, or large batch deletes.
- Work from your stream's GitHub issues. Small tasks, frequent commits, commit messages `area: what changed` (for example `scout: inspect panel shows sender`).
- If you go in circles for more than 15 minutes: stop, revert, and split the task smaller.
- The game never calls an AI service at runtime.

## Contract change rule

A stream that needs to change a shared contract (anything in the **Shared** row above, or a JSON schema):

1. Opens an issue labelled **`contract-change`** describing the change and why.
2. Makes the change in a **small PR straight to `main`** (branch `contract/<short-name>`), compiling with no Console errors, with stubs updated to match.
3. The other stream **merges `main` into its stream branch before continuing**.

**Never change contracts inside a feature branch.** Adding a new event or interface member counts as a contract change.

## Branches and merge windows (PRD 10.2)

- `main` is always playable; no direct pushes. Stream branches: `stream/a-game-core`, `stream/b-screens-build`. Feature branches off the stream branch: `a/scout-shift`, `b/dialogue-bubble`; content: `c/data-day2`.
- Merge windows: Saturday 13:00, 16:00, 19:00 (tag `playable-sat`), 22:00; Sunday 08:30, 10:30. Feature freeze Sunday 10:00 (tag `freeze`), final build tagged `demo`.
- At each window: every stream merges `main` into its stream branch, compiles, reads the Console, runs the reviewer subagent, then opens a PR. Merge order: **A, then B (after merging main again), then content.** Use merge commits, not squash, for stream PRs.
