#!/usr/bin/env python3
"""
Convert ASCII text grids in Tools/sprites/grids/ into PNG sprites
for normal, dim, and alert palettes (PRD Section 6.6).
"""

from pathlib import Path
from PIL import Image

GRIDS_DIR = Path(__file__).parent / "grids"
OUTPUT_DIR = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Art" / "Sprites"

# Palette definitions based on PRD Section 6.6:
# 1: ink, 2: signal blue, 3: neon cyan, 4: byte cream, 5: alert pink
PALETTES = {
    "normal": {
        ".": (0, 0, 0, 0),
        "1": (0x1D, 0x1B, 0x33, 255),  # Ink outline
        "2": (0x4A, 0x5F, 0xC1, 255),  # Signal blue (eyes, feet)
        "3": (0x47, 0xE6, 0xFF, 255),  # Neon cyan (belly)
        "4": (0xFF, 0xF4, 0xD6, 255),  # Byte cream (body)
        "5": (0xFF, 0x5D, 0x73, 255),  # Alert pink (antenna tip, attacker eyes)
    },
    "alert": {
        ".": (0, 0, 0, 0),
        "1": (0x1D, 0x1B, 0x33, 255),
        "2": (0x4A, 0x5F, 0xC1, 255),
        "3": (0xFF, 0xB3, 0xBD, 255),  # Belly swaps to #FFB3BD in alert state (Section 6.6)
        "4": (0xFF, 0xF4, 0xD6, 255),
        "5": (0xFF, 0x5D, 0x73, 255),
    },
    "dim": {
        ".": (0, 0, 0, 0),
        "1": (0x1D, 0x1B, 0x33, 255),
        "2": (0x3B, 0x3F, 0x6E, 255),  # Dimmed blue (server cliff dark)
        "3": (0x57, 0x5D, 0x9A, 255),  # Dimmed belly (server cliff light)
        "4": (0xD9, 0xC4, 0x8F, 255),  # Dimmed cream (road edge)
        "5": (0x9C, 0x6A, 0x35, 255),  # Dimmed antenna
    },
}


def load_grid(grid_path: Path):
    lines = [
        line.strip()
        for line in grid_path.read_text(encoding="utf-8").splitlines()
        if line.strip() and not line.strip().startswith("#")
    ]
    if not lines:
        raise ValueError(f"Empty grid file: {grid_path}")

    width = len(lines[0])
    for idx, row in enumerate(lines):
        if len(row) != width:
            raise ValueError(f"{grid_path.name} row {idx + 1} has length {len(row)}, expected {width}")
    return lines


def render_grid(lines, palette, out_path: Path):
    height = len(lines)
    width = len(lines[0])
    img = Image.new("RGBA", (width, height))
    pixels = img.load()

    for y, row in enumerate(lines):
        for x, ch in enumerate(row):
            if ch not in palette:
                raise ValueError(f"Invalid character '{ch}' at ({x}, {y})")
            pixels[x, y] = palette[ch]

    out_path.parent.mkdir(parents=True, exist_ok=True)
    img.save(out_path, "PNG")


def main():
    grid_files = sorted(GRIDS_DIR.glob("*.txt"))
    if not grid_files:
        print(f"No .txt grids found in {GRIDS_DIR}")
        return

    for grid_path in grid_files:
        lines = load_grid(grid_path)
        name = grid_path.stem
        for mood, palette in PALETTES.items():
            out_file = OUTPUT_DIR / f"{name}_{mood}.png"
            render_grid(lines, palette, out_file)
            print(f"Generated {out_file.relative_to(OUTPUT_DIR.parents[3])} ({len(lines[0])}x{len(lines)})")


if __name__ == "__main__":
    main()