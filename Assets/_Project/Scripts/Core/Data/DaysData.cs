// SHARED CONTRACT. Mirrors Resources/Data/days.json (PRD 6.5). JsonUtility rules:
// public fields only, no dictionaries, no nested arrays (alertGroups use AlertGroup objects).
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class DaysFile
    {
        public DayData[] days;
        public FallbackDay3 fallbackDay3;
    }

    [Serializable]
    public class DayData
    {
        public int day;
        public string mode;                 // "tutorial" | "shifts" | "build"
        public AlertGroup[] alertGroups;    // e.g. [{ "jobs": ["scout", "gigi"] }]
        public float alertDelaySeconds;
        public float waitPenaltyPerSecond;  // Day 2: seconds off the waiting Byte's timer per second waited
        public float maxWaitPenalty;        // Day 2: cap as a fraction of the timer (0.5 = 50%)
        public ShiftData[] shifts;
    }

    [Serializable]
    public class AlertGroup
    {
        public string[] jobs;               // "scout" | "gigi" | "twins" | "patch"
    }

    [Serializable]
    public class ShiftData
    {
        public string job;
        public float timerSeconds;
        public float spawnIntervalSeconds;
        public int maxWrongBlocks;          // 0 = complaints off (Day 1)
        public GigiRules rules;             // Gigi only
        public RuleChange ruleChange;       // Gigi only, optional (atSeconds 0 = none)
        public ItemData[] items;
    }

    /// <summary>One incoming thing. Holds the fields of every job; unused ones stay at their defaults.</summary>
    [Serializable]
    public class ItemData
    {
        public string id;
        public string kind;                 // "safe" | "attack"
        public string title;
        public string attacker;             // attack items: "phisher" | "ram" | "worm"
        public string hint;                 // Day 1: shown on the first mistake

        // Scout
        public string from;
        public string body;

        // Gigi
        public int door;
        public string says;

        // Twins
        public bool passwordCorrect;
        public string phoneCode;
        public string visitorCode;
        public bool codeExpired;
        public bool ownerRequested;

        // Patch
        public string type;                 // "crate" | "fakeCrate" | "crack"
        public bool worm;
        public float wormSpeed;

        public bool IsAttack => kind == "attack";
    }

    [Serializable]
    public class GigiRules
    {
        public int[] open;
        public int[] closed;
        public string strangers;            // "block" | "allow"
        public string[] known;              // visitors on Gigi's list
    }

    [Serializable]
    public class RuleChange
    {
        public float atSeconds;
        public int[] open;
        public int[] close;
        public string announce;
    }

    [Serializable]
    public class FallbackDay3
    {
        public AlertGroup[] alertGroups;
        public int itemsPerShift;
        public int sourceDay;
        public float timerSeconds;
    }
}
