using UnityEngine;

namespace Vice
{
    [AddComponentMenu("Vice/Objects/Traffic Spawner")]
    public class TrafficSpawner : MonoBehaviour
    {
        [Tooltip("Cars to pick from at random. Each must face +Z; a CarDriver is added if the prefab has none.")]
        [SerializeField] GameObject[] carPrefabs;

        [Tooltip("Shortest wait between spawns, in seconds. Keep it above car length / speed or a new car spawns inside the last one.")]
        [Min(0.1f)]
        [SerializeField] float minInterval = 1.5f;

        [Tooltip("Longest wait between spawns, in seconds.")]
        [Min(0.1f)]
        [SerializeField] float maxInterval = 4f;

        [Tooltip("Speed every car in this lane drives at, in metres per second. Shared so a fast car never drives through a slow one ahead of it.")]
        [Min(0.01f)]
        [SerializeField] float speed = 8f;

        [Tooltip("Lane length in metres along this object's forward axis. Cars are destroyed at the end; put both ends off screen.")]
        [Min(0.01f)]
        [SerializeField] float laneLength = 40f;

        float timeUntilNextSpawn;

        void Update()
        {
            timeUntilNextSpawn -= Time.deltaTime;
            if (timeUntilNextSpawn > 0f) return;

            Spawn();
            timeUntilNextSpawn = Random.Range(minInterval, Mathf.Max(minInterval, maxInterval));
        }

        void Spawn()
        {
            if (carPrefabs == null || carPrefabs.Length == 0) return;

            GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
            if (prefab == null) return;

            GameObject car = Instantiate(prefab, transform.position, transform.rotation, transform);

            // Instantiate has already run the car's Awake at this pose, so it has captured the spawn point as its route start.
            CarDriver driver = car.GetComponent<CarDriver>();
            if (driver == null) driver = car.AddComponent<CarDriver>();
            driver.Configure(speed, laneLength, CarDriver.RouteEnd.Destroy);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 laneEnd = transform.position + transform.forward * laneLength;

            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, laneEnd);

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
            Gizmos.DrawWireSphere(laneEnd, 0.3f);
        }
    }
}
