// Stream B. The test-run side of build mode (PRD 4.10, screens B1 and BR): the Run button while building
// (the goal line sits in the top bar), the packets travelling along the cables during a TestRun, and the result card after.
// Run / Fix the build go to GameManager as UiActions; a pass raises PuzzlePassed (the reveal follows).
// Outcomes come from TrafficRun (plain C#); this only plays them back.
using System.Collections;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    [RequireComponent(typeof(BuildManager))]
    public class BuildRunner : MonoBehaviour
    {
        const float PacketSpeed = 900f;      // board pixels per second
        const float PacketStagger = 0.35f;   // seconds between messages leaving the router
        const float PacketSize = 30f;

        [SerializeField] Button runButton;
        [SerializeField] TMP_Text runLabel;
        [SerializeField] RectTransform packetLayer;
        [SerializeField] GameObject resultCard;
        [SerializeField] TMP_Text resultLine;
        [SerializeField] TMP_Text resultSummary;
        [SerializeField] Button resultButton;
        [SerializeField] TMP_Text resultButtonLabel;

        BuildManager manager;
        SiegeRunner siege;
        Coroutine running;
        readonly List<GameObject> packets = new();
        bool lastRunPassed;

        void Awake()
        {
            manager = GetComponent<BuildManager>();
            siege = GetComponent<SiegeRunner>();
            runButton.onClick.AddListener(OnRun);
            resultButton.onClick.AddListener(OnResultButton);
            HideAll();
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.PuzzleStarted += OnPuzzleStarted;
            manager.Ended += StopRun;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.PuzzleStarted -= OnPuzzleStarted;
            manager.Ended -= StopRun;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Build:
                    StopRun();
                    ShowBuilding();
                    break;
                case GameState.TestRun:
                    StartRun();
                    break;
                default:
                    StopRun();
                    HideAll();
                    break;
            }
        }

        // ---------- Building ----------

        // GameManager sets Build before calling Begin, so this runs again on PuzzleStarted once the board is loaded.
        void ShowBuilding()
        {
            resultCard.SetActive(false);
            if (manager.Puzzle == null) return;
            runLabel.text = UiStrings.Ui.run ?? "";
            runButton.gameObject.SetActive(true);
        }

        void OnPuzzleStarted(int puzzleId, int index, int count)
        {
            if (Services.Flow.State == GameState.Build) ShowBuilding();
        }

        void OnRun()
        {
            if (Services.Flow.State != GameState.Build) return;
            runButton.gameObject.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.Run);
        }

        // ---------- Test run ----------

        void StartRun()
        {
            StopRun();
            runButton.gameObject.SetActive(false);
            resultCard.SetActive(false);
            if (manager.Puzzle == null) return;
            manager.ClearSelection();
            // Quest 4: the siege plays its own scripted run (PRD-v2 4.8) and reports back through Finish.
            if (manager.IsSiege && siege != null)
            {
                siege.Play(this);
                return;
            }
            running = StartCoroutine(Run(TrafficRun.Plan(manager.Graph, manager.Puzzle, manager.Rules), manager.Puzzle));
        }

        public void StopRun()
        {
            if (siege != null) siege.Stop();
            StopAllCoroutines(); // the run and every packet still moving
            running = null;
            foreach (var p in packets) if (p != null) Destroy(p);
            packets.Clear();
        }

        IEnumerator Run(TrafficRun run, PuzzleData puzzle)
        {
            var max = ComplaintRules.Limit(puzzle.pass?.maxWrongBlocks ?? 0);
            var complaints = 0;
            var inFlight = 0;

            foreach (var plan in run.packets)
            {
                inFlight++;
                var complaint = plan.Complaint;
                StartCoroutine(PlayPacket(plan, () =>
                {
                    inFlight--;
                    if (complaint)
                        GameEvents.RaiseComplaint(new ComplaintInfo { source = SnatchSource.TestRun, count = ++complaints, max = max });
                }));
                yield return new WaitForSeconds(PacketStagger);
            }
            while (inFlight > 0) yield return null;
            // Quest 3: pods in the wrong lane blink pink.
            foreach (var pod in run.MisplacedPods) manager.FlashSlot(pod);
            yield return new WaitForSeconds(0.4f);

            var result = run.ToResult(puzzle.id, puzzle.pass);
            running = null;

            // An attacker got through: that costs a backup, like a snatch on Days 1 and 2 (PRD 4.5).
            // Only the backup nest decides; with none left it collapses Byte World.
            if (run.Leaked > 0 && ReportLeak(run) == SnatchResult.Collapsed) yield break;

            Finish(result, ResultLine(run, result));
        }

        /// <summary>Ends a run: tells GameManager and shows the result card (the siege calls this too).</summary>
        public void Finish(RunResult result, string line)
        {
            lastRunPassed = result.passed;
            GameEvents.RaiseTestRunFinished(result);
            ShowResult(result, line);
        }

        static string ResultLine(TrafficRun run, RunResult result)
        {
            var build = Services.Data?.Dialogue?.build;
            if (result.passed) return build?.success;
            if (run.Leaked > 0) return build?.failLeak;
            if (run.MisplacedPods.Count > 0) return build?.failWrongHouse;
            if (run.Blocked > 0) return build?.failBlocked;
            return build?.failNoPath;
        }

        static SnatchResult ReportLeak(TrafficRun run)
        {
            var attacker = "";
            foreach (var p in run.packets)
                if (p.outcome == PacketOutcome.Leaked) { attacker = p.traffic.attacker ?? ""; break; }
            return ReportLeak(attacker);
        }

        /// <summary>An attacker got in during a test run: one backup, through the backup nest only.</summary>
        public static SnatchResult ReportLeak(string attacker)
        {
            // The Byte leading build day is the one who reacts to the leak.
            JobIds.TryParse(Services.Data?.Dialogue?.build?.speaker, out var job);
            return Services.Backups.ReportSnatch(new SnatchInfo { source = SnatchSource.TestRun, job = job, attacker = attacker });
        }

        IEnumerator PlayPacket(PacketPlan plan, System.Action done)
        {
            var packet = NewPacket(plan.traffic.IsAttack ? Palette.AlertPink : Palette.PacketYellow);
            var router = FindRouter(plan);
            var size = manager.BoardSize;
            var image = packet.GetComponent<Image>();

            if (plan.path != null && plan.path.Count > 0)
            {
                var rect = (RectTransform)packet.transform;
                rect.anchorMin = rect.anchorMax = manager.AnchorOf(plan.path[0]);
                for (var i = 0; i + 1 < plan.path.Count; i++)
                    yield return Move(packet, manager.AnchorOf(plan.path[i]), manager.AnchorOf(plan.path[i + 1]), size);
                if (plan.outcome == PacketOutcome.Blocked)
                {
                    // Turned away at the door: an attacker stopped blinks cyan, a real citizen turned away pink.
                    image.color = plan.traffic.IsAttack ? Palette.NeonCyan : Palette.AlertPink;
                    manager.FlashSlot(plan.path[plan.path.Count - 1]);
                    yield return new WaitForSeconds(0.35f);
                }
                else if (plan.outcome == PacketOutcome.Leaked && !string.IsNullOrEmpty(plan.targetSlot))
                {
                    manager.FlashSlot(plan.targetSlot);
                }
            }
            else
            {
                // No cable path: the message leaves the router, wanders a little towards the house and is lost.
                var from = router != null ? manager.AnchorOf(router) : new Vector2(0.5f, 0.5f);
                var to = string.IsNullOrEmpty(plan.targetSlot) ? from : Vector2.Lerp(from, manager.AnchorOf(plan.targetSlot), 0.3f);
                yield return Move(packet, from, to, size);
                packet.GetComponent<Image>().color = Palette.AlertPink;
                if (!string.IsNullOrEmpty(plan.targetSlot)) manager.FlashSlot(plan.targetSlot);
                yield return new WaitForSeconds(0.35f);
            }
            packet.SetActive(false);
            done();
        }

        public IEnumerator Move(GameObject packet, Vector2 from, Vector2 to, Vector2 boardSize)
        {
            if (packet == null) yield break;
            var rect = (RectTransform)packet.transform;
            var duration = Mathf.Max(0.2f, Vector2.Scale(to - from, boardSize).magnitude / PacketSpeed);
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                if (rect == null) yield break; // the run was stopped and the packet cleared
                rect.anchorMin = rect.anchorMax = Vector2.Lerp(from, to, t / duration);
                yield return null;
            }
            if (rect != null) rect.anchorMin = rect.anchorMax = to;
        }

        string FindRouter(PacketPlan plan)
        {
            if (plan.path != null && plan.path.Count > 0) return plan.path[0];
            foreach (var s in manager.Puzzle.slots)
                if (s.@fixed == "router") return s.id;
            return null;
        }

        public GameObject NewPacket(Color color)
        {
            var go = new GameObject("Packet", typeof(RectTransform), typeof(Image), typeof(Outline));
            var rect = (RectTransform)go.transform;
            rect.SetParent(packetLayer, false);
            rect.sizeDelta = new Vector2(PacketSize, PacketSize);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(4, -4);
            packets.Add(go);
            return go;
        }

        // ---------- Result ----------

        void ShowResult(RunResult result, string line)
        {
            var ui = UiStrings.Ui;
            resultLine.text = line ?? "";
            // The counts only mean something when packets were counted (not in the scripted siege).
            var counted = result.delivered + result.blocked + result.leaked > 0;
            resultSummary.text = counted ? UiStrings.Format(ui.runResultFormat, result.delivered, result.blocked, result.leaked) : "";
            resultSummary.gameObject.SetActive(counted);
            resultButtonLabel.text = (result.passed ? ui.next : ui.fixBuild) ?? "";
            resultCard.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)resultCard.transform);
        }

        void OnResultButton()
        {
            if (!resultCard.activeSelf || Services.Flow.State != GameState.TestRun) return;
            resultCard.SetActive(false);
            if (lastRunPassed) GameEvents.RaisePuzzlePassed(manager.Puzzle.id);
            else GameEvents.RaiseUiAction(UiAction.FixBuild);
        }

        void HideAll()
        {
            runButton.gameObject.SetActive(false);
            resultCard.SetActive(false);
        }
    }
}
