// Stream B. Day 3 build mode (IBuildMode, PRD 4.10). Lives on UIRoot.prefab and registers itself, so
// GameManager can call Services.BuildMode.Begin(puzzle) as soon as Day 3 starts. Loads puzzles only from
// build.json, keeps the network in a NetworkGraph and the tap rules in a CableTool; the board, slots,
// cables and tray only display and report taps.
using System.Collections.Generic;
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public class BuildManager : MonoBehaviour, IBuildMode
    {
        /// <summary>Sockets on a switch from the tray: one cable in, several out.</summary>
        public const int SwitchSockets = 5;
        public const float MapWidth = 480f;  // build.json slot positions are in 480 x 270 map pixels
        public const float MapHeight = 270f;

        /// <summary>dialogue.json props[] entry shown the first time a switch is placed in a lanes puzzle (PRD-v2 4.7).</summary>
        public const string ManagedSwitchInfo = "managedSwitch";

        [SerializeField] GameObject view;
        [SerializeField] RectTransform board;
        [SerializeField] RectTransform cableLayer;
        [SerializeField] RectTransform slotLayer;
        [SerializeField] RectTransform tray;

        readonly Dictionary<string, BuildSlot> slots = new();
        readonly Dictionary<string, SlotData> slotData = new();
        readonly List<CableLine> cableViews = new();
        readonly List<TrayPiece> trayViews = new();
        Vector2 lastBoardSize;

        public NetworkGraph Graph { get; private set; }
        public CableTool Tool { get; private set; }
        public DoorRules Rules { get; private set; }
        public Lanes Lanes { get; private set; }
        public PuzzleData Puzzle { get; private set; }
        public bool IsSiege => Puzzle?.siege != null && Puzzle.siege.enabled;
        bool switchInfoShown;
        public int PuzzleIndex { get; private set; } // 1-based
        public int PuzzleCount => Services.Data?.Build?.puzzles?.Length ?? 0;

        /// <summary>Build mode was closed (End): anything playing on the board should stop.</summary>
        public event System.Action Ended;

        /// <summary>A new puzzle is on the board (the rules panel and siege rebuild themselves).</summary>
        public event System.Action Loaded;

        /// <summary>The board changed (piece placed, cable, door or rule): displays refresh.</summary>
        public event System.Action Changed;

        /// <summary>A cable was tapped while the board is locked (the siege's ThousandEyes step listens).</summary>
        public event System.Action<Cable> LockedCableTapped;

        void Awake()
        {
            Services.Register<IBuildMode>(this);
            view.SetActive(false);
        }

        void OnDestroy() => Services.Unregister<IBuildMode>(this);

        void OnEnable()
        {
            GameEvents.RevealClosed += OnRevealClosed;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.RevealClosed -= OnRevealClosed;
            GameEvents.StateChanged -= OnStateChanged;
        }

        // A Day 3 collapse (or going back to the title) leaves build mode without End(): clear the board so
        // it doesn't sit under the collapse and build-intro screens. LetsBuild calls Begin again.
        void OnStateChanged(GameState from, GameState to)
        {
            if (Graph == null) return;
            if (to == GameState.Collapse || to == GameState.BuildIntro || to == GameState.Title) End();
        }

        // GameManager returns to Build after a reveal but only calls Begin once, so the next puzzle loads here.
        // After the last puzzle GameManager calls End instead.
        void OnRevealClosed(int puzzleId)
        {
            if (Puzzle == null || puzzleId != Puzzle.id || PuzzleIndex >= PuzzleCount) return;
            // v2: after the Switch Yard's last puzzle GameManager goes back to the map; the Signal Tower opens the next.
            var next = Services.Data.Build.puzzles[PuzzleIndex];
            if (QuestCheck.QuestOf(next) != QuestCheck.QuestOf(Puzzle)) return;
            LoadPuzzle(PuzzleIndex + 1);
        }

        // ---------- IBuildMode ----------

        public void Begin(int startPuzzle)
        {
            var count = PuzzleCount;
            if (count == 0)
            {
                Debug.LogError("[BuildManager] build.json has no puzzles.");
                return;
            }
            LoadPuzzle(Mathf.Clamp(startPuzzle, 1, count));
        }

        public void End()
        {
            Ended?.Invoke();
            ClearBoard();
            view.SetActive(false);
        }

        // ---------- Puzzle setup ----------

        void LoadPuzzle(int index)
        {
            ClearBoard();
            PuzzleIndex = index;
            Puzzle = Services.Data.Build.puzzles[index - 1];
            Graph = new NetworkGraph();

            var open = new List<string>();
            foreach (var s in Puzzle.slots ?? System.Array.Empty<SlotData>())
            {
                slotData[s.id] = s;
                switch (s.@fixed)
                {
                    case "router":
                        Graph.AddPiece(s.id, PieceKind.Router, Mathf.Max(1, Puzzle.router?.sockets ?? 1));
                        break;
                    case "house":
                        Graph.AddPiece(s.id, PieceKind.House, 1);
                        break;
                    default:
                        open.Add(s.id);
                        break;
                }
                slots[s.id] = BuildSlot.Create(this, slotLayer, s.id, s.x / MapWidth, 1f - s.y / MapHeight);
            }

            var trayCounts = new Dictionary<string, int>();
            foreach (var t in Puzzle.tray ?? System.Array.Empty<TrayEntry>())
            {
                trayCounts[t.piece] = t.count;
                trayViews.Add(TrayPiece.Create(this, tray, t.piece, Palette.NeonCyan));
            }

            Tool = new CableTool(Graph, open, trayCounts, DescribeTrayPiece);
            Rules = new DoorRules(Puzzle);
            Lanes = new Lanes(Puzzle);
            if (IsSiege) AutoBuild(open, trayCounts);
            view.SetActive(true);
            Refresh();
            Loaded?.Invoke();
            GameEvents.RaisePuzzleStarted(Puzzle.id, index, PuzzleCount);
        }

        // "switch" is the only tray piece so far (build.json tray[].piece).
        static (PieceKind, int) DescribeTrayPiece(string piece) => (PieceKind.Switch, SwitchSockets);

        void ClearBoard()
        {
            foreach (var s in slots.Values) if (s != null) Destroy(s.gameObject);
            foreach (var c in cableViews) if (c != null) Destroy(c.gameObject);
            foreach (var t in trayViews) if (t != null) Destroy(t.gameObject);
            slots.Clear();
            slotData.Clear();
            cableViews.Clear();
            trayViews.Clear();
            Graph = null;
            Tool = null;
            Rules = null;
            Lanes = null;
            Puzzle = null;
        }

        // The siege starts on a finished network (PRD-v2 4.8): a switch in every open slot, each switch cabled to
        // the router, each house to its nearest switch with a free socket. Same input, same network every time.
        void AutoBuild(List<string> open, Dictionary<string, int> trayCounts)
        {
            var router = Graph.Router;
            var switches = new List<string>();
            var budget = 0;
            foreach (var n in trayCounts.Values) budget += n;
            if (budget == 0) budget = open.Count;
            foreach (var id in open)
            {
                if (switches.Count >= budget) break;
                Graph.AddPiece(id, PieceKind.Switch, SwitchSockets);
                switches.Add(id);
                if (router != null) Graph.Connect(router, id);
            }
            foreach (var s in slotData.Values)
            {
                if (s.@fixed != "house") continue;
                string best = null;
                var bestDistance = float.MaxValue;
                foreach (var sw in switches)
                {
                    if (Graph.FreeSockets(sw) <= 0) continue;
                    var d = (AnchorOf(sw) - AnchorOf(s.id)).sqrMagnitude;
                    if (d < bestDistance) { bestDistance = d; best = sw; }
                }
                if (best != null) Graph.Connect(best, s.id);
                else if (router != null) Graph.Connect(router, s.id);
            }
        }

        /// <summary>Scripted siege fix: moves a house's cable from its switch to another (false if not possible).</summary>
        public bool MoveHouse(string house, string toSwitch)
        {
            if (Graph == null || !Graph.HasPiece(house) || !Graph.HasPiece(toSwitch)) return false;
            foreach (var next in Graph.Neighbours(house)) Graph.Disconnect(house, next);
            var ok = Graph.Connect(toSwitch, house) == ConnectResult.Ok;
            Refresh();
            return ok;
        }

        public CableLine ViewOf(Cable cable)
        {
            foreach (var c in cableViews)
                if (c != null && c.gameObject.activeSelf && c.Cable.Equals(cable)) return c;
            return null;
        }

        public IEnumerable<CableLine> CableViews
        {
            get
            {
                foreach (var c in cableViews)
                    if (c != null && c.gameObject.activeSelf) yield return c;
            }
        }

        /// <summary>A cable id from build.json ("s1-h14", either order) as a Cable.</summary>
        public static bool TryParseCable(string id, out Cable cable)
        {
            cable = default;
            if (string.IsNullOrEmpty(id)) return false;
            var parts = id.Split('-');
            if (parts.Length != 2) return false;
            cable = new Cable(parts[0].Trim(), parts[1].Trim());
            return true;
        }

        // ---------- Taps ----------

        // The board only changes while building, not during a test run or a reveal. The siege board is fixed.
        public bool CanEdit => Tool != null && !IsSiege && Services.Flow.State == GameState.Build;

        public void OnSlotTapped(string id)
        {
            if (!CanEdit) return;
            var hadPiece = Graph.HasPiece(id);
            if (Tool.TapSlot(id) == TapOutcome.Rejected) slots[id].Flash();
            Refresh();
            // First switch in a lanes puzzle: what a managed switch is (PRD-v2 4.7).
            if (!hadPiece && Graph.HasPiece(id) && Lanes.Any && !switchInfoShown)
            {
                switchInfoShown = true;
                PropCard.ShowInfo(ManagedSwitchInfo);
            }
        }

        public void OnCableTapped(Cable cable)
        {
            if (Tool != null && !CanEdit)
            {
                LockedCableTapped?.Invoke(cable);
                return;
            }
            if (!CanEdit) return;
            Tool.TapCable(cable);
            Refresh();
        }

        bool HasOpenSlot()
        {
            foreach (var id in slots.Keys)
                if (!Graph.HasPiece(id)) return true;
            return false;
        }

        public void OnTrayTapped(string piece)
        {
            if (!CanEdit) return;
            Tool.TapTray(piece);
            Refresh();
        }

        public void ClearSelection()
        {
            Tool?.ClearSelection();
            Refresh();
        }

        // ---------- Doors and rules (the rules panel calls these) ----------

        public void CycleDoor(int port)
        {
            if (!CanEdit || Rules == null) return;
            Rules.CycleDoor(port);
            Changed?.Invoke();
        }

        public void MoveRuleUp(int position)
        {
            if (!CanEdit || Rules == null) return;
            Rules.MoveUp(position);
            Changed?.Invoke();
        }

        public void FlashSlot(string id)
        {
            if (slots.TryGetValue(id, out var slot)) slot.Flash();
        }

        public Vector2 BoardSize => board.rect.size;

        // ---------- Path check ----------

        /// <summary>Each house's address and whether a message from the router can reach it.</summary>
        public List<(string slot, int address, bool connected)> HouseReport()
        {
            var report = new List<(string, int, bool)>();
            if (Graph == null) return report;
            foreach (var s in slotData.Values)
                if (s.@fixed == "house") report.Add((s.id, s.address, Graph.ReachesRouter(s.id)));
            return report;
        }

        // ---------- Display ----------

        void LateUpdate()
        {
            // Cables are laid out in board pixels, so redo them if the screen size changes.
            if (Graph != null && board.rect.size != lastBoardSize) LayCables();
        }

        void Refresh()
        {
            if (Graph == null) return;
            foreach (var pair in slots)
            {
                var id = pair.Key;
                if (!Graph.HasPiece(id))
                {
                    pair.Value.ShowOpen(Tool.SelectedTrayPiece != null);
                    continue;
                }
                var kind = Graph.KindOf(id);
                pair.Value.ShowPiece(kind, Graph.SocketsOf(id), Graph.UsedSockets(id), Tool.SelectedSlot == id,
                    kind == PieceKind.House && Graph.ReachesRouter(id), slotData[id].address);
                pair.Value.ShowLane(kind == PieceKind.House ? LaneColor(Lanes?.LaneOf(id)) : null);
            }
            // Guide the next tap: the tray blinks until a piece is picked up, then the empty spots blink.
            foreach (var t in trayViews)
                t.Show(Tool.TrayCount(t.Piece), Tool.SelectedTrayPiece == t.Piece,
                    Tool.SelectedTrayPiece == null && Tool.TrayCount(t.Piece) > 0 && HasOpenSlot());
            LayCables();
            Changed?.Invoke();
        }

        /// <summary>A colour per lane, in the order build.json lists them.</summary>
        public Color? LaneColor(string lane)
        {
            if (string.IsNullOrEmpty(lane) || Puzzle?.lanes == null) return null;
            for (var i = 0; i < Puzzle.lanes.Length; i++)
                if (Puzzle.lanes[i]?.id == lane) return LaneColors[i % LaneColors.Length];
            return null;
        }

        static readonly Color[] LaneColors = { Palette.NeonCyan, Palette.PacketYellow, Palette.PodOrange, Palette.GrassLight };

        void LayCables()
        {
            lastBoardSize = board.rect.size;
            var cables = Graph.Cables;
            while (cableViews.Count < cables.Count) cableViews.Add(CableLine.Create(this, cableLayer));
            for (var i = 0; i < cableViews.Count; i++)
            {
                if (i >= cables.Count)
                {
                    cableViews[i].gameObject.SetActive(false);
                    continue;
                }
                var c = cables[i];
                cableViews[i].Show(c, AnchorOf(c.a), AnchorOf(c.b), lastBoardSize);
            }
        }

        /// <summary>Where a slot sits on the board, in anchor space (0..1, origin bottom left).</summary>
        public Vector2 AnchorOf(string id)
        {
            var s = slotData[id];
            return new Vector2(s.x / MapWidth, 1f - s.y / MapHeight);
        }
    }
}
