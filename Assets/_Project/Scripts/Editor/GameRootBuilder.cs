// Stream A. Builds Resources/Core/GameRoot.prefab through the Editor API (never hand-written YAML).
// Runs once automatically after compiling if the prefab doesn't exist; rebuild any time from
// Byte World > Stream A > Build GameRoot Prefab. Add more Stream A components (BackupNest, DataLoader,
// DayManager...) here as they are written, then rebuild.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    [InitializeOnLoad]
    public static class GameRootBuilder
    {
        const string PrefabPath = "Assets/_Project/Resources/Core/GameRoot.prefab";

        static GameRootBuilder()
        {
            if (!File.Exists(PrefabPath) && !Application.isBatchMode)
                EditorApplication.delayCall += () =>
                {
                    if (File.Exists(PrefabPath)) return;
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                        Debug.LogWarning("[Stream A] GameRoot.prefab is missing; leave Play mode and run Byte World > Stream A > Build GameRoot Prefab.");
                    else
                        Build();
                };
        }

        [MenuItem("Byte World/Stream A/Build GameRoot Prefab")]
        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("GameRoot");
            try
            {
                root.AddComponent<GameManager>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Stream A] Saved {PrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
