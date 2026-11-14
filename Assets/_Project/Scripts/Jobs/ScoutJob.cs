// Stream A. Scout's harbour (A-10, PRD 4.6): sea at the bottom, dock on the left, message boats sailing in from
// the right. Tap a boat to read its name, sender and cargo, then let it dock or send it away. Placeholder
// visuals drawn in code until person C's sea, dock and boat art lands.
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public class ScoutJob : JobShift
    {
        const int SortBase = 500;

        // Waiting spots for boats in map pixels (x, waterline y), filled in arrival order.
        static readonly Vector2Int[] Moorings =
        {
            new(290, 205), new(380, 205), new(450, 205), new(290, 262), new(380, 262), new(450, 262),
        };

        readonly Dictionary<string, Boat> boats = new();
        Transform root;
        Sprite hull;
        Sprite hullOdd;
        int nextMooring;

        protected override JobId Job => JobId.Scout;

        protected override void BuildScene()
        {
            WorldSetup.EnsureRaycaster();
            root = new GameObject("Scout's Lookout").transform;
            root.SetParent(transform, false);

            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            c.Fill(0, 0, c.Width, c.Height, Palette.GrassLight);
            c.Fill(0, 150, c.Width, 120, Palette.WaterDeep);
            for (var y = 160; y < 270; y += 14)
                for (var x = (y / 14 % 2) * 20; x < c.Width; x += 40) c.Fill(x, y, 8, 1, Palette.WaterLight);
            // The dock: planks on the left with posts in the water.
            c.Fill(0, 140, 220, 12, Palette.Ink);
            c.Fill(0, 141, 218, 9, Palette.Wood);
            for (var x = 4; x < 216; x += 7) c.Fill(x, 141, 1, 9, Palette.WoodDark);
            for (var x = 20; x < 220; x += 50) c.Fill(x, 152, 4, 10, Palette.WoodDark);
            // The radar mast.
            c.Fill(30, 96, 6, 44, Palette.Ink);
            c.Fill(31, 97, 4, 43, Palette.ByteCream);
            c.Fill(22, 90, 22, 6, Palette.SignalBlue);
            PixelArt.AddSprite(root, c.ToSprite("Harbour", new Vector2(0.5f, 0.5f)), "Harbour", SortBase);

            // Scout stands on the dock, drawn twice as big so the player sees who they're helping.
            var scout = PixelArt.AddSprite(root, PixelArt.Load("Characters/scout", () => PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Scout")), "Scout", SortBase + 50);
            scout.transform.localPosition = PixelArt.MapToWorld(70, 140);
            scout.transform.localScale = PixelArt.CharacterScale(scout.sprite);

            hull = PixelArt.Load("Characters/boat", () => BoatSprite(Palette.SignalBlue));
            hullOdd = hull; // Fakes look the same: the player has to read the name.
        }

        protected override void SpawnItem(ItemData item)
        {
            var spot = Moorings[nextMooring % Moorings.Length];
            nextMooring++;
            var go = new GameObject($"Boat {item.id}");
            go.transform.SetParent(root, false);
            var boat = go.AddComponent<Boat>();
            boat.Init(item.IsAttack ? hullOdd : hull, ShortName(item), SortBase + 100 + nextMooring * 2);
            boat.transform.localPosition = PixelArt.MapToWorld(520, spot.y);
            boat.SailTo(PixelArt.MapToWorld(spot.x, spot.y), 6f);
            boat.Tapped += () => OpenInspect(item);
            boats[item.id] = boat;
        }

        protected override Vector3 ItemPosition(ItemData item) =>
            boats.TryGetValue(item.id, out var boat) ? boat.transform.position : Vector3.zero;

        protected override void OnItemResolved(ItemData item, bool allowed)
        {
            if (!boats.TryGetValue(item.id, out var boat)) return;
            boats.Remove(item.id);
            // Docked boats sail to the pier; sent-away boats sail back out to sea.
            boat.SailTo(allowed ? PixelArt.MapToWorld(180, 166) : PixelArt.MapToWorld(560, boat.MapY), 9f, destroyOnArrival: true);
        }

        protected override void ClearItems()
        {
            foreach (var boat in boats.Values)
                if (boat != null) Destroy(boat.gameObject);
            boats.Clear();
            nextMooring = 0;
        }

        // v2 (PRD-v2 4.5): From, Reply-To, Sender check, Link; the message itself last. Fields without data are left out.
        protected override InspectField[] InspectFields(ItemData item)
        {
            var fields = new System.Collections.Generic.List<InspectField> { new(FieldLabel(0, "From"), item.from) };
            if (!string.IsNullOrEmpty(item.replyTo)) fields.Add(new InspectField(FieldLabel(2, "Reply-To"), item.replyTo));
            if (!string.IsNullOrEmpty(item.senderCheck)) fields.Add(new InspectField(FieldLabel(3, "Sender check"), item.senderCheck));
            if (!string.IsNullOrEmpty(item.link)) fields.Add(new InspectField(FieldLabel(4, "Link"), item.link));
            fields.Add(new InspectField(FieldLabel(1, "Cargo"), item.body));
            // The inspect panel shows up to four fields: the message gives way to the checks.
            if (fields.Count > 4) fields.RemoveAt(fields.Count - 1);
            return fields.ToArray();
        }

        protected override string LogLine(ItemData item, bool allowed) =>
            $"harbour from={item.from} link={(string.IsNullOrEmpty(item.link) ? "-" : item.link)} action={(allowed ? "dock" : "quarantine")}";

        /// <summary>The name painted on the boat: the title without the "Post boat:" prefix.</summary>
        static string ShortName(ItemData item)
        {
            var title = item.title ?? "";
            var colon = title.IndexOf(':');
            return colon >= 0 ? title.Substring(colon + 1).Trim() : title;
        }

        static Sprite BoatSprite(Color32 flag)
        {
            // 32 x 18: mast and flag on top, cream hull with an ink outline below.
            var c = new PixelCanvas(32, 18);
            c.Fill(15, 0, 1, 9, Palette.Ink);
            c.Fill(16, 1, 7, 4, flag);
            c.Fill(2, 9, 28, 8, Palette.Ink);
            c.Fill(3, 10, 26, 6, Palette.ByteCream);
            c.Fill(5, 16, 22, 2, Palette.Ink);
            return c.ToSprite("Boat", new Vector2(0.5f, 0f));
        }

        /// <summary>One boat: sails to a target, carries a name label, taps through to the inspect panel.</summary>
        class Boat : MonoBehaviour
        {
            public event System.Action Tapped;

            Vector3 target;
            float speed;
            bool destroyOnArrival;

            public float MapY => PixelArt.MapHeight / 2f - transform.localPosition.y / PixelArt.Pixel;

            public void Init(Sprite sprite, string label, int sortingOrder)
            {
                var body = PixelArt.AddSprite(transform, sprite, "Hull", sortingOrder);
                Tappable.Add(body.gameObject, body, 0.4f).Tapped += () => Tapped?.Invoke();

                var text = new GameObject("Name").AddComponent<TextMeshPro>();
                text.transform.SetParent(transform, false);
                text.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                text.text = label;
                text.fontSize = 3f;
                text.color = Palette.ByteCream;
                text.alignment = TextAlignmentOptions.Center;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.rectTransform.sizeDelta = new Vector2(6f, 0.6f);
                text.GetComponent<MeshRenderer>().sortingOrder = sortingOrder + 1;
            }

            public void SailTo(Vector2 position, float unitsPerSecond, bool destroyOnArrival = false)
            {
                target = position;
                speed = unitsPerSecond;
                this.destroyOnArrival = destroyOnArrival;
            }

            void Update()
            {
                var p = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
                transform.localPosition = p;
                if (destroyOnArrival && (p - target).sqrMagnitude < 0.0001f) Destroy(gameObject);
            }
        }
    }
}
