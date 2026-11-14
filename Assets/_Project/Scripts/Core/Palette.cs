// SHARED CONTRACT. The techy town palette, exactly as listed in PRD 6.6. Use these constants
// everywhere in code (camera clears, UI tints, placeholder sprites). Never type hex values elsewhere.
using UnityEngine;

namespace ByteWorld
{
    public static class Palette
    {
        public static readonly Color32 Ink = Hex(0x1D1B33);          // outlines, text, shadows
        public static readonly Color32 ByteCream = Hex(0xFFF4D6);    // Byte bodies, house walls, UI panels
        public static readonly Color32 SignalBlue = Hex(0x4A5FC1);   // Byte feet, doors, roofs, UI
        public static readonly Color32 NeonCyan = Hex(0x47E6FF);     // Byte bellies, cable lines, LEDs
        public static readonly Color32 AlertPink = Hex(0xFF5D73);    // antennas, alerts, shut doors, attackers' eyes
        public static readonly Color32 PacketYellow = Hex(0xFFD23F); // packets, lit windows
        public static readonly Color32 PodOrange = Hex(0xFF9A3C);    // pods, crates
        public static readonly Color32 GrassLight = Hex(0x84D86F);
        public static readonly Color32 GrassDark = Hex(0x5FBD5A);
        public static readonly Color32 LeavesDark = Hex(0x2F9A55);
        public static readonly Color32 LeavesLight = Hex(0x5FC96E);
        public static readonly Color32 RoadLight = Hex(0xF3E3B8);
        public static readonly Color32 RoadEdge = Hex(0xD9C48F);
        public static readonly Color32 WaterDeep = Hex(0x3F8CF0);
        public static readonly Color32 WaterLight = Hex(0x7CC4FF);
        public static readonly Color32 CliffDark = Hex(0x3B3F6E);
        public static readonly Color32 CliffLight = Hex(0x575D9A);
        public static readonly Color32 Wood = Hex(0xC98F4E);
        public static readonly Color32 WoodDark = Hex(0x9C6A35);

        /// <summary>Alert mood: the cyan belly swaps to this pink (PRD 6.6).</summary>
        public static readonly Color32 AlertBelly = Hex(0xFFB3BD);

        /// <summary>Sprite grid indices 1-5: ink, blue, cyan, cream, pink. Index 0 is unused ('.' is transparent).</summary>
        public static readonly Color32[] GridNormal = { Clear, Ink, SignalBlue, NeonCyan, ByteCream, AlertPink };

        /// <summary>Alert mood: same as normal with the alert belly.</summary>
        public static readonly Color32[] GridAlert = { Clear, Ink, SignalBlue, AlertBelly, ByteCream, AlertPink };

        /// <summary>Dim mood (snatched Bytes, collapse), from the screen-flow reference.</summary>
        public static readonly Color32[] GridDim =
        {
            Clear, Hex(0x2A2940), Hex(0x4F5068), Hex(0x7B7D92), Hex(0xB8B7C4), Hex(0x6F6878)
        };

        static Color32 Clear => new(0, 0, 0, 0);

        static Color32 Hex(int rgb) => new((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
