#!/usr/bin/env python3
"""Byte World sprite pipeline (PRD 6.6): text grids -> PNGs for Unity. No installs needed (plain Python 3).

Grid files: Tools/sprites/grids/<name>.txt, one character per pixel, one row per line.
  .  transparent
  1  ink #1D1B33      2  signal blue #4A5FC1   3  neon cyan #47E6FF
  4  Byte cream #FFF4D6   5  alert pink #FF5D73
  y  packet yellow #FFD23F   o  pod orange #FF9A3C   w  wood #C98F4E   W  wood dark #9C6A35
  g  grass light #84D86F     G  grass dark #5FBD5A   c  cliff light #575D9A   C  cliff dark #3B3F6E
  p  road light #F3E3B8 (cream shade)   P  road edge #D9C48F   a  alert belly #FFB3BD
  k  water light #7CC4FF    K  water deep #3F8CF0   e  leaves dark #2F9A55  E  leaves light #5FC96E
Header lines start with '#':
  # folder: Characters      -> Assets/_Project/Resources/Sprites/Characters (pivot at the feet)
  # folder: Props           -> .../Sprites/Props (pivot in the centre); also Build, UI
  # moods: normal alert dim -> extra PNGs <name>_alert.png and <name>_dim.png (swap indices 1-5 only)

Usage:
  python3 Tools/sprites/make_sprites.py                 # write every PNG
  python3 Tools/sprites/make_sprites.py --preview out.png   # also write one enlarged sheet to look at
Unity imports them with Point filter, no compression, 16 PPU (Scripts/Editor/PixelArtImporter.cs).
"""
import os
import struct
import sys
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
GRIDS = os.path.join(ROOT, "Tools", "sprites", "grids")
OUT = os.path.join(ROOT, "Assets", "_Project", "Resources", "Sprites")


def hex_rgb(h):
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)


# PRD 6.6 palette. Index 0/'.' is transparent.
BASE = {
    "1": "1D1B33", "2": "4A5FC1", "3": "47E6FF", "4": "FFF4D6", "5": "FF5D73",
    "y": "FFD23F", "o": "FF9A3C", "w": "C98F4E", "W": "9C6A35",
    "g": "84D86F", "G": "5FBD5A", "c": "575D9A", "C": "3B3F6E",
    "p": "F3E3B8", "P": "D9C48F", "a": "FFB3BD", "k": "7CC4FF", "K": "3F8CF0", "e": "2F9A55", "E": "5FC96E",
}
MOODS = {
    "normal": {},
    "alert": {"3": "FFB3BD"},  # cyan belly turns pink
    "dim": {"1": "2A2940", "2": "4F5068", "3": "7B7D92", "4": "B8B7C4", "5": "6F6878"},
}


def read_grid(path):
    header, rows = {}, []
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.rstrip("\n")
            if line.startswith("#"):
                if ":" in line:
                    key, value = line[1:].split(":", 1)
                    header[key.strip()] = value.strip()
            elif line.strip():
                rows.append(line.rstrip())
    width = max(len(r) for r in rows)
    rows = [r.ljust(width, ".") for r in rows]
    return header, rows


def render(rows, mood):
    colours = dict(BASE)
    colours.update(MOODS[mood])
    pixels = []
    for row in rows:
        line = []
        for ch in row:
            if ch == ".":
                line.append((0, 0, 0, 0))
            elif ch in colours:
                line.append(hex_rgb(colours[ch]))
            else:
                raise ValueError(f"unknown colour '{ch}'")
        pixels.append(line)
    return pixels


def write_png(path, pixels):
    height, width = len(pixels), len(pixels[0])
    raw = b"".join(b"\x00" + bytes(c for px in row for c in px) for row in pixels)

    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    if os.path.dirname(path):
        os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(png)


def preview(sheets, path, scale=6, gap=4):
    """One enlarged sheet of every sprite on a grey background, for checking by eye."""
    width = sum(len(p[0]) * scale + gap for _, p in sheets) + gap
    height = max(len(p) * scale for _, p in sheets) + 2 * gap
    canvas = [[(140, 140, 150, 255)] * width for _ in range(height)]
    x = gap
    for _, pixels in sheets:
        for py, row in enumerate(pixels):
            for px, colour in enumerate(row):
                if colour[3] == 0:
                    continue
                for dy in range(scale):
                    for dx in range(scale):
                        canvas[gap + py * scale + dy][x + px * scale + dx] = colour
        x += len(pixels[0]) * scale + gap
    write_png(path, canvas)


def main():
    preview_path = sys.argv[sys.argv.index("--preview") + 1] if "--preview" in sys.argv else None
    sheets, count = [], 0
    for name in sorted(os.listdir(GRIDS)):
        if not name.endswith(".txt"):
            continue
        header, rows = read_grid(os.path.join(GRIDS, name))
        base = name[:-4]
        folder = header.get("folder", "Props")
        for mood in header.get("moods", "normal").split():
            pixels = render(rows, mood)
            suffix = "" if mood == "normal" else f"_{mood}"
            write_png(os.path.join(OUT, folder, f"{base}{suffix}.png"), pixels)
            sheets.append((f"{base}{suffix}", pixels))
            count += 1
    if preview_path:
        preview(sheets, preview_path)
    print(f"Wrote {count} PNG(s) to {os.path.relpath(OUT, ROOT)}" + (f", preview {preview_path}" if preview_path else ""))


if __name__ == "__main__":
    main()
