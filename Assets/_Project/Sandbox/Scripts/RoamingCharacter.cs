// SANDBOX. A Byte that wanders the field: walk to a random spot, pause, pick another. Two-frame bob
// while walking (fast) and idling (slow), moves in whole pixels so it stays crisp.
using UnityEngine;

namespace ByteWorld.Sandbox
{
    public class RoamingCharacter : MonoBehaviour
    {
        const float Pixel = 1f / SpriteGrids.PixelsPerUnit;

        [SerializeField] float speed = 1.6f;              // world units per second (about 26 px/s)
        [SerializeField] Vector2 pauseSeconds = new(0.6f, 2f);
        [SerializeField] float margin = 1.5f;              // keep away from the screen edge

        SpriteRenderer body;
        System.Random random;
        Vector2 position;
        Vector2 target;
        float pauseLeft;
        float bobTimer;
        bool bobUp;

        public void Init(Sprite sprite, int mapX, int mapY, int seed)
        {
            random = new System.Random(seed);
            position = SandboxField.MapToWorld(mapX, mapY);

            var visual = new GameObject("Body") { hideFlags = HideFlags.DontSave };
            visual.transform.SetParent(transform, false);
            body = visual.AddComponent<SpriteRenderer>();
            body.sprite = sprite;

            PickTarget();
            Apply();
        }

        void Update()
        {
            if (body == null) return;

            var moving = pauseLeft <= 0f;
            if (moving)
            {
                position = Vector2.MoveTowards(position, target, speed * Time.deltaTime);
                if ((position - target).sqrMagnitude < 0.0001f)
                {
                    pauseLeft = Mathf.Lerp(pauseSeconds.x, pauseSeconds.y, (float)random.NextDouble());
                    PickTarget();
                }
            }
            else
            {
                pauseLeft -= Time.deltaTime;
            }

            bobTimer -= Time.deltaTime;
            if (bobTimer <= 0f)
            {
                bobUp = !bobUp;
                bobTimer = moving ? 0.15f : 0.5f;
            }

            Apply();
        }

        void PickTarget()
        {
            var bounds = SandboxField.WorldBounds;
            target = new Vector2(
                Mathf.Lerp(bounds.xMin + margin, bounds.xMax - margin, (float)random.NextDouble()),
                Mathf.Lerp(bounds.yMin + margin, bounds.yMax - margin, (float)random.NextDouble()));
        }

        void Apply()
        {
            // Snap to the pixel grid; the bob lifts the body by one pixel.
            var snapped = new Vector3(Mathf.Round(position.x / Pixel) * Pixel, Mathf.Round(position.y / Pixel) * Pixel, 0f);
            transform.localPosition = snapped;
            body.transform.localPosition = new Vector3(0f, bobUp ? Pixel : 0f, 0f);
            // Lower on screen draws in front.
            body.sortingOrder = -Mathf.RoundToInt(snapped.y / Pixel);
        }
    }
}
