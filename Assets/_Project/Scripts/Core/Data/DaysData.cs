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
        public int rulesChangeAtItem;       // v2 Gigi: the rule change happens when this item (1-based) arrives; 0 = none
        public string rulesChangeLine;      // v2 Gigi: the Webex post announcing it
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
        public string replyTo;              // v2
        public string senderCheck;          // v2: "pass" | "fail"
        public string link;                 // v2: the link domain shown in the inspect panel

        // Gigi
        public int door;
        public string says;

        // Twins
        public bool passwordCorrect;
        public string phoneCode;
        public string visitorCode;
        public bool codeExpired;
        public bool ownerRequested;
        public int pushCount;               // v2: approval requests in the last minute (push bombing when > 1)

        // Patch
        public string type;                 // "crate" | "fakeCrate" | "crack"
        public bool worm;
        public float wormSpeed;
        public string signature;            // v2: "valid" | "invalid"

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
