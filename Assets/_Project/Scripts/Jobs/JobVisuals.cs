// Stream A. Small helpers the job scenes share: world-space labels and a walker that moves to a point.
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public static class JobVisuals
    {
        /// <summary>Labels are drawn this much larger than the size asked for, so ports and codes read on a phone.</summary>
        public const float ReadableScale = 1.7f;

        /// <summary>A world-space TextMeshPro label. size 3 reads at about 10 map pixels tall (after ReadableScale).
        /// It sits on a dark plate so it reads on any background: light text keeps its colour, dark text turns cream.
        /// A SortingGroup keeps the plate right under its own text.</summary>
        public static TextMeshPro Label(Transform parent, string text, Vector3 localPosition, float size, Color color, int sortingOrder, float width = 6f)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var group = go.AddComponent<UnityEngine.Rendering.SortingGroup>();
            group.sortingOrder = sortingOrder;

            var plate = new GameObject("Plate").AddComponent<SpriteRenderer>();
            plate.transform.SetParent(go.transform, false);
            plate.sprite = PlateSprite();
            plate.color = PlateColor;
            plate.sortingOrder = 0;

            var label = new GameObject("Text").AddComponent<TextMeshPro>();
            label.transform.SetParent(go.transform, false);
            label.fontSize = size * ReadableScale;
            label.fontStyle = FontStyles.Bold;
            label.color = IsLight(color) ? color : (Color)Palette.ByteCream;
            label.alignment = TextAlignmentOptions.Center;
            // Long titles wrap inside width (world units), so neighbouring labels never run into each other.
            label.textWrappingMode = TextWrappingModes.Normal;
            label.verticalAlignment = VerticalAlignmentOptions.Top;
            label.rectTransform.sizeDelta = new Vector2(width, size * ReadableScale * 0.2f);
            label.GetComponent<MeshRenderer>().sortingOrder = 1;
            label.text = text;
            FitPlate(plate, label);
            label.gameObject.AddComponent<LabelPlate>().Init(plate, label);
            return label;
        }

        static Color PlateColor
        {
            get
            {
                Color c = Palette.Ink;
                c.a = 0.88f;
                return c;
            }
        }
        static Sprite plateSprite;

        static Sprite PlateSprite()
        {
            if (plateSprite != null) return plateSprite;
            var tex = Texture2D.whiteTexture;
            plateSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            return plateSprite;
        }

        /// <summary>Sizes the plate to the text with a little padding; hides it when there is no text.</summary>
        public static void FitPlate(SpriteRenderer plate, TMP_Text label)
        {
            var empty = string.IsNullOrEmpty(label.text);
            plate.enabled = !empty;
            if (empty) return;
            label.ForceMeshUpdate();
            var bounds = label.textBounds;
            var pad = label.fontSize * 0.05f;
            plate.transform.localPosition = bounds.center;
            plate.transform.localScale = new Vector3(bounds.size.x + pad * 2f, bounds.size.y + pad, 1f);
        }

        static bool IsLight(Color c) => c.r * 0.3f + c.g * 0.59f + c.b * 0.11f > 0.5f;

        /// <summary>A thick outline in the opposite tone: cream round dark text, ink round light text.</summary>
        public static void Outline(TMP_Text label, Color color)
        {
            label.outlineWidth = 0.25f;
            label.outlineColor = IsLight(color) ? (Color32)Palette.Ink : Palette.ByteCream;
        }
    }

    /// <summary>Keeps a label's plate fitted when its text changes later (e.g. the Twins' phone).</summary>
    public class LabelPlate : MonoBehaviour
    {
        SpriteRenderer plate;
        TMP_Text label;
        string last;

        public void Init(SpriteRenderer p, TMP_Text l)
        {
            plate = p;
            label = l;
            last = l.text;
        }

        void LateUpdate()
        {
            if (label == null || label.text == last) return;
            last = label.text;
            JobVisuals.FitPlate(plate, label);
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
