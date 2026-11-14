---
name: content-writer
description: Byte World content rules for person C (also pasted into DeepSeek prompts) — voice, the no-jargon rule, difficulty per day, JSON schemas for days.json, dialogue.json, roaming.json and build.json. Use when writing or checking any game text or item data.
---

# content-writer (Content C)

**Voice:** Bytes speak in first person about their job, like a shopkeeper. Teen-friendly, short, playful. Max ~110 characters per line (one bubble); reveal cards max ~140.

**Never use security or networking jargon on Days 1 and 2**: no "phishing", "firewall", "2FA", "malware", "port", "update" as a lecture. Day 3 reveal cards are the only place real terms appear (router, switch, port, socket, firewall); traffic labels stay plain ("Luca's photo", "to door 8080").

**Every success line names the trick in plain words** ("the name had a zero in it").

**Difficulty (PRD 4.7):** Day 1 is a tutorial, 3–4 items per shift, one obvious attack, and every attack item has a `hint`. Day 2: 7–9 items, sneaky tricks mixed in one shift, mixed signals (suspicious-looking but safe), `maxWrongBlocks: 3`, a `waitingLine` and `complaintLine` per job. Day 2 tricks must match things teens really meet.

**JSON (schemas are the C# classes in `Assets/_Project/Scripts/Core/Data/`):**
- Valid JSON, unique `id` on every item, `attacker` on every attack item, `kind` is `safe` or `attack`.
- `alertGroups` are objects: `[{ "jobs": ["scout", "gigi"] }]` (JsonUtility can't read nested arrays).
- Button labels and HUD formats live in `dialogue.json` → `ui`.
- **Patch:** a real crate is `"kind": "safe"` (the right call is Fit now); give it `"worm": true`, `"wormSpeed"` (world units per second, 0.5 ≈ 20 s to the wall) and `"attacker": "worm"` when a Worm heads for its crack. A fake crate is `"type": "fakeCrate", "kind": "attack"` (the right call is Later). Twins value words: `valueLabels` = [right, wrong, expired, not asked], in that order.
- Every build puzzle must have at least one correct solution: solve it on paper first.
- Check: in Unity, **Byte World > Validate Data** reports "All data valid" (it reads the files with the real C# classes, checks ids, attackers, hints and line lengths).

**Must not:** edit code. Deliver through `c/*` branches.
