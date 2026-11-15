---
name: content-writer
description: Byte World v2 content rules for Person A — voice, real Cisco terms, the Cisco marketing rules (PRD-v2 4.11), item counts per quest, and the JSON schemas for days.json, dialogue.json, hunt.json, build.json, flags.json and checkpoints.json. Use when writing or checking any game text, item data, Field Card, check question or checkpoint materials.
---

# content-writer (Person A owns all JSON)

**Voice:** Bytes speak in the first person about their job, friendly and short. Pax is mostly silent. One bubble ≤ ~110 characters; cards ≤ 140 a line; checkpoint lines ≤ 110, without/with ≤ 40 each, materials lines ≤ 60.

**Real terms are allowed everywhere** (v2 replaces the v1 no-jargon rule). Explain each term once, on its Field Card (term, plain line ≤ 140, Cisco tool, general concept, career role).

**Cisco rules (PRD-v2 4.11):** full product name where the player acts ("Cisco Secure Firewall", not "the firewall"; Splunk and Webex as is); without/with is always the same piece of town; concrete outcomes, never adjectives; **only true claims** from cisco.com: no invented stats, customers, prices or awards; "In the real world" says how organisations use it in one line; every materials panel and the Learner Card point to Cisco Networking Academy; no logos; Splunk is "part of Cisco since 2024".

**Items (PRD-v2 5):** Quest 1 Scout 6 (2 attacks, 1 odd-but-safe; fields From, Reply-To, Sender check, Link), Gigi 6 (2 attacks, 1 odd-but-safe; rules 443 Everyone, 8080 Friends only, 23 Closed; mid-quest change via `rulesChangeAtItem`/`rulesChangeLine`); Quest 2 Twins 5 (wrong code, expired code, push bombing via `pushCount`; 1 odd-but-safe), Patch 5 (`signature` invalid, look-alike sender; 1 odd-but-safe). Quest 1 attacks carry a `hint`. Every attack has an `attacker`. All domains, IPs and CVEs fictional; IPs only from 192.0.2.x, 198.51.100.x, 203.0.113.x.

**JSON (classes in `Assets/_Project/Scripts/Core/Data/` are the schema):**
- JsonUtility: public fields, no dictionaries, **no nested arrays** — `alertGroups` are `[{ "jobs": ["scout", "gigi"] }]`, never `[["scout"]]`.
- `checkpoints.json`: `plaza`, `checkpoints[] {id, quest, station, character, job (scout|gigi|twins|patch|hunt|build|siege), tool, x, y, path[], requires[], line, without, with, materials {what, withoutLines[3], withLines[3], realWorld, learnMoreLabel, learnMoreUrl}}` in 480 × 270 map pixels.
- `dialogue.json`: `briefs[]`, `jobs[].days[].fieldCard`, `checks[] {quest, question, options[4], answer, explanation}`, `props[]`, `learnerCard`, `incident`, UI labels in `ui`.
- `hunt.json`, `build.json` (lanes, testPacket, rules, siege), `flags.json` (`quest4Mode`, `heroMode`, `heroSpeed`).
- **Patch:** a real crate is `"kind": "safe"` (Fit now); fake updates are `"type": "fakeCrate", "kind": "attack"` (Later); `"type": "crack"` is a crack to repair.
- Check questions: 4 options, one correct, a one-line explanation, never a trick.
- Check: **Byte World > Validate Data** says "All data valid". Read every item before merging: a wrong answer key is worse than a missing item.

**Italian (the default language):** every English line you add or change needs its Italian in `Tools/localize/it.json` (English text as the key), then run `python3 Tools/localize/make_locale.py`, which rewrites `Resources/Data/it/` and fails on a missing translation or a `{0}` mismatch. Keep real security terms and Cisco product names as they are; informal "tu"; same length limits.
