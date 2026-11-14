// SHARED CONTRACT. Mirrors Resources/Data/dialogue.json (PRD 6.5, 4.8).
// Every player-facing string lives here or in another JSON file, never in code.
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class DialogueFile
    {
        public JobDialogue[] jobs;
        public string[] intro;              // story intro panels (screen 02)
        public DayLine[] dayEnd;            // screen D1, one per day
        public DayLine[] dayStart;          // screen D2, one per day
        public string collapse;
        public string peace;
        public BuildDialogue build;
        public UiText ui;

        // v2 learning layer (PRD-v2 4.9, 6.5)
        public Brief[] briefs;              // Webex briefs per quest
        public CheckQuestion[] checks;      // 2 per quest
        public PropData[] props;
        public LearnerCardData learnerCard;
        public IncidentData incident;
    }

    [Serializable]
    public class Brief
    {
        public int quest;
        public string[] lines;              // 2-3 Webex lines from Gigi
    }

    [Serializable]
    public class FieldCardData
    {
        public string term;
        public string text;                 // plain line, max ~140 characters
        public string cisco;                // the Cisco tool, e.g. "Cisco Umbrella"
        public string concept;              // the general concept
        public string role;                 // the career role
    }

    [Serializable]
    public class CheckQuestion
    {
        public int quest;
        public string question;
        public string[] options;            // exactly 4
        public int answer;                  // index of the correct option (0-3)
        public string explanation;          // one line, shown after answering
    }

    [Serializable]
    public class PropData
    {
        public string id;
        public string name;
        public string place;
        public string concept;
        public string text;
        public string tryLabel;
    }

    [Serializable]
    public class LearnerCardData
    {
        public string title;
        public string[] outcomes;           // the 9 learning outcomes
        public string cta;
        public string url;
    }

    [Serializable]
    public class IncidentData
    {
        public string prompt;
        public string[] cards;              // in the correct order
    }

    [Serializable]
    public class JobDialogue
    {
        public string job;                  // "scout" | "gigi" | "twins" | "patch"
        public string name;                 // "Scout", "Uno and Due"
        public string place;                // "Scout's Lookout"
        public string jobLine;              // tapped in peacetime
        public string primaryAction;        // "Let it dock"
        public string secondaryAction;      // "Send away"
        public string[] fieldLabels;        // inspect panel labels, e.g. ["From", "Cargo"]
        public string[] valueLabels;        // words a job shows as values; Twins: [right, wrong, expired, not asked]
        public JobDayDialogue[] days;
    }

    [Serializable]
    public class JobDayDialogue
    {
        public int day;
        public string alertLine;
        public string[] introLines;         // max 2
        public string successLine;
        public string snatchLine;
        public string backupLine;
        public string waitingLine;          // Day 2: player arrived late
        public string complaintLine;        // a safe item was blocked
        public string complaintFailLine;    // complaints meter full
        public FieldCardData fieldCard;     // v2: shown after this station's shift
    }

    [Serializable]
    public class DayLine
    {
        public int day;
        public string speaker;              // job key, or "" for none
        public string title;                // "Day 1 done", "Day 2"
        public string line;
    }

    [Serializable]
    public class BuildDialogue
    {
        public string speaker;              // job key of the Byte leading build day
        public string[] intro;              // max 2
        public string failNoPath;
        public string failWrongHouse;
        public string failLeak;
        public string failBlocked;
        public string success;
    }

    /// <summary>Button labels and HUD text. Formats use string.Format placeholders.</summary>
    [Serializable]
    public class UiText
    {
        public string gameTitle;
        public string tagline;
        public string play;
        public string next;
        public string skip;
        public string tryAgain;
        public string backToWorld;
        public string keepWatching;
        public string sleep;
        public string startDay;
        public string startAgain;
        public string letsBuild;
        public string run;
        public string fixBuild;
        public string playAgain;
        public string backups;
        public string complaints;
        public string dayAlertFormat;       // "Day {0}: alert {1} of {2}"
        public string dayPuzzleFormat;      // "Day {0}: puzzle {1} of {2}"
        public string backupsLeftFormat;    // "Backups left: {0}"
        public string snatchedFormat;       // "{0} was snatched!"
        public string runResultFormat;      // "Delivered {0} · Blocked {1} · Leaked {2}"
        public string rotatePhone;
        public string pause;                // pause menu title, "Paused"
        public string resume;               // pause menu button
        public string restartDay;           // pause menu button: restart the current day with three backups

        // v2 labels
        public string call;
        public string priority;
        public string pin;
        public string report;
        public string quarantine;
        public string fieldNotesFormat;     // "Field Notes {0}/{1}"
        public string tickerPrefix;
        public string enter;
        public string goIn;
        public string seeMaterials;
        public string toolkit;              // "Cisco toolkit"
        public string toolkitCountFormat;   // "Cisco toolkit {0}/{1}"
        public string toolkitToastFormat;   // "{0} added to your Cisco toolkit"
        public string keyHint;              // "Enter: go in · M: materials · Esc: close"
    }
}
