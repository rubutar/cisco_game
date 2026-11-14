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

        public void OnPointerClick(PointerEventData eventData) => manager.OnCableTapped(Cable);
    }
}
