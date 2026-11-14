// Stream A. Small helpers the job scenes share: world-space labels and a walker that moves to a point.
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public static class JobVisuals
    {
        /// <summary>A world-space TextMeshPro label. size 3 reads at about 6 map pixels tall.</summary>
        public static TextMeshPro Label(Transform parent, string text, Vector3 localPosition, float size, Color color, int sortingOrder, float width = 6f)
        {
            var label = new GameObject("Label").AddComponent<TextMeshPro>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(width, size * 0.2f);
            label.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return label;
        }
    }

    /// <summary>Moves its object to a target at a steady speed; optionally destroys it on arrival.</summary>
    public class Mover : MonoBehaviour
    {
        Vector3 target;
        float speed;
        bool destroyOnArrival;
        bool moving;

        public void MoveTo(Vector2 position, float unitsPerSecond, bool destroyWhenThere = false)
        {
            target = position;
            speed = unitsPerSecond;
            destroyOnArrival = destroyWhenThere;
            moving = true;
        }

        void Update()
        {
            if (!moving) return;
            var p = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
            transform.localPosition = p;
            if ((p - target).sqrMagnitude > 0.0001f) return;
            moving = false;
            if (destroyOnArrival) Destroy(gameObject);
        }
    }
}
