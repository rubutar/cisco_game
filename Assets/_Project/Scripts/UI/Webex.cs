// Stream B. Small helpers for the Webex chat look (PRD-v2 4.2, B-02): avatar initials from a speaker's
// display name and a town clock for message timestamps. The clock starts at 14:00 and moves on one
// minute per message, so every post reads like a live chat. No game text here.
namespace ByteWorld
{
    public static class Webex
    {
        const int StartMinutes = 14 * 60;
        static int minutes = StartMinutes;

        /// <summary>The next message's time, "14:03".</summary>
        public static string NextTime()
        {
            minutes++;
            var m = minutes % (24 * 60);
            return $"{m / 60:00}:{m % 60:00}";
        }

        public static void ResetClock() => minutes = StartMinutes;

        /// <summary>"Gigi" → "G", "Uno and Due" → "UD": first letters of the first and last capitalised words.</summary>
        public static string Initials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var words = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            string first = null, last = null;
            foreach (var w in words)
            {
                if (!char.IsUpper(w[0])) continue;
                first ??= w;
                last = w;
            }
            if (first == null) return name.Substring(0, 1).ToUpperInvariant();
            return first == last ? first.Substring(0, 1) : $"{first[0]}{last[0]}";
        }
    }
}
