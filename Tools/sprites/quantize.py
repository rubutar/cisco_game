#!/usr/bin/env python3
"""
Quantize AI-generated sprites to the Techy Town palette and target pixel grid.
Usage:
    python3 Tools/sprites/quantize.py input.png output.png --size 16 16
    python3 Tools/sprites/quantize.py input.png output.png --size 16 16 --byte-only
"""

import argparse
import math
from PIL import Image

TECHY_TOWN_PALETTE = {
    "ink": (0x1D, 0x1B, 0x33),
    "byte_cream": (0xFF, 0xF4, 0xD6),
    "signal_blue": (0x4A, 0x5F, 0xC1),
    "neon_cyan": (0x47, 0xE6, 0xFF),
    "alert_pink": (0xFF, 0x5D, 0x73),
    "packet_yellow": (0xFF, 0xD2, 0x3F),
    "pod_orange": (0xFF, 0x9A, 0x3C),
    "grass_light": (0x84, 0xD8, 0x6F),
    "grass_dark": (0x5F, 0xBD, 0x5A),
    "leaves_dark": (0x2F, 0x9A, 0x55),
    "leaves_light": (0x5F, 0xC9, 0x6E),
    "road_light": (0xF3, 0xE3, 0xB8),
    "road_edge": (0xD9, 0xC4, 0x8F),
    "water_deep": (0x3F, 0x8C, 0xF0),
    "water_light": (0x7C, 0xC4, 0xFF),
    "server_cliff_dark": (0x3B, 0x3F, 0x6E),
    "server_cliff_light": (0x57, 0x5D, 0x9A),
    "wood_light": (0xC9, 0x8F, 0x4E),
    "wood_dark": (0x9C, 0x6A, 0x35),
}

# Restricted 5-color Byte palette (indices 1-5 in Section 6.6)
BYTE_PALETTE = {
    "ink": (0x1D, 0x1B, 0x33),         # 1: outline
    "signal_blue": (0x4A, 0x5F, 0xC1), # 2: eyes and feet
    "neon_cyan": (0x47, 0xE6, 0xFF),   # 3: belly
    "byte_cream": (0xFF, 0xF4, 0xD6),  # 4: body
    "alert_pink": (0xFF, 0x5D, 0x73),  # 5: antenna tip
}


def nearest_color(rgb, palette_colors):
    r, g, b = rgb
    best_color = palette_colors[0]
    min_dist = float("inf")
    for pr, pg, pb in palette_colors:
        # Weighted perceptual Euclidean distance
        dist = (
            2 * (r - pr) ** 2
            + 4 * (g - pg) ** 2
            + 3 * (b - pb) ** 2
        )
        if dist < min_dist:
            min_dist = dist
            best_color = (pr, pg, pb)
    return best_color


def trim_transparency(img, alpha_threshold=128):
    """Crop transparent margins so the sprite fills the target grid cleanly."""
    alpha = img.getchannel("A")
    mask = alpha.point(lambda a: 255 if a >= alpha_threshold else 0)
    bbox = mask.getbbox()
    return img.crop(bbox) if bbox else img


def quantize_sprite(
    input_path,
    output_path,
    width=16,
    height=16,
    byte_only=False,
    alpha_threshold=128,
    auto_trim=True,
):
    img = Image.open(input_path).convert("RGBA")

    if auto_trim:
        img = trim_transparency(img, alpha_threshold)


    img = img.resize((width, height), Image.Resampling.NEAREST)

    palette_dict = BYTE_PALETTE if byte_only else TECHY_TOWN_PALETTE
    palette_colors = list(palette_dict.values())

    pixels = img.load()
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a < alpha_threshold:
                pixels[x, y] = (0, 0, 0, 0)
            else:
                nr, ng, nb = nearest_color((r, g, b), palette_colors)
                pixels[x, y] = (nr, ng, nb, 255)

    img.save(output_path, "PNG")
    print(f"Saved {width}x{height} quantized sprite -> {output_path}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Quantize sprites to the Techy Town palette.")
    parser.add_argument("input", help="Path to source PNG")
    parser.add_argument("output", help="Path to output PNG")
    parser.add_argument("--size", type=int, nargs=2, default=[16, 16], metavar=("W", "H"),
                        help="Target width and height (default: 16 16)")
    parser.add_argument("--byte-only", action="store_true",
                        help="Restrict snapping to the 5-color Byte palette (1-5)")
    parser.add_argument("--no-trim", action="store_true",
                        help="Do not trim transparent padding before resizing")
    parser.add_argument("--alpha-threshold", type=int, default=128,
                        help="Alpha cutoff (0-255) for hard pixel transparency")

    args = parser.parse_args()
    quantize_sprite(
        args.input,
        args.output,
        width=args.size[0],
        height=args.size[1],
        byte_only=args.byte_only,
        alpha_threshold=args.alpha_threshold,
        auto_trim=not args.no_trim,
    )