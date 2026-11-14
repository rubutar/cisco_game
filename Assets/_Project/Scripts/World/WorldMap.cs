// Person A. The learning map (PRD-v2 4.2, 6.6): Router Plaza in the middle, roads out to the seven Cisco
// stations from checkpoints.json (lit with a cyan line once a station is open, grey while locked), station
// buildings (greyed while locked), the station Bytes beside their buildings, roaming Bytes on lit roads
// only, and Pax. Redraws when station states change. Hidden while a station world is open so its taps
// don't reach the map.
// When Props/town_map.png exists (the hi-fi design's map, from Tools/sprites/import_hifi.py) it is the whole
// town: buildings and station Bytes are baked in, the game's station Bytes are drawn exactly over them,
// a grey road turns to sand (Props/road_<id>) once its station opens, and stations on alert get the ring.
using System.Collections.Generic;
using UnityEngine;

namespace ByteWorld
{
    public class WorldMap : MonoBehaviour
    {
        public static WorldMap Instance { get; private set; }



        // The hi-fi map's station Bytes: the top-left of each 16 x 16 Byte baked into Props/town_map.png
        // (the Twins' second Byte stands beside the first). Stations missing here use the door offset.
        static readonly Dictionary<string, Vector2Int[]> HifiBytes = new()
        {
            ["harbour"] = new[] { new Vector2Int(373, 185) },
            ["gate"] = new[] { new Vector2Int(105, 201) },
            ["vault"] = new[] { new Vector2Int(113, 65), new Vector2Int(129, 65) },
            ["workshop"] = new[] { new Vector2Int(253, 217) },
            ["observatory"] = new[] { new Vector2Int(213, 85) },
            ["switchyard"] = new[] { new Vector2Int(253, 40) },
            ["tower"] = new[] { new Vector2Int(363, 55) },
        };

        // On the hi-fi map the river is a chain of ponds from x 386; Pax stays on the town's side of it
        // (map px, feet: half his width short of the water).
        const float HifiRiverBank = 380f;

        // The drawn town's river (used when there's no hi-fi map) and its bridge.
        static readonly Rect Bridge = new(352, 120, 56, 24);

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
        readonly Dictionary<string, SpriteRenderer> buildings = new();
        readonly Dictionary<string, SpriteRenderer> roads = new();
        readonly Dictionary<string, SpriteRenderer> rings = new();
        readonly Dictionary<string, SpriteRenderer> stationBytes = new();
        Sprite hifiMap;
        readonly List<GameObject> roamers = new();
        GameObject content;
        SpriteRenderer town;

        public JobByte GetJobByte(JobId job) => jobBytes.TryGetValue(job, out var b) ? b : null;

        void Awake()
        {
            Instance = this;
            content = new GameObject("Content");
            content.transform.SetParent(transform, false);
            hifiMap = PixelArt.Load("Props/town_map", () => null);
            town = PixelArt.AddSprite(content.transform, hifiMap != null ? hifiMap : DrawTown(), "Town", -1000);

            foreach (var c in Services.Checkpoints.All)
            {
                if (hifiMap != null)
                {
                    // Grey road turned to sand, and the alert ring, both shown by Refresh.
                    var road = PixelArt.Load($"Props/road_{c.id}", () => null);
                    if (road != null) roads[c.id] = PixelArt.AddSprite(content.transform, road, $"Road {c.id}", -990);
                    var ring = PixelArt.AddSprite(content.transform, PixelArt.Load("Props/alert_ring", () => null), $"Ring {c.id}", -980);
                    ring.transform.localPosition = PixelArt.MapToWorld(c.x - 2, c.y - 4);
                    rings[c.id] = ring;
                }
                else
                {
                    var building = PixelArt.AddSprite(content.transform, null, $"Station {c.id}", 5);
                    // Station art is 64 x 48 with the door at the bottom centre.
                    building.transform.localPosition = PixelArt.MapToWorld(c.x, c.y - 24);
                    buildings[c.id] = building;
                }

                // Spots are the top-left of a station Byte; on the hi-fi map, exactly over the baked one.
                var spots = hifiMap != null && HifiBytes.TryGetValue(c.id, out var baked)
                    ? baked
                    : new[] { new Vector2Int(c.x + 34, c.y - 14), new Vector2Int(c.x + 48, c.y - 14) };

                // The four job stations keep their Byte beside the building (it flashes on alert).
                if (!JobIds.TryParse(c.job, out var job))
                {
                    // Hunt, build and siege stations have a Byte too on the hi-fi map (grey until open).
                    if (hifiMap == null) continue;
                    var b = PixelArt.AddSprite(content.transform, null, $"{c.id} Byte", 10);
                    b.transform.localPosition = PixelArt.MapToWorld(spots[0].x + 8, spots[0].y + 16);
                    stationBytes[c.id] = b;
                    continue;
                }
                if (jobBytes.ContainsKey(job)) continue;
                var go = new GameObject($"{job} Byte");
                go.transform.SetParent(content.transform, false);
                var jobByte = go.AddComponent<JobByte>();
                // 16 x 16 spots are the sprite's top-left; JobByte wants the 12 x 12 one's (2 in, 4 down).
                var jobSpots = System.Array.ConvertAll(job == JobId.Twins ? spots : new[] { spots[0] },
                    p => hifiMap != null ? new Vector2Int(p.x + 2, p.y + 4) : p);
                // On the hi-fi map the Bytes stand still, so they always cover the baked ones.
                jobByte.Init(job, jobSpots, bob: hifiMap == null);
                jobBytes[job] = jobByte;
            }

            var hero = new GameObject("Pax");
            hero.transform.SetParent(content.transform, false);
            hero.AddComponent<HeroController>();

            Refresh();
        }

        /// <summary>True where Pax can't walk: the river, except over the drawn town's bridge.</summary>
        public static bool IsWater(Vector2 p)
        {
            if (Instance != null && Instance.hifiMap != null) return p.x >= HifiRiverBank;
            if (Bridge.Contains(p)) return false;
            foreach (var r in River)
                if ((p - r).sqrMagnitude < 13f * 13f) return true;
            return false;
        }

        void Start()
        {
            WorldSetup.EnsureRaycaster();
            WorldSetup.FitCameraToWindow();
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.CheckpointStatesChanged += Refresh;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.CheckpointStatesChanged -= Refresh;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            var inJob = to is GameState.JobIntro or GameState.JobShift or GameState.JobSuccess or GameState.Snatched;
            content.SetActive(!inJob);
        }

        /// <summary>Redraw roads and buildings for the current station states, and put roamers on lit roads.</summary>
        void Refresh()
        {
            if (town == null) return;
            if (hifiMap == null) town.sprite = DrawTown();
            foreach (var (id, road) in roads) road.enabled = Services.Checkpoints.StateOf(id) != CheckpointState.Locked;
            foreach (var (id, ring) in rings) ring.enabled = Services.Checkpoints.StateOf(id) == CheckpointState.Alert;
            foreach (var (id, b) in stationBytes)
                b.sprite = PixelArt.Load(Services.Checkpoints.StateOf(id) == CheckpointState.Locked ? "Characters/map_byte_dim" : "Characters/map_byte",
                    () => PixelArt.GridSprite(PixelArt.Byte, Palette.GridDim, "Station Byte"));
            foreach (var c in Services.Checkpoints.All)
                if (JobIds.TryParse(c.job, out var j)) GetJobByte(j)?.SetLocked(Services.Checkpoints.StateOf(c.id) == CheckpointState.Locked);
            foreach (var (id, building) in buildings)
            {
                var locked = Services.Checkpoints.StateOf(id) == CheckpointState.Locked;
                building.sprite = PixelArt.Load($"Props/station_{id}{(locked ? "_dim" : "")}", () => PixelArt.Block(64, 48, locked ? Palette.CliffLight : Palette.ByteCream, Palette.Ink, id));
            }

            foreach (var r in roamers) if (r != null) Destroy(r);
            roamers.Clear();
            var lit = new List<Vector2Int[]>();
            foreach (var c in Services.Checkpoints.All)
                if (Services.Checkpoints.StateOf(c.id) != CheckpointState.Locked && c.path != null && c.path.Length > 1)
                    lit.Add(System.Array.ConvertAll(c.path, p => new Vector2Int(p.x, p.y)));
            for (var i = 0; i < lit.Count * 3; i++)
            {
                var go = new GameObject($"Roaming Byte {i + 1}");
                go.transform.SetParent(content.transform, false);
                go.AddComponent<RoamingByte>().Init(lit[i % lit.Count], (i * 0.37f) % 1f, 0.9f + (i % 4) * 0.25f, i % 3 == 0, i);
                roamers.Add(go);
            }
        }

        static Sprite DrawTown()
        {
            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            var random = new System.Random(11);
            var stations = Services.Checkpoints.All;
            var plaza = Services.Data.Checkpoints?.plaza ?? new Data.MapPoint { x = 240, y = 140 };

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

            // Trees, kept clear of the roads and the stations.
            foreach (var t in Trees)
            {
                if (NearRoadOrStation(t, stations, plaza)) continue;
                c.Disc(t.x, t.y, 12, Palette.LeavesDark);
                c.Disc(t.x - 2, t.y - 2, 9, Palette.LeavesLight);
            }

            // Roads to every station: sandy with a cyan line when open, grey when locked.
            foreach (var s in stations)
            {
                if (s.path == null || s.path.Length < 2) continue;
                var points = System.Array.ConvertAll(s.path, p => new Vector2Int(p.x, p.y));
                var open = Services.Checkpoints.StateOf(s.id) != CheckpointState.Locked;
                c.Path(points, 13, open ? Palette.RoadEdge : Palette.CliffDark);
                c.Path(points, 11, open ? Palette.RoadLight : Palette.CliffLight);
                if (!open) continue;
                for (var i = 0; i < points.Length - 1; i++)
                {
                    var a = points[i];
                    var b = points[i + 1];
                    var length = Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y);
                    for (var step = 0; step < length; step += 6)
                    {
                        var x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, (float)step / length));
                        var y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, (float)step / length));
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(plaza.x, plaza.y)) > 26)
                            c.Fill(x, y, a.x == b.x ? 1 : 3, a.x == b.x ? 3 : 1, Palette.NeonCyan);
                    }
                }
            }

            // Bridge over the river.
            c.Fill(354, 124, 52, 17, Palette.Ink);
            c.Fill(356, 126, 48, 13, Palette.Wood);
            for (var x = 358; x < 404; x += 5) c.Fill(x, 126, 1, 13, Palette.WoodDark);

            // Router Plaza: the router with its LEDs in the middle of a round plaza.
            c.Disc(plaza.x, plaza.y, 26, Palette.RoadEdge);
            c.Disc(plaza.x, plaza.y, 25, Palette.RoadLight);
            for (var a = 0; a < 24; a++)
            {
                var t = a / 24f * Mathf.PI * 2f;
                c.Fill(plaza.x + Mathf.RoundToInt(Mathf.Cos(t) * 21), plaza.y + Mathf.RoundToInt(Mathf.Sin(t) * 21), 2, 2, Palette.NeonCyan);
            }
            c.Fill(plaza.x - 14, plaza.y - 14, 28, 22, Palette.Ink);
            c.Fill(plaza.x - 12, plaza.y - 12, 24, 18, Palette.ByteCream);
            c.Fill(plaza.x - 12, plaza.y, 24, 6, Palette.SignalBlue);
            for (var i = 0; i < 5; i++) c.Fill(plaza.x - 10 + i * 4, plaza.y + 2, 2, 2, i % 2 == 0 ? Palette.PacketYellow : Palette.NeonCyan);
            c.Fill(plaza.x - 10, plaza.y - 28, 2, 14, Palette.Ink);
            c.Fill(plaza.x + 8, plaza.y - 28, 2, 14, Palette.Ink);
            c.Fill(plaza.x - 11, plaza.y - 30, 4, 3, Palette.AlertPink);
            c.Fill(plaza.x + 7, plaza.y - 30, 4, 3, Palette.AlertPink);

            return c.ToSprite("Learning Map", new Vector2(0.5f, 0.5f));
        }

        static bool NearRoadOrStation(Vector2Int t, Data.CheckpointData[] stations, Data.MapPoint plaza)
        {
            if (Vector2.Distance(t, new Vector2(plaza.x, plaza.y)) < 40) return true;
            foreach (var s in stations)
            {
                if (Mathf.Abs(t.x - s.x) < 46 && t.y > s.y - 60 && t.y < s.y + 14) return true;
                if (s.path == null) continue;
                for (var i = 0; i < s.path.Length - 1; i++)
                {
                    var a = new Vector2(s.path[i].x, s.path[i].y);
                    var b = new Vector2(s.path[i + 1].x, s.path[i + 1].y);
                    var ab = b - a;
                    var u = ab.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot((Vector2)t - a, ab) / ab.sqrMagnitude) : 0f;
                    if (Vector2.Distance(t, a + ab * u) < 18) return true;
                }
            }
            return false;
        }
    }
}
