// Stream A. An ordinary Byte walking a cable road back and forth (A-07, PRD 4.2). Every third one carries a
// glowing data orb. Tap one for a line from roaming.json. Moves in whole pixels so it stays crisp.
// Crowd reactions (A-18): stops to look while an alert is on, hops at the end of the day, and vanishes on
// collapse, one Byte after another.
using UnityEngine;

namespace ByteWorld
{
    public class RoamingByte : MonoBehaviour
    {
        static readonly string[] OrbGrid =
        {
            "..11..",
            ".1451.",
            "145551",
            "155551",
            ".1551.",
            "..11..",
        };

        Vector2[] path;     // world positions
        float[] cumulative; // distance from the start to each point
        float distance;
        float direction = 1f;
        float speed;
        SpriteRenderer body;
        float bobTimer;
        bool bobUp;
        int lineIndex;
        bool cheering;
        float vanishAt = -1f;

        public void Init(Vector2Int[] mapPath, float startFraction, float unitsPerSecond, bool carriesOrb, int seed)
        {
            path = new Vector2[mapPath.Length];
            cumulative = new float[mapPath.Length];
            for (var i = 0; i < mapPath.Length; i++)
            {
                path[i] = PixelArt.MapToWorld(mapPath[i].x, mapPath[i].y + 4); // feet a little below the road's centre line
                if (i > 0) cumulative[i] = cumulative[i - 1] + Vector2.Distance(path[i - 1], path[i]);
            }
            distance = startFraction * cumulative[cumulative.Length - 1];
            direction = seed % 2 == 0 ? 1f : -1f;
            speed = unitsPerSecond;
            lineIndex = seed;

            body = PixelArt.AddSprite(transform, PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Roaming Byte"), "Body", 0);
            Tappable.Add(body.gameObject, body, 0.3f).Tapped += Talk;
            if (carriesOrb)
            {
                var orb = PixelArt.AddSprite(body.transform, PixelArt.GridSprite(OrbGrid, Palette.GridNormal, "Orb"), "Orb", 0);
                orb.transform.localPosition = new Vector3(7 * PixelArt.Pixel, 2 * PixelArt.Pixel, 0f);
            }
            Place();
        }

        void OnEnable()
        {
            GameEvents.DayEnded += OnDayEnded;
            GameEvents.DayStarted += OnDayStarted;
            GameEvents.Collapsed += OnCollapsed;
        }

        void OnDisable()
        {
            GameEvents.DayEnded -= OnDayEnded;
            GameEvents.DayStarted -= OnDayStarted;
            GameEvents.Collapsed -= OnCollapsed;
        }

        void OnDayEnded(int day) => cheering = true;

        void OnDayStarted(int day, DayMode mode) => cheering = false;

        // Each Byte leaves a moment after the one before it.
        void OnCollapsed(int day) => vanishAt = Time.time + 0.3f * (lineIndex % 12);

        void Update()
        {
            if (path == null) return;
            if (vanishAt >= 0f)
            {
                if (Time.time >= vanishAt) gameObject.SetActive(false);
                return;
            }

            // Everyone stops and looks while an alert is on; otherwise they walk.
            var state = Services.Flow.State;
            var watching = state == GameState.Alert;
            if (!watching && !cheering)
            {
                var total = cumulative[cumulative.Length - 1];
                distance += direction * speed * Time.deltaTime;
                if (distance > total || distance < 0f)
                {
                    direction = -direction;
                    distance = Mathf.Clamp(distance, 0f, total);
                }
            }

            bobTimer -= Time.deltaTime;
            if (bobTimer <= 0f)
            {
                bobUp = !bobUp;
                bobTimer = cheering ? 0.1f : watching ? 0.6f : 0.15f;
            }
            Place();
        }

        void Place()
        {
            var i = 1;
            while (i < cumulative.Length - 1 && cumulative[i] < distance) i++;
            var segment = cumulative[i] - cumulative[i - 1];
            var t = segment > 0f ? (distance - cumulative[i - 1]) / segment : 0f;
            var p = PixelArt.Snap(Vector2.Lerp(path[i - 1], path[i], t));
            transform.localPosition = p;
            // A cheer is a two-pixel hop; walking and watching bob by one pixel.
            body.transform.localPosition = bobUp ? new Vector3(0f, (cheering ? 2 : 1) * PixelArt.Pixel, 0f) : Vector3.zero;
            // Lower on screen draws in front; job Bytes (order 10) stay on top of the crowd.
            body.sortingOrder = Mathf.Clamp(-Mathf.RoundToInt(p.y / PixelArt.Pixel), -200, 9) - 200;
            foreach (var child in body.GetComponentsInChildren<SpriteRenderer>())
                if (child != body) child.sortingOrder = body.sortingOrder + 1;
        }

        void Talk()
        {
            var state = Services.Flow.State;
            if (state != GameState.WorldPeace && state != GameState.Alert) return;
            var lines = DataLoader.Instance?.Roaming?.lines;
            if (lines == null || lines.Length == 0) return;
            Services.Dialogue.Say("", lines[lineIndex % lines.Length]);
            lineIndex++;
        }
    }
}
