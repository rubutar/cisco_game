// Stream A. Keeps the 480 x 270 town filling the window with sharp text (world labels on boats, visitors...).
//   An exact multiple of 480 x 270 (1920 x 1080; the WebGL page always sizes the canvas this way): the Pixel
//   Perfect Camera draws at full resolution, so pixels and text are both crisp.
//   Any other window (the Editor's Free Aspect, an odd browser size): the Pixel Perfect Camera is switched off and
//   the camera zooms so the map fills the window, still at full resolution, so text stays sharp. (Stretch Fill
//   drew the world into a 480 x 270 texture and enlarged it, which blurred every world-space label.)
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ByteWorld
{
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        Camera cam;
        PixelPerfectCamera pixelPerfect;
        int width, height;

        void Awake()
        {
            cam = GetComponent<Camera>();
            pixelPerfect = GetComponent<PixelPerfectCamera>();
            if (pixelPerfect != null)
            {
                pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.None;
                pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.None;
            }
        }

        void LateUpdate()
        {
            if (Screen.width == width && Screen.height == height) return;
            width = Screen.width;
            height = Screen.height;

            var exact = width % PixelArt.MapWidth == 0 && height % PixelArt.MapHeight == 0
                        && width / PixelArt.MapWidth == height / PixelArt.MapHeight;
            if (pixelPerfect != null) pixelPerfect.enabled = exact;
            if (exact || height == 0) return;

            // Fill the window: fit the map's height, or its width when the window is narrower than 16:9.
            var mapAspect = (float)PixelArt.MapWidth / PixelArt.MapHeight;
            var halfHeight = PixelArt.MapHeight / 2f / PixelArt.PixelsPerUnit;
            var aspect = (float)width / height;
            cam.orthographicSize = aspect >= mapAspect ? halfHeight : halfHeight * mapAspect / aspect;
        }
    }
}
