using Saga.Gameplay;
using System.Collections;
using UnityEngine;

namespace Vice
{
    [DisallowMultipleComponent]
    public class Door : MonoBehaviour, IInteractable
    {
        // on the floor where the player's feet land; its blue axis is the way they'll face
        [SerializeField] Transform destination;
        [SerializeField] string prompt = "Open";

        [Header("Scene travel")]
        [Tooltip("Empty: same-scene door using Destination. Set: loads this interior, or the exterior scene's name to go back outside.")]
        [SerializeField] string targetScene;
        [Tooltip("Id of the SpawnPoint to land on in the target scene.")]
        [SerializeField] string spawnId;

        public string Prompt => prompt;

        public void Interact(Interactor interactor)
        {
            if (!interactor.TryGetComponent(out PlayerTravel travel)) return;

            if (string.IsNullOrEmpty(targetScene))
            {
                if (destination == null) return;
                travel.Travel(null, () => Landing(destination));
                return;
            }

            var loader = InteriorLoader.Instance;
            if (loader == null)
            {
                Debug.LogError("[Door] Scene travel needs an InteriorLoader in the exterior scene.", this);
                return;
            }

            IEnumerator swap = targetScene == loader.ExteriorSceneName ? loader.Exit() : loader.Enter(targetScene);
            travel.Travel(swap, () => Landing(SpawnPoint.Find(spawnId)));
        }

        static Pose? Landing(Transform spot)
        {
            if (spot == null) return null;
            // yaw only; the body never pitches or rolls, FirstPersonLook keeps the head's pitch
            return new Pose(spot.position, Quaternion.Euler(0f, spot.eulerAngles.y, 0f));
        }
    }
}
