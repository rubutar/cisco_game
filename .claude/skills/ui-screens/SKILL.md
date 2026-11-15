---
name: ui-screens
description: Person B (screens, checkpoint UI, build mode, Quest 4 siege) rules for Byte World v2 — UIRoot, fonts and button style, Webex-styled dialogue, Field Cards, station bubbles, popup and card, materials panel, Cisco toolkit, quest-end checks, Quest 3 lanes, Quest 4 siege with the ThousandEyes overlay, Learner Card, WebGL build. Use for any work in Scripts/UI (except UI/Hunt), Scripts/Build, Boot.unity or Build.unity.
---

# ui-screens (Person B)

Spec: `PRD-v2.md` (v2.1). Stack: Unity 6000.6.9f1, uGUI, TextMeshPro, URP 2D, WebGL. Visual reference: https://claude.ai/artifact/Rt5KF4SqTzdRjUDNtrUdXc

**Style (PRD-v2 6.6):** Pixelify Sans for titles and buttons, Nunito for body, JetBrains Mono for inspected values and log lines. Main buttons Packet Yellow with ink text (pressed state), secondary buttons white with an ink border, Alert Pink only for danger and alert states, no text under 34 at 1920 × 1080. Every size and colour in `UI/UiStyle.cs` from `Palette`.

**What B builds**
- **UIRoot** (Screen Space Overlay, 1920 × 1080, match 0.5, `InputSystemUIInputModule`, safe area). Change UI only by editing `UIRootBuilder.cs` and re-running Byte World > UI > Build UIRoot Prefab.
- **Webex chat skin** for the dialogue bubble (space name, avatar initials, timestamp), the brief at quest start, the Town Space strip on the map.
- **Field Card** after each station (RevealCard) with Back to Byte World.
- **Checkpoint UI (PRD-v2 4.2a, 4.4.4, 4.4.5):** station bubbles in three states, the arrival popup on `HeroArrived` (hide on `HeroLeft`), the expanded card (Go in / See materials / Play again / lock line; flips near edges), the half-screen **materials panel** with the without/with slider (two images per station drawn in code, RectMask2D), the **Cisco toolkit** button, toast on `StationFinished`, panel with the Visitor Pass and the Learner Card slot, the iris transition. Read everything from `ICheckpointService`; send `walk:<id>`, `goin:<id>` through `GameEvents.RaiseUiAction`; `materials:<id>` stays inside the UI.
- **Quest-end sequence** on `DayEnded`: Check card (4 options + explanation), then the quest card.
- **Build mode:** Quest 3 lanes (lane membership in NetworkGraph, test packet in TrafficRun, unmanaged-vs-managed pop-up), rules order and the pinned IP; **Quest 4 siege** scripted first (`quest4Mode`): call stream, flood, blind phase, Call → ThousandEyes overlay (link colours, loss %, latency ms), priority toggle, second run passes. Leaks via `Services.Backups.ReportSnatch`, passes via `ComplaintRules.RunPasses`.
- **Learner Card** on the peace screen, WebGL build and the `demo` tag.

**Rules:** screens listen to events and answer only with `RaiseUiAction`; never gameplay logic, never JSON (ask A); test with the Fake Event Panel and the stubs. Read the Unity Console (MCP) after every compile.

**Must not:** edit any `Resources/Data/*.json`, `Scripts/Core`, `World/`, `Jobs/`, `Debug/`, `UI/Hunt/`, `GameRoot.prefab`, `World.unity` or `Job_*.unity`.
