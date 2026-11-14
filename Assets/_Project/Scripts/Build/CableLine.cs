// Stream B. One cable drawn between two pieces on the build board. The visible line is thin, but the
// tap area is wide so a cable is easy to hit with a finger. Tapping it asks BuildManager to remove it.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ByteWorld
{
    public class CableLine : MonoBehaviour, IPointerClickHandler
    {
        const float TapThickness = 56f;
        const float LineThickness = 12f;

        public Cable Cable { get; private set; }

        BuildManager manager;
        RectTransform rect;
        Image line;

        public static CableLine Create(BuildManager manager, RectTransform parent)
        {
            var go = new GameObject("Cable", typeof(RectTransform), typeof(Image));
            var cable = go.AddComponent<CableLine>();
            cable.manager = manager;
            cable.rect = (RectTransform)go.transform;
            cable.rect.SetParent(parent, false);
            cable.rect.pivot = new Vector2(0, 0.5f);
            var hit = go.GetComponent<Image>();
            hit.color = new Color(0, 0, 0, 0); // invisible, but catches taps
            hit.raycastTarget = true;

            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(Outline));
            var lineRect = (RectTransform)lineGo.transform;
            lineRect.SetParent(cable.rect, false);
            lineRect.anchorMin = new Vector2(0, 0.5f);
            lineRect.anchorMax = new Vector2(1, 0.5f);
            lineRect.sizeDelta = new Vector2(0, LineThickness);
            cable.line = lineGo.GetComponent<Image>();
            cable.line.color = Palette.NeonCyan;
            cable.line.raycastTarget = false;
            var outline = lineGo.GetComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(3, -3);
            return cable;
        }

        /// <summary>Lays the cable from <paramref name="from"/> to <paramref name="to"/> (anchor space 0..1 on a board of <paramref name="boardSize"/>).</summary>
        public void Show(Cable cable, Vector2 from, Vector2 to, Vector2 boardSize)
        {
            Cable = cable;
            gameObject.SetActive(true);
            var delta = Vector2.Scale(to - from, boardSize);
            rect.anchorMin = rect.anchorMax = from;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(delta.magnitude, TapThickness);
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        TMPro.TextMeshProUGUI healthText;
        Image healthTag;

        /// <summary>ThousandEyes view (PRD-v2 4.8): the cable's colour and a "loss · latency" tag; null text clears it.</summary>
        public void ShowHealth(Color color, string text)
        {
            line.color = color;
            if (healthTag == null)
            {
                var tagGo = new GameObject("Health", typeof(RectTransform), typeof(Image), typeof(Outline));
                var tagRect = (RectTransform)tagGo.transform;
                tagRect.SetParent(transform.parent, false);
                healthTag = tagGo.GetComponent<Image>();
                healthTag.raycastTarget = false;
                var o = tagGo.GetComponent<Outline>();
                o.effectColor = Palette.Ink;
                o.effectDistance = new Vector2(3, -3);
                healthText = new GameObject("Text", typeof(RectTransform)).AddComponent<TMPro.TextMeshProUGUI>();
                healthText.rectTransform.SetParent(tagRect, false);
                healthText.rectTransform.anchorMin = Vector2.zero;
                healthText.rectTransform.anchorMax = Vector2.one;
                healthText.rectTransform.offsetMin = healthText.rectTransform.offsetMax = Vector2.zero;
                UiStyle.Apply(healthText, UiFont.Mono);
                healthText.fontSize = UiStyle.Small;
                healthText.alignment = TMPro.TextAlignmentOptions.Center;
                healthText.color = Palette.Ink;
                healthText.raycastTarget = false;
            }
            var show = !string.IsNullOrEmpty(text);
            healthTag.gameObject.SetActive(show);
            if (!show) return;
            healthTag.color = color;
            healthText.text = text;
            var tagRt = healthTag.rectTransform;
            tagRt.sizeDelta = new Vector2(Mathf.Max(220, healthText.preferredWidth + 28), 52);
            // Sit on the middle of the cable.
            tagRt.anchorMin = tagRt.anchorMax = rect.anchorMin;
            var mid = rect.localRotation * new Vector3(rect.sizeDelta.x * 0.5f, 0, 0);
            tagRt.anchoredPosition = new Vector2(mid.x, mid.y);
        }

        public void ClearHealth()
        {
            line.color = Palette.NeonCyan;
            if (healthTag != null) healthTag.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            if (healthTag != null) healthTag.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (healthTag != null) Destroy(healthTag.gameObject);
        }

        public void OnPointerClick(PointerEventData eventData) => manager.OnCableTapped(Cable);
    }
}
