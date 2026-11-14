// FOUNDATION. Checks the JSON in Resources/Data against the shared data classes.
// Menu: Byte World > Validate Data. Batch mode: -executeMethod ByteWorld.EditorTools.DataValidator.RunBatch
using System.Collections.Generic;
using System.Linq;
using ByteWorld.Data;
using ByteWorld.Stubs;
using UnityEditor;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    public static class DataValidator
    {
        const int MaxLine = 110;
        const int MaxReveal = 140;

        [MenuItem("Byte World/Validate Data")]
        public static void Run() => Validate();

        public static void RunBatch() => EditorApplication.Exit(Validate() == 0 ? 0 : 1);

        static int Validate()
        {
            var problems = new List<string>();
            var days = ResourcesGameData.Load<DaysFile>("days");
            var dialogue = ResourcesGameData.Load<DialogueFile>("dialogue");
            var roaming = ResourcesGameData.Load<RoamingFile>("roaming");
            var build = ResourcesGameData.Load<BuildFile>("build");
            var flags = ResourcesGameData.Load<FlagsFile>("flags");

            var ids = new HashSet<string>();
            foreach (var day in days.days ?? new DayData[0])
            {
                foreach (var group in day.alertGroups ?? new AlertGroup[0])
                    foreach (var job in group.jobs ?? new string[0])
                        if (!JobIds.TryParse(job, out _)) problems.Add($"days: day {day.day} alert group has unknown job '{job}'");

                foreach (var shift in day.shifts ?? new ShiftData[0])
                {
                    if (!JobIds.TryParse(shift.job, out _)) problems.Add($"days: day {day.day} shift has unknown job '{shift.job}'");
                    foreach (var item in shift.items ?? new ItemData[0])
                    {
                        if (string.IsNullOrEmpty(item.id) || !ids.Add(item.id)) problems.Add($"days: missing or duplicate item id '{item.id}'");
                        if (item.kind != "safe" && item.kind != "attack") problems.Add($"days: {item.id} kind must be safe or attack");
                        if (item.IsAttack && string.IsNullOrEmpty(item.attacker)) problems.Add($"days: {item.id} is an attack with no attacker");
                        if (day.mode == "tutorial" && item.IsAttack && string.IsNullOrEmpty(item.hint)) problems.Add($"days: Day 1 attack {item.id} has no hint");
                        CheckLength(problems, $"days {item.id} hint", item.hint, MaxLine);
                    }
                }
            }

            foreach (var job in dialogue.jobs ?? new JobDialogue[0])
            {
                CheckLength(problems, $"dialogue {job.job} jobLine", job.jobLine, MaxLine);
                foreach (var d in job.days ?? new JobDayDialogue[0])
                {
                    var where = $"dialogue {job.job} day {d.day}";
                    foreach (var line in new[] { d.alertLine, d.successLine, d.snatchLine, d.backupLine, d.waitingLine, d.complaintLine, d.complaintFailLine })
                        CheckLength(problems, where, line, MaxLine);
                    foreach (var line in d.introLines ?? new string[0]) CheckLength(problems, where, line, MaxLine);
                }
            }

            foreach (var line in roaming.lines ?? new string[0]) CheckLength(problems, "roaming", line, MaxLine);

            var trafficIds = new HashSet<string>();
            foreach (var puzzle in build.puzzles ?? new PuzzleData[0])
            {
                foreach (var packet in puzzle.traffic ?? new TrafficData[0])
                {
                    if (string.IsNullOrEmpty(packet.id) || !trafficIds.Add(packet.id)) problems.Add($"build: missing or duplicate traffic id '{packet.id}'");
                    if (packet.IsAttack && string.IsNullOrEmpty(packet.attacker)) problems.Add($"build: {packet.id} is an attack with no attacker");
                }
                if (puzzle.reveal != null) CheckLength(problems, $"build puzzle {puzzle.id} reveal", puzzle.reveal.text, MaxReveal);
            }

            if (flags.day3Mode != "build" && flags.day3Mode != "fallback") problems.Add($"flags: day3Mode must be build or fallback, not '{flags.day3Mode}'");

            Debug.Log($"[DataValidator] days {days.days?.Length ?? 0}, items {ids.Count}, dialogue jobs {dialogue.jobs?.Length ?? 0}, " +
                      $"roaming lines {roaming.lines?.Length ?? 0}, puzzles {build.puzzles?.Length ?? 0}, day3Mode {flags.day3Mode}");
            foreach (var problem in problems) Debug.LogError($"[DataValidator] {problem}");
            if (problems.Count == 0) Debug.Log("[DataValidator] All data valid.");
            return problems.Count;
        }

        static void CheckLength(List<string> problems, string where, string text, int max)
        {
            if (text != null && text.Length > max) problems.Add($"{where}: {text.Length} characters (max {max}): \"{text}\"");
        }
    }
}
