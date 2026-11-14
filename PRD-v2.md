# Byte World PRD v2.1 — Four Quests (Cisco edition)

14 November 2026, v2.1. Changes from v2.0: a hero character (Pax, a visiting learner drawn at 32 × 32, with Bytes at 16 × 16) who walks the town, checkpoint popups with **Go in** and **See materials**, the half-screen materials panel with a without/with Cisco view, the Cisco toolkit, and the learning-map look for the town. Sections that changed are marked **(v2.1)**.

Visual reference for the town, the hero and the checkpoint flow: <https://claude.ai/artifact/Rt5KF4SqTzdRjUDNtrUdXc>

---

## 1. Summary

Byte World is a landscape, pixel-art story game in which Cisco Networking Academy learners defend a town that lives inside their Wi-Fi router. The game runs as four quests, and every quest uses a real Cisco tool and a real security concept from the first minute: Umbrella, Secure Firewall, Duo, Secure Endpoint, Splunk, Catalyst switches, Webex and ThousandEyes.

**(v2.1)** The player is **Pax**, a learner who has been invited into Byte World: a person visiting the town of Bytes, not one of them. Pax walks the town from Router Plaza to seven Cisco stations. When Pax reaches a station, a popup names the station and its Cisco tool. Tapping **Enter** expands it into a card that shows what the tool does and the town without and with it. **Go in** takes Pax into that station’s world to play. **See materials** slides in a half-screen lesson. Every finished station adds its tool to Pax’s **Cisco toolkit**, so by the end the player has collected the stack.

One-line pitch: **learn the stack by saving the town.**

This PRD replaces v1.3. It keeps the engine, the art style, the stack and the section numbers that CLAUDE.md refers to. It changes the audience, the story (four quests instead of three days), the content depth and the learning layer. It is written for two people, each running Claude Code with the Unity MCP, from Saturday 20:00 to the Sunday 10:00 feature freeze.

Priority order: (1) the 3-minute pitch video, (2) the screens the video shows, playable where possible, (3) everything else. Every feature in this PRD must be something we can film.

---

## 2. The challenge and how we win

Challenge 4, “Hack the Route”, set by Consorzio CLARA for Cisco. The mid-hackathon review gave three pieces of feedback, and this PRD answers each one:

| Feedback | v1.3 problem | v2.x answer |
|---|---|---|
| “What does the learner get?” | No visible learning outcome | Field Cards, a 2-question check per quest, clickable props, the materials panel, and a Learner Card linked to NetAcad (4.9) |
| “Where is Cisco?” | No Cisco terms anywhere | Every station is a Cisco tool, named on its map bubble and popup. See materials shows the town without and with the tool. The tools are collected in the Cisco toolkit. Webex and Splunk run throughout. ThousandEyes is the climax (4.3, 4.11) |
| “Too game-first, content too shallow” | 8 decisions, all jargon-free | About 30 real-data decisions, plus a Splunk search, a network build and a root-cause diagnosis (4.5–4.8) |

How judging works in practice: the judges have about 30 minutes for every team’s game, so most of them will see the video and only a few will play. The video and the deck are the main deliverable, and the app exists to supply real footage for them.

What makes us win:

1. A clear answer to “what does the learner get?” in the first minute of the video.
2. Real Cisco tools used on real-looking data, not mentioned on a slide.
3. **(v2.1)** Every tool shows its value in one picture: the same piece of town without it and with it.
4. One memorable moment: the ThousandEyes turn in Quest 4.
5. An honest MVP. Real footage wherever we have it, and screens that aren’t built yet are labelled “Prototype screen”.

---

## 3. Audience and learning outcomes

Audience: Cisco Networking Academy learners aged 16 and up, taking Introduction to Cybersecurity or the Junior Cybersecurity Analyst career path, which prepares learners for CCST Cybersecurity (Cisco). Players are on a phone or laptop, play for 10–15 minutes, and have no Cisco products installed.

Learning outcomes. After playing, a learner can:

| # | Outcome | Quest | Cisco tool | General concept | CCST Cybersecurity area |
|---|---|---|---|---|---|
| L1 | Spot a phishing message from its sender, Reply-To, sender check and link domain | 1 | Umbrella | Phishing, look-alike domains, urgency, spoofing | Threats, security principles |
| L2 | Allow traffic by guest list and port, keep Telnet (23) shut, and refuse unverified claims | 1 | Secure Firewall | Allow-listing, default deny, attack surface, Zero Trust | Network security |
| L3 | Explain why a correct password is not enough, and recognise MFA push fatigue | 2 | Duo | MFA factors, credential theft, push bombing | Access management |
| L4 | Reject unsigned updates and patch by severity and exposure | 2 | Secure Endpoint | Code signing, supply chain, CVSS, risk-based patching | Vulnerability and risk |
| L5 | Build a basic SPL search and find a reconnaissance pattern in logs | 2 | Splunk | Logging, SIEM, signs of compromise, threat hunting | Incident handling |
| L6 | Segment a network into lanes and prove an infected device can’t reach the vault | 3 | Catalyst switches | Segmentation, VLANs, lateral movement | Network security |
| L7 | Order firewall rules for least privilege, with the implicit deny last | 3 | Secure Firewall | Least privilege, rule order | Network security |
| L8 | Find the root cause of poor call quality from loss, latency and jitter, then reroute and prioritise | 4 | ThousandEyes, Webex | Observability, QoS, availability under attack | Network troubleshooting |
| L9 | Put an incident’s evidence in order, from reconnaissance to the flood | 4 | Splunk, Webex | Incident response lifecycle | Incident handling |

Proof of learning: every quest ends with a 2-question check. The game ends with an incident report the player must order, then the Learner Card (4.9).

---

## 4. Game design

### 4.1 Pillars

1. **Real tools, real terms, from minute one.** Every station names its Cisco tool and the general concept behind it. There is no jargon-free tutorial. (Replaces v1.3’s “Jobs, not lessons”.)
2. **Notice what’s off.** Most items are fine, a few are subtly wrong, and one per station looks wrong but is fine.
3. **Loss you can feel**, never scary or gory.
4. **Short and readable.** One bubble per line (about 110 characters at most), cards under 140 characters a line. Shifts take 45–60 seconds.
5. **Safe isn’t blocking everything.** Blocking real citizens fills the Complaints Meter.
6. **Every quest proves learning.** A Field Card and a 2-question check close each quest.
7. **If we can’t film it, it doesn’t ship.** Every feature must show up clearly in a 1920×1080 capture.
8. **(v2.1) Show the value, not the logo.** Every tool is shown as the same piece of town without it and with it. We never claim more than the product does.

### 4.2 World and lifelines (v2.1)

Byte World lives inside the player’s home Wi-Fi router: circuit-board streets, data rivers, and digital citizens called Bytes. The town uses the **learning-map** look from the visual reference:

- **Router Plaza** in the centre: the router with blinking LEDs and signal rings, and the **Splunk Observatory** beside it as the town’s live dashboard. Every road starts at the plaza.
- **Seven stations** at the ends of the roads (4.3). Each has a **bubble** above or below it with the station name and its Cisco tool.
- **Station states:**

| State | Building | Road | Bubble | Guide Byte |
|---|---|---|---|---|
| Alert (available, not done) | Full colour, pink pulse ring | Lit, with a moving cyan line and packets | Pink border, “!” badge, tool name | Flashing pink “!” |
| Done | Full colour, small animation (charts, lanes, signal rings) | Lit | Green check, tool name | Hops, green tick |
| Locked | Greyscale | Grey, no packets | Grey, lock, “Quest N” | Grey |

- **Roaming Bytes** walk the lit roads only, so the town visibly comes alive as the player unlocks it.
- **Lifelines:**
  - **Three Backup Bytes (shared).** Every breach costs one, and losing all three collapses the town. Restart the quest, not the game.
  - **Complaints Meter (per quest).** Turning away 3 real citizens or messages fails the quest.
- **Webex Town Space.** Briefings, rule changes and alert-bot posts arrive as chat messages. On the map they show in a strip along the bottom left; inside a station they show as the brief.
- **Splunk ticker.** Each decision appears as one log line in a strip along the bottom right, for example `14:02 gate src=Luca door=23 action=deny`.
- **Cisco toolkit** button in the top bar: “Cisco toolkit 3/7”. Opens the toolkit panel (4.4.5).

### 4.2a The hero and checkpoints (v2.1, new)

**Pax** is the player’s character: a young learner visiting Byte World. The Bytes live in the town and do its jobs; Pax is the guest who helps them, the same way a NetAcad learner looks inside a real network. Pax starts the game by stepping into the router at Router Plaza, and starts each quest there. “Pax” is a working name.

**How Pax differs from the Bytes:**

| | Bytes (residents) | Pax (visitor) |
|---|---|---|
| Shape | Small, round, blob-like, antenna | Person: head, hair, arms, legs |
| Size on the map | 16 × 16 frame, about 12 px tall | 32 × 32 frame, about 24 px tall: twice a Byte’s height |
| Colours | Byte Cream body, Neon Cyan belly, Signal Blue eyes, Alert Pink antenna | Packet Yellow hoodie (the only warm moving colour in town), Signal Blue trousers, ink hair and outline |
| Signature details | Antenna light shows mood | Headset (a nod to Webex), glowing **Visitor Pass** on a lanyard |
| Faces | Simple eyes, mood by antenna and palette swap | Expressive face: neutral, happy, surprised, worried, determined |
| Voice | Speak in first person about their job | Mostly silent; reacts with expressions. The Bytes call Pax by name |

**The Visitor Pass is the Cisco toolkit.** Each finished station adds a small badge to the pass (one colour per tool), so the toolkit is something Pax wears in the world, not only a menu. The toolkit panel (4.4.5) shows the pass up close.

**Intro (2 panels, replaces the v2.0 intro):** “Every message you send passes through a tiny town inside your router.” Then Pax steps through Router Plaza, the Bytes look up, and Gigi says “A visitor! Perfect timing. We need help.” The first Webex brief follows.

**Moving (two ways, same flow).**

- **Keyboard (laptop):** WASD or the arrow keys walk Pax freely around the town, on roads and grass. The map edges and the river block him. Diagonals work. This is the main way to play on a laptop and for filming.
- **Tap (phone and laptop):** the player taps a station or its bubble, and Pax walks there along the roads: back to the plaza if he is on a road, or straight across if he is on the grass. Tapping another station mid-walk turns him around.
- Walking speed is fast (about 2 seconds plaza to station) so it never feels like waiting. Locked stations can be visited too: the popup shows what’s coming.
- Pax is twice the height of a citizen Byte (6.6), so the hero reads clearly on a phone and in the video.

**Arrival popup (small).** When Pax comes within about 26 map pixels of a station’s door, the station’s bubble turns into a small popup anchored to the station: badge, station name, Cisco tool, and an **Enter** button. Walking away closes it. Alert stations bounce gently.

**Keys:** Enter (or E) expands the popup; Enter again is **Go in** on an alert station; M opens **See materials**; Esc closes the card or panel. A small key hint sits in the map’s top-left corner, and the expanded card shows “Enter: go in · M: materials · Esc: close”.

**Expanded card.** Tapping **Enter** expands the popup in place into the card (see the reference screenshot and the visual reference):

- Badge (“!” alert, green check done, lock locked), the line “Quest 1, Scout’s Harbour. Up next”, and the **Cisco tool name as the title**
- One sentence on what the tool does
- A without → with pair, for example “Every link gets clicked → Bad domains blocked first”
- Buttons by state:

| State | Primary | Secondary |
|---|---|---|
| Alert | **Go in** (Packet Yellow) | **See materials** (white) |
| Done | **Play again** (white) | **See materials** (white) |
| Locked | none; a lock line says “Opens after Quest 1” | **See materials** (white) |

- × closes back to the small popup. Tapping the map closes it too.
- Cards near the screen edge open inward; stations near the top open downward.

**Go in.** An iris transition zooms from the station into the **station world**: the existing `Job_*` scene (or the Hunt, the build mode or the siege). Left side: the tool name, station name, the station art with Pax and the station’s Byte, and the Webex brief. Right side: the inspect panel and actions. Finishing the shift shows the station’s Field Card with **Back to Byte World**, which irises back to the map, marks the station done, adds the tool to the toolkit with a toast (“Cisco Umbrella added to your Cisco toolkit”), posts the next Webex brief and lights the next roads.

**See materials.** A half-screen panel slides in from the right over the map. The left half of the map stays visible, with the station circled in yellow. Contents in 4.4.4.

**Quest 1 dual alert.** Both Harbour and Gate alert at once. The patience bar runs on the station Pax is not at, including while Pax is walking, so choosing a route is part of the triage.

### 4.3 Cast, stations and Cisco tools (v2.1)

The hero, **Pax**, is a visiting learner and appears at every station (4.2a). The Bytes below each run one station.

| Station (map) | Character | Cisco tool | Teaches | Quest | Map position |
|---|---|---|---|---|---|
| Scout’s Harbour | Scout | Cisco Umbrella | Phishing, sender checks | 1 | Right, on the data river |
| Gigi’s Gate | Gigi (also Webex host) | Cisco Secure Firewall | Ports, allow-lists, least privilege | 1 | Bottom left, town wall |
| The Duo Vault | The Duo Twins (Uno and Due) | Cisco Duo | MFA, push fatigue | 2 | Top left |
| Patch’s Workshop | Patch | Cisco Secure Endpoint | Patching, fake updates, risk | 2 | Bottom centre |
| Splunk Observatory | Gigi | Splunk | Logs, hunting | 2 (after Vault and Workshop) | Beside Router Plaza |
| The Switch Yard | Patch | Cisco Catalyst switches | Segmentation | 3 | Top centre |
| The Signal Tower | Gigi and the network team | Cisco ThousandEyes, Webex | Path visibility, root cause | 4 | Top right |

Use product names in text only, with no Cisco logos unless CLARA approves them.

### 4.4 Quest flow (v2.1)

The game is a linear chain of quests: title, intro, Quest 1, Quest 2 (ending in the Splunk trace), Quest 3, Quest 4 (ending in the incident report), then the Learner Card.

Inside a quest, the player moves on the map:

**Map → tap a station → Pax walks → arrival popup → Enter → expanded card → Go in (play) or See materials (learn) → Field Card → back to the map.**

Each quest runs Brief (Webex) → Play (one or more stations) → Learn (Field Cards) → Check (2 questions). Between quests, a quest card shows the title and one line, as the day cards do now. Props can be tapped on the map at any time the map is showing.

Unlock rules: all stations of a quest are open together, except the Splunk Observatory, which opens when the Vault and the Workshop are done. A quest’s check runs when its last station is done. The next quest’s stations unlock after the check.

#### 4.4.4 Materials panel (v2.1, new)

Half the screen, from the right. The same panel for every station, filled from `checkpoints.json`:

1. Header: “Quest 1: The Outer Perimeter. Scout’s Harbour” and the tool name as the title. ×.
2. **What it does**: 1–2 sentences.
3. **Without / with slider**: a small scene of the same piece of town. Without: grey and dim, attackers walking into houses, red alerts. With: bright, a cyan shield around the station, the attacker bounced off, packets flowing. The player drags the slider between them.
4. **Without {tool}** and **With {tool}**: three short lines each.
5. **Field Card**: “You’ll earn this Field Card” before the station is done, “Your Field Card” after. Term, plain line, concept, career role.
6. **In the real world**: one factual line on how organisations use the tool.
7. **Go deeper**: Cisco Networking Academy, Introduction to Cybersecurity (text and link).
8. Footer: **Go in** if the station is on alert, “Completed. In your Cisco toolkit.” if done, the lock line if locked.

#### 4.4.5 Cisco toolkit panel (v2.1, new)

Opened from the top bar. The same half-screen panel, listing the four quests and their tools with done, alert or locked badges, and the **Learner Card** at the bottom: locked until the end, then showing the outcomes, the check score, Field Notes and the QR code (4.9). After the last station, the toolkit opens automatically with the Learner Card.

### 4.5 Quest 1: The Outer Perimeter (Scout and Gigi)

- **Dual-alert triage.** Both stations alert at once. The one Pax isn’t at drains a patience bar, and waiting fills the Complaints Meter.
- **Scout’s Harbour: 6 messages.** The inspect fields are From, Reply-To, Sender check (pass or fail) and Link. Actions: Dock or Quarantine. 2 are attacks: a look-alike domain (bank-supp0rt.co) and a fake prize. 1 looks odd but is real: an urgent school-trip form from the real school domain. The Umbrella verdict shows only on the Field Card after the decision.
- **Gigi’s Gate: 6 visitors.** The inspect fields are Name, Door and Says. The rules board shows 443 Everyone, 8080 Friends only, 23 Closed. Actions: Let in or Turn away. Twists: Luca (a friend) asks for door 23 and must be turned away. A stranger says “Gigi knows me”. Halfway through, Gigi posts in Webex that 8080 is now Friends only.
- **Field Cards:** Phishing (Umbrella), Ports and allow-lists (Secure Firewall). **Check:** 2 questions.

### 4.6 Quest 2: The Inner Ring and the Splunk Trace (Twins and Patch)

- **The Twins’ Vault: 5 requests.** The fields are Password (right or wrong), Phone code (match, wrong or expired) and Requested by owner (yes or no). Push bombing appears as one item with “4 approval requests in 60 s”. The right action is Keep shut, and the card adds “and report fraud”.
- **Patch’s Wall: 5 crates.** The fields are From, Signature (valid or invalid) and Worm speed. An invalid signature, or a look-alike sender such as bu1lders, is a fake update, and the right call is Later. Real crates with a Worm coming must be fitted now. Stretch: ranking 3 cracks by CVSS and exposure.
- **The Splunk Trace (v2.1: at the Splunk Observatory).** When the Vault and the Workshop are done, the Observatory alerts and Gigi posts “Something is probing from the inside.” Pax walks there and goes in. The player taps 4 chips in order to build `index=byteworld sourcetype=firewall action=allowed | stats count by src_ip, dest_port | sort -count`. The results table appears, and the player pins `203.0.113.7 → 8080`. Any Quest 1 mistakes appear as extra rows. The pinned row goes to the Incident Board.
- **Field Cards:** MFA fatigue (Duo), Fake updates (Secure Endpoint), Threat hunting (Splunk). **Check:** 2 questions.

### 4.7 Quest 3: The Blueprint Shift

- **(v2.1)** Going in at the Switch Yard turns the map into a blueprint (the existing build mode).
- **Puzzle 1, Lanes:** place 2 managed switches and connect Pods 12 and 14 to the media lane and Pods 10 and 11 to the vault lane. Run sends normal traffic plus a test packet from infected Pod 12 to the vault, which must be blocked. A pop-up the first time a switch is placed: unmanaged vs managed switch.
- **Puzzle 2, Doors and rules:** set each door to Everyone, Friends only or Closed, and block the IP pinned in Quest 2. Rules apply in order, and anything unmatched is dropped.
- **Field Cards:** Segmentation (Catalyst), Least privilege (Secure Firewall). **Check:** 2 questions.

### 4.8 Quest 4: The Ultimate Siege

- **(v2.1)** Going in at the Signal Tower starts the siege.
- **Run.** A Webex call streams from Pod 14 (a steady line of priority packets) while a flood hits every door. The Splunk alert bot posts the surge in the Webex Town Space.
- **Blind phase (about 15 s).** The call indicator shows loss, jitter and frozen video. All links look the same.
- **Call in ThousandEyes.** Gigi posts “We can’t see inside the cables. Call the network team?” and the player taps Call. Every link then shows its colour and numbers (loss %, latency ms): pink for bad, green for good.
- **Fix.** Tap the root-cause link, move the call onto the green switch, turn on Priority for Webex, and lock the last rule. Run again, and the call stays clear.
- **Incident report.** Put 5 evidence cards in order: scan, phishing, push spam, 8080 probe, flood. Then the peace celebration, the toolkit at 7/7 and the Learner Card.

### 4.9 Learning layer (v2.1)

| Element | When | What it shows |
|---|---|---|
| Station bubble and popup | Always on the map; popup on arrival | Station, Cisco tool, state |
| Expanded card | Tap Enter | Tool, one sentence, without → with |
| Materials panel | See materials, any time, any state | What it does, without/with slider, without/with lists, Field Card preview, real-world line, NetAcad link |
| Webex brief | Start of each quest and inside each station | 2–3 chat lines from Gigi: the goal and what to watch for |
| Splunk ticker | Every decision | One log line |
| Field Card | After each station’s shift | Term, a plain line, “In the real world: Cisco tool”, the general concept, the career role |
| Cisco toolkit | Top bar, any time; opens itself at the end | Tools collected per quest, then the Learner Card |
| Check | End of each quest | 2 questions with 4 options each, then a one-line explanation. A wrong answer is never punished |
| Props | Any time on the map | A 15-second cutaway with one tap to try. Adds a Field Note. 3 notes earn a hint token |
| Learner Card | End | 9 outcomes ticked, Field Notes x/8, check score, QR code to NetAcad Introduction to Cybersecurity and the Splunk courses |

Props (8): Rusted Door 23 (encryption), Signal Beacon (sender checks), Dual-Key Pedestal (MFA), Fracture Lens (exposure, fake updates), Telemetry Stones (reconnaissance trail), Conduit Bridges (segmentation), Signal Tower (loss, latency, jitter), Town Space Bell (alerting). MVP ships 2: Rusted Door 23 and Conduit Bridges. **(v2.1)** Props are tapped directly; Pax doesn’t walk to them.

### 4.10 Difficulty and fallback

- **Quest 1:** hints on every attack, with a retry and no backup lost on the first mistake.
- **Quests 2–4:** no hints, timers on, and breaches cost backups.
- **Fallback** (`flags.json` `"quest4Mode": "scripted"`): Quest 4 plays as a scripted sequence (blind phase, Call, overlay, a fixed reroute, then success). It still looks right on camera if the free-form fix isn’t stable by the Sunday 08:00 cut-off.
- **(v2.1) Hero fallback** (`flags.json` `"heroMode": "teleport"`): if walking along roads isn’t stable by Sunday 08:00, tapping a station places Pax there instantly and opens the popup. The popup, card and panel flow stays the same.

### 4.11 Cisco marketing rules (v2.1, new)

The game markets Cisco by showing what each tool changes, not by advertising. Rules for every line we write:

1. **Name the tool where the player acts.** Bubble, popup title, station world header, Field Card, toolkit. Always the full product name (“Cisco Secure Firewall”, not “the firewall”), except Splunk and Webex, which are used as is.
2. **Without / with, always the same piece of town.** The picture carries the message. The lines under it are concrete outcomes (“Bad domains blocked first”), never adjectives (“best-in-class”).
3. **Only true claims.** Describe what the product does, from Cisco’s own product pages. No invented statistics, customer names, prices or awards. Person A checks every “What it does” and “In the real world” line against cisco.com before merging.
4. **Real world, not sales.** “In the real world” says how organisations use the tool, in one line.
5. **One path forward.** Every materials panel and the Learner Card point to Cisco Networking Academy (Introduction to Cybersecurity, Junior Cybersecurity Analyst path).
6. **Collecting is the reward.** “Cisco Umbrella added to your Cisco toolkit” after each station; the toolkit at 7/7 is the last thing before the Learner Card.
7. **No logos** unless CLARA approves them. Product names in text only.
8. **Splunk** is introduced once as “part of Cisco since 2024”.

---

## 5. Content

All content lives in `Resources/Data/*.json` (6.5). Person A owns every JSON file and writes it with Claude Code, but reads every item before merging and runs **Byte World > Validate Data**. A wrong answer key is worse than a missing item.

Rules:

- Bytes speak in the first person about their job, friendly and short. Real terms are allowed everywhere. Explain each one once, on its Field Card.
- Each station has at least one safe item that looks suspicious.
- Every attack has an attacker and, in Quest 1, a hint.
- Each Field Card has: term, a plain line (≤140 characters), the Cisco tool, the general concept, and the career role.
- Each check question has 4 options, one correct answer, and a one-line explanation. Never a trick question.
- All domains, IPs and CVE numbers are fictional. IPs come from the documentation ranges (192.0.2.x, 198.51.100.x, 203.0.113.x).
- **(v2.1)** Each checkpoint has: a one-sentence line (≤110 characters), a without → with pair (≤40 characters each), “What it does” (≤2 sentences), three without lines and three with lines (≤60 characters each), and one real-world line. All of it follows 4.11.

Item counts:

| Quest | Station | Items | Attacks | Looks odd but safe |
|---|---|---|---|---|
| 1 | Scout | 6 | 2 | 1 |
| 1 | Gigi | 6 | 2 | 1 (friend at an open door who sounds pushy) |
| 2 | Twins | 5 | 3 (wrong code, expired code, push bombing) | 1 (new phone, correct code, owner asked) |
| 2 | Patch | 5 | 2 (invalid signature, look-alike sender) | 1 (real crate from a new supplier, valid signature) |
| 2 | Splunk trace | 1 search, 3–5 result rows | 1 target row | |
| 3 | Build | 2 puzzles | Test packet, pinned IP | |
| 4 | Siege | 1 run | Flood | |

Check questions (draft, Person A finalises):

| Quest | Question | Answer |
|---|---|---|
| 1 | Which detail proved the bank email was fake? | The sender check failed, and the domain used a zero |
| 1 | Why stay closed to Luca at door 23? | Telnet sends everything unencrypted. Being a friend doesn’t make a closed door safe |
| 2 | The password was correct. Why was it still an attack? | The owner never asked to log in, and the second factor stopped it |
| 2 | What did the Splunk search show? | One address scanning door 8080 many times: reconnaissance |
| 3 | Pod 12 is infected. What stops it reaching the vault? | The pods are on separate switch lanes |
| 3 | What happens to traffic no rule matches? | It’s dropped (implicit deny) |
| 4 | How did ThousandEyes help? | It showed which link was losing packets, so we fixed the cause, not the symptom |
| 4 | Why give the Webex call priority? | Real-time video breaks with loss and jitter, so it goes first |

Checkpoint materials (draft from the visual reference, Person A checks against cisco.com):

| Station | Tool | Line | Without → with |
|---|---|---|---|
| Scout’s Harbour | Cisco Umbrella | Blocks dangerous domains and links before a device ever connects. | Every link gets clicked → Bad domains blocked first |
| Gigi’s Gate | Cisco Secure Firewall | Decides which doors are open, and for whom. Anything else is dropped. | Every door is open → Only the doors you need |
| The Duo Vault | Cisco Duo | A second check on your own phone, so a stolen password isn’t enough. | A stolen password opens it → A second step on your phone |
| Patch’s Workshop | Cisco Secure Endpoint | Protects every device, and keeps fake updates and malware out. | Any update gets installed → Only signed updates go in |
| Splunk Observatory | Splunk | Every decision in town is logged. Splunk searches all of it at once. | Logs in every house → One search for the town |
| The Switch Yard | Cisco Catalyst switches | Split the town into lanes so one infected pod can’t reach the vault. | One flat network → Separate lanes per group |
| The Signal Tower | Cisco ThousandEyes | Sees inside every cable: which link is losing packets, and why. | The call freezes, nobody knows why → The broken link shows up pink |

---

## 6. Technical

### 6.1 Stack (unchanged)

Unity 6000.6.9f1, URP 2D, uGUI + TextMeshPro, WebGL, landscape. Taps go through the uGUI EventSystem with InputSystemUIInputModule. **(v2.1)** The keyboard is added for the hero only: one Input System action map, `Hero`, with `Move` (WASD and arrows, Vector2), `Interact` (Enter, E), `Materials` (M) and `Back` (Esc). All UI stays tappable, and nothing requires the keyboard. Pixel Perfect Camera 480×270 at 16 PPU. Canvas Scaler 1920×1080, match 0.5. No threads, no file system, no AI calls at runtime, no saving.

### 6.2 Ownership (two people)

There’s no Content C any more. A owns all JSON. Nobody edits a file the other person owns.

| Area | Owner |
|---|---|
| All `Resources/Data/*.json` (days, dialogue, roaming, build, flags, hunt, and the new checkpoints) | A |
| `Scripts/Core/` (non-contract), `World/` (including `HeroController`, `CheckpointManager`), `Jobs/`, `Debug/`, `GameRoot.prefab`, `World.unity`, `Job_*.unity` | A |
| The Splunk Trace module: `Scripts/UI/Hunt/`, `Editor/HuntBuilder.cs`, `Resources/UI/Hunt.prefab` | A |
| `Scripts/UI/` (everything else, including `CheckpointPopup`, `MaterialsPanel`, `ToolkitPanel`, `StationTransition`), `UIRootBuilder.cs`, `UIRoot.prefab`, `Scripts/Build/`, `Boot.unity`, `Build.unity`, WebGL template, `Art/Fonts/` | B |
| Contract files (`GameState`, `Events`, `Services`, `Contracts/`, `Data/`, `Stubs/`) | A, in one PR per change (6.4) |

### 6.3 Reuse map: quests on the existing engine (v2.1)

The four quests run on the existing day and puzzle machinery, so GameManager’s state machine barely changes. **(v2.1)** What changes is how a shift starts: the player now starts it with **Go in** at a station instead of tapping an alerting Byte. `AlertController` still raises the alerts; `CheckpointManager` maps each alert to a station and decides its state.

| Quest | Engine | What’s reused | What’s new |
|---|---|---|---|
| Map | `World.unity` | Map art, roaming Bytes, alert flashes | Learning-map relayout (plaza hub, seven stations), lit roads, `HeroController` (walk on road waypoints), `CheckpointManager` (states, unlocks), popup, card, materials and toolkit panels, iris transition |
| 1 | Day 1, alert group {scout, gigi} | AlertController, JobShift, ScoutJob, GigiJob, InspectPanel, patience bar, complaints | New items and fields, Webex brief, ticker, Field Cards, check |
| 2 | Day 2, alert group {twins, patch}, then the Splunk Trace at the Observatory before the day-end card | TwinsJob, PatchJob, backups | New fields (push count, signature), Hunt screen, check |
| 3 | Day 3 build mode, puzzles 1–2 | BuildManager, NetworkGraph, CableTool, DoorRules, TrafficRun | Lane check and test packet, rule order, unmanaged-vs-managed pop-up |
| 4 | Day 3 build mode, puzzle 3 with `siege: true` | TrafficRun, RevealCard | Webex call stream, flood, blind phase, ThousandEyes overlay, priority toggle, incident report |

Quest cards reuse the day start and day end cards with new text. The sequence after a quest is owned by B: on `DayEnded`, B shows the Check card, then shows the quest card. **(v2.1)** The Splunk Trace is no longer called from the quest-end sequence; it starts from **Go in** at the Observatory (`Services.Hunt.Begin(...)`). Quest 4’s report and check run before the last `RevealClosed`.

### 6.4 Architecture and contract changes (v2.1)

The rules from CLAUDE.md still apply: gameplay raises events, UI listens, and UI answers only through `GameEvents.RaiseUiAction`. Every snatch goes through `Services.Backups.ReportSnatch`. Complaints use `ComplaintRules`.

Contract PR 1, `contract/quests-v2`, by A (as in v2.0):

- `GameEvents.ItemResolved(ItemResolvedInfo)`: `{ day, job, itemId, title, allowed, correct, attacker, logLine }`. Raised by JobShift for every decision. B’s ticker listens to it, and the Hunt reads the session’s list of mistakes from it.
- `GameEvents.PropTapped(string propId)`: raised by World, shown by B’s Prop card.
- `IHuntScreen` service: `Begin(Action<string> onPinned)`. Implemented by A. The stub pins the target row at once.
- The data classes in 6.5, with stubs updated so each person can still run their half alone.

**Contract PR 2, `contract/hero` (v2.1, new), by A, before 23:30 Saturday:**

- `CheckpointState` enum: `Locked`, `Alert`, `Done`.
- `GameEvents.CheckpointStatesChanged()`: raised by `CheckpointManager` whenever any state changes. UI re-reads `Services.Checkpoints`.
- `GameEvents.HeroArrived(string checkpointId)` and `GameEvents.HeroLeft(string checkpointId)`: raised by `HeroController`. B shows and hides the popup.
- `ICheckpointService`: `CheckpointData Get(string id)`, `CheckpointState StateOf(string id)`, `string LockReason(string id)`, `int ToolsCollected`. Implemented by A; the stub returns `checkpoints.json` with the first two stations on Alert.
- UI actions through `RaiseUiAction`: `"walk:<id>"` (tap on a bubble), `"goin:<id>"`, `"materials:<id>"`. A handles `walk` and `goin`; `materials` is UI-only and stays in B.
- `GameEvents.StationFinished(string checkpointId)`: raised by A after a station’s shift succeeds and its Field Card is closed. B shows the toolkit toast and the next Webex brief.

After these PRs, contract changes need both people to agree, and go in a small PR straight to `main` as before.

### 6.5 Data schemas (additions)

The JsonUtility rules still hold: public fields, no dictionaries, no nested arrays (an array of objects that each hold a string array is fine).

- `days.json` items: `replyTo`, `senderCheck` (`"pass"` or `"fail"`), `link` (Scout); `rulesChangeAtItem`, `rulesChangeLine` (Gigi); `pushCount` (Twins; existing `codeExpired` and `ownerRequested` stay); `signature` (`"valid"` or `"invalid"`) (Patch).
- `dialogue.json`: `briefs[] {quest, lines[]}`; `jobs[].days[].fieldCard {term, text, cisco, concept, role}`; `checks[] {quest, question, options[4], answer, explanation}`; `props[] {id, name, place, concept, text, tryLabel}`; `learnerCard {title, outcomes[], cta, url}`; `incident {prompt, cards[]}` (in the correct order); new UI labels (call, priority, pin, report, quarantine, fieldNotesFormat, tickerPrefix, enter, goIn, seeMaterials, playAgain, toolkit, toolkitToast, and so on).
- New `hunt.json`: `{ prompt, chips[] {label, order}, distractors[], rows[] {srcIp, destPort, count, target}, fieldCard }`.
- `build.json` puzzles: `lanes[] {id, slots[]}`, `testPacket {from, to}`, `rules[] {match, action}` (applied in order, implicit drop), and `siege {callFrom, floodDoors[], blindSeconds, rootCauseLink, priorityToggle}`.
- `flags.json`: `quest4Mode` (`"live"` or `"scripted"`), **(v2.1)** `heroMode` (`"walk"` or `"teleport"`), `heroSpeed` (map pixels per second, default 110).
- **(v2.1) New `checkpoints.json`:**

```json
{
  "plaza": { "x": 240, "y": 140 },
  "checkpoints": [
    {
      "id": "harbour",
      "quest": 1,
      "station": "Scout's Harbour",
      "character": "scout",
      "job": "scout",
      "tool": "Cisco Umbrella",
      "x": 360, "y": 200,
      "path": [ {"x":240,"y":140}, {"x":300,"y":140}, {"x":300,"y":200}, {"x":360,"y":200} ],
      "requires": [],
      "line": "Blocks dangerous domains and links before a device ever connects.",
      "without": "Every link gets clicked",
      "with": "Bad domains blocked first",
      "materials": {
        "what": "Cisco Umbrella checks every domain a device tries to reach and blocks the dangerous ones before a connection is made.",
        "withoutLines": ["Look-alike domains get clicked", "One click can hand over a password", "Nobody sees which links were risky"],
        "withLines": ["Known bad domains are blocked at the lookup", "Phishing pages never load", "The team sees every blocked attempt"],
        "realWorld": "Schools and companies use Umbrella so one wrong click doesn't reach a phishing page.",
        "learnMoreLabel": "Cisco Networking Academy: Introduction to Cybersecurity",
        "learnMoreUrl": "https://www.netacad.com"
      }
    }
  ]
}
```

`job` names what Go in starts: a job id (`scout`, `gigi`, `twins`, `patch`), `hunt`, `build` or `siege`. `requires` lists checkpoint ids that must be done first (the Observatory requires `vault` and `workshop`). The map position and path are in the 480×270 map’s pixels; `HeroController` converts them to world units.

### 6.6 Look (v2.1)

- **Palette:** unchanged (`Palette.cs`).
- **Fonts:** Pixelify Sans for titles and buttons, Nunito for body text, JetBrains Mono for every inspected value and log line. The mono font has a distinct zero, so 0 and O can’t be confused.
- **Colours:** main buttons Packet Yellow with ink text. Alert Pink only for danger and alert states. Secondary buttons white with an ink border.
- **Town:** the learning-map look in the visual reference: plaza hub, sandy roads with a moving cyan line when lit, grey roads when locked, greyscale buildings for locked stations, roaming Bytes only on lit roads.
- **Sprite sizes (v2.1):**

| Sprite | Frame | Figure height | PPU | Notes |
|---|---|---|---|---|
| Pax (map) | 32 × 32 | about 24 px | 16 (so 2 × 2 units) | Pivot at the feet, bottom centre |
| Bytes (residents, station Bytes) | 16 × 16 | about 12 px | 16 | Up from 12 × 12 in v1.3, so the faces have room |
| Attackers (Phishers, Rams, Worms) | 16 × 16 | 10–14 px | 16 | |
| Station buildings | 64 × 48 | | 16 | On a 4 px grid |
| Pax portrait (Webex messages, Field Card, station world) | 48 × 48 | head and shoulders | UI | Shown at 3× or 4× in the UI |

- **Pax animation set (map, 32 × 32):**

| Animation | Directions | Frames | Tier |
|---|---|---|---|
| Idle (breathing, blink) | down, side | 2 | Must |
| Walk | side (flipped for left) | 4 | Must |
| Walk | down, up | 4 each | Should |
| Interact (lean in toward the station) | side | 2 | Should |
| Cheer (station finished) | down | 2 | Should |
| Worried (breach, backup lost) | down | 1 | Should |

- **Pax portraits (48 × 48):** neutral, happy, surprised, worried, determined. Must: neutral and happy. The others are Should.
- **Expressions in use:** surprised when an alert fires near Pax; determined on Go in; happy on the Field Card and the toolkit toast; worried when a backup is lost; cheer at the peace screen.
- **Visitor Pass:** 4 × 6 px on the lanyard on the map sprite, glowing Neon Cyan; badges appear on the large version in the toolkit panel (one colour per tool, 7 slots).
- **Bytes at 16 × 16:** same body shape and palette as before, with a two-pixel face that can show happy, worried and surprised, and the antenna light for mood. Station Bytes get one prop each (Scout’s spyglass, Gigi’s clipboard, the Twins’ keys, Patch’s wrench).
- **Making the art:** draw in Piskel or Aseprite with the Palette.cs colours as the swatch, export sprite sheets as PNG (one row per animation). Unity AI Sprite generator is allowed for a first pass of the portraits, then quantise to the palette. Import: Sprite (Multiple), PPU 16, Point filter, no compression, pivot bottom centre for map sprites.
- **Fallback:** if the 32 × 32 set isn’t ready by Sunday 08:00, use the current placeholder (a Byte with a yellow cap, drawn at 2×) on the map and keep the portraits.
- **Popup and card:** cream background, 2 px ink border (pink when on alert, grey when locked), 4 px ink drop shadow, a pointer toward the station; the card layout in 4.2a.
- **Materials and toolkit panels:** half the width of the screen, cream, 4 px ink left border, slide in from the right in 0.24 s.
- **Iris transition:** a circle closes on the station and opens on the station world in 0.45 s, and the reverse when leaving.
- **Reference:** the visual reference for the map, the hero, the popup, the card, the materials panel, the toolkit and the station world. The pitch deck’s screens are the visual spec for the Hunt, the ticker, the Webex chat, the ThousandEyes overlay and the Learner Card.

### 6.8 WebGL (unchanged)

The v1.3 build settings, template and itch.io setup stay as they are: **Byte World > WebGL > Build**, Gzip with fallback, a 960×540 viewport, landscape, link-only until the demo.

---

## 7. MVP scope and cut order (v2.1)

| Feature | MVP | Why |
|---|---|---|
| Quest 1 and Quest 2 shifts with the new items and fields | Must | The core of the learning, and already mostly built |
| **(v2.1)** Map relayout, station states and lit roads | Must | It is the first thing the video shows |
| **(v2.1)** Pax walking to a station, arrival popup, Enter, expanded card, Go in | Must | The new main loop. `heroMode: teleport` is the safety net |
| **(v2.1)** Materials panel with the without/with slider | Must | The clearest “Where is Cisco?” answer, in one shot |
| Webex brief (chat-styled bubble) | Must | CLARA named Webex. It’s cheap: a restyle plus text |
| Field Cards after each station | Must | The clearest answer to “what does the learner get?” |
| Splunk Trace (Hunt screen) | Must | The hero shot of the video |
| Quest 3 lanes puzzle with the test packet | Must | Shows networking depth, and build mode exists already |
| Quest 4 siege with the ThousandEyes overlay | Must (scripted fallback allowed) | The video’s climax |
| Learner Card on the peace screen | Must | The final frame of the learning story |
| Fonts and button style | Must | 45 minutes, and every shot looks better |
| **(v2.1)** Cisco toolkit button, toast and panel | Should | Cheap, and makes collecting the stack visible |
| Check card (2 questions per quest) | Should | Proof of learning. Cut to Quests 1 and 2 only if behind |
| Splunk ticker | Should | Makes “Splunk always on” visible |
| Quest 3 rules puzzle (order and pinned IP) | Should | Puzzle 2 with DoorRules already works; the order is new |
| Incident report (order 5 cards) | Should | Can be a static card in the video |
| **(v2.1)** Iris transition | Should | A hard cut also works on camera |
| Props: Rusted Door 23 and Conduit Bridges | Stretch (was Should) | The hero and stations now carry the exploring |
| Other 6 props, crack ranking, hint tokens | Stretch | |
| Live (non-scripted) Quest 4 fix | Stretch | `quest4Mode: scripted` is the safety net |

**Cut order if behind:** props, then the iris transition, then the ticker, then the incident report becomes a static card, then Quest 3 rules ordering, then checks for Quests 3–4, then the toolkit panel (keep the toast), then `heroMode: teleport`, then Quest 4 switches to scripted.

**Never cut:** Quests 1–2 with new content, the station popup and card with Go in and See materials, the materials panel, Field Cards, the Hunt, the Quest 3 lanes puzzle, the ThousandEyes overlay, the Learner Card.

---

## 8. Task board (v2.1)

Must-haves now come to about 15 hours for A (9.5 in v2.0, plus 3.75 for the hero and map, plus 2 for the hero art) and 12.5 hours for B (10 in v2.0, plus 2.75 for the popup and materials panel, minus 0.25 now that the Hunt starts from the map), which is more than the time left before the Sunday 10:00 freeze. To fit: B-09 to B-12 drop first, A-11 (props) is now stretch, and Quest 4 stays scripted. Work top to bottom. A task is done only when it meets 10.3. New tasks are marked **(v2.1)**.

**Person A: game core, content, Splunk Trace, hero**

| ID | Task | Files | Est. | Tier | Merge by |
|---|---|---|---|---|---|
| A-01 | Contract PR `contract/quests-v2` (6.4, 6.5), stubs updated | `Scripts/Core/` contract files | 45m | Must | Sat 21:00 |
| A-02 | Quest 1 content: 12 items, Webex brief, Field Cards, hints. Day 1 alert group {scout, gigi} | days.json, dialogue.json | 1h15 | Must | Sat 22:00 |
| A-03 | JobShift raises ItemResolved with a log line. Scout shows Reply-To, Sender check and Link. Gigi’s mid-quest rule change | Jobs/JobShift.cs, ScoutJob.cs, GigiJob.cs | 1h | Must | Sat 22:00 |
| A-04 | Quest 2 content plus all remaining text: Twins and Patch items, Field Cards, hunt.json, checks, props, Learner Card, incident cards | all JSON | 1h15 | Must | Sat 22:00 |
| **A-13** | **(v2.1)** Contract PR `contract/hero` (6.4) with stubs; `checkpoints.json` for all 7 stations, materials text checked against cisco.com (4.11) | contract files, checkpoints.json | 1h | Must | Sat 23:30 |
| **A-14** | **(v2.1)** Map relayout in `World.unity`: plaza hub, 7 station buildings, roads from `checkpoints.json`, lit and locked roads, greyscale locked buildings, roaming Bytes on lit roads only | World/, World.unity | 1h15 | Must | Sun 08:00 |
| **A-16** | **(v2.1)** Pax art: 32 × 32 map sheet (idle down and side, walk side 4 frames; Should: walk up and down, interact, cheer, worried) and 48 × 48 portraits (neutral, happy; Should: surprised, worried, determined). Bytes redrawn at 16 × 16 with faces | Art/Sprites/Pax/, Art/Sprites/Bytes/ | 2h | Must (Must frames only) | Sun 08:00 |
| **A-15** | **(v2.1)** `HeroController`: Pax sprite (32 × 32 sheet from A-16, placeholder until then), keyboard movement through the `Hero` action map (river and edge blocking), tap-to-walk along road waypoints via the plaza, turn mid-walk, proximity check (26 px) raising `HeroArrived` / `HeroLeft`, `Interact`/`Materials`/`Back` raising UI actions, `heroMode: teleport`. `CheckpointManager`: states, unlocks (`requires`), `walk` and `goin` actions start the right job, hunt, build or siege; `StationFinished` | World/HeroController.cs, World/CheckpointManager.cs, flags.json | 1h30 | Must | Sun 08:00 |
| A-05 | Twins push count and Patch signature fields. Day 2 alert group {twins, patch}. Check each scene copes with 5–6 items | TwinsJob.cs, PatchJob.cs, days.json | 45m | Must | Sun 08:00 |
| A-06 | Splunk Trace: chips build the query in order, the results table animates in, tap a row to pin it, extra rows for the player’s mistakes, Field Card. **(v2.1)** Started by Go in at the Observatory | UI/Hunt/, Editor/HuntBuilder.cs, Hunt.prefab | 2h | Must | Sun 08:00 |
| A-07 | build.json data for Quest 3 (lanes, test packet, rules) and Quest 4 (siege), agreed with B | build.json | 45m | Must | Sun 08:00 |
| A-08 | Filming shortcuts: `?quest=1..4`, `?hunt=1` and **(v2.1)** `?at=<checkpointId>` (Pax starts at that station with the card open) URL parameters, plus flags.json `quest4Mode` | Debug/, flags.json | 30m | Must | Sun 08:00 |
| A-09 | QR code PNG to NetAcad for the Learner Card (Python qrcode), handed to B | Art/ | 15m | Must | Sun 08:00 |
| A-10 | Phone playthrough, fix blockers, capture clean footage (11) | | 1h | Must | Sun 10:00 |
| A-11 | Props: Rusted Door 23 and Conduit Bridges on the map raise PropTapped | World/WorldMap.cs | 45m | Stretch | |
| A-12 | Hunt stretch: wrong chips give hints, a distractor chip | UI/Hunt/ | 30m | Stretch | |

**Person B: screens, build mode, Quest 4, checkpoint UI**

| ID | Task | Files | Est. | Tier | Merge by |
|---|---|---|---|---|---|
| B-01 | Fonts (Pixelify Sans, Nunito, JetBrains Mono), yellow main buttons with a pressed state, white secondary buttons, mono font for inspected values | UIRootBuilder.cs, UI/UiStyle.cs, Art/Fonts/ | 1h | Must | Sat 22:00 |
| B-02 | Webex chat skin for the dialogue bubble: space name, avatar initials, timestamp. Brief at quest start. **(v2.1)** Town Space strip on the map | UI/DialogueBubble.cs | 1h | Must | Sat 22:00 |
| B-03 | Field Card after each station’s success, reusing RevealCard, with **Back to Byte World** | UI/Screens/JobScreens.cs, RevealCard.cs | 45m | Must | Sat 22:00 |
| **B-14** | **(v2.1)** Station bubbles (3 states), arrival popup with Enter, expanded card (4.2a) with Go in / See materials / Play again and the lock line; anchors flip near edges; raises `walk`, `goin`, `materials` | UI/Checkpoints/CheckpointPopup.cs, UIRootBuilder.cs | 1h30 | Must | Sun 08:00 |
| **B-15** | **(v2.1)** Materials panel (4.4.4): half screen from the right, text from `ICheckpointService`, without/with slider over two images (RectMask2D), Field Card preview, NetAcad line, footer by state | UI/Checkpoints/MaterialsPanel.cs | 1h15 | Must | Sun 08:00 |
| B-04 | Quest-end sequence on DayEnded: Check card (4 options plus explanation), then the quest card | UI/Screens/ | 1h15 | Must | Sun 08:00 |
| B-05 | Quest 3 lanes: lane membership in NetworkGraph, the test packet in TrafficRun, the unmanaged-vs-managed pop-up | Scripts/Build/ | 2h | Must | Sun 08:00 |
| B-06 | Quest 4 siege, scripted: call stream and flood, a blind-phase call indicator, the Call button, ThousandEyes overlay (link colours plus loss and latency), the priority toggle, the second run passes | Scripts/Build/, UI/ | 2h | Must | Sun 08:00 |
| B-07 | Learner Card on the peace screen: outcomes, Field Notes x/8, check score, QR | UI/Screens/PeaceScreen.cs | 1h | Must | Sun 08:00 |
| B-08 | WebGL build, tag demo, check on iPhone and Android, help capture | | 45m | Must | Sun 10:30 |
| **B-16** | **(v2.1)** Cisco toolkit: top-bar button with count, toast on `StationFinished`, toolkit panel showing the Visitor Pass with its tool badges and the Learner Card slot | UI/Checkpoints/ToolkitPanel.cs | 45m | Should | Sun 10:00 |
| **B-18** | **(v2.1)** Pax portraits in the UI: in the station world header, on the Field Card (happy) and on the toolkit panel; expression chosen by event (alert, go in, finished, backup lost) | UI/Screens/, UI/Checkpoints/ | 30m | Should | Sun 10:00 |
| **B-17** | **(v2.1)** Iris transition into and out of station worlds | UI/StationTransition.cs | 30m | Should | Sun 10:00 |
| B-09 | Splunk ticker on ItemResolved | UI/ | 30m | Should | Sun 10:00 |
| B-10 | Prop card on PropTapped | UI/ | 30m | Stretch | |
| B-11 | Incident report: order 5 cards | UI/ | 45m | Should | Sun 10:00 |
| B-12 | Quest 3 rules puzzle: order matters and the pinned IP is blocked | Scripts/Build/DoorRules | 45m | Should | Sun 10:00 |
| B-13 | Quest 4 live mode: a free reroute instead of the scripted fix | Scripts/Build/ | 1h30 | Stretch | |

**Without/with images:** the materials slider needs two small images per station (14 in total, 240 × 135). B uses one generic scene per station built from the station sprite, three pod houses and the attacker sprite, drawn in code: grey and dim with attackers for “without”, bright with a cyan shield for “with”, as in the visual reference. No extra art is needed.

---

## 9. Claude Code setup

### 9.1 Repo files to update first (A, part of A-01 and A-13)

- Save this PRD as `PRD-v2.md` at the repo root. Claude Code can’t read the doc version.
- CLAUDE.md and AGENTS.md: replace the pillars with 4.1, the ownership table with 6.2, and add the contract list from 6.4. Keep every other rule.
- STREAMS.md: two people, the new ownership, the kickoff prompts below.
- TASKS.md: the section 8 tables, with a status column.

### 9.2 Kickoff prompt: Person A

```
You are Person A for Byte World (Unity 6000.6.9f1, URP 2D, uGUI, TMP, WebGL, landscape, tap only).
Read PRD-v2.md, CLAUDE.md and STREAMS.md first, and use the game-dev and content-writer skills.
You own: every Resources/Data/*.json, Scripts/Core (non-contract), World/ (including HeroController
and CheckpointManager), Jobs/, Debug/, GameRoot.prefab, World.unity, Job_*.unity, the contract files
(one PR per change only), and the Hunt module (Scripts/UI/Hunt/, Editor/HuntBuilder.cs,
Resources/UI/Hunt.prefab).
Never touch UIRootBuilder.cs, UIRoot.prefab, Scripts/Build/ or any other Scripts/UI file.

Do the Person A tasks from PRD-v2.md section 8 in table order. A-01 and A-13 are the only contract
changes: each is one PR straight to main, compiling, stubs updated.
For JSON: follow PRD section 5 and 4.11. Show me each new item list before writing it, keep lines
under 110 characters (cards under 140), fictional domains and documentation-range IPs only. Check
every Cisco product line against cisco.com. Then run Byte World > Validate Data.

After every compile, read the Unity Console through the MCP and fix every new error. Never hand-edit
.unity or .prefab YAML. Commit messages "area: what changed". For each task: plan briefly,
implement, run its check, tell me what you verified, and wait for me before the next task.
```

### 9.3 Kickoff prompt: Person B

```
You are Person B for Byte World (Unity 6000.6.9f1, URP 2D, uGUI, TMP, WebGL, landscape, tap only).
Read PRD-v2.md, CLAUDE.md and STREAMS.md first, and use the ui-screens skill.
You own: Scripts/UI (except Scripts/UI/Hunt/), UIRootBuilder.cs, UIRoot.prefab, Scripts/Build/,
Boot.unity, Build.unity, the WebGL template and Art/Fonts/. You never edit JSON: ask Person A.

Do the Person B tasks from PRD-v2.md section 8 in table order, then the Shoulds. Start B-01 to B-03
now. Before B-14, merge main to pick up A's contract/hero PR. Build Quest 4 in scripted mode first
(flags.json quest4Mode).
Style: every size and colour in UI/UiStyle.cs from Palette constants only. Main buttons Packet
Yellow with ink text, secondary buttons white with an ink border, Alert Pink only for danger and
alert states, no text under 34 at 1920x1080. Use the visual reference
(https://claude.ai/artifact/Rt5KF4SqTzdRjUDNtrUdXc) for the popup, the card, the materials panel
and the toolkit, and the pitch deck screens for the rest.

Change UI only by editing UIRootBuilder.cs and re-running Byte World > UI > Build UIRoot Prefab.
Never hand-edit prefab YAML. After every compile, read the Unity Console through the MCP and fix
every new error. Test with the Fake Event Panel. For each task: plan briefly, implement, check,
tell me what you verified, and wait for me before the next task.
```

### 9.4 Unity MCP and reviewer (unchanged)

The Unity MCP setup from v1.3 stays: a personal `.mcp.json`, git-ignored. Run the reviewer subagent before each merge.

---

## 10. Process

### 10.1 Schedule (v2.1)

| Time | A | B |
|---|---|---|
| Sat 22:00 | Merge window | Merge window |
| 22:00–23:30 | A-13 contract/hero and checkpoints.json | B-14 popup and card (on stubs) |
| 23:30 | Voice-over recording (11.3) | Voice-over recording |
| 23:45–02:00 | A-14 map relayout, A-15 hero and checkpoints | B-15 materials panel, B-04 quest-end sequence |
| Sun 08:00 | Merge window and cut-off: set `quest4Mode` and `heroMode` | |
| 08:00–10:00 | A-05 to A-08, then A-10 | B-05 to B-07, then Shoulds |
| 10:00 | Feature freeze, tag `freeze` | |
| 10:30 | Final WebGL build, tag `demo` | |

### 10.2 Branches and merge windows

- `main` is always playable. No direct pushes, except the contract PRs.
- Branches: `a/<short-name>` and `b/<short-name>`, merged into `stream/a-quests` and `stream/b-screens`.
- Merge windows: Sat 22:00 · Sat 23:30 (contract/hero) · Sun 08:00 (cut-off: set `quest4Mode` and `heroMode`) · 08:30 · 10:00 (feature freeze, tag `freeze`) · 10:30 (final WebGL build, tag `demo`).
- At each window, A merges first, then B merges main again and merges. Use merge commits, not squash. A WebGL build of main is checked on a phone after every window.

### 10.3 Definition of done

A task is done when:

1. It compiles with no new Unity Console errors (read through the MCP).
2. **Byte World > Validate Data** says “All data valid”.
3. It plays end to end with the Fake Event Panel or the URL shortcuts.
4. It looks right in a 1920×1080 capture.
5. The reviewer subagent has run, and TASKS.md is updated.

---

## 11. Pitch and video

### 11.1 The deck is the spine

The 3-minute video is the pitch deck recorded with voice-over, with real game footage laid over the slides wherever we have it. Each slide’s speaker notes hold its voice-over line and its time slot, and the slots add up to exactly 3:00. Structure: hook, problem, insight, product, loop, Quests 1–4 (6 slides), toolkit and materials, learner outcomes, close.

### 11.2 Shot list (v2.1)

Capture at 1920×1080, 30 fps, using the A-08 URL shortcuts so each shot starts clean.

| # | Slide it covers | Shot | Length | Fallback |
|---|---|---|---|---|
| 1 | Product | Map with two alerts; Pax walks with WASD from Router Plaza to Scout’s Harbour; the popup appears | 6 s | Visual reference recording |
| 2 | Loop | Press Enter, the card expands, Enter again to go in, iris into the Harbour | 5 s | Visual reference recording |
| 3 | Quest 1 | Scout inspects bank-supp0rt.co, taps Quarantine, the Field Card appears, “Cisco Umbrella added to your Cisco toolkit” | 8 s | Slide only |
| 4 | Toolkit and materials | See materials at Gigi’s Gate: drag the slider from without to with Cisco Secure Firewall | 6 s | Visual reference recording |
| 5 | Quest 1 | Gigi turns Luca away at door 23 while the other alert flashes | 6 s | Slide only |
| 6 | Quest 2 | The Twins see 4 push requests and Keep shut | 5 s | Slide only |
| 7 | Splunk trace | Chips tap in, the table drops, the row is pinned | 10 s | Slide (prototype screen) |
| 8 | Quest 3 | A switch is placed, the test packet bounces off the vault lane | 8 s | Slide (prototype screen) |
| 9 | Quest 4 | The call freezes, Call is tapped, the links turn pink and green | 10 s | Slide (prototype screen) |
| 10 | Close | Peace celebration, the toolkit at 7/7, the Learner Card | 6 s | Slide only |

### 11.3 Production

- The voice-over is recorded at 23:30 Saturday, one take per section, in a quiet room with the phone about 20 cm away.
- Edit in CapCut, iMovie or DaVinci Resolve. Burn in captions, and keep music about 20 dB under the voice.
- Check the export is exactly 3:00. Hold the end card to fill any gap, and trim gameplay pauses, never the voice.
- Add team names and a QR code to the playable build on the end card. Export H.264 MP4, and watch it once on a phone before submitting.

---

## 12. Risks and open questions (v2.1)

| Risk | Likelihood | Mitigation |
|---|---|---|
| Quest 4 siege isn’t stable by Sunday 08:00 | High | `quest4Mode: scripted` is built first (B-06). The live fix is stretch |
| Both people are overloaded (about 15 h for A and 12.5 h for B) | High | B-09 to B-12 drop first, props are stretch, the iris is a Should. A picks up B-16 if ahead, after agreeing file ownership |
| The 32 × 32 hero art takes too long | High | Only the Must frames tonight (idle and side walk, 2 portraits). Placeholder hero at 2× if not ready by Sunday 08:00 |
| Hero walking or road waypoints are buggy | Medium | `heroMode: teleport` keeps the same popup and card flow. Decide at Sunday 08:00 |
| Contract PR 2 (`contract/hero`) lands late and blocks B-14 | Medium | B builds B-14 against the stub first; the stub returns `checkpoints.json` with two stations on Alert |
| The map relayout breaks existing alerts | Medium | `CheckpointManager` wraps `AlertController` instead of replacing it; Quest 1 is tested end to end before the 08:00 window |
| Wrong answer keys or wrong product claims in JSON | Medium | A reads every item, checks product lines against cisco.com (4.11), and Validate Data runs on each merge |
| Footage isn’t ready for the video | Medium | Every shot in 11.2 has a fallback; shots 1, 2 and 4 can be recorded from the visual reference |
| CLARA objects to Cisco names or logos | Low | Names in text only. Ask a mentor tonight |

Open questions:

- [ ] When is the video due, and where is it submitted? At the 2025 special edition, projects were due by 12:30 on Sunday (regulations). Confirm this year’s time with the organisers tonight.
- [ ] Is the video judged alone, or alongside a live demo? That changes how much playable footage matters.
- [ ] Who records the voice-over?
- [ ] Can we use Cisco product names in the game and video? Logos?
- [ ] **(v2.1)** Is “Pax” the hero’s name?
- [ ] **(v2.1)** Who draws the Pax sheet tonight? A-16 sits with A because A owns `World/`, but it can move to whoever is faster at pixel art, as long as the files stay in `Art/Sprites/`.
- [ ] **(v2.1)** Is the toolkit language (“collect the stack”) welcome to CLARA, or too salesy? Ask a mentor tonight.
