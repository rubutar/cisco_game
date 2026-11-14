// Stream B. The Field Card (PRD-v2 4.9, B-03), reused for every "you learned this" moment:
//   * after each station's shift (JobScreens calls ShowFieldCard with the day's fieldCard and
//     "Back to Byte World"),
//   * after each passed build puzzle (screen BV): build.json reveal {byte, term, text}; Next raises RevealClosed.
// Layout: the Byte's name tag, their success line, the term, a plain line, then chips for the Cisco tool,
// the general concept and the career role. Every string comes from the JSON.
using System;
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
        [SerializeField] TMP_Text headlineText;
        [SerializeField] TMP_Text termText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] GameObject chipRow;
        [SerializeField] GameObject toolChip;
        [SerializeField] TMP_Text toolText;
        [SerializeField] GameObject conceptChip;
        [SerializeField] TMP_Text conceptText;
        [SerializeField] GameObject roleChip;
        [SerializeField] TMP_Text roleText;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;

        public static RevealCard Instance { get; private set; }

        /// <summary>Raised when a Field Card closes (station or puzzle); the toolkit and Learner Card count them.</summary>
        public static event Action<FieldCardData> FieldCardClosed;

        int puzzleId;
        bool pending;        // PuzzlePassed seen, card not shown yet
        GameState shownIn;   // the card hides as soon as the game leaves this state
        Action onButton;
        FieldCardData current;

        /// <summary>Lets another screen step in before the puzzle's RevealClosed (quest-end check, incident report).</summary>
        public static Func<int, Action, bool> BeforeRevealClosed;

        void Awake()
        {
            Instance = this;
            nextButton.onClick.AddListener(OnNext);
            overlay.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
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

        public bool IsShowing => overlay.activeSelf;

        /// <summary>Shows a Field Card. speaker is a job key ("" for none); headline is an optional line above the term.</summary>
        public void ShowFieldCard(string speaker, string headline, FieldCardData card, string button, Action pressed)
        {
            current = card;
            onButton = pressed;
            shownIn = Services.Flow.State;

            var name = UiStrings.SpeakerName(speaker);
            speakerText.text = name;
            speakerTag.SetActive(!string.IsNullOrEmpty(name));
            SetText(headlineText, headline);
            SetText(termText, card?.term);
            SetText(bodyText, card?.text);
            var tool = SetChip(toolChip, toolText, card?.cisco);
            var concept = SetChip(conceptChip, conceptText, card?.concept);
            var role = SetChip(roleChip, roleText, card?.role);
            chipRow.SetActive(tool || concept || role);
            nextLabel.text = button ?? "";

            Services.Dialogue.Hide();
            Services.Inspect.Hide();
            overlay.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)termText.transform.parent);
        }

        public void Hide()
        {
            overlay.SetActive(false);
            onButton = null;
        }

        // GameManager switches to Reveal from inside PuzzlePassed, so either may arrive first.
        void OnPuzzlePassed(int id)
        {
            puzzleId = id;
            pending = true;
            if (Services.Flow.State == GameState.Reveal) ShowPuzzleReveal();
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (overlay.activeSelf && to != shownIn) Hide();
            if (to == GameState.Reveal && pending) ShowPuzzleReveal();
        }

        void ShowPuzzleReveal()
        {
            pending = false;
            var reveal = FindPuzzle(puzzleId)?.reveal;
            var id = puzzleId;
            ShowFieldCard(reveal?.@byte, null, new FieldCardData { term = reveal?.term, text = reveal?.text },
                UiStrings.Ui.next, () => CloseReveal(id));
        }

        void CloseReveal(int id)
        {
            void Close() => GameEvents.RaiseRevealClosed(id);
            // The quest-end check (and Quest 4's incident report) run before the last RevealClosed of a quest.
            if (BeforeRevealClosed != null && BeforeRevealClosed(id, Close)) return;
            Close();
        }

        void OnNext()
        {
            if (!overlay.activeSelf) return;
            var pressed = onButton;
            var card = current;
            Hide();
            FieldCardClosed?.Invoke(card);
            pressed?.Invoke();
        }

        static void SetText(TMP_Text text, string value)
        {
            text.text = value ?? "";
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        static bool SetChip(GameObject chip, TMP_Text text, string value)
        {
            var has = !string.IsNullOrEmpty(value);
            text.text = value ?? "";
            chip.SetActive(has);
            return has;
        }

        public static PuzzleData FindPuzzle(int id)
        {
            var puzzles = Services.Data?.Build?.puzzles;
            if (puzzles == null) return null;
            foreach (var p in puzzles)
                if (p != null && p.id == id) return p;
            return null;
        }
    }
}
