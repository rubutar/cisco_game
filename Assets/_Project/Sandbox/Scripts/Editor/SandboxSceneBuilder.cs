// SANDBOX. Builds Sandbox_Roaming.unity through the Editor API (never hand-written YAML).
// Runs once automatically after this script compiles if the scene doesn't exist yet;
// rebuild any time from Byte World > Sandbox > Create Roaming Scene.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ByteWorld.Sandbox.EditorTools
{
    [InitializeOnLoad]
    public static class SandboxSceneBuilder
    {
        const string ScenePath = "Assets/_Project/Sandbox/Scenes/Sandbox_Roaming.unity";

        static SandboxSceneBuilder()
        {
            if (!File.Exists(ScenePath) && !Application.isBatchMode)
                EditorApplication.delayCall += () =>
                {
                    if (!File.Exists(ScenePath) && !EditorApplication.isPlayingOrWillChangePlaymode) Create();
                };
        }

        [MenuItem("Byte World/Sandbox/Create Roaming Scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Ink;
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var pixelPerfect = cameraObject.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = SpriteGrids.PixelsPerUnit;
            pixelPerfect.refResolutionX = SandboxField.Width;
            pixelPerfect.refResolutionY = SandboxField.Height;

            new GameObject("Field").AddComponent<SandboxField>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Sandbox] Created {ScenePath}. Press Play to watch the Byte roam.");
        }
    }
}
