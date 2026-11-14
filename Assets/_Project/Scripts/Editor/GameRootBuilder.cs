// Stream A. Builds Resources/Core/GameRoot.prefab through the Editor API (never hand-written YAML).
// After every compile it checks that the prefab exists and has every component in Components, and rebuilds
// it if not. Rebuild by hand from Byte World > Stream A > Build GameRoot Prefab.
// To add a new Stream A system to GameRoot: add its type to Components below.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ByteWorld.EditorTools
{
    [InitializeOnLoad]
    public static class GameRootBuilder
    {
        const string PrefabPath = "Assets/_Project/Resources/Core/GameRoot.prefab";

        // Order matters only for the Inspector; every system registers itself in Awake.
        static readonly Type[] Components =
        {
            typeof(DataLoader),
            typeof(BackupNest),
            typeof(GameManager),
        };

        static GameRootBuilder()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += () =>
            {
                if (IsUpToDate()) return;
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    Debug.LogWarning("[Stream A] GameRoot.prefab is out of date; leave Play mode and run Byte World > Stream A > Build GameRoot Prefab.");
                else
                    Build();
            };
        }

        static bool IsUpToDate()
        {
            if (!File.Exists(PrefabPath)) return false;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return prefab != null && Components.All(type => prefab.GetComponent(type) != null);
        }

        [MenuItem("Byte World/Stream A/Build GameRoot Prefab")]
        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("GameRoot");
            try
            {
                foreach (var type in Components) root.AddComponent(type);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Stream A] Saved {PrefabPath} with {string.Join(", ", Components.Select(t => t.Name))}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
