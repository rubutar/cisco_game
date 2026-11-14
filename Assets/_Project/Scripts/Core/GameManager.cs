// Stream A. The game's state machine (PRD 6.4) and scene loader. Lives on Resources/Core/GameRoot.prefab,
// which Bootstrap spawns before the first scene, and registers itself as IGameFlow.
//
// Flow: Title -> Intro -> WorldPeace -> Alert -> JobIntro -> JobShift -> (JobSuccess | Snatched) -> ...
//       -> DayEnd -> DayStart -> ... -> BuildIntro -> Build -> TestRun -> (Reveal -> Build ... ) -> Peace,
//       with Collapse reachable from any snatch, and FinalAttack replacing the Day 3 states in the fallback.
//
// GameManager only moves between states and loads scenes. Other Stream A systems decide *when*:
// AlertController raises AlertRaised and calls EnterJob, JobShift raises ShiftSucceeded, DayManager raises
// DayEnded, BackupNest raises Snatched/Collapsed. Stream B's screens send button presses as UiActions.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace ByteWorld
{
    public class GameManager : MonoBehaviour, IGameFlow
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public int Day { get; private set; } = 1;
        public DayMode Mode { get; private set; } = DayMode.Tutorial;
        public JobId? CurrentJob { get; private set; }
        public int CurrentPuzzle { get; private set; } = 1;

        string loadedJobScene;

        // Expected transitions. Anything else still happens but logs a warning, to catch wiring bugs early.
        static readonly Dictionary<GameState, GameState[]> Expected = new()
        {
            { GameState.Boot, new[] { GameState.Title, GameState.WorldPeace, GameState.BuildIntro, GameState.Build } },
            { GameState.Title, new[] { GameState.Intro, GameState.WorldPeace, GameState.BuildIntro, GameState.FinalAttack } },
            { GameState.Intro, new[] { GameState.WorldPeace } },
            { GameState.WorldPeace, new[] { GameState.Alert, GameState.DayEnd } },
            { GameState.Alert, new[] { GameState.JobIntro, GameState.FinalAttack } },
            { GameState.JobIntro, new[] { GameState.JobShift } },
            { GameState.JobShift, new[] { GameState.JobSuccess, GameState.Snatched, GameState.Collapse } },
            { GameState.JobSuccess, new[] { GameState.WorldPeace } },
            { GameState.Snatched, new[] { GameState.JobShift, GameState.Collapse } },
            { GameState.Collapse, new[] { GameState.WorldPeace, GameState.BuildIntro, GameState.FinalAttack } },
            { GameState.DayEnd, new[] { GameState.DayStart } },
            { GameState.DayStart, new[] { GameState.WorldPeace, GameState.BuildIntro, GameState.FinalAttack } },
            { GameState.BuildIntro, new[] { GameState.Build } },
            { GameState.Build, new[] { GameState.TestRun } },
            { GameState.TestRun, new[] { GameState.Build, GameState.Reveal, GameState.Collapse } },
            { GameState.Reveal, new[] { GameState.Build, GameState.Peace } },
            { GameState.FinalAttack, new[] { GameState.JobIntro, GameState.Peace, GameState.Collapse } },
            { GameState.Peace, new[] { GameState.Title } },
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Services.Register<IGameFlow>(this);
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Services.Unregister<IGameFlow>(this);
        }

        void OnEnable()
        {
            GameEvents.UiActionRequested += OnUiAction;
            GameEvents.RestartDayRequested += RestartDay;
            GameEvents.AlertRaised += OnAlertRaised;
            GameEvents.ShiftSucceeded += OnShiftSucceeded;
            GameEvents.Snatched += OnSnatched;
            GameEvents.Collapsed += OnCollapsed;
            GameEvents.DayEnded += OnDayEnded;
            GameEvents.TestRunFinished += OnTestRunFinished;
            GameEvents.PuzzlePassed += OnPuzzlePassed;
            GameEvents.RevealClosed += OnRevealClosed;
        }

        void OnDisable()
        {
            GameEvents.UiActionRequested -= OnUiAction;
            GameEvents.RestartDayRequested -= RestartDay;
            GameEvents.AlertRaised -= OnAlertRaised;
            GameEvents.ShiftSucceeded -= OnShiftSucceeded;
            GameEvents.Snatched -= OnSnatched;
            GameEvents.Collapsed -= OnCollapsed;
            GameEvents.DayEnded -= OnDayEnded;
            GameEvents.TestRunFinished -= OnTestRunFinished;
            GameEvents.PuzzlePassed -= OnPuzzlePassed;
            GameEvents.RevealClosed -= OnRevealClosed;
        }

        void Start()
        {
            // Pressing Play in any scene works: Boot starts at the title, World and Build start that day directly.
            switch (SceneManager.GetActiveScene().name)
            {
                case SceneNames.World:
                    BeginDay(1, loadScene: false);
                    break;
                case SceneNames.Build:
                    BeginDay(3, loadScene: false);
                    break;
                default:
                    SetState(GameState.Title);
                    break;
            }
        }

        // ---------- IGameFlow ----------

        public void StartNewGame() => StartAt(1, 1);

        public void StartAt(int day, int puzzle)
        {
            CurrentPuzzle = Mathf.Max(1, puzzle);
            Services.Backups.Refill();
            BeginDay(Mathf.Clamp(day, 1, 3), loadScene: true);
        }

        // ---------- Called by other Stream A systems ----------

        /// <summary>AlertController calls this when the player taps an alerting Byte.</summary>
        public void EnterJob(JobContext context)
        {
            CurrentJob = context.job;
            UnloadJobScene();
            loadedJobScene = SceneNames.ForJob(context.job);
            SceneManager.LoadSceneAsync(loadedJobScene, LoadSceneMode.Additive);
            SetState(GameState.JobIntro);
            GameEvents.RaiseJobEntered(context);
        }

        /// <summary>Restart the current day with three backups (PRD 4.5). A-17 owns refining this.</summary>
        public void RestartDay()
        {
            CurrentPuzzle = 1;
            Services.Backups.Refill();
            BeginDay(Day, loadScene: true);
        }

        // ---------- Event handlers ----------

        void OnUiAction(UiAction action)
        {
            switch (action)
            {
                case UiAction.Play:
                    if (State == GameState.Title) SetState(GameState.Intro);
                    break;
                // Stream B pages through the intro panels itself; either action means the intro is over.
                case UiAction.IntroNext:
                case UiAction.IntroSkip:
                    if (State == GameState.Intro) StartNewGame();
                    break;
                case UiAction.JobIntroDone:
                    if (State == GameState.JobIntro) SetState(GameState.JobShift);
                    break;
                case UiAction.KeepWatching:
                    if (State == GameState.Snatched) SetState(GameState.JobShift);
                    break;
                case UiAction.BackToWorld:
                    if (State == GameState.JobSuccess) ReturnToWorld();
                    break;
                case UiAction.Sleep:
                    if (State == GameState.DayEnd) StartNextDay();
                    break;
                case UiAction.StartDay:
                    if (State == GameState.DayStart) EnterDay();
                    break;
                case UiAction.StartAgain:
                    if (State == GameState.Collapse) RestartDay();
                    break;
                case UiAction.LetsBuild:
                    if (State == GameState.BuildIntro) BeginBuild();
                    break;
                case UiAction.Run:
                    if (State == GameState.Build) SetState(GameState.TestRun);
                    break;
                case UiAction.FixBuild:
                    if (State == GameState.TestRun) SetState(GameState.Build);
                    break;
                case UiAction.PlayAgain:
                    if (State == GameState.Peace) GoToTitle();
                    break;
                case UiAction.Pause:
                    Time.timeScale = 0f;
                    break;
                case UiAction.Resume:
                    Time.timeScale = 1f;
                    break;
            }
        }

        void OnAlertRaised(AlertInfo info)
        {
            if (State == GameState.WorldPeace) SetState(GameState.Alert);
        }

        void OnShiftSucceeded(JobId job)
        {
            if (State == GameState.JobShift) SetState(GameState.JobSuccess);
        }

        void OnSnatched(SnatchInfo info)
        {
            // Day 3 leaks are shown by build mode's result screen; only shifts get the snatch screen.
            if (info.source == SnatchSource.JobShift && State == GameState.JobShift) SetState(GameState.Snatched);
        }

        void OnCollapsed(int day)
        {
            UnloadJobScene();
            SetState(GameState.Collapse);
        }

        void OnDayEnded(int day)
        {
            if (State == GameState.WorldPeace) SetState(GameState.DayEnd);
        }

        void OnTestRunFinished(RunResult result)
        {
            // A failed run waits for UiAction.FixBuild; a pass waits for PuzzlePassed.
        }

        void OnPuzzlePassed(int puzzleId)
        {
            if (State == GameState.TestRun) SetState(GameState.Reveal);
        }

        void OnRevealClosed(int puzzleId)
        {
            if (State != GameState.Reveal) return;
            var count = Services.Data.Build?.puzzles?.Length ?? 0;
            if (puzzleId >= count)
            {
                Services.BuildMode.End();
                SetState(GameState.Peace);
                GameEvents.RaisePeaceReached();
            }
            else
            {
                CurrentPuzzle = puzzleId + 1;
                SetState(GameState.Build);
            }
        }

        // ---------- Flow helpers ----------

        void BeginDay(int day, bool loadScene)
        {
            Day = day;
            Mode = ModeFor(day);
            UnloadJobScene();
            CurrentJob = null;

            if (Mode == DayMode.Build && !UseFallback())
            {
                if (loadScene) SceneManager.LoadScene(SceneNames.Build);
                SetState(GameState.BuildIntro);
            }
            else
            {
                if (loadScene) SceneManager.LoadScene(SceneNames.World);
                SetState(Mode == DayMode.Build ? GameState.FinalAttack : GameState.WorldPeace);
            }
            GameEvents.RaiseDayStarted(Day, Mode);
        }

        void StartNextDay()
        {
            Day = Mathf.Min(Day + 1, 3);
            Mode = ModeFor(Day);
            CurrentPuzzle = 1;
            SetState(GameState.DayStart);
            GameEvents.RaiseDayStarted(Day, Mode);
        }

        void EnterDay()
        {
            if (Mode == DayMode.Build && !UseFallback())
            {
                SceneManager.LoadScene(SceneNames.Build);
                SetState(GameState.BuildIntro);
            }
            else
            {
                SetState(Mode == DayMode.Build ? GameState.FinalAttack : GameState.WorldPeace);
            }
        }

        void BeginBuild()
        {
            SetState(GameState.Build);
            Services.BuildMode.Begin(CurrentPuzzle);
        }

        void ReturnToWorld()
        {
            UnloadJobScene();
            CurrentJob = null;
            SetState(GameState.WorldPeace);
        }

        void GoToTitle()
        {
            UnloadJobScene();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Boot);
            SetState(GameState.Title);
        }

        void UnloadJobScene()
        {
            if (string.IsNullOrEmpty(loadedJobScene)) return;
            if (SceneManager.GetSceneByName(loadedJobScene).isLoaded) SceneManager.UnloadSceneAsync(loadedJobScene);
            loadedJobScene = null;
        }

        DayMode ModeFor(int day)
        {
            var days = Services.Data.Days?.days;
            if (days != null)
                foreach (var data in days)
                    if (data.day == day) return DayModes.Parse(data.mode);
            return day == 1 ? DayMode.Tutorial : day == 2 ? DayMode.Shifts : DayMode.Build;
        }

        static bool UseFallback() => Services.Data.Flags?.UseFallbackDay3 ?? false;

        void SetState(GameState next)
        {
            var previous = State;
            if (previous == next) return;
            if (Expected.TryGetValue(previous, out var allowed) && System.Array.IndexOf(allowed, next) < 0)
                Debug.LogWarning($"[GameManager] Unexpected transition {previous} -> {next}");
            State = next;
            Debug.Log($"[GameManager] {previous} -> {next} (day {Day})");
            GameEvents.RaiseStateChanged(previous, next);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Debug: F2 forces the next state in the enum (no scene loading), F3 logs the current state.
        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f2Key.wasPressedThisFrame)
            {
                var next = (GameState)(((int)State + 1) % System.Enum.GetValues(typeof(GameState)).Length);
                Debug.Log($"[GameManager] Debug key: forcing {next}");
                SetState(next);
            }
            if (keyboard.f3Key.wasPressedThisFrame)
                Debug.Log($"[GameManager] State {State}, day {Day} ({Mode}), job {CurrentJob?.ToString() ?? "none"}, puzzle {CurrentPuzzle}");
        }
#endif
    }
}
