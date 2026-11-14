// Stream B. The Splunk ticker (PRD-v2 4.2, B-09): every decision appears as one log line in a strip along
// the bottom right, e.g. "14:02 gate src=Luca door=23 action=deny". Lines come from ItemResolved.logLine
// (Person A's JobShift); a wrong call shows in pink. Visible on the map and inside a station, out of the way
// while a dialogue bubble is open.
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public class SplunkTicker : MonoBehaviour
    {
        [SerializeField] GameObject strip;
        [SerializeField] TMP_Text prefixText;
        [SerializeField] TMP_Text[] lineTexts;

        struct Entry
        {
            public string line;
            public bool correct;
        }

        readonly List<Entry> entries = new();

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
            Redraw();
        }

        void LateUpdate()
        {
            var show = entries.Count > 0 && Shows(Services.Flow.State) && !Services.Dialogue.IsShowing;
            if (strip.activeSelf != show) strip.SetActive(show);
        }

        void Redraw()
        {
            prefixText.text = UiStrings.Ui.tickerPrefix ?? "";
            prefixText.gameObject.SetActive(!string.IsNullOrEmpty(prefixText.text));
            // Newest at the bottom.
            var offset = lineTexts.Length - entries.Count;
            for (var i = 0; i < lineTexts.Length; i++)
            {
                var has = i >= offset;
                lineTexts[i].gameObject.SetActive(has);
                if (!has) continue;
                var e = entries[i - offset];
                lineTexts[i].text = e.line;
                lineTexts[i].color = e.correct ? (Color)Palette.NeonCyan : UiStyle.Danger;
            }
        }
    }
}
