// Stream B. The stations on the map (PRD-v2 4.2a, B-14):
//   Bubble  - one per station above (or below, near the top edge) its building: the tool badge (and the tool
//             name while on Alert), a pink border and "!" on Alert, green on Done, grey on Locked.
//             Tap: Pax walks there ("walk:<id>").
//   Popup   - when Pax arrives (HeroArrived) the bubble becomes a small popup: badge, station, tool, Enter.
//             Alert stations bounce gently. Walking away (HeroLeft) closes it.
//   Card    - Enter (button, or "interact:<id>" from the Enter key) expands it: the station, the tool as the
//             title, one sentence, without → with, and the buttons by state (Go in / See materials / Play again,
//             or the lock line). × or a tap on the map closes it. It opens inward near the screen edges.
// Sends "walk:<id>" and "goin:<id>" through GameEvents.RaiseUiAction; "materials:<id>" goes to MaterialsPanel.
// Text from checkpoints.json and dialogue.json ui (enter, goIn, seeMaterials, playAgain, keyHint).
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class CheckpointMap : MonoBehaviour
    {
        const float BubbleAbove = 58f;   // map pixels above the door
        const float BubbleBelow = 14f;
        const float TopEdge = 70f;       // stations nearer the top than this show their bubble below

        [SerializeField] RectTransform layer;
        [SerializeField] MaterialsPanel materials;

        class Bubble
        {
            public CheckpointData data;
            public RectTransform rect;
            public Image fill;
            public Outline border;
            public Image badge;
            public GameObject mark;
            public Image markFill;
            public TMP_Text markText;
            public TMP_Text tool;
        }

        readonly Dictionary<string, Bubble> bubbles = new();
        string arrivedAt;
        string cardFor;

        // Popup
        RectTransform popup;
        Outline popupBorder;
        Image popupBadge;
        TMP_Text popupStation, popupTool;
        TMP_Text enterLabel;

        // Card
        GameObject cardCatcher;
        RectTransform card;
        Outline cardBorder;
        Image cardBadge;
        TMP_Text cardStation, cardTitle, cardLine, cardWithout, cardWith, cardLock, cardHint;
        GameObject withRow;
        Button primary, secondary;
        TMP_Text primaryLabel, secondaryLabel;
        Image primaryFill;

        void Awake()
        {
            BuildPopup();
            BuildCard();
            HideAll();
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.CheckpointStatesChanged += Redraw;
            GameEvents.HeroArrived += OnArrived;
            GameEvents.HeroLeft += OnLeft;
            GameEvents.UiCommandRequested += OnCommand;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.CheckpointStatesChanged -= Redraw;
            GameEvents.HeroArrived -= OnArrived;
            GameEvents.HeroLeft -= OnLeft;
            GameEvents.UiCommandRequested -= OnCommand;
        }

        bool OnMap => Stations.IsMapState(Services.Flow.State);

        void OnStateChanged(GameState from, GameState to)
        {
            if (!Stations.IsMapState(to))
            {
                HideAll();
                return;
            }
            EnsureBubbles();
            Redraw();
        }

        void HideAll()
        {
            foreach (var b in bubbles.Values) b.rect.gameObject.SetActive(false);
            popup.gameObject.SetActive(false);
            CloseCard();
        }

        // ---------- Events and keys ----------

        void OnArrived(string id)
        {
            arrivedAt = id;
            if (cardFor != null && cardFor != id) CloseCard();
            Redraw();
        }

        void OnLeft(string id)
        {
            if (arrivedAt == id) arrivedAt = null;
            if (cardFor == id) CloseCard();
            Redraw();
        }

        // Keys from HeroController: "interact:<id>" (Enter / E), "materials:<id>" (M), "back" (Esc).
        void OnCommand(string command)
        {
            if (!OnMap || string.IsNullOrEmpty(command)) return;
            if (command == "back")
            {
                if (materials != null && materials.IsOpen) materials.Close();
                else CloseCard();
                return;
            }
            var colon = command.IndexOf(':');
            if (colon < 0) return;
            var verb = command.Substring(0, colon);
            var id = command.Substring(colon + 1);
            switch (verb)
            {
                case "interact":
                    if (cardFor == id && Services.Checkpoints.StateOf(id) == CheckpointState.Alert) GoIn(id);
                    else OpenCard(id);
                    break;
                case "materials":
                    OpenMaterials(id);
                    break;
            }
        }

        void Tap(string id)
        {
            if (!OnMap) return;
            if (arrivedAt == id) OpenCard(id);
            else GameEvents.RaiseUiAction($"walk:{id}");
        }

        void GoIn(string id)
        {
            CloseCard();
            GameEvents.RaiseUiAction($"goin:{id}");
        }

        void OpenMaterials(string id)
        {
            if (materials == null) return;
            CloseCard();
            materials.Open(id);
        }

        // ---------- Bubbles ----------

        void EnsureBubbles()
        {
            var all = Services.Checkpoints?.All;
            if (all == null) return;
            foreach (var c in all)
            {
                if (c == null || bubbles.ContainsKey(c.id)) continue;
                bubbles[c.id] = NewBubble(c);
            }
        }

        Bubble NewBubble(CheckpointData c)
        {
            var b = new Bubble { data = c };
            b.rect = UiKit.Rect($"Bubble {c.id}", layer);
            b.rect.pivot = new Vector2(0.5f, 0);
            b.fill = UiKit.Image(b.rect.gameObject, UiStyle.Panel, true);
            b.border = UiKit.Border(b.rect.gameObject, 4);
            UiKit.Row(b.rect.gameObject, 12, new RectOffset(12, 18, 8, 8));
            UiKit.Fit(b.rect.gameObject, true, true);
            var button = b.rect.gameObject.AddComponent<Button>();
            button.targetGraphic = b.fill;
            var id = c.id;
            button.onClick.AddListener(() => Tap(id));

            var badge = UiKit.Rect("Badge", b.rect);
            b.badge = UiKit.Image(badge.gameObject, Color.white);
            b.badge.preserveAspect = true;
            UiKit.Size(badge.gameObject, 52, 52);

            b.tool = UiKit.Text("Tool", b.rect, UiStyle.Small, UiStyle.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            b.tool.textWrappingMode = TextWrappingModes.NoWrap;

            // "!" (alert) or a small square (done) on the corner.
            var mark = UiKit.Rect("Mark", b.rect);
            mark.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Place(mark, new Vector2(1, 1), new Vector2(14, 14), new Vector2(40, 40));
            b.markFill = UiKit.Image(mark.gameObject, UiStyle.Danger);
            UiKit.Border(mark.gameObject, 3);
            b.markText = UiKit.Text("Text", mark, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            UiKit.Stretch(b.markText.rectTransform);
            b.mark = mark.gameObject;
            return b;
        }

        void Redraw()
        {
            if (!OnMap || Services.Checkpoints == null) return;
            EnsureBubbles();
            foreach (var b in bubbles.Values)
            {
                var state = Services.Checkpoints.StateOf(b.data.id);
                var showPopup = b.data.id == arrivedAt;
                b.rect.gameObject.SetActive(!showPopup && cardFor != b.data.id);
                b.badge.sprite = Stations.Badge(b.data, state == CheckpointState.Locked);
                b.badge.gameObject.SetActive(b.badge.sprite != null);
                // Seven stations share a small map: only the ones to act on carry their tool name; the rest show the badge.
                b.tool.text = state == CheckpointState.Alert ? b.data.tool ?? "" : "";
                b.tool.gameObject.SetActive(state == CheckpointState.Alert);
                b.tool.color = state == CheckpointState.Locked ? UiStyle.LockedDark : UiStyle.Ink;
                b.fill.color = state == CheckpointState.Locked ? (Color)Palette.GridDim[4] : UiStyle.Panel;
                b.border.effectColor = state == CheckpointState.Alert ? UiStyle.Danger : state == CheckpointState.Done ? UiStyle.Done : UiStyle.Locked;
                b.mark.SetActive(state != CheckpointState.Locked);
                b.markFill.color = state == CheckpointState.Alert ? UiStyle.Danger : UiStyle.Done;
                b.markText.text = state == CheckpointState.Alert ? "!" : "";
            }
            UpdatePopup();
        }

        void LateUpdate()
        {
            if (!OnMap) return;
            // Follow the camera every frame (the map may scale with the screen).
            foreach (var b in bubbles.Values)
                if (b.rect.gameObject.activeSelf) Anchor(b.rect, b.data, 0);
            if (popup.gameObject.activeSelf && arrivedAt != null)
            {
                var c = Services.Checkpoints.Get(arrivedAt);
                var bounce = Services.Checkpoints.StateOf(arrivedAt) == CheckpointState.Alert
                    ? Mathf.Floor(Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * 2f) * 6f : 0f;
                Anchor(popup, c, bounce);
            }
            if (card.gameObject.activeSelf && cardFor != null) PlaceCard(Services.Checkpoints.Get(cardFor));
        }

        // Above the station, or below it when the station sits near the top of the map.
        void Anchor(RectTransform rect, CheckpointData c, float lift)
        {
            if (c == null) return;
            var below = c.y < TopEdge;
            var mapY = below ? c.y + BubbleBelow : c.y - BubbleAbove;
            if (!Stations.MapToLocal(layer, c.x, mapY, out var local)) return;
            rect.pivot = new Vector2(0.5f, below ? 1 : 0);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y + (below ? -lift : lift)));
            ClampInside(rect);
        }

        void ClampInside(RectTransform rect)
        {
            var half = layer.rect.size * 0.5f;
            var size = rect.rect.size;
            var p = rect.anchoredPosition;
            var minX = -half.x + size.x * rect.pivot.x + 16;
            var maxX = half.x - size.x * (1 - rect.pivot.x) - 16;
            if (minX < maxX) p.x = Mathf.Clamp(p.x, minX, maxX);
            rect.anchoredPosition = p;
        }

        // ---------- Popup ----------

        void BuildPopup()
        {
            popup = UiKit.Rect("Popup", layer);
            UiKit.Image(popup.gameObject, UiStyle.Panel, true);
            popupBorder = UiKit.Border(popup.gameObject, 4);
            var shadow = popup.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Palette.Ink;
            shadow.effectDistance = new Vector2(6, -6);
            UiKit.Row(popup.gameObject, 16, new RectOffset(16, 16, 12, 12));
            UiKit.Fit(popup.gameObject, true, true);

            var badge = UiKit.Rect("Badge", popup);
            popupBadge = UiKit.Image(badge.gameObject, Color.white);
            popupBadge.preserveAspect = true;
            UiKit.Size(badge.gameObject, 72, 72);

            var column = UiKit.Rect("Text", popup);
            UiKit.Column(column.gameObject, 2);
            popupStation = UiKit.Text("Station", column, UiStyle.Small, UiStyle.Accent, TextAlignmentOptions.MidlineLeft);
            popupStation.textWrappingMode = TextWrappingModes.NoWrap;
            popupTool = UiKit.Text("Tool", column, UiStyle.Body, UiStyle.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            popupTool.textWrappingMode = TextWrappingModes.NoWrap;

            var enter = UiKit.Button("Enter", popup, ButtonStyle.Main, out enterLabel, 84);
            UiKit.Size(enter.gameObject, 180, 84);
            enter.onClick.AddListener(() => { if (arrivedAt != null) OpenCard(arrivedAt); });
        }

        void UpdatePopup()
        {
            var show = OnMap && arrivedAt != null && cardFor != arrivedAt;
            popup.gameObject.SetActive(show);
            if (!show) return;
            var c = Services.Checkpoints.Get(arrivedAt);
            var state = Services.Checkpoints.StateOf(arrivedAt);
            popupBadge.sprite = Stations.Badge(c, state == CheckpointState.Locked);
            popupBadge.gameObject.SetActive(popupBadge.sprite != null);
            popupStation.text = c?.station ?? "";
            popupTool.text = c?.tool ?? "";
            popupBorder.effectColor = Stations.StateColor(state);
            enterLabel.text = UiStrings.Ui.enter ?? "";
            LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
        }

        // ---------- Card ----------

        void BuildCard()
        {
            // Tapping the map around the card closes it.
            var catcher = UiKit.Rect("CardCatcher", layer);
            UiKit.Stretch(catcher);
            var catcherImage = UiKit.Image(catcher.gameObject, new Color(0, 0, 0, 0), true);
            var catcherButton = catcher.gameObject.AddComponent<Button>();
            catcherButton.targetGraphic = catcherImage;
            catcherButton.transition = Selectable.Transition.None;
            catcherButton.onClick.AddListener(CloseCard);
            cardCatcher = catcher.gameObject;

            card = UiKit.Rect("Card", layer);
            card.sizeDelta = new Vector2(900, 0);
            UiKit.Image(card.gameObject, UiStyle.Panel, true);
            cardBorder = UiKit.Border(card.gameObject, 6);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Palette.Ink;
            shadow.effectDistance = new Vector2(8, -8);
            UiKit.Column(card.gameObject, 18, new RectOffset(36, 36, 28, 32));
            UiKit.Fit(card.gameObject, false, true);

            // Header: badge, station, ×.
            var header = UiKit.Rect("Header", card);
            UiKit.Row(header.gameObject, 16);
            var badge = UiKit.Rect("Badge", header);
            cardBadge = UiKit.Image(badge.gameObject, Color.white);
            cardBadge.preserveAspect = true;
            UiKit.Size(badge.gameObject, 64, 64);
            cardStation = UiKit.Text("Station", header, UiStyle.Small, UiStyle.Accent);
            cardStation.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var close = UiKit.Button("Close", header, ButtonStyle.Secondary, out var closeLabel, 64);
            UiKit.Size(close.gameObject, 64, 64);
            closeLabel.text = "×";
            closeLabel.font = UiStyle.Font(UiFont.Mono);
            close.onClick.AddListener(CloseCard);

            cardTitle = UiKit.Text("Tool", card, UiStyle.Heading, UiStyle.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            cardLine = UiKit.Text("Line", card, UiStyle.Body, UiStyle.Ink);

            // Without → with.
            var row = UiKit.Rect("WithoutWith", card);
            UiKit.Row(row.gameObject, 14);
            withRow = row.gameObject;
            cardWithout = Chip(row, UiStyle.Locked, Palette.ByteCream);
            var arrow = UiKit.Text("Arrow", row, UiStyle.Body, UiStyle.Ink, TextAlignmentOptions.Center, UiFont.Mono);
            arrow.text = "→";
            arrow.textWrappingMode = TextWrappingModes.NoWrap;
            cardWith = Chip(row, UiStyle.Done, Palette.ByteCream);

            cardLock = UiKit.Text("Lock", card, UiStyle.Small, UiStyle.LockedDark, TextAlignmentOptions.MidlineLeft, UiFont.Mono);

            var buttons = UiKit.Rect("Buttons", card);
            UiKit.Row(buttons.gameObject, 20);
            primary = UiKit.Button("Primary", buttons, ButtonStyle.Main, out primaryLabel);
            primaryFill = (Image)primary.targetGraphic;
            UiKit.Size(primary.gameObject, 300, 0);
            secondary = UiKit.Button("Secondary", buttons, ButtonStyle.Secondary, out secondaryLabel);
            UiKit.Size(secondary.gameObject, 420, 0);
            primary.onClick.AddListener(OnPrimary);
            secondary.onClick.AddListener(() => { if (cardFor != null) OpenMaterials(cardFor); });

            cardHint = UiKit.Text("KeyHint", card, UiStyle.Small, UiStyle.LockedDark, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
        }

        static TMP_Text Chip(RectTransform parent, Color fill, Color text)
        {
            var chip = UiKit.Rect("Chip", parent);
            UiKit.Image(chip.gameObject, fill);
            UiKit.Border(chip.gameObject, 3);
            UiKit.Row(chip.gameObject, 0, new RectOffset(16, 16, 6, 8));
            chip.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var label = UiKit.Text("Text", chip, UiStyle.Small, text);
            return label;
        }

        void OpenCard(string id)
        {
            var c = Services.Checkpoints?.Get(id);
            if (c == null || !OnMap) return;
            cardFor = id;
            var state = Services.Checkpoints.StateOf(id);
            var ui = UiStrings.Ui;

            cardBadge.sprite = Stations.Badge(c, state == CheckpointState.Locked);
            cardBadge.gameObject.SetActive(cardBadge.sprite != null);
            cardStation.text = c.station ?? "";
            cardTitle.text = c.tool ?? "";
            cardLine.text = c.line ?? "";
            cardLine.gameObject.SetActive(!string.IsNullOrEmpty(c.line));
            cardWithout.text = c.without ?? "";
            cardWith.text = c.with ?? "";
            withRow.SetActive(!string.IsNullOrEmpty(c.without) || !string.IsNullOrEmpty(c.with));
            cardBorder.effectColor = state == CheckpointState.Alert ? UiStyle.Danger : state == CheckpointState.Locked ? UiStyle.Locked : UiStyle.Ink;

            // Buttons by state (4.2a): Alert Go in + See materials; Done Play again + See materials; Locked lock line + See materials.
            var lockLine = state == CheckpointState.Locked ? Services.Checkpoints.LockReason(id) : "";
            cardLock.text = lockLine;
            cardLock.gameObject.SetActive(!string.IsNullOrEmpty(lockLine));
            primary.gameObject.SetActive(state != CheckpointState.Locked);
            primaryLabel.text = (state == CheckpointState.Alert ? ui.goIn : ui.playAgain) ?? "";
            primaryFill.color = UiStyle.ButtonFill(state == CheckpointState.Alert ? ButtonStyle.Main : ButtonStyle.Secondary);
            secondaryLabel.text = ui.seeMaterials ?? "";
            cardHint.text = ui.keyHint ?? "";
            cardHint.gameObject.SetActive(!string.IsNullOrEmpty(cardHint.text));

            cardCatcher.SetActive(true);
            card.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);
            PlaceCard(c);
            Redraw();
        }

        void OnPrimary()
        {
            if (cardFor == null) return;
            var state = Services.Checkpoints.StateOf(cardFor);
            if (state != CheckpointState.Locked) GoIn(cardFor);
        }

        public void CloseCard()
        {
            if (card == null) return;
            var was = cardFor;
            cardFor = null;
            card.gameObject.SetActive(false);
            cardCatcher.SetActive(false);
            if (was != null) Redraw();
        }

        // In place over the station; inward near the left and right edges, downward for stations near the top.
        void PlaceCard(CheckpointData c)
        {
            if (c == null || !Stations.MapToLocal(layer, c.x, c.y - 20, out var local)) return;
            var half = layer.rect.size * 0.5f;
            var size = card.rect.size;
            var below = c.y < TopEdge + 40;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, below ? 1 : 0);
            var y = below ? local.y - 40 : local.y + 40;
            var x = Mathf.Clamp(local.x, -half.x + size.x * 0.5f + 24, half.x - size.x * 0.5f - 24);
            // Keep it on screen vertically too (below the top bar).
            if (below) y = Mathf.Min(y, half.y - 130);
            else y = Mathf.Min(y, half.y - 130 - size.y);
            y = Mathf.Max(y, -half.y + (below ? size.y : 0) + 24);
            card.anchoredPosition = new Vector2(Mathf.Round(x), Mathf.Round(y));
        }
    }
}
