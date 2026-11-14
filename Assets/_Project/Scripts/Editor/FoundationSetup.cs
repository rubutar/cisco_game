// FOUNDATION. Creates the empty scenes and Build Settings through the Editor API, so no scene
// YAML is ever written by hand. Safe to re-run: existing scenes are left untouched.
// Batch mode: Unity -batchmode -quit -projectPath . -executeMethod ByteWorld.EditorTools.FoundationSetup.Run
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ByteWorld.EditorTools
{
    public static class FoundationSetup
    {
        const string SceneFolder = "Assets/_Project/Scenes";

        // Boot first: it is the scene the game starts in.
        static readonly string[] Scenes = { "Boot", "World", "Job_Scout", "Job_Gigi", "Job_Twins", "Job_Patch", "Build" };

        // Job scenes are loaded additively on top of World, so they have no camera of their own.
        static readonly string[] ScenesWithCamera = { "Boot", "World", "Build" };

        [MenuItem("Byte World/Foundation/Create Scenes and Build Settings")]
        public static void Run()
        {
            Directory.CreateDirectory(SceneFolder);
            foreach (var name in Scenes)
            {
                var path = $"{SceneFolder}/{name}.unity";
                if (File.Exists(path)) continue;

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (ScenesWithCamera.Contains(name)) AddPixelPerfectCamera();
                EditorSceneManager.SaveScene(scene, path);
                Debug.Log($"[Foundation] Created {path}");
            }

            EditorBuildSettings.scenes = Scenes
                .Select(name => new EditorBuildSettingsScene($"{SceneFolder}/{name}.unity", true))
                .ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[Foundation] Build Settings: " + string.Join(", ", Scenes));
        }

        static void AddPixelPerfectCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Ink;
            go.transform.position = new Vector3(0, 0, -10);

            var pixelPerfect = go.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = 16;
            pixelPerfect.refResolutionX = 480;
            pixelPerfect.refResolutionY = 270;
        }
    }
}
