// Stream A. Fills Stream A's scenes in code when they load, so the scene files stay empty and nobody has to
// edit scene YAML: World gets the map, job Bytes and AlertController; each Job_* scene gets its job.
// Objects are moved into the loaded scene, so unloading the scene removes them.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ByteWorld
{
    public static class SceneSpawner
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Subscribe()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case SceneNames.World:
                    Spawn<WorldMap>(scene, "World Map");
                    Spawn<AlertController>(scene, "Alert Controller");
                    break;
                case "Job_Scout":
                    Spawn<ScoutJob>(scene, "Scout Job");
                    break;
                case "Job_Gigi":
                    Spawn<GigiJob>(scene, "Gigi Job");
                    break;
                case "Job_Twins":
                    Spawn<TwinsJob>(scene, "Twins Job");
                    break;
                case "Job_Patch":
                    Spawn<PatchJob>(scene, "Patch Job");
                    break;
            }
        }

        static T Spawn<T>(Scene scene, string name) where T : Component
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go.AddComponent<T>();
        }
    }
}
