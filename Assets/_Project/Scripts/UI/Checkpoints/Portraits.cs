// Stream B. Pax's portraits in the UI (PRD-v2 6.6, B-18): Resources/Sprites/UI/pax_portrait_*.png from
// Person A. Expressions by moment: happy on the Field Card and the toolkit toast, determined when going into
// a station, worried when a backup is lost, surprised when an alert fires, neutral otherwise.
using UnityEngine;

namespace ByteWorld
{
    public static class Portraits
    {
        public const string Neutral = "neutral";
        public const string Happy = "happy";
        public const string Surprised = "surprised";
        public const string Worried = "worried";
        public const string Determined = "determined";

        public static Sprite Get(string expression) =>
            UiKit.Sprite($"UI/pax_portrait_{expression}") ?? UiKit.Sprite($"UI/pax_portrait_{Neutral}");
    }
}
