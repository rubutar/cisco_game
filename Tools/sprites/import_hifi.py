#!/usr/bin/env python3
"""Turns the hi-fi design's images (Tools/sprites/hifi/, downloaded from the Byte World hi-fi Design artifact)
into the game's sprites, so the map, Pax and the Bytes look exactly like the hi-fi:

  Props/town_map.png        the 480 x 270 map, with the two alert rings and the four roaming Bytes taken out
                            (the game draws those itself; the station Bytes stay baked in and the game's
                            Bytes are drawn exactly over them)
  Props/road_<id>.png       one overlay per station whose road is grey in the hi-fi: the same road in sand with
                            the cyan centre dashes, shown once that station opens
  Props/alert_ring.png      the pink alert ring, cut out of the map, shown at stations on alert
  Characters/map_*.png      the hi-fi Byte (normal, alert, dim) for every map Byte
  Characters/pax_*.png      Pax: the hi-fi standing and walking frames for every pose
  UI/pax_portrait_*.png     48 x 48 head-and-shoulders crops of the standing frame, scaled 2x

Plain Python, no libraries. Run from anywhere: python3 Tools/sprites/import_hifi.py
Never edit these PNGs by hand: change the hi-fi, download its images again into hifi/ and re-run."""
import os
import struct
import zlib

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, "hifi")
OUT = os.path.join(HERE, "..", "..", "Assets", "_Project", "Resources", "Sprites")

PINK = (255, 93, 115)
ROAD_GREY, ROAD_GREY_EDGE = (211, 214, 201), (182, 187, 171)
ROAD_SAND, ROAD_SAND_EDGE = (243, 227, 184), (217, 196, 143)
CYAN = (71, 230, 255)

# Alert rings baked into the hi-fi map (centre, outer radius), at the Gate and the Harbour.
RINGS = [((90, 213), 34), ((357, 195), 34)]
# Roaming Bytes baked into the hi-fi map (top-left of the 16 x 16 sprite); the game spawns its own.
ROAMERS = [(153, 126), (292, 165), (150, 201), (320, 186)]
# The roads under them (x0, y0, x1, y1, inclusive; True when the road runs left to right), to rebuild the ground.
ROAMER_ROADS = [(154, 134, 306, 146, True), (154, 87, 166, 133, False), (154, 147, 166, 221, False),
                (294, 147, 306, 206, False), (294, 194, 341, 206, True), (108, 209, 166, 221, True)]
# Grey roads to light when a station opens: rectangles (x0, y0, x1, y1, inclusive) and the dash line,
# (x, y, length, horizontal). Shared trunk pieces are listed for every station that uses them.
TRUNK = [(234, 107, 246, 123)]
ROADS = {
    "vault": ([(94, 74, 166, 86), (154, 87, 166, 133)], [(100, 80, 66, True), (160, 86, 48, False)]),
    "observatory": ([(194, 94, 246, 106)] + TRUNK, [(200, 100, 40, True), (240, 106, 18, False)]),
    "switchyard": ([(234, 49, 246, 106)] + TRUNK, [(240, 58, 66, False)]),
    "tower": ([(234, 64, 356, 76), (234, 77, 246, 106)] + TRUNK, [(246, 70, 104, True), (240, 76, 48, False)]),
    "workshop": ([(234, 162, 246, 190), (234, 224, 246, 238)], [(240, 164, 26, False)]),
}


# ---------- PNG in and out ----------

def read_png(path):
    data = open(path, "rb").read()
    pos, idat, palette, trns = 8, b"", None, None
    while pos < len(data):
        length, = struct.unpack(">I", data[pos:pos + 4])
        kind, body = data[pos + 4:pos + 8], data[pos + 8:pos + 8 + length]
        pos += 12 + length
        if kind == b"IHDR":
            width, height, depth, color, _, _, interlace = struct.unpack(">IIBBBBB", body)
            assert depth == 8 and interlace == 0, "only 8-bit, non-interlaced PNGs"
        elif kind == b"PLTE":
            palette = [tuple(body[i:i + 3]) for i in range(0, len(body), 3)]
        elif kind == b"tRNS":
            trns = list(body)
        elif kind == b"IDAT":
            idat += body
    bpp = {6: 4, 2: 3, 3: 1, 0: 1, 4: 2}[color]
    raw, stride = zlib.decompress(idat), width * bpp
    rows, prev, i = [], bytearray(stride), 0
    for _ in range(height):
        f, line = raw[i], bytearray(raw[i + 1:i + 1 + stride])
        i += 1 + stride
        for x in range(stride):
            a = line[x - bpp] if x >= bpp else 0
            b = prev[x]
            c = prev[x - bpp] if x >= bpp else 0
            if f == 1: line[x] = (line[x] + a) & 255
            elif f == 2: line[x] = (line[x] + b) & 255
            elif f == 3: line[x] = (line[x] + (a + b) // 2) & 255
            elif f == 4:
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                line[x] = (line[x] + (a if pa <= pb and pa <= pc else b if pb <= pc else c)) & 255
        rows.append(line)
        prev = line
    pixels = []
    for line in rows:
        row = []
        for x in range(width):
            if color == 6: row.append(tuple(line[x * 4:x * 4 + 4]))
            elif color == 2: row.append(tuple(line[x * 3:x * 3 + 3]) + (255,))
            elif color == 3:
                k = line[x]
                row.append(palette[k] + ((trns[k] if trns and k < len(trns) else 255),))
            elif color == 4: row.append((line[x * 2],) * 3 + (line[x * 2 + 1],))
            else: row.append((line[x],) * 3 + (255,))
        pixels.append(row)
    return pixels


def write_png(name, pixels):
    path = os.path.join(OUT, name + ".png")
    height, width = len(pixels), len(pixels[0])
    raw = b"".join(b"\x00" + bytes(c for px in row for c in px) for row in pixels)

    def chunk(kind, data):
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(png)


def blank(w, h):
    return [[(0, 0, 0, 0)] * w for _ in range(h)]


def mirrored(pixels):
    return [list(reversed(row)) for row in pixels]


# ---------- The map ----------

def clean_colours(m):
    """Colours that appear often enough to be part of the art (blends from the rings don't)."""
    counts = {}
    for row in m:
        for p in row:
            counts[p[:3]] = counts.get(p[:3], 0) + 1
    return [c for c, n in counts.items() if n >= 20 and c != PINK]


def take_out_rings(m):
    """Each ring pixel is the art underneath blended toward pink: find the art colour and how much pink.
    Where the ring is almost solid pink the art can't be read back, so those take their neighbours' colour."""
    palette = clean_colours(m)
    exact = set(palette)
    ring = blank(72, 72)
    for (cx, cy), radius in RINGS:
        solid = []
        for y in range(cy - radius, cy + radius + 1):
            for x in range(cx - radius, cx + radius + 1):
                p = m[y][x][:3]
                # Art colours, and solid pink art (the Gate's shut door, the lighthouse stripes), stay.
                if p in exact or p == PINK or (x - cx) ** 2 + (y - cy) ** 2 > radius * radius:
                    continue
                best = None
                for c in palette:
                    d = [PINK[i] - c[i] for i in range(3)]
                    dd = sum(v * v for v in d)
                    if dd == 0:
                        continue
                    a = max(0.0, min(1.0, sum((p[i] - c[i]) * d[i] for i in range(3)) / dd))
                    err = sum((p[i] - (c[i] + a * d[i])) ** 2 for i in range(3))
                    if best is None or err < best[0]:
                        best = (err, c, a)
                if best is None:
                    continue
                alpha = best[2]
                if alpha >= 0.95:
                    solid.append((x, y))
                    alpha = 1.0
                else:
                    m[y][x] = best[1] + (255,)
                # The Gate's ring is the one cut out as the sprite (a faint tint below 0.05 is left out).
                rx, ry = x - cx + 36, y - cy + 36
                if (cx, cy) == RINGS[0][0] and alpha > 0.05 and 0 <= rx < 72 and 0 <= ry < 72:
                    ring[ry][rx] = PINK + (round(alpha * 255),)
        for x, y in solid:
            around = [m[y + dy][x + dx][:3] for dx in (-1, 0, 1) for dy in (-1, 0, 1)
                      if (dx or dy) and (x + dx, y + dy) not in solid and m[y + dy][x + dx][:3] in exact]
            if around:
                m[y][x] = max(set(around), key=around.count) + (255,)
    # The castle and the Gate's Byte hide part of the ring's right side: complete it from the left half
    # (the ring's centre is between pixels 35 and 36).
    for y in range(72):
        for x in range(36):
            a, b = ring[y][x], ring[y][71 - x]
            ring[y][71 - x] = a if a[3] > b[3] else b
            ring[y][x] = ring[y][71 - x]
    return ring


def take_out_roamers(m, byte):
    mask = [(x, y) for y in range(16) for x in range(16) if byte[y][x][3]]
    covered = {(bx + x, by + y) for bx, by in ROAMERS for x, y in mask}
    grass = {(132, 216, 111), (95, 189, 90), (134, 224, 142), (113, 182, 100)}
    for bx, by in ROAMERS:
        for x, y in mask:
            X, Y = bx + x, by + y
            m[Y][X] = ground(m, X, Y, covered, grass)


def ground(m, X, Y, covered, grass):
    """The ground under a roaming Byte: copied along the road it stands on (18 px is three road dashes,
    so the dashes line up), or from nearby grass."""
    for x0, y0, x1, y1, across in ROAMER_ROADS:
        if not (x0 <= X <= x1 and y0 <= Y <= y1):
            continue
        for d in (18, -18, 36, -36, 54, -54):
            sx, sy = (X + d, Y) if across else (X, Y + d)
            if x0 <= sx <= x1 and y0 <= sy <= y1 and (sx, sy) not in covered and not in_road_cross(sx, sy, x0, y0, x1, y1):
                return m[sy][sx]
    for d in (18, -18, 36, -36):
        for sx, sy in ((X + d, Y), (X, Y + d)):
            if (sx, sy) not in covered and m[sy][sx][:3] in grass and not any(
                    a <= sx <= c and b <= sy <= e for a, b, c, e, _ in ROAMER_ROADS):
                return m[sy][sx]
    return (132, 216, 111, 255)


def in_road_cross(x, y, x0, y0, x1, y1):
    """True inside another road crossing this one (its pixels belong to the other road's pattern)."""
    return any(a <= x <= c and b <= y <= e and (a, b, c, e) != (x0, y0, x1, y1) for a, b, c, e, _ in ROAMER_ROADS)


def road_overlays(m):
    for station, (rects, dashes) in ROADS.items():
        out = blank(len(m[0]), len(m))
        for x0, y0, x1, y1 in rects:
            for y in range(y0, y1 + 1):
                for x in range(x0, x1 + 1):
                    p = m[y][x][:3]
                    if p == ROAD_GREY: out[y][x] = ROAD_SAND + (255,)
                    elif p == ROAD_GREY_EDGE: out[y][x] = ROAD_SAND_EDGE + (255,)
        for x, y, length, horizontal in dashes:
            for i in range(length):
                if i % 6 >= 3:
                    continue
                X, Y = (x + i, y) if horizontal else (x, y + i)
                if out[Y][X][3]:
                    out[Y][X] = CYAN + (255,)
        write_png(f"Props/road_{station}", out)


# ---------- Bytes, Pax and portraits ----------

def dim_byte(byte, grey_from_map):
    """The hi-fi's grey Byte: the normal Byte in the grey colours read off the map, without its antenna."""
    table = {(255, 244, 214): grey_from_map["body"], (29, 27, 51): grey_from_map["ink"],
             (71, 230, 255): grey_from_map["belly"], (255, 179, 189): grey_from_map["cheek"],
             (74, 95, 193): grey_from_map["feet"], (255, 93, 115): grey_from_map["feet"],
             (255, 255, 255): grey_from_map["body"]}
    grey = [[(table.get(p[:3], p[:3]) + (p[3],)) if p[3] else p for p in row] for row in byte]
    return [[(0, 0, 0, 0)] * 16 if y < 3 else row for y, row in enumerate(grey)]


def portrait(stand):
    """Head and shoulders (24 x 24 from the top of the standing frame), scaled 2x to 48 x 48."""
    crop = [row[4:28] for row in stand[0:24]]
    return [[crop[y // 2][x // 2] for x in range(48)] for y in range(48)]


def main():
    m = read_png(os.path.join(SRC, "map.png"))
    byte = read_png(os.path.join(SRC, "byte.png"))
    byte_alert = read_png(os.path.join(SRC, "byte_alert.png"))
    stand = read_png(os.path.join(SRC, "pax_stand.png"))
    walk = read_png(os.path.join(SRC, "pax_walk.png"))

    # The grey (locked) Byte's colours, read off the Switch Yard's Byte in the hi-fi map.
    grey = {"ink": m[43][257][:3], "body": m[44][257][:3], "belly": m[50][258][:3],
            "cheek": m[48][257][:3], "feet": m[55][258][:3]}

    ring = take_out_rings(m)
    take_out_roamers(m, byte)
    write_png("Props/town_map", m)
    write_png("Props/alert_ring", ring)
    road_overlays(m)

    dim = dim_byte(byte, grey)
    for name in ("byte", "scout", "gigi", "uno", "due", "patch"):
        write_png(f"Characters/map_{name}", byte)
        write_png(f"Characters/map_{name}_alert", byte_alert)
        write_png(f"Characters/map_{name}_dim", dim)

    # Pax: standing for idle and poses, a two-step walk (the step and its mirror image).
    frames = {"idle_down": [stand, stand], "idle_side": [stand, stand],
              "walk_side": [stand, walk, stand, mirrored(walk)], "walk_down": [stand, walk, stand, mirrored(walk)],
              "walk_up": [stand, walk, stand, mirrored(walk)], "interact_side": [stand, stand],
              "cheer_down": [stand, walk], "worried_down": [stand]}
    for pose, list_ in frames.items():
        for i, frame in enumerate(list_):
            write_png(f"Characters/pax_{pose}_{i}", frame)
    head = portrait(stand)
    for mood in ("neutral", "happy", "surprised", "worried", "determined"):
        write_png(f"UI/pax_portrait_{mood}", head)
    print("hi-fi sprites written")


if __name__ == "__main__":
    main()
