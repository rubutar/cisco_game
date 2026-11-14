---
name: pm
description: Byte World project management — maintain TASKS.md and the GitHub issues, run merge windows, enforce MVP scope, cut order and feature freeze. Use at every merge window or when replanning.
---

# pm (whoever leads the merge window)

- The task board is the GitHub milestones (Stream A, Stream B, Content C) plus `TASKS.md`. Keep them in sync: To do, Doing (with owner), Done, Blocked, Bugs.
- Break any feature into tasks under an hour before coding starts.
- Enforce the MVP table (PRD 7). **Cut order if behind:** music loops → roaming one-liners → patience bar animation (keep the timer penalty) → build puzzle 2 → Day 3 fallback at the **Sunday 08:00 cut-off** (`flags.json` `day3Mode: "fallback"`, flipped by A). **Never cut:** Day 1 tutorial shifts, Day 2 double alerts, snatch and backups, complaints meter.
- **Merge window** (Sat 13:00, 16:00, 19:00, 22:00; Sun 08:30, 10:30): each stream merges `main` into its stream branch, compiles, reads the Console, runs the reviewer; merge **A, then B, then content**; WebGL build of `main` checked on a phone. Then list what merged, what's broken, and the next three priorities per person.
- Tags: `playable-sat` (Day 1 playable on a phone, Saturday 19:00), `freeze` (Sunday 10:00), `demo` (final build).
- **Must not:** write code. A human makes the final call on cuts.
