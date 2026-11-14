// Stream B. Reveal card (screen BV) after each passed Day 3 puzzle: the Byte from build.json reveal.byte
// names the real thing the player just built (reveal.term) and explains it (reveal.text). This is the only
// place real networking words appear (PRD 4.1). Next raises RevealClosed; GameManager decides what follows.
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class RevealCard : MonoBehaviour
    {
        [SerializeField] GameObject overlay;
        [SerializeField] GameObject speakerTag;
        [SerializeField] TMP_Text speakerText;
        [SerializeField] TMP_Text termText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;

        int puzzleId;
        bool pending; // PuzzlePassed seen, card not shown yet

        void Awake()
        {
            nextButton.onClick.AddListener(OnNext);
            overlay.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.PuzzlePassed += OnPuzzlePassed;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.PuzzlePassed -= OnPuzzlePassed;
            GameEvents.StateChanged -= OnStateChanged;
        }

        // GameManager switches to Reveal from inside PuzzlePassed, so either may arrive first.
        void OnPuzzlePassed(int id)
        {
            puzzleId = id;
            pending = true;
            if (Services.Flow.State == GameState.Reveal) Show();
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.Reveal)
            {
                overlay.SetActive(false);
                return;
            }
            if (pending) Show();
        }

        void Show()
        {
            pending = false;
            var reveal = FindPuzzle(puzzleId)?.reveal;
            var speaker = UiStrings.SpeakerName(reveal?.@byte);
            speakerText.text = speaker;
            speakerTag.SetActive(!string.IsNullOrEmpty(speaker));
            termText.text = reveal?.term ?? "";
            bodyText.text = reveal?.text ?? "";
            nextLabel.text = UiStrings.Ui.next ?? "";
            Services.Dialogue.Hide();
            overlay.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)termText.transform.parent);
        }

        void OnNext()
        {
            if (!overlay.activeSelf) return;
            overlay.SetActive(false);
            GameEvents.RaiseRevealClosed(puzzleId);
        }

        static PuzzleData FindPuzzle(int id)
        {
            var puzzles = Services.Data?.Build?.puzzles;
            if (puzzles == null) return null;
            foreach (var p in puzzles)
                if (p != null && p.id == id) return p;
            return null;
        }
    }
}
