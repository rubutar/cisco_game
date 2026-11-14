// FOUNDATION. Editor window that fires any shared event with sample data, so each stream can test
// its side without the other. Open it from Byte World > Fake Event Panel, then enter Play mode.
using UnityEditor;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    public class FakeEventPanel : EditorWindow
    {
        JobId job = JobId.Scout;
        int day = 1;
        int puzzle = 1;
        int complaints = 1;
        Vector2 scroll;

        [MenuItem("Byte World/Fake Event Panel")]
        static void Open() => GetWindow<FakeEventPanel>("Fake Events");

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to fire events.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            job = (JobId)EditorGUILayout.EnumPopup("Job", job);
            day = EditorGUILayout.IntSlider("Day", day, 1, 3);
            puzzle = EditorGUILayout.IntSlider("Puzzle", puzzle, 1, 3);
            complaints = EditorGUILayout.IntSlider("Complaints", complaints, 0, 3);

            EditorGUILayout.LabelField("Services", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Flow: {Describe<IGameFlow>()}  State: {Services.Flow.State}  Day: {Services.Flow.Day}");
            EditorGUILayout.LabelField($"Backups: {Describe<IBackupNest>()}  {Services.Backups.Left}/{Services.Backups.Max}");
            EditorGUILayout.LabelField($"Dialogue: {Describe<IDialogueService>()}  Inspect: {Describe<IInspectPanel>()}  Build: {Describe<IBuildMode>()}");

            EditorGUILayout.LabelField("Flow", EditorStyles.boldLabel);
            if (GUILayout.Button("StartAt(day, puzzle)")) Services.Flow.StartAt(day, puzzle);
            if (GUILayout.Button("DayStarted")) GameEvents.RaiseDayStarted(day, DayModes.Parse(day == 1 ? "tutorial" : day == 2 ? "shifts" : "build"));
            if (GUILayout.Button("DayEnded")) GameEvents.RaiseDayEnded(day);
            if (GUILayout.Button("Alert (job)")) GameEvents.RaiseAlert(new AlertInfo { jobs = new[] { job }, groupIndex = 0, groupCount = 4 });
            if (GUILayout.Button("Double alert (Scout + Gigi)")) GameEvents.RaiseAlert(new AlertInfo { jobs = new[] { JobId.Scout, JobId.Gigi }, groupIndex = 0, groupCount = 2 });
            if (GUILayout.Button("JobEntered")) GameEvents.RaiseJobEntered(new JobContext { job = job, day = day, isTutorial = day == 1, timerSeconds = 45 });
            if (GUILayout.Button("ShiftProgress 4 of 8")) GameEvents.RaiseShiftProgress(new ShiftProgressInfo { job = job, handled = 4, total = 8, secondsLeft = 20 });
            if (GUILayout.Button("ShiftSucceeded")) GameEvents.RaiseShiftSucceeded(job);
            if (GUILayout.Button("PeaceReached")) GameEvents.RaisePeaceReached();

            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
            if (GUILayout.Button("Complaint")) GameEvents.RaiseComplaint(new ComplaintInfo { source = SnatchSource.JobShift, count = complaints, max = 3 });
            if (GUILayout.Button("Snatch (through IBackupNest)")) Services.Backups.ReportSnatch(new SnatchInfo { source = SnatchSource.JobShift, job = job, attacker = "phisher" });
            if (GUILayout.Button("Refill backups")) Services.Backups.Refill();

            EditorGUILayout.LabelField("Day 3", EditorStyles.boldLabel);
            if (GUILayout.Button("PuzzleStarted")) GameEvents.RaisePuzzleStarted(puzzle, puzzle, 3);
            if (GUILayout.Button("TestRunFinished (fail: 1 leak)")) GameEvents.RaiseTestRunFinished(new RunResult { puzzleId = puzzle, delivered = 6, leaked = 1 });
            if (GUILayout.Button("TestRunFinished (pass)")) GameEvents.RaiseTestRunFinished(new RunResult { puzzleId = puzzle, delivered = 7, passed = true });
            if (GUILayout.Button("PuzzlePassed")) GameEvents.RaisePuzzlePassed(puzzle);
            if (GUILayout.Button("RevealClosed")) GameEvents.RaiseRevealClosed(puzzle);

            EditorGUILayout.LabelField("UI requests", EditorStyles.boldLabel);
            foreach (UiAction action in System.Enum.GetValues(typeof(UiAction)))
                if (GUILayout.Button($"UiAction.{action}")) GameEvents.RaiseUiAction(action);
            if (GUILayout.Button("RestartDayRequested")) GameEvents.RaiseRestartDay();
            if (GUILayout.Button("DebugMenuRequested")) GameEvents.RaiseDebugMenu();

            EditorGUILayout.EndScrollView();
        }

        static string Describe<T>() where T : class => Services.IsStub<T>() ? "stub" : "real";

        void OnInspectorUpdate() => Repaint();
    }
}
