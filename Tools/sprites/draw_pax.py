#!/usr/bin/env python3
"""Draws Pax, the visiting learner (PRD-v2 4.2a, 6.6), into Tools/sprites/grids/: the 32 x 32 map frames
(idle, walk in three directions, interact, cheer, worried) and the 48 x 48 UI portraits. Pax is a chibi
boy in an orange hoodie, navy trousers, navy headphones with pink lights and a glowing cyan Visitor Pass,
so he stands out from the cream-and-cyan Bytes. Re-run after a change, then run make_sprites.py."""
import os

GRIDS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "grids")


class C:
    def __init__(s, w=32, h=32): s.w, s.h, s.g = w, h, [["."] * w for _ in range(h)]
    def put(s, x, y, ch):
        x, y = int(x), int(y)
        if 0 <= x < s.w and 0 <= y < s.h: s.g[y][x] = ch
    def rect(s, x, y, w, h, ch):
        for yy in range(int(y), int(y + h)):
            for xx in range(int(x), int(x + w)): s.put(xx, yy, ch)
    def ell(s, cx, cy, rx, ry, ch, only=None):
        for y in range(s.h):
            for x in range(s.w):
                if ((x + .5 - cx) / rx) ** 2 + ((y + .5 - cy) / ry) ** 2 <= 1 and (only is None or only(x, y)): s.put(x, y, ch)
    def pts(s, ch, *p):
        for x, y in p: s.put(x, y, ch)
    def outlined(s):
        out = [r[:] for r in s.g]
        for y in range(s.h):
            for x in range(s.w):
                if s.g[y][x] == ".":
                    for dx, dy in ((1,0),(-1,0),(0,1),(0,-1)):
                        xx, yy = x + dx, y + dy
                        if 0 <= xx < s.w and 0 <= yy < s.h and s.g[yy][xx] not in ".1":
                            out[y][x] = "1"; break
        return ["".join(r) for r in out]

# Palette letters: 1 ink, 4 cream (skin), p shade, o orange hoodie, y yellow highlight, C navy, K blue, 3 cyan pass, 5/a pink

def legs_front(c, y, step):
    l, r = [(0, 0), (-1, 1), (0, 0), (1, -1)][step]
    c.rect(12, y + l, 3, 5 - abs(l), "C"); c.rect(17, y + r, 3, 5 - abs(r), "C")
    c.rect(11, y + 5, 4, 2, "4"); c.rect(17, y + 5, 4, 2, "4")          # sneakers
    c.rect(11, y + 6, 4, 1, "K"); c.rect(17, y + 6, 4, 1, "K")

def hoodie_front(c, y, arms="down", step=0):
    c.ell(16, y + 4, 6.5, 5, "o"); c.rect(10, y + 3, 12, 6, "o")       # torso
    c.rect(10, y + 3, 2, 6, "y")                                        # highlight on the left
    c.rect(15, y + 1, 1, 4, "y"); c.rect(17, y + 1, 1, 4, "y")          # drawstrings
    c.rect(12, y + 7, 8, 2, "W")                                        # pocket shadow
    if arms == "up":
        c.rect(7, y - 5, 3, 8, "o"); c.rect(22, y - 5, 3, 8, "o"); c.rect(7, y - 7, 3, 2, "4"); c.rect(22, y - 7, 3, 2, "4")
    else:
        sw = [0, 1, 0, -1][step]
        c.rect(7, y + 2 + sw, 3, 6, "o"); c.rect(22, y + 2 - sw, 3, 6, "o")
        c.rect(7, y + 8 + sw, 3, 2, "4"); c.rect(22, y + 8 - sw, 3, 2, "4")
    # Visitor Pass on a yellow lanyard
    c.pts("y", (13, y), (19, y), (14, y + 1), (18, y + 1))
    c.rect(14, y + 2, 5, 4, "3"); c.rect(15, y + 3, 3, 1, "K")

def head_front(c, y, eyes="open", mouth="smile", brows="normal", sweat=False):
    c.ell(16, y + 6, 7, 6.5, "4")                                       # face
    c.ell(16, y + 6, 7, 6.5, "p", only=lambda x, yy: x >= 21)           # shade
    # hair: dark cap with spiky fringe and a blue highlight
    c.ell(16, y + 3, 7.5, 5, "1", only=lambda x, yy: yy <= y + 4)
    c.pts("1", (10, y + 4), (11, y + 5), (13, y + 4), (14, y + 5), (17, y + 4), (19, y + 5), (20, y + 4), (21, y + 5), (22, y + 4))
    c.pts("K", (12, y + 1), (13, y + 1), (14, y + 2), (11, y + 2))
    c.rect(15, y - 2, 2, 2, "1"); c.put(17, y - 2, "1")                 # cowlick
    # pink headset: band over the hair, cups at the ears, mic
    c.rect(7, y + 5, 3, 5, "C"); c.rect(22, y + 5, 3, 5, "C"); c.pts("5", (8, y + 6), (23, y + 6))
    c.pts("C", (10, y + 10), (11, y + 11), (12, y + 11)); c.put(13, y + 11, "5")
    # eyes
    for ex in (13, 18):
        if eyes == "open": c.rect(ex, y + 6, 2, 3, "1"); c.put(ex, y + 6, "4")
        elif eyes == "happy": c.pts("1", (ex, y + 8), (ex + 1, y + 7), (ex + 2, y + 8))
        elif eyes == "wide": c.rect(ex, y + 5, 2, 4, "1"); c.put(ex, y + 5, "4")
    if brows == "worried": c.pts("1", (13, y + 4), (19, y + 4))
    # mouth and cheeks
    if mouth == "smile": c.pts("1", (15, y + 10), (16, y + 10)); c.put(14, y + 9, "1"); c.put(17, y + 9, "1")
    elif mouth == "open": c.rect(15, y + 9, 2, 2, "1"); c.put(15, y + 10, "5")
    elif mouth == "wavy": c.pts("1", (14, y + 10), (15, y + 9), (16, y + 10), (17, y + 9))
    c.pts("a", (12, y + 9), (20, y + 9))
    if sweat: c.ell(23, y + 3, 1.2, 1.8, "k")

def front(step=0, bob=0, eyes="open", mouth="smile", brows="normal", arms="down", sweat=False):
    c = C()
    legs_front(c, 22 + bob, step)
    hoodie_front(c, 14 + bob, arms, step)
    head_front(c, 2 + bob, eyes, mouth, brows, sweat)
    return c

def back(step=0, bob=0):
    c = C()
    legs_front(c, 22 + bob, step)
    y = 14 + bob
    c.ell(16, y + 4, 6.5, 5, "o"); c.rect(10, y + 3, 12, 6, "o"); c.rect(10, y + 3, 2, 6, "y")
    c.ell(16, y + 1, 5, 2.5, "W", only=lambda x, yy: yy <= y + 2)       # hood on the back
    sw = [0, 1, 0, -1][step]
    c.rect(7, y + 2 + sw, 3, 6, "o"); c.rect(22, y + 2 - sw, 3, 6, "o")
    y = 2 + bob
    c.ell(16, y + 6, 7, 6.5, "1"); c.pts("K", (12, y + 2), (13, y + 2), (13, y + 3)); c.rect(15, y - 2, 2, 2, "1")
    c.rect(7, y + 5, 3, 5, "C"); c.rect(22, y + 5, 3, 5, "C"); c.rect(10, y, 12, 1, "C"); c.pts("5", (8, y + 6), (23, y + 6))
    return c

def side(step=0, bob=0, eyes="open", lean=False):
    c = C()
    y = 22 + bob
    a, b = [(-2, 2), (0, 0), (2, -2), (0, 0)][step]
    c.rect(14 + a, y, 3, 5, "C"); c.rect(16 + b, y, 3, 5, "1")          # far leg darker
    c.rect(14 + a, y + 5, 5, 2, "4"); c.rect(16 + b, y + 5, 5, 2, "K")
    y = 14 + bob
    c.ell(16, y + 4, 5.5, 5, "o"); c.rect(11, y + 3, 10, 6, "o"); c.rect(11, y + 3, 2, 6, "y")
    c.ell(12, y + 1, 3, 2.5, "W", only=lambda x, yy: x <= 13)           # hood at the back
    sw = [1, 0, -1, 0][step]
    reach = 3 if lean else 0
    c.rect(16 + sw, y + 2, 3 + reach, 6 if not lean else 3, "o"); c.rect(16 + sw + reach, y + 8 if not lean else y + 4, 3, 2, "4")
    c.put(19, y + 1, "y"); c.rect(18, y + 2, 3, 3, "3")                 # the pass, seen from the side
    y = 2 + bob
    c.ell(16, y + 6, 6.5, 6.5, "4"); c.ell(16, y + 6, 6.5, 6.5, "p", only=lambda x, yy: x <= 12)
    c.ell(15, y + 3, 7, 5, "1", only=lambda x, yy: yy <= y + 4 or x <= 12)
    c.pts("1", (19, y + 5), (20, y + 5), (21, y + 4)); c.pts("K", (12, y + 1), (13, y + 1)); c.rect(14, y - 2, 2, 2, "1")
    c.rect(11, y + 5, 3, 5, "C"); c.put(12, y + 6, "5"); c.pts("C", (14, y + 10), (15, y + 11), (16, y + 11)); c.put(17, y + 11, "5")
    if eyes == "open": c.rect(19, y + 6, 2, 3, "1"); c.put(19, y + 6, "4")
    else: c.rect(19, y + 8, 2, 1, "1")
    c.put(22, y + 8, "p"); c.pts("1", (19, y + 10), (20, y + 10)); c.put(18, y + 9, "a")
    return c



def portrait(mood):
    """Head and shoulders, 48 x 48, same design as the map sprite."""
    c = C(48, 48)
    c.ell(24, 47, 19, 11, "o"); c.rect(5, 41, 38, 7, "o"); c.rect(5, 41, 4, 7, "y")          # hoodie
    c.ell(24, 38, 8, 3, "W", only=lambda x, y: y >= 37)                                      # hood collar shadow
    c.rect(20, 34, 8, 4, "4")                                                                 # neck
    c.pts("y", (19, 40), (28, 40), (20, 41), (27, 41)); c.rect(20, 42, 8, 6, "3"); c.rect(22, 44, 4, 1, "K")  # lanyard, pass
    c.ell(24, 22, 13, 13, "4"); c.ell(24, 22, 13, 13, "p", only=lambda x, y: x >= 33)        # face
    c.ell(24, 15, 14, 9, "1", only=lambda x, y: y <= 16)                                       # hair
    for x, y in ((11, 17), (13, 18), (16, 17), (19, 18), (22, 17), (27, 18), (30, 17), (33, 18), (36, 17)): c.rect(x, 16, 2, y - 15, "1")
    c.pts("K", (17, 9), (18, 9), (19, 10), (16, 10)); c.rect(23, 4, 3, 3, "1")              # highlight, cowlick
    c.rect(6, 17, 5, 11, "C"); c.rect(37, 17, 5, 11, "C"); c.rect(8, 20, 2, 2, "5"); c.rect(38, 20, 2, 2, "5")  # headphones
    c.pts("C", (11, 29), (12, 30), (13, 31), (14, 31)); c.rect(15, 31, 2, 1, "5")              # mic
    eyes = {"neutral": "open", "happy": "happy", "surprised": "wide", "worried": "open", "determined": "narrow"}[mood]
    for ex in (16, 29):
        if eyes == "open": c.rect(ex, 20, 3, 5, "1"); c.rect(ex, 20, 1, 2, "4")
        elif eyes == "happy": c.pts("1", (ex, 23), (ex + 1, 22), (ex + 2, 23))
        elif eyes == "wide": c.rect(ex, 19, 4, 6, "1"); c.rect(ex + 1, 20, 1, 2, "4")
        elif eyes == "narrow": c.rect(ex, 22, 3, 2, "1"); c.put(ex, 22, "4")
    if mood == "worried": c.pts("1", (15, 18), (16, 17), (31, 17), (32, 18)); c.ell(39, 14, 1.5, 2.3, "k")
    if mood == "determined": c.pts("1", (15, 18), (16, 18), (17, 19), (30, 19), (31, 18), (32, 18))
    if mood == "surprised": c.rect(16, 16, 3, 1, "1"); c.rect(29, 16, 3, 1, "1")
    mouth = {"neutral": "smile", "happy": "open", "surprised": "o", "worried": "wavy", "determined": "flat"}[mood]
    if mouth == "smile": c.rect(22, 29, 4, 1, "1"); c.put(21, 28, "1"); c.put(26, 28, "1")
    elif mouth == "open": c.ell(24, 29, 3, 2.2, "1"); c.rect(23, 30, 2, 1, "5")
    elif mouth == "o": c.ell(24, 30, 1.6, 2, "1")
    elif mouth == "wavy": [c.put(21 + i, 29 + (i % 2), "1") for i in range(6)]
    elif mouth == "flat": c.rect(21, 29, 6, 1, "1")
    c.rect(13, 26, 3, 1, "a"); c.rect(32, 26, 3, 1, "a")
    return c


def save(name, folder, c):
    with open(os.path.join(GRIDS, name + ".txt"), "w") as f:
        f.write(f"# folder: {folder}\n# moods: normal\n" + "\n".join(c.outlined()) + "\n")


frames = {
 "pax_idle_down_0": front(), "pax_idle_down_1": front(bob=1, eyes="happy" if False else "open"),
 "pax_idle_side_0": side(step=1), "pax_idle_side_1": side(step=1, bob=1, eyes="blink"),
 **{f"pax_walk_side_{i}": side(step=i, bob=i % 2) for i in range(4)},
 **{f"pax_walk_down_{i}": front(step=i, bob=i % 2) for i in range(4)},
 **{f"pax_walk_up_{i}": back(step=i, bob=i % 2) for i in range(4)},
 "pax_interact_side_0": side(step=1), "pax_interact_side_1": side(step=1, lean=True),
 "pax_cheer_down_0": front(eyes="happy", mouth="open", arms="up"), "pax_cheer_down_1": front(eyes="happy", mouth="open", arms="up", bob=-1),
 "pax_worried_down_0": front(eyes="wide", mouth="wavy", brows="worried", sweat=True),
}

for name, c in frames.items():
    save(name, "Characters", c)
for mood in ("neutral", "happy", "surprised", "worried", "determined"):
    save(f"pax_portrait_{mood}", "UI", portrait(mood))
print(f"Pax: {len(frames)} map frames and 5 portraits written")
