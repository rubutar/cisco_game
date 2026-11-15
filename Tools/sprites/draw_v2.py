#!/usr/bin/env python3
"""Draws the v2 grids (PRD-v2 6.6) with simple shapes and writes them to Tools/sprites/grids/: the seven
station buildings (64 x 48, with a dim mood for locked stations), station Bytes, map attackers, the
Visitor Pass and tool badges. Pax, the station Bytes and the town map come from the hi-fi design (import_hifi.py). Re-run after changing a shape; hand-edit the grids for pixel tweaks.
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


# Pax comes from the hi-fi design (import_hifi.py).


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


# ---------- Station Bytes on the map, 16 x 16, holding their props (PRD-v2 6.6), and their faces
def byte16(face="smile", ant=(8,), extra=None):
    c = C(16, 16)
    c.rect(4, 14, 3, 1, "2"); c.rect(9, 14, 3, 1, "2")
    c.ell(8, 9, 6, 5.5, "4"); c.ell(8, 9, 6, 5.5, "p", only=lambda x, y: x >= 11)
    c.ell(8, 9, 6, 5.5, "3", only=lambda x, y: y >= 11)
    for ax in ant: c.rect(ax, 2, 1, 2, "1"); c.put(ax, 1, "5")
    if face == "worried":
        c.rect(5, 7, 1, 2, "1"); c.rect(10, 7, 1, 2, "1"); c.put(4, 6, "1"); c.put(11, 6, "1"); c.put(7, 10, "2"); c.put(8, 11, "2"); c.put(9, 10, "2")
    elif face == "surprised":
        c.rect(5, 6, 1, 3, "1"); c.rect(10, 6, 1, 3, "1"); c.rect(7, 10, 2, 2, "1")
    elif face == "happy":
        c.put(5, 7, "1"); c.put(4, 8, "1"); c.put(10, 7, "1"); c.put(11, 8, "1"); c.rect(7, 10, 2, 1, "1")
    else:
        c.rect(5, 7, 1, 2, "1"); c.rect(10, 7, 1, 2, "1"); c.rect(7, 10, 2, 1, "2")
    c.put(4, 10, "a"); c.put(11, 10, "a")
    if extra: extra(c)
    return c
# The station Bytes on the map (map_byte, map_scout, map_gigi, map_uno, map_due, map_patch, normal, alert and
# dim) come from the hi-fi design: Tools/sprites/import_hifi.py writes them.
for face in ("happy", "worried", "surprised"):
    save(f"map_byte_{face}", "Characters", byte16(face))


# ---------- Attackers on the map, 16 x 16
def phisher16():
    c = C(16, 16)
    c.ell(6, 9, 4.5, 5.5, "2"); c.ell(6, 7, 3.5, 3, "C"); c.put(5, 7, "5"); c.put(8, 7, "5")
    c.rect(11, 2, 1, 12, "W"); c.rect(12, 2, 3, 1, "1"); c.rect(14, 2, 1, 6, "1"); c.rect(13, 8, 2, 1, "5")
    return c
def ram16():
    c = C(16, 16)
    c.ell(8, 10, 6, 4.5, "2"); [c.rect(x, 13, 2, 2, "C") for x in (4, 7, 10)]
    c.ell(3, 6, 2.5, 2.5, "p"); c.ell(13, 6, 2.5, 2.5, "p"); c.put(3, 6, "W"); c.put(13, 6, "W")
    c.ell(8, 7, 3.5, 3, "c"); c.put(6, 7, "5"); c.put(10, 7, "5"); c.rect(6, 4, 5, 1, "4")
    return c
def worm16():
    c = C(16, 16)
    for i, (x, y) in enumerate(((3, 11), (6, 12), (9, 12))): c.ell(x, y, 2, 2, "2" if i % 2 else "c")
    c.ell(12.5, 9, 3, 3, "2"); c.put(13, 8, "5")
    return c
save("map_phisher", "Characters", phisher16())
save("map_ram", "Characters", ram16())
save("map_worm", "Characters", worm16())


# ---------- Visitor Pass and the seven tool badges (UI), one colour per tool (PRD-v2 4.2a, 6.6)
def badge(symbol, ring):
    c = C(16, 16)
    c.ell(8, 8, 7.5, 7.5, ring); c.ell(8, 8, 6, 6, "4")
    symbol(c)
    return c
def umbrella(c): c.ell(8, 7, 5, 3.5, "3", only=lambda x, y: y <= 7); c.rect(8, 7, 1, 5, "1"); c.put(7, 12, "1")
def firewall(c):
    for y in (5, 8, 11): c.rect(3, y, 10, 2, "5")
    for x, y in ((6, 5), (9, 8), (6, 11)): c.put(x, y, "4")
def duo(c): c.rect(5, 3, 6, 10, "1"); c.rect(6, 4, 4, 7, "G"); c.put(7, 8, "4"); c.put(8, 9, "4"); c.put(9, 7, "4")
def endpoint(c):  # a shield with a tick
    for y in range(3, 13):
        half = 5 if y < 9 else 5 - (y - 8)
        c.rect(8 - half, y, half * 2 + 1, 1, "2")
    c.put(5, 7, "4"); c.put(6, 8, "4"); c.put(7, 9, "4"); c.put(8, 8, "4"); c.put(9, 7, "4"); c.put(10, 6, "4")
def splunk(c):
    for i, h in enumerate((3, 6, 4, 8)): c.rect(4 + i * 2, 12 - h, 1, h, "o")
def catalyst(c):
    for y in (5, 8, 11): c.rect(3, y, 10, 1, "K")
    for x, y in ((5, 5), (10, 8), (7, 11)): c.rect(x, y - 1, 2, 3, "1")
def thousandeyes(c): c.ell(8, 8, 5, 3, "4"); c.ell(8, 8, 5, 3, "1", only=lambda x, y: abs(((x + .5 - 8) / 5) ** 2 + ((y + .5 - 8) / 3) ** 2 - 1) < .45); c.ell(8, 8, 2, 2, "K"); c.put(8, 8, "1")
for name, sym, ring in (("umbrella", umbrella, "3"), ("firewall", firewall, "5"), ("duo", duo, "G"), ("endpoint", endpoint, "2"),
                        ("splunk", splunk, "o"), ("catalyst", catalyst, "K"), ("thousandeyes", thousandeyes, "y")):
    save(f"badge_{name}", "UI", badge(sym, ring), "normal dim")

def visitor_pass():
    c = C(48, 64)
    c.rect(22, 0, 4, 8, "2")                                                    # lanyard
    c.rect(4, 8, 40, 54, "4"); c.rect(4, 8, 40, 12, "3"); c.rect(18, 10, 12, 4, "1")  # card and cyan header
    c.rect(14, 22, 20, 18, "k")                                                 # photo background
    c.ell(24, 40, 8, 5, "y", only=lambda x, y: y <= 39)                          # hoodie shoulders
    c.ell(24, 30, 5, 5.5, "4")                                                  # face
    c.ell(24, 27, 5.5, 3.5, "1", only=lambda x, y: y <= 27)                     # hair
    c.put(22, 30, "1"); c.put(26, 30, "1"); c.rect(23, 33, 3, 1, "5")           # eyes, smile
    for i in range(7):                                                          # seven badge slots
        x = 8 + (i % 4) * 9; y = 42 + (i // 4) * 9
        c.rect(x, y, 7, 7, "P"); c.rect(x + 1, y + 1, 5, 5, "p")
    return c
save("visitor_pass", "UI", visitor_pass())



# ---------- Language flags for the pause menu (UI, 16 x 12, ink outline): Italian and English (UK)
def flag_it():
    c = C(16, 12)
    c.rect(1, 1, 5, 10, "e"); c.rect(6, 1, 4, 10, "4"); c.rect(10, 1, 5, 10, "5")
    return c
def flag_en():
    c = C(16, 12)
    c.rect(1, 1, 14, 10, "2")
    for i in range(10):                                              # white diagonals, corner to corner
        x = 1 + round(i * 13 / 9)
        c.put(x, 1 + i, "4"); c.put(14 - (x - 1), 1 + i, "4")
    c.rect(1, 4, 14, 4, "4"); c.rect(6, 1, 4, 10, "4")             # white cross
    c.rect(1, 5, 14, 2, "5"); c.rect(7, 1, 2, 10, "5")             # red cross
    return c
# Settings gear (UI, 16 x 16): cream gear with an ink outline and an ink ring round the hole
def gear():
    c = C(16, 16)
    c.ell(8, 8, 4.6, 4.6, "4")
    for x, y, w, h in ((7, 1, 2, 3), (7, 12, 2, 3), (1, 7, 3, 2), (12, 7, 3, 2),
                       (3, 3, 2, 2), (11, 3, 2, 2), (3, 11, 2, 2), (11, 11, 2, 2)):
        c.rect(x, y, w, h, "4")
    c.ell(8, 8, 1.6, 1.6, ".")
    return c
save("gear", "UI", gear())
save("flag_it", "UI", flag_it())
save("flag_en", "UI", flag_en())
print("bytes, attackers, badges, gear, flags written")
