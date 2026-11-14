// Stream B. Peace screen (screen 09): Byte World is safe again. Shows dialogue.json "peace" under the game
// title, the Learner Card beside it (PRD-v2 4.9, B-07), and "Play again" (UiAction.PlayAgain) sends
// GameManager back to the title.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class PeaceScreen : MonoBehaviour
    {
        [SerializeField] GameObject screen;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text lineText;
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonLabel;
        [SerializeField] LearnerCard learnerCard;

        void Awake()
        {
            button.onClick.AddListener(OnPlayAgain);
            screen.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.PeaceReached += Show;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.PeaceReached -= Show;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Peace) Show();
            else screen.SetActive(false);
        }

        void Show()
        {
            var ui = UiStrings.Ui;
            titleText.text = ui.gameTitle ?? "";
            lineText.text = Services.Data?.Dialogue?.peace ?? "";
            buttonLabel.text = ui.playAgain ?? "";
            Services.Dialogue.Hide();
            Services.Inspect.Hide();
            screen.SetActive(true);
            if (learnerCard != null) learnerCard.Refresh(true);
        }

        void OnPlayAgain()
        {
            if (!screen.activeSelf) return;
            screen.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.PlayAgain);
        }
    }
}
