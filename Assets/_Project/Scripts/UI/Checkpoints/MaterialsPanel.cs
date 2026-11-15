// Stream B. The materials panel (PRD-v2 4.4.4, B-15): half the screen, sliding in from the right over the map
// (0.24 s); the station stays visible on the left, circled in yellow. Same panel for every station:
//   header (station, tool as the title, ×) · what it does · the without / with slider over two little scenes of
//   the same piece of town (drawn in code, RectMask2D) · without and with, three lines each · the Field Card
//   (dim until earned) · in the real world · Cisco Networking Academy · footer by state (Go in, done, lock line).
// Text from checkpoints.json materials, dialogue.json and ui labels. "materials:<id>" stays inside the UI.
// Over a shift (added by A at Rudi's request): MistakeNote's "See materials" link calls OpenOverShift after a
// wrong decision; the shift pauses while the panel is open (UiAction.Pause / Resume), and the map ring and
// Go in stay hidden because the player is already inside the station.
using System.Collections;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class MaterialsPanel : MonoBehaviour
    {
        const float SceneWidth = 780f;
        const float SceneHeight = 300f;

        [SerializeField] RectTransform root;     // full-screen host (under the safe area)

        RectTransform panel;
        RectTransform ring;
        TMP_Text station, title, what, withoutHead, withHead, realWorld, linkText, footerText;
        TMP_Text[] withoutLines = new TMP_Text[3], withLines = new TMP_Text[3];
        RectTransform maskRect;
        Slider slider;
        GameObject fieldCard;
        TMP_Text cardTerm, cardText, cardTool, cardConcept, cardRole;
        CanvasGroup cardGroup;
        Button goIn;
        TMP_Text goInLabel;
        Button link;
        Image withoutTint;
        string openFor;
        bool overShift;
        string url;
        Coroutine slide;

        public bool IsOpen => panel != null && panel.gameObject.activeSelf;

        /// <summary>True while any materials panel is showing (the toolkit button hides).</summary>
        public static bool AnyOpen { get; private set; }

        void Update() => AnyOpen = IsOpen;

        void Awake()
        {
            Build();
            panel.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            MistakeNote.Create(root, this);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnStateChanged(GameState from, GameState to)
        {
            if (overShift && to == GameState.JobShift) return;
            if (!Stations.IsMapState(to)) CloseNow();
            else if (IsOpen && openFor != null) Fill(openFor); // states may have changed
        }

        // ---------- Open and close ----------

        public void Open(string id)
        {
            var c = Services.Checkpoints?.Get(id);
            if (c == null) return;
            openFor = id;
            Fill(id);
            panel.gameObject.SetActive(true);
            ring.gameObject.SetActive(true);
            slider.SetValueWithoutNotify(0.5f);
            ApplySlider(0.5f);
            if (slide != null) StopCoroutine(slide);
            slide = StartCoroutine(Slide(true));
        }

        /// <summary>Opens the station's materials over its shift, pausing the shift until the panel closes.</summary>
        public void OpenOverShift(string id)
        {
            if (Services.Flow.State != GameState.JobShift || Services.Checkpoints?.Get(id) == null) return;
            overShift = true;
            Open(id);
            ring.gameObject.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.Pause);
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (slide != null) StopCoroutine(slide);
            slide = StartCoroutine(Slide(false));
        }

        void CloseNow()
        {
            if (slide != null) StopCoroutine(slide);
            panel.gameObject.SetActive(false);
            ring.gameObject.SetActive(false);
            openFor = null;
            if (!overShift) return;
            overShift = false;
            GameEvents.RaiseUiAction(UiAction.Resume);
        }

        IEnumerator Slide(bool show)
        {
            var width = panel.rect.width;
            var from = show ? width : 0f;
            var to = show ? 0f : width;
            for (var t = 0f; t < UiStyle.PanelSlideSeconds; t += Time.unscaledDeltaTime)
            {
                var k = 1f - Mathf.Pow(1f - t / UiStyle.PanelSlideSeconds, 3f);
                panel.anchoredPosition = new Vector2(Mathf.Lerp(from, to, k), 0);
                yield return null;
            }
            panel.anchoredPosition = new Vector2(to, 0);
            if (!show) CloseNow();
            slide = null;
        }

        void LateUpdate()
        {
            if (!IsOpen || openFor == null || overShift) return;
            var c = Services.Checkpoints.Get(openFor);
            if (c != null && Stations.MapToLocal(root, c.x, c.y - 20, out var local))
            {
                ring.anchoredPosition = local;
                var pulse = 1f + Mathf.Floor(Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f)) * 2f) * 0.06f;
                ring.localScale = new Vector3(pulse, pulse, 1);
            }
        }

        // ---------- Content ----------

        void Fill(string id)
        {
            var c = Services.Checkpoints.Get(id);
            if (c == null) return;
            var state = Services.Checkpoints.StateOf(id);
            var m = c.materials;
            var ui = UiStrings.Ui;

            station.text = c.station ?? "";
            title.text = c.tool ?? "";
            Set(what, m?.what);
            withoutHead.text = c.without ?? "";
            withHead.text = c.with ?? "";
            for (var i = 0; i < 3; i++)
            {
                Set(withoutLines[i], m?.withoutLines != null && i < m.withoutLines.Length ? m.withoutLines[i] : null);
                Set(withLines[i], m?.withLines != null && i < m.withLines.Length ? m.withLines[i] : null);
            }

            // Field Card: "you'll earn this" (dim) before the station is done, "yours" (bright) after.
            var fc = Stations.FieldCard(c);
            fieldCard.SetActive(fc != null && !string.IsNullOrEmpty(fc.term));
            if (fc != null)
            {
                cardTerm.text = fc.term ?? "";
                Set(cardText, fc.text);
                SetChip(cardTool, fc.cisco);
                SetChip(cardConcept, fc.concept);
                SetChip(cardRole, fc.role);
                cardGroup.alpha = state == CheckpointState.Done ? 1f : 0.55f;
            }

            Set(realWorld, m?.realWorld);
            url = m?.learnMoreUrl;
            linkText.text = m?.learnMoreLabel ?? url ?? "";
            link.gameObject.SetActive(!string.IsNullOrEmpty(linkText.text));

            // Footer by state.
            goIn.gameObject.SetActive(state == CheckpointState.Alert && !overShift);
            goInLabel.text = ui.goIn ?? "";
            var footer = state == CheckpointState.Locked ? Services.Checkpoints.LockReason(id)
                : state == CheckpointState.Done ? UiStrings.Format(ui.toolkitCountFormat, Services.Checkpoints.ToolsCollected, Services.Checkpoints.ToolsTotal)
                : "";
            Set(footerText, footer);
            footerText.color = state == CheckpointState.Done ? UiStyle.Done : UiStyle.LockedDark;

            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        // A chip is the label's parent: hide the whole chip when there's nothing to say.
        static void SetChip(TMP_Text label, string value)
        {
            label.text = value ?? "";
            label.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        static void Set(TMP_Text text, string value)
        {
            text.text = value ?? "";
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        void ApplySlider(float value)
        {
            // The "without" scene covers the left part, up to the handle.
            maskRect.sizeDelta = new Vector2(SceneWidth * value, SceneHeight);
        }

        void OnGoIn()
        {
            var id = openFor;
            CloseNow();
            if (id != null) GameEvents.RaiseUiAction($"goin:{id}");
        }

        void OpenLink()
        {
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        // ---------- Layout (built once, in code) ----------

        void Build()
        {
            // Yellow ring around the station on the visible half of the map.
            ring = UiKit.Rect("StationRing", root);
            ring.anchorMin = ring.anchorMax = new Vector2(0.5f, 0.5f);
            ring.sizeDelta = new Vector2(180, 180);
            var ringImage = UiKit.Image(ring.gameObject, Palette.PacketYellow);
            ringImage.sprite = UiKit.Ring();

            panel = UiKit.Rect("Panel", root);
            panel.anchorMin = new Vector2(0.5f, 0);
            panel.anchorMax = new Vector2(1, 1);
            panel.pivot = new Vector2(1, 0.5f);
            panel.offsetMin = new Vector2(0, 0);
            panel.offsetMax = new Vector2(0, -110); // under the top bar
            UiKit.Image(panel.gameObject, UiStyle.Panel, true);
            // 4 px ink left border.
            var edge = UiKit.Rect("Edge", panel);
            edge.anchorMin = new Vector2(0, 0);
            edge.anchorMax = new Vector2(0, 1);
            edge.pivot = new Vector2(1, 0.5f);
            edge.sizeDelta = new Vector2(8, 0);
            UiKit.Image(edge.gameObject, Palette.Ink);

            // Scroll view: phones are short.
            var scroll = UiKit.Rect("Scroll", panel);
            UiKit.Stretch(scroll);
            scroll.offsetMax = new Vector2(0, -8);
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scroll.gameObject.AddComponent<RectMask2D>();
            UiKit.Image(scroll.gameObject, new Color(0, 0, 0, 0), true);

            var content = UiKit.Rect("Content", scroll);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = Vector2.zero;
            UiKit.Column(content.gameObject, 20, new RectOffset(48, 48, 32, 48));
            UiKit.Fit(content.gameObject, false, true);
            scrollRect.content = content;
            scrollRect.viewport = scroll;

            // Header.
            var header = UiKit.Rect("Header", content);
            UiKit.Row(header.gameObject, 16);
            var titles = UiKit.Rect("Titles", header);
            UiKit.Column(titles.gameObject, 2);
            titles.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            station = UiKit.Text("Station", titles, UiStyle.Small, UiStyle.Accent);
            title = UiKit.Text("Tool", titles, UiStyle.Heading, UiStyle.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            var close = UiKit.Button("Close", header, ButtonStyle.Secondary, out var closeLabel, 72);
            UiKit.Size(close.gameObject, 72, 72);
            closeLabel.text = "×";
            closeLabel.font = UiStyle.Font(UiFont.Mono);
            close.onClick.AddListener(Close);

            what = UiKit.Text("What", content, UiStyle.Body, UiStyle.Ink);

            BuildScenes(content);

            // Without / with lists, side by side.
            var lists = UiKit.Rect("Lists", content);
            var listsLayout = UiKit.Row(lists.gameObject, 24, null, TextAnchor.UpperLeft);
            listsLayout.childForceExpandWidth = true;
            withoutHead = ListColumn(lists, UiStyle.Danger, withoutLines);
            withHead = ListColumn(lists, UiStyle.Done, withLines);

            BuildFieldCard(content);

            realWorld = UiKit.Text("RealWorld", content, UiStyle.Body, UiStyle.Ink);
            realWorld.fontStyle = FontStyles.Italic;

            var linkRect = UiKit.Rect("Link", content);
            var linkImage = UiKit.Image(linkRect.gameObject, new Color(0, 0, 0, 0), true);
            link = linkRect.gameObject.AddComponent<Button>();
            link.targetGraphic = linkImage;
            link.onClick.AddListener(OpenLink);
            UiKit.Row(linkRect.gameObject, 0);
            linkText = UiKit.Text("Text", linkRect, UiStyle.Small, UiStyle.Accent);
            linkText.fontStyle = FontStyles.Underline;

            // Footer.
            var footer = UiKit.Rect("Footer", content);
            UiKit.Row(footer.gameObject, 24);
            goIn = UiKit.Button("GoIn", footer, ButtonStyle.Main, out goInLabel);
            UiKit.Size(goIn.gameObject, 320, 0);
            goIn.onClick.AddListener(OnGoIn);
            footerText = UiKit.Text("State", footer, UiStyle.Small, UiStyle.LockedDark, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
        }

        TMP_Text ListColumn(RectTransform parent, Color color, TMP_Text[] lines)
        {
            var column = UiKit.Rect("Column", parent);
            UiKit.Column(column.gameObject, 8);
            column.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var head = UiKit.Rect("Head", column);
            UiKit.Image(head.gameObject, color);
            UiKit.Border(head.gameObject, 3);
            UiKit.Row(head.gameObject, 0, new RectOffset(14, 14, 6, 8));
            var headText = UiKit.Text("Text", head, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            for (var i = 0; i < lines.Length; i++)
                lines[i] = UiKit.Text($"Line{i}", column, UiStyle.Small, UiStyle.Ink);
            return headText;
        }

        void BuildFieldCard(RectTransform parent)
        {
            var cardRect = UiKit.Rect("FieldCard", parent);
            UiKit.Image(cardRect.gameObject, UiStyle.White);
            UiKit.Border(cardRect.gameObject, 4, Palette.SignalBlue);
            UiKit.Column(cardRect.gameObject, 8, new RectOffset(24, 24, 18, 20));
            cardGroup = cardRect.gameObject.AddComponent<CanvasGroup>();
            fieldCard = cardRect.gameObject;
            cardTerm = UiKit.Text("Term", cardRect, UiStyle.Body, Palette.SignalBlue, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            cardText = UiKit.Text("Text", cardRect, UiStyle.Small, UiStyle.Ink);
            var chips = UiKit.Rect("Chips", cardRect);
            UiKit.Row(chips.gameObject, 12);
            cardTool = Chip(chips, Palette.SignalBlue, Palette.ByteCream, UiFont.Title);
            cardConcept = Chip(chips, Palette.NeonCyan, Palette.Ink, UiFont.Body);
            cardRole = Chip(chips, Palette.PacketYellow, Palette.Ink, UiFont.Body);
        }

        static TMP_Text Chip(RectTransform parent, Color fill, Color text, UiFont font)
        {
            var chip = UiKit.Rect("Chip", parent);
            UiKit.Image(chip.gameObject, fill);
            UiKit.Border(chip.gameObject, 3);
            UiKit.Row(chip.gameObject, 0, new RectOffset(14, 14, 4, 6));
            var label = UiKit.Text("Text", chip, UiStyle.Small, text, TextAlignmentOptions.Center, font);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        // ---------- Without / with scenes ----------

        void BuildScenes(RectTransform parent)
        {
            var frame = UiKit.Rect("Scenes", parent);
            UiKit.Size(frame.gameObject, SceneWidth, SceneHeight + 56);

            var view = UiKit.Rect("View", frame);
            UiKit.Place(view, new Vector2(0, 1), Vector2.zero, new Vector2(SceneWidth, SceneHeight));
            UiKit.Border(view.gameObject, 4);
            UiKit.Image(view.gameObject, Palette.Ink);

            var with = UiKit.Rect("With", view);
            UiKit.Stretch(with);
            DrawScene(with, true);

            // "Without" sits on top, clipped to the slider position.
            maskRect = UiKit.Rect("WithoutMask", view);
            UiKit.Place(maskRect, new Vector2(0, 0.5f), Vector2.zero, new Vector2(SceneWidth * 0.5f, SceneHeight));
            maskRect.pivot = new Vector2(0, 0.5f);
            maskRect.gameObject.AddComponent<RectMask2D>();
            var without = UiKit.Rect("Without", maskRect);
            UiKit.Place(without, new Vector2(0, 0.5f), Vector2.zero, new Vector2(SceneWidth, SceneHeight));
            without.pivot = new Vector2(0, 0.5f);
            DrawScene(without, false);

            // The slider (drag or tap anywhere on the track).
            var sliderRect = UiKit.Rect("Slider", frame);
            UiKit.Place(sliderRect, new Vector2(0, 0), Vector2.zero, new Vector2(SceneWidth, 44));
            slider = sliderRect.gameObject.AddComponent<Slider>();
            var track = UiKit.Rect("Track", sliderRect);
            UiKit.Stretch(track);
            track.offsetMin = new Vector2(0, 14);
            track.offsetMax = new Vector2(0, -14);
            UiKit.Image(track.gameObject, Palette.CliffLight, true);
            var area = UiKit.Rect("HandleArea", sliderRect);
            UiKit.Stretch(area);
            var handle = UiKit.Rect("Handle", area);
            handle.sizeDelta = new Vector2(44, 0);
            var handleImage = UiKit.Image(handle.gameObject, Palette.PacketYellow, true);
            UiKit.Border(handle.gameObject, 4);
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.onValueChanged.AddListener(ApplySlider);
        }

        // The same piece of town, without the tool (grey, attackers in the houses) and with it (bright, shielded).
        void DrawScene(RectTransform scene, bool protectedTown)
        {
            UiKit.Image(scene.gameObject, protectedTown ? (Color)Palette.GrassLight : (Color)Palette.GridDim[2]);
            Block(scene, "Road", new Vector2(0.5f, 0.42f), new Vector2(SceneWidth, 34), protectedTown ? Palette.RoadLight : Palette.GridDim[3]);
            var houseColor = protectedTown ? (Color)Palette.ByteCream : (Color)Palette.GridDim[4];
            for (var i = 0; i < 4; i++)
            {
                var x = 0.12f + i * 0.25f;
                if (i == 1) continue; // the station sits here
                Block(scene, $"House{i}", new Vector2(x, 0.72f), new Vector2(84, 70), houseColor);
                Block(scene, $"Window{i}", new Vector2(x, 0.74f), new Vector2(22, 22), protectedTown ? Palette.PacketYellow : Palette.GridDim[1]);
            }
            Block(scene, "Station", new Vector2(0.37f, 0.66f), new Vector2(120, 110), protectedTown ? Palette.SignalBlue : Palette.GridDim[3]);

            if (protectedTown)
            {
                // Cyan shield around the station, attackers bounced off at its edge, packets flowing on the road.
                var shield = UiKit.Rect("Shield", scene);
                shield.anchorMin = shield.anchorMax = new Vector2(0.37f, 0.66f);
                shield.sizeDelta = new Vector2(200, 200);
                var ringImage = UiKit.Image(shield.gameObject, Palette.NeonCyan);
                ringImage.sprite = UiKit.Ring();
                Block(scene, "Attacker", new Vector2(0.10f, 0.25f), new Vector2(30, 30), Palette.AlertPink);
                Block(scene, "Attacker2", new Vector2(0.06f, 0.15f), new Vector2(30, 30), Palette.AlertPink);
                for (var i = 0; i < 5; i++)
                    Block(scene, $"Packet{i}", new Vector2(0.2f + i * 0.15f, 0.42f), new Vector2(20, 20), Palette.PacketYellow);
            }
            else
            {
                // Attackers walking into the houses, red alerts.
                Block(scene, "Attacker", new Vector2(0.37f, 0.48f), new Vector2(30, 30), Palette.AlertPink);
                Block(scene, "Attacker2", new Vector2(0.62f, 0.62f), new Vector2(30, 30), Palette.AlertPink);
                Block(scene, "Attacker3", new Vector2(0.87f, 0.6f), new Vector2(30, 30), Palette.AlertPink);
                Block(scene, "Alert", new Vector2(0.62f, 0.9f), new Vector2(18, 18), Palette.AlertPink);
                Block(scene, "Alert2", new Vector2(0.87f, 0.9f), new Vector2(18, 18), Palette.AlertPink);
            }
        }

        static void Block(RectTransform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            var r = UiKit.Rect(name, parent);
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;
            UiKit.Image(r.gameObject, color);
            UiKit.Border(r.gameObject, 3);
        }
    }
}
