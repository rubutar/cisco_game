// Stream B. The Webex Town Space (PRD-v2 4.2, B-02).
//   * On the map, a chat strip in the bottom-left corner shows the last few posts: the quest brief and
//     each Byte's alert line. It never blocks taps.
//   * At the start of a quest (first time the map shows on a new day) the brief also plays in the
//     dialogue bubble, from Gigi, so the player reads it once in full.
// Text comes from dialogue.json (briefs[], jobs[].days[].alertLine). Other UI can post with Post().
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TownSpace : MonoBehaviour
    {
        public const string BriefSpeaker = "gigi";

        [SerializeField] GameObject strip;
        [SerializeField] TMP_Text spaceText;
        [SerializeField] GameObject[] rows;
        [SerializeField] Image[] avatars;
        [SerializeField] TMP_Text[] initials;
        [SerializeField] TMP_Text[] names;
        [SerializeField] TMP_Text[] times;
        [SerializeField] TMP_Text[] lines;

        struct Post
        {
            public string speaker, name, time, line;
        }

        static TownSpace instance;
        readonly List<Post> posts = new();
        int briefedQuest;
        int lastAlertGroup = -1;
        int lastAlertDay = -1;
        bool dirty;

        void Awake()
        {
            instance = this;
            strip.SetActive(false);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.AlertRaised += OnAlertRaised;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.AlertRaised -= OnAlertRaised;
        }

        /// <summary>Adds a chat post to the strip (speaker is a job key, or "" for the town / alert bot).</summary>
        public static void Add(string speaker, string line) => instance?.Post_(speaker, line);

        /// <summary>The brief lines for a quest from dialogue.json, or null.</summary>
        public static string[] BriefFor(int quest)
        {
            var briefs = Services.Data?.Dialogue?.briefs;
            if (briefs == null) return null;
            foreach (var b in briefs)
                if (b != null && b.quest == quest) return b.lines;
            return null;
        }

        static bool IsMap(GameState s) => s == GameState.WorldPeace || s == GameState.Alert || s == GameState.FinalAttack;

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title)
            {
                posts.Clear();
                briefedQuest = 0;
                lastAlertGroup = lastAlertDay = -1;
                Webex.ResetClock();
            }

            if (to == GameState.WorldPeace) BriefIfNew(Services.Flow.Day);
        }

        // Quests 1 and 2 are days 1 and 2. Build mode (Quests 3 and 4) shows its own briefs.
        void BriefIfNew(int quest)
        {
            if (quest <= briefedQuest) return;
            briefedQuest = quest;
            var brief = BriefFor(quest);
            if (brief == null || brief.Length == 0) return;
            foreach (var line in brief) Post_(BriefSpeaker, line);
            Services.Dialogue.SayLines(BriefSpeaker, brief);
        }

        void OnAlertRaised(AlertInfo info)
        {
            var day = Services.Flow.Day;
            // AlertController raises a group again for Bytes still waiting: post each group once.
            if (info.groupIndex == lastAlertGroup && day == lastAlertDay) return;
            lastAlertGroup = info.groupIndex;
            lastAlertDay = day;
            if (info.jobs == null) return;
            foreach (var job in info.jobs)
                Post_(job.ToKey(), UiStrings.JobDay(job, day)?.alertLine);
        }

        void Post_(string speaker, string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            posts.Add(new Post { speaker = speaker, name = UiStrings.SpeakerName(speaker), time = Webex.NextTime(), line = line });
            while (posts.Count > rows.Length) posts.RemoveAt(0);
            Redraw();
        }

        // Shown on the map only, and out of the way while a dialogue bubble is open (it sits on the same corner).
        void LateUpdate()
        {
            var show = posts.Count > 0 && IsMap(Services.Flow.State) && !Services.Dialogue.IsShowing;
            if (strip.activeSelf != show) strip.SetActive(show);
            if (show && dirty)
            {
                dirty = false;
                // Wrapped lines need the width first, then the heights: rebuild twice.
                var rect = (RectTransform)strip.transform;
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        void Redraw()
        {
            spaceText.text = UiStrings.Ui.gameTitle ?? "";
            // Oldest at the top, newest at the bottom, like a chat.
            var offset = rows.Length - posts.Count;
            for (var i = 0; i < rows.Length; i++)
            {
                var has = i >= offset;
                rows[i].SetActive(has);
                if (!has) continue;
                var p = posts[i - offset];
                avatars[i].color = UiStyle.AvatarFill(p.speaker);
                initials[i].text = Webex.Initials(p.name);
                initials[i].color = UiStyle.AvatarText(p.speaker);
                names[i].text = p.name;
                times[i].text = p.time;
                lines[i].text = p.line;
            }
            dirty = true;
        }
    }
}
