# Byte World v2 task board

PRD-v2.md section 8. Tick tasks here and in the pinned issues: Person A [#83](https://github.com/rubutar/cisco_game/issues/83), Person B [#84](https://github.com/rubutar/cisco_game/issues/84). Status: To do, Doing, Done, Cut. The v1 board is retired.

## Person A: game core, content, Splunk Trace, hero, art (`stream/a-quests`)

| ID | Task | Est. | Tier | Merge by | Status |
|---|---|---|---|---|---|
| A-01 | Contract PR `contract/quests-v2` (6.4, 6.5), stubs updated | 45m | Must | Sat 21:00 | Done |
| A-02 | Quest 1 content: 12 items, Webex brief, Field Cards, hints. Day 1 alert group {scout, gigi} | 1h15 | Must | Sat 22:00 | Done |
| A-03 | JobShift raises ItemResolved with a log line. Scout shows Reply-To, Sender check and Link. Gigi’s mid-quest rule change | 1h | Must | Sat 22:00 | Done |
| A-04 | Quest 2 content plus all remaining text: Twins and Patch items, Field Cards, hunt.json, checks, props, Learner Card, incident cards | 1h15 | Must | Sat 22:00 | Done |
| A-13 | **(v2.1)** Contract PR `contract/hero` (6.4) with stubs; `checkpoints.json` for all 7 stations, materials text checked against cisco.com (4.11) | 1h | Must | Sat 23:30 | Done |
| A-14 | **(v2.1)** Map relayout in `World.unity`: plaza hub, 7 station buildings, roads from `checkpoints.json`, lit and locked roads, greyscale locked buildings, roaming Bytes on lit roads only | 1h15 | Must | Sun 08:00 | Done |
| A-16 | **(v2.1)** Pax art: 32 × 32 map sheet (idle down and side, walk side 4 frames; Should: walk up and down, interact, cheer, worried) and 48 × 48 portraits (neutral, happy; Should: surprised, worried, determined). Bytes redrawn at 16 × 16 with faces | 2h | Must (Must frames only) | Sun 08:00 | Done |
| A-15 | **(v2.1)** `HeroController`: Pax sprite (32 × 32 sheet from A-16, placeholder until then), keyboard movement through the `Hero` action map (river and edge blocking), tap-to-walk along road waypoints via the plaza, turn mid-walk, proximity check (26 px) raising `HeroArrived` / `HeroLeft`, `Interact`/`Materials`/`Back` raising UI actions, `heroMode: teleport`. `CheckpointManager`: states, unlocks (`requires`), `walk` and `goin` actions start the right job, hunt, build or siege; `StationFinished` | 1h30 | Must | Sun 08:00 | Done |
| A-05 | Twins push count and Patch signature fields. Day 2 alert group {twins, patch}. Check each scene copes with 5–6 items | 45m | Must | Sun 08:00 | Done |
| A-06 | Splunk Trace: chips build the query in order, the results table animates in, tap a row to pin it, extra rows for the player’s mistakes, Field Card. **(v2.1)** Started by Go in at the Observatory | 2h | Must | Sun 08:00 | Done |
| A-07 | build.json data for Quest 3 (lanes, test packet, rules) and Quest 4 (siege), agreed with B | 45m | Must | Sun 08:00 | Done |
| A-08 | Filming shortcuts: `?quest=1..4`, `?hunt=1` and **(v2.1)** `?at=<checkpointId>` (Pax starts at that station with the card open) URL parameters, plus flags.json `quest4Mode` | 30m | Must | Sun 08:00 | Done |
| A-09 | QR code PNG to NetAcad for the Learner Card (Python qrcode), handed to B | 15m | Must | Sun 08:00 | Done |
| A-10 | Phone playthrough, fix blockers, capture clean footage (11) | 1h | Must | Sun 10:00 | To do |
| A-11 | Props: Rusted Door 23 and Conduit Bridges on the map raise PropTapped | 45m | Stretch |  | Done |
| A-12 | Hunt stretch: wrong chips give hints, a distractor chip | 30m | Stretch |  | Done |

Must: 15h15.

## Person B: screens, checkpoint UI, build mode, Quest 4 (`stream/b-screens`)

| ID | Task | Est. | Tier | Merge by | Status |
|---|---|---|---|---|---|
| B-01 | Fonts (Pixelify Sans, Nunito, JetBrains Mono), yellow main buttons with a pressed state, white secondary buttons, mono font for inspected values | 1h | Must | Sat 22:00 | Done |
| B-02 | Webex chat skin for the dialogue bubble: space name, avatar initials, timestamp. Brief at quest start. **(v2.1)** Town Space strip on the map | 1h | Must | Sat 22:00 | Done |
| B-03 | Field Card after each station’s success, reusing RevealCard, with **Back to Byte World** | 45m | Must | Sat 22:00 | Done |
| B-14 | **(v2.1)** Station bubbles (3 states), arrival popup with Enter, expanded card (4.2a) with Go in / See materials / Play again and the lock line; anchors flip near edges; raises `walk`, `goin`, `materials` | 1h30 | Must | Sun 08:00 | Done |
| B-15 | **(v2.1)** Materials panel (4.4.4): half screen from the right, text from `ICheckpointService`, without/with slider over two images (RectMask2D), Field Card preview, NetAcad line, footer by state | 1h15 | Must | Sun 08:00 | Done |
| B-04 | Quest-end sequence on DayEnded: Check card (4 options plus explanation), then the quest card | 1h15 | Must | Sun 08:00 | Done |
| B-05 | Quest 3 lanes: lane membership in NetworkGraph, the test packet in TrafficRun, the unmanaged-vs-managed pop-up | 2h | Must | Sun 08:00 | Done |
| B-06 | Quest 4 siege, scripted: call stream and flood, a blind-phase call indicator, the Call button, ThousandEyes overlay (link colours plus loss and latency), the priority toggle, the second run passes | 2h | Must | Sun 08:00 | Done |
| B-07 | Learner Card on the peace screen: outcomes, Field Notes x/8, check score, QR | 1h | Must | Sun 08:00 | Done |
| B-08 | WebGL build, tag demo, check on iPhone and Android, help capture | 45m | Must | Sun 10:30 | To do |
| B-16 | **(v2.1)** Cisco toolkit: top-bar button with count, toast on `StationFinished`, toolkit panel showing the Visitor Pass with its tool badges and the Learner Card slot | 45m | Should | Sun 10:00 | Done |
| B-18 | **(v2.1)** Pax portraits in the UI: in the station world header, on the Field Card (happy) and on the toolkit panel; expression chosen by event (alert, go in, finished, backup lost) | 30m | Should | Sun 10:00 | Done |
| B-17 | **(v2.1)** Iris transition into and out of station worlds | 30m | Should | Sun 10:00 | Done |
| B-09 | Splunk ticker on ItemResolved | 30m | Should | Sun 10:00 | Done |
| B-10 | Prop card on PropTapped | 30m | Stretch |  | Done |
| B-11 | Incident report: order 5 cards | 45m | Should | Sun 10:00 | Done |
| B-12 | Quest 3 rules puzzle: order matters and the pinned IP is blocked | 45m | Should | Sun 10:00 | Done |
| B-13 | Quest 4 live mode: a free reroute instead of the scripted fix | 1h30 | Stretch |  | To do |

Must: 12h30.

## Cut order if behind (PRD-v2 7)

Props → iris transition → ticker → incident report as a static card → Quest 3 rules ordering → checks for Quests 3–4 → toolkit panel (keep the toast) → `heroMode: teleport` → Quest 4 scripted. **Never cut:** Quests 1–2 with new content, the station popup and card, the materials panel, Field Cards, the Hunt, the Quest 3 lanes puzzle, the ThousandEyes overlay, the Learner Card.

## Bugs

_None yet. Format: what happened, steps to reproduce, owner._
