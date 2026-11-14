// Stream B. Works out what happens to each message in a Day 3 test run (PRD 4.10). Plain C#: no
// animation, no events, so the outcome rules can be unit tested. BuildManager plays the result back.
//   Delivered - a cable path leads from the router to the right house
//   Lost      - no path: the house isn't connected (counts as a complaint)
//   Blocked / Leaked - door rules, added with puzzles 2 and 3 (B-12)
using System.Collections.Generic;
using ByteWorld.Data;

namespace ByteWorld
{
    public class PacketPlan
    {
        public TrafficData traffic;
        public string targetSlot;      // house the message is meant for ("" if no house has that address)
        public List<string> path;      // router ... target; null when Lost
        public PacketOutcome outcome;
    }

    public class TrafficRun
    {
        public readonly List<PacketPlan> packets = new();
        public int Delivered { get; private set; }
        public int Lost { get; private set; }
        public int Blocked { get; private set; }
        public int Leaked { get; private set; }

        /// <summary>Complaints the run causes: friends whose message didn't arrive (PRD 4.5).</summary>
        public int Complaints => Lost + Blocked;

        public static TrafficRun Plan(NetworkGraph graph, PuzzleData puzzle)
        {
            var run = new TrafficRun();
            var houseByAddress = new Dictionary<int, string>();
            foreach (var s in puzzle.slots ?? System.Array.Empty<SlotData>())
                if (s.@fixed == "house") houseByAddress[s.address] = s.id;

            foreach (var t in puzzle.traffic ?? System.Array.Empty<TrafficData>())
            {
                var plan = new PacketPlan { traffic = t, targetSlot = "" };
                if (houseByAddress.TryGetValue(t.to, out var slot))
                {
                    plan.targetSlot = slot;
                    plan.path = graph.PathFromRouter(slot);
                }
                plan.outcome = plan.path != null ? PacketOutcome.Delivered : PacketOutcome.Lost;
                run.Add(plan);
            }
            return run;
        }

        void Add(PacketPlan plan)
        {
            packets.Add(plan);
            switch (plan.outcome)
            {
                case PacketOutcome.Delivered: Delivered++; break;
                case PacketOutcome.Lost: Lost++; break;
                case PacketOutcome.Blocked: Blocked++; break;
                case PacketOutcome.Leaked: Leaked++; break;
            }
        }

        public bool Passes(PassRule pass) => ComplaintRules.RunPasses(Leaked, Complaints, pass);

        public RunResult ToResult(int puzzleId, PassRule pass) => new()
        {
            puzzleId = puzzleId,
            delivered = Delivered,
            lost = Lost,
            blocked = Blocked,
            leaked = Leaked,
            passed = Passes(pass)
        };
    }
}
