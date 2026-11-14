#!/usr/bin/env python3
"""Makes Tools/sprites/splash/town_clean.png for the splash art: the hi-fi town (Tools/sprites/hifi/map.png) on
plain grass, without the river, the roads, the houses and the clutter. It keeps the trees, the station
buildings (not the Harbour, which stands on the river), Router Plaza's router, and every small Byte.
Run before render.sh: python3 Tools/sprites/splash/make_town.py"""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, ".."))
from import_hifi import read_png  # noqa: E402

GRASS_PATCH = (28, 32, 48, 48)          # a clean 48 x 48 piece of grass on the hi-fi map, tiled as the base
LEAVES = {(47, 154, 85), (95, 201, 110), (134, 224, 142), (113, 182, 100)}
GROUND = {(132, 216, 111), (95, 189, 90), (243, 227, 184), (217, 196, 143), (211, 214, 201), (182, 187, 171),
          (63, 140, 240), (124, 196, 255), (71, 230, 255)}
# Kept from the map (x, y, width, height): station buildings and the router.
KEEP = [
    (80, 42, 47, 39),      # Duo Vault
    (176, 57, 45, 37),     # Splunk Observatory
    (203, 48, 18, 9),      # its dish (the red house beside it stays out)
    (214, 13, 53, 43),     # Switch Yard
    (336, 22, 42, 50),     # Signal Tower
    (214, 191, 53, 42),    # Endpoint Workshop
    (66, 176, 50, 34),     # Gigi's Gate
    (225, 93, 30, 43),     # the router and its Wi-Fi arcs
]
# Every small Byte (top-left of its 16 x 16 sprite): station Bytes and the roaming ones.
BYTES = [(373, 185), (105, 201), (113, 65), (253, 217), (213, 85), (253, 40), (363, 55),
         (153, 126), (292, 165), (150, 201), (320, 186)]


def main():
    m = read_png(os.path.join(HERE, "..", "hifi", "map.png"))
    # Buildings come from the game's town map, which already has the alert rings taken out.
    clean = read_png(os.path.join(HERE, "..", "..", "..", "Assets", "_Project", "Resources", "Sprites", "Props", "town_map.png"))
    byte = read_png(os.path.join(HERE, "..", "hifi", "byte.png"))
    h, w = len(m), len(m[0])
    px, py, pw, ph = GRASS_PATCH
    out = [[m[py + y % ph][px + x % pw] for x in range(w)] for y in range(h)]

    # Trees: their leaf colours wherever they are.
    for y in range(h):
        for x in range(w):
            if m[y][x][:3] in LEAVES:
                out[y][x] = m[y][x]
    # Buildings and the router, without the road or water under them.
    for bx, by, bw, bh in KEEP:
        for y in range(by, by + bh):
            for x in range(bx, bx + bw):
                if clean[y][x][:3] not in GROUND:
                    out[y][x] = clean[y][x]
    # The small Bytes, exactly their own pixels.
    for bx, by in BYTES:
        for y in range(16):
            for x in range(16):
                if byte[y][x][3]:
                    out[by + y][bx + x] = m[by + y][bx + x]
    write_png_at(os.path.join(HERE, "town_clean.png"), out)
    print("town_clean.png written")


def write_png_at(path, pixels):
    import struct
    import zlib
    height, width = len(pixels), len(pixels[0])
    raw = b"".join(b"\x00" + bytes(c for p in row for c in p) for row in pixels)

    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
                + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b""))


if __name__ == "__main__":
    main()
