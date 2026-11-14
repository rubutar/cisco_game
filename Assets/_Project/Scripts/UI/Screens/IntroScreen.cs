// Stream B. Story intro panels (screen 02): one line from dialogue.json "intro" per panel.
// Next pages through the panels here; on the last one it sends UiAction.IntroNext. Skip sends
// UiAction.IntroSkip. GameManager treats either as "intro over" and starts Day 1.
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
        string[] panels = System.Array.Empty<string>();
        int index;

        void Awake()
        {
            nextButton.onClick.AddListener(OnNext);
            skipButton.onClick.AddListener(OnSkip);
            screen.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

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
            for (var i = 0; i < dotImages.Count; i++)
                dotImages[i].color = i == index ? (Color)Palette.NeonCyan : Palette.CliffLight;
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
