// Stream B. Build day intro (screen B0): the lead Byte's intro lines from dialogue.json build.intro in the
// bubble, then "Let's build" (UiAction.LetsBuild). GameManager then starts build mode.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class BuildIntroScreen : MonoBehaviour
    {
        [SerializeField] GameObject buttonRoot;
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonLabel;

        void Awake()
        {
            button.onClick.AddListener(OnLetsBuild);
            buttonRoot.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnStateChanged(GameState from, GameState to)
        {
            buttonRoot.SetActive(false);
            if (to != GameState.BuildIntro) return;

            var build = Services.Data?.Dialogue?.build;
            buttonLabel.text = UiStrings.Ui.letsBuild ?? "";
            Services.Dialogue.SayLines(build?.speaker ?? "", build?.intro, ShowButton);
        }

        void ShowButton()
        {
            if (Services.Flow.State == GameState.BuildIntro) buttonRoot.SetActive(true);
        }

        void OnLetsBuild()
        {
            if (!buttonRoot.activeSelf) return;
            buttonRoot.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.LetsBuild);
        }
    }
}
