---
name: game-dev
description: Stream A (game core) rules for Byte World — GameManager, DayManager, AlertController, BackupNest, DataLoader, JobShift and the four job scripts, roaming Bytes, fallback final attack, debug tools. Use for any work in Scripts/Core (non-contract), World/, Jobs/, Debug/ or the World and Job scenes.
---

# game-dev (Stream A)

Stack: Unity 6000.6.9f1, C#, URP 2D, uGUI, WebGL (no threads, no file system, low allocations). Tap input via the uGUI EventSystem only (`IPointerClickHandler`; `Physics2DRaycaster` on the camera for world sprites).

**How it fits together**
- `GameManager` (on `Resources/Core/GameRoot.prefab`) implements `IGameFlow`, owns the `GameState` machine from PRD 6.4, and loads scenes: World, job scenes additively, Build on Day 3. It reacts to `GameEvents.UiActionRequested` and `RestartDayRequested`.
- `DayManager` walks `days.json` → `alertGroups`; `AlertController` flashes every Byte in the group, tracks each one's wait time and passes `waitedSeconds` into `JobContext`.
- `JobShift` (base class) does everything shared: spawn items on a timer, open `Services.Inspect`, apply the two actions, timer, `ShiftProgress`, `ItemResolved`, success or fail. Tutorial flag (Day 1): first mistake → `Services.Dialogue.ShowHint(item.hint)` and the item comes back; never a backup. Complaints (Day 2+): `GameEvents.RaiseComplaint`, fail with `ComplaintRules.ShiftFailed`. Wait penalty: `timer -= min(waited * waitPenaltyPerSecond, timer * maxWaitPenalty)`. Job scripts only override presentation and special rules (Patch fit/later, Gigi rule change, Twins code timer).
- Every failure goes through `Services.Backups.ReportSnatch(...)`. `BackupNest` implements `IBackupNest` and is the only raiser of `Snatched`, `BackupsChanged` and `Collapsed`.
- Fallback final attack: `flags.json` `day3Mode = "fallback"` → Day 3 runs `fallbackDay3` as one alert group of four. Data only.
- Debug: `?day=2`, `?day=3&puzzle=3` from `Application.absoluteURL`; debug menu opens on `GameEvents.DebugMenuRequested` (B's title logo, tapped five times) or the backquote key in the Editor. Namespace `ByteWorld.DebugTools`.

**Rules**
- All text and item lists come from JSON through `Services.Data`. Never hard-code game text.
- Show UI only through `Services.Dialogue` and `Services.Inspect`, and tell UI about everything else with events. Never reference UI types.
- Read the Unity Console through MCP after every compile.

**Must not:** edit UI prefabs or `Scripts/UI`/`Scripts/Build`, `Boot.unity`, `Build.unity`, C's JSON content (`days.json`, `dialogue.json`, `roaming.json`, `build.json`), or shared contracts outside a contract-change PR.
