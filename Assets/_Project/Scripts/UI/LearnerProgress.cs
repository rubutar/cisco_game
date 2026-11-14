// Stream B. What the Learner Card shows at the end (PRD-v2 4.9): check answers and Field Notes (props tried).
// Session only (no saving); cleared when the game goes back to the title.
using System.Collections.Generic;

namespace ByteWorld
{
    public static class LearnerProgress
    {
        /// <summary>Props in the full game (PRD-v2 4.9); the MVP ships 2 of them.</summary>
        public const int FieldNotesTotal = 8;

        static readonly HashSet<string> fieldNotes = new();
        static readonly HashSet<int> checkedQuests = new();

        public static int ChecksCorrect { get; private set; }
        public static int ChecksAnswered { get; private set; }
        public static int FieldNotes => fieldNotes.Count;

        public static event System.Action Changed;

        public static void AddFieldNote(string propId)
        {
            if (!string.IsNullOrEmpty(propId) && fieldNotes.Add(propId)) Changed?.Invoke();
        }

        public static bool HasFieldNote(string propId) => fieldNotes.Contains(propId);

        /// <summary>Counts a quest's check only once, even if the quest is replayed after a collapse.</summary>
        public static bool StartCheck(int quest) => checkedQuests.Add(quest);

        public static void AddAnswer(bool correct)
        {
            ChecksAnswered++;
            if (correct) ChecksCorrect++;
            Changed?.Invoke();
        }

        public static void Reset()
        {
            fieldNotes.Clear();
            checkedQuests.Clear();
            ChecksCorrect = ChecksAnswered = 0;
            Changed?.Invoke();
        }
    }
}
