---
name: game-dev
description: Person A (game core, quests, hero, Splunk Trace) rules for Byte World v2 — GameManager, DayManager, AlertController, CheckpointManager, HeroController, BackupNest, DataLoader, JobShift and the job scripts, the Hunt module, debug tools and the two v2 contract PRs. Use for any work in Scripts/Core (non-contract), World/, Jobs/, Debug/, UI/Hunt/ or the World and Job scenes.
---

# game-dev (Person A)

Spec: `PRD-v2.md` (v2.1). Stack: Unity 6000.6.9f1, C#, URP 2D, uGUI, WebGL (no threads, no file system, low allocations). Taps via the uGUI EventSystem (`IPointerClickHandler`, `Physics2DRaycaster` for world sprites); the **hero alone** also reads the keyboard through the Input System action map `Hero` (Move, Interact, Materials, Back).

**What A builds (PRD-v2 6.3)**
- **Quests on the day machinery:** Quest 1 = Day 1 alert group {scout, gigi}; Quest 2 = Day 2 {twins, patch}, then the Splunk Trace; Quest 3 = build puzzles 1–2; Quest 4 = build puzzle 3 with `siege: true` (B builds the siege).
- **CheckpointManager** wraps AlertController (never replaces it): maps alerts to stations from `checkpoints.json`, owns `CheckpointState` (Locked/Alert/Done) and `requires`, implements `ICheckpointService`, raises `CheckpointStatesChanged` and `StationFinished`, and handles the UI actions `walk:<id>` and `goin:<id>` (Go in starts the job, `Services.Hunt.Begin`, build or siege).
- **HeroController:** Pax on the map, keyboard walking (river and edges block), tap-to-walk along the station `path` via the plaza, turning mid-walk, proximity 26 map px → `HeroArrived` / `HeroLeft`, `heroMode: teleport` fallback, `heroSpeed` from flags.json.
- **JobShift** raises `ItemResolved` with a Splunk-style `logLine` for every decision. Tutorial (Quest 1): hint and retry, never a backup. Complaints through `ComplaintRules`; every failure through `Services.Backups.ReportSnatch`.
- **Splunk Trace** (`UI/Hunt/`, `Editor/HuntBuilder.cs`, `Resources/UI/Hunt.prefab`): chips build the query in order, results table, pin the target row, extra rows from the session's mistakes, Field Card.
- **Debug:** URL `?quest=1..4`, `?hunt=1`, `?at=<checkpointId>`; flags `quest4Mode`, `heroMode`.

**Rules**
- All text and data from JSON through `Services.Data`; never hard-code game text.
- Show UI only through the service interfaces and events; never reference B's UI types.
- Contract changes only as the two planned PRs (`contract/quests-v2`, `contract/hero`) straight to `v2dev`, stubs updated.
- Scenes are filled in code (`SceneSpawner`); never hand-edit `.unity` or `.prefab` YAML.
- After every compile, read the Unity Console (MCP) and fix new errors; run Validate Data after any JSON change.

**Must not:** edit `UIRootBuilder.cs`, `UIRoot.prefab`, `Scripts/Build/` or any `Scripts/UI/` file outside `UI/Hunt/`.
