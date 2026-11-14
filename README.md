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

## WebGL build and itch.io (Stream B)

1. In Unity: **Byte World > WebGL > Build**. It applies the PRD 6.8 settings (Gzip with decompression fallback, data caching, stripping) and the `ByteWorld` page template, then builds to `Builds/WebGL` (git-ignored).
2. Zip the **contents** of `Builds/WebGL` (so `index.html` is at the top of the zip): `cd Builds/WebGL && zip -r ../ByteWorld-webgl.zip .`
3. On the itch.io project page (**Kind of project: HTML**), upload the zip and tick **This file will be played in the browser**, then set:
   - **Viewport dimensions:** 960 × 540
   - **Mobile friendly:** on, orientation **Landscape**
   - **Fullscreen button:** on
   - **Automatically start on page load:** off (a tap to start also unlocks audio on phones)
   - **Visibility:** Restricted (link only) until the demo
4. Check the page on an iPhone and an Android phone. Held upright, the game asks you to turn the phone sideways.
