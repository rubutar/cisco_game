// Stream B. Doors and rules for Quests 3 and 4 (PRD-v2 4.7, B-12), plain C#.
//   Doors: each door (build.json doors[].port) is set to one of ruleOptions[] (Everyone, Friends only, Closed).
//   Rules: build.json rules[] {match, action} are checked first, in the player's order; the first match decides.
//          match tokens: "src=<from>", "door=<port>", "friends" (all must match); action "allow" | "drop".
//   Then the door setting decides. A door with no setting and anything else unmatched is dropped.
using System.Collections.Generic;
using ByteWorld.Data;

namespace ByteWorld
{
    public enum DoorSetting
    {
        Everyone,
        Friends,
        Closed
    }

    public class DoorRules
    {
        readonly PuzzleData puzzle;
        readonly Dictionary<int, int> doorOption = new(); // port -> index in ruleOptions
        readonly List<int> order = new();                  // indices into puzzle.rules, in the player's order

        public bool HasDoors => puzzle?.doors != null && puzzle.doors.Length > 0;
        public bool HasRules => puzzle?.rules != null && puzzle.rules.Length > 0;
        public string[] Options => puzzle?.ruleOptions;
        public IReadOnlyList<int> Order => order;

        public DoorRules(PuzzleData puzzle)
        {
            this.puzzle = puzzle;
            if (HasDoors)
                foreach (var d in puzzle.doors) doorOption[d.port] = 0;
            if (HasRules)
            {
                // build.json lists the rules in the right order; the player starts from a fixed scramble
                // (reversed), so putting them in order is the puzzle and the same every time.
                for (var i = puzzle.rules.Length - 1; i >= 0; i--) order.Add(i);
            }
        }

        // ---------- Doors ----------

        public int OptionIndex(int port) => doorOption.TryGetValue(port, out var i) ? i : 0;

        /// <summary>Moves a door on to its next setting (taps cycle through ruleOptions).</summary>
        public void CycleDoor(int port)
        {
            var count = Options?.Length ?? 0;
            if (count == 0 || !doorOption.ContainsKey(port)) return;
            doorOption[port] = (doorOption[port] + 1) % count;
        }

        public void SetDoor(int port, int optionIndex)
        {
            if (doorOption.ContainsKey(port)) doorOption[port] = optionIndex;
        }

        public DoorSetting SettingOf(int port)
        {
            var options = Options;
            if (options == null || options.Length == 0) return DoorSetting.Everyone;
            return Parse(options[OptionIndex(port) % options.Length]);
        }

        /// <summary>"everyone" / "Friends only" / "Closed": read by keyword, so the JSON can use display words.</summary>
        public static DoorSetting Parse(string option)
        {
            var o = (option ?? "").ToLowerInvariant();
            if (o.Contains("friend")) return DoorSetting.Friends;
            if (o.Contains("close") || o.Contains("shut")) return DoorSetting.Closed;
            return DoorSetting.Everyone;
        }

        // ---------- Rules ----------

        public RuleData RuleAt(int position) => puzzle.rules[order[position]];

        /// <summary>Moves the rule at <paramref name="position"/> one place up (the top one goes to the bottom).</summary>
        public void MoveUp(int position)
        {
            if (position < 0 || position >= order.Count || order.Count < 2) return;
            var target = position == 0 ? order.Count - 1 : position - 1;
            (order[position], order[target]) = (order[target], order[position]);
        }

        public static bool Matches(RuleData rule, TrafficData t)
        {
            if (rule == null || string.IsNullOrEmpty(rule.match)) return false;
            foreach (var token in rule.match.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
            {
                var t0 = token.ToLowerInvariant();
                if (t0.StartsWith("src="))
                {
                    if (!string.Equals(token.Substring(4), t.from, System.StringComparison.OrdinalIgnoreCase)) return false;
                }
                else if (t0.StartsWith("door="))
                {
                    if (!int.TryParse(token.Substring(5), out var port) || port != t.port) return false;
                }
                else if (t0.StartsWith("friend"))
                {
                    if (!t.friend) return false;
                }
                else return false; // unknown token: never match rather than guess
            }
            return true;
        }

        // ---------- Decision ----------

        /// <summary>Whether this message gets in. Puzzles with neither doors nor rules let everything in.</summary>
        public bool Allows(TrafficData t)
        {
            if (!HasDoors && !HasRules) return true;
            if (HasRules)
                foreach (var i in order)
                {
                    var rule = puzzle.rules[i];
                    if (Matches(rule, t)) return string.Equals(rule.action, "allow", System.StringComparison.OrdinalIgnoreCase);
                }
            if (HasDoors && doorOption.ContainsKey(t.port))
            {
                return SettingOf(t.port) switch
                {
                    DoorSetting.Everyone => true,
                    DoorSetting.Friends => t.friend,
                    _ => false
                };
            }
            return false; // anything unmatched is dropped
        }
    }
}
