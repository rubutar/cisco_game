// Stream B. Shared helpers for the checkpoint UI (PRD-v2 4.2a): where a station sits on screen, which badge
// sprite shows its tool, and its Field Card. Reads ICheckpointService and dialogue.json; no game text here.
using ByteWorld.Data;
using UnityEngine;

namespace ByteWorld
{
    public static class Stations
    {
        public static bool IsMapState(GameState s) => s is GameState.WorldPeace or GameState.Alert or GameState.FinalAttack;

        /// <summary>A map point (480 × 270 map pixels, origin top-left) in a rect's local space; false if off screen.</summary>
        public static bool MapToLocal(RectTransform space, float mapX, float mapY, out Vector2 local)
        {
            local = Vector2.zero;
            var cam = Camera.main;
            if (cam == null) return false;
            var screen = cam.WorldToScreenPoint(PixelArt.MapToWorld(mapX, mapY));
            if (screen.z < 0) return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(space, screen, null, out local);
        }

        /// <summary>Screen point of a station's door, for the iris and the materials ring.</summary>
        public static bool ScreenPoint(CheckpointData c, out Vector2 screen)
        {
            screen = Vector2.zero;
            var cam = Camera.main;
            if (c == null || cam == null) return false;
            screen = cam.WorldToScreenPoint(PixelArt.MapToWorld(c.x, c.y - 20));
            return true;
        }

        /// <summary>Resources/Sprites/UI/badge_*: one colour per Cisco tool (Person A's art), dim when locked.</summary>
        public static Sprite Badge(CheckpointData c, bool dim)
        {
            var key = BadgeKey(c?.tool);
            if (key == null) return null;
            return UiKit.Sprite($"UI/badge_{key}{(dim ? "_dim" : "")}") ?? UiKit.Sprite($"UI/badge_{key}");
        }

        static string BadgeKey(string tool)
        {
            var t = (tool ?? "").ToLowerInvariant();
            if (t.Contains("umbrella")) return "umbrella";
            if (t.Contains("firewall")) return "firewall";
            if (t.Contains("duo")) return "duo";
            if (t.Contains("endpoint")) return "endpoint";
            if (t.Contains("splunk")) return "splunk";
            if (t.Contains("catalyst")) return "catalyst";
            if (t.Contains("thousandeyes")) return "thousandeyes";
            return null;
        }

        /// <summary>The Field Card a station gives: a job's day card, the hunt's, or the build puzzle reveal.</summary>
        public static FieldCardData FieldCard(CheckpointData c)
        {
            if (c == null) return null;
            switch (c.job)
            {
                case "hunt":
                    return Services.Data?.Hunt?.fieldCard;
                case "build":
                case "siege":
                {
                    var puzzles = Services.Data?.Build?.puzzles;
                    if (puzzles == null || puzzles.Length == 0) return null;
                    var p = c.job == "siege" ? puzzles[puzzles.Length - 1] : puzzles[0];
                    return p?.reveal == null ? null : new FieldCardData { term = p.reveal.term, text = p.reveal.text, cisco = c.tool };
                }
                default:
                    return JobIds.TryParse(c.job, out var job) ? UiStrings.JobDay(job, c.quest)?.fieldCard : null;
            }
        }

        public static Color StateColor(CheckpointState state) => state switch
        {
            CheckpointState.Alert => UiStyle.Danger,
            CheckpointState.Done => UiStyle.Done,
            _ => UiStyle.Locked
        };
    }
}
