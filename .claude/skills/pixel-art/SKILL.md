---
name: pixel-art
description: Byte World v2 art rules for Person A — palette, sprite sizes (Pax 32 × 32, Bytes 16 × 16, stations 64 × 48, portraits 48 × 48), animation sets, the grid → PNG pipeline (Tools/sprites/make_sprites.py), quantize.py for Unity AI output, Unity import settings and the in-code fallbacks. Use when drawing or editing any sprite.
---

# pixel-art (Person A owns all art)

**Palette:** PRD 6.6 / `Palette.cs` only. Grid letters in `Tools/sprites/make_sprites.py`: `.` transparent, `1` ink, `2` signal blue, `3` neon cyan, `4` Byte cream, `5` alert pink, `y` packet yellow, `o` pod orange, `w`/`W` wood, `g`/`G` grass, `c`/`C` cliff, `p`/`P` road (cream shade), `a` alert belly, `k`/`K` water, `e`/`E` leaves. Moods: `alert` (cyan → #FFB3BD), `dim`.

**Sizes (PRD-v2 6.6):** Pax map 32 × 32 frame (~24 px tall, Packet Yellow hoodie, Signal Blue trousers, headset, glowing Visitor Pass), pivot at the feet; Bytes and attackers 16 × 16 (two-pixel faces: happy, worried, surprised; station Bytes carry one prop each); station buildings 64 × 48 on a 4 px grid; Pax portraits 48 × 48 (neutral, happy; Should: surprised, worried, determined). Job-scene close-ups may use the 32 × 32 Byte set already in `Resources/Sprites/Characters/`.

**Pax animations:** Must: idle (down, side, 2 frames), walk side (4 frames, flipped for left). Should: walk up and down, interact, cheer, worried. One row per animation in a sheet; Unity import Sprite (Multiple) for sheets.

**Pipeline:** text grids in `Tools/sprites/grids/<name>.txt` with headers `# folder: Characters|Props|Build|UI` and `# moods: normal alert dim` → `python3 Tools/sprites/make_sprites.py [--preview out.png]` → PNGs in `Assets/_Project/Resources/Sprites/<folder>/`. Look at the `--preview` sheet before committing. The game loads them with `PixelArt.Load("Characters/name", fallback)`, which falls back to the in-code drawing when a PNG is missing. Piskel or Aseprite exports and Unity AI output go through `Tools/sprites/quantize.py` (needs Pillow) to snap to the palette.

**Import (`Scripts/Editor/PixelArtImporter.cs`, first import only):** Sprite, Point filter, no compression, no mipmaps, 16 PPU; `Characters` folders get the pivot at the feet. If a PNG was imported before the importer existed, delete its `.meta` to reimport.

**Style:** round, cute, readable at size, ink outline, never scary; original designs only, no logos. **Fallback:** if Pax's sheet isn't ready by Sunday 08:00, a Byte with a yellow cap drawn at 2× on the map.
