// Stream B. Collapse screen: no backups were left, so Byte World fell (PRD 4.5). Dim, calm, never scary.
// "Start again" raises RestartDayRequested; GameManager restarts the day with three backups (A-17).
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class CollapseScreen : MonoBehaviour
    {
        [SerializeField] GameObject screen;
        [SerializeField] TMP_Text lineText;
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonLabel;

        void Awake()
        {
            button.onClick.AddListener(OnStartAgain);
            screen.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.Collapse)
            {
                screen.SetActive(false);
                return;
            }

            Services.Dialogue.Hide();
            Services.Inspect.Hide();
            lineText.text = Services.Data?.Dialogue?.collapse ?? "";
            buttonLabel.text = UiStrings.Ui.startAgain ?? "";
            screen.SetActive(true);
        }

        void OnStartAgain()
        {
            if (!screen.activeSelf) return;
            screen.SetActive(false);
            GameEvents.RaiseRestartDay();
        }
    }
}
