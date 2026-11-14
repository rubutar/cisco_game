// Person A. The Splunk Trace (PRD-v2 4.6, A-06): implements IHuntScreen on GameRoot.prefab. Go in at the
// Splunk Observatory calls Begin. The player taps the query chips in order to build the search, the results
// table drops in (with extra rows for this session's mistakes), and tapping the target row pins it. The
// Field Card from hunt.json follows; Back to Byte World closes the screen and reports the pinned row.
// Hints (A-12): a wrong chip makes the right next chip pulse, and after two misses the search bar shows the
// next piece faintly; two wrong rows make the target row pulse. Hints use the chips' own labels only.
// The UI is built in code (no prefab to keep in sync). Text comes from hunt.json, checkpoints.json and
// dialogue.json; the search syntax and column names are Splunk's own.
using System;
using System.Collections;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class HuntScreen : MonoBehaviour, IHuntScreen
    {
        public static bool IsOpen { get; private set; }

        readonly List<ItemResolvedInfo> mistakes = new();
        Canvas canvas;
        RectTransform root;
        TextMeshProUGUI query;
        RectTransform chipRow;
        RectTransform table;
        RectTransform card;
        Action<string> onPinned;
        HuntFile hunt;
        int nextOrder;
        string pinned;
        int chipMisses;
        int rowMisses;
        readonly Dictionary<int, RectTransform> chipsByOrder = new();
        Image targetRow;

        void Awake()
        {
            Services.Register<IHuntScreen>(this);
            GameEvents.ItemResolved += OnItemResolved;
        }

        void OnDestroy()
        {
            Services.Unregister<IHuntScreen>(this);
            GameEvents.ItemResolved -= OnItemResolved;
            IsOpen = false;
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (!info.correct) mistakes.Add(info);
        }

        // ---------- IHuntScreen ----------

        public void Begin(Action<string> pinnedCallback)
        {
            onPinned = pinnedCallback;
            hunt = Services.Data.Hunt;
            if (hunt?.chips == null || hunt.chips.Length == 0)
            {
                Debug.LogWarning("[Hunt] hunt.json has no chips; pinning straight away");
                Finish(TargetRow());
                return;
            }
            Build();
            IsOpen = true;
        }

        // ---------- Screen ----------

        void Build()
        {
            if (canvas != null) Destroy(canvas.gameObject);
            nextOrder = 1;
            pinned = null;
            chipMisses = 0;
            built = "";
            rowMisses = 0;
            chipsByOrder.Clear();
            targetRow = null;

            var go = new GameObject("Splunk Trace", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root = (RectTransform)go.transform;

            var dim = Panel(root, "Dim", Palette.Ink, 0.88f);
            Stretch(dim);

            var station = Services.Checkpoints.Get(CheckpointManager.Instance?.StationForJob("hunt") ?? "observatory");
            var header = Text(root, station != null ? $"{station.tool} · {station.station}" : "Splunk", 56, Palette.PacketYellow, FontStyles.Bold);
            Place(header.rectTransform, 0.5f, 1f, new Vector2(0, -70), new Vector2(1700, 80));
            var prompt = Text(root, hunt.prompt, 38, Palette.ByteCream, FontStyles.Normal);
            Place(prompt.rectTransform, 0.5f, 1f, new Vector2(0, -145), new Vector2(1700, 60));

            // The search bar, filled chip by chip.
            var bar = Panel(root, "Search bar", Palette.CliffDark, 1f);
            Place(bar, 0.5f, 1f, new Vector2(0, -250), new Vector2(1700, 110));
            query = Text(bar, "", 36, Palette.NeonCyan, FontStyles.Normal);
            query.alignment = TextAlignmentOptions.MidlineLeft;
            Place(query.rectTransform, 0.5f, 0.5f, Vector2.zero, new Vector2(1640, 100));

            // The chips, shuffled with the distractors.
            chipRow = new GameObject("Chips", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            chipRow.SetParent(root, false);
            Place(chipRow, 0.5f, 1f, new Vector2(0, -385), new Vector2(1800, 100));
            var layout = chipRow.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            var labels = new List<(string label, int order)>();
            foreach (var c in hunt.chips) labels.Add((c.label, c.order));
            foreach (var d in hunt.distractors ?? new string[0]) labels.Add((d, 0));
            var rng = new System.Random(7);
            for (var i = labels.Count - 1; i > 0; i--) { var j = rng.Next(i + 1); (labels[i], labels[j]) = (labels[j], labels[i]); }
            foreach (var (label, order) in labels) Chip(label, order);

            table = new GameObject("Results", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            table.SetParent(root, false);
            Place(table, 0.5f, 1f, new Vector2(0, -470), new Vector2(1200, 500), pivotY: 1f);
            var vertical = table.GetComponent<VerticalLayoutGroup>();
            vertical.spacing = 10;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;
        }

        void Chip(string label, int order)
        {
            var chip = Panel(chipRow, "Chip", Palette.ByteCream, 1f);
            chip.sizeDelta = new Vector2(Mathf.Max(220, label.Length * 21 + 50), 84);
            var text = Text(chip, label, 34, Palette.Ink, FontStyles.Normal);
            Stretch(text.rectTransform);
            var button = chip.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => OnChip(chip, label, order));
            if (order > 0) chipsByOrder[order] = chip;
        }

        void OnChip(RectTransform chip, string label, int order)
        {
            if (order != nextOrder)
            {
                StartCoroutine(Flash(chip.GetComponent<Image>(), Palette.AlertPink, Palette.ByteCream));
                chipMisses++;
                if (chipsByOrder.TryGetValue(nextOrder, out var next)) StartCoroutine(Pulse(next.GetComponent<Image>(), Palette.ByteCream));
                if (chipMisses >= 2) ShowQuery(NextLabel());
                return;
            }
            built = string.IsNullOrEmpty(built) ? label : $"{built} {label}";
            ShowQuery(null);
            chip.gameObject.SetActive(false);
            nextOrder++;
            if (nextOrder > hunt.chips.Length) StartCoroutine(ShowResults());
        }

        string built = "";

        /// <summary>The query so far, plus the next piece faintly when the player needs a hint.</summary>
        void ShowQuery(string ghost)
        {
            query.text = string.IsNullOrEmpty(ghost) ? built
                : string.IsNullOrEmpty(built) ? $"<alpha=#55>{ghost}" : $"{built} <alpha=#55>{ghost}";
        }

        string NextLabel()
        {
            foreach (var c in hunt.chips) if (c.order == nextOrder) return c.label;
            return "";
        }

        IEnumerator ShowResults()
        {
            foreach (Transform child in chipRow) child.gameObject.SetActive(false);
            Row(new[] { "src_ip", "dest_port", "count" }, Palette.CliffLight, Palette.ByteCream, null);

            var rows = new List<HuntRow>(hunt.rows ?? new HuntRow[0]);
            // Mistakes from this session show up in the logs too (PRD-v2 4.6).
            foreach (var m in mistakes)
                rows.Add(new HuntRow { srcIp = m.title, destPort = m.job == JobId.Gigi ? 23 : 443, count = 1 });
            rows.Sort((a, b) => b.count.CompareTo(a.count));

            foreach (var row in rows)
            {
                yield return new WaitForSecondsRealtime(0.18f);
                var r = row;
                var image = Row(new[] { r.srcIp, r.destPort.ToString(), r.count.ToString() }, Palette.ByteCream, Palette.Ink, img => OnRow(img, r));
                if (r.target) targetRow = image;
            }
        }

        Image Row(string[] cells, Color fill, Color ink, Action<Image> onTap)
        {
            var row = Panel(table, "Row", fill, 1f);
            row.sizeDelta = new Vector2(1200, 70);
            var horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.childControlWidth = true;
            horizontal.childForceExpandWidth = true;
            horizontal.padding = new RectOffset(24, 24, 0, 0);
            foreach (var cell in cells)
            {
                var text = Text(row, cell, 36, ink, FontStyles.Normal);
                text.alignment = TextAlignmentOptions.MidlineLeft;
            }
            var image = row.GetComponent<Image>();
            if (onTap == null) return image;
            row.gameObject.AddComponent<Button>().onClick.AddListener(() => onTap(image));
            return image;
        }

        void OnRow(Image image, HuntRow row)
        {
            if (pinned != null) return;
            if (!row.target)
            {
                StartCoroutine(Flash(image, Palette.AlertPink, Palette.ByteCream));
                if (++rowMisses >= 2 && targetRow != null) StartCoroutine(Pulse(targetRow, Palette.ByteCream));
                return;
            }
            pinned = $"{row.srcIp}:{row.destPort}";
            image.color = Palette.PacketYellow;
            ShowFieldCard();
        }

        void ShowFieldCard()
        {
            var fc = hunt.fieldCard;
            card = Panel(root, "Field Card", Palette.ByteCream, 1f);
            Place(card, 0.5f, 0f, new Vector2(0, 60), new Vector2(1300, 300), pivotY: 0f);
            card.gameObject.AddComponent<Outline>().effectColor = Palette.Ink;
            var term = Text(card, fc != null ? $"{fc.term} · {fc.cisco}" : "", 44, Palette.Ink, FontStyles.Bold);
            Place(term.rectTransform, 0.5f, 1f, new Vector2(0, -50), new Vector2(1220, 60));
            var body = Text(card, fc != null ? $"{fc.text}\n<size=34>{fc.concept} · {fc.role}</size>" : "", 36, Palette.Ink, FontStyles.Normal);
            Place(body.rectTransform, 0.5f, 1f, new Vector2(0, -140), new Vector2(1220, 120));

            var button = Panel(card, "Back", Palette.PacketYellow, 1f);
            Place(button, 1f, 0f, new Vector2(-30, 30), new Vector2(460, 80), pivotX: 1f, pivotY: 0f);
            var label = Text(button, Services.Data.Dialogue?.ui?.backToWorld ?? "", 36, Palette.Ink, FontStyles.Bold);
            Stretch(label.rectTransform);
            button.gameObject.AddComponent<Button>().onClick.AddListener(() => Finish(pinned));
        }

        void Finish(string row)
        {
            if (canvas != null) Destroy(canvas.gameObject);
            canvas = null;
            IsOpen = false;
            var callback = onPinned;
            onPinned = null;
            callback?.Invoke(row);
        }

        string TargetRow()
        {
            foreach (var r in Services.Data.Hunt?.rows ?? new HuntRow[0])
                if (r.target) return $"{r.srcIp}:{r.destPort}";
            return "";
        }

        // ---------- uGUI helpers ----------

        static RectTransform Panel(Transform parent, string name, Color color, float alpha)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            color.a = alpha;
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, FontStyles style)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void Place(RectTransform rect, float anchorX, float anchorY, Vector2 position, Vector2 size, float pivotX = 0.5f, float pivotY = 0.5f)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(anchorX, anchorY);
            rect.pivot = new Vector2(pivotX, pivotY);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>Three yellow blinks: "this one".</summary>
        IEnumerator Pulse(Image image, Color back)
        {
            for (var i = 0; i < 3 && image != null; i++)
            {
                image.color = Palette.PacketYellow;
                yield return new WaitForSecondsRealtime(0.22f);
                if (image == null || pinned != null) yield break;
                image.color = back;
                yield return new WaitForSecondsRealtime(0.18f);
            }
        }

        static IEnumerator Flash(Image image, Color flash, Color back)
        {
            image.color = flash;
            yield return new WaitForSecondsRealtime(0.25f);
            if (image != null) image.color = back;
        }
    }
}
