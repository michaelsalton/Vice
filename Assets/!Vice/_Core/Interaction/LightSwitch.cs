using UnityEngine;

namespace Vice
{
    [DisallowMultipleComponent]
    public class LightSwitch : MonoBehaviour, IInteractable
    {
        [SerializeField] Light target;

        public string Prompt => target != null && target.enabled ? "Turn off" : "Turn on";

        public void Interact(Interactor interactor)
        {
            if (target != null) target.enabled = !target.enabled;
        }
    }
}
