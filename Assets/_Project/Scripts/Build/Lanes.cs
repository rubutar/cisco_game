// Stream B. Quest 3 lanes (PRD-v2 4.7, B-05), plain C#: build.json lanes[] say which houses (pods) belong to
// which lane. A managed switch carries one lane, so a lane is right when every house in it hangs off a switch
// that carries only that lane. The router keeps lanes apart: traffic from one lane to another is stopped there,
// so the test packet from an infected pod only gets through if it shares a switch with its target.
using System.Collections.Generic;
using ByteWorld.Data;

namespace ByteWorld
{
    public class Lanes
    {
        readonly Dictionary<string, string> laneOf = new();     // house slot -> lane id
        readonly Dictionary<string, List<string>> slotsOf = new();

        public bool Any => laneOf.Count > 0;

        public Lanes(PuzzleData puzzle)
        {
            foreach (var lane in puzzle?.lanes ?? System.Array.Empty<LaneData>())
            {
                if (lane == null || string.IsNullOrEmpty(lane.id)) continue;
                slotsOf[lane.id] = new List<string>();
                foreach (var slot in lane.slots ?? System.Array.Empty<string>())
                {
                    laneOf[slot] = lane.id;
                    slotsOf[lane.id].Add(slot);
                }
            }
        }

        public string LaneOf(string slot) => laneOf.TryGetValue(slot, out var lane) ? lane : null;

        public bool IsLane(string id) => slotsOf.ContainsKey(id);

        public IReadOnlyList<string> SlotsIn(string lane) => slotsOf.TryGetValue(lane, out var list) ? list : null;

        /// <summary>Houses that are in a lane but not on a switch carrying only their lane.</summary>
        public List<string> Misplaced(NetworkGraph graph)
        {
            var wrong = new List<string>();
            foreach (var pair in laneOf)
            {
                var house = pair.Key;
                if (!graph.HasPiece(house)) continue;
                var ok = false;
                foreach (var next in graph.Neighbours(house))
                {
                    if (graph.KindOf(next) != PieceKind.Switch) continue;
                    ok = CarriesOnly(graph, next, pair.Value);
                    break;
                }
                if (!ok) wrong.Add(house);
            }
            return wrong;
        }

        bool CarriesOnly(NetworkGraph graph, string sw, string lane)
        {
            foreach (var next in graph.Neighbours(sw))
            {
                var other = LaneOf(next);
                if (other != null && other != lane) return false;
            }
            return true;
        }

        /// <summary>Where a test packet lands: a slot id, or the first house of a lane id.</summary>
        public string Resolve(string target)
        {
            if (string.IsNullOrEmpty(target)) return null;
            if (slotsOf.TryGetValue(target, out var list)) return list.Count > 0 ? list[0] : null;
            return target;
        }
    }
}
