// SHARED CONTRACT. Mirrors Resources/Data/roaming.json and flags.json.
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class RoamingFile
    {
        public string[] lines;              // one-liners for tapped roaming Bytes
    }

    /// <summary>hunt.json: the Splunk Trace (PRD-v2 4.6).</summary>
    [Serializable]
    public class HuntFile
    {
        public string prompt;
        public HuntChip[] chips;            // the query pieces, tapped in order
        public string[] distractors;        // wrong chips
        public HuntRow[] rows;              // the results table
        public FieldCardData fieldCard;
    }

    [Serializable]
    public class HuntChip
    {
        public string label;
        public int order;
    }

    [Serializable]
    public class HuntRow
    {
        public string srcIp;
        public int destPort;
        public int count;
        public bool target;                 // the row to pin
    }

    /// <summary>
    /// Team switches, owned by Person A. v2: quest4Mode ("scripted" safety net) and heroMode ("teleport"
    /// safety net) are the Sunday 08:00 cut-off switches; heroSpeed is in map pixels per second.
    /// </summary>
    [Serializable]
    public class FlagsFile
    {
        public string day3Mode = "build";
        public string quest4Mode = "scripted";  // v2: "live" | "scripted"
        public string heroMode = "walk";        // v2: "walk" | "teleport"
        public float heroSpeed = 110f;          // v2: map pixels per second

        public bool UseFallbackDay3 => day3Mode == "fallback";
        public bool Quest4Scripted => quest4Mode != "live";
        public bool HeroTeleports => heroMode == "teleport";
    }
}
