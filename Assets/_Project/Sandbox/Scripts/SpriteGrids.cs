// SANDBOX. Turns text grids (PRD 6.6 format: '.' transparent, 1-5 palette index) into sprites at runtime,
// so we can try ideas before person C's PNG pipeline exists.
using UnityEngine;

namespace ByteWorld.Sandbox
{
    public static class SpriteGrids
    {
        public const int PixelsPerUnit = 16;

        // The Byte from the screen-flow reference.
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

        public static Sprite Create(string[] grid, Color32[] palette, string name)
        {
            var height = grid.Length;
            var width = grid[0].Length;
            var pixels = new Color32[width * height];
            for (var row = 0; row < height; row++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = grid[row][x];
                    var index = c >= '1' && c <= '5' ? c - '0' : 0;
                    // Grid rows run top to bottom; texture rows run bottom to top.
                    pixels[(height - 1 - row) * width + x] = palette[index];
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = name,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply();
            // Pivot at the feet so sorting by y looks right.
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), PixelsPerUnit);
        }
    }
}
