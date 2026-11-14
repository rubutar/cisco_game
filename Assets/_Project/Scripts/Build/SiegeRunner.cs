// Stream B. Quest 4, the Ultimate Siege (PRD-v2 4.8, B-06), scripted mode (flags.json quest4Mode "scripted",
// the safety net; the free-form "live" fix is the B-13 stretch and also plays this script for now).
// It runs inside GameManager's TestRun state and ends with BuildRunner.Finish, so the flow is unchanged:
//   1. Run: the Webex call streams from callFrom (yellow priority packets) while a flood hits the router.
//   2. Blind phase (siege.blindSeconds): the call indicator shows loss and jitter, then the video freezes.
//      Every link looks the same.
//   3. Gigi's second Quest 4 brief asks for the network team; the Call button appears.
//   4. Call: the ThousandEyes view colours every link and tags it with loss % and latency ms (pink bad, green good).
//   5. Tap the root-cause link: the call moves onto the green switch. Priority for Webex goes on
//      (siege.priorityToggle). Run again: the flood bounces off, the call stays clear, the run passes.
// Text: briefs[] (quest 4: the first entry when the puzzle starts, the second at step 3), ui.call, ui.priority,
// ui.run, build.success. Numbers on the links are display values, not game text.
using System.Collections;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    [RequireComponent(typeof(BuildManager))]
    public class SiegeRunner : MonoBehaviour
    {
        public const int Quest = 4;
        const float DefaultBlindSeconds = 15f;
        const float CallInterval = 0.3f;
        const float FloodInterval = 0.12f;
        const float ClearRunSeconds = 4.5f;

        [Header("Call indicator (top right)")]
        [SerializeField] GameObject indicator;
        [SerializeField] Image videoTile;
        [SerializeField] GameObject frozenMark;
        [SerializeField] Image[] bars;
        [SerializeField] TMP_Text statsText;
        [SerializeField] TMP_Text callerText;

        [Header("Buttons")]
        [SerializeField] Button callButton;
        [SerializeField] TMP_Text callLabel;
        [SerializeField] Button priorityButton;
        [SerializeField] Image priorityFill;
        [SerializeField] TMP_Text priorityLabel;
        [SerializeField] Button runAgainButton;
        [SerializeField] TMP_Text runAgainLabel;

        enum Stage { Idle, Blind, WaitCall, Overlay, WaitPriority, WaitRun, Clear }

        BuildManager manager;
        BuildRunner runner;
        Stage stage;
        bool overlayOn;
        bool priorityOn;
        bool pressed;
        Cable rootCause;
        float loss;             // 0..1, drives the indicator
        readonly List<GameObject> packets = new();
        readonly Dictionary<Cable, (bool bad, int lossPercent, int latency)> health = new();
        Coroutine callStream, flood;

        SiegeData Siege => manager.Puzzle?.siege;

        void Awake()
        {
            manager = GetComponent<BuildManager>();
            callButton.onClick.AddListener(() => Press(Stage.WaitCall));
            priorityButton.onClick.AddListener(() => Press(Stage.WaitPriority));
            runAgainButton.onClick.AddListener(() => Press(Stage.WaitRun));
            HideAll();
        }

        void OnEnable()
        {
            manager.Loaded += OnLoaded;
            manager.LockedCableTapped += OnCableTapped;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            manager.Loaded -= OnLoaded;
            manager.LockedCableTapped -= OnCableTapped;
            GameEvents.StateChanged -= OnStateChanged;
        }

        // The Quest 4 brief itself plays on the build intro (BuildIntroScreen).
        void OnLoaded() { }

        void OnStateChanged(GameState from, GameState to)
        {
            if (stage != Stage.Idle && to != GameState.TestRun) Stop();
        }

        static string[] Brief(int index)
        {
            var briefs = Services.Data?.Dialogue?.briefs;
            if (briefs == null) return null;
            var n = 0;
            foreach (var b in briefs)
                if (b != null && b.quest == Quest && n++ == index) return b.lines;
            return null;
        }

        // ---------- Run ----------

        public void Play(BuildRunner owner)
        {
            Stop();
            runner = owner;
            StartCoroutine(Sequence());
        }

        public void Stop()
        {
            StopAllCoroutines();
            foreach (var p in packets) if (p != null) Destroy(p);
            packets.Clear();
            ShowOverlay(false);
            HideAll();
            stage = Stage.Idle;
        }

        IEnumerator Sequence()
        {
            var siege = Siege;
            var callFrom = siege?.callFrom;
            if (string.IsNullOrEmpty(callFrom) || !manager.Graph.HasPiece(callFrom))
            {
                Debug.LogWarning("[SiegeRunner] build.json siege.callFrom is missing or not a house on the board.");
                runner.Finish(Pass(), Services.Data?.Dialogue?.build?.success);
                yield break;
            }

            priorityOn = false;
            loss = 0;
            ShowIndicator(callFrom);
            callStream = StartCoroutine(CallStream(callFrom));
            flood = StartCoroutine(Flood(bounce: false));

            // Blind phase: things get worse and nobody can see why.
            stage = Stage.Blind;
            var blind = siege.blindSeconds > 0 ? siege.blindSeconds : DefaultBlindSeconds;
            for (var t = 0f; t < blind; t += Time.deltaTime)
            {
                loss = Mathf.Clamp01(t / (blind * 0.7f));
                yield return null;
            }

            // Gigi asks for the network team: a second Quest 4 brief if there is one, else the brief's last line.
            var ask = Brief(1);
            if (ask == null)
            {
                var first = Brief(0);
                if (first != null && first.Length > 0) ask = new[] { first[first.Length - 1] };
            }
            if (ask != null)
            {
                foreach (var line in ask) TownSpace.Add(TownSpace.BriefSpeaker, line);
                Services.Dialogue.SayLines(TownSpace.BriefSpeaker, ask);
            }
            yield return WaitFor(Stage.WaitCall, callButton, callLabel, UiStrings.Ui.call);

            // ThousandEyes: every link shows its health.
            stage = Stage.Overlay;
            rootCause = FindRootCause(callFrom);
            BuildHealth();
            ShowOverlay(true);
            // OnCableTapped moves on; with no root-cause cable on the board there's nothing to tap, so don't wait.
            if (rootCause.a == null) stage = Stage.WaitPriority;
            while (stage == Stage.Overlay) yield return null;

            // The fix: the call moves to the green switch.
            var green = GreenSwitch(callFrom);
            if (green != null) manager.MoveHouse(callFrom, green);
            BuildHealth();

            if (siege.priorityToggle)
            {
                priorityFill.color = UiStyle.White;
                yield return WaitFor(Stage.WaitPriority, priorityButton, priorityLabel, UiStrings.Ui.priority);
                priorityOn = true;
                priorityFill.color = Palette.PacketYellow;
                priorityButton.gameObject.SetActive(true);
                var colors = priorityButton.colors;
                colors.disabledColor = Color.white; // stays bright yellow: Priority is on
                priorityButton.colors = colors;
                priorityButton.interactable = false;
            }

            yield return WaitFor(Stage.WaitRun, runAgainButton, runAgainLabel, UiStrings.Ui.run);

            // Second run: the flood bounces off, the call stays clear.
            stage = Stage.Clear;
            // Not StopAllCoroutines: that would also end this sequence.
            if (flood != null) StopCoroutine(flood);
            flood = StartCoroutine(Flood(bounce: true));
            for (var t = 0f; t < ClearRunSeconds; t += Time.deltaTime)
            {
                loss = Mathf.MoveTowards(loss, 0, Time.deltaTime * 0.8f);
                yield return null;
            }
            loss = 0;
            yield return new WaitForSeconds(0.6f);

            // Last step, no more yields: safe to stop every packet coroutine, this one included.
            StopAllCoroutines();
            foreach (var p in packets) if (p != null) Destroy(p);
            packets.Clear();
            ShowOverlay(false);
            HideAll();
            stage = Stage.Idle;
            runner.Finish(Pass(), Services.Data?.Dialogue?.build?.success);
        }

        RunResult Pass() => new() { puzzleId = manager.Puzzle?.id ?? 0, passed = true };

        IEnumerator WaitFor(Stage waitStage, Button button, TMP_Text label, string text)
        {
            stage = waitStage;
            pressed = false;
            label.text = text ?? "";
            button.interactable = true;
            button.gameObject.SetActive(true);
            while (!pressed) yield return null;
            button.gameObject.SetActive(false);
        }

        void Press(Stage expected)
        {
            if (stage == expected) pressed = true;
        }

        void OnCableTapped(Cable cable)
        {
            if (stage != Stage.Overlay) return;
            if (cable.Equals(rootCause))
            {
                stage = Stage.WaitPriority; // leaves the overlay wait; the sequence carries on
                return;
            }
            manager.FlashSlot(cable.a);
            manager.FlashSlot(cable.b);
        }

        // ---------- The network ----------

        // build.json siege.rootCauseLink, or else the cable from the caller's house to its switch.
        Cable FindRootCause(string callFrom)
        {
            if (BuildManager.TryParseCable(Siege?.rootCauseLink, out var named) && manager.Graph.AreConnected(named.a, named.b))
                return named;
            foreach (var next in manager.Graph.Neighbours(callFrom)) return new Cable(callFrom, next);
            return default;
        }

        // A switch with a free socket that isn't on the bad link.
        string GreenSwitch(string callFrom)
        {
            var graph = manager.Graph;
            foreach (var c in graph.Cables)
                foreach (var id in new[] { c.a, c.b })
                {
                    if (!graph.HasPiece(id) || graph.KindOf(id) != PieceKind.Switch) continue;
                    if (rootCause.Touches(id) || graph.AreConnected(id, callFrom)) continue;
                    if (graph.FreeSockets(id) > 0) return id;
                }
            return null;
        }

        void BuildHealth()
        {
            health.Clear();
            var i = 0;
            foreach (var c in manager.Graph.Cables)
            {
                var bad = c.Equals(rootCause);
                health[c] = bad ? (true, 18, 240) : (false, 0, 8 + (i * 7) % 23);
                i++;
            }
        }

        void ShowOverlay(bool on)
        {
            overlayOn = on;
            if (!on)
                foreach (var view in manager.CableViews) view.ClearHealth();
        }

        void LateUpdate()
        {
            if (stage == Stage.Idle) return;
            UpdateIndicator();
            if (!overlayOn) return;
            // Cables get re-laid when the board changes, so re-apply the colours every frame.
            foreach (var view in manager.CableViews)
            {
                if (!health.TryGetValue(view.Cable, out var h)) continue;
                view.ShowHealth(h.bad ? UiStyle.Danger : UiStyle.Done, $"{h.lossPercent}% · {h.latency} ms");
            }
        }

        // ---------- Packets ----------

        IEnumerator CallStream(string callFrom)
        {
            var size = manager.BoardSize;
            while (true)
            {
                var path = manager.Graph.PathFromRouter(callFrom);
                if (path != null)
                {
                    path.Reverse(); // the call goes out from the pod
                    StartCoroutine(Travel(path, Palette.PacketYellow, size, drop: loss > 0.35f && !priorityOn && Random.value < loss));
                }
                yield return new WaitForSeconds(CallInterval);
            }
        }

        IEnumerator Flood(bool bounce)
        {
            var router = manager.Graph.Router;
            if (router == null) yield break;
            var size = manager.BoardSize;
            var doors = Siege?.floodDoors;
            var n = 0;
            while (true)
            {
                // From the board edge towards the router, one lane per flooded door.
                var lanes = doors != null && doors.Length > 0 ? doors.Length : 4;
                var lane = n++ % lanes;
                var from = new Vector2(lane % 2 == 0 ? 0f : 1f, 0.15f + 0.7f * ((lane * 0.37f) % 1f));
                StartCoroutine(FloodPacket(from, manager.AnchorOf(router), size, bounce));
                yield return new WaitForSeconds(FloodInterval);
            }
        }

        IEnumerator FloodPacket(Vector2 from, Vector2 to, Vector2 size, bool bounce)
        {
            var packet = runner.NewPacket(Palette.AlertPink);
            packets.Add(packet);
            var rect = (RectTransform)packet.transform;
            rect.anchorMin = rect.anchorMax = from;
            if (bounce)
            {
                // Stopped at the door: goes a third of the way and turns back.
                var door = Vector2.Lerp(from, to, 0.35f);
                yield return runner.Move(packet, from, door, size);
                if (packet == null) yield break;
                packet.GetComponent<Image>().color = Palette.NeonCyan;
                yield return runner.Move(packet, door, from, size);
            }
            else
            {
                yield return runner.Move(packet, from, to, size);
            }
            packets.Remove(packet);
            Destroy(packet);
        }

        IEnumerator Travel(List<string> path, Color color, Vector2 size, bool drop)
        {
            var packet = runner.NewPacket(color);
            packets.Add(packet);
            var rect = (RectTransform)packet.transform;
            rect.anchorMin = rect.anchorMax = manager.AnchorOf(path[0]);
            var stopAt = drop ? Mathf.Max(1, path.Count / 2) : path.Count - 1;
            for (var i = 0; i < stopAt; i++)
                yield return runner.Move(packet, manager.AnchorOf(path[i]), manager.AnchorOf(path[i + 1]), size);
            if (packet == null) yield break;
            if (drop)
            {
                packet.GetComponent<Image>().color = Palette.AlertPink;
                yield return new WaitForSeconds(0.25f);
            }
            packets.Remove(packet);
            Destroy(packet);
        }

        // ---------- Call indicator ----------

        void ShowIndicator(string callFrom)
        {
            indicator.SetActive(true);
            var address = 0;
            foreach (var s in manager.Puzzle.slots)
                if (s.id == callFrom) address = s.address;
            callerText.text = address > 0 ? address.ToString() : "";
        }

        void UpdateIndicator()
        {
            if (!indicator.activeSelf) return;
            var quality = 1f - loss;
            var lit = Mathf.CeilToInt(quality * bars.Length);
            for (var i = 0; i < bars.Length; i++)
                bars[i].color = i < lit ? (quality > 0.5f ? UiStyle.Done : (Color)Palette.PacketYellow) : UiStyle.Locked;
            var frozen = loss > 0.75f;
            frozenMark.SetActive(frozen);
            videoTile.color = frozen ? UiStyle.LockedDark : (Color)Palette.SignalBlue;
            var lossPercent = Mathf.RoundToInt(loss * 22f);
            var jitter = Mathf.RoundToInt(2 + loss * 60f);
            statsText.text = $"{lossPercent}% · {jitter} ms";
            statsText.color = loss > 0.35f ? UiStyle.Danger : (Color)Palette.NeonCyan;
        }

        void HideAll()
        {
            indicator.SetActive(false);
            callButton.gameObject.SetActive(false);
            priorityButton.gameObject.SetActive(false);
            runAgainButton.gameObject.SetActive(false);
        }
    }
}
