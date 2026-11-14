// Stream B. One slot on the build board: an empty spot for a tray piece, or a router, switch or house.
// Shape and colour tell the pieces apart (no networking words before the reveal cards):
//   router = big blue box, switch = cyan bar, house = cream box with its number and a window
//   that lights up when a message can reach it. Small dots show free (cyan) and used (ink) sockets.
// Only displays and reports taps; BuildManager owns the rules.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ByteWorld
{
    public class BuildSlot : MonoBehaviour, IPointerClickHandler
    {
        const float DotSize = 14f;

        public string Id { get; private set; }

        BuildManager manager;
        RectTransform rect;
        Image body;
        Outline outline;
        Image window;
        TMP_Text addressText;
        RectTransform dotRow;
        readonly List<Image> dots = new();
        Coroutine flash;

        public static BuildSlot Create(BuildManager manager, RectTransform parent, string id, float u, float v)
        {
            var go = new GameObject($"Slot {id}", typeof(RectTransform), typeof(Image), typeof(Outline));
            var slot = go.AddComponent<BuildSlot>();
            slot.manager = manager;
            slot.Id = id;
            slot.rect = (RectTransform)go.transform;
            slot.rect.SetParent(parent, false);
            slot.rect.anchorMin = slot.rect.anchorMax = new Vector2(u, v);
            slot.rect.pivot = new Vector2(0.5f, 0.5f);
            slot.body = go.GetComponent<Image>();
            slot.outline = go.GetComponent<Outline>();

            slot.window = NewImage("Window", slot.rect, new Vector2(24, 24), new Vector2(0, 32));
            slot.dotRow = (RectTransform)new GameObject("Sockets", typeof(RectTransform)).transform;
            slot.dotRow.SetParent(slot.rect, false);
            slot.dotRow.anchorMin = slot.dotRow.anchorMax = new Vector2(0.5f, 0);
            slot.dotRow.pivot = new Vector2(0.5f, 0);
            slot.dotRow.anchoredPosition = new Vector2(0, 10);
            var layout = slot.dotRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            slot.dotRow.sizeDelta = new Vector2(200, DotSize);

            var text = new GameObject("Address", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(slot.rect, false);
            // House number inside the house, so cables never cover it.
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = new Vector2(0, -6);
            text.rectTransform.sizeDelta = new Vector2(100, 44);
            text.fontSize = 38;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Palette.Ink;
            text.raycastTarget = false;
            slot.addressText = text;
            return slot;
        }

        static Image NewImage(string name, RectTransform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.sizeDelta = size;
            r.anchoredPosition = position;
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        /// <summary>An empty spot waiting for a tray piece.</summary>
        public void ShowOpen(bool highlighted)
        {
            rect.sizeDelta = new Vector2(96, 96);
            var fill = (Color)Palette.ByteCream;
            fill.a = highlighted ? 0.45f : 0.15f;
            body.color = fill;
            SetOutline(highlighted ? Palette.PacketYellow : Palette.ByteCream, 4);
            window.gameObject.SetActive(false);
            addressText.text = "";
            SetDots(0, 0, Palette.NeonCyan);
        }

        public void ShowPiece(PieceKind kind, int sockets, int used, bool selected, bool reachesRouter, int address)
        {
            switch (kind)
            {
                case PieceKind.Router:
                    rect.sizeDelta = new Vector2(132, 132);
                    body.color = Palette.SignalBlue;
                    break;
                case PieceKind.Switch:
                    rect.sizeDelta = new Vector2(132, 76);
                    body.color = Palette.NeonCyan;
                    break;
                default:
                    rect.sizeDelta = new Vector2(112, 112);
                    body.color = Palette.ByteCream;
                    break;
            }

            var isHouse = kind == PieceKind.House;
            window.gameObject.SetActive(isHouse);
            window.color = reachesRouter ? (Color)Palette.PacketYellow : Palette.GridDim[2];
            addressText.text = isHouse && address > 0 ? address.ToString() : "";
            SetOutline(selected ? Palette.PacketYellow : Palette.Ink, selected ? 10 : 6);
            // On the cyan switch, free sockets need a darker colour to show up.
            SetDots(sockets, used, kind == PieceKind.Switch ? Palette.SignalBlue : Palette.NeonCyan);
        }

        /// <summary>A quick pink blink when a tap can't do anything here (no free socket, etc.).</summary>
        public void Flash()
        {
            if (flash != null) StopCoroutine(flash);
            flash = StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            var keep = outline.effectColor;
            for (var i = 0; i < 2; i++)
            {
                outline.effectColor = Palette.AlertPink;
                yield return new WaitForSecondsRealtime(0.12f);
                outline.effectColor = keep;
                yield return new WaitForSecondsRealtime(0.12f);
            }
            flash = null;
        }

        void SetOutline(Color color, float size)
        {
            if (flash != null) return;
            outline.effectColor = color;
            outline.effectDistance = new Vector2(size, -size);
        }

        void SetDots(int sockets, int used, Color freeColor)
        {
            while (dots.Count < sockets)
            {
                var dot = NewImage($"Dot{dots.Count}", dotRow, new Vector2(DotSize, DotSize), Vector2.zero);
                dots.Add(dot);
            }
            for (var i = 0; i < dots.Count; i++)
            {
                dots[i].gameObject.SetActive(i < sockets);
                dots[i].color = i < used ? (Color)Palette.Ink : freeColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData) => manager.OnSlotTapped(Id);
    }
}
