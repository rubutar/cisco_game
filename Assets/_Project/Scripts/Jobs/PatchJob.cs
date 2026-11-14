// Stream A. Patch's workshop (A-13, PRD 4.6): crates of new parts arrive by the brick wall. A real crate
// (kind "safe") must be fitted now: it repairs its crack. A crate with "worm": true opens a crack in the wall
// and a Worm crawls toward it at wormSpeed (world units per second); if the Worm arrives before the crate is
// fitted, it gets through. Choosing Later on a real crate lets the Worm in too. A fake crate ("fakeCrate",
// kind "attack", e.g. from "b1lders") must be refused with Later: fitting it lets the attacker in.
// Fitting takes a moment, during which Patch can't open another crate. Day 1: a Worm that arrives is sent
// back with a hint instead of a snatch.
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public class PatchJob : JobShift
    {
        const int SortBase = 500;
        const int WallLeft = 230;
        const int WallRight = 300;
        const float FitSeconds = 1.2f;

        static readonly Vector2Int[] CrateSpots =
        {
            new(120, 120), new(160, 120), new(120, 170), new(160, 170), new(120, 220), new(160, 220),
        };

        // Where cracks open on the wall's right (outside) face, in map pixels.
        static readonly int[] CrackRows = { 60, 120, 180, 230, 90, 150 };

        static readonly string[] WormGrid =
        {
            "........111.",
            ".......12221",
            ".......12521",
            ".111...12221",
            "12221..1221.",
            "1222211221..",
            ".12222221...",
            "..111111....",
        };

        readonly Dictionary<string, GameObject> crates = new();
        readonly Dictionary<string, Crack> cracks = new();
        Transform root;
        Sprite crateSprite;
        Sprite crackSprite;
        Sprite wormSprite;
        int nextSpot;
        int nextCrack;
        float busyUntil;

        protected override JobId Job => JobId.Patch;

        class Crack
        {
            public ItemData Item;
            public SpriteRenderer Mark;
            public Transform Worm;
            public Vector3 WormStart;
            public Vector3 Target;
        }

        protected override void BuildScene()
        {
            WorldSetup.EnsureRaycaster();
            root = new GameObject("Patch's Workshop").transform;
            root.SetParent(transform, false);

            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            c.Fill(0, 0, c.Width, c.Height, Palette.GrassDark);
            c.Fill(0, 0, WallLeft, c.Height, Palette.GrassLight);
            // The brick wall Patch keeps repaired.
            c.Fill(WallLeft, 0, WallRight - WallLeft, c.Height, Palette.ByteCream);
            for (var y = 0; y < c.Height; y += 12)
            {
                c.Fill(WallLeft, y, WallRight - WallLeft, 2, Palette.RoadEdge);
                for (var x = WallLeft + (y / 12 % 2) * 10; x < WallRight; x += 20) c.Fill(x, y, 2, 12, Palette.RoadEdge);
            }
            c.Fill(WallLeft - 2, 0, 2, c.Height, Palette.Ink);
            c.Fill(WallRight, 0, 2, c.Height, Palette.Ink);
            // The workshop's striped awning behind the crates.
            for (var x = 90; x < 200; x += 8)
            {
                c.Fill(x, 84, 4, 10, Palette.AlertPink);
                c.Fill(x + 4, 84, 4, 10, Palette.ByteCream);
            }
            c.Fill(90, 94, 110, 2, Palette.Ink);
            PixelArt.AddSprite(root, c.ToSprite("Workshop", new Vector2(0.5f, 0.5f)), "Workshop", SortBase);

            var patch = PixelArt.AddSprite(root, PixelArt.Load("Characters/patch", () => PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Patch")), "Patch", SortBase + 50);
            patch.transform.localPosition = PixelArt.MapToWorld(50, 170);
            patch.transform.localScale = PixelArt.CharacterScale(patch.sprite);

            crateSprite = PixelArt.Load("Props/crate", CrateSprite);
            crackSprite = PixelArt.Load("Props/crack", CrackSprite);
            wormSprite = PixelArt.Load("Characters/worm", () => PixelArt.GridSprite(WormGrid, Palette.GridNormal, "Worm"));
        }

        protected override void OnShiftStarted()
        {
            busyUntil = 0f;
            nextCrack = 0;
        }

        protected override bool CanInspect(ItemData item) => Time.time >= busyUntil;

        protected override bool BlockingLetsAttackerIn(ItemData item) => item.worm;

        protected override void SpawnItem(ItemData item)
        {
            if (item.type == "crack")
            {
                SpawnWallCrack(item);
                return;
            }
            var spot = CrateSpots[nextSpot % CrateSpots.Length];
            nextSpot++;
            var crate = new GameObject($"Crate {item.id}");
            crate.transform.SetParent(root, false);
            crate.transform.localPosition = PixelArt.MapToWorld(spot.x, -20);
            var body = PixelArt.AddSprite(crate.transform, crateSprite, "Body", SortBase + 100 + nextSpot);
            Tappable.Add(body.gameObject, body, 0.4f).Tapped += () => OpenInspect(item);
            JobVisuals.Label(crate.transform, item.title, new Vector3(0f, -0.75f, 0f), 2.2f, Palette.Ink, SortBase + 101 + nextSpot, 4f);
            crate.AddComponent<Mover>().MoveTo(PixelArt.MapToWorld(spot.x, spot.y), 8f);
            crates[item.id] = crate;

            if (item.worm) OpenCrack(item);
        }

        /// <summary>A crack to repair (type "crack"): tap it on the wall; Fit now fixes it.</summary>
        void SpawnWallCrack(ItemData item)
        {
            var row = CrackRows[nextCrack % CrackRows.Length];
            nextCrack++;
            var go = new GameObject($"Wall crack {item.id}");
            go.transform.SetParent(root, false);
            go.transform.localPosition = PixelArt.MapToWorld((WallLeft + WallRight) / 2, row);
            var mark = PixelArt.AddSprite(go.transform, crackSprite, "Mark", SortBase + 30);
            mark.transform.localScale = new Vector3(2f, 2f, 1f);
            Tappable.Add(mark.gameObject, mark, 0.6f).Tapped += () => OpenInspect(item);
            JobVisuals.Label(go.transform, item.title, new Vector3(0f, -0.75f, 0f), 2.2f, Palette.Ink, SortBase + 31, 4f);
            crates[item.id] = go;
        }

        void OpenCrack(ItemData item)
        {
            var row = CrackRows[nextCrack % CrackRows.Length];
            nextCrack++;
            var mark = PixelArt.AddSprite(root, crackSprite, $"Crack {item.id}", SortBase + 30);
            mark.transform.localPosition = PixelArt.MapToWorld(WallRight - 6, row);

            var wormRenderer = PixelArt.AddSprite(root, wormSprite, $"Worm {item.id}", SortBase + 40);
            wormRenderer.flipX = true; // the grid's head is on the right; Worms crawl left toward the wall
            var worm = wormRenderer.transform;
            var start = (Vector3)PixelArt.MapToWorld(500, row + 4);
            worm.localPosition = start;
            cracks[item.id] = new Crack
            {
                Item = item,
                Mark = mark,
                Worm = worm,
                WormStart = start,
                Target = PixelArt.MapToWorld(WallRight + 8, row + 4),
            };
        }

        protected override void Update()
        {
            base.Update();
            if (!Running) return;

            foreach (var crack in new List<Crack>(cracks.Values))
            {
                var speed = crack.Item.wormSpeed > 0f ? crack.Item.wormSpeed : 0.5f;
                crack.Worm.localPosition = Vector3.MoveTowards(crack.Worm.localPosition, crack.Target, speed * Time.deltaTime);
                if ((crack.Worm.localPosition - crack.Target).sqrMagnitude > 0.0001f) continue;

                if (IsTutorial)
                {
                    // Day 1: the Worm is sent back and Patch points at the crate to fit.
                    crack.Worm.localPosition = crack.WormStart;
                    if (!string.IsNullOrEmpty(crack.Item.hint))
                        Services.Dialogue.ShowHint(Job.ToKey(), crack.Item.hint, ItemPosition(crack.Item), () => { });
                }
                else
                {
                    Fail(string.IsNullOrEmpty(crack.Item.attacker) ? "worm" : crack.Item.attacker);
                    return;
                }
            }
        }

        protected override Vector3 ItemPosition(ItemData item) =>
            crates.TryGetValue(item.id, out var crate) ? crate.transform.position : Vector3.zero;

        protected override void OnItemResolved(ItemData item, bool allowed)
        {
            if (item.type == "crack" && crates.TryGetValue(item.id, out var wallCrack))
            {
                crates.Remove(item.id);
                Destroy(wallCrack);
                if (allowed) busyUntil = Time.time + FitSeconds;
                return;
            }
            if (crates.TryGetValue(item.id, out var crate))
            {
                crates.Remove(item.id);
                // Fitted parts go into the wall; refused crates go back up the road they came down.
                var p = crate.transform.localPosition;
                var target = allowed ? new Vector2(PixelArt.MapToWorld(WallLeft, 0).x, p.y) : new Vector2(p.x, PixelArt.MapToWorld(0, -30).y);
                crate.GetComponent<Mover>().MoveTo(target, 8f, destroyWhenThere: true);
            }
            if (allowed) busyUntil = Time.time + FitSeconds;
            // Any right call clears the crack: a fitted crate repairs it, a refused fake crate never opens it.
            RepairCrack(item);
        }

        void RepairCrack(ItemData item)
        {
            if (!cracks.TryGetValue(item.id, out var crack)) return;
            cracks.Remove(item.id);
            Destroy(crack.Mark.gameObject);
            // The Worm finds the crack fixed and wriggles off.
            var mover = crack.Worm.gameObject.AddComponent<Mover>();
            mover.MoveTo(crack.WormStart, 3f, destroyWhenThere: true);
        }

        protected override void ClearItems()
        {
            foreach (var crate in crates.Values)
                if (crate != null) Destroy(crate);
            foreach (var crack in cracks.Values)
            {
                if (crack.Mark != null) Destroy(crack.Mark.gameObject);
                if (crack.Worm != null) Destroy(crack.Worm.gameObject);
            }
            crates.Clear();
            cracks.Clear();
            nextSpot = 0;
        }

        protected override InspectField[] InspectFields(ItemData item) => new[]
        {
            new InspectField(FieldLabel(0, "From"), item.from),
        };

        static Sprite CrateSprite()
        {
            var c = new PixelCanvas(20, 16);
            c.Fill(0, 0, 20, 16, Palette.Ink);
            c.Fill(1, 1, 18, 14, Palette.PodOrange);
            c.Fill(1, 7, 18, 2, Palette.WoodDark);
            c.Fill(9, 1, 2, 14, Palette.WoodDark);
            return c.ToSprite("Crate", new Vector2(0.5f, 0.5f));
        }

        static Sprite CrackSprite()
        {
            var c = new PixelCanvas(10, 12);
            c.Fill(4, 0, 2, 4, Palette.Ink);
            c.Fill(2, 3, 3, 3, Palette.Ink);
            c.Fill(5, 5, 3, 3, Palette.Ink);
            c.Fill(3, 7, 3, 5, Palette.Ink);
            c.Fill(1, 4, 8, 4, Palette.AlertPink);
            c.Fill(3, 3, 3, 6, Palette.Ink);
            return c.ToSprite("Crack", new Vector2(0.5f, 0.5f));
        }
    }
}
