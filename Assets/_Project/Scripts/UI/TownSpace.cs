// Stream B. The comms dock (PRD-v2 4.2, B-02 and B-09): one calm panel in the bottom-left corner with two tabs,
//   * the Webex Town Space (quest briefs and each Byte's alert line, as chat posts with avatar and time),
//   * the Splunk log (every decision as one log line, e.g. "splunk> 14:02 gate src=Luca door=23 action=deny";
//     a wrong call shows in pink), from ItemResolved.logLine.
// The dock stays on screen (on the map and during a shift) and shows only the newest item of the open tab, in
// full. Tapping it expands to the last few items. A new item switches to its tab; the other tab shows a dot
// while it has unread items. Hidden while a dialogue bubble or the Splunk Trace is open.
// At the start of a quest (first time the map shows on a new day) the brief also plays in the dialogue bubble,
// from Gigi, so the player reads it once in full.
// Text: dialogue.json (briefs[], jobs[].days[].alertLine, ui.gameTitle, ui.tickerPrefix). Other UI posts with Add().
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TownSpace : MonoBehaviour
    {
        public const string BriefSpeaker = "gigi";

        const int MaxItems = 5;     // history kept per tab (and shown when expanded)
        const float Width = 700f;

        enum Tab { Chat, Log }

        struct Item
        {
            public string speaker, name, time, line;
            public bool wrong; // log: a wrong call
        }

        class Row
        {
            public GameObject go;
            public GameObject avatar;
            public Image avatarFill;
            public TMP_Text initials;
            public GameObject top;
            public TMP_Text name, time, line;
        }

        [SerializeField] RectTransform root;

        static TownSpace instance;
        readonly List<Item> chat = new();
        readonly List<Item> log = new();
        readonly List<Row> rows = new();
        Tab tab = Tab.Chat;
        bool expanded;
        bool unreadChat, unreadLog;
        int briefedQuest;
        int lastAlertGroup = -1;
        int lastAlertDay = -1;
        bool dirty;
        float newAt = -100f;

        RectTransform panel;
        Outline panelBorder;
        Image chatTabFill, logTabFill;
        TMP_Text chatTabText, logTabText, expandText;
        GameObject chatDot, logDot;

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
            GameEvents.ItemResolved += OnItemResolved;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.AlertRaised -= OnAlertRaised;
            GameEvents.ItemResolved -= OnItemResolved;
        }

        /// <summary>Adds a chat post (speaker is a job key, or "" for the town / alert bot).</summary>
        public static void Add(string speaker, string line) => instance?.AddChat(speaker, line);

        /// <summary>The brief lines for a quest from dialogue.json, or null.</summary>
        public static string[] BriefFor(int quest)
        {
            var briefs = Services.Data?.Dialogue?.briefs;
            if (briefs == null) return null;
            foreach (var b in briefs)
                if (b != null && b.quest == quest) return b.lines;
            return null;
        }

        static bool Shows(GameState s) => s is GameState.WorldPeace or GameState.Alert or GameState.FinalAttack or GameState.JobShift;

        // ---------- Feeds ----------

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title)
            {
                chat.Clear();
                log.Clear();
                unreadChat = unreadLog = expanded = false;
                tab = Tab.Chat;
                briefedQuest = 0;
                lastAlertGroup = lastAlertDay = -1;
                Webex.ResetClock();
                dirty = true;
            }
            // Back on the map the town's chat is what matters; the log is one tap away.
            if (Stations.IsMapState(to) && !Stations.IsMapState(from) && chat.Count > 0) Show(Tab.Chat);
            if (to == GameState.WorldPeace) BriefIfNew(Services.Flow.Day);
        }

        // Quests 1 and 2 are days 1 and 2. Build mode (Quests 3 and 4) shows its own briefs.
        void BriefIfNew(int quest)
        {
            if (quest <= briefedQuest) return;
            briefedQuest = quest;
            var brief = BriefFor(quest);
            if (brief == null || brief.Length == 0) return;
            foreach (var line in brief) AddChat(BriefSpeaker, line);
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
                AddChat(job.ToKey(), UiStrings.JobDay(job, day)?.alertLine);
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (string.IsNullOrEmpty(info.logLine)) return;
            Push(log, new Item { line = info.logLine, wrong = !info.correct });
            Arrive(Tab.Log);
        }

        void AddChat(string speaker, string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            Push(chat, new Item { speaker = speaker, name = UiStrings.SpeakerName(speaker), time = Webex.NextTime(), line = line });
            Arrive(Tab.Chat);
        }

        static void Push(List<Item> list, Item item)
        {
            list.Add(item);
            while (list.Count > MaxItems) list.RemoveAt(0);
        }

        // A new item opens its tab, unless the player is reading the other tab's history: then it leaves a dot.
        void Arrive(Tab where)
        {
            newAt = Time.unscaledTime;
            if (expanded && tab != where)
            {
                if (where == Tab.Chat) unreadChat = true;
                else unreadLog = true;
                dirty = true;
                return;
            }
            Show(where);
        }

        void Show(Tab t)
        {
            tab = t;
            if (t == Tab.Chat) unreadChat = false;
            else unreadLog = false;
            dirty = true;
        }

        void Toggle() { expanded = !expanded; dirty = true; }

        // ---------- View ----------

        void LateUpdate()
        {
            var has = chat.Count > 0 || log.Count > 0;
            var show = has && Shows(Services.Flow.State) && !Services.Dialogue.IsShowing && !HuntScreen.IsOpen;
            if (panel.gameObject.activeSelf != show)
            {
                panel.gameObject.SetActive(show);
                dirty |= show;
            }
            if (!show) return;

            // The border glows for a moment when something new arrives.
            var fresh = Time.unscaledTime - newAt < 1.2f && Mathf.Repeat(Time.unscaledTime, 0.4f) < 0.2f;
            panelBorder.effectColor = fresh ? (Color)Palette.PacketYellow : UiStyle.Ink;

            if (!dirty) return;
            dirty = false;
            Redraw();
            // Wrapped lines need the width first, then the heights: rebuild twice.
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        void Redraw()
        {
            var ui = UiStrings.Ui;
            chatTabText.text = ui.gameTitle ?? "";
            logTabText.text = ui.tickerPrefix ?? "";
            StyleTab(chatTabFill, chatTabText, tab == Tab.Chat, Palette.SignalBlue, Palette.ByteCream);
            StyleTab(logTabFill, logTabText, tab == Tab.Log, Palette.Ink, Palette.PacketYellow);
            // A tab appears once it has something in it.
            chatTabFill.gameObject.SetActive(chat.Count > 0);
            logTabFill.gameObject.SetActive(log.Count > 0);
            chatDot.SetActive(unreadChat && tab != Tab.Chat);
            logDot.SetActive(unreadLog && tab != Tab.Log);
            var items = tab == Tab.Chat ? chat : log;
            expandText.text = expanded ? "▼" : "▲";
            expandText.gameObject.SetActive(items.Count > 1);

            // Newest at the bottom, like a chat. Collapsed: only the newest.
            var count = expanded ? items.Count : Mathf.Min(1, items.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var has = i < count;
                r.go.SetActive(has);
                if (!has) continue;
                var item = items[items.Count - count + i];
                if (tab == Tab.Chat) FillChat(r, item);
                else FillLog(r, item);
            }
        }

        static void StyleTab(Image fill, TMP_Text text, bool active, Color activeFill, Color activeText)
        {
            fill.color = active ? activeFill : (Color)Palette.GridDim[1];
            text.color = active ? activeText : (Color)Palette.GridDim[4];
        }

        static void FillChat(Row r, Item p)
        {
            r.avatar.SetActive(true);
            r.top.SetActive(true);
            r.avatarFill.color = UiStyle.AvatarFill(p.speaker);
            r.initials.text = Webex.Initials(p.name);
            r.initials.color = UiStyle.AvatarText(p.speaker);
            r.name.text = p.name;
            r.time.text = p.time;
            UiStyle.Apply(r.line, UiFont.Body);
            r.line.fontSize = UiStyle.Small;
            r.line.color = Palette.ByteCream;
            r.line.text = p.line;
        }

        static void FillLog(Row r, Item e)
        {
            r.avatar.SetActive(false);
            r.top.SetActive(false);
            UiStyle.Apply(r.line, UiFont.Mono);
            r.line.fontSize = UiStyle.Small;
            r.line.color = e.wrong ? UiStyle.Danger : (Color)Palette.NeonCyan;
            r.line.text = e.line;
        }

        // ---------- Build (runtime, like the checkpoint UI) ----------

        void Build()
        {
            panel = UiKit.Rect("Dock", root);
            UiKit.Place(panel, Vector2.zero, new Vector2(24, 24), new Vector2(Width, 0));
            var fill = UiKit.Image(panel.gameObject, UiStyle.Dim(0.94f), true);
            panelBorder = UiKit.Border(panel.gameObject, 4, UiStyle.Ink);
            UiKit.Column(panel.gameObject, 12, new RectOffset(18, 18, 14, 18));
            UiKit.Fit(panel.gameObject, false, true);
            var tap = panel.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(Toggle);

            // Tabs: [BYTE WORLD] [splunk>] ............ [▲]
            var tabs = UiKit.Rect("Tabs", panel);
            UiKit.Row(tabs.gameObject, 10);
            NewTab(tabs, "ChatTab", UiFont.Title, out chatTabFill, out chatTabText, out chatDot, () => { Show(Tab.Chat); });
            NewTab(tabs, "LogTab", UiFont.Mono, out logTabFill, out logTabText, out logDot, () => { Show(Tab.Log); });
            var spacer = UiKit.Rect("Spacer", tabs);
            spacer.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            expandText = UiKit.Text("Expand", tabs, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Mono);
            UiKit.Size(expandText.gameObject, 48, 48);

            for (var i = 0; i < MaxItems; i++) rows.Add(NewRow(panel, i));
        }

        void NewTab(RectTransform parent, string name, UiFont font, out Image fill, out TMP_Text text, out GameObject dot, System.Action pressed)
        {
            var t = UiKit.Rect(name, parent);
            fill = UiKit.Image(t.gameObject, Palette.SignalBlue, true);
            UiKit.Row(t.gameObject, 0, new RectOffset(16, 16, 4, 6));
            var button = t.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => pressed());
            text = UiKit.Text("Text", t, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.Center, font);
            text.textWrappingMode = TextWrappingModes.NoWrap;

            // Unread dot on the tab's corner.
            var d = UiKit.Rect("Dot", t);
            d.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Place(d, new Vector2(1, 1), new Vector2(6, 6), new Vector2(18, 18));
            UiKit.Image(d.gameObject, UiStyle.Danger);
            UiKit.Border(d.gameObject, 3);
            dot = d.gameObject;
            dot.SetActive(false);
        }

        static Row NewRow(RectTransform parent, int index)
        {
            var r = new Row();
            var row = UiKit.Rect($"Item{index}", parent);
            UiKit.Row(row.gameObject, 14, null, TextAnchor.UpperLeft);
            r.go = row.gameObject;

            var avatar = UiKit.Rect("Avatar", row);
            r.avatarFill = UiKit.Image(avatar.gameObject, Palette.SignalBlue);
            UiKit.Border(avatar.gameObject, 3);
            UiKit.Size(avatar.gameObject, 56, 56);
            r.initials = UiKit.Text("Initials", avatar, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            UiKit.Stretch(r.initials.rectTransform);
            r.avatar = avatar.gameObject;

            var column = UiKit.Rect("Text", row);
            UiKit.Column(column.gameObject, 0);
            column.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var top = UiKit.Rect("Top", column);
            UiKit.Row(top.gameObject, 14, null, TextAnchor.LowerLeft);
            r.name = UiKit.Text("Name", top, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.BottomLeft, UiFont.Title);
            r.name.textWrappingMode = TextWrappingModes.NoWrap;
            r.time = UiKit.Text("Time", top, UiStyle.Small, Palette.GridDim[4], TextAlignmentOptions.BottomLeft, UiFont.Mono);
            r.time.textWrappingMode = TextWrappingModes.NoWrap;
            r.top = top.gameObject;
            r.line = UiKit.Text("Line", column, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.TopLeft);
            r.line.textWrappingMode = TextWrappingModes.Normal;
            return r;
        }
    }
}
