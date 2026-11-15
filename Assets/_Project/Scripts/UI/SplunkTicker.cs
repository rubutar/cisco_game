// Stream B. The Splunk log (PRD-v2 4.2, B-09): inside a station, every decision appears as one log line in a
// panel in the bottom-left corner, e.g. "14:02 gate src=Luca door=23 action=deny". Lines come from
// ItemResolved.logLine (Person A's JobShift); a wrong call shows in pink. The two newest lines show in full
// (they wrap, never cut off) and stay for the whole shift. Each station starts with a clean log.
// Hidden while a dialogue bubble is open. The map's corner belongs to the Town Space chat instead.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class SplunkTicker : MonoBehaviour
    {
        const int Shown = 2;
        const float Width = 760f;

        struct Entry
        {
            public string line;
            public bool correct;
        }

        [SerializeField] RectTransform root;

        readonly List<Entry> entries = new();
        readonly List<TMP_Text> lines = new();
        RectTransform panel;
        Outline border;
        TMP_Text prefix;
        bool dirty;
        float newAt = -100f;

        void Awake()
        {
            Build();
            panel.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.ItemResolved += OnItemResolved;
            GameEvents.JobEntered += OnJobEntered;
        }

        void OnDisable()
        {
            GameEvents.ItemResolved -= OnItemResolved;
            GameEvents.JobEntered -= OnJobEntered;
        }

        void OnJobEntered(JobContext ctx)
        {
            entries.Clear();
            dirty = true;
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (string.IsNullOrEmpty(info.logLine)) return;
            entries.Add(new Entry { line = info.logLine, correct = info.correct });
            while (entries.Count > Shown) entries.RemoveAt(0);
            newAt = Time.unscaledTime;
            dirty = true;
        }

        void LateUpdate()
        {
            var show = entries.Count > 0 && Services.Flow.State == GameState.JobShift && !Services.Dialogue.IsShowing;
            if (panel.gameObject.activeSelf != show)
            {
                panel.gameObject.SetActive(show);
                dirty |= show;
            }
            if (!show) return;

            var fresh = Time.unscaledTime - newAt < 1.2f && Mathf.Repeat(Time.unscaledTime, 0.4f) < 0.2f;
            border.effectColor = fresh ? (Color)Palette.PacketYellow : UiStyle.Ink;

            if (!dirty) return;
            dirty = false;
            prefix.text = UiStrings.Ui.tickerPrefix ?? "";
            for (var i = 0; i < lines.Count; i++)
            {
                var has = i < entries.Count;
                lines[i].gameObject.SetActive(has);
                if (!has) continue;
                lines[i].text = entries[i].line;
                lines[i].color = entries[i].correct ? (Color)Palette.NeonCyan : UiStyle.Danger;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        void Build()
        {
            panel = UiKit.Rect("Log", root);
            UiKit.Place(panel, Vector2.zero, new Vector2(24, 24), new Vector2(Width, 0));
            UiKit.Image(panel.gameObject, UiStyle.Dim(0.92f));
            border = UiKit.Border(panel.gameObject, 4, UiStyle.Ink);
            UiKit.Column(panel.gameObject, 6, new RectOffset(18, 18, 10, 14));
            UiKit.Fit(panel.gameObject, false, true);
            prefix = UiKit.Text("Prefix", panel, UiStyle.Small, Palette.PacketYellow, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            prefix.textWrappingMode = TextWrappingModes.NoWrap;
            for (var i = 0; i < Shown; i++)
            {
                var line = UiKit.Text($"Line{i}", panel, UiStyle.Small, Palette.NeonCyan, TextAlignmentOptions.TopLeft, UiFont.Mono);
                line.textWrappingMode = TextWrappingModes.Normal;
                lines.Add(line);
            }
        }
    }
}
