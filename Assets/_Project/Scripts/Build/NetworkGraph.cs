// Stream B. The Day 3 network as plain data (PRD 4.10): pieces in slots and the cables between them.
// No MonoBehaviour, so the rules can be unit tested. Scene objects only display it and take taps.
using System.Collections.Generic;

namespace ByteWorld
{
    public enum PieceKind
    {
        Router,
        Switch,
        House
    }

    public enum ConnectResult
    {
        Ok,
        SamePiece,
        AlreadyConnected,
        NoFreeSocket,
        NotAllowed, // two houses can't be cabled straight to each other
        UnknownPiece
    }

    public readonly struct Cable : System.IEquatable<Cable>
    {
        public readonly string a;
        public readonly string b;

        public Cable(string a, string b)
        {
            // Stored in a fixed order so (a, b) and (b, a) are the same cable.
            if (string.CompareOrdinal(a, b) <= 0) { this.a = a; this.b = b; }
            else { this.a = b; this.b = a; }
        }

        public bool Touches(string id) => a == id || b == id;
        public string Other(string id) => a == id ? b : a;

        public bool Equals(Cable other) => a == other.a && b == other.b;
        public override bool Equals(object obj) => obj is Cable other && Equals(other);
        public override int GetHashCode() => (a?.GetHashCode() ?? 0) * 31 + (b?.GetHashCode() ?? 0);
    }

    public class NetworkGraph
    {
        class Piece
        {
            public PieceKind kind;
            public int sockets;
        }

        readonly Dictionary<string, Piece> pieces = new();
        readonly List<Cable> cables = new();

        public IReadOnlyList<Cable> Cables => cables;

        public void AddPiece(string id, PieceKind kind, int sockets) =>
            pieces[id] = new Piece { kind = kind, sockets = sockets };

        /// <summary>Removes a piece and every cable plugged into it.</summary>
        public void RemovePiece(string id)
        {
            cables.RemoveAll(c => c.Touches(id));
            pieces.Remove(id);
        }

        public bool HasPiece(string id) => pieces.ContainsKey(id);

        public PieceKind KindOf(string id) => pieces[id].kind;

        public int SocketsOf(string id) => pieces.TryGetValue(id, out var p) ? p.sockets : 0;

        public int UsedSockets(string id)
        {
            var used = 0;
            foreach (var c in cables)
                if (c.Touches(id)) used++;
            return used;
        }

        public int FreeSockets(string id) => SocketsOf(id) - UsedSockets(id);

        public bool AreConnected(string a, string b) => cables.Contains(new Cable(a, b));

        public ConnectResult CanConnect(string a, string b)
        {
            if (!pieces.TryGetValue(a, out var pa) || !pieces.TryGetValue(b, out var pb)) return ConnectResult.UnknownPiece;
            if (a == b) return ConnectResult.SamePiece;
            if (AreConnected(a, b)) return ConnectResult.AlreadyConnected;
            if (pa.kind == PieceKind.House && pb.kind == PieceKind.House) return ConnectResult.NotAllowed;
            if (FreeSockets(a) <= 0 || FreeSockets(b) <= 0) return ConnectResult.NoFreeSocket;
            return ConnectResult.Ok;
        }

        public ConnectResult Connect(string a, string b)
        {
            var result = CanConnect(a, b);
            if (result == ConnectResult.Ok) cables.Add(new Cable(a, b));
            return result;
        }

        public bool Disconnect(string a, string b) => cables.Remove(new Cable(a, b));

        /// <summary>
        /// The pieces a message travels through from the router to <paramref name="id"/> (router first,
        /// target last), or null if there's no cable path. Messages only pass through the router and
        /// switches, never through another house.
        /// </summary>
        public List<string> PathFromRouter(string id)
        {
            string router = null;
            foreach (var pair in pieces)
                if (pair.Value.kind == PieceKind.Router) { router = pair.Key; break; }
            if (router == null || !pieces.ContainsKey(id)) return null;
            if (id == router) return new List<string> { router };

            var cameFrom = new Dictionary<string, string> { [router] = null };
            var queue = new Queue<string>();
            queue.Enqueue(router);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var c in cables)
                {
                    if (!c.Touches(current)) continue;
                    var next = c.Other(current);
                    if (cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = current;
                    if (next == id) return Walk(cameFrom, id);
                    if (pieces[next].kind != PieceKind.House) queue.Enqueue(next);
                }
            }
            return null;
        }

        public bool ReachesRouter(string id) => PathFromRouter(id) != null;

        /// <summary>The router's slot id, or null.</summary>
        public string Router
        {
            get
            {
                foreach (var pair in pieces)
                    if (pair.Value.kind == PieceKind.Router) return pair.Key;
                return null;
            }
        }

        /// <summary>Every piece cabled straight to <paramref name="id"/>.</summary>
        public List<string> Neighbours(string id)
        {
            var list = new List<string>();
            foreach (var c in cables)
                if (c.Touches(id)) list.Add(c.Other(id));
            return list;
        }

        /// <summary>
        /// Shortest cable path from <paramref name="from"/> to <paramref name="to"/> that never passes through
        /// another house, optionally also avoiding the router (traffic that stays inside one switch). Null if none.
        /// </summary>
        public List<string> PathBetween(string from, string to, bool avoidRouter)
        {
            if (!pieces.ContainsKey(from) || !pieces.ContainsKey(to)) return null;
            if (from == to) return new List<string> { from };
            var cameFrom = new Dictionary<string, string> { [from] = null };
            var queue = new Queue<string>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var c in cables)
                {
                    if (!c.Touches(current)) continue;
                    var next = c.Other(current);
                    if (cameFrom.ContainsKey(next)) continue;
                    if (avoidRouter && pieces[next].kind == PieceKind.Router) continue;
                    cameFrom[next] = current;
                    if (next == to) return Walk(cameFrom, to);
                    if (pieces[next].kind != PieceKind.House) queue.Enqueue(next);
                }
            }
            return null;
        }

        static List<string> Walk(Dictionary<string, string> cameFrom, string end)
        {
            var path = new List<string>();
            for (var at = end; at != null; at = cameFrom[at]) path.Add(at);
            path.Reverse();
            return path;
        }
    }
}
