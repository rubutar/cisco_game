// Stream B. The Splunk ticker (PRD-v2 4.2, B-09): every decision appears as one log line in a slim strip in
// the bottom-right corner, e.g. "splunk> 14:02 gate src=Luca door=23 action=deny". Lines come from
// ItemResolved.logLine (Person A's JobShift); a wrong call shows in pink. It slides in when a line arrives and
// fades out a few seconds later, so it never sits on the map for long. Hidden while a dialogue bubble or the
// Splunk Trace is open.
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public class SplunkTicker : MonoBehaviour
    {
        const float ShowSeconds = 6f;
        const float FadeSeconds = 0.4f;

        [SerializeField] GameObject strip;
        [SerializeField] CanvasGroup group;
        [SerializeField] TMP_Text[] lineTexts;

        struct Entry
        {
            public string line;
            public bool correct;
        }

        readonly List<Entry> entries = new();
        float shownAt = -100f;

        void Awake() => strip.SetActive(false);

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

        static bool Shows(GameState s) => s is GameState.WorldPeace or GameState.Alert or GameState.JobShift or GameState.FinalAttack;

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title) entries.Clear();
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (string.IsNullOrEmpty(info.logLine)) return;
            entries.Add(new Entry { line = info.logLine, correct = info.correct });
            while (entries.Count > lineTexts.Length) entries.RemoveAt(0);
            shownAt = Time.unscaledTime;
            Redraw();
        }

        void LateUpdate()
        {
            var age = Time.unscaledTime - shownAt;
            var show = entries.Count > 0 && age < ShowSeconds && Shows(Services.Flow.State)
                       && !Services.Dialogue.IsShowing && !HuntScreen.IsOpen;
            if (strip.activeSelf != show) strip.SetActive(show);
            if (show && group != null) group.alpha = Mathf.Clamp01((ShowSeconds - age) / FadeSeconds);
        }

        void Redraw()
        {
            var prefix = UiStrings.Ui.tickerPrefix;
            var tag = string.IsNullOrEmpty(prefix) ? "" : $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PacketYellow)}>{prefix}</color> ";
            // Newest at the bottom.
            var offset = lineTexts.Length - entries.Count;
            for (var i = 0; i < lineTexts.Length; i++)
            {
                var has = i >= offset;
                lineTexts[i].gameObject.SetActive(has);
                if (!has) continue;
                var e = entries[i - offset];
                lineTexts[i].text = tag + e.line;
                lineTexts[i].color = e.correct ? (Color)Palette.NeonCyan : UiStyle.Danger;
            }
        }
    }
}
