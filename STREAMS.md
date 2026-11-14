# Byte World: two workstreams

Two people build Byte World in parallel, each with their own Claude Pro account running Claude Code in VS Code with the official Unity MCP. Person C feeds both streams with content, art and sound. Everything meets on `main` at fixed merge windows. Rules for agents are in `CLAUDE.md`; the task board is `TASKS.md` and the GitHub milestones.

## The split

| | Stream A: Game core | Stream B: Screens, flow & Day 3 build | Content (C) |
|---|---|---|---|
| Person | A, Claude account 1 | B, Claude account 2 | C, no Claude Code stream |
| Branch | `stream/a-game-core` | `stream/b-screens-build` | `c/*` straight to `main` |
| Feature branches | `a/scout-shift`, `a/backup-nest` | `b/dialogue-bubble`, `b/day-cards` | `c/data-day2`, `c/sprites-bytes` |
| Milestone | [Stream A: Game core](https://github.com/rubutar/cisco_game/milestone/1) | [Stream B: Screens, flow & Day 3 build](https://github.com/rubutar/cisco_game/milestone/2) | [Content (C)](https://github.com/rubutar/cisco_game/milestone/3) |
| Start here | [#63](https://github.com/rubutar/cisco_game/issues/63) | [#64](https://github.com/rubutar/cisco_game/issues/64) | the milestone, top to bottom |
| Skill | game-dev | ui-screens | pixel-art, content-writer, pm |

## Ownership

| Owner | Scenes | Prefabs | Code and data |
|---|---|---|---|
| **A** | `World.unity`, `Job_Scout.unity`, `Job_Gigi.unity`, `Job_Twins.unity`, `Job_Patch.unity` | `Resources/Core/GameRoot.prefab`, world and job prefabs, debug menu | `Scripts/Core/` GameManager, DayManager, BackupNest, DataLoader; `Scripts/World/`, `Scripts/Jobs/`, `Scripts/Debug/`; `Resources/Data/flags.json` (the Day 3 fallback switch) |
| **B** | `Boot.unity`, `Build.unity` | `Resources/UI/UIRoot.prefab` and every UI prefab (bubble, inspect panel, top bar, screens, day cards, reveal card) | `Scripts/UI/`, `Scripts/Build/`; WebGL template, WebGL player settings, itch.io |
| **C** | none | none | `days.json`, `dialogue.json`, `roaming.json`, `build.json`; `Art/`, `Audio/`, `Tools/sprites/` |
| **Shared** | none | none | Contract files in `Scripts/Core/`: `GameState.cs`, `Events.cs`, `Services.cs`, `Palette.cs`, `Bootstrap.cs`, `Contracts/`, `Data/`, `Stubs/`. Changed only by the contract-change rule. |

**Moved from the PRD** (by scene and file ownership):
- B-08 advancing the day → A-14. B keeps the day cards.
- B-09 restart-day logic → new A-17. B keeps the collapse screen.
- C-13 crowd reactions → A-18, because it's RoamingByte code in `World.unity`.
- ALL-07 audio hooks → B-15. Pause menu → B-16.
- Day cards, collapse and peace screens live in B's UIRoot, not in `World.unity`.
- A-01, A-02, B-01, B-02 and ALL-02b were done in foundation.

## How the streams stay unblocked

Neither stream waits for the other: every cross-stream dependency has a stub in `Scripts/Core/Stubs/`, and **Byte World > Fake Event Panel** fires any event with sample data.

| Waits on | Stub until the real one merges |
|---|---|
| A's shifts need B's inspect panel and bubble | IMGUI `StubInspectPanel` and `StubDialogueService`: A can play a whole shift |
| B's screens need A's events | Fake Event Panel |
| B's top bar and Day 3 leaks need BackupNest | `StubBackupNest` follows the contract exactly |
| B's day cards (B-08, Sat 16–19) need DayManager (A-14, Sat 19–22) | Fake DayEnded / DayStarted; real integration after the 22:00 merge |
| B's collapse "Start again" needs restart logic (A-17) | B raises `RestartDayRequested`; A-17 handles it |
| A's Day 3 hand-off and `?day=3&puzzle=3` need build mode | `StubBuildMode` passes every puzzle at once |
| A's debug menu needs the 5-tap on B's title logo | `DebugMenuRequested`, plus the backquote key in the Editor |
| Both need C's content and art | Stub JSON; palette-coloured placeholder shapes |

## Merge windows and order

Saturday **13:00, 16:00, 19:00** (tag `playable-sat`: Day 1 playable on a phone), **22:00**. Sunday **08:00** Day 3 cut-off, **08:30**, **10:00** feature freeze (tag `freeze`), **10:30**, then the final build tagged `demo`.

At every window:
1. Each stream merges its finished feature branches into its stream branch.
2. Each stream merges `main` into its stream branch, compiles, reads the Unity Console through MCP, and runs the reviewer subagent.
3. **A** opens a PR `stream/a-game-core` → `main`, and it's merged.
4. **B** merges `main` again (now with A's work), re-checks the Console, then opens its PR, and it's merged.
5. **C**'s `c/*` PRs are merged.
6. One person makes a WebGL build of `main` and checks it on a phone; the pm updates `TASKS.md`.

Use **merge commits, not squash**, for stream PRs, so the stream branches don't re-conflict at the next window. No big merge at the end: anything not merged by 10:30 Sunday doesn't ship.

**Contract changes** happen any time, outside the windows: open an issue labelled `contract-change`, make the change in a small PR from `contract/<name>` straight to `main`, then the other stream merges `main` before continuing. Never change contracts inside a feature branch.

## Kickoff prompt: Stream A

Paste into a fresh Claude Code session on person A's account, with Unity open on this repo and the Unity MCP connected (issue #1 done).

```
You are the Stream A (game core) builder for Byte World, a Unity 6000.6.9f1 game (URP 2D, uGUI,
TextMeshPro, WebGL target, landscape, tap input only). Read CLAUDE.md and STREAMS.md first, and use
the game-dev skill.

Branch: work on stream/a-game-core. For each issue, create a feature branch off it named a/<short-name>
(for example a/game-manager), and merge it back into stream/a-game-core when its "Done when" check passes.
Milestone: "Stream A: Game core". Your issue order is in pinned issue #63.
Start with these three, in order:
  #4 A-03 Pixel Perfect Camera in World
  #5 A-04 GameManager and state machine
  #6 A-05 DataLoader and BackupNest

Never touch: Boot.unity, Build.unity, Scripts/UI/, Scripts/Build/, Resources/UI/UIRoot.prefab or any UI
prefab, C's JSON (days.json, dialogue.json, roaming.json, build.json), and the shared contract files
(Scripts/Core: GameState.cs, Events.cs, Services.cs, Palette.cs, Bootstrap.cs, Contracts/, Data/, Stubs/)
except through the contract-change rule in CLAUDE.md. Never hand-edit .unity or .prefab YAML: use the
Unity MCP, and only in World.unity, the Job_*.unity scenes and your own prefabs.

After every compile, read the Unity Console through the Unity MCP and fix every new error before
continuing. Save the scene and commit after each working MCP change. Commit messages: "area: what changed".
Show UI only through Services.Dialogue and Services.Inspect; the stubs work until Stream B's prefabs land.

For each issue: plan briefly, implement, run its "Done when" check, then tell me what you verified and
wait for me before closing the issue and moving on.
```

## Kickoff prompt: Stream B

Paste into a fresh Claude Code session on person B's account, with Unity open on this repo and the Unity MCP connected (issue #20 done).

```
You are the Stream B (screens, flow and Day 3 build) builder for Byte World, a Unity 6000.6.9f1 game
(URP 2D, uGUI, TextMeshPro, WebGL target, landscape, tap input only). Read CLAUDE.md and STREAMS.md
first, and use the ui-screens skill.

Branch: work on stream/b-screens-build. For each issue, create a feature branch off it named
b/<short-name> (for example b/dialogue-bubble), and merge it back into stream/b-screens-build when its
"Done when" check passes.
Milestone: "Stream B: Screens, flow & Day 3 build". Your issue order is in pinned issue #64.
Start with these three, in order:
  #23 B-03 WebGL test build and itch.io
  #24 B-04 UIRoot, dialogue bubble, top bar
  #25 B-05 Title screen

Never touch: World.unity, the Job_*.unity scenes, Scripts/Jobs/, Scripts/World/, Scripts/Debug/,
GameManager / DayManager / BackupNest / DataLoader, Resources/Core/GameRoot.prefab, flags.json, C's JSON
(days.json, dialogue.json, roaming.json, build.json), and the shared contract files (Scripts/Core:
GameState.cs, Events.cs, Services.cs, Palette.cs, Bootstrap.cs, Contracts/, Data/, Stubs/) except through
the contract-change rule in CLAUDE.md. Never hand-edit .unity or .prefab YAML: use the Unity MCP, and only
in Boot.unity, Build.unity and your own prefabs.

After every compile, read the Unity Console through the Unity MCP and fix every new error before
continuing. Save the scene and commit after each working MCP change. Commit messages: "area: what changed".
Screens listen to GameEvents and answer with GameEvents.RaiseUiAction; test them with
Byte World > Fake Event Panel until Stream A's events are real. Project Settings changes for WebGL are
agreed with the team first.

For each issue: plan briefly, implement, run its "Done when" check, then tell me what you verified and
wait for me before closing the issue and moving on.
```
