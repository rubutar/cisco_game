#!/usr/bin/env python3
"""Writes the intro story illustrations' HTML (Tools/sprites/splash/intro_<n>.html), one per line of
dialogue.json "intro", in order. render.sh turns them into Resources/Sprites/UI/intro_<n>.png (960 x 540,
shown full screen behind the line; the bottom stays dark for the text). All art is the game's own sprites
and the hi-fi map at whole-number scales.
  1  your message goes into the router, and inside it is a town of Bytes
  2  strangers sneak in, and Pax arrives with an empty Cisco toolkit
  3  how to play: WASD or the arrow keys to walk, Enter at a station on alert"""
import os

HERE = os.path.dirname(os.path.abspath(__file__))
SPRITES = "../../../Assets/_Project/Resources/Sprites/"
HIFI = "../hifi/"
W, H = 960, 540

HEAD = """<!doctype html><html><head><meta charset="utf-8">
<link href="https://fonts.googleapis.com/css2?family=Pixelify+Sans:wght@700&display=swap" rel="stylesheet"><style>
body{margin:0;background:#1d1b33}
.f{position:relative;width:960px;height:540px;overflow:hidden;background:#1d1b33}
.f *{position:absolute;image-rendering:pixelated}
.text-shade{left:0;right:0;bottom:0;height:200px;background:linear-gradient(0deg,rgba(29,27,51,.95) 0%,rgba(29,27,51,.75) 45%,rgba(29,27,51,0) 100%)}
.grid{inset:0;background-image:linear-gradient(rgba(71,230,255,.08) 2px,transparent 2px),linear-gradient(90deg,rgba(71,230,255,.08) 2px,transparent 2px);background-size:32px 32px}
.shadow{filter:drop-shadow(0 6px 0 rgba(29,27,51,.45))}
</style></head><body><div class="f">"""
TAIL = '<div class="text-shade"></div></div></body></html>'


def sprite(path, x, y, scale, w, h, extra=""):
    return (f'<img src="{SPRITES}{path}.png" class="shadow" style="left:{x}px;top:{y}px;'
            f'width:{w * scale}px;height:{h * scale}px;{extra}">')


def mapview(src, mx, my, scale, x=0, y=0, w=W, h=H, extra=""):
    """A window onto a 480 x 270 map: map pixel (mx, my) at the top-left, drawn at scale."""
    return (f'<div style="left:{x}px;top:{y}px;width:{w}px;height:{h}px;background:url({src}) '
            f'{-mx * scale}px {-my * scale}px / {480 * scale}px {270 * scale}px no-repeat;{extra}"></div>')


def packet(x, y, size=16, colour="#ffd23f"):
    return f'<div style="left:{x}px;top:{y}px;width:{size}px;height:{size}px;background:{colour};border:4px solid #1d1b33"></div>'


def envelope(x, y):
    return (f'<div style="left:{x}px;top:{y}px;width:56px;height:40px;background:#fff4d6;border:5px solid #1d1b33"></div>'
            f'<div style="left:{x + 5}px;top:{y + 2}px;width:46px;height:20px;border-bottom:5px solid #1d1b33;'
            f'clip-path:polygon(0 0,50% 100%,100% 0,100% 30%,50% 130%,0 30%);background:#1d1b33"></div>')


def rings(cx, cy, colour, radii, dash="12 10"):
    out = f'<svg style="left:0;top:0" width="{W}" height="{H}">'
    for r, o in radii:
        out += f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="{colour}" stroke-width="5" stroke-dasharray="{dash}" opacity="{o}"/>'
    return out + "</svg>"


def scene1():
    # Inside the device: a phone sends a message into the router; the zoom shows the town inside it.
    s = '<div class="grid" style="position:absolute"></div>'
    s += sprite("Props/phone", 70, 70, 5, 24, 40)
    s += ('<svg style="left:0;top:0" width="960" height="540"><path d="M190 170 L330 170" stroke="#47e6ff" stroke-width="6" '
          'stroke-dasharray="14 10" fill="none"/><path d="M455 170 L560 120 M455 210 L560 330" stroke="#47e6ff" '
          'stroke-width="4" stroke-dasharray="8 8" fill="none" opacity=".7"/></svg>')
    s += envelope(220, 148)
    s += mapview("town_clean.png", 225, 93, 4, 335, 100, 120, 172)            # the router
    s += rings(395, 170, "#47e6ff", [(110, .5), (150, .25)])
    s += mapview(HIFI + "map.png", 150, 70, 2, 560, 60, 360, 300,
                 "border-radius:50%;border:8px solid #47e6ff;box-shadow:0 0 40px rgba(71,230,255,.6)")
    return s


def scene2():
    # Trouble and help: strangers creep in on the dark side; Pax arrives on the bright side with an
    # empty toolkit and the Visitor Pass.
    s = mapview("town_clean.png", 120, 90, 4)
    s += '<div style="inset:0;background:linear-gradient(90deg,rgba(29,27,51,0) 30%,rgba(29,27,51,.55) 62%)"></div>'
    s += '<div style="inset:0;background:radial-gradient(ellipse 45% 70% at 100% 45%,rgba(255,93,115,.32) 0,rgba(255,93,115,0) 100%)"></div>'
    s += '<div style="inset:0;background:radial-gradient(circle at 245px 260px,rgba(71,230,255,.45) 0,rgba(71,230,255,0) 190px)"></div>'
    s += sprite("Characters/pax_idle_down_0", 150, 110, 6, 32, 32)
    s += sprite("UI/visitor_pass", 345, 150, 2, 48, 64, "transform:rotate(8deg)")
    tools = ["umbrella", "firewall", "duo", "endpoint", "splunk", "catalyst", "thousandeyes"]
    s += '<div style="left:40px;top:20px;width:412px;height:76px;background:rgba(29,27,51,.85);border:4px solid #ffd23f;border-radius:38px"></div>'
    for i, t in enumerate(tools):
        s += sprite(f"UI/badge_{t}_dim", 66 + i * 54, 34, 3, 16, 16, "filter:grayscale(1) opacity(.7)")
    s += sprite("Characters/byte_worried", 460, 220, 3, 32, 32)
    s += sprite("Characters/phisher", 610, 70, 4, 32, 32)
    s += sprite("Characters/ram", 760, 190, 4, 32, 32)
    s += sprite("Characters/map_worm", 600, 275, 5, 16, 16)
    return s


def key(label, x, y, w=52, size=30):
    return (f'<div style="left:{x}px;top:{y}px;width:{w}px;height:56px;box-sizing:border-box;background:#fff4d6;'
            f'border:4px solid #1d1b33;border-bottom-width:10px;border-radius:10px;color:#1d1b33;'
            f"font-family:'Pixelify Sans',monospace;font-size:{size}px;line-height:42px;text-align:center\">{label}</div>")


def scene3():
    # How to play: walk with WASD or the arrow keys, press Enter at a station on alert.
    s = mapview("town_clean.png", 40, 120, 3)
    s += f'<img src="{SPRITES}Props/alert_ring.png" style="left:{150 - 108}px;top:{273 - 108 - 20}px;width:216px;height:216px">'
    s += sprite("Characters/pax_walk_side_1", 300, 120, 5, 32, 32, "transform:scaleX(-1)")
    s += ('<svg style="left:0;top:0" width="960" height="540"><path d="M290 240 L215 240" stroke="#47e6ff" stroke-width="8" '
          'stroke-dasharray="14 10" fill="none"/><path d="M226 226 L206 240 L226 254" stroke="#47e6ff" stroke-width="8" fill="none"/></svg>')
    s += ('<div style="left:536px;top:36px;width:404px;height:290px;background:rgba(29,27,51,.88);border:4px solid #47e6ff;'
          'border-radius:24px"></div>')
    # WASD, a slash, then the arrow keys in the same shape; Enter under them.
    for label, x, y in (("W", 614, 64), ("A", 556, 128), ("S", 614, 128), ("D", 672, 128)):
        s += key(label, x, y)
    s += ("<div style=\"left:728px;top:128px;width:32px;color:#47e6ff;font-family:'Pixelify Sans',monospace;font-size:40px;"
          'line-height:56px;text-align:center">/</div>')
    for label, x, y in (("&#9650;", 822, 64), ("&#9664;", 764, 128), ("&#9660;", 822, 128), ("&#9654;", 880, 128)):
        s += key(label, x, y, 52, 24)
    s += key("Enter", 648, 222, 180, 34)
    s += ('<svg style="left:0;top:0" width="960" height="540"><path d="M640 256 C 520 300, 360 330, 230 300" stroke="#ffd23f" '
          'stroke-width="6" stroke-dasharray="12 10" fill="none"/></svg>')
    return s


def main():
    for i, scene in enumerate((scene1, scene2, scene3), start=1):
        with open(os.path.join(HERE, f"intro_{i}.html"), "w") as f:
            f.write(HEAD + scene() + TAIL)
    print("intro scenes written")


if __name__ == "__main__":
    main()
