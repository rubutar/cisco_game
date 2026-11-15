// Stream B. The iris transition (PRD-v2 6.6, B-17): a circle that opens over 0.45 s.
//   Going in at a station: the station world appears through a circle growing from the station's door.
//   Coming back to the map after a station: the map opens from the same station.
// An ink cover with a round hole (UiKit.Hole) plus four ink bars around it, so it needs no shader.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class IrisTransition : MonoBehaviour
    {
        [SerializeField] RectTransform root;

        RectTransform hole;
        RectTransform[] bars = new RectTransform[4];
        Vector2 stationScreen;
        bool hasStation;
        bool inStation;
        Coroutine running;

        void Awake()
        {
            hole = UiKit.Rect("Hole", root);
            hole.anchorMin = hole.anchorMax = new Vector2(0.5f, 0.5f);
            var image = UiKit.Image(hole.gameObject, Palette.Ink, true);
            image.sprite = UiKit.Hole();
            for (var i = 0; i < 4; i++)
            {
                bars[i] = UiKit.Rect($"Bar{i}", root);
                bars[i].anchorMin = bars[i].anchorMax = new Vector2(0.5f, 0.5f);
                UiKit.Image(bars[i].gameObject, Palette.Ink, true);
            }
            root.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.UiCommandRequested += OnCommand;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.UiCommandRequested -= OnCommand;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnCommand(string command)
        {
            if (command == null || !command.StartsWith("goin:")) return;
            var c = Services.Checkpoints?.Get(command.Substring(5));
            if (c == null || Services.Checkpoints.StateOf(c.id) == CheckpointState.Locked) return;
            hasStation = Stations.ScreenPoint(c, out stationScreen);
            inStation = true;
            Play(stationScreen);
        }

        void OnStateChanged(GameState from, GameState to)
        {
            // Back on the map after a station (Field Card closed, build mode or the hunt done).
            if (inStation && to == GameState.WorldPeace && from != GameState.WorldPeace)
            {
                inStation = false;
                if (hasStation) Play(stationScreen);
            }
            if (to == GameState.Title || to == GameState.Collapse) inStation = false;
        }

        void Play(Vector2 screenPoint)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Open(screenPoint));
        }

        IEnumerator Open(Vector2 screenPoint)
        {
            root.gameObject.SetActive(true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, null, out var centre);
            var size = root.rect.size;
            var maxRadius = (new Vector2(Mathf.Abs(centre.x) + size.x * 0.5f, Mathf.Abs(centre.y) + size.y * 0.5f)).magnitude;
            for (var t = 0f; t < UiStyle.IrisSeconds; t += Time.unscaledDeltaTime)
            {
                var k = t / UiStyle.IrisSeconds;
                Layout(centre, Mathf.Lerp(0f, maxRadius, k * k), size);
                yield return null;
            }
            root.gameObject.SetActive(false);
            running = null;
        }

        void Layout(Vector2 c, float radius, Vector2 size)
        {
            var d = Mathf.Max(2f, radius * 2f);
            hole.anchoredPosition = c;
            hole.sizeDelta = new Vector2(d, d);
            var big = size * 2f + new Vector2(d, d);
            // Left, right, top, bottom of the hole square.
            Bar(bars[0], new Vector2(c.x - d / 2f - big.x / 2f, c.y), new Vector2(big.x, big.y * 2f));
            Bar(bars[1], new Vector2(c.x + d / 2f + big.x / 2f, c.y), new Vector2(big.x, big.y * 2f));
            Bar(bars[2], new Vector2(c.x, c.y + d / 2f + big.y / 2f), new Vector2(d, big.y));
            Bar(bars[3], new Vector2(c.x, c.y - d / 2f - big.y / 2f), new Vector2(d, big.y));
        }

        static void Bar(RectTransform r, Vector2 position, Vector2 size)
        {
            r.anchoredPosition = position;
            r.sizeDelta = size;
        }
    }
}
