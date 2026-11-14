#!/usr/bin/env python3
"""Writes the intro story illustrations' HTML (Tools/sprites/splash/intro_<n>.html), one per line of
dialogue.json "intro", in order. render.sh turns them into Resources/Sprites/UI/intro_<n>.png (960 x 540,
shown full screen behind the line; the bottom stays dark for the text). All art is the game's own sprites
and the hi-fi map at whole-number scales.
  1  your message goes into the router, and inside it is a town
  2  the Bytes carry everyone's messages door to door
  3  strangers sneak in: a phisher, a look-alike visitor, a worm
  4  a visitor arrives with an empty Cisco toolkit: Pax
  5  Gigi meets Pax; two stations are on alert"""
import os

HERE = os.path.dirname(os.path.abspath(__file__))
SPRITES = "../../../Assets/_Project/Resources/Sprites/"
HIFI = "../hifi/"
W, H = 960, 540

HEAD = """<!doctype html><html><head><meta charset="utf-8"><style>
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
    # The Bytes at work around Router Plaza, packets on their way.
    s = mapview("town_clean.png", 160, 70, 4)
    bytes_ = [("gigi", 150, 250), ("scout", 330, 280), ("uno", 560, 270), ("patch", 740, 245)]
    for i, (name, x, y) in enumerate(bytes_):
        s += sprite(f"Characters/{name}", x, y, 3, 32, 32)
        s += packet(x + 36, y - 36, 22, "#ffd23f" if i % 2 == 0 else "#ff9a3c")
    for x, y in ((470, 120), (520, 150), (420, 200)):
        s += packet(x, y, 14)
    return s


def scene3():
    # Trouble: dusk over the town, a phisher, a look-alike visitor and a worm creep in from the edge.
    s = mapview("town_clean.png", 120, 90, 4)
    s += '<div style="inset:0;background:rgba(29,27,51,.5)"></div>'
    s += '<div style="inset:0;background:radial-gradient(ellipse 70% 80% at 30% 50%,rgba(255,93,115,0) 45%,rgba(255,93,115,.32) 100%)"></div>'
    s += sprite("Characters/byte_worried", 150, 200, 4, 32, 32)
    s += sprite("Characters/phisher", 560, 90, 4, 32, 32)
    s += sprite("Characters/ram", 720, 190, 4, 32, 32)
    s += sprite("Characters/map_worm", 470, 270, 5, 16, 16)
    s += rings(214, 264, "#ff5d73", [(120, .8), (165, .4)])
    return s


def scene4():
    # Pax arrives at Router Plaza, Visitor Pass on, toolkit still empty.
    s = mapview("town_clean.png", 160, 60, 4)
    s += '<div style="inset:0;background:radial-gradient(circle at 480px 300px,rgba(71,230,255,.45) 0,rgba(71,230,255,0) 220px)"></div>'
    s += sprite("Characters/pax_idle_down_0", 384, 150, 6, 32, 32)
    s += sprite("UI/visitor_pass", 640, 130, 3, 48, 64, "transform:rotate(8deg)")
    tools = ["umbrella", "firewall", "duo", "endpoint", "splunk", "catalyst", "thousandeyes"]
    for i, t in enumerate(tools):
        s += sprite(f"UI/badge_{t}_dim", 208 + i * 80, 30, 4, 16, 16, "filter:grayscale(1) opacity(.55)")
    return s


def scene5():
    # Gigi meets Pax; the Harbour and the Gate are on alert.
    s = mapview("town_clean.png", 0, 0, 2)
    for cx, cy in ((90 * 2, 211 * 2), (381 * 2, 196 * 2)):
        s += f'<img src="{SPRITES}Props/alert_ring.png" style="left:{cx - 72}px;top:{cy - 72}px;width:144px;height:144px">'
    s += sprite("Characters/gigi", 230, 170, 6, 32, 32)
    s += sprite("Characters/pax_idle_down_0", 530, 150, 6, 32, 32, "transform:scaleX(-1)")
    return s


def main():
    for i, scene in enumerate((scene1, scene2, scene3, scene4, scene5), start=1):
        with open(os.path.join(HERE, f"intro_{i}.html"), "w") as f:
            f.write(HEAD + scene() + TAIL)
    print("intro scenes written")


if __name__ == "__main__":
    main()
