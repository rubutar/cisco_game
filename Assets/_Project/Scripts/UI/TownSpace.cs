// Stream B. The Webex Town Space (PRD-v2 4.2, B-02): a small chat panel in the bottom-left corner of the map
// with the two newest posts (the quest brief and each Byte's alert line). A new post opens it; a few seconds
// later it folds into a small "■ BYTE WORLD ▲" pill so the map stays clear. Tap the pill to read the posts
// again, tap again to fold it. Only shown on the map.
// At the start of a quest (first time the map shows on a new day) the brief also plays in the dialogue bubble,
// from Gigi, so the player reads it once in full.
// Text: dialogue.json (briefs[], jobs[].days[].alertLine, ui.gameTitle). Other UI posts with Add().
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TownSpace : MonoBehaviour
    {
        public const string BriefSpeaker = "gigi";

        const int Shown = 2;
        const float Width = 640f;
        const float OpenSeconds = 7f;   // a new post stays open this long, then folds into the pill

        struct Post
        {
            public string speaker, name, time, line;
        }

        class Row
        {
            public GameObject go;
            public Image avatar;
            public TMP_Text initials, name, time, line;
        }

        [SerializeField] RectTransform root;

        static TownSpace instance;
        readonly List<Post> posts = new();
        readonly List<Row> rows = new();
        int briefedQuest;
        int lastAlertGroup = -1;
        int lastAlertDay = -1;
        bool dirty;
        float newAt = -100f;

        RectTransform panel;
        Outline border;
        TMP_Text spaceText, foldText;
        RectTransform header;
        bool open;
        float openedAt;

        void Awake()
        {
            instance = this;
            Build();
            panel.gameObject.SetActive(false);
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

        /// <summary>Right edge of the chat panel in canvas units (12 when hidden), so the splunk> pill can sit next to it.</summary>
        public static float RightEdge =>
            instance != null && instance.panel != null && instance.panel.gameObject.activeSelf ? 24 + instance.panel.sizeDelta.x : 12;

        /// <summary>Adds a chat post (speaker is a job key, or "" for the town / alert bot).</summary>
        public static void Add(string speaker, string line) => instance?.AddPost(speaker, line);

        /// <summary>The brief lines for a quest from dialogue.json, or null.</summary>
        public static string[] BriefFor(int quest)
        {
            var briefs = Services.Data?.Dialogue?.briefs;
            if (briefs == null) return null;
            foreach (var b in briefs)
                if (b != null && b.quest == quest) return b.lines;
            return null;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title)
            {
                posts.Clear();
                briefedQuest = 0;
                lastAlertGroup = lastAlertDay = -1;
                Webex.ResetClock();
                dirty = true;
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
            foreach (var line in brief) AddPost(BriefSpeaker, line);
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
                AddPost(job.ToKey(), UiStrings.JobDay(job, day)?.alertLine);
        }

        void AddPost(string speaker, string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            posts.Add(new Post { speaker = speaker, name = UiStrings.SpeakerName(speaker), time = Webex.NextTime(), line = line });
            while (posts.Count > Shown) posts.RemoveAt(0);
            newAt = Time.unscaledTime;
            Open(true);
        }

        // ---------- View ----------

        void LateUpdate()
        {
            var show = posts.Count > 0 && Stations.IsMapState(Services.Flow.State) && !Services.Dialogue.IsShowing
                       && !HuntScreen.IsOpen && !CheckpointMap.CardShowing;
            if (panel.gameObject.activeSelf != show)
            {
                panel.gameObject.SetActive(show);
                dirty |= show;
            }
            if (!show) return;

            // The border glows for a moment when a post arrives.
            var fresh = Time.unscaledTime - newAt < 1.2f && Mathf.Repeat(Time.unscaledTime, 0.4f) < 0.2f;
            border.effectColor = fresh ? (Color)Palette.PacketYellow : UiStyle.Ink;

            if (open && Time.unscaledTime - openedAt > OpenSeconds) Open(false);

            if (!dirty) return;
            dirty = false;
            Redraw();
            // Wrapped lines need the width first, then the heights: rebuild twice.
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        void Open(bool on)
        {
            open = on;
            if (on) openedAt = Time.unscaledTime;
            dirty = true;
        }

        void Redraw()
        {
            spaceText.text = UiStrings.Ui.gameTitle ?? "";
            foldText.text = open ? "▼" : "▲";
            // Folded, the panel is just its header: as narrow as the space name.
            // Measured from the texts themselves (the layout may not have run yet): dot 14 + two gaps of 12 + padding 36.
            var width = open ? Width : spaceText.GetPreferredValues(spaceText.text).x + foldText.GetPreferredValues(foldText.text).x + 14 + 24 + 36;
            panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
            // Oldest at the top, newest at the bottom, like a chat.
            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var has = open && i < posts.Count;
                r.go.SetActive(has);
                if (!has) continue;
                var p = posts[i];
                r.avatar.color = UiStyle.AvatarFill(p.speaker);
                r.initials.text = Webex.Initials(p.name);
                r.initials.color = UiStyle.AvatarText(p.speaker);
                r.name.text = p.name;
                r.time.text = p.time;
                r.line.text = p.line;
            }
        }

        void Build()
        {
            panel = UiKit.Rect("Chat", root);
            UiKit.Place(panel, Vector2.zero, new Vector2(24, 24), new Vector2(Width, 0));
            var fill = UiKit.Image(panel.gameObject, UiStyle.Dim(0.92f), true);
            border = UiKit.Border(panel.gameObject, 4, UiStyle.Ink);
            UiKit.Column(panel.gameObject, 10, new RectOffset(18, 18, 12, 16));
            UiKit.Fit(panel.gameObject, false, true);
            var tap = panel.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(() => Open(!open));

            // "■ BYTE WORLD ▲": the Webex space name and the fold arrow.
            header = UiKit.Rect("Header", panel);
            UiKit.Row(header.gameObject, 12);
            var dot = UiKit.Rect("Dot", header);
            UiKit.Image(dot.gameObject, Palette.NeonCyan);
            UiKit.Size(dot.gameObject, 14, 14);
            spaceText = UiKit.Text("Space", header, UiStyle.Small, Palette.NeonCyan, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            spaceText.textWrappingMode = TextWrappingModes.NoWrap;
            foldText = UiKit.Text("Fold", header, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            foldText.textWrappingMode = TextWrappingModes.NoWrap;

            for (var i = 0; i < Shown; i++) rows.Add(NewRow(panel, i));
        }

        static Row NewRow(RectTransform parent, int index)
        {
            var r = new Row();
            var row = UiKit.Rect($"Post{index}", parent);
            UiKit.Row(row.gameObject, 14, null, TextAnchor.UpperLeft);
            r.go = row.gameObject;

            var avatar = UiKit.Rect("Avatar", row);
            r.avatar = UiKit.Image(avatar.gameObject, Palette.SignalBlue);
            UiKit.Border(avatar.gameObject, 3);
            UiKit.Size(avatar.gameObject, 52, 52);
            r.initials = UiKit.Text("Initials", avatar, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            UiKit.Stretch(r.initials.rectTransform);

            var column = UiKit.Rect("Text", row);
            UiKit.Column(column.gameObject, 0);
            column.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var top = UiKit.Rect("Top", column);
            UiKit.Row(top.gameObject, 14, null, TextAnchor.LowerLeft);
            r.name = UiKit.Text("Name", top, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.BottomLeft, UiFont.Title);
            r.name.textWrappingMode = TextWrappingModes.NoWrap;
            r.time = UiKit.Text("Time", top, UiStyle.Small, Palette.GridDim[4], TextAlignmentOptions.BottomLeft, UiFont.Mono);
            r.time.textWrappingMode = TextWrappingModes.NoWrap;
            r.line = UiKit.Text("Line", column, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.TopLeft);
            r.line.textWrappingMode = TextWrappingModes.Normal;
            return r;
        }
    }
}
