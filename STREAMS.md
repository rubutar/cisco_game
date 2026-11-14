# Byte World v2: two people, one go each

The spec is `PRD-v2.md` (v2.1, Four Quests, Cisco edition). Two people, each with Claude Code and the Unity MCP, work in one go through their task list until the Sunday 10:00 freeze. Person A also makes all the art and owns all the content (there is no Content C in v2). Rules for agents are in `CLAUDE.md`.

## The split

| | Person A: game core, content, hero, art | Person B: screens, checkpoint UI, build, siege |
|---|---|---|
| Who | Rudi (`rubutar`) | Syed (`Syed0903`) |
| Branch | `stream/a-quests` (from `v2dev`) | `stream/b-screens` (from `v2dev`) |
| Task list | [#83](https://github.com/rubutar/cisco_game/issues/83) (pinned) | [#84](https://github.com/rubutar/cisco_game/issues/84) (pinned) |
| Skills | game-dev, content-writer, pixel-art | ui-screens |
| Must hours | 15h15 | 12h30 |

QA (`citruswhale`) tests `v2dev` builds after each merge window and files bugs with the `bug` and `v2` labels.

## Ownership (PRD-v2 6.2)

| Owner | Files |
|---|---|
| **A** | All `Resources/Data/*.json` (days, dialogue, roaming, build, flags, hunt, checkpoints); `Scripts/Core/` (non-contract); `World/` incl. `HeroController`, `CheckpointManager`; `Jobs/`; `Debug/`; `GameRoot.prefab`; `World.unity`; `Job_*.unity`; the Splunk Trace (`Scripts/UI/Hunt/`, `Editor/HuntBuilder.cs`, `Resources/UI/Hunt.prefab`); all art (`Tools/sprites/`, `Resources/Sprites/`, `Art/Sprites/`); the contract files (two planned PRs only) |
| **B** | `Scripts/UI/` (everything except `UI/Hunt/`, incl. `Checkpoints/`), `UIRootBuilder.cs`, `UIRoot.prefab`, `Scripts/Build/`, `Boot.unity`, `Build.unity`, the WebGL template, `Art/Fonts/` |

B never edits JSON: ask A. A never edits B's UI files.

## How the streams stay unblocked

- **Contracts first.** A lands `contract/quests-v2` (Sat 21:00) and `contract/hero` (Sat 23:30) straight into `v2dev`, each with stubs. B builds against the stubs until then: the checkpoint stub returns `checkpoints.json` with the first two stations on Alert.
- **Fake Event Panel** fires any event (`HeroArrived`, `StationFinished`, `DayEnded` …) so B can test every screen without A's hero.
- **Fallbacks** keep the video safe: `flags.json` `heroMode: teleport` and `quest4Mode: scripted`, decided at Sunday 08:00.

## Merge windows into `v2dev` (PRD-v2 10.2)

Sat **22:00** · Sat **23:30** (`contract/hero`) · Sun **08:00** (cut-off: set `quest4Mode` and `heroMode`) · **08:30** · **10:00** feature freeze (tag `freeze`, then `v2dev` → `main`) · **10:30** final WebGL build from `main` (tag `demo`).

At each window: merge `v2dev` into your stream branch, compile, read the Console, run Validate Data, open a PR into `v2dev`. **A merges first, then B.** Merge commits, not squash.

## Kickoff prompt: Person A

Paste into a fresh Claude Code session, with Unity open on this repo, on branch `stream/a-quests`, and the Unity MCP connected.

```
You are Person A for Byte World v2 (Unity 6000.6.9f1, URP 2D, uGUI, TMP, WebGL, landscape; taps,
plus the hero's keyboard action map). Read PRD-v2.md, CLAUDE.md and STREAMS.md first, and use the
game-dev, content-writer and pixel-art skills.

Branch: stream/a-quests (from v2dev). Your full task list is GitHub issue #83, in PRD-v2 section 8
order. Work through it IN ONE GO, top to bottom, without waiting for me between tasks:
- A-01 and A-13 are the only contract changes: each is one PR straight into v2dev, compiling, stubs
  updated, merged before you continue.
- For each task: implement, compile, read the Unity Console through the MCP and fix every new
  error, run Byte World > Validate Data after JSON changes, play it (URL shortcuts or Fake Event
  Panel), commit "area: what changed", tick it in issue #83 and TASKS.md, move on.
- At each merge window (Sat 22:00, 23:30, Sun 08:00, 08:30, 10:00), merge v2dev into your branch and
  open a PR into v2dev. Stop and ask me only if you're blocked, or at the Sunday 08:00 cut-off
  (heroMode, quest4Mode).

You own: every Resources/Data/*.json, Scripts/Core (non-contract), World/ (incl. HeroController and
CheckpointManager), Jobs/, Debug/, GameRoot.prefab, World.unity, Job_*.unity, the Hunt module
(Scripts/UI/Hunt/, Editor/HuntBuilder.cs, Resources/UI/Hunt.prefab), all art (Tools/sprites,
Resources/Sprites, Art/Sprites) and the contract files. Never touch UIRootBuilder.cs, UIRoot.prefab,
Scripts/Build/ or any other Scripts/UI file.
Content: follow PRD-v2 section 5 and 4.11; lines under 110 characters (cards under 140), fictional
domains and documentation-range IPs only, every Cisco product line checked against cisco.com.
Art: grids through Tools/sprites/make_sprites.py, look at the --preview sheet before committing.
Never hand-edit .unity or .prefab YAML.
```

## Kickoff prompt: Person B

Paste into a fresh Claude Code session, with Unity open on this repo, on branch `stream/b-screens`, and the Unity MCP connected.

```
You are Person B for Byte World v2 (Unity 6000.6.9f1, URP 2D, uGUI, TMP, WebGL, landscape, tap only).
Read PRD-v2.md, CLAUDE.md and STREAMS.md first, and use the ui-screens skill.

Branch: stream/b-screens (from v2dev). Your full task list is GitHub issue #84, in PRD-v2 section 8
order (Musts first, then Shoulds). Work through it IN ONE GO, top to bottom, without waiting for me
between tasks:
- Start B-01 to B-03 now. Before B-14, merge v2dev to pick up A's contract/hero PR; until it lands,
  build B-14 against the checkpoint stub. Build Quest 4 in scripted mode first (quest4Mode).
- For each task: implement, compile, read the Unity Console through the MCP and fix every new
  error, test it with the Fake Event Panel, commit "area: what changed", tick it in issue #84 and
  TASKS.md, move on.
- At each merge window (Sat 22:00, 23:30, Sun 08:00, 08:30, 10:00), merge v2dev into your branch and
  open a PR into v2dev after A's. Stop and ask me only if you're blocked.

You own: Scripts/UI (except Scripts/UI/Hunt/), UIRootBuilder.cs, UIRoot.prefab, Scripts/Build/,
Boot.unity, Build.unity, the WebGL template and Art/Fonts/. You never edit JSON: ask Person A.
Style: every size and colour in UI/UiStyle.cs from Palette constants only. Main buttons Packet
Yellow with ink text, secondary buttons white with an ink border, Alert Pink only for danger and
alert states, no text under 34 at 1920x1080. Visual reference for the popup, card, materials panel
and toolkit: https://claude.ai/artifact/Rt5KF4SqTzdRjUDNtrUdXc
Change UI only by editing UIRootBuilder.cs and re-running Byte World > UI > Build UIRoot Prefab.
Never hand-edit prefab YAML.
```
