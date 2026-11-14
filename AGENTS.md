<!-- Generated from CLAUDE.md for Codex. Edit CLAUDE.md, then re-copy: keep both identical apart from this header and the Codex note. -->

# Byte World: rules for every agent session (Codex copy)

Byte World is a landscape, pixel-art story game in which Cisco Networking Academy learners (16+) defend a town that lives inside their Wi-Fi router, built for The Big Hack 2026 (Consorzio CLARA / Cisco, Challenge 4 "Hack the Route"). The player is **Pax**, a visiting learner who walks the town from Router Plaza to seven Cisco stations (Umbrella, Secure Firewall, Duo, Secure Endpoint, Splunk, Catalyst switches, ThousandEyes and Webex). At each station a popup offers **Go in** (play the station's shift, hunt, build or siege) and **See materials** (a half-screen lesson with the town without and with the tool). The game runs as four quests on the existing day and puzzle machinery; every finished station adds its tool to Pax's **Cisco toolkit**, and the game ends with a Learner Card. **The spec is `PRD-v2.md` (v2.1)**; `STREAMS.md` says who owns what. The v1.3 PRD is retired.

## Pillars (PRD-v2 4.1)

1. **Real tools, real terms, from minute one.** Every station names its Cisco tool and the concept behind it.
2. **Notice what's off.** Most items are fine, a few are subtly wrong, and one per station looks wrong but is fine.
3. **Loss you can feel**, never scary or gory.
4. **Short and readable.** One bubble per line (about 110 characters), cards under 140 a line. Shifts 45–60 s.
5. **Safe isn't blocking everything.** Blocking real citizens fills the Complaints Meter.
6. **Every quest proves learning.** A Field Card and a 2-question check close each quest.
7. **If we can't film it, it doesn't ship.** Every feature must read clearly in a 1920 × 1080 capture.
8. **Show the value, not the logo.** Each tool is shown as the same piece of town without it and with it. Only true claims (PRD-v2 4.11), product names in text, no logos.

## Stack (state this in every prompt)

Unity **6000.6.9f1** (everyone on exactly this version), URP 2D, uGUI + TextMeshPro, **WebGL** target, landscape, **tap input** via the uGUI EventSystem (`IPointerClickHandler`); the only keyboard use is the hero's `Hero` action map (Move WASD/arrows, Interact Enter/E, Materials M, Back Esc), and every UI action stays tappable. The project uses the Input System package only, so the EventSystem needs `InputSystemUIInputModule`. Pixel Perfect Camera 480 × 270, 16 PPU, Point filter, no compression. Canvas Scaler: Scale With Screen Size, 1920 × 1080, match 0.5. WebGL rules: no threads, no file system, no `System.IO` at runtime, keep allocations low.

## Folder structure

```
Assets/_Project/
  Scripts/
    Core/         GameState.cs, Events.cs, Services.cs, Palette.cs, Bootstrap.cs   (shared contracts)
    Core/Contracts/  interfaces + ComplaintRules                                     (shared contracts)
    Core/Data/    JSON data classes                                                  (shared contracts)
    Core/Stubs/   stub implementations                                               (shared contracts)
    Core/         GameManager.cs, DayManager.cs, BackupNest.cs, DataLoader.cs        (Stream A)
    World/        WorldMap, RoamingByte, JobByte, AlertController, HeroController, CheckpointManager (A)
    Jobs/         JobShift, ItemQueue, ScoutJob, GigiJob, TwinsJob, PatchJob         (Stream A)
    Debug/        DebugStart, debug menu  (namespace ByteWorld.DebugTools)            (Stream A)
    UI/           DialogueBubble, InspectPanel, TopBar, RevealCard, Screens/, Checkpoints/ (B); UI/Hunt/ (A)
    Build/        BuildManager, NetworkGraph, BuildSlot, CableTool, DoorRules, TrafficRun (Stream B)
    Editor/       editor tools (FoundationSetup, FakeEventPanel, PixelArtImporter)
  Scenes/  Prefabs/  Art/Sprites/  Art/Palettes/  Audio/
  Resources/Data/   days.json, dialogue.json, roaming.json, build.json, flags.json, hunt.json, checkpoints.json (all A)
  Resources/Core/   GameRoot.prefab (Stream A)
  Resources/UI/     UIRoot.prefab   (Stream B)
Tools/sprites/      grids/*.txt, make_sprites.py, quantize.py (A: all art)
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

## Ownership (PRD-v2 6.2): two people, no Content C

| Area | Owner |
|---|---|
| **All `Resources/Data/*.json`** (days, dialogue, roaming, build, flags, hunt, checkpoints) | **A** |
| `Scripts/Core/` (non-contract), `World/` (incl. `HeroController`, `CheckpointManager`), `Jobs/`, `Debug/`, `GameRoot.prefab`, `World.unity`, `Job_*.unity` | **A** |
| The Splunk Trace module: `Scripts/UI/Hunt/`, `Editor/HuntBuilder.cs`, `Resources/UI/Hunt.prefab` | **A** |
| **All art**: `Tools/sprites/`, `Resources/Sprites/`, `Art/Sprites/` (Pax, Bytes, stations, props) | **A** |
| `Scripts/UI/` (everything else, incl. `CheckpointPopup`, `MaterialsPanel`, `ToolkitPanel`, `StationTransition`), `UIRootBuilder.cs`, `UIRoot.prefab`, `Scripts/Build/`, `Boot.unity`, `Build.unity`, WebGL template, `Art/Fonts/` | **B** |
| Contract files (`GameState`, `Events`, `Services`, `Contracts/`, `Data/`, `Stubs/`, `Palette`, `Bootstrap`) | **A**, one PR per change (see below) |

**Nobody edits a file they don't own.** B never edits JSON: ask A. Need something from the other person? Open an issue for them, or hand it over in your own file.

## Rules for agents

- **Codex note:** you do not have the Unity MCP. Never create or change scenes, prefabs or Project Settings; write C# and JSON only, and ask the owner to wire scene changes.

- **Never hand-edit `.unity` or `.prefab` YAML.** Scene and prefab changes go through the Unity MCP (the Editor makes the change) or a human, and only in scenes and prefabs your stream owns. Save the scene and commit after each working MCP change.
- **After every compile, read the Unity Console through MCP** (`Unity_ReadConsole`) and fix every new error before moving on. Don't report a task done with Console errors.
- Don't use the MCP for Project Settings changes (agree them as a team), other people's scenes, or large batch deletes.
- Work from your person's v2 issue checklist (one issue each). Small tasks, frequent commits, commit messages `area: what changed` (for example `scout: inspect panel shows sender`).
- If you go in circles for more than 15 minutes: stop, revert, and split the task smaller.
- The game never calls an AI service at runtime.

## Contract change rule

v2 has two planned contract PRs, both by A, each one PR straight to `v2dev` with stubs updated (PRD-v2 6.4):

- **`contract/quests-v2`**: `ItemResolvedInfo` gains `{ day, title, attacker, logLine }`; `GameEvents.PropTapped(string)`; `IHuntScreen.Begin(Action<string> onPinned)`; the PRD-v2 6.5 data classes.
- **`contract/hero`**: `CheckpointState { Locked, Alert, Done }`; `GameEvents.CheckpointStatesChanged()`, `HeroArrived(string id)`, `HeroLeft(string id)`, `StationFinished(string id)`; `ICheckpointService { Get, StateOf, LockReason, ToolsCollected }`; UI actions `walk:<id>`, `goin:<id>` (A handles) and `materials:<id>` (UI only, B).

Any other contract change needs both people to agree: an issue labelled **`contract-change`**, a small PR from `contract/<name>` straight to `v2dev`, compiling with no Console errors and stubs updated, then the other person merges `v2dev` before continuing. **Never change contracts inside a feature branch.**

## Branches and merge windows (PRD-v2 10.2)

- **`v2dev`** is the v2 integration branch; `main` keeps the last v1 build until v2 is ready. No direct pushes to either: PRs only.
- Work branches: **`stream/a-quests`** (A) and **`stream/b-screens`** (B), branched from `v2dev`. Optional short feature branches `a/<name>`, `b/<name>` merge into the stream branch.
- Merge windows into `v2dev`: Sat 22:00 · Sat 23:30 (`contract/hero`) · Sun 08:00 (cut-off: set `quest4Mode` and `heroMode`) · 08:30 · 10:00 (feature freeze, tag `freeze`, then `v2dev` → `main`) · 10:30 (final WebGL build from `main`, tag `demo`).
- At each window: merge `v2dev` into your stream branch, compile, read the Console, run Validate Data and the reviewer, then open a PR into `v2dev`. **A merges first, then B** (after merging `v2dev` again). Merge commits, not squash.
- Definition of done (PRD-v2 10.3): no new Console errors, Validate Data says "All data valid", it plays end to end with the Fake Event Panel or the URL shortcuts, it looks right in a 1920 × 1080 capture, and TASKS.md is updated.
