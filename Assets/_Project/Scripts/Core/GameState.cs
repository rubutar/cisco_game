// SHARED CONTRACT. Change only through a contract-change PR to main (see CLAUDE.md).
namespace ByteWorld
{
    /// <summary>Top-level game states (PRD 6.4). Owned by GameManager (Stream A).</summary>
    public enum GameState
    {
        Boot,
        Title,
        Intro,
        WorldPeace,
        Alert,
        JobIntro,
        JobShift,
        JobSuccess,
        Snatched,
        Collapse,
        DayEnd,
        DayStart,
        BuildIntro,
        Build,
        TestRun,
        Reveal,
        FinalAttack,
        Peace
    }

    /// <summary>The four job Bytes. JSON uses lower-case strings; convert with <see cref="JobIds"/>.</summary>
    public enum JobId
    {
        Scout,
        Gigi,
        Twins,
        Patch
    }

    /// <summary>A day's mode, from days.json "mode".</summary>
    public enum DayMode
    {
        Tutorial,
        Shifts,
        Build
    }

    /// <summary>Where a snatch came from: a Day 1–2 shift or a Day 3 test run leak.</summary>
    public enum SnatchSource
    {
        JobShift,
        TestRun
    }

    /// <summary>What happened when a snatch was reported to the backup nest.</summary>
    public enum SnatchResult
    {
        BackupUsed,
        Collapsed
    }

    /// <summary>Outcome of one Day 3 test-run packet (PRD 4.10).</summary>
    public enum PacketOutcome
    {
        Delivered,
        Lost,
        Blocked,
        Leaked
    }

    /// <summary>
    /// Every button the player can press on a screen. UI raises
    /// <see cref="GameEvents.RaiseUiAction"/>; GameManager decides what happens next.
    /// </summary>
    public enum UiAction
    {
        Play,
        IntroNext,
        IntroSkip,
        JobIntroDone,
        BackToWorld,
        KeepWatching,
        Sleep,
        StartDay,
        StartAgain,
        LetsBuild,
        Run,
        FixBuild,
        RevealNext,
        PlayAgain,
        Pause,
        Resume
    }

    public static class JobIds
    {
        public static string ToKey(this JobId job) => job switch
        {
            JobId.Scout => "scout",
            JobId.Gigi => "gigi",
            JobId.Twins => "twins",
            _ => "patch"
        };

        public static bool TryParse(string key, out JobId job)
        {
            switch (key)
            {
                case "scout": job = JobId.Scout; return true;
                case "gigi": job = JobId.Gigi; return true;
                case "twins": job = JobId.Twins; return true;
                case "patch": job = JobId.Patch; return true;
                default: job = JobId.Scout; return false;
            }
        }
    }

    public static class DayModes
    {
        public static DayMode Parse(string mode) => mode switch
        {
            "shifts" => DayMode.Shifts,
            "build" => DayMode.Build,
            _ => DayMode.Tutorial
        };
    }
}
