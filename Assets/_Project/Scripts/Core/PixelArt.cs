// Stream A. Placeholder pixel art drawn in code until person C's PNGs land: sprite grids (PRD 6.6 format),
// a canvas for drawing backgrounds in map pixels, and map <-> world conversion. 16 pixels per unit,
// map pixels have their origin top-left (like the screen-flow reference and build.json).
using UnityEngine;

namespace ByteWorld
{
    public static class PixelArt
    {
        public const int PixelsPerUnit = 16;
        public const int MapWidth = 480;
        public const int MapHeight = 270;
        public const float Pixel = 1f / PixelsPerUnit;

        public static readonly string[] Byte =
        {
            ".....5......",
            ".....1......",
            "...111111...",
            "..14444441..",
            ".1444444441.",
            ".1414444141.",
            ".1444444441.",
            ".1444224441.",
            ".1333333331.",
            "..13333331..",
            "...111111...",
            "...1....1...",
        };

        public static readonly string[] Phisher =
        {
            "......1111..",
            "...111...1..",
            "..12221..1..",
            ".1222221.1..",
            ".1252521.1..",
            ".1222221.5..",
            ".1221221.55.",
            ".1222221....",
            "..122221....",
            "..122221....",
            "..1.1.1.....",
            "............",
        };

        public static readonly string[] AlertMark =
        {
            "11111",
            "15551",
            "15551",
            "11511",
            ".151.",
            ".111.",
            ".151.",
            ".111.",
        };

        static readonly System.Collections.Generic.Dictionary<string, Sprite> Loaded = new();

        /// <summary>
        /// A sprite from Resources/Sprites (PNGs written by Tools/sprites/make_sprites.py), e.g. "Characters/scout".
        /// Falls back to the in-code drawing when the PNG doesn't exist, so a missing file never breaks the game.
        /// </summary>
        public static Sprite Load(string path, System.Func<Sprite> fallback)
        {
            if (Loaded.TryGetValue(path, out var cached) && cached != null) return cached;
            var sprite = Resources.Load<Sprite>("Sprites/" + path);
            if (sprite == null) sprite = fallback();
            Loaded[path] = sprite;
            return sprite;
        }

        /// <summary>
        /// Scale for a character standing in a job scene: the 32 x 32 art at its true size, the small
        /// 12 x 12 fallback drawn twice as big.
        /// </summary>
        public static Vector3 CharacterScale(Sprite sprite) =>
            sprite != null && sprite.rect.height >= 24 ? Vector3.one : new Vector3(2f, 2f, 1f);

        /// <summary>True when the PNG for a path exists (used where a PNG replaces a tint).</summary>
        public static bool HasSprite(string path) => Resources.Load<Sprite>("Sprites/" + path) != null;

        /// <summary>Map pixel (origin top-left) to world position (origin at the centre of the 480 x 270 view).</summary>
        public static Vector2 MapToWorld(float x, float y) =>
            new((x - MapWidth / 2f) * Pixel, (MapHeight / 2f - y) * Pixel);

        public static Vector3 Snap(Vector3 p) =>
            new(Mathf.Round(p.x / Pixel) * Pixel, Mathf.Round(p.y / Pixel) * Pixel, p.z);

        /// <summary>A sprite from a text grid. Pivot at the bottom centre (feet), so sorting by y looks right.</summary>
        public static Sprite GridSprite(string[] grid, Color32[] palette, string name)
        {
            var canvas = new PixelCanvas(grid[0].Length, grid.Length);
            for (var row = 0; row < grid.Length; row++)
                for (var x = 0; x < grid[row].Length; x++)
                {
                    var c = grid[row][x];
                    if (c >= '1' && c <= '5') canvas.Fill(x, row, 1, 1, palette[c - '0']);
                }
            return canvas.ToSprite(name, new Vector2(0.5f, 0f));
        }

        /// <summary>A plain coloured block, pivot centre (placeholder boats, crates, panels).</summary>
        public static Sprite Block(int width, int height, Color32 fill, Color32 outline, string name)
        {
            var canvas = new PixelCanvas(width, height);
            canvas.Fill(0, 0, width, height, outline);
            canvas.Fill(1, 1, width - 2, height - 2, fill);
            return canvas.ToSprite(name, new Vector2(0.5f, 0.5f));
        }

        /// <summary>A sprite renderer child at a sorting order.</summary>
        public static SpriteRenderer AddSprite(Transform parent, Sprite sprite, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }

    /// <summary>Draws into a pixel buffer with top-left coordinates, then becomes a point-filtered sprite.</summary>
    public class PixelCanvas
    {
        public readonly int Width;
        public readonly int Height;
        readonly Color32[] pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            pixels = new Color32[width * height];
        }

        public void Fill(int x, int y, int w, int h, Color32 color)
        {
            for (var py = Mathf.Max(0, y); py < Mathf.Min(Height, y + h); py++)
                for (var px = Mathf.Max(0, x); px < Mathf.Min(Width, x + w); px++)
                    pixels[(Height - 1 - py) * Width + px] = color;
        }

        public void Disc(int cx, int cy, int r, Color32 color)
        {
            for (var dy = -r; dy <= r; dy++)
            {
                var w = Mathf.FloorToInt(Mathf.Sqrt(r * r - dy * dy));
                Fill(cx - w, cy + dy, w * 2 + 1, 1, color);
            }
        }

        /// <summary>Axis-aligned road segments (each from point i to i+1), like the screen-flow cable roads.</summary>
        public void Path(Vector2Int[] points, int width, Color32 color)
        {
            var half = width / 2;
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                Fill(Mathf.Min(a.x, b.x) - half, Mathf.Min(a.y, b.y) - half, Mathf.Abs(b.x - a.x) + width, Mathf.Abs(b.y - a.y) + width, color);
            }
        }

        public Sprite ToSprite(string name, Vector2 pivot)
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = name
            };
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, Width, Height), pivot, PixelArt.PixelsPerUnit);
        }
    }
}
