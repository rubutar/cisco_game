// Stream B. Pause button and pause menu (B-16). The button shows while the player is actually playing;
// the menu sends UiAction.Pause / UiAction.Resume (GameManager stops and restarts time) and
// RestartDayRequested (GameManager restarts the day with three backups).
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject pauseButton;
        [SerializeField] GameObject menu;
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button resumeButton;
        [SerializeField] TMP_Text resumeLabel;
        [SerializeField] Button restartButton;
        [SerializeField] TMP_Text restartLabel;

        void Awake()
        {
            pauseButton.GetComponent<Button>().onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(RestartDay);
            menu.SetActive(false);
            pauseButton.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void Start() => pauseButton.SetActive(CanPause(Services.Flow.State));

        // Only while the player is playing: not on title, story, cards, results or a test run in progress.
        static bool CanPause(GameState state) => state switch
        {
            GameState.WorldPeace or GameState.Alert or GameState.JobShift or GameState.Build or GameState.FinalAttack => true,
            _ => false
        };

        void OnStateChanged(GameState from, GameState to)
        {
            pauseButton.SetActive(CanPause(to) && !menu.activeSelf);
            if (menu.activeSelf && !CanPause(to)) Resume(); // never leave the game frozen behind another screen
        }

        void Pause()
        {
            if (menu.activeSelf) return;
            var ui = UiStrings.Ui;
            titleText.text = ui.pause ?? "";
            resumeLabel.text = ui.resume ?? "";
            restartLabel.text = ui.restartDay ?? "";
            menu.SetActive(true);
            pauseButton.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.Pause);
        }

        void Resume()
        {
            if (!menu.activeSelf) return;
            menu.SetActive(false);
            pauseButton.SetActive(CanPause(Services.Flow.State));
            GameEvents.RaiseUiAction(UiAction.Resume);
        }

        void RestartDay()
        {
            if (!menu.activeSelf) return;
            // Resume first: GameManager's restart doesn't touch the time scale.
            Resume();
            GameEvents.RaiseRestartDay();
        }
    }
}
