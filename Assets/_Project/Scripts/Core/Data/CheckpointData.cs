// SHARED CONTRACT. Mirrors Resources/Data/checkpoints.json (PRD-v2 4.2a, 4.4.4, 6.5).
using System;

namespace ByteWorld.Data
{
    [Serializable]
    public class CheckpointsFile
    {
        public MapPoint plaza;
        public CheckpointData[] checkpoints;
    }

    [Serializable]
    public class MapPoint
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class CheckpointData
    {
        public string id;                   // "harbour", "gate", "vault", "workshop", "observatory", "switchyard", "tower"
        public int quest;
        public string station;              // "Scout's Harbour"
        public string character;            // job key of the station Byte
        public string job;                  // what Go in starts: scout | gigi | twins | patch | hunt | build | siege
        public string tool;                 // "Cisco Umbrella"
        public int x;                       // door position, 480 x 270 map pixels, origin top-left
        public int y;
        public MapPoint[] path;             // road waypoints from the plaza to the door
        public string[] requires;           // checkpoint ids that must be done first
        public string line;                 // one sentence, <= 110 characters
        public string without;              // <= 40 characters
        public string with;                 // <= 40 characters
        public MaterialsData materials;
    }

    [Serializable]
    public class MaterialsData
    {
        public string what;                 // 1-2 sentences
        public string[] withoutLines;       // 3 lines, <= 60 characters
        public string[] withLines;          // 3 lines, <= 60 characters
        public string realWorld;
        public string learnMoreLabel;
        public string learnMoreUrl;
    }
}
