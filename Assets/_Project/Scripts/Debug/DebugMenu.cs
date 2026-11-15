// Stream A. Demo and debug shortcuts (A-16, PRD 6.9), on GameRoot.prefab:
// - URL parameters in the web build: ?quest=1..4, ?hunt=1, ?at=<checkpointId> (v2, PRD-v2 A-08), and the v1
//   ?day=N&puzzle=M.
// - A hidden debug menu: five taps on the title logo (Stream B raises DebugMenuRequested) or the backquote key.
//   Start at Day 1/2/3, jump to build puzzle 1/2/3, switch Day 3 to the fallback final attack, refill backups.
// Developer-only tool: its IMGUI labels are not player-facing game text.
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ByteWorld.DebugTools
{
    public class DebugMenu : MonoBehaviour
    {
        bool open;

        void OnEnable() => GameEvents.DebugMenuRequested += Toggle;

        void OnDisable() => GameEvents.DebugMenuRequested -= Toggle;

        IEnumerator Start()
        {
            // Let GameManager settle on the title first, then jump.
            yield return null;
            var url = Application.absoluteURL;
            var quest = ReadInt(url, "quest");
            var at = ReadString(url, "at");
            if (ReadInt(url, "hunt") > 0)
            {
                Debug.Log("[DebugMenu] URL start: the Splunk Trace");
                yield return StartQuest(2);
                CheckpointManager.Instance?.SetDone("vault", "workshop");
                CheckpointManager.Instance?.GoIn(CheckpointManager.Instance.StationForJob("hunt"));
            }
            else if (quest > 0)
            {
                Debug.Log($"[DebugMenu] URL start: quest {quest}");
                yield return StartQuest(quest);
            }
            else if (TryReadUrl(url, out var day, out var puzzle))
            {
                Debug.Log($"[DebugMenu] URL start: day {day}, puzzle {puzzle}");
                Services.Flow.StartAt(day, puzzle);
            }

            if (!string.IsNullOrEmpty(at))
            {
                // ?at=<checkpointId>: Pax starts at that station with its card open.
                var c = Services.Checkpoints.Get(at);
                if (c != null && quest <= 0 && Services.Flow.State == GameState.Title) yield return StartQuest(c.quest);
                while (HeroController.Instance == null) yield return null;
                yield return null;
                HeroController.Instance.TeleportTo(at);
                GameEvents.RaiseUiAction($"interact:{at}");
            }
        }

        /// <summary>
        /// Start a quest (PRD-v2 A-08): earlier stations count as done; Quests 1-3 start on the map of their day,
        /// Quest 4 opens the siege puzzle.
        /// </summary>
        IEnumerator StartQuest(int quest)
        {
            quest = Mathf.Clamp(quest, 1, 4);
            CheckpointManager.Instance?.SkipTo(quest);
            if (quest == 4) Services.Flow.StartAt(3, Services.Data.Build?.puzzles?.Length ?? 3);
            else Services.Flow.StartAt(quest, 1);
            yield return null;
        }

        static int ReadInt(string url, string key) => int.TryParse(ReadString(url, key), out var v) ? v : 0;

        static string ReadString(string url, string key)
        {
            var query = url?.IndexOf('?') ?? -1;
            if (query < 0) return "";
            foreach (var pair in url.Substring(query + 1).Split('&', '#'))
            {
                var parts = pair.Split('=');
                if (parts.Length == 2 && parts[0] == key) return parts[1];
            }
            return "";
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.backquoteKey.wasPressedThisFrame) Toggle();
        }

        void Toggle() => open = !open;

        /// <summary>Reads ?day=N and &amp;puzzle=M from a page address. False when there's no day parameter.</summary>
        public static bool TryReadUrl(string url, out int day, out int puzzle)
        {
            day = 0;
            puzzle = 1;
            var query = url?.IndexOf('?') ?? -1;
            if (query < 0) return false;
            foreach (var pair in url.Substring(query + 1).Split('&', '#'))
            {
                var parts = pair.Split('=');
                if (parts.Length != 2 || !int.TryParse(parts[1], out var value)) continue;
                if (parts[0] == "day") day = Mathf.Clamp(value, 1, 3);
                else if (parts[0] == "puzzle") puzzle = Mathf.Clamp(value, 1, 3);
            }
            return day > 0;
        }

        void OnGUI()
        {
            if (!open) return;
            // Scale to a 960 x 540 layout so the buttons are finger-sized on any screen.
            var scale = Screen.height / 540f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUILayout.BeginArea(new Rect(20, 20, 300, 500), GUI.skin.box);
            var flow = Services.Flow;
            var flags = DataLoader.Instance != null ? DataLoader.Instance.Flags : null;
            GUILayout.Label($"Debug  |  {flow.State}, day {flow.Day}  |  backups {Services.Backups.Left}/{Services.Backups.Max}  |  Day 3: {flags?.day3Mode}");

            GUILayout.BeginHorizontal();
            for (var q = 1; q <= 4; q++)
                if (GUILayout.Button($"Quest {q}", GUILayout.Height(36))) { open = false; StartCoroutine(StartQuest(q)); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Splunk Trace", GUILayout.Height(36)))
            {
                open = false;
                StartCoroutine(HuntShortcut());
            }
            if (GUILayout.Button("Day 1 (skip intro)", GUILayout.Height(36))) Go(1, 1);
            if (GUILayout.Button("Day 2", GUILayout.Height(36))) Go(2, 1);
            GUILayout.BeginHorizontal();
            for (var p = 1; p <= 3; p++)
                if (GUILayout.Button($"Day 3, puzzle {p}", GUILayout.Height(36))) Go(3, p);
            GUILayout.EndHorizontal();

            if (flags != null)
            {
                var fallback = flags.UseFallbackDay3;
                if (GUILayout.Button(fallback ? "Day 3 mode: FALLBACK (switch to build)" : "Day 3 mode: build (switch to fallback)", GUILayout.Height(36)))
                    flags.day3Mode = fallback ? "build" : "fallback";
                if (GUILayout.Button("Start the fallback final attack", GUILayout.Height(36)))
                {
                    flags.day3Mode = "fallback";
                    Go(3, 1);
                }
            }

            if (GUILayout.Button("Backups to 3", GUILayout.Height(36))) Services.Backups.Refill();
            if (GUILayout.Button("Close", GUILayout.Height(36))) open = false;
            GUILayout.EndArea();
        }

        IEnumerator HuntShortcut()
        {
            yield return StartQuest(2);
            CheckpointManager.Instance?.SetDone("vault", "workshop");
            CheckpointManager.Instance?.GoIn(CheckpointManager.Instance.StationForJob("hunt"));
        }

        void Go(int day, int puzzle)
        {
            open = false;
            Services.Flow.StartAt(day, puzzle);
        }
    }
}
