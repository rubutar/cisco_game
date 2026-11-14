// Stream B. Five quick taps on the title logo open the debug menu (PRD: hidden demo shortcut).
// Taps must come within GapSeconds of each other or the count starts over.
using UnityEngine;
using UnityEngine.EventSystems;

namespace ByteWorld
{
    public class SecretTaps : MonoBehaviour, IPointerClickHandler
    {
        const int TapsNeeded = 5;
        const float GapSeconds = 1f;

        int taps;
        float lastTap = -999f;

        public void OnPointerClick(PointerEventData eventData)
        {
            var now = Time.unscaledTime;
            if (now - lastTap > GapSeconds) taps = 0;
            lastTap = now;
            taps++;
            if (taps < TapsNeeded) return;

            taps = 0;
            GameEvents.RaiseDebugMenu();
        }
    }
}
