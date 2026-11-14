// SHARED CONTRACT. Mirrors Resources/Data/roaming.json and flags.json.
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class RoamingFile
    {
        public string[] lines;              // one-liners for tapped roaming Bytes
    }

    /// <summary>
    /// Team switches, owned by Stream A. day3Mode is the Sunday 08:00 cut-off switch:
    /// "build" runs Stream B's build mode, "fallback" runs the final attack (PRD 4.7).
    /// </summary>
    [Serializable]
    public class FlagsFile
    {
        public string day3Mode = "build";

        public bool UseFallbackDay3 => day3Mode == "fallback";
    }
}
