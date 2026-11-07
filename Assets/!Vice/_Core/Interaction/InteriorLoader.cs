using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vice
{
    // lives in the exterior scene; swaps interiors in additively so the player, cameras and UI never reload
    [DisallowMultipleComponent]
    public class InteriorLoader : MonoBehaviour
    {
        [Tooltip("Everything that belongs to the outdoors: city, sun, traffic, water. Off while inside.")]
        [SerializeField] GameObject exteriorRoot;

        public static InteriorLoader Instance { get; private set; }

        public string ExteriorSceneName => gameObject.scene.name;

        Scene loadedInterior;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public IEnumerator Enter(string sceneName)
        {
            // interior to interior: drop the old one first so two rooms never share the active-scene lighting
            if (loadedInterior.IsValid()) yield return SceneManager.UnloadSceneAsync(loadedInterior);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadedInterior = SceneManager.GetSceneByName(sceneName);
            // ambient, skybox and fog come from the active scene, so this is what relights the player
            SceneManager.SetActiveScene(loadedInterior);
            // takes the sun with it, which would otherwise still light the room
            if (exteriorRoot != null) exteriorRoot.SetActive(false);
        }

        public IEnumerator Exit()
        {
            // re-enabled before the spawn lookup, so exterior spawn points are findable even under the root
            if (exteriorRoot != null) exteriorRoot.SetActive(true);
            SceneManager.SetActiveScene(gameObject.scene);
            if (loadedInterior.IsValid()) yield return SceneManager.UnloadSceneAsync(loadedInterior);
            loadedInterior = default;
        }
    }
}
