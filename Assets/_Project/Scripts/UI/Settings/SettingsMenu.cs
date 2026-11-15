// Person A (at Rudi's request), on GameRoot.prefab so it's on every scene. The settings button: a signal-blue
// tile with a pixel gear in the top-right corner of every screen from the title on (UIRootBuilder leaves room
// for it). It opens a small settings card: the language, as two flags (Italian, English) with the current one
// ringed in yellow, and ×. No words needed, so no new text. Time stops while the card is open.
// It hides while a full-height side panel (materials) or the Splunk Trace is open: those have their own × there.
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class SettingsMenu : MonoBehaviour
    {
        const float Tile = 78;          // same height as the toolkit button beside it
        const float Corner = 24;

        GameObject button;
        GameObject overlay;
        Image itRing, enRing;
        bool pausedTime;

        void Awake()
        {
            var go = new GameObject("Settings", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110; // above UIRoot (100) and the Splunk Trace (60)
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var safe = UiKit.Rect("SafeArea", go.transform);
            UiKit.Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            BuildButton(safe);
            BuildCard(safe);
            overlay.SetActive(false);
        }

        void OnEnable() => Language.Changed += ShowLanguage;
        void OnDisable() => Language.Changed -= ShowLanguage;

        void BuildButton(RectTransform parent)
        {
            var rect = UiKit.Rect("SettingsButton", parent);
            UiKit.Place(rect, new Vector2(1, 1), new Vector2(-Corner, -16), new Vector2(Tile, Tile));
            var tile = UiKit.Image(rect.gameObject, Palette.SignalBlue, true);
            UiKit.Border(rect.gameObject, 4, (Color)Palette.Ink);
            var b = rect.gameObject.AddComponent<Button>();
            b.targetGraphic = tile;
            b.onClick.AddListener(Open);
            Icon(rect, "UI/gear", 64, 64);
            button = rect.gameObject;
        }

        void BuildCard(RectTransform parent)
        {
            overlay = UiKit.Rect("SettingsOverlay", parent).gameObject;
            var overlayRect = (RectTransform)overlay.transform;
            UiKit.Stretch(overlayRect);
            var dim = (Color)Palette.Ink;
            dim.a = 0.8f;
            UiKit.Image(overlay, dim, true); // blocks taps on the game underneath

            var card = UiKit.Rect("Card", overlayRect);
            UiKit.Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 400));
            UiKit.Image(card.gameObject, Palette.ByteCream, true);
            UiKit.Border(card.gameObject, UiStyle.CardBorder, (Color)Palette.Ink);

            // Header: the gear, so the card reads as settings without a word.
            var head = UiKit.Rect("Gear", card);
            UiKit.Place(head, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(Tile, Tile));
            UiKit.Image(head.gameObject, Palette.SignalBlue);
            UiKit.Border(head.gameObject, 4, (Color)Palette.Ink);
            Icon(head, "UI/gear", 64, 64);

            var close = UiKit.Button("Close", card, ButtonStyle.Secondary, out var closeLabel, 72);
            UiKit.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(-28, -28), new Vector2(72, 72));
            closeLabel.text = "×";
            closeLabel.font = UiStyle.Font(UiFont.Mono);
            close.onClick.AddListener(Close);

            itRing = Flag(card, Language.Italian, new Vector2(-130, -40));
            enRing = Flag(card, Language.English, new Vector2(130, -40));
            ShowLanguage();
        }

        /// <summary>A flag button; returns its yellow selection ring.</summary>
        Image Flag(RectTransform card, string language, Vector2 position)
        {
            var ring = UiKit.Rect($"Ring {language}", card);
            UiKit.Place(ring, new Vector2(0.5f, 0.5f), position, new Vector2(212, 172));
            var ringImage = UiKit.Image(ring.gameObject, Palette.PacketYellow);

            var b = UiKit.Button($"Flag {language}", card, ButtonStyle.Secondary, out var label, 150);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), position, new Vector2(180, 140));
            label.text = "";
            Icon((RectTransform)b.transform, $"UI/flag_{language}", 128, 96); // 16 x 12 at 8x
            b.onClick.AddListener(() => Language.Set(language));
            return ringImage;
        }

        static void Icon(RectTransform parent, string sprite, float width, float height)
        {
            var icon = UiKit.Rect("Icon", parent);
            icon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Place(icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
            var image = UiKit.Image(icon.gameObject, Color.white);
            image.sprite = UiKit.Sprite(sprite);
            image.preserveAspect = true;
        }

        void ShowLanguage()
        {
            if (itRing == null) return;
            itRing.enabled = Language.Current == Language.Italian;
            enRing.enabled = Language.Current == Language.English;
        }

        void Update()
        {
            // Every screen from the title on, except under panels that put their own × in this corner.
            var state = Services.Flow.State;
            var show = state != GameState.Boot && !overlay.activeSelf && !MaterialsPanel.AnyOpen && !HuntScreen.IsOpen;
            if (button.activeSelf != show) button.SetActive(show);
        }

        void Open()
        {
            ShowLanguage();
            overlay.SetActive(true);
            // Freeze the game unless something (the pause menu) already did.
            pausedTime = Time.timeScale > 0f;
            if (pausedTime) GameEvents.RaiseUiAction(UiAction.Pause);
        }

        void Close()
        {
            overlay.SetActive(false);
            if (pausedTime) GameEvents.RaiseUiAction(UiAction.Resume);
            pausedTime = false;
        }
    }
}
