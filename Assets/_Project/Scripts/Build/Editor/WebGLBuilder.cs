// Stream B. One-click WebGL build with the PRD 6.8 settings, so every build is made the same way.
// Menu: Byte World > WebGL > Apply Settings / Build.
// Batch mode: Unity -batchmode -quit -projectPath . -executeMethod ByteWorld.EditorTools.WebGLBuilder.BuildFromCommandLine
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    public static class WebGLBuilder
    {
        public const string OutputPath = "Builds/WebGL";
        const long SizeBudgetBytes = 20L * 1024 * 1024; // PRD 6.8: build under ~20 MB

        /// <summary>PRD 6.8: Gzip + Decompression Fallback, Data Caching, strip engine code, managed stripping Medium.</summary>
        [MenuItem("Byte World/WebGL/Apply Settings")]
        public static void ApplySettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);

            // B-09b: landscape page template (Assets/WebGLTemplates/ByteWorld) at a 16:9 default size.
            PlayerSettings.WebGL.template = "PROJECT:ByteWorld";
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 540;
            PlayerSettings.productName = "Byte World"; // browser tab title
            AssetDatabase.SaveAssets();
            Debug.Log("[WebGLBuilder] Applied PRD 6.8 WebGL settings.");
        }

        [MenuItem("Byte World/WebGL/Build")]
        public static void BuildMenu() => Build();

        public static void BuildFromCommandLine()
        {
            if (!Build()) EditorApplication.Exit(1);
        }

        public static bool Build()
        {
            ApplySettings();

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[WebGLBuilder] No scenes in Build Settings.");
                return false;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[WebGLBuilder] Build {summary.result} with {summary.totalErrors} errors.");
                return false;
            }

            long bytes = new DirectoryInfo(OutputPath).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            float mb = bytes / (1024f * 1024f);
            if (bytes > SizeBudgetBytes)
                Debug.LogWarning($"[WebGLBuilder] Build is {mb:0.0} MB, over the ~20 MB budget.");
            Debug.Log($"[WebGLBuilder] Build succeeded: {Path.GetFullPath(OutputPath)} ({mb:0.0} MB, {summary.totalTime.TotalSeconds:0}s).");
            return true;
        }
    }
}
