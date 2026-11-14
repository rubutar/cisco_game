# Byte World task board

Kept up to date by the pm skill at every merge window. The GitHub issues (one per task, milestones **Stream A: Game core**, **Stream B: Screens, flow & Day 3 build**, **Content (C)**) are the detailed version; tick tasks here and close the issue.

Status: `To do`, `Doing (who)`, `Done`, `Blocked`. **Moved** marks tasks reassigned from the PRD by scene and file ownership.

## Done in foundation (tag `foundation`)

| ID | Task | Status |
|---|---|---|
| [A-01](https://github.com/rubutar/cisco_game/issues/2) | Repo and Unity project, Force Text, Visible Meta Files, `.gitignore` | Done |
| [A-02](https://github.com/rubutar/cisco_game/issues/3) | Folder structure, empty scenes, Build Settings | Done |
| [B-01](https://github.com/rubutar/cisco_game/issues/21) | `CLAUDE.md`, `AGENTS.md` | Done |
| [B-02](https://github.com/rubutar/cisco_game/issues/22) | Skills, reviewer subagent, `TASKS.md` | Done |
| [ALL-02b](https://github.com/rubutar/cisco_game/issues/54) | Milestones, labels, issues | Done |

## Stream A: Game core (person A, `stream/a-game-core`)

| ID | Est. | Task | Status |
|---|---|---|---|
| [A-00](https://github.com/rubutar/cisco_game/issues/1) | 20m | Unity MCP set up for Claude Code | To do |
| [A-03](https://github.com/rubutar/cisco_game/issues/4) | 30m | Pixel Perfect Camera check in World (camera and import settings come from foundation) | To do |
| [A-04](https://github.com/rubutar/cisco_game/issues/5) | 40m | GameManager (IGameFlow) and state machine on GameRoot.prefab | To do |
| [A-05](https://github.com/rubutar/cisco_game/issues/6) | 30m | DataLoader (IGameData) and BackupNest (IBackupNest) | To do |
| [A-06](https://github.com/rubutar/cisco_game/issues/7) | 30m | World map: four job Bytes, tap for job line | To do |
| [A-07](https://github.com/rubutar/cisco_game/issues/8) | 45m | RoamingByte: 10–14 Bytes on cable paths, orbs, tap for a line | To do |
| [A-08](https://github.com/rubutar/cisco_game/issues/9) | 45m | AlertController with alert groups; next group after each shift | To do |
| [A-09](https://github.com/rubutar/cisco_game/issues/10) | 1h | JobShift base and ItemQueue | To do |
| [A-10](https://github.com/rubutar/cisco_game/issues/11) | 45m | ScoutJob: Day 1 Scout end to end | To do |
| [A-11](https://github.com/rubutar/cisco_game/issues/12) | 1h15 | GigiJob | To do |
| [A-12](https://github.com/rubutar/cisco_game/issues/13) | 1h15 | TwinsJob | To do |
| [A-13](https://github.com/rubutar/cisco_game/issues/14) | 1h30 | PatchJob | To do |
| [A-14](https://github.com/rubutar/cisco_game/issues/15) | 1h15 | Day progression 1 → 2 → 3 (**moved in:** advancing the day, from B-08) and tutorial mode | To do |
| [A-15](https://github.com/rubutar/cisco_game/issues/16) | 1h30 | Day 2 double alerts, wait penalty, complaints meter | To do |
| [A-16](https://github.com/rubutar/cisco_game/issues/17) | 45m | Fallback final attack (`flags.json`), debug menu, URL parameters | To do |
| [A-17](https://github.com/rubutar/cisco_game/issues/18) | 30m | **Moved in (from B-09):** restart the same day with 3 backups on `RestartDayRequested` | To do |
| [A-18](https://github.com/rubutar/cisco_game/issues/19) | 45m | Stretch. **Moved in (from C-13):** crowd reactions of roaming Bytes | To do |

Must: 13h45. With stretch: 14h30.

## Stream B: Screens, flow & Day 3 build (person B, `stream/b-screens-build`)

| ID | Est. | Task | Status |
|---|---|---|---|
| [B-00](https://github.com/rubutar/cisco_game/issues/20) | 20m | Unity MCP set up for Claude Code | To do |
| [B-03](https://github.com/rubutar/cisco_game/issues/23) | 1h | WebGL test build, itch.io, check on iPhone and Android | To do |
| [B-04](https://github.com/rubutar/cisco_game/issues/24) | 1h | UIRoot, dialogue bubble (IDialogueService), top bar with backups, progress and complaints pips | To do |
| [B-05](https://github.com/rubutar/cisco_game/issues/25) | 15m | Title screen in Boot; 5 taps on the logo raise `DebugMenuRequested` | To do |
| [B-06](https://github.com/rubutar/cisco_game/issues/26) | 45m | Inspect panel (IInspectPanel) | To do |
| [B-07](https://github.com/rubutar/cisco_game/issues/27) | 1h15 | Job intro, success and snatch screens | To do |
| [B-08](https://github.com/rubutar/cisco_game/issues/28) | 30m | Day end and day start cards (**moved out:** the DayManager logic went to A-14) | To do |
| [B-09](https://github.com/rubutar/cisco_game/issues/29) | 30m | Collapse screen and "Start again" (**moved out:** the restart logic went to A-17) | To do |
| [B-09b](https://github.com/rubutar/cisco_game/issues/30) | 1h15 | Intro panels, WebGL landscape template, itch.io page | To do |
| [B-10](https://github.com/rubutar/cisco_game/issues/31) | 1h30 | Build mode base: blueprint overlay, BuildSlot, CableTool, NetworkGraph, puzzle 1 | To do |
| [B-11](https://github.com/rubutar/cisco_game/issues/32) | 1h30 | Build intro, goal line, Run, TrafficRun for puzzle 1 | To do |
| [B-12](https://github.com/rubutar/cisco_game/issues/33) | 2h | Puzzles 2 and 3: DoorRules, all four outcomes, leaks and complaints | To do |
| [B-13](https://github.com/rubutar/cisco_game/issues/34) | 1h | Reveal card, peace screen | To do |
| [B-14](https://github.com/rubutar/cisco_game/issues/35) | 45m | Final WebGL build, tag `demo`, QR code, backup video | To do |
| [B-15](https://github.com/rubutar/cisco_game/issues/36) | 45m | Should. **Moved in (from ALL-07):** sound effects and music hooked to events | To do |
| [B-16](https://github.com/rubutar/cisco_game/issues/37) | 30m | Should: pause menu (Resume, Restart day) | To do |

Must: 13h35. With Should: 14h50.

## Content (C) (person C, `c/*` branches)

| ID | Est. | Task | Status |
|---|---|---|---|
| [C-00](https://github.com/rubutar/cisco_game/issues/38) | 30m | Unity AI credits, style lock, `quantize.py` | To do |
| [C-01](https://github.com/rubutar/cisco_game/issues/39) | 1h | `make_sprites.py`, Byte and Phisher grids | To do |
| [C-02](https://github.com/rubutar/cisco_game/issues/40) | 1h | `days.json` Day 1 for all four jobs | To do |
| [C-03](https://github.com/rubutar/cisco_game/issues/41) | 1h | `dialogue.json` Day 1 | To do |
| [C-04](https://github.com/rubutar/cisco_game/issues/42) | ongoing | Kickoff notes, pitch angle | To do |
| [C-05](https://github.com/rubutar/cisco_game/issues/43) | 1h | Scout, world map background, dock and river props | To do |
| [C-06](https://github.com/rubutar/cisco_game/issues/44) | 1h | Final Day 1 data, `roaming.json` (15 lines) | To do |
| [C-07](https://github.com/rubutar/cisco_game/issues/45) | 1h30 | Ram, Worm, boats, sea and dock | To do |
| [C-08](https://github.com/rubutar/cisco_game/issues/46) | 1h30 | Gate, doors, list board, vault, Twins | To do |
| [C-09](https://github.com/rubutar/cisco_game/issues/47) | 1h30 | Day 2 data and dialogue, Day 1 hints | To do |
| [C-10](https://github.com/rubutar/cisco_game/issues/48) | 1h30 | Patch art, `build.json` puzzles 1–3, Day 3 lines, build piece art | To do |
| [C-11](https://github.com/rubutar/cisco_game/issues/49) | 1h30 | Animation frames: snatch, backup, peace, alert (owners wire them) | To do |
| [C-12](https://github.com/rubutar/cisco_game/issues/50) | 1h30 | Pitch deck draft, playtest survey | To do |
| [C-14](https://github.com/rubutar/cisco_game/issues/51) | 1h | Playtest with 5–10 visitors | To do |

## Team (label `team`)

| ID | When | Task | Status |
|---|---|---|---|
| [ALL-01](https://github.com/rubutar/cisco_game/issues/52) | Fri | Everyone reads the PRD; confirm roles and who pitches | To do |
| [ALL-02](https://github.com/rubutar/cisco_game/issues/53) | Fri | Everyone on Unity 6000.6.9f1 with **WebGL Build Support** | To do |
| [ALL-03](https://github.com/rubutar/cisco_game/issues/55) | Fri | Charge devices, chargers, hotspot | To do |
| [ALL-04](https://github.com/rubutar/cisco_game/issues/56) | Sat kickoff | Confirm challenge text, deadline, submission, AI and pre-built code rules, Cisco naming | To do |
| [ALL-05](https://github.com/rubutar/cisco_game/issues/57) | Sun 08:00 | Play Day 1–3 on a phone, log bugs | To do |
| [ALL-05b](https://github.com/rubutar/cisco_game/issues/58) | Sun 08:00 | Day 3 cut-off: build mode or fallback | To do |
| [ALL-06](https://github.com/rubutar/cisco_game/issues/59) | Sun 08:30 | Merge window, Codex review | To do |
| [ALL-07](https://github.com/rubutar/cisco_game/issues/60) | Sun until 10:00 | Bug fixing from the list | To do |
| [ALL-08](https://github.com/rubutar/cisco_game/issues/61) | Sun | Rehearse the pitch twice | To do |
| [ALL-09](https://github.com/rubutar/cisco_game/issues/62) | Sun | Submit | To do |

## Bugs

_None yet. Format: what happened, steps to reproduce, owner._
