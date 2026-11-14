# Byte World

A short, landscape, pixel-art story game for teens about cybersecurity, built for The Big Hack 2026 (Consorzio CLARA / Cisco). Unity **6000.6.9f1**, URP 2D, uGUI + TextMeshPro, WebGL.

## Getting started

1. Install Unity **6000.6.9f1** with **WebGL Build Support** via Unity Hub. Everyone uses exactly this version.
2. Clone the repo, then in Unity Hub choose **Add > Add project from disk** and select the repo root.
3. Open `Assets/_Project/Scenes/Boot.unity` and press Play. Missing systems fall back to stubs, so it runs from day one.
4. Set up the Unity MCP for Claude Code (PRD 9.4). Your `.mcp.json` holds a personal path and is git-ignored.

## Where to read next

- `STREAMS.md`: the two workstreams, ownership, merge windows, and the kickoff prompts.
- `CLAUDE.md` (and `AGENTS.md` for Codex): architecture, contracts and rules for every agent.
- `TASKS.md`: the task board.

## Testing one stream alone

In Play mode, open **Byte World > Fake Event Panel** to fire any game event with sample data, or call `Services.Flow.StartAt(day, puzzle)`.
