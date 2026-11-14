#!/usr/bin/env python3
"""Draws the v2 grids (PRD-v2 6.6) with simple shapes and writes them to Tools/sprites/grids/:
Pax (32 x 32: idle down and side, walk side 4 frames) and the seven station buildings (64 x 48, with a
dim mood for locked stations). Re-run after changing a shape; hand-edit the grids for pixel tweaks.
Then run make_sprites.py."""
import os

GRIDS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "grids")


class C:
    def __init__(s, w, h): s.w, s.h, s.g = w, h, [["."] * w for _ in range(h)]
    def put(s, x, y, ch):
        if 0 <= x < s.w and 0 <= y < s.h: s.g[int(y)][int(x)] = ch
    def rect(s, x, y, w, h, ch):
        for yy in range(int(y), int(y + h)):
            for xx in range(int(x), int(x + w)): s.put(xx, yy, ch)
    def ell(s, cx, cy, rx, ry, ch, only=None):
        for y in range(s.h):
            for x in range(s.w):
                if ((x + .5 - cx) / rx) ** 2 + ((y + .5 - cy) / ry) ** 2 <= 1 and (only is None or only(x, y)): s.put(x, y, ch)
    def outlined(s):
        out = [r[:] for r in s.g]
        for y in range(s.h):
            for x in range(s.w):
                if s.g[y][x] == ".":
                    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                        xx, yy = x + dx, y + dy
                        if 0 <= xx < s.w and 0 <= yy < s.h and s.g[yy][xx] not in ".1":
                            out[y][x] = "1"; break
        return ["".join(r) for r in out]


def save(name, folder, c, moods="normal"):
    with open(os.path.join(GRIDS, name + ".txt"), "w") as f:
        f.write(f"# folder: {folder}\n# moods: {moods}\n" + "\n".join(c.outlined()) + "\n")


# ---------- Pax: a young learner (person, not a Byte). Yellow hoodie, blue trousers, headset, Visitor Pass.
def pax(facing="down", leg=0, blink=False, bob=0):
    c = C(32, 32)
    y0 = 6 + bob
    # legs (blue trousers) and shoes
    if facing == "down":
        c.rect(12, y0 + 17, 3, 6, "2"); c.rect(17, y0 + 17, 3, 6, "2")
        c.rect(11, y0 + 23, 4, 2, "1"); c.rect(17, y0 + 23, 4, 2, "1")
    else:
        stride = [(-2, 2), (0, 0), (2, -2), (0, 0)][leg]
        c.rect(14 + stride[0], y0 + 17, 3, 6, "2"); c.rect(16 + stride[1], y0 + 17, 3, 6, "C")
        c.rect(13 + stride[0], y0 + 23, 4, 2, "1"); c.rect(15 + stride[1], y0 + 23, 4, 2, "1")
    # hoodie body and arms
    c.ell(16, y0 + 13, 6, 5.5, "y"); c.rect(10, y0 + 12, 12, 6, "y"); c.rect(10, y0 + 16, 12, 2, "o")
    if facing == "down":
        c.rect(8, y0 + 11, 2, 6, "y"); c.rect(22, y0 + 11, 2, 6, "y"); c.rect(8, y0 + 17, 2, 2, "4"); c.rect(22, y0 + 17, 2, 2, "4")
    else:
        swing = [1, 0, -1, 0][leg]
        c.rect(15 + swing, y0 + 11, 3, 6, "o"); c.rect(15 + swing, y0 + 17, 3, 2, "4")
    # Visitor Pass on its lanyard (glows cyan)
    if facing == "down":
        c.put(14, y0 + 9, "1"); c.put(18, y0 + 9, "1"); c.rect(15, y0 + 10, 3, 1, "1"); c.rect(15, y0 + 11, 3, 4, "3")
    # head, hair, face
    c.ell(16, y0 + 4, 5.5, 5, "4")
    c.ell(16, y0 + 1, 6, 3.5, "1", only=lambda x, y: y <= y0 + 2)
    if facing == "down":
        if blink: c.rect(13, y0 + 5, 2, 1, "1"); c.rect(18, y0 + 5, 2, 1, "1")
        else: c.rect(13, y0 + 4, 2, 2, "1"); c.rect(18, y0 + 4, 2, 2, "1")
        c.rect(15, y0 + 7, 3, 1, "5"); c.put(12, y0 + 6, "a"); c.put(20, y0 + 6, "a")
        c.rect(10, y0 + 3, 1, 4, "C"); c.rect(21, y0 + 3, 1, 4, "C")       # headset ear cups
        c.rect(10, y0 + 7, 3, 1, "C"); c.put(13, y0 + 7, "3")               # mic
    else:
        if blink: c.rect(18, y0 + 5, 2, 1, "1")
        else: c.rect(18, y0 + 4, 2, 2, "1")
        c.rect(19, y0 + 7, 2, 1, "5"); c.put(20, y0 + 6, "a")
        c.rect(13, y0 + 3, 2, 4, "C"); c.rect(14, y0 + 7, 4, 1, "C"); c.put(18, y0 + 7, "3")
        c.ell(13, y0 + 3, 3, 3, "1", only=lambda x, y: x <= 14)             # hair at the back
    return c


for i, (f, blink) in enumerate((("down", False), ("down", True))):
    save(f"pax_idle_down_{i}", "Characters", pax("down", blink=blink, bob=0 if i == 0 else 1))
for i, blink in enumerate((False, True)):
    save(f"pax_idle_side_{i}", "Characters", pax("side", leg=1, blink=blink, bob=0 if i == 0 else 1))
for i in range(4):
    save(f"pax_walk_side_{i}", "Characters", pax("side", leg=i, bob=1 if i % 2 else 0))


# ---------- Station buildings, 64 x 48, bottom centre is the door (PRD-v2 4.2, 4.3)
def base(c, roof):
    c.rect(4, 44, 56, 4, "G")                                   # ground shadow
def harbour():
    c = C(64, 48); base(c, "2")
    c.rect(2, 34, 60, 6, "w"); [c.rect(x, 34, 1, 6, "W") for x in range(4, 62, 6)]   # pier
    c.rect(6, 40, 3, 6, "W"); c.rect(54, 40, 3, 6, "W")
    c.rect(36, 12, 4, 22, "4"); c.rect(36, 12, 1, 22, "p")          # radar mast
    c.ell(38, 9, 10, 4, "c", only=lambda x, y: y <= 9); c.rect(37, 4, 2, 6, "1"); c.put(38, 3, "5")
    c.rect(10, 18, 20, 16, "4"); c.rect(10, 14, 20, 4, "2"); c.rect(16, 24, 6, 10, "2")   # lookout hut
    c.rect(24, 22, 4, 4, "k")
    c.rect(44, 40, 14, 4, "K"); c.rect(46, 41, 6, 1, "k")           # water under the pier
    return c
def gate():
    c = C(64, 48); base(c, "c")
    c.rect(0, 12, 64, 32, "c"); [c.rect(x, 12, 1, 32, "C") for x in range(0, 64, 8)]
    for y in range(12, 44, 6): c.rect(0, y, 64, 1, "C")
    for x in range(0, 64, 8): c.rect(x, 8, 5, 4, "c")               # battlements
    for i, (x, col) in enumerate(((6, "2"), (20, "2"), (34, "5"), (48, "2"))):
        c.rect(x, 26, 10, 18, col); c.rect(x + 2, 28, 6, 2, "4"); c.put(x + 7, 36, "y")
    return c
def vault():
    c = C(64, 48); base(c, "C")
    c.rect(8, 8, 48, 36, "c"); c.rect(8, 8, 48, 6, "2"); [c.put(x, 10, "3") for x in range(11, 54, 4)]
    c.ell(32, 28, 12, 12, "4"); c.ell(32, 28, 8, 8, "2"); c.ell(32, 28, 5, 5, "k"); c.ell(32, 28, 2, 2, "5")
    c.rect(31, 18, 2, 20, "1"); c.rect(22, 27, 20, 2, "1")
    c.rect(12, 26, 6, 8, "1"); c.rect(13, 27, 4, 6, "3"); c.rect(46, 26, 6, 8, "1"); c.rect(47, 27, 4, 6, "3")
    return c
def workshop():
    c = C(64, 48); base(c, "2")
    c.rect(8, 16, 48, 28, "2"); [c.rect(x, 12, 2, 4, "y") for x in range(10, 56, 5)]; [c.rect(x, 44, 2, 2, "y") for x in range(10, 56, 5)]
    for x in range(6, 58, 8): c.rect(x, 10, 4, 6, "5"); c.rect(x + 4, 10, 4, 6, "4")   # striped awning
    c.rect(26, 26, 12, 18, "4"); c.ell(32, 33, 3, 3, "1"); c.ell(32, 33, 1.5, 1.5, "4")
    for x, y in ((10, 34), (16, 34), (13, 28)): c.rect(x, y, 6, 6, "o"); c.rect(x, y + 2, 6, 1, "W")
    return c
def observatory():
    c = C(64, 48); base(c, "4")
    c.ell(32, 22, 18, 14, "4", only=lambda x, y: y <= 22); c.rect(14, 22, 36, 22, "4"); c.rect(14, 22, 36, 2, "p")
    c.rect(28, 6, 8, 4, "C"); c.rect(30, 3, 4, 4, "1")              # telescope slot
    c.rect(18, 26, 28, 12, "C")                                      # dashboard screen
    for i, h in enumerate((3, 6, 4, 8, 5, 9)): c.rect(21 + i * 4, 36 - h, 2, h, "3" if i % 2 else "y")
    c.rect(28, 38, 8, 6, "2")
    return c
def switchyard():
    c = C(64, 48); base(c, "2")
    for y in (14, 24, 34):
        c.rect(6, y, 52, 8, "2"); c.rect(6, y, 52, 1, "k")
        for x in range(10, 54, 6): c.rect(x, y + 3, 4, 3, "1"); c.rect(x + 1, y + 4, 2, 1, "3")
    c.rect(2, 10, 4, 34, "W"); c.rect(58, 10, 4, 34, "W")
    return c
def tower():
    c = C(64, 48); base(c, "c")
    for y in range(8, 44):                                          # lattice tower narrowing upward
        half = 4 + (y - 8) // 3
        c.put(32 - half, y, "c"); c.put(32 + half, y, "c")
        if y % 6 == 0: c.rect(32 - half, y, half * 2, 1, "c")
    c.rect(31, 2, 2, 8, "1"); c.ell(32, 2, 2, 2, "5")
    for r in (8, 13, 18): c.ell(32, 4, r, r * .6, "3", only=lambda x, y, r=r: y < 4 and abs(((x + .5 - 32) / r) ** 2 + ((y + .5 - 4) / (r * .6)) ** 2 - 1) < .2)
    c.rect(26, 36, 12, 8, "4"); c.rect(30, 39, 4, 5, "2")
    return c


for name, f in (("station_harbour", harbour), ("station_gate", gate), ("station_vault", vault), ("station_workshop", workshop),
                ("station_observatory", observatory), ("station_switchyard", switchyard), ("station_tower", tower)):
    save(name, "Props", f(), "normal dim")
print("v2 grids written")
