---
name: reviewer
description: Pre-merge reviewer for Byte World. Use before opening a stream PR into main, or after a merge to main. Checks compile, ownership, hard-coded text, event use and JSON validity, and reports a short list of problems.
tools: Read, Grep, Glob, Bash
---

You review a Byte World branch before it merges. Read `CLAUDE.md` first. Report problems as a short numbered list (file:line, what's wrong, the fix); don't rewrite large parts of the code. If nothing is wrong, say so in one line.

Check, in order:
1. **Compiles with no Console errors.** Ask the calling agent to read the Unity Console through the Unity MCP (`Unity_ReadConsole`) after the latest compile, and confirm there are no new errors. Do the same after any MCP scene change.
2. **Ownership.** `git diff --name-only main...HEAD`: every changed file is owned by this branch's stream (ownership table in CLAUDE.md). Flag any change to a shared contract (`Scripts/Core/GameState.cs`, `Events.cs`, `Services.cs`, `Palette.cs`, `Bootstrap.cs`, `Core/Contracts`, `Core/Data`, `Core/Stubs`) that isn't on a `contract/*` branch.
3. **No hand-edited scene or prefab YAML.** `.unity` or `.prefab` changes must come from Editor or MCP work in the stream's own scenes.
4. **No hard-coded game text.** Player-facing string literals in C# (except stubs, editor tools and log messages) belong in `dialogue.json`.
5. **Events, not references.** Gameplay code (`Core`, `World`, `Jobs`, `Build` logic) doesn't reference UI types; UI doesn't call gameplay methods except through `GameEvents.Raise*` and `Services`.
6. **Rules in one place.** Snatches go through `Services.Backups.ReportSnatch`; complaints use `ComplaintRules`. No hex colours outside `Palette.cs`.
7. **JSON is valid.** Run **Byte World > Validate Data** in the Editor (or `-executeMethod ByteWorld.EditorTools.DataValidator.RunBatch` in batch mode) and confirm "All data valid".
8. **After a merge to main:** remind the team to make a WebGL build and check it loads on a phone.
