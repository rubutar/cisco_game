// Stream B. The Splunk log (PRD-v2 4.2, B-09): every decision becomes one log line, e.g.
// "14:02 gate src=Luca door=23 action=deny", from ItemResolved.logLine (Person A's JobShift); wrong calls in pink.
//   * Inside a station: one slim line along the very bottom edge with the newest decision. The bottom edge is
//     empty in every station, so it never covers anything that moves. Tap it to open the history.
//   * On the map: a small "splunk>" pill next to the Town Space chat. Tap it to read the session's log.
//   * History: the last few decisions from every station so far, newest at the bottom, in full.
// Hidden while a dialogue bubble or the Splunk Trace is open.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class SplunkTicker : MonoBehaviour
    {
        const int HistorySize = 8;
        const float HistoryWidth = 960f;

        struct Entry
        {
            public string line;
            public bool correct;
        }

        [SerializeField] RectTransform root;

        readonly List<Entry> entries = new();
        readonly List<TMP_Text> historyLines = new();
        RectTransform strip, pill, history;
        TMP_Text stripText, pillText, historyTitle;
        Outline stripBorder;
        bool historyOpen;
        bool dirty;
        float newAt = -100f;

        void Awake()
        {
            Build();
            strip.gameObject.SetActive(false);
            pill.gameObject.SetActive(false);
            history.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.ItemResolved += OnItemResolved;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.ItemResolved -= OnItemResolved;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title) entries.Clear();
            historyOpen = false; // a new screen starts with the history folded away
            dirty = true;
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (string.IsNullOrEmpty(info.logLine)) return;
            entries.Add(new Entry { line = info.logLine, correct = info.correct });
            while (entries.Count > HistorySize) entries.RemoveAt(0);
            newAt = Time.unscaledTime;
            dirty = true;
        }

        void LateUpdate()
        {
            var state = Services.Flow.State;
            var free = entries.Count > 0 && !Services.Dialogue.IsShowing && !HuntScreen.IsOpen;
            var inShift = free && state == GameState.JobShift;
            var onMap = free && Stations.IsMapState(state) && !CheckpointMap.CardShowing && !MaterialsPanel.AnyOpen;
            SetActive(strip, inShift);
            SetActive(pill, onMap);
            SetActive(history, historyOpen && (inShift || onMap));

            // The pill sits just right of the Town Space chat, whatever its size.
            if (onMap) pill.anchoredPosition = new Vector2(TownSpace.RightEdge + 12, 24);
            if (inShift)
            {
                var fresh = Time.unscaledTime - newAt < 1.2f && Mathf.Repeat(Time.unscaledTime, 0.4f) < 0.2f;
                stripBorder.effectColor = fresh ? (Color)Palette.PacketYellow : UiStyle.Ink;
            }
            // The history opens just above whichever opened it.
            history.anchoredPosition = new Vector2(inShift ? 24 : pill.anchoredPosition.x, inShift ? 24 + strip.rect.height + 10 : 24 + pill.rect.height + 10);

            if (!dirty) return;
            dirty = false;
            Redraw();
        }

        static void SetActive(RectTransform rect, bool on)
        {
            if (rect.gameObject.activeSelf != on) rect.gameObject.SetActive(on);
        }

        void Redraw()
        {
            var prefix = UiStrings.Ui.tickerPrefix ?? "";
            var tag = string.IsNullOrEmpty(prefix) ? "" : $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PacketYellow)}>{prefix}</color> ";
            pillText.text = $"{prefix} {(historyOpen ? "▼" : "▲")}";
            historyTitle.text = prefix;
            if (entries.Count > 0)
            {
                var last = entries[^1];
                stripText.text = $"{tag}{last.line}  <color=#{ColorUtility.ToHtmlStringRGB(Palette.ByteCream)}>{(historyOpen ? "▼" : "▲")}</color>";
                stripText.color = last.correct ? (Color)Palette.NeonCyan : UiStyle.Danger;
            }
            for (var i = 0; i < historyLines.Count; i++)
            {
                var has = i < entries.Count;
                historyLines[i].gameObject.SetActive(has);
                if (!has) continue;
                historyLines[i].text = entries[i].line;
                historyLines[i].color = entries[i].correct ? (Color)Palette.NeonCyan : UiStyle.Danger;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(pill);
            LayoutRebuilder.ForceRebuildLayoutImmediate(history);
            LayoutRebuilder.ForceRebuildLayoutImmediate(history);
        }

        void ToggleHistory()
        {
            historyOpen = !historyOpen;
            dirty = true;
        }

        // ---------- Build (runtime) ----------

        void Build()
        {
            // Slim strip along the bottom edge, inside a station.
            strip = UiKit.Rect("LogStrip", root);
            strip.anchorMin = new Vector2(0, 0);
            strip.anchorMax = new Vector2(1, 0);
            strip.pivot = new Vector2(0.5f, 0);
            strip.offsetMin = new Vector2(24, 12);
            strip.offsetMax = new Vector2(-24, 64);
            var stripFill = UiKit.Image(strip.gameObject, UiStyle.Dim(0.88f), true);
            stripBorder = UiKit.Border(strip.gameObject, 3, UiStyle.Ink);
            AddTap(strip.gameObject, stripFill);
            stripText = UiKit.Text("Line", strip, UiStyle.Small, Palette.NeonCyan, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            UiKit.Stretch(stripText.rectTransform);
            stripText.rectTransform.offsetMin = new Vector2(16, 0);
            stripText.rectTransform.offsetMax = new Vector2(-16, 0);
            stripText.textWrappingMode = TextWrappingModes.NoWrap;
            stripText.overflowMode = TextOverflowModes.Ellipsis;
            stripText.richText = true;

            // "splunk> ▲" pill on the map.
            pill = UiKit.Rect("LogPill", root);
            UiKit.Place(pill, Vector2.zero, new Vector2(0, 24), new Vector2(0, 0));
            var pillFill = UiKit.Image(pill.gameObject, UiStyle.Dim(0.92f), true);
            UiKit.Border(pill.gameObject, 4, UiStyle.Ink);
            UiKit.Row(pill.gameObject, 0, new RectOffset(18, 18, 12, 14));
            UiKit.Fit(pill.gameObject, true, true);
            AddTap(pill.gameObject, pillFill);
            pillText = UiKit.Text("Text", pill, UiStyle.Small, Palette.PacketYellow, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            pillText.textWrappingMode = TextWrappingModes.NoWrap;

            // History panel: opens above the strip or the pill; tap it to fold it away.
            history = UiKit.Rect("LogHistory", root);
            UiKit.Place(history, Vector2.zero, new Vector2(24, 100), new Vector2(HistoryWidth, 0));
            var historyFill = UiKit.Image(history.gameObject, UiStyle.Dim(0.95f), true);
            UiKit.Border(history.gameObject, 4, UiStyle.Ink);
            UiKit.Column(history.gameObject, 6, new RectOffset(20, 20, 14, 16));
            UiKit.Fit(history.gameObject, false, true);
            AddTap(history.gameObject, historyFill);
            historyTitle = UiKit.Text("Title", history, UiStyle.Small, Palette.PacketYellow, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            for (var i = 0; i < HistorySize; i++)
            {
                var line = UiKit.Text($"Line{i}", history, UiStyle.Small, Palette.NeonCyan, TextAlignmentOptions.TopLeft, UiFont.Mono);
                line.textWrappingMode = TextWrappingModes.Normal;
                historyLines.Add(line);
            }
        }

        void AddTap(GameObject go, Graphic target)
        {
            var button = go.AddComponent<Button>();
            button.targetGraphic = target;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(ToggleHistory);
        }
    }
}
