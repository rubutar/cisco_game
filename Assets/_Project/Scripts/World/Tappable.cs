// Stream A. Makes a world sprite tappable through the uGUI EventSystem (PRD 6.1: tap input only).
// Needs a Physics2DRaycaster on the camera; WorldSetup.EnsureRaycaster adds one.
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ByteWorld
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Tappable : MonoBehaviour, IPointerClickHandler
    {
        public event Action Tapped;

        /// <summary>Adds a tap area sized to the sprite (plus a little slack for fingers).</summary>
        public static Tappable Add(GameObject target, SpriteRenderer sizeFrom, float slack = 0.25f)
        {
            var tappable = target.AddComponent<Tappable>();
            var box = target.GetComponent<BoxCollider2D>();
            if (sizeFrom != null && sizeFrom.sprite != null)
            {
                var bounds = sizeFrom.sprite.bounds;
                box.size = (Vector2)bounds.size + Vector2.one * slack;
                // The collider sits on the target itself, so only the sprite's own centre matters.
                box.offset = sizeFrom.transform == target.transform
                    ? (Vector2)bounds.center
                    : (Vector2)sizeFrom.transform.localPosition + (Vector2)bounds.center;
            }
            return tappable;
        }

        public void OnPointerClick(PointerEventData eventData) => Tapped?.Invoke();
    }

    public static class WorldSetup
    {
        public static void EnsureRaycaster()
        {
            var camera = Camera.main;
            if (camera != null && camera.GetComponent<Physics2DRaycaster>() == null)
                camera.gameObject.AddComponent<Physics2DRaycaster>();
        }

        /// <summary>
        /// The 480 x 270 town fills the whole window, like the hi-fi frame, so the HUD sits on the map. Without
        /// this the Pixel Perfect Camera only zooms in whole steps and leaves a border of empty world around the
        /// map in any window that isn't a multiple of 480 x 270 (the HUD then floats outside the town).
        /// </summary>
        public static void FitCameraToWindow()
        {
            var camera = Camera.main;
            var pixelPerfect = camera != null ? camera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>() : null;
            if (pixelPerfect == null) return;
            pixelPerfect.gridSnapping = UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            pixelPerfect.cropFrame = UnityEngine.Rendering.Universal.PixelPerfectCamera.CropFrame.StretchFill;
        }
    }
}
