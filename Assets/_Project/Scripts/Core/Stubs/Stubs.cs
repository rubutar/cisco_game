// FOUNDATION STUBS. Stand-ins used until the owning stream registers the real implementation.
// Keep them dumb: no gameplay rules beyond what the contract describes. The IMGUI text here is
// developer-only placeholder UI and never ships to players.
using System;
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld.Stubs
{
    /// <summary>Marker so <see cref="Services.IsStub{T}"/> can tell stubs from real implementations.</summary>
    public interface IStub { }

    public static class StubBehaviour
    {
        public static T Create<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject($"[{name}]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }
    }

    /// <summary>Stand-in for Stream A's BackupNest. Follows the contract exactly.</summary>
    public class StubBackupNest : IBackupNest, IStub
    {
        public int Left { get; private set; } = 3;
        public int Max => 3;

        public SnatchResult ReportSnatch(SnatchInfo info)
        {
            if (Left <= 0)
            {
                GameEvents.RaiseCollapsed(Services.Flow.Day);
                return SnatchResult.Collapsed;
            }

            Left--;
            info.backupsLeft = Left;
            GameEvents.RaiseSnatched(info);
            GameEvents.RaiseBackupsChanged(Left, Max);
            return SnatchResult.BackupUsed;
        }

        public void Refill()
        {
            Left = Max;
            GameEvents.RaiseBackupsChanged(Left, Max);
        }
    }

    /// <summary>Loads the JSON files from Resources/Data. Stream A's DataLoader replaces it (A-05).</summary>
    public class ResourcesGameData : IGameData, IStub
    {
        public DaysFile Days { get; } = Load<DaysFile>("days");
        public DialogueFile Dialogue { get; } = Load<DialogueFile>("dialogue");
        public RoamingFile Roaming { get; } = Load<RoamingFile>("roaming");
        public BuildFile Build { get; } = Load<BuildFile>("build");
        public FlagsFile Flags { get; } = Load<FlagsFile>("flags");

        public static T Load<T>(string name) where T : new()
        {
            var asset = Resources.Load<TextAsset>($"Data/{name}");
            if (asset == null)
            {
                Debug.LogWarning($"[Data] Resources/Data/{name}.json not found");
                return new T();
            }
            return JsonUtility.FromJson<T>(asset.text);
        }
    }

    /// <summary>Stand-in for Stream A's GameManager. Only tracks the day and fires day events.</summary>
    public class StubGameFlow : IGameFlow, IStub
    {
        public GameState State { get; private set; } = GameState.Title;
        public int Day { get; private set; } = 1;
        public DayMode Mode { get; private set; } = DayMode.Tutorial;

        public void StartNewGame() => StartAt(1, 1);

        public void StartAt(int day, int puzzle)
        {
            Day = Mathf.Clamp(day, 1, 3);
            Mode = Day == 1 ? DayMode.Tutorial : Day == 2 ? DayMode.Shifts : DayMode.Build;
            Services.Backups.Refill();
            SetState(Day == 3 ? GameState.BuildIntro : GameState.WorldPeace);
            GameEvents.RaiseDayStarted(Day, Mode);
            if (Day == 3) Services.BuildMode.Begin(Mathf.Max(1, puzzle));
        }

        void SetState(GameState next)
        {
            var previous = State;
            State = next;
            GameEvents.RaiseStateChanged(previous, next);
        }
    }

    /// <summary>Stand-in for Stream B's BuildManager: passes every remaining puzzle at once.</summary>
    public class StubBuildMode : IBuildMode, IStub
    {
        public void Begin(int startPuzzle)
        {
            var count = Services.Data.Build?.puzzles?.Length ?? 0;
            if (count == 0) count = 3;
            for (var index = Mathf.Max(1, startPuzzle); index <= count; index++)
            {
                GameEvents.RaisePuzzleStarted(index, index, count);
                GameEvents.RaiseTestRunFinished(new RunResult { puzzleId = index, passed = true });
                GameEvents.RaisePuzzlePassed(index);
                GameEvents.RaiseRevealClosed(index);
            }
        }

        public void End() { }
    }

    /// <summary>Stand-in for Stream B's dialogue bubble: an IMGUI box at the top of the screen.</summary>
    public class StubDialogueService : MonoBehaviour, IDialogueService, IStub
    {
        readonly Queue<string> pending = new();
        string speaker;
        string current;
        Action onDone;
        Action onRetry;

        public bool IsShowing => current != null;

        public void Say(string who, string text, Action done = null) => SayLines(who, new[] { text }, done);

        public void SayLines(string who, IReadOnlyList<string> lines, Action done = null)
        {
            pending.Clear();
            foreach (var line in lines) pending.Enqueue(line);
            speaker = who;
            onDone = done;
            onRetry = null;
            current = pending.Count > 0 ? pending.Dequeue() : null;
            if (current == null) Finish();
        }

        public void ShowHint(string who, string text, Vector3 worldPosition, Action retry)
        {
            pending.Clear();
            speaker = who;
            current = text;
            onDone = null;
            onRetry = retry;
        }

        public void Hide()
        {
            pending.Clear();
            current = null;
            onDone = null;
            onRetry = null;
        }

        void Finish()
        {
            var done = onDone;
            Hide();
            done?.Invoke();
        }

        void OnGUI()
        {
            if (current == null) return;
            GUILayout.BeginArea(new Rect(10, 10, Screen.width * 0.5f, 140), GUI.skin.box);
            GUILayout.Label($"[stub bubble] {speaker}: {current}");
            if (onRetry != null)
            {
                if (GUILayout.Button("Try again (stub)"))
                {
                    var retry = onRetry;
                    Hide();
                    retry();
                }
            }
            else if (GUILayout.Button("Next (stub)"))
            {
                if (pending.Count > 0) current = pending.Dequeue();
                else Finish();
            }
            GUILayout.EndArea();
        }
    }

    /// <summary>Stand-in for Stream B's inspect panel: an IMGUI box on the left of the screen.</summary>
    public class StubInspectPanel : MonoBehaviour, IInspectPanel, IStub
    {
        InspectData data;
        Action onPrimary;
        Action onSecondary;

        public bool IsOpen { get; private set; }

        public void Show(InspectData inspectData, Action primary, Action secondary)
        {
            data = inspectData;
            onPrimary = primary;
            onSecondary = secondary;
            IsOpen = true;
        }

        public void Hide() => IsOpen = false;

        void OnGUI()
        {
            if (!IsOpen) return;
            GUILayout.BeginArea(new Rect(10, 160, 280, 220), GUI.skin.box);
            GUILayout.Label($"[stub inspect] {data.title}");
            if (data.fields != null)
                foreach (var field in data.fields) GUILayout.Label($"{field.label}: {field.value}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(data.primaryLabel ?? "Primary")) { Hide(); onPrimary?.Invoke(); }
            if (GUILayout.Button(data.secondaryLabel ?? "Secondary")) { Hide(); onSecondary?.Invoke(); }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
