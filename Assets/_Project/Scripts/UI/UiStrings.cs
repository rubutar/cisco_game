// Stream B. Read-only access to the player-facing text in dialogue.json. Never put game text in code:
// if a string is missing from the JSON, show nothing rather than a hard-coded fallback.
using ByteWorld.Data;

namespace ByteWorld
{
    public static class UiStrings
    {
        static readonly UiText Empty = new();

        public static UiText Ui => Services.Data?.Dialogue?.ui ?? Empty;

        /// <summary>Display name for a speaker key ("scout" → "Scout", "twins" → "Uno and Due"); "" for narration.</summary>
        public static string SpeakerName(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            var jobs = Services.Data?.Dialogue?.jobs;
            if (jobs == null) return "";
            foreach (var job in jobs)
                if (job != null && job.job == key) return job.name ?? "";
            return "";
        }

        /// <summary>Title for a day from dayStart ("Day 2"), or "" if it's not in the JSON.</summary>
        public static string DayTitle(int day)
        {
            var lines = Services.Data?.Dialogue?.dayStart;
            if (lines == null) return "";
            foreach (var line in lines)
                if (line != null && line.day == day) return line.title ?? "";
            return "";
        }

        /// <summary>string.Format that never throws on a missing or malformed format from the JSON.</summary>
        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format)) return "";
            try { return string.Format(format, args); }
            catch (System.FormatException) { return format; }
        }
    }
}
