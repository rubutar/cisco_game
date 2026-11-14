// Stream B. Developer helpers for checking UI work from the Unity MCP (editor only, never in the build).
//   ConsoleWatch: remembers errors and warnings logged since the last domain reload or Clear(), short form.
//   UiCapture:    renders the UI canvas (Screen Space Overlay) to a 1920 × 1080 PNG in Play mode.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    [InitializeOnLoad]
    public static class ConsoleWatch
    {
        static readonly List<string> entries = new();

        static ConsoleWatch()
        {
            Application.logMessageReceived += OnLog;
            CompilationPipeline.assemblyCompilationFinished += (assembly, messages) =>
            {
                foreach (var m in messages)
                    if (m.type == CompilerMessageType.Error) entries.Add($"[compile] {m.file}:{m.line} {m.message}");
            };
        }

        static void OnLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;
            var first = message.Split('\n')[0];
            var where = "";
            foreach (var line in stackTrace.Split('\n'))
                if (line.Contains("Assets/_Project")) { where = " @ " + line.Trim(); break; }
            entries.Add($"[{type}] {first}{where}");
            if (entries.Count > 200) entries.RemoveAt(0);
        }

        public static string Report()
        {
            if (entries.Count == 0) return "Console: no errors or warnings.";
            return $"Console: {entries.Count} entries\n" + string.Join("\n", entries);
        }

        public static void Clear() => entries.Clear();
    }

    public static class UiCapture
    {
        /// <summary>Renders the UIRoot canvas (and the main camera behind it) to a PNG. Play mode only.</summary>
        public static string Capture(string path)
        {
            var canvas = GameObject.Find("UIRoot(Clone)")?.GetComponent<Canvas>() ?? Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return "No canvas";

            var rt = new RenderTexture(1920, 1080, 24);
            var cam = new GameObject("UiCaptureCamera").AddComponent<Camera>();
            var main = Camera.main;
            if (main != null) cam.CopyFrom(main);
            cam.cullingMask |= 1 << 5; // the UI layer, which station cameras may leave out
            cam.targetTexture = rt;

            var mode = canvas.renderMode;
            var order = canvas.sortingOrder;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1;
            Canvas.ForceUpdateCanvases();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());

            canvas.renderMode = mode;
            canvas.sortingOrder = order;
            Object.DestroyImmediate(cam.gameObject);
            Object.DestroyImmediate(tex);
            rt.Release();
            return path;
        }
    }
}
