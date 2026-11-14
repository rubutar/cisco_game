// Stream A. The Twins' vault (A-12, PRD 4.6): a vault door with Uno (checks the password) and Due (checks the
// owner's phone). Visitors queue to open someone's vault, each saying a code. Tap a visitor: Due's phone shows
// what the owner's phone says, and the inspect panel shows the password check, the phone code (marked
// expired, or "not asked" when the owner never asked) and the visitor's code. Open vault / Keep it shut.
// Value words come from dialogue.json valueLabels: [right, wrong, expired, not asked].
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public class TwinsJob : JobShift
    {
        const int SortBase = 500;

        static readonly Vector2Int[] QueueSpots =
        {
            new(340, 130), new(400, 130), new(455, 130), new(340, 215), new(400, 215), new(455, 215),
        };

        readonly Dictionary<string, GameObject> visitors = new();
        Transform root;
        Sprite visitorSprite;
        TextMeshPro phone;
        int nextSpot;

        protected override JobId Job => JobId.Twins;

        protected override void BuildScene()
        {
            WorldSetup.EnsureRaycaster();
            root = new GameObject("The Twins' Vault").transform;
            root.SetParent(transform, false);

            var c = new PixelCanvas(PixelArt.MapWidth, PixelArt.MapHeight);
            c.Fill(0, 0, c.Width, c.Height, Palette.CliffDark);
            for (var y = 6; y < c.Height; y += 12)
                for (var x = (y / 12 % 2) * 10; x < c.Width; x += 20) c.Fill(x, y, 16, 8, Palette.CliffLight);
            // The vault door with its dial.
            c.Fill(196, 40, 88, 170, Palette.Ink);
            c.Fill(199, 43, 82, 164, Palette.ByteCream);
            c.Fill(199, 43, 82, 10, Palette.SignalBlue);
            c.Disc(240, 125, 22, Palette.Ink);
            c.Disc(240, 125, 19, Palette.ByteCream);
            c.Fill(239, 107, 2, 36, Palette.Ink);
            c.Fill(222, 124, 36, 2, Palette.Ink);
            c.Disc(240, 125, 4, Palette.AlertPink);
            // Floor in front of the vault.
            c.Fill(0, 210, c.Width, 60, Palette.RoadEdge);
            c.Fill(0, 212, c.Width, 58, Palette.RoadLight);
            PixelArt.AddSprite(root, c.ToSprite("Vault", new Vector2(0.5f, 0.5f)), "Vault", SortBase);

            var byteSprite = PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Twin");
            var uno = PixelArt.AddSprite(root, byteSprite, "Uno", SortBase + 50);
            uno.transform.localPosition = PixelArt.MapToWorld(150, 120);
            uno.transform.localScale = new Vector3(2f, 2f, 1f);
            var due = PixelArt.AddSprite(root, byteSprite, "Due", SortBase + 50);
            due.transform.localPosition = PixelArt.MapToWorld(150, 210);
            due.transform.localScale = new Vector3(2f, 2f, 1f);

            // Due's phone: shows the owner's code for the visitor being checked.
            var handset = PixelArt.AddSprite(root, PixelArt.Block(44, 18, Palette.Ink, Palette.Ink, "Phone"), "Phone", SortBase + 60);
            handset.transform.localPosition = PixelArt.MapToWorld(100, 190);
            phone = JobVisuals.Label(handset.transform, "", Vector3.zero, 2.8f, Palette.NeonCyan, SortBase + 61, 3f);

            visitorSprite = PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Visitor");
        }

        protected override void OnShiftStarted() => phone.text = "";

        protected override void SpawnItem(ItemData item)
        {
            var spot = QueueSpots[nextSpot % QueueSpots.Length];
            nextSpot++;
            var go = new GameObject($"Visitor {item.id}");
            go.transform.SetParent(root, false);
            go.transform.localPosition = PixelArt.MapToWorld(500, spot.y);
            var body = PixelArt.AddSprite(go.transform, visitorSprite, "Body", SortBase + 100 + nextSpot);
            Tappable.Add(body.gameObject, body, 0.4f).Tapped += () =>
            {
                phone.text = PhoneShows(item);
                OpenInspect(item);
            };
            JobVisuals.Label(go.transform, $"\"{item.visitorCode}\"", new Vector3(0f, -0.3f, 0f), 2.5f, Palette.ByteCream, SortBase + 101 + nextSpot);
            go.AddComponent<Mover>().MoveTo(PixelArt.MapToWorld(spot.x, spot.y), 6f);
            visitors[item.id] = go;
        }

        protected override Vector3 ItemPosition(ItemData item) =>
            visitors.TryGetValue(item.id, out var v) ? v.transform.position : Vector3.zero;

        protected override void OnItemResolved(ItemData item, bool allowed)
        {
            phone.text = "";
            if (!visitors.TryGetValue(item.id, out var visitor)) return;
            visitors.Remove(item.id);
            // Opened: the visitor goes into the vault. Kept shut: they leave.
            var target = allowed ? PixelArt.MapToWorld(240, 205) : PixelArt.MapToWorld(520, PixelArt.MapHeight / 2f - visitor.transform.localPosition.y / PixelArt.Pixel);
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
            new InspectField(FieldLabel(0, "Password"), Word(item.passwordCorrect ? 0 : 1)),
            new InspectField(FieldLabel(1, "Phone code"), PhoneShows(item)),
            new InspectField(FieldLabel(2, "Visitor's code"), item.visitorCode),
        };

        /// <summary>What the owner's phone says: the code (marked expired), or "not asked" if the owner never asked.</summary>
        string PhoneShows(ItemData item)
        {
            if (!item.ownerRequested) return Word(3);
            return item.codeExpired ? $"{item.phoneCode} ({Word(2)})" : item.phoneCode;
        }

        string Word(int index) =>
            Lines?.valueLabels != null && index < Lines.valueLabels.Length ? Lines.valueLabels[index] : "";
    }
}
