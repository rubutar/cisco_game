// SHARED CONTRACT. Mirrors Resources/Data/build.json (PRD 4.10, 6.5).
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class BuildFile
    {
        public PuzzleData[] puzzles;
    }

    [Serializable]
    public class PuzzleData
    {
        public int id;
        public string name;
        public string goal;                 // goal line shown during the puzzle
        public RouterData router;
        public SlotData[] slots;            // positions in 480 x 270 map pixels, origin top-left
        public TrayEntry[] tray;
        public DoorData[] doors;
        public string[] ruleOptions;        // "everyone" | "friends" | "closed"
        public TrafficData[] traffic;
        public PassRule pass;
        public RevealData reveal;
    }

    [Serializable]
    public class RouterData
    {
        public string slot;
        public int sockets;
    }

    [Serializable]
    public class SlotData
    {
        public string id;
        public int x;
        public int y;
        public string @fixed;               // "router" | "house" | "" (empty slot for a tray piece)
        public int address;                 // houses
        public string service;              // houses: "mail" | "photos" | "games" | "home"
    }

    [Serializable]
    public class TrayEntry
    {
        public string piece;                // "switch"
        public int count;
    }

    [Serializable]
    public class DoorData
    {
        public int port;
    }

    [Serializable]
    public class TrafficData
    {
        public string id;
        public int to;                      // target house address (puzzle 1)
        public int port;                    // door number (puzzles 2 and 3)
        public string from;
        public bool friend;
        public string kind;                 // "safe" | "attack"
        public string attacker;
        public string label;

        public bool IsAttack => kind == "attack";
    }

    [Serializable]
    public class PassRule
    {
        public int maxLeaks;
        public int maxWrongBlocks;
    }

    [Serializable]
    public class RevealData
    {
        public string @byte;                // job key of the Byte who says it
        public string term;
        public string text;
    }
}
