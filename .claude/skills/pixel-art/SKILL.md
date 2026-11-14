---
name: pixel-art
description: Byte World pixel-art rules for person C — palette, sprite grid format, sizes, make_sprites.py and quantize.py, Unity import settings. Use when drawing or editing sprite grids or processing generated art.
---

# pixel-art (Content C)

**Palette:** the techy town palette, exactly the hex codes in PRD 6.6 (also in `Assets/_Project/Scripts/Core/Palette.cs`). Grid indices: `.` transparent, `1` ink #1D1B33, `2` signal blue #4A5FC1, `3` neon cyan #47E6FF, `4` Byte cream #FFF4D6, `5` alert pink #FF5D73.
- `alert` mood: index 3 becomes #FFB3BD. `dim` mood: 1 #2A2940, 2 #4F5068, 3 #7B7D92, 4 #B8B7C4, 5 #6F6878.

**Grids:** `Tools/sprites/grids/<name>.txt`, one character per pixel, one row per line. Bytes and attackers 12 × 12 or 16 × 16; props on a 4-pixel grid; the world map is 480 × 270 matching the techy town layout (screen-flow reference).
**Style:** round, cute, big eyes, readable at 12 × 12, outlined in ink. Original designs only: no look-alikes of existing characters, no logos (including Cisco's).

**Pipeline:** `python3 Tools/sprites/make_sprites.py` writes one PNG per grid per mood into `Assets/_Project/Art/Sprites/`. Generated art (Unity AI) goes through `quantize.py` (snap to palette, nearest-neighbour downscale) before use. The importer in `Scripts/Editor/PixelArtImporter.cs` sets Sprite, 16 PPU, Point, no compression, no mipmaps for anything under `Assets/_Project/Art/`.

**Must not:** edit code, scenes or prefabs. Hand PNGs over through a `c/*` branch; the owning stream wires them in.
