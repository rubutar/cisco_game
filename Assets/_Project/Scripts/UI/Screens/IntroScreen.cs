// Stream B. Story intro panels (screen 02): one line from dialogue.json "intro" per panel.
// Next pages through the panels here; on the last one it sends UiAction.IntroNext. Skip sends
// UiAction.IntroSkip. GameManager treats either as "intro over" and starts Day 1.
// Story art (A): when Sprites/UI/intro_<n> exists for panel n, it fills the screen behind the line, and the
// line sits low on the picture's dark band.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class IntroScreen : MonoBehaviour
    {
        const float DotSize = 24f;

        [SerializeField] GameObject screen;
        [SerializeField] TMP_Text panelText;
        [SerializeField] RectTransform dots;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] Button skipButton;
        [SerializeField] TMP_Text skipLabel;

        readonly List<Image> dotImages = new();
        Image art;
        string[] panels = System.Array.Empty<string>();
        int index;

        void Awake()
        {
            nextButton.onClick.AddListener(OnNext);
            skipButton.onClick.AddListener(OnSkip);
            screen.SetActive(false);

            var go = new GameObject("Art", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            go.transform.SetParent(screen.transform, false);
            go.transform.SetAsFirstSibling();
            art = go.GetComponent<Image>();
            art.raycastTarget = false;
            var rect = art.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;
            art.enabled = false;
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            Language.Changed += OnLanguageChanged;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            Language.Changed -= OnLanguageChanged;
        }

        // The settings card can switch language mid-story: show the same panel in the new language.
        void OnLanguageChanged()
        {
            if (!screen.activeSelf) return;
            panels = Services.Data?.Dialogue?.intro ?? System.Array.Empty<string>();
            if (panels.Length == 0) return;
            index = Mathf.Min(index, panels.Length - 1);
            var ui = UiStrings.Ui;
            nextLabel.text = ui.next ?? "";
            skipLabel.text = ui.skip ?? "";
            ShowPanel();
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.Intro)
            {
                screen.SetActive(false);
                return;
            }

            panels = Services.Data?.Dialogue?.intro ?? System.Array.Empty<string>();
            if (panels.Length == 0)
            {
                // Nothing to show: let GameManager move straight on.
                GameEvents.RaiseUiAction(UiAction.IntroSkip);
                return;
            }

            var ui = UiStrings.Ui;
            nextLabel.text = ui.next ?? "";
            skipLabel.text = ui.skip ?? "";
            BuildDots(panels.Length);
            index = 0;
            ShowPanel();
            screen.SetActive(true);
        }

        void ShowPanel()
        {
            panelText.text = panels[index];
            var sprite = Resources.Load<Sprite>($"Sprites/UI/intro_{index + 1}");
            art.sprite = sprite;
            art.enabled = sprite != null;
            PlaceText(sprite != null);
            for (var i = 0; i < dotImages.Count; i++)
                dotImages[i].color = i == index ? (Color)Palette.NeonCyan : Palette.CliffLight;
        }

        /// <summary>Over a picture the line sits low, on its dark band; without one it stays in the middle.</summary>
        void PlaceText(bool overArt)
        {
            var rect = panelText.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, overArt ? 0f : 0.5f);
            rect.pivot = new Vector2(0.5f, overArt ? 0f : 0.5f);
            rect.anchoredPosition = overArt ? new Vector2(0, 190) : new Vector2(0, 60);
            rect.sizeDelta = overArt ? new Vector2(1500, 200) : new Vector2(1500, 500);
            panelText.fontSize = overArt ? 56 : 76;
        }

        void OnNext()
        {
            if (!screen.activeSelf) return;
            if (index < panels.Length - 1)
            {
                index++;
                ShowPanel();
                return;
            }
            screen.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.IntroNext);
        }

        void OnSkip()
        {
            if (!screen.activeSelf) return;
            screen.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.IntroSkip);
        }

        void BuildDots(int count)
        {
            while (dotImages.Count < count)
            {
                var go = new GameObject($"Dot{dotImages.Count}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(dots, false);
                ((RectTransform)go.transform).sizeDelta = new Vector2(DotSize, DotSize);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                dotImages.Add(image);
            }
            for (var i = 0; i < dotImages.Count; i++) dotImages[i].gameObject.SetActive(i < count);
        }
    }
}
