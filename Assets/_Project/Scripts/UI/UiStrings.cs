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

        public static JobDialogue Job(JobId job)
        {
            var jobs = Services.Data?.Dialogue?.jobs;
            if (jobs == null) return null;
            var key = job.ToKey();
            foreach (var j in jobs)
                if (j != null && j.job == key) return j;
            return null;
        }

        /// <summary>
        /// A job's lines for a day. Some Bytes only have Day 1 lines, so this falls back to the latest
        /// earlier day that exists. Null if the job has no lines at all.
        /// </summary>
        public static JobDayDialogue JobDay(JobId job, int day)
        {
            var days = Job(job)?.days;
            if (days == null) return null;
            JobDayDialogue best = null;
            foreach (var d in days)
                if (d != null && d.day <= day && (best == null || d.day > best.day)) best = d;
            return best;
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
