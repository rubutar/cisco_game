// Stream A. Demo and debug shortcuts (A-16, PRD 6.9), on GameRoot.prefab:
// - URL parameters in the web build: ?day=2 starts at the first Day 2 pair; ?day=3&puzzle=3 opens build puzzle 3.
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
            if (TryReadUrl(Application.absoluteURL, out var day, out var puzzle))
            {
                Debug.Log($"[DebugMenu] URL start: day {day}, puzzle {puzzle}");
                Services.Flow.StartAt(day, puzzle);
            }
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

        void Go(int day, int puzzle)
        {
            open = false;
            Services.Flow.StartAt(day, puzzle);
        }
    }
}
