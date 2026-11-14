// Person A. The seven Cisco stations (PRD-v2 4.2a, 4.4, 6.3, 6.4), on GameRoot.prefab so progress survives
// scene loads. Implements ICheckpointService: a station is Done once finished, on Alert once everything it
// requires is done, otherwise Locked. Handles the UI commands "walk:<id>" (HeroController) and "goin:<id>"
// (start the station's shift, the Splunk Trace or build mode), marks stations done and raises
// StationFinished, and holds a quest's day-end card until all of that quest's stations are finished.
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public class CheckpointManager : MonoBehaviour, ICheckpointService
    {
        public static CheckpointManager Instance { get; private set; }

        readonly HashSet<string> done = new();
        string pendingJobStation;   // station whose shift just succeeded; done once its Field Card closes
        int pendingDayEnd = -1;     // a day whose alerts are over but whose stations aren't all done yet

        public CheckpointData[] All => Services.Data.Checkpoints?.checkpoints ?? new CheckpointData[0];
        public int ToolsCollected => done.Count;
        public int ToolsTotal => All.Length;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            Services.Register<ICheckpointService>(this);
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Services.Unregister<ICheckpointService>(this);
        }

        void OnEnable()
        {
            GameEvents.UiCommandRequested += OnCommand;
            GameEvents.ShiftSucceeded += OnShiftSucceeded;
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.RevealClosed += OnRevealClosed;
            GameEvents.RestartDayRequested += OnRestartDay;
        }

        void OnDisable()
        {
            GameEvents.UiCommandRequested -= OnCommand;
            GameEvents.ShiftSucceeded -= OnShiftSucceeded;
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.RevealClosed -= OnRevealClosed;
            GameEvents.RestartDayRequested -= OnRestartDay;
        }

        // ---------- ICheckpointService ----------

        public CheckpointData Get(string id)
        {
            foreach (var c in All) if (c.id == id) return c;
            return null;
        }

        public CheckpointState StateOf(string id)
        {
            if (done.Contains(id)) return CheckpointState.Done;
            var c = Get(id);
            if (c == null) return CheckpointState.Locked;
            foreach (var r in c.requires ?? new string[0])
                if (!done.Contains(r)) return CheckpointState.Locked;
            return CheckpointState.Alert;
        }

        public string LockReason(string id)
        {
            if (StateOf(id) != CheckpointState.Locked) return "";
            var c = Get(id);
            var format = Services.Data.Dialogue?.ui?.lockFormat;
            return string.IsNullOrEmpty(format) || c == null ? "" : string.Format(format, Mathf.Max(1, c.quest - 1));
        }

        // ---------- Commands ----------

        void OnCommand(string command)
        {
            var colon = command.IndexOf(':');
            if (colon < 0) return;
            var verb = command.Substring(0, colon);
            var id = command.Substring(colon + 1);
            if (verb == "walk") HeroController.Instance?.WalkTo(id);
            else if (verb == "goin") GoIn(id);
        }

        /// <summary>Go in: start what the station runs (PRD-v2 4.2a). Locked stations do nothing.</summary>
        public void GoIn(string id)
        {
            var c = Get(id);
            if (c == null || StateOf(id) == CheckpointState.Locked) return;
            Debug.Log($"[Checkpoints] Go in: {id} ({c.job})");
            switch (c.job)
            {
                case "hunt":
                    Services.Hunt.Begin(pinned =>
                    {
                        Debug.Log($"[Checkpoints] Splunk Trace pinned {pinned}");
                        MarkDone(id);
                    });
                    break;
                case "build":
                    GameManager.Instance?.EnterBuild(1);
                    break;
                case "siege":
                    GameManager.Instance?.EnterBuild(LastPuzzle());
                    break;
                default:
                    if (JobIds.TryParse(c.job, out var job)) AlertController.Instance?.TryEnter(job);
                    break;
            }
        }

        // ---------- Finishing stations ----------

        void OnShiftSucceeded(JobId job) => pendingJobStation = StationForJob(job.ToKey());

        void OnStateChanged(GameState from, GameState to)
        {
            // The Field Card closes with "Back to Byte World": the station is finished.
            if (from == GameState.JobSuccess && to == GameState.WorldPeace && pendingJobStation != null)
            {
                var id = pendingJobStation;
                pendingJobStation = null;
                MarkDone(id);
            }
        }

        void OnRevealClosed(int puzzleId)
        {
            // Puzzles 1-2 are the Switch Yard (Quest 3); the last puzzle is the Signal Tower (Quest 4).
            if (puzzleId == 2) MarkDone(StationForJob("build"));
            if (puzzleId >= LastPuzzle()) MarkDone(StationForJob("siege"));
        }

        void OnRestartDay()
        {
            // Restarting a quest forgets that quest's stations, so they can be played again.
            var day = Services.Flow.Day;
            foreach (var c in All)
                if (QuestDay(c.quest) == day) done.Remove(c.id);
            GameEvents.RaiseCheckpointStatesChanged();
        }

        public void MarkDone(string id)
        {
            if (string.IsNullOrEmpty(id) || !done.Add(id)) return;
            Debug.Log($"[Checkpoints] Finished {id}: toolkit {ToolsCollected}/{ToolsTotal}");
            GameEvents.RaiseCheckpointStatesChanged();
            GameEvents.RaiseStationFinished(id);
            if (pendingDayEnd >= 0) TryEndDay(pendingDayEnd);
        }

        /// <summary>
        /// AlertController calls this when a day's alert groups are over. The day-end card waits until every
        /// station of that day's quest is done (Day 2 also needs the Splunk Observatory).
        /// </summary>
        public void TryEndDay(int day)
        {
            foreach (var c in All)
                if (QuestDay(c.quest) == day && c.job != "build" && c.job != "siege" && !done.Contains(c.id))
                {
                    pendingDayEnd = day;
                    return;
                }
            pendingDayEnd = -1;
            GameEvents.RaiseDayEnded(day);
        }

        // ---------- Helpers ----------

        public string StationForJob(string job)
        {
            foreach (var c in All) if (c.job == job) return c.id;
            return null;
        }

        /// <summary>Quests 1 and 2 are Days 1 and 2; Quests 3 and 4 both run on Day 3 (build mode).</summary>
        static int QuestDay(int quest) => Mathf.Min(quest, 3);

        static int LastPuzzle() => Mathf.Max(1, Services.Data.Build?.puzzles?.Length ?? 3);
    }
}
