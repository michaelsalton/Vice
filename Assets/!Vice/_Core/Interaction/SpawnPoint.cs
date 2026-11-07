using UnityEngine;

namespace Vice
{
    // a floor landing spot, blue axis = facing; cross-scene doors can't hold a Transform, so they find these by id
    [DisallowMultipleComponent]
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] string id;

        public string Id => id;

        // inactive objects are skipped, so a spawn point under a disabled root can't be found
        public static Transform Find(string id)
        {
            foreach (var point in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
            {
                if (point.Id == id) return point.transform;
            }
            return null;
        }
    }
}
