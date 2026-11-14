// Stream A. Everything a job shift shares (A-09, PRD 4.5–4.7): items arrive on a timer, a tap opens the inspect
// panel, the two actions resolve the item, a shift timer runs, and the shift ends in success or a snatch.
// Also here because every job needs them: Day 1 tutorial mode (hint and retry, no timer, never a backup) and
// Day 2 complaints (blocked safe items; ComplaintRules decides when the shift fails).
// A job script only draws its scene and its items: override BuildScene, SpawnItem, ItemPosition,
// OnItemResolved and ClearItems, and InspectFields for what its inspect panel shows.
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public abstract class JobShift : MonoBehaviour
    {
        protected abstract JobId Job { get; }

        protected JobContext Context { get; private set; }
        protected ShiftData Shift { get; private set; }
        protected JobDialogue Lines { get; private set; }
        protected JobDayDialogue DayLines { get; private set; }

        ItemQueue queue;
        bool running;
        float timeLeft;
        float spawnTimer;
        float progressTimer;
        int complaints;

        protected bool Running => running;

        // ---------- What a job script provides ----------

        /// <summary>Draw the workplace once, when the job scene loads.</summary>
        protected abstract void BuildScene();

        /// <summary>Show a newly arrived item; call OpenInspect(item) when the player taps it.</summary>
        protected abstract void SpawnItem(ItemData item);

        /// <summary>World position of an item's visual (where a tutorial hint points).</summary>
        protected abstract Vector3 ItemPosition(ItemData item);

        /// <summary>Animate the item away: allowed = let through (dock, let in, open, fit).</summary>
        protected abstract void OnItemResolved(ItemData item, bool allowed);

        /// <summary>Remove every item visual (before a shift starts or restarts).</summary>
        protected abstract void ClearItems();

        /// <summary>The inspect panel's fields for an item.</summary>
        protected abstract InspectField[] InspectFields(ItemData item);

        // ---------- Lifecycle ----------

        protected virtual void Start()
        {
            var game = GameManager.Instance;
            var day = Services.Flow.Day;
            Context = game != null && game.CurrentContext.job == Job
                ? game.CurrentContext
                : new JobContext { job = Job, day = day, isTutorial = day == 1, timerSeconds = 45f };

            var data = DataLoader.Instance;
            Shift = data?.GetShift(Context.day, Job) ?? new ShiftData { job = Job.ToKey(), items = new ItemData[0] };
            Lines = data?.GetJobDialogue(Job);
            DayLines = data?.GetJobDayDialogue(Job, Context.day);

            BuildScene();
            if (Services.Flow.State == GameState.JobShift) StartShift();
        }

        protected virtual void OnEnable() => GameEvents.StateChanged += OnStateChanged;

        protected virtual void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            if (running) Services.Inspect.Hide();
        }

        void OnStateChanged(GameState from, GameState to)
        {
            // JobIntro -> JobShift starts the shift; Snatched -> JobShift restarts it with the same data.
            if (to == GameState.JobShift) StartShift();
            else if (to != GameState.JobShift && running) running = false;
        }

        void StartShift()
        {
            ClearItems();
            Services.Inspect.Hide();
            queue = new ItemQueue(Shift.items);
            complaints = 0;
            timeLeft = Context.timerSeconds > 0f ? Context.timerSeconds : 45f;
            spawnTimer = 0f;
            progressTimer = 0f;
            running = true;
            Debug.Log($"[{Job}] Shift start: day {Context.day}, {queue.Total} item(s), {(Context.isTutorial ? "tutorial, no timer" : $"{timeLeft:0}s")}");
            RaiseProgress();
        }

        protected virtual void Update()
        {
            if (!running) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && queue.HasPending)
            {
                SpawnItem(queue.Next());
                spawnTimer = Shift.spawnIntervalSeconds > 0f ? Shift.spawnIntervalSeconds : 4f;
            }

            // The tutorial has no timer: a learner can take as long as they need.
            if (!Context.isTutorial)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0f)
                {
                    TimeUp();
                    return;
                }
            }

            progressTimer -= Time.deltaTime;
            if (progressTimer <= 0f) RaiseProgress();
        }

        // ---------- Called by job scripts ----------

        protected void OpenInspect(ItemData item)
        {
            if (!running || queue.IsDone(item)) return;
            var data = new InspectData
            {
                title = item.title,
                fields = InspectFields(item),
                primaryLabel = Lines?.primaryAction,
                secondaryLabel = Lines?.secondaryAction,
            };
            Services.Inspect.Show(data, () => Resolve(item, true), () => Resolve(item, false));
        }

        protected string FieldLabel(int index, string fallback) =>
            Lines?.fieldLabels != null && index < Lines.fieldLabels.Length ? Lines.fieldLabels[index] : fallback;

        // ---------- Rules ----------

        void Resolve(ItemData item, bool allowed)
        {
            if (!running || queue.IsDone(item)) return;
            var correct = allowed != item.IsAttack;
            GameEvents.RaiseItemResolved(new ItemResolvedInfo { job = Job, itemId = item.id, allowed = allowed, correct = correct });

            if (correct)
            {
                queue.MarkDone(item);
                OnItemResolved(item, allowed);
                RaiseProgress();
                if (queue.AllDone) Succeed();
                return;
            }

            if (Context.isTutorial)
            {
                // Day 1: point at what was off; the item stays for another try. Never a backup.
                var hint = !string.IsNullOrEmpty(item.hint) ? item.hint : DayLines?.complaintLine;
                if (!string.IsNullOrEmpty(hint)) Services.Dialogue.ShowHint(Job.ToKey(), hint, ItemPosition(item), () => { });
                return;
            }

            if (allowed)
            {
                // An attack let through: the attacker gets in.
                Fail(item.attacker);
                return;
            }

            // A safe item blocked: it's gone, and it costs a complaint.
            queue.MarkDone(item);
            OnItemResolved(item, false);
            complaints++;
            GameEvents.RaiseComplaint(new ComplaintInfo { source = SnatchSource.JobShift, count = complaints, max = ComplaintRules.Limit(Shift.maxWrongBlocks) });
            if (ComplaintRules.ShiftFailed(complaints, Shift.maxWrongBlocks))
            {
                Fail("complaints");
                return;
            }
            if (!string.IsNullOrEmpty(DayLines?.complaintLine)) Services.Dialogue.Say(Job.ToKey(), DayLines.complaintLine);
            RaiseProgress();
            if (queue.AllDone) Succeed();
        }

        void TimeUp()
        {
            // Time ran out: any attack not handled yet counts as let through.
            var attack = queue.FirstUnhandledAttack();
            if (attack != null) Fail(attack.attacker);
            else Succeed();
        }

        void Succeed()
        {
            running = false;
            Services.Inspect.Hide();
            Debug.Log($"[{Job}] Shift succeeded");
            GameEvents.RaiseShiftSucceeded(Job);
        }

        void Fail(string attacker)
        {
            running = false;
            Services.Inspect.Hide();
            Debug.Log($"[{Job}] Shift failed: {attacker}");
            Services.Backups.ReportSnatch(new SnatchInfo { source = SnatchSource.JobShift, job = Job, attacker = attacker });
        }

        void RaiseProgress()
        {
            progressTimer = 0.25f;
            GameEvents.RaiseShiftProgress(new ShiftProgressInfo
            {
                job = Job,
                handled = queue.DoneCount,
                total = queue.Total,
                secondsLeft = Context.isTutorial ? -1f : Mathf.Max(0f, timeLeft),
            });
        }
    }
}
