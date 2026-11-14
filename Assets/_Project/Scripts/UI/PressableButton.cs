// Stream B. The pressed state of a chunky button (PRD-v2 6.6): while the finger is down the drop shadow
// shrinks and the face moves down-right by the same amount, so the button looks pushed in.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ByteWorld
{
    [RequireComponent(typeof(Button))]
    public class PressableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] Shadow shadow;
        [SerializeField] RectTransform face;

        Button button;
        Vector2 restShadow;
        Vector2 restFace;
        bool pressed;

        void Awake()
        {
            button = GetComponent<Button>();
            if (shadow != null) restShadow = shadow.effectDistance;
            if (face != null) restFace = face.anchoredPosition;
        }

        void OnDisable() => Release();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.IsInteractable() || pressed) return;
            pressed = true;
            var press = restShadow.x - UiStyle.PressedShadow;
            if (shadow != null) shadow.effectDistance = new Vector2(UiStyle.PressedShadow, -UiStyle.PressedShadow);
            if (face != null) face.anchoredPosition = restFace + new Vector2(press, -press) * 0.5f;
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        void Release()
        {
            if (!pressed) return;
            pressed = false;
            if (shadow != null) shadow.effectDistance = restShadow;
            if (face != null) face.anchoredPosition = restFace;
        }
    }
}
