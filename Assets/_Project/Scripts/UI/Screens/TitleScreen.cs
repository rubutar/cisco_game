// Stream B. Title screen (screen 01): game title, tagline and Play. Shown while the game is in the
// Title state. Play only asks GameManager to start (UiAction.Play); GameManager decides what's next.
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
            SetVisible(Services.Flow.State == GameState.Title);
        }

        void OnStateChanged(GameState from, GameState to) => SetVisible(to == GameState.Title);

        void SetVisible(bool visible)
        {
            if (screen != null) screen.SetActive(visible);
        }

        static void OnPlay() => GameEvents.RaiseUiAction(UiAction.Play);
    }
}
