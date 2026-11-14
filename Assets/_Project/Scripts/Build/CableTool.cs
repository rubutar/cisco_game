// Stream B. The tap rules of build mode as plain C# (PRD 4.10). Fixed slots, no dragging:
//   tap a tray piece, then an empty slot        -> place it
//   tap a piece, then another piece             -> lay a cable between them
//   tap the selected piece again                -> deselect (a placed piece with no cables goes back to the tray)
//   tap a cable                                 -> remove it
using System.Collections.Generic;

namespace ByteWorld
{
    public enum TapOutcome
    {
        Nothing,
        TraySelected,
        TrayDeselected,
        Placed,
        PieceSelected,
        PieceDeselected,
        PickedUp,
        Connected,
        Rejected,     // see LastRejection
        CableRemoved
    }

    public class CableTool
    {
        readonly NetworkGraph graph;
        readonly HashSet<string> openSlots;          // slots that take a tray piece
        readonly Dictionary<string, int> tray;       // piece key -> count left
        readonly Dictionary<string, string> placed = new(); // slot id -> piece key placed from the tray
        readonly System.Func<string, (PieceKind kind, int sockets)> describePiece;

        public string SelectedTrayPiece { get; private set; }
        public string SelectedSlot { get; private set; }
        public ConnectResult LastRejection { get; private set; }

        public CableTool(NetworkGraph graph, IEnumerable<string> openSlots, Dictionary<string, int> tray,
            System.Func<string, (PieceKind kind, int sockets)> describePiece)
        {
            this.graph = graph;
            this.openSlots = new HashSet<string>(openSlots);
            this.tray = tray;
            this.describePiece = describePiece;
        }

        public int TrayCount(string piece) => tray.TryGetValue(piece, out var n) ? n : 0;

        public bool IsPlacedFromTray(string slot) => placed.ContainsKey(slot);

        public void ClearSelection()
        {
            SelectedTrayPiece = null;
            SelectedSlot = null;
        }

        public TapOutcome TapTray(string piece)
        {
            SelectedSlot = null;
            if (SelectedTrayPiece == piece)
            {
                SelectedTrayPiece = null;
                return TapOutcome.TrayDeselected;
            }
            if (TrayCount(piece) <= 0) return TapOutcome.Nothing;
            SelectedTrayPiece = piece;
            return TapOutcome.TraySelected;
        }

        public TapOutcome TapSlot(string slot)
        {
            if (SelectedTrayPiece != null)
            {
                if (openSlots.Contains(slot) && !graph.HasPiece(slot)) return Place(slot);
                SelectedTrayPiece = null; // tapped a piece instead: fall through to cabling
            }

            if (!graph.HasPiece(slot)) return TapOutcome.Nothing;

            if (SelectedSlot == null)
            {
                SelectedSlot = slot;
                return TapOutcome.PieceSelected;
            }

            if (SelectedSlot == slot)
            {
                SelectedSlot = null;
                if (placed.ContainsKey(slot) && graph.UsedSockets(slot) == 0) return PickUp(slot);
                return TapOutcome.PieceDeselected;
            }

            var result = graph.Connect(SelectedSlot, slot);
            if (result == ConnectResult.Ok)
            {
                SelectedSlot = null;
                return TapOutcome.Connected;
            }
            LastRejection = result;
            return TapOutcome.Rejected;
        }

        public TapOutcome TapCable(Cable cable)
        {
            ClearSelection();
            return graph.Disconnect(cable.a, cable.b) ? TapOutcome.CableRemoved : TapOutcome.Nothing;
        }

        TapOutcome Place(string slot)
        {
            var piece = SelectedTrayPiece;
            var (kind, sockets) = describePiece(piece);
            graph.AddPiece(slot, kind, sockets);
            placed[slot] = piece;
            tray[piece] = TrayCount(piece) - 1;
            if (tray[piece] <= 0) SelectedTrayPiece = null;
            return TapOutcome.Placed;
        }

        TapOutcome PickUp(string slot)
        {
            var piece = placed[slot];
            placed.Remove(slot);
            graph.RemovePiece(slot);
            tray[piece] = TrayCount(piece) + 1;
            return TapOutcome.PickedUp;
        }
    }
}
