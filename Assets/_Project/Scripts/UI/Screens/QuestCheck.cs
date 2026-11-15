// Stream B. The quest-end sequence (PRD-v2 4.4, 4.9, B-04): the Check card, then the quest card.
//   Quests 1 and 2: on DayEnded the Check card covers the day-end card (DayCards); when the last question
//   is done it closes and the quest card underneath is what the player sees.
//   Quests 3 and 4 (build mode): the check runs after the Field Card of the quest's last puzzle, before
//   RevealClosed. Quest 4 first plays the incident report (B-11).
// Each question: 4 options; the answer turns green, a wrong pick pink, then the explanation and Next.
// A wrong answer is never punished. Text from dialogue.json checks[].
using System;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class QuestCheck : MonoBehaviour
    {
        public const int OptionCount = 4;

        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text progressText;
        [SerializeField] TMP_Text questionText;
        [SerializeField] Button[] optionButtons;
        [SerializeField] Image[] optionImages;
        [SerializeField] TMP_Text[] optionLabels;
        [SerializeField] TMP_Text explanationText;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] IncidentReport incident;

        readonly List<CheckQuestion> questions = new();
        int index;
        bool answered;
        bool counts;
        Action onDone;

        void Awake()
        {
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var option = i;
                optionButtons[i].onClick.AddListener(() => Choose(option));
            }
            nextButton.onClick.AddListener(Next);
            overlay.SetActive(false);
            RevealCard.BeforeRevealClosed = BeforeRevealClosed;
        }

        void OnDestroy()
        {
            if (RevealCard.BeforeRevealClosed == (Func<int, Action, bool>)BeforeRevealClosed) RevealCard.BeforeRevealClosed = null;
        }

        void OnEnable()
        {
            GameEvents.DayEnded += OnDayEnded;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.DayEnded -= OnDayEnded;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnDayEnded(int day) => Begin(day, null);

        void OnStateChanged(GameState from, GameState to)
        {
            if (to == GameState.Title) LearnerProgress.Reset();
            // Never leave the check over a screen it doesn't belong to (collapse, restart, title).
            if (overlay.activeSelf && to != GameState.DayEnd && to != GameState.Reveal && to != GameState.WorldPeace) Close(false);
        }

        // ---------- Build quests (3 and 4) ----------

        /// <summary>Quest of a build puzzle: the siege puzzle is Quest 4, the others Quest 3.</summary>
        public static int QuestOf(PuzzleData puzzle) => puzzle?.siege != null && puzzle.siege.enabled ? 4 : 3;

        bool BeforeRevealClosed(int puzzleId, Action close)
        {
            var puzzles = Services.Data?.Build?.puzzles;
            var puzzle = RevealCard.FindPuzzle(puzzleId);
            if (puzzles == null || puzzle == null) return false;

            // Last puzzle of its quest: no later puzzle with the same quest number.
            var quest = QuestOf(puzzle);
            foreach (var p in puzzles)
                if (p != null && p.id > puzzleId && QuestOf(p) == quest) return false;

            if (quest == 4 && incident != null && incident.HasCards)
            {
                incident.Begin(() => Begin(quest, close));
                return true;
            }
            return Begin(quest, close);
        }

        // ---------- The check ----------

        /// <summary>Shows the quest's questions; false (and nothing shown) if it has none.</summary>
        public bool Begin(int quest, Action done)
        {
            questions.Clear();
            var all = Services.Data?.Dialogue?.checks;
            if (all != null)
                foreach (var q in all)
                    if (q != null && q.quest == quest && q.options != null && q.options.Length > 0) questions.Add(q);

            if (questions.Count == 0)
            {
                done?.Invoke();
                return done != null;
            }

            counts = LearnerProgress.StartCheck(quest);
            onDone = done;
            index = 0;
            Services.Dialogue.Hide();
            overlay.SetActive(true);
            ShowQuestion();
            return true;
        }

        void ShowQuestion()
        {
            var q = questions[index];
            answered = false;
            progressText.text = $"{index + 1}/{questions.Count}";
            questionText.text = q.question ?? "";
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var has = i < q.options.Length && !string.IsNullOrEmpty(q.options[i]);
                optionButtons[i].gameObject.SetActive(has);
                optionButtons[i].interactable = true;
                optionImages[i].color = UiStyle.ButtonFill(ButtonStyle.Secondary);
                optionLabels[i].color = UiStyle.Ink;
                optionLabels[i].text = has ? q.options[i] : "";
            }
            explanationText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            Rebuild();
        }

        void Choose(int option)
        {
            if (answered || !overlay.activeSelf) return;
            answered = true;
            var q = questions[index];
            var correct = option == q.answer;
            if (counts) LearnerProgress.AddAnswer(correct);

            for (var i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i].interactable = false;
                if (i == q.answer) Mark(i, UiStyle.Done, Palette.ByteCream);
                else if (i == option) Mark(i, UiStyle.Danger, Palette.ByteCream);
                else Mark(i, UiStyle.White, Palette.CliffLight);
            }
            explanationText.text = q.explanation ?? "";
            explanationText.gameObject.SetActive(!string.IsNullOrEmpty(explanationText.text));
            nextLabel.text = UiStrings.Ui.next ?? "";
            nextButton.gameObject.SetActive(true);
            Rebuild();
        }

        void Mark(int i, Color fill, Color text)
        {
            // Disabled buttons tint their graphic; keep the result colours readable.
            var colors = optionButtons[i].colors;
            colors.disabledColor = Color.white;
            optionButtons[i].colors = colors;
            optionImages[i].color = fill;
            optionLabels[i].color = text;
        }

        void Next()
        {
            if (!answered) return;
            index++;
            if (index < questions.Count) ShowQuestion();
            else Close(true);
        }

        void Close(bool finished)
        {
            overlay.SetActive(false);
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var colors = optionButtons[i].colors;
                colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
                optionButtons[i].colors = colors;
            }
            var done = onDone;
            onDone = null;
            if (finished) done?.Invoke();
        }

        void Rebuild() => LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)questionText.transform.parent);
    }
}
