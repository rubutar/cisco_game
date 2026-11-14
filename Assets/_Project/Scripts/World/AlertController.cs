// Stream A. Runs a day's alert groups (A-08, PRD 4.4 and 4.7): after alertDelaySeconds every Byte in the next
// group flashes; tapping one enters its job with the time it waited (Day 2 wait penalty, PRD 4.7). Bytes of the
// group still waiting keep flashing; when the whole group is done, the next group follows; after the last
// group the day ends. Built for groups from the start: Day 1 uses groups of one, Day 2 pairs.
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public class AlertController : MonoBehaviour
    {
        public static AlertController Instance { get; private set; }

        readonly Dictionary<JobId, float> waitingSince = new();
        AlertGroup[] groups;
        DayData dayData;
        int startedDay = -1;
        int groupIndex;
        float nextAlertAt = -1f;
        bool groupActive;

        /// <summary>Seconds the job has been flashing (for the Day 2 patience bar); 0 when not waiting.</summary>
        public float WaitedSeconds(JobId job) => waitingSince.TryGetValue(job, out var t) ? Time.time - t : 0f;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;

        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void Update()
        {
            UpdatePatience();
            var flow = Services.Flow;
            // FinalAttack is the fallback Day 3: the same groups, from DataLoader's fallback day.
            if (flow.State != GameState.WorldPeace && flow.State != GameState.FinalAttack) return;

            // A new day (or a restarted one) begins as soon as the world is at peace on it.
            if (startedDay != flow.Day) BeginDay(flow.Day);

            if (!groupActive && nextAlertAt >= 0f && Time.time >= nextAlertAt) RaiseNextGroup();
        }

        /// <summary>
        /// Day 2 patience bars (A-15): in a group of two or more, each waiting Byte's bar shows how much of its
        /// shift timer is left after the wait penalty, from full down to the maxWaitPenalty cap.
        /// </summary>
        void UpdatePatience()
        {
            if (!groupActive || dayData == null || dayData.maxWaitPenalty <= 0f || (groups[groupIndex].jobs?.Length ?? 1) < 2) return;
            foreach (var job in waitingSince.Keys)
            {
                var shift = DataLoader.Instance?.GetShift(startedDay, job);
                var timer = shift != null && shift.timerSeconds > 0f ? shift.timerSeconds : 45f;
                var maxPenalty = timer * dayData.maxWaitPenalty;
                var penalty = Mathf.Min(WaitedSeconds(job) * dayData.waitPenaltyPerSecond, maxPenalty);
                WorldMap.Instance?.GetJobByte(job)?.SetPatience(1f - penalty / maxPenalty);
            }
        }

        void BeginDay(int day)
        {
            startedDay = day;
            groupIndex = 0;
            groupActive = false;
            waitingSince.Clear();
            dayData = DataLoader.Instance?.GetDay(day);
            groups = dayData?.alertGroups ?? new AlertGroup[0];
            nextAlertAt = groups.Length > 0 ? Time.time + Mathf.Max(1f, dayData.alertDelaySeconds) : -1f;
            Debug.Log($"[AlertController] Day {day}: {groups.Length} alert group(s), first in {dayData?.alertDelaySeconds ?? 0}s");
        }

        void RaiseNextGroup()
        {
            var jobs = new List<JobId>();
            foreach (var key in groups[groupIndex].jobs ?? new string[0])
                if (JobIds.TryParse(key, out var job)) jobs.Add(job);

            groupActive = true;
            foreach (var job in jobs)
            {
                waitingSince[job] = Time.time;
                WorldMap.Instance?.GetJobByte(job)?.SetAlerting(true);
            }
            GameEvents.RaiseAlert(new AlertInfo { jobs = jobs.ToArray(), groupIndex = groupIndex, groupCount = groups.Length });
        }

        /// <summary>
        /// Go in at a station (PRD-v2): enter the job if it's flashing; if its group hasn't been raised yet,
        /// raise it now (the player walked there before the alert fired) and enter.
        /// </summary>
        public void TryEnter(JobId job)
        {
            var flow = Services.Flow;
            if (startedDay != flow.Day && (flow.State == GameState.WorldPeace || flow.State == GameState.FinalAttack)) BeginDay(flow.Day);
            if (!waitingSince.ContainsKey(job) && !groupActive && groups != null && groupIndex < groups.Length)
            {
                var key = job.ToKey();
                foreach (var k in groups[groupIndex].jobs ?? new string[0])
                    if (k == key) { RaiseNextGroup(); break; }
            }
            OnJobTapped(job);
        }

        public void OnJobTapped(JobId job)
        {
            var state = Services.Flow.State;
            if (!waitingSince.ContainsKey(job) || (state != GameState.Alert && state != GameState.WorldPeace && state != GameState.FinalAttack)) return;

            var waited = WaitedSeconds(job);
            waitingSince.Remove(job);
            WorldMap.Instance?.GetJobByte(job)?.SetAlerting(false);

            var shift = DataLoader.Instance?.GetShift(startedDay, job);
            var timer = shift != null && shift.timerSeconds > 0f ? shift.timerSeconds : 45f;
            // Only a Byte left waiting while another was helped gets the penalty (never more than maxWaitPenalty).
            var groupSize = groups[groupIndex].jobs?.Length ?? 1;
            if (groupSize > 1 && dayData != null)
                timer -= Mathf.Min(waited * dayData.waitPenaltyPerSecond, timer * dayData.maxWaitPenalty);

            GameManager.Instance.EnterJob(new JobContext
            {
                job = job,
                day = startedDay,
                isTutorial = DayModes.Parse(dayData?.mode) == DayMode.Tutorial,
                timerSeconds = timer,
                waitedSeconds = waited,
            });
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.WorldPeace || from != GameState.JobSuccess || !groupActive) return;

            if (waitingSince.Count > 0)
            {
                // Others in the group are still flashing: back to the alert.
                var waiting = new List<JobId>(waitingSince.Keys);
                GameEvents.RaiseAlert(new AlertInfo { jobs = waiting.ToArray(), groupIndex = groupIndex, groupCount = groups.Length });
                return;
            }

            groupActive = false;
            groupIndex++;
            if (groupIndex < groups.Length)
            {
                nextAlertAt = Time.time + Mathf.Max(1f, dayData.alertDelaySeconds);
            }
            else
            {
                nextAlertAt = -1f;
                Debug.Log($"[AlertController] Day {startedDay}: all alert groups done");
                // The fallback final attack ends the game in peace; any other day ends with its day card.
                if (Services.Flow.Mode == DayMode.Build) GameManager.Instance.FinishFinalAttack();
                // v2: the day-end card waits until the quest's stations are done (Day 2: the Splunk Observatory).
                else if (CheckpointManager.Instance != null) CheckpointManager.Instance.TryEndDay(startedDay);
                else GameEvents.RaiseDayEnded(startedDay);
            }
        }
    }
}
