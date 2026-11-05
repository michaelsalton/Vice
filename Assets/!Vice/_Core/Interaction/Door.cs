using UnityEngine;

namespace Vice
{
    [DisallowMultipleComponent]
    public class Door : MonoBehaviour, IInteractable
    {
        // on the floor where the player's feet land; its blue axis is the way they'll face
        [SerializeField] Transform destination;
        [SerializeField] string prompt = "Open";

        public string Prompt => prompt;

        public void Interact(Interactor interactor)
        {
            if (destination == null || !interactor.TryGetComponent(out PlayerTravel travel)) return;
            // yaw only; the body never pitches or rolls, FirstPersonLook keeps the head's pitch
            var facing = Quaternion.Euler(0f, destination.eulerAngles.y, 0f);
            travel.Travel(destination.position, facing);
        }
    }
}
