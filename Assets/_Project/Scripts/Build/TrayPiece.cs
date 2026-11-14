// Stream B. A piece waiting in the build tray (e.g. a switch) with how many are left. Tap to pick it up,
// then tap an empty slot on the board to place it.
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TrayPiece : MonoBehaviour, IPointerClickHandler
    {
        public string Piece { get; private set; }

        BuildManager manager;
        Image body;
        Outline outline;
        TMP_Text countText;

        public static TrayPiece Create(BuildManager manager, RectTransform parent, string piece, Color color)
        {
            var go = new GameObject($"Tray {piece}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
            var tray = go.AddComponent<TrayPiece>();
            tray.manager = manager;
            tray.Piece = piece;
            ((RectTransform)go.transform).SetParent(parent, false);
            var element = go.GetComponent<LayoutElement>();
            element.preferredWidth = 132;
            element.preferredHeight = 76;
            tray.body = go.GetComponent<Image>();
            tray.body.color = color;
            tray.outline = go.GetComponent<Outline>();

            var text = new GameObject("Count", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(go.transform, false);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(1, 1);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.sizeDelta = new Vector2(56, 56);
            text.fontSize = 36;
            UiStyle.Apply(text, UiFont.Title);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Palette.Ink;
            text.raycastTarget = false;
            var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            var badgeRect = (RectTransform)badge.transform;
            badgeRect.SetParent(go.transform, false);
            badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.sizeDelta = new Vector2(52, 52);
            var badgeImage = badge.GetComponent<Image>();
            badgeImage.color = Palette.PacketYellow;
            badgeImage.raycastTarget = false;
            text.transform.SetAsLastSibling();
            tray.countText = text;
            return tray;
        }

        public void Show(int count, bool selected)
        {
            countText.text = count.ToString();
            var c = body.color;
            c.a = count > 0 ? 1f : 0.35f;
            body.color = c;
            outline.effectColor = selected ? Palette.PacketYellow : Palette.Ink;
            var size = selected ? 10f : 6f;
            outline.effectDistance = new Vector2(size, -size);
        }

        public void OnPointerClick(PointerEventData eventData) => manager.OnTrayTapped(Piece);
    }
}
