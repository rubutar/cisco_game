// SANDBOX. Draws a 480 x 270 techy-town style field (grass, cable roads, router plaza, trees) into one
// texture and spawns roaming Bytes on it. Coordinates below are map pixels with the origin top-left,
// the same as the screen-flow reference and build.json.
using UnityEngine;

namespace ByteWorld.Sandbox
{
    // ExecuteAlways: the field is also drawn in edit mode, so the scene shows it without pressing Play.
    // Generated objects are DontSave, so nothing generated ends up in the scene file.
    [ExecuteAlways]
    public class SandboxField : MonoBehaviour
    {
        public const int Width = 480;
        public const int Height = 270;

        [SerializeField, Range(1, 14)] int characterCount = 1;
        [SerializeField] int seed = 11;

        Color32[] pixels;
        System.Random random;
        GameObject generated;

        static readonly Vector2Int Plaza = new(226, 132);

        static readonly Vector2Int[] Trees =
        {
            new(14, 14), new(64, 16), new(120, 14), new(176, 10), new(284, 16), new(332, 10), new(10, 76),
            new(16, 104), new(8, 196), new(96, 254), new(150, 252), new(300, 236), new(420, 250), new(466, 90),
            new(470, 170), new(380, 40), new(60, 230), new(330, 200), new(110, 190), new(400, 196),
        };

        public static Rect WorldBounds => new(-Width / 2f / SpriteGrids.PixelsPerUnit, -Height / 2f / SpriteGrids.PixelsPerUnit,
            (float)Width / SpriteGrids.PixelsPerUnit, (float)Height / SpriteGrids.PixelsPerUnit);

        void OnEnable() => Build();

        void OnDisable() => Clear();

#if UNITY_EDITOR
        // Rebuild when Character Count or Seed changes in the Inspector (edit mode).
        void OnValidate()
        {
            if (Application.isPlaying) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && isActiveAndEnabled) Build();
            };
        }
#endif

        void Build()
        {
            Clear();
            generated = new GameObject("Generated") { hideFlags = HideFlags.DontSave };
            generated.transform.SetParent(transform, false);

            CreateGround();
            var byteSprite = SpriteGrids.Create(SpriteGrids.Byte, Palette.GridNormal, "Byte");
            for (var i = 0; i < characterCount; i++)
            {
                var go = new GameObject($"Byte {i + 1}") { hideFlags = HideFlags.DontSave };
                go.transform.SetParent(generated.transform, false);
                // In edit mode the Bytes stand still (RoamingCharacter only updates in Play mode).
                go.AddComponent<RoamingCharacter>().Init(byteSprite, Plaza.x + i * 7, Plaza.y, seed + i);
            }
        }

        void Clear()
        {
            if (generated == null) return;
            if (Application.isPlaying) Destroy(generated);
            else DestroyImmediate(generated);
            generated = null;
        }

        void CreateGround()
        {
            random = new System.Random(seed);
            pixels = new Color32[Width * Height];

            Fill(0, 0, Width, Height, Palette.GrassLight);
            for (var i = 0; i < 900; i++)
                Fill(random.Next(Width), random.Next(Height), random.Next(2) + 1, 1, Palette.GrassDark);

            // Cable roads: sandy edge, sandy road, glowing cyan dashes down the middle.
            Fill(0, Plaza.y - 6, Width, 13, Palette.RoadEdge);
            Fill(Plaza.x - 6, 0, 13, Height, Palette.RoadEdge);
            Fill(0, Plaza.y - 5, Width, 11, Palette.RoadLight);
            Fill(Plaza.x - 5, 0, 11, Height, Palette.RoadLight);
            Disc(Plaza.x, Plaza.y, 26, Palette.RoadEdge);
            Disc(Plaza.x, Plaza.y, 25, Palette.RoadLight);
            for (var x = 0; x < Width; x += 6)
                if (Mathf.Abs(x - Plaza.x) > 26) Fill(x, Plaza.y, 3, 1, Palette.NeonCyan);
            for (var y = 0; y < Height; y += 6)
                if (Mathf.Abs(y - Plaza.y) > 26) Fill(Plaza.x, y, 1, 3, Palette.NeonCyan);
            for (var a = 0; a < 24; a++)
            {
                var t = a / 24f * Mathf.PI * 2f;
                Fill(Plaza.x + Mathf.RoundToInt(Mathf.Cos(t) * 21), Plaza.y + Mathf.RoundToInt(Mathf.Sin(t) * 21), 2, 2, Palette.NeonCyan);
            }

            foreach (var tree in Trees)
            {
                Disc(tree.x, tree.y, 12, Palette.LeavesDark);
                Disc(tree.x - 2, tree.y - 2, 9, Palette.LeavesLight);
                Fill(tree.x + 3, tree.y - 5, 1, 1, Palette.NeonCyan); // tiny fibre light
            }

            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "Sandbox Field",
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply();

            var ground = new GameObject("Ground") { hideFlags = HideFlags.DontSave }.AddComponent<SpriteRenderer>();
            ground.transform.SetParent(generated.transform, false);
            ground.sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f), SpriteGrids.PixelsPerUnit);
            ground.sortingOrder = -1000;
        }

        /// <summary>Map pixel (origin top-left) to world position (origin at the centre).</summary>
        public static Vector2 MapToWorld(float x, float y) =>
            new((x - Width / 2f) / SpriteGrids.PixelsPerUnit, (Height / 2f - y) / SpriteGrids.PixelsPerUnit);

        void Fill(int x, int y, int w, int h, Color32 color)
        {
            for (var py = Mathf.Max(0, y); py < Mathf.Min(Height, y + h); py++)
                for (var px = Mathf.Max(0, x); px < Mathf.Min(Width, x + w); px++)
                    pixels[(Height - 1 - py) * Width + px] = color;
        }

        void Disc(int cx, int cy, int r, Color32 color)
        {
            for (var dy = -r; dy <= r; dy++)
            {
                var w = Mathf.FloorToInt(Mathf.Sqrt(r * r - dy * dy));
                Fill(cx - w, cy + dy, w * 2 + 1, 1, color);
            }
        }
    }
}
