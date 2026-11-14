// Stream A. Loads the JSON content from Resources/Data once at startup and serves it as IGameData,
// plus lookups the rest of Stream A needs (a day, a job's shift, a job's lines for a day).
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
            Days = ResourcesGameData.Load<DaysFile>("days");
            Dialogue = ResourcesGameData.Load<DialogueFile>("dialogue");
            Roaming = ResourcesGameData.Load<RoamingFile>("roaming");
            Build = ResourcesGameData.Load<BuildFile>("build");
            Flags = ResourcesGameData.Load<FlagsFile>("flags");
        }

        public DayData GetDay(int day)
        {
            if (Days?.days == null) return null;
            foreach (var data in Days.days)
                if (data.day == day) return data;
            return null;
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
