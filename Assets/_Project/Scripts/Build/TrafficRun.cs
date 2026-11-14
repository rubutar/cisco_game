// Stream B. Works out what happens to each message in a test run (PRD 4.10, PRD-v2 4.7). Plain C#: no
// animation, no events, so the outcome rules can be unit tested. BuildRunner plays the result back.
//   Delivered - let in, and a cable path leads from the router to the right house
//   Lost      - let in, but no path: the house isn't connected (a complaint)
//   Blocked   - stopped by a door or rule: a complaint for a real citizen, a win for an attacker
//   Leaked    - an attacker got in (costs a backup)
// v2: the Quest 3 test packet from an infected pod must be stopped (lanes), and every pod must be in its lane.
using System.Collections.Generic;
using ByteWorld.Data;

namespace ByteWorld
{
    public class PacketPlan
    {
        public TrafficData traffic;
        public string targetSlot;      // house the message is meant for ("" if no house has that address)
        public string fromSlot;        // where it starts: the router, or a house for the test packet
        public List<string> path;      // fromSlot ... where it ends; null when it never leaves (Lost)
        public PacketOutcome outcome;
        public bool isTest;            // the Quest 3 test packet

        /// <summary>A real citizen whose message didn't arrive (fills the complaints meter).</summary>
        public bool Complaint => !traffic.IsAttack &&
                                 (outcome == PacketOutcome.Lost || outcome == PacketOutcome.Blocked);
    }

    public class TrafficRun
    {
        public readonly List<PacketPlan> packets = new();
        public int Delivered { get; private set; }
        public int Lost { get; private set; }
        public int Blocked { get; private set; }     // real citizens turned away
        public int Stopped { get; private set; }     // attackers turned away
        public int Leaked { get; private set; }
        public List<string> MisplacedPods { get; private set; } = new();

        /// <summary>Complaints the run causes: real citizens whose message didn't arrive (PRD 4.5).</summary>
        public int Complaints => Lost + Blocked;

        public static TrafficRun Plan(NetworkGraph graph, PuzzleData puzzle, DoorRules rules = null)
        {
            var run = new TrafficRun();
            rules ??= new DoorRules(puzzle);
            var router = graph.Router;
            var houseByAddress = new Dictionary<int, string>();
            foreach (var s in puzzle.slots ?? System.Array.Empty<SlotData>())
                if (s.@fixed == "house") houseByAddress[s.address] = s.id;

            foreach (var t in puzzle.traffic ?? System.Array.Empty<TrafficData>())
            {
                if (t == null) continue;
                var plan = new PacketPlan { traffic = t, targetSlot = "", fromSlot = router };
                if (houseByAddress.TryGetValue(t.to, out var slot)) plan.targetSlot = slot;
                var housePath = string.IsNullOrEmpty(plan.targetSlot) ? null : graph.PathFromRouter(plan.targetSlot);

                if (!rules.Allows(t))
                {
                    plan.outcome = PacketOutcome.Blocked;
                    plan.path = router != null ? new List<string> { router } : null;
                }
                else if (t.IsAttack)
                {
                    plan.outcome = PacketOutcome.Leaked;
                    plan.path = housePath ?? (router != null ? new List<string> { router } : null);
                }
                else
                {
                    plan.path = housePath;
                    plan.outcome = housePath != null ? PacketOutcome.Delivered : PacketOutcome.Lost;
                }
                run.Add(plan);
            }

            var lanes = new Lanes(puzzle);
            if (lanes.Any) run.MisplacedPods = lanes.Misplaced(graph);
            if (puzzle.testPacket != null && !string.IsNullOrEmpty(puzzle.testPacket.from))
                run.Add(PlanTestPacket(graph, puzzle.testPacket, lanes));
            return run;
        }

        // The router keeps lanes apart, so the test packet only reaches its target over a switch it shares.
        static PacketPlan PlanTestPacket(NetworkGraph graph, TestPacket test, Lanes lanes)
        {
            var target = lanes.Resolve(test.to);
            var plan = new PacketPlan
            {
                traffic = new TrafficData { id = "test", kind = "attack", from = test.from },
                fromSlot = test.from,
                targetSlot = target ?? "",
                isTest = true
            };
            var direct = target != null ? graph.PathBetween(test.from, target, avoidRouter: true) : null;
            if (direct != null)
            {
                plan.outcome = PacketOutcome.Leaked;
                plan.path = direct;
            }
            else
            {
                // Up to the router (if it can get there) and no further.
                var router = graph.Router;
                var up = router != null && graph.HasPiece(test.from) ? graph.PathBetween(test.from, router, false) : null;
                plan.outcome = PacketOutcome.Blocked;
                plan.path = up ?? (graph.HasPiece(test.from) ? new List<string> { test.from } : null);
            }
            return plan;
        }

        void Add(PacketPlan plan)
        {
            packets.Add(plan);
            switch (plan.outcome)
            {
                case PacketOutcome.Delivered: Delivered++; break;
                case PacketOutcome.Lost: Lost++; break;
                case PacketOutcome.Blocked:
                    if (plan.traffic.IsAttack) Stopped++;
                    else Blocked++;
                    break;
                case PacketOutcome.Leaked: Leaked++; break;
            }
        }

        /// <summary>The shared complaints rule, plus every pod in its lane (Quest 3).</summary>
        public bool Passes(PassRule pass) => MisplacedPods.Count == 0 && ComplaintRules.RunPasses(Leaked, Complaints, pass);

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
