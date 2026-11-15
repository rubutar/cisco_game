// Stream A. Gigi's gate (A-11, PRD 4.6): the town wall with numbered doors, Gigi's list pinned beside it,
// and visitors queueing outside asking for a door. Let in / Turn away. Doors on the list as open are blue,
// closed ones pink. A ruleChange can open or close doors mid-shift (Gigi announces it). Placeholder visuals
// drawn in code until person C's gate art lands; the list board shows only data (door numbers and names).
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public class GigiJob : JobShift
    {
        const int SortBase = 500;
        const int WallX = 250;

        // The world's four doors, in the order they sit on the wall; doors only named in data are added after.
        static readonly int[] WorldDoors = { 80, 443, 23, 8080 };

        // Queue spots for visitors outside the wall, in map pixels (feet).
        static readonly Vector2Int[] QueueSpots =
        {
            new(310, 110), new(370, 110), new(430, 110), new(310, 200), new(370, 200), new(430, 200),
        };

        readonly Dictionary<string, GameObject> visitors = new();
        readonly Dictionary<int, SpriteRenderer> doorSprites = new();
        readonly Dictionary<int, int> doorY = new();
        readonly HashSet<int> open = new();
        readonly HashSet<int> closed = new();
        Transform root;
        Transform board;
        Sprite visitorSprite;
        int nextSpot;
        bool ruleChangeDone;

        protected override JobId Job => JobId.Gigi;

        protected override void BuildScene()
        {
            WorldSetup.EnsureRaycaster();
            root = new GameObject("Gigi's Gate").transform;
            root.SetParent(transform, false);

            // Inside the town (left) is grass; outside (right) is the road the visitors arrive on.
            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            c.Fill(0, 0, c.Width, c.Height, Palette.GrassLight);
            c.Fill(WallX, 0, c.Width - WallX, c.Height, Palette.RoadLight);
            for (var x = WallX + 6; x < c.Width; x += 9) c.Fill(x, 150, 4, 1, Palette.NeonCyan);
            c.Fill(WallX - 6, 0, 16, c.Height, Palette.Ink);
            c.Fill(WallX - 5, 0, 14, c.Height, Palette.CliffLight);
            for (var y = 0; y < c.Height; y += 8) c.Fill(WallX - 5, y, 14, 2, Palette.CliffDark);
            PixelArt.AddSprite(root, c.ToSprite("Gate", new Vector2(0.5f, 0.5f)), "Gate", SortBase);

            var gigi = PixelArt.AddSprite(root, PixelArt.Load("Characters/gigi", () => PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Gigi")), "Gigi", SortBase + 50);
            gigi.transform.localPosition = PixelArt.MapToWorld(200, 170);
            gigi.transform.localScale = PixelArt.CharacterScale(gigi.sprite);

            visitorSprite = PixelArt.Load("Characters/visitor", () => PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Visitor"));

            // Doors: the world's four plus any other door the shift mentions.
            var doors = new List<int>(WorldDoors);
            foreach (var d in Shift.rules?.open ?? new int[0]) if (!doors.Contains(d)) doors.Add(d);
            foreach (var d in Shift.rules?.closed ?? new int[0]) if (!doors.Contains(d)) doors.Add(d);
            foreach (var item in Shift.items ?? new ItemData[0]) if (item.door > 0 && !doors.Contains(item.door)) doors.Add(item.door);
            var step = Mathf.Min(52, 240 / Mathf.Max(1, doors.Count));
            for (var i = 0; i < doors.Count; i++)
            {
                var y = 30 + i * step;
                doorY[doors[i]] = y;
                var door = PixelArt.AddSprite(root, PixelArt.Block(20, 30, Palette.ByteCream, Palette.Ink, $"Door {doors[i]}"), $"Door {doors[i]}", SortBase + 10);
                door.transform.localPosition = PixelArt.MapToWorld(WallX + 2, y + 15);
                doorSprites[doors[i]] = door;
                // The 32 x 32 door art has a number plate at the top; the plain block takes the number in its middle.
                var plate = PixelArt.HasSprite("Props/door_open") ? new Vector3(0f, 15 * PixelArt.Pixel, 0f) : Vector3.zero;
                JobVisuals.Label(door.transform, doors[i].ToString(), plate, 2.2f, Palette.Ink, SortBase + 11);
            }
        }

        // The board starts below the HUD's top bar (about 28 map pixels tall), so its door chips stay visible.
        const float BoardTop = 34f;

        protected override void OnShiftStarted()
        {
            // A restart begins from the shift's original list.
            open.Clear();
            closed.Clear();
            foreach (var d in Shift.rules?.open ?? new int[0]) open.Add(d);
            foreach (var d in Shift.rules?.closed ?? new int[0]) closed.Add(d);
            ruleChangeDone = false;
            Refresh();
        }

        protected override void Update()
        {
            base.Update();
            var change = Shift.ruleChange;
            // v2 triggers the change when a given item arrives (rulesChangeAtItem, see SpawnItem); v1 by time.
            if (!Running || ruleChangeDone || change == null || Shift.rulesChangeAtItem > 0 || change.atSeconds <= 0f || Elapsed < change.atSeconds) return;
            ApplyRuleChange();
        }

        void ApplyRuleChange()
        {
            var change = Shift.ruleChange;
            if (ruleChangeDone || change == null) return;

            ruleChangeDone = true;
            foreach (var d in change.close ?? new int[0]) { open.Remove(d); closed.Add(d); }
            foreach (var d in change.open ?? new int[0]) { closed.Remove(d); open.Add(d); }
            Refresh();
            // The Webex post: the v2 line if there is one, else the v1 announcement.
            var line = !string.IsNullOrEmpty(Shift.rulesChangeLine) ? Shift.rulesChangeLine : change.announce;
            if (!string.IsNullOrEmpty(line)) Services.Dialogue.Say(Job.ToKey(), line);
        }

        void Refresh()
        {
            // With the door PNGs each state has its own sprite; without them the cream block is tinted.
            var pngDoors = PixelArt.HasSprite("Props/door_open");
            foreach (var (door, sprite) in doorSprites)
            {
                var state = open.Contains(door) ? "door_open" : closed.Contains(door) ? "door_shut" : "door_unlisted";
                if (pngDoors)
                {
                    sprite.sprite = PixelArt.Load($"Props/{state}", () => sprite.sprite);
                    sprite.color = Color.white;
                }
                else
                {
                    sprite.color = open.Contains(door) ? (Color)Palette.SignalBlue : closed.Contains(door) ? (Color)Palette.AlertPink : Color.white;
                }
            }
            DrawBoard();
        }

        void DrawBoard()
        {
            if (board != null) Destroy(board.gameObject);
            board = new GameObject("Gigi's list").transform;
            board.SetParent(root, false);

            // Door chips (blue = open, pink = closed) and the names of known visitors: data only, no fixed words.
            var known = Shift.rules?.known ?? new string[0];
            var doors = new List<int>(open);
            doors.AddRange(closed);
            var height = 16 + 12 * (Mathf.CeilToInt(doors.Count / 3f) + known.Length);
            var panel = PixelArt.AddSprite(board, PixelArt.Block(100, height, Palette.ByteCream, Palette.Ink, "List"), "Panel", SortBase + 20);
            panel.transform.localPosition = PixelArt.MapToWorld(60, BoardTop + height / 2f);

            for (var i = 0; i < doors.Count; i++)
            {
                var chip = PixelArt.AddSprite(board, PixelArt.Block(26, 10, open.Contains(doors[i]) ? Palette.SignalBlue : Palette.AlertPink, Palette.Ink, "Chip"), "Chip", SortBase + 21);
                chip.transform.localPosition = PixelArt.MapToWorld(26 + (i % 3) * 32, BoardTop + 12 + (i / 3) * 12);
                JobVisuals.Label(chip.transform, doors[i].ToString(), Vector3.zero, 2.2f, Palette.ByteCream, SortBase + 22);
            }
            var top = BoardTop + 12 + Mathf.CeilToInt(doors.Count / 3f) * 12;
            for (var i = 0; i < known.Length; i++)
                JobVisuals.Label(board, known[i], PixelArt.MapToWorld(60, top + i * 12), 2.5f, Palette.Ink, SortBase + 22);
        }

        protected override void SpawnItem(ItemData item)
        {
            var spot = QueueSpots[nextSpot % QueueSpots.Length];
            nextSpot++;
            if (Shift.rulesChangeAtItem > 0 && nextSpot == Shift.rulesChangeAtItem) ApplyRuleChange();
            var go = new GameObject($"Visitor {item.id}");
            go.transform.SetParent(root, false);
            go.transform.localPosition = PixelArt.MapToWorld(500, spot.y);
            var body = PixelArt.AddSprite(go.transform, visitorSprite, "Body", SortBase + 100 + nextSpot);
            Tappable.Add(body.gameObject, body, 0.4f).Tapped += () => OpenInspect(item);
            JobVisuals.Label(go.transform, $"{item.title} → {item.door}", new Vector3(0f, -0.3f, 0f), 2.5f, Palette.Ink, SortBase + 101 + nextSpot);
            go.AddComponent<Mover>().MoveTo(PixelArt.MapToWorld(spot.x, spot.y), 6f);
            visitors[item.id] = go;
        }

        protected override Vector3 ItemPosition(ItemData item) =>
            visitors.TryGetValue(item.id, out var v) ? v.transform.position : Vector3.zero;

        protected override void OnItemResolved(ItemData item, bool allowed)
        {
            if (!visitors.TryGetValue(item.id, out var visitor)) return;
            visitors.Remove(item.id);
            // Let in: walk to the door they asked for. Turned away: walk back down the road.
            var target = allowed && doorY.TryGetValue(item.door, out var y)
                ? PixelArt.MapToWorld(WallX + 2, y + 28)
                : PixelArt.MapToWorld(520, PixelArt.MapHeight / 2f - visitor.transform.localPosition.y / PixelArt.Pixel);
            visitor.GetComponent<Mover>().MoveTo(target, 8f, destroyWhenThere: true);
        }

        protected override void ClearItems()
        {
            foreach (var v in visitors.Values)
                if (v != null) Destroy(v);
            visitors.Clear();
            nextSpot = 0;
        }

        protected override InspectField[] InspectFields(ItemData item) => new[]
        {
            new InspectField(FieldLabel(0, "Door"), item.door.ToString()),
            new InspectField(FieldLabel(1, "Says"), item.says),
        };

        protected override string LogLine(ItemData item, bool allowed) =>
            $"gate src={item.title} door={item.door} action={(allowed ? "allow" : "deny")}";
    }
}
