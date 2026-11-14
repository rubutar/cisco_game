// SHARED CONTRACT. Change only through a contract-change PR to main (see CLAUDE.md).
using System;
using UnityEngine;

namespace ByteWorld
{
    public struct AlertInfo
    {
        public JobId[] jobs;      // every Byte flashing in this group (1 on Day 1, 2 on Day 2, 4 in the fallback)
        public int groupIndex;    // 0-based index of this group in the day
        public int groupCount;    // number of groups in the day
    }

    public struct JobContext
    {
        public JobId job;
        public int day;
        public bool isTutorial;     // Day 1: hints and retry, never a snatch
        public float timerSeconds;  // after the wait penalty
        public float waitedSeconds; // how long this Byte waited during a double alert
    }

    public struct ShiftProgressInfo
    {
        public JobId job;
        public int handled;
        public int total;
        public float secondsLeft;
    }

    public struct ItemResolvedInfo
    {
        public int day;        // the day (= quest) the decision was made in
        public JobId job;
        public string itemId;
        public string title;   // the item's title, e.g. "Post boat: Your Bank"
        public bool allowed;   // true = let through (dock, let in, open, fit)
        public bool correct;   // true = the player made the right call
        public string attacker;// the item's attacker ("" for safe items)
        public string logLine; // Splunk-style line for the ticker, e.g. "14:02 gate src=Luca door=23 action=deny"
    }

    public struct ComplaintInfo
    {
        public SnatchSource source; // JobShift or TestRun
        public int count;           // complaints so far in this shift or run
        public int max;             // maxWrongBlocks
    }

    public struct SnatchInfo
    {
        public SnatchSource source;
        public JobId job;           // the Byte that was snatched (TestRun: the job Byte that reacts)
        public string attacker;     // "phisher", "ram", "worm"; "complaints" when the complaints meter failed the shift
        public int backupsLeft;     // filled in by IBackupNest
    }

    public struct RunResult
    {
        public int puzzleId;
        public int delivered;
        public int lost;
        public int blocked;
        public int leaked;
        public bool passed;
    }

    /// <summary>
    /// The event bus (PRD 6.4). Gameplay raises, UI listens. UI talks back only through
    /// <see cref="UiActionRequested"/>, <see cref="RestartDayRequested"/> and <see cref="DebugMenuRequested"/>.
    /// Only <see cref="IBackupNest"/> raises Snatched, BackupsChanged and Collapsed.
    /// </summary>
    public static class GameEvents
    {
        // Flow (raised by Stream A)
        public static event Action<GameState, GameState> StateChanged;
        public static event Action<int, DayMode> DayStarted;
        public static event Action<int> DayEnded;
        public static event Action<AlertInfo> AlertRaised;
        public static event Action<JobContext> JobEntered;
        public static event Action<ShiftProgressInfo> ShiftProgress;
        public static event Action<ItemResolvedInfo> ItemResolved;
        public static event Action<JobId> ShiftSucceeded;
        public static event Action PeaceReached;

        // Hero and stations (PRD-v2 4.2a, 6.4): raised by Person A, shown by Person B
        public static event Action CheckpointStatesChanged;  // re-read Services.Checkpoints
        public static event Action<string> HeroArrived;      // checkpoint id: show the popup
        public static event Action<string> HeroLeft;         // checkpoint id: hide the popup
        public static event Action<string> StationFinished;  // checkpoint id: toolkit toast, next brief

        // UI -> core text commands: "walk:<id>", "goin:<id>" (Person A) and "materials:<id>" (UI only)
        public static event Action<string> UiCommandRequested;

        // World props (raised by Person A's map, shown by Person B's prop card)
        public static event Action<string> PropTapped;

        // Shared rules (shifts: Stream A; test runs: Stream B)
        public static event Action<ComplaintInfo> Complaint;

        // Backup nest only
        public static event Action<SnatchInfo> Snatched;
        public static event Action<int, int> BackupsChanged; // (left, max)
        public static event Action<int> Collapsed;           // day

        // Day 3 build (raised by Stream B)
        public static event Action<int, int, int> PuzzleStarted; // (puzzleId, index, count)
        public static event Action<RunResult> TestRunFinished;
        public static event Action<int> PuzzlePassed;            // puzzleId
        public static event Action<int> RevealClosed;            // puzzleId

        // UI -> core requests (raised by Stream B)
        public static event Action<UiAction> UiActionRequested;
        public static event Action RestartDayRequested;
        public static event Action DebugMenuRequested;

        public static void RaiseStateChanged(GameState from, GameState to) => StateChanged?.Invoke(from, to);
        public static void RaiseDayStarted(int day, DayMode mode) => DayStarted?.Invoke(day, mode);
        public static void RaiseDayEnded(int day) => DayEnded?.Invoke(day);
        public static void RaiseAlert(AlertInfo info) => AlertRaised?.Invoke(info);
        public static void RaiseJobEntered(JobContext ctx) => JobEntered?.Invoke(ctx);
        public static void RaiseShiftProgress(ShiftProgressInfo info) => ShiftProgress?.Invoke(info);
        public static void RaiseItemResolved(ItemResolvedInfo info) => ItemResolved?.Invoke(info);
        public static void RaiseShiftSucceeded(JobId job) => ShiftSucceeded?.Invoke(job);
        public static void RaisePeaceReached() => PeaceReached?.Invoke();
        public static void RaisePropTapped(string propId) => PropTapped?.Invoke(propId);
        public static void RaiseCheckpointStatesChanged() => CheckpointStatesChanged?.Invoke();
        public static void RaiseHeroArrived(string checkpointId) => HeroArrived?.Invoke(checkpointId);
        public static void RaiseHeroLeft(string checkpointId) => HeroLeft?.Invoke(checkpointId);
        public static void RaiseStationFinished(string checkpointId) => StationFinished?.Invoke(checkpointId);

        /// <summary>Text command from the UI: "walk:harbour", "goin:harbour", "materials:harbour".</summary>
        public static void RaiseUiAction(string command) => UiCommandRequested?.Invoke(command);
        public static void RaiseComplaint(ComplaintInfo info) => Complaint?.Invoke(info);
        public static void RaiseSnatched(SnatchInfo info) => Snatched?.Invoke(info);
        public static void RaiseBackupsChanged(int left, int max) => BackupsChanged?.Invoke(left, max);
        public static void RaiseCollapsed(int day) => Collapsed?.Invoke(day);
        public static void RaisePuzzleStarted(int puzzleId, int index, int count) => PuzzleStarted?.Invoke(puzzleId, index, count);
        public static void RaiseTestRunFinished(RunResult result) => TestRunFinished?.Invoke(result);
        public static void RaisePuzzlePassed(int puzzleId) => PuzzlePassed?.Invoke(puzzleId);
        public static void RaiseRevealClosed(int puzzleId) => RevealClosed?.Invoke(puzzleId);
        public static void RaiseUiAction(UiAction action) => UiActionRequested?.Invoke(action);
        public static void RaiseRestartDay() => RestartDayRequested?.Invoke();
        public static void RaiseDebugMenu() => DebugMenuRequested?.Invoke();

        // Static events survive play sessions when domain reload is off; clear them on start.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetAll()
        {
            StateChanged = null; DayStarted = null; DayEnded = null; AlertRaised = null;
            JobEntered = null; ShiftProgress = null; ItemResolved = null; ShiftSucceeded = null;
            PeaceReached = null; PropTapped = null; Complaint = null;
            CheckpointStatesChanged = null; HeroArrived = null; HeroLeft = null; StationFinished = null;
            UiCommandRequested = null; Snatched = null; BackupsChanged = null;
            Collapsed = null; PuzzleStarted = null; TestRunFinished = null; PuzzlePassed = null;
            RevealClosed = null; UiActionRequested = null; RestartDayRequested = null;
            DebugMenuRequested = null;
        }
    }
}
