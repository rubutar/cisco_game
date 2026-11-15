// Stream A. A job Byte on the world map (A-06): idle bob, tap for its job line in peacetime, and the alert look
// (pink belly, flashing "!" mark, faster bob) while AlertController has it alerting. The Twins are one job
// with two Bytes.
using UnityEngine;

namespace ByteWorld
{
    public class JobByte : MonoBehaviour
    {
        public JobId Job { get; private set; }
        public bool Alerting { get; private set; }

        SpriteRenderer[] bodies;
        Sprite[] normals;
        Sprite[] alerts;
        Sprite[] dims;
        bool bob = true;
        bool locked;
        Vector3[] basePositions;
        SpriteRenderer mark;
        SpriteRenderer patienceBack;
        SpriteRenderer patienceFill;
        Sprite normal;
        Sprite alert;
        float timer;
        bool frame;

        /// <summary>Spots are each Byte's top-left in map pixels. With bob off the Bytes stand still (on the hi-fi
        /// map they cover the Bytes baked into the art).</summary>
        public void Init(JobId job, Vector2Int[] spots, bool bob = true)
        {
            Job = job;
            this.bob = bob;
            normal = PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, $"{job} normal");
            alert = PixelArt.GridSprite(PixelArt.Byte, Palette.GridAlert, $"{job} alert");

            bodies = new SpriteRenderer[spots.Length];
            normals = new Sprite[spots.Length];
            alerts = new Sprite[spots.Length];
            dims = new Sprite[spots.Length];
            basePositions = new Vector3[spots.Length];
            for (var i = 0; i < spots.Length; i++)
            {
                // Spots are the sprite's top-left in map pixels; the sprite's pivot is its bottom centre.
                basePositions[i] = PixelArt.MapToWorld(spots[i].x + 6, spots[i].y + 12);
                var look = SpriteName(job, i);
                normals[i] = PixelArt.Load($"Characters/map_{look}", () => normal);
                alerts[i] = PixelArt.Load($"Characters/map_{look}_alert", () => alert);
                var n = normals[i];
                dims[i] = PixelArt.Load($"Characters/map_{look}_dim", () => n);
                bodies[i] = PixelArt.AddSprite(transform, normals[i], $"Body {i + 1}", 10);
                bodies[i].transform.localPosition = basePositions[i];
                Tappable.Add(bodies[i].gameObject, bodies[i]).Tapped += OnTapped;
            }

            mark = PixelArt.AddSprite(transform, PixelArt.Load("Characters/alert_mark", () => PixelArt.GridSprite(PixelArt.AlertMark, Palette.GridNormal, "Alert mark")), "Alert mark", 20);
            // Above the head: the map sprites are 16 px tall (the in-code fallback 12 px).
            var head = normals[0] != null ? normals[0].rect.height : 12f;
            mark.transform.localPosition = basePositions[0] + new Vector3(0.5f, (head + 3) * PixelArt.Pixel, 0f);
            mark.enabled = false;

            // Day 2 patience bar (A-15): drains while this Byte waits for help.
            var back = new PixelCanvas(16, 4);
            back.Fill(0, 0, 16, 4, Palette.Ink);
            patienceBack = PixelArt.AddSprite(transform, back.ToSprite("Patience back", new Vector2(0f, 0.5f)), "Patience", 21);
            patienceBack.transform.localPosition = basePositions[0] + new Vector3(-8 * PixelArt.Pixel, (head + 14) * PixelArt.Pixel, 0f); // above the "!" mark
            var fill = new PixelCanvas(1, 2);
            fill.Fill(0, 0, 1, 2, Palette.AlertPink);
            patienceFill = PixelArt.AddSprite(patienceBack.transform, fill.ToSprite("Patience fill", new Vector2(0f, 0.5f)), "Fill", 22);
            patienceFill.transform.localPosition = new Vector3(PixelArt.Pixel, 0f, 0f);
            SetPatience(-1f);
        }

        /// <summary>Shows the patience bar at 0..1 full; a negative value hides it.</summary>
        public void SetPatience(float fraction)
        {
            var show = fraction >= 0f;
            patienceBack.enabled = show;
            patienceFill.enabled = show;
            if (show) patienceFill.transform.localScale = new Vector3(Mathf.Round(14f * Mathf.Clamp01(fraction)), 1f, 1f);
        }

        public void SetAlerting(bool on)
        {
            Alerting = on;
            if (!on) SetPatience(-1f);
            ShowLook();
            mark.enabled = on;
        }

        /// <summary>A locked station's Byte is grey (dim) unless it's alerting.</summary>
        public void SetLocked(bool on)
        {
            locked = on;
            ShowLook();
        }

        void ShowLook()
        {
            for (var i = 0; i < bodies.Length; i++) bodies[i].sprite = Alerting ? alerts[i] : locked ? dims[i] : normals[i];
        }

        /// <summary>Sprite file per job Byte: scout, gigi, uno and due (the Twins), patch.</summary>
        public static string SpriteName(JobId job, int index = 0) => job switch
        {
            JobId.Scout => "scout",
            JobId.Gigi => "gigi",
            JobId.Twins => index == 0 ? "uno" : "due",
            _ => "patch"
        };

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            frame = !frame;
            timer = Alerting ? 0.2f : 0.5f;
            for (var i = 0; i < bodies.Length && bob; i++)
            {
                // One-pixel bob; the Twins bob out of step with each other.
                var up = frame ^ (i % 2 == 1);
                bodies[i].transform.localPosition = basePositions[i] + (up ? new Vector3(0f, PixelArt.Pixel, 0f) : Vector3.zero);
            }
            if (Alerting) mark.enabled = frame;
        }

        void OnTapped()
        {
            if (Alerting)
            {
                // v2: tapping a flashing station Byte walks Pax to its station (the popup does the rest).
                var station = CheckpointManager.Instance?.StationForJob(Job.ToKey());
                if (station != null) GameEvents.RaiseUiAction($"walk:{station}");
                else AlertController.Instance?.OnJobTapped(Job);
                return;
            }
            var state = Services.Flow.State;
            if (state != GameState.WorldPeace && state != GameState.Alert) return;
            var line = DataLoader.Instance?.GetJobDialogue(Job)?.jobLine;
            if (!string.IsNullOrEmpty(line)) Services.Dialogue.Say(Job.ToKey(), line);
        }
    }
}
