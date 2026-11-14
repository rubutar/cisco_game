// FOUNDATION. Creates each stream's persistent root prefab at startup, so neither stream has to
// edit the other's scene. Stream A owns Resources/Core/GameRoot.prefab (GameManager, BackupNest,
// DataLoader, DayManager...). Stream B owns Resources/UI/UIRoot.prefab (canvas, dialogue bubble,
// inspect panel, top bar, screens). Each root registers its services in Awake. If a prefab doesn't
// exist yet, the stubs in Services take over.
using UnityEngine;

namespace ByteWorld
{
    public static class Bootstrap
    {
        public const string GameRootPath = "Core/GameRoot";
        public const string UIRootPath = "UI/UIRoot";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateRoots()
        {
            Spawn(GameRootPath);
            Spawn(UIRootPath);
        }

        static void Spawn(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null) return;
            var instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
            Object.DontDestroyOnLoad(instance);
        }
    }
}
