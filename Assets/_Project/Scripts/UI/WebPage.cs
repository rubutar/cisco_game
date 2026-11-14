// Stream B. Talks to the WebGL page template: hands it the rotate-phone text from dialogue.json once the
// data is loaded. Does nothing in the Editor or on other platforms.
using System.Runtime.InteropServices;
using UnityEngine;

namespace ByteWorld
{
    public static class WebPage
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void ByteWorldSetRotateText(string text);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SendPageText()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ByteWorldSetRotateText(UiStrings.Ui.rotatePhone ?? "");
#endif
        }
    }
}
