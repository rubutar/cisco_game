// SHARED CONTRACT. Change only through a contract-change PR to main (see CLAUDE.md).
using System;
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    /// <summary>
    /// Three shared backups for the whole game (PRD 4.5). Implemented by Stream A (BackupNest).
    /// The ONLY way to use a backup: job shifts and Day 3 test-run leaks both call ReportSnatch.
    /// </summary>
    public interface IBackupNest
    {
        int Left { get; }
        int Max { get; }

        /// <summary>
        /// Uses a backup if one is left (raises Snatched and BackupsChanged, returns BackupUsed),
        /// otherwise raises Collapsed and returns Collapsed. Never call it for Day 1 tutorial mistakes.
        /// </summary>
        SnatchResult ReportSnatch(SnatchInfo info);

        /// <summary>Back to Max (new game, or restart a day after a collapse). Raises BackupsChanged.</summary>
        void Refill();
    }

    /// <summary>Loaded JSON content. Implemented by Stream A (DataLoader).</summary>
    public interface IGameData
    {
        DaysFile Days { get; }
        DialogueFile Dialogue { get; }
        RoamingFile Roaming { get; }
        BuildFile Build { get; }
        FlagsFile Flags { get; }
        HuntFile Hunt { get; }
        CheckpointsFile Checkpoints { get; }
    }

    /// <summary>
    /// The seven stations (PRD-v2 4.2a, 6.4). Implemented by Person A (CheckpointManager). UI re-reads it on
    /// GameEvents.CheckpointStatesChanged.
    /// </summary>
    public interface ICheckpointService
    {
        CheckpointData[] All { get; }
        CheckpointData Get(string id);
        CheckpointState StateOf(string id);
        /// <summary>"" when open; otherwise the lock line, e.g. "Opens after Quest 1".</summary>
        string LockReason(string id);
        int ToolsCollected { get; }
        int ToolsTotal { get; }
    }

    /// <summary>
    /// The Splunk Trace screen (PRD-v2 4.6). Implemented by Person A (UI/Hunt). Begin shows it; onPinned
    /// receives the pinned row as "srcIp:destPort" (e.g. "203.0.113.7:8080") when the player pins the target.
    /// </summary>
    public interface IHuntScreen
    {
        void Begin(Action<string> onPinned);
    }

    /// <summary>Game flow, read by UI and debug tools. Implemented by Stream A (GameManager).</summary>
    public interface IGameFlow
    {
        GameState State { get; }
        int Day { get; }
        DayMode Mode { get; }

        void StartNewGame();

        /// <summary>Debug and demo shortcuts: ?day=2, ?day=3&amp;puzzle=3. puzzle is 1-based; ignored before Day 3.</summary>
        void StartAt(int day, int puzzle);
    }

    /// <summary>Speech bubbles. Implemented by Stream B (DialogueBubble). Text comes from dialogue.json.</summary>
    public interface IDialogueService
    {
        bool IsShowing { get; }

        /// <summary>speaker is a job key ("scout") or "" for narration; onDone runs after the tap to continue.</summary>
        void Say(string speaker, string text, Action onDone = null);

        /// <summary>Shows lines one after another; onDone runs after the last one.</summary>
        void SayLines(string speaker, IReadOnlyList<string> lines, Action onDone = null);

        /// <summary>Day 1 tutorial hint (screen T1): bubble pointing at worldPosition with a "Try again" button.</summary>
        void ShowHint(string speaker, string text, Vector3 worldPosition, Action onRetry);

        void Hide();
    }

    public struct InspectField
    {
        public string label;
        public string value;

        public InspectField(string label, string value)
        {
            this.label = label;
            this.value = value;
        }
    }

    public struct InspectData
    {
        public string title;
        public InspectField[] fields;  // up to 4
        public string primaryLabel;    // the "let it through" action, e.g. "Let it dock"
        public string secondaryLabel;  // the "block" action, e.g. "Send away"
    }

    /// <summary>The inspect panel used by all four jobs (PRD 4.6). Implemented by Stream B (InspectPanel).</summary>
    public interface IInspectPanel
    {
        bool IsOpen { get; }
        void Show(InspectData data, Action onPrimary, Action onSecondary);
        void Hide();
    }

    /// <summary>
    /// Day 3 build mode (PRD 4.10). Implemented by Stream B (BuildManager); started by Stream A's GameManager.
    /// Build mode raises PuzzleStarted, TestRunFinished, PuzzlePassed and RevealClosed. After RevealClosed for
    /// the last puzzle, GameManager moves to Peace. Leaks go through IBackupNest.ReportSnatch.
    /// </summary>
    public interface IBuildMode
    {
        /// <summary>startPuzzle is 1-based. Puzzles before it count as already solved.</summary>
        void Begin(int startPuzzle);
        void End();
    }

    /// <summary>The complaints rule shared by job shifts (Stream A) and test runs (Stream B), PRD 4.5.</summary>
    public static class ComplaintRules
    {
        public const int DefaultMaxWrongBlocks = 3;

        public static int Limit(int maxWrongBlocks) => maxWrongBlocks > 0 ? maxWrongBlocks : DefaultMaxWrongBlocks;

        /// <summary>A shift fails when complaints reach the limit. Not used on Day 1.</summary>
        public static bool ShiftFailed(int complaints, int maxWrongBlocks) => complaints >= Limit(maxWrongBlocks);

        /// <summary>A test run passes with at most maxLeaks leaks and fewer complaints than the limit.</summary>
        public static bool RunPasses(int leaks, int complaints, PassRule pass) =>
            leaks <= (pass?.maxLeaks ?? 0) && complaints < Limit(pass?.maxWrongBlocks ?? 0);
    }
}
