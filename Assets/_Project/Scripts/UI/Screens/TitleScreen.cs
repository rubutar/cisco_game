// Stream B. Title screen (screen 01): game title, tagline and Play. Shown while the game is in the
// Title state. Play only asks GameManager to start (UiAction.Play); GameManager decides what's next.
// Splash art (A): when Sprites/UI/title_backdrop and Sprites/UI/logo exist, the town backdrop fills the
// screen and the BYTE WORLD wordmark image replaces the title text (the text stays, invisible, so the
// five-tap debug menu still works); the tagline gets a dark pill so it reads on the grass.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] GameObject screen;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text titleShadow;
        [SerializeField] TMP_Text taglineText;
        [SerializeField] Button playButton;
        [SerializeField] TMP_Text playLabel;

        void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void Start()
        {
            var ui = UiStrings.Ui;
            titleText.text = ui.gameTitle ?? "";
            if (titleShadow != null) titleShadow.text = titleText.text;
            taglineText.text = ui.tagline ?? "";
            playLabel.text = ui.play ?? "";
            ApplySplashArt();
            SetVisible(Services.Flow.State == GameState.Title);
        }

        void ApplySplashArt()
        {
            if (screen == null) return;
            var backdrop = Resources.Load<Sprite>("Sprites/UI/title_backdrop");
            if (backdrop != null)
            {
                var image = NewImage("Backdrop", screen.transform, backdrop, Color.white);
                image.transform.SetAsFirstSibling();
                Envelope(image);
            }

            var logo = Resources.Load<Sprite>("Sprites/UI/logo");
            if (logo != null && titleText != null)
            {
                var image = NewImage("LogoArt", titleText.transform.parent, logo, Color.white);
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = titleText.rectTransform.anchorMin;
                rect.anchoredPosition = titleText.rectTransform.anchoredPosition + new Vector2(0, 20);
                rect.sizeDelta = new Vector2(1600, 260);
                image.transform.SetSiblingIndex(titleText.transform.GetSiblingIndex() + 1);
                // The text stays where it is for the secret taps, just invisible.
                titleText.color = new Color(0, 0, 0, 0);
                if (titleShadow != null) titleShadow.gameObject.SetActive(false);
            }

            if (backdrop != null && taglineText != null && !string.IsNullOrEmpty(taglineText.text))
            {
                var pill = NewImage("TaglinePill", taglineText.transform.parent, null, Palette.Ink);
                var rect = pill.rectTransform;
                rect.anchorMin = rect.anchorMax = taglineText.rectTransform.anchorMin;
                rect.anchoredPosition = taglineText.rectTransform.anchoredPosition;
                rect.sizeDelta = new Vector2(taglineText.GetPreferredValues(taglineText.text).x + 80, 96);
                pill.transform.SetSiblingIndex(taglineText.transform.GetSiblingIndex());
                taglineText.color = Palette.PacketYellow;
            }
        }

        static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = sprite != null;
            return image;
        }

        /// <summary>Fills the parent, cropping the edges rather than letterboxing (16:9 art on any window).</summary>
        static void Envelope(Image image)
        {
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            image.preserveAspect = false;
            var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        }

        void OnStateChanged(GameState from, GameState to) => SetVisible(to == GameState.Title);

        void SetVisible(bool visible)
        {
            if (screen != null) screen.SetActive(visible);
        }

        static void OnPlay() => GameEvents.RaiseUiAction(UiAction.Play);
    }
}
