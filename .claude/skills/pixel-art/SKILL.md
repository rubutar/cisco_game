---
name: pixel-art
description: Byte World v2 art rules for Person A — palette, sprite sizes (Pax 32 × 32, Bytes 16 × 16, stations 64 × 48, portraits 48 × 48), animation sets, the hi-fi import (Tools/sprites/import_hifi.py), the grid → PNG pipeline (Tools/sprites/make_sprites.py), quantize.py for Unity AI output, Unity import settings and the in-code fallbacks. Use when drawing or editing any sprite.
---

# pixel-art (Person A owns all art)

**Palette:** PRD 6.6 / `Palette.cs` only. Grid letters in `Tools/sprites/make_sprites.py`: `.` transparent, `1` ink, `2` signal blue, `3` neon cyan, `4` Byte cream, `5` alert pink, `y` packet yellow, `o` pod orange, `w`/`W` wood, `g`/`G` grass, `c`/`C` cliff, `p`/`P` road (cream shade), `a` alert belly, `k`/`K` water, `e`/`E` leaves. Moods: `alert` (cyan → #FFB3BD), `dim`.

**The hi-fi design is the source of truth for the map, Pax and the map Bytes** (Byte World hi-fi Design artifact, 14 Nov). Its images live in `Tools/sprites/hifi/` (`map.png` 480 × 270, `pax_stand.png` and `pax_walk.png` 32 × 32, `byte.png` and `byte_alert.png` 16 × 16). `python3 Tools/sprites/import_hifi.py` turns them into `Props/town_map.png` (rings and roaming Bytes taken out; the station Bytes stay baked in and the game draws its Bytes exactly over them), `Props/road_<id>.png` (a grey road in sand, shown when its station opens), `Props/alert_ring.png`, every `Characters/map_{byte,scout,gigi,uno,due,patch}[_alert|_dim].png`, every `Characters/pax_*.png` and the `UI/pax_portrait_*.png` crops. Never edit those PNGs or add grids for them: change the hi-fi, download its images into `hifi/` again and re-run. Station doors and roads in `checkpoints.json` and the Byte spots in `WorldMap.HifiBytes` are measured off this map, so move them together.

**Sizes (PRD-v2 6.6):** Pax map 32 × 32 frame, pivot at the feet (Pax is the hi-fi's girl in a yellow jacket and headphones). Bytes and attackers 16 × 16; station buildings 64 × 48 on a 4 px grid (only used when there's no hi-fi map); Pax portraits 48 × 48. Job-scene close-ups may use the 32 × 32 Byte set already in `Resources/Sprites/Characters/`.

**Pax animations:** the hi-fi has a standing and a walking frame; `import_hifi.py` makes idle from standing and every walk a stand, step, stand, mirrored-step cycle (flipped for left). The poses (interact, cheer, worried) use the standing frame until the hi-fi draws them.

**Pipeline (everything else):** text grids in `Tools/sprites/grids/<name>.txt` with headers `# folder: Characters|Props|Build|UI` and `# moods: normal alert dim` → `python3 Tools/sprites/make_sprites.py [--preview out.png]` → PNGs in `Assets/_Project/Resources/Sprites/<folder>/`. Look at the `--preview` sheet before committing. The game loads them with `PixelArt.Load("Characters/name", fallback)`, which falls back to the in-code drawing when a PNG is missing. Piskel or Aseprite exports and Unity AI output go through `Tools/sprites/quantize.py` (needs Pillow) to snap to the palette.

**Import (`Scripts/Editor/PixelArtImporter.cs`, first import only):** Sprite, Point filter, no compression, no mipmaps, 16 PPU; `Characters` folders get the pivot at the feet. If a PNG was imported before the importer existed, delete its `.meta` to reimport.

**Style:** round, cute, readable at size, ink outline, never scary; original designs only, no logos.
