// Stream A. Loads the JSON content from Resources/Data once at startup (in the current Language) and serves it as IGameData,
// plus lookups the rest of Stream A needs (a day, a job's shift, a job's lines for a day).
using System.Collections.Generic;
using System.Linq;
using ByteWorld.Data;
using ByteWorld.Stubs;
using UnityEngine;

namespace ByteWorld
{
    public class DataLoader : MonoBehaviour, IGameData
    {
        public DaysFile Days { get; private set; }
        public DialogueFile Dialogue { get; private set; }
        public RoamingFile Roaming { get; private set; }
        public BuildFile Build { get; private set; }
        public FlagsFile Flags { get; private set; }
        public HuntFile Hunt { get; private set; }
        public CheckpointsFile Checkpoints { get; private set; }

        public static DataLoader Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            Reload();
            Services.Register<IGameData>(this);
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Services.Unregister<IGameData>(this);
        }

        // Editor and development builds only: confirms the data loaded.
        void Start()
        {
            if (!Debug.isDebugBuild) return;
            var scout = GetShift(1, JobId.Scout);
            var items = scout?.items ?? new ItemData[0];
            Debug.Log($"[DataLoader] Day 1 Scout: {items.Length} item(s): " +
                      string.Join(", ", System.Array.ConvertAll(items, i => $"{i.id} {i.kind} \"{i.title}\"")) +
                      $". Backups {Services.Backups.Left}/{Services.Backups.Max}. Day 3 mode: {Flags.day3Mode}.");
        }

        /// <summary>Reads every file again (handy after editing JSON during Play mode).</summary>
        public void Reload()
        {
            Days = Load<DaysFile>("days");
            Dialogue = Load<DialogueFile>("dialogue");
            Roaming = Load<RoamingFile>("roaming");
            Build = Load<BuildFile>("build");
            Flags = Load<FlagsFile>("flags");
            Hunt = Load<HuntFile>("hunt");
            Checkpoints = Load<CheckpointsFile>("checkpoints");
        }

        /// <summary>The file in the current language (Resources/Data/it/...) when there is one, else the English one.</summary>
        static T Load<T>(string name) where T : new()
        {
            var language = Language.Current;
            if (language != Language.English && Resources.Load<TextAsset>($"Data/{language}/{name}") != null)
                return ResourcesGameData.Load<T>($"{language}/{name}");
            return ResourcesGameData.Load<T>(name);
        }

        /// <summary>
        /// A day's data. With flags.json day3Mode = "fallback", Day 3 is the final attack (PRD 4.7): the
        /// fallbackDay3 alert groups, playing the source day's shifts cut to itemsPerShift with its timer.
        /// </summary>
        public DayData GetDay(int day)
        {
            if (day == 3 && (Flags?.UseFallbackDay3 ?? false)) return FallbackDay();
            return RawDay(day);
        }

        DayData RawDay(int day)
        {
            if (Days?.days == null) return null;
            foreach (var data in Days.days)
                if (data.day == day) return data;
            return null;
        }

        DayData FallbackDay()
        {
            var fallback = Days?.fallbackDay3 ?? new FallbackDay3();
            var source = RawDay(fallback.sourceDay > 0 ? fallback.sourceDay : 2) ?? new DayData();
            var shifts = new List<ShiftData>();
            foreach (var shift in source.shifts ?? new ShiftData[0])
            {
                var items = shift.items ?? new ItemData[0];
                var count = fallback.itemsPerShift > 0 ? Mathf.Min(fallback.itemsPerShift, items.Length) : items.Length;
                shifts.Add(new ShiftData
                {
                    job = shift.job,
                    timerSeconds = fallback.timerSeconds > 0f ? fallback.timerSeconds : shift.timerSeconds,
                    spawnIntervalSeconds = shift.spawnIntervalSeconds,
                    maxWrongBlocks = shift.maxWrongBlocks,
                    rules = shift.rules,
                    ruleChange = shift.ruleChange,
                    items = items.Take(count).ToArray(),
                });
            }
            return new DayData
            {
                day = 3,
                mode = "shifts",
                alertGroups = fallback.alertGroups ?? source.alertGroups,
                alertDelaySeconds = source.alertDelaySeconds,
                waitPenaltyPerSecond = source.waitPenaltyPerSecond,
                maxWaitPenalty = source.maxWaitPenalty,
                shifts = shifts.ToArray(),
            };
        }

        public ShiftData GetShift(int day, JobId job)
        {
            var data = GetDay(day);
            if (data?.shifts == null) return null;
            var key = job.ToKey();
            foreach (var shift in data.shifts)
                if (shift.job == key) return shift;
            return null;
        }

        public JobDialogue GetJobDialogue(JobId job)
        {
            if (Dialogue?.jobs == null) return null;
            var key = job.ToKey();
            foreach (var lines in Dialogue.jobs)
                if (lines.job == key) return lines;
            return null;
        }

        /// <summary>A job's lines for a day; falls back to the latest earlier day that has lines.</summary>
        public JobDayDialogue GetJobDayDialogue(JobId job, int day)
        {
            var days = GetJobDialogue(job)?.days;
            if (days == null) return null;
            JobDayDialogue best = null;
            foreach (var lines in days)
                if (lines.day <= day && (best == null || lines.day > best.day)) best = lines;
            return best;
        }
    }
}
