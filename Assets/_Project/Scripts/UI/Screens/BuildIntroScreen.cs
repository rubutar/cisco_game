// Stream B. Build day intro (screen B0): Quest 3's Webex brief (dialogue.json briefs, quest 3) or the v1
// build.intro lines in the bubble, then "Let's build" (UiAction.LetsBuild). GameManager then starts build mode.
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

        int quest = 3;

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.UiCommandRequested += OnCommand;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.UiCommandRequested -= OnCommand;
        }

        // "goin:switchyard" is Quest 3, "goin:tower" Quest 4 (checkpoints.json quest).
        void OnCommand(string command)
        {
            if (command == null || !command.StartsWith("goin:")) return;
            var c = Services.Checkpoints?.Get(command.Substring(5));
            if (c != null && (c.job == "build" || c.job == "siege")) quest = c.quest;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            buttonRoot.SetActive(false);
            if (to != GameState.BuildIntro) return;

            var build = Services.Data?.Dialogue?.build;
            buttonLabel.text = UiStrings.Ui.letsBuild ?? "";
            // v2: the brief of the quest about to start (the puzzle GameManager is opening; read only), else the
            // station the player went into, else the v1 build intro.
            var puzzles = Services.Data?.Build?.puzzles;
            var next = GameManager.Instance != null ? GameManager.Instance.CurrentPuzzle : 0;
            if (puzzles != null && next >= 1 && next <= puzzles.Length) quest = QuestCheck.QuestOf(puzzles[next - 1]);
            var brief = TownSpace.BriefFor(quest);
            if (brief != null && brief.Length > 0) Services.Dialogue.SayLines(TownSpace.BriefSpeaker, brief, ShowButton);
            else Services.Dialogue.SayLines(build?.speaker ?? "", build?.intro, ShowButton);
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
