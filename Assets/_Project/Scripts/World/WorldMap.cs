// Stream A. The world map (PRD 4.2, screen 03): a placeholder techy town drawn in code with the palette and
// the screen-flow layout (router plaza, cable roads, data river, the four workplaces), and the four job Bytes.
// Person C's 480 x 270 background replaces the drawing later; job positions stay the same.
// Hidden while a job scene is open so its taps don't reach the map.
using System.Collections.Generic;
using UnityEngine;

namespace ByteWorld
{
    public class WorldMap : MonoBehaviour
    {
        public static WorldMap Instance { get; private set; }

        // Job Byte positions in map pixels (top-left of the 12 x 12 sprite), from the screen-flow reference.
        static readonly (JobId job, Vector2Int[] at)[] JobSpots =
        {
            (JobId.Scout, new[] { new Vector2Int(344, 182) }),
            (JobId.Gigi, new[] { new Vector2Int(56, 226) }),
            (JobId.Twins, new[] { new Vector2Int(66, 76), new Vector2Int(90, 76) }),
            (JobId.Patch, new[] { new Vector2Int(178, 210) }),
        };

        static readonly Vector2Int[][] Roads =
        {
            new[] { new Vector2Int(226, 132), new Vector2Int(226, 90), new Vector2Int(140, 90), new Vector2Int(140, 74), new Vector2Int(104, 74) },
            new[] { new Vector2Int(226, 132), new Vector2Int(300, 132), new Vector2Int(440, 132) },
            new[] { new Vector2Int(226, 132), new Vector2Int(226, 160), new Vector2Int(300, 160), new Vector2Int(300, 200), new Vector2Int(340, 200) },
            new[] { new Vector2Int(226, 132), new Vector2Int(190, 132), new Vector2Int(190, 206), new Vector2Int(176, 206) },
            new[] { new Vector2Int(190, 172), new Vector2Int(110, 172), new Vector2Int(110, 222), new Vector2Int(60, 222), new Vector2Int(60, 242) },
        };

        static readonly Vector2Int[] River =
        {
            new(410, 58), new(402, 82), new(388, 104), new(378, 128), new(372, 150), new(382, 174), new(372, 198),
            new(356, 222), new(346, 246), new(342, 272),
        };

        static readonly Vector2Int[] Trees =
        {
            new(14, 14), new(38, 8), new(64, 16), new(92, 8), new(120, 14), new(176, 10), new(204, 16), new(258, 8),
            new(284, 16), new(332, 10), new(10, 44), new(8, 76), new(16, 104), new(8, 160), new(14, 196), new(96, 264),
            new(150, 262), new(214, 262), new(268, 264), new(300, 252), new(470, 240), new(466, 90), new(470, 160),
            new(300, 230), new(150, 108), new(262, 206), new(420, 262),
        };

        readonly Dictionary<JobId, JobByte> jobBytes = new();
        GameObject content;

        public JobByte GetJobByte(JobId job) => jobBytes.TryGetValue(job, out var b) ? b : null;

        void Awake()
        {
            Instance = this;
            content = new GameObject("Content");
            content.transform.SetParent(transform, false);
            PixelArt.AddSprite(content.transform, DrawTown(), "Town", -1000);

            // Ordinary Bytes walking the cable roads (A-07); every third carries a data orb.
            const int roamingCount = 12;
            for (var i = 0; i < roamingCount; i++)
            {
                var go = new GameObject($"Roaming Byte {i + 1}");
                go.transform.SetParent(content.transform, false);
                go.AddComponent<RoamingByte>().Init(Roads[i % Roads.Length], (i * 0.31f) % 1f, 0.9f + (i % 4) * 0.25f, i % 3 == 0, i);
            }

            foreach (var (job, spots) in JobSpots)
            {
                var go = new GameObject($"{job} Byte");
                go.transform.SetParent(content.transform, false);
                var jobByte = go.AddComponent<JobByte>();
                jobByte.Init(job, spots);
                jobBytes[job] = jobByte;
            }
        }

        void Start() => WorldSetup.EnsureRaycaster();

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;

        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            var inJob = to is GameState.JobIntro or GameState.JobShift or GameState.JobSuccess or GameState.Snatched;
            content.SetActive(!inJob);
        }

        static Sprite DrawTown()
        {
            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            var random = new System.Random(11);

            c.Fill(0, 0, c.Width, c.Height, Palette.GrassLight);
            for (var i = 0; i < 900; i++) c.Fill(random.Next(c.Width), random.Next(c.Height), random.Next(2) + 1, 1, Palette.GrassDark);

            // Server cliff (top right) and the data river with its pond.
            c.Fill(356, 0, 124, 44, Palette.CliffDark);
            for (var y = 4; y < 44; y += 8)
                for (var x = 360; x < 480; x += 10) c.Fill(x, y, 8, 6, Palette.CliffLight);
            c.Fill(356, 44, 124, 4, Palette.Ink);
            foreach (var p in River) c.Disc(p.x, p.y, 15, Palette.WaterDeep);
            c.Disc(410, 60, 22, Palette.WaterDeep);
            foreach (var p in River) c.Disc(p.x - 3, p.y, 7, Palette.WaterLight);

            foreach (var t in Trees)
            {
                c.Disc(t.x, t.y, 12, Palette.LeavesDark);
                c.Disc(t.x - 2, t.y - 2, 9, Palette.LeavesLight);
            }

            // Cable roads with cyan centre dashes, and the router plaza.
            foreach (var road in Roads) c.Path(road, 13, Palette.RoadEdge);
            foreach (var road in Roads) c.Path(road, 11, Palette.RoadLight);
            foreach (var road in Roads)
                for (var i = 0; i < road.Length - 1; i++)
                {
                    var a = road[i];
                    var b = road[i + 1];
                    var length = Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y);
                    for (var s = 0; s < length; s += 6)
                    {
                        var x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, (float)s / length));
                        var y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, (float)s / length));
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(226, 132)) > 26)
                            c.Fill(x, y, a.x == b.x ? 1 : 3, a.x == b.x ? 3 : 1, Palette.NeonCyan);
                    }
                }
            c.Disc(226, 132, 26, Palette.RoadEdge);
            c.Disc(226, 132, 25, Palette.RoadLight);
            c.Fill(212, 118, 28, 22, Palette.Ink);
            c.Fill(214, 120, 24, 18, Palette.ByteCream);
            c.Fill(214, 132, 24, 6, Palette.SignalBlue);

            // Bridge over the river and Scout's radar dock.
            c.Fill(354, 124, 52, 17, Palette.Ink);
            c.Fill(356, 126, 48, 13, Palette.Wood);
            c.Fill(336, 194, 34, 11, Palette.Ink);
            c.Fill(338, 196, 30, 7, Palette.Wood);
            c.Fill(330, 178, 8, 22, Palette.Ink);
            c.Fill(331, 179, 6, 20, Palette.ByteCream);

            // The Twins' vault (top left).
            c.Fill(62, 40, 44, 34, Palette.Ink);
            c.Fill(64, 42, 40, 30, Palette.ByteCream);
            c.Fill(64, 42, 40, 6, Palette.SignalBlue);
            c.Disc(84, 60, 8, Palette.Ink);
            c.Disc(84, 60, 6, Palette.ByteCream);
            c.Disc(84, 60, 2, Palette.AlertPink);

            // Patch's workshop (bottom centre) with its striped awning and crates.
            c.Fill(130, 190, 44, 30, Palette.Ink);
            c.Fill(132, 192, 40, 26, Palette.SignalBlue);
            for (var x = 128; x < 178; x += 6)
            {
                c.Fill(x, 184, 3, 8, Palette.AlertPink);
                c.Fill(x + 3, 184, 3, 8, Palette.ByteCream);
            }
            c.Fill(145, 198, 14, 14, Palette.ByteCream);
            foreach (var (x, y) in new[] { (120, 206), (120, 214), (114, 212) })
            {
                c.Fill(x, y, 8, 7, Palette.Ink);
                c.Fill(x + 1, y + 1, 6, 5, Palette.PodOrange);
            }

            // Gigi's wall (bottom left) with doors 80, 443, 23 (shut, pink) and 8080.
            c.Fill(0, 244, 124, 14, Palette.Ink);
            c.Fill(0, 246, 122, 10, Palette.CliffLight);
            foreach (var (x, shut) in new[] { (10, false), (30, false), (52, true), (74, false) })
            {
                c.Fill(x, 238, 18, 20, Palette.Ink);
                c.Fill(x + 1, 239, 16, 18, shut ? Palette.AlertPink : Palette.SignalBlue);
            }

            return c.ToSprite("Techy Town", new Vector2(0.5f, 0.5f));
        }
    }
}
