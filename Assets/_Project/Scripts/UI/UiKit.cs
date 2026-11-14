// Stream B. Small runtime helpers for UI made in code (station bubbles, cards, panels), with the same look as
// UIRootBuilder: UiStyle fonts, sizes and colours, Palette only. Also makes the few procedural sprites the UI
// needs (a ring and an iris hole), once, so nothing is drawn by hand.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public static class UiKit
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Image Image(GameObject go, Color color, bool raycast = false)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Outline Border(GameObject go, float size, Color? color = null)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color ?? Palette.Ink;
            outline.effectDistance = new Vector2(size, -size);
            return outline;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, UiFont font = UiFont.Body)
        {
            var rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            UiStyle.Apply(text, font);
            text.fontSize = Mathf.Max(size, UiStyle.MinText);
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.text = "";
            return text;
        }

        public static Button Button(string name, Transform parent, ButtonStyle style, out TMP_Text label, float height = UiStyle.ButtonHeight)
        {
            var rect = Rect(name, parent);
            var image = Image(rect.gameObject, UiStyle.ButtonFill(style), true);
            Border(rect.gameObject, UiStyle.Border);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Palette.Ink;
            shadow.effectDistance = new Vector2(UiStyle.Shadow, -UiStyle.Shadow);
            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = height;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
            button.colors = colors;
            var text = Text("Label", rect, UiStyle.Button, UiStyle.ButtonText(style), TextAlignmentOptions.Center, UiFont.Title);
            label = text;
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(16, 0);
            text.rectTransform.offsetMax = new Vector2(-16, 0);
            rect.gameObject.AddComponent<PressableButton>().Init(shadow, text.rectTransform);
            return button;
        }

        public static VerticalLayoutGroup Column(GameObject go, float spacing, RectOffset padding = null, TextAnchor align = TextAnchor.UpperLeft)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = align;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup Row(GameObject go, float spacing, RectOffset padding = null, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = align;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static void Fit(GameObject go, bool width, bool height)
        {
            var fit = go.AddComponent<ContentSizeFitter>();
            if (width) fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (height) fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public static LayoutElement Size(GameObject go, float width, float height)
        {
            // Not "??": Unity's fake-null for missing components would slip through.
            var e = go.GetComponent<LayoutElement>();
            if (e == null) e = go.AddComponent<LayoutElement>();
            if (width > 0) e.minWidth = e.preferredWidth = width;
            if (height > 0) e.minHeight = e.preferredHeight = height;
            return e;
        }

        /// <summary>A sprite from Resources/Sprites (Person A's art), or null.</summary>
        public static Sprite Sprite(string path) => string.IsNullOrEmpty(path) ? null : Resources.Load<Sprite>("Sprites/" + path);

        // ---------- Procedural sprites ----------

        static Sprite ring, hole;

        /// <summary>A pixel ring (transparent inside), for circling a station.</summary>
        public static Sprite Ring()
        {
            if (ring != null) return ring;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var c = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    tex.SetPixel(x, y, d <= c && d >= c - 5 ? Color.white : Color.clear);
                }
            tex.Apply();
            ring = UnityEngine.Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return ring;
        }

        /// <summary>A white square with a transparent circle in the middle: the iris opening.</summary>
        public static Sprite Hole()
        {
            if (hole != null) return hole;
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var c = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    tex.SetPixel(x, y, d <= c ? Color.clear : Color.white);
                }
            tex.Apply();
            hole = UnityEngine.Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return hole;
        }
    }
}
