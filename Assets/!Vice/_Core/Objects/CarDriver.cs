using System.Collections.Generic;
using UnityEngine;

namespace Vice
{
    [AddComponentMenu("Vice/Objects/Car Driver")]
    public class CarDriver : MonoBehaviour
    {
        public enum RouteEnd { Loop, Park, Destroy }

        [Tooltip("Travel speed in metres per second. 8 is roughly 30 km/h; the whole screen is only ~18 m wide.")]
        [Min(0f)]
        [SerializeField] float speed = 8f;

        [Tooltip("How far the car drives from its start point, in metres. Put both ends off screen so the loop jump is never seen.")]
        [Min(0.01f)]
        [SerializeField] float routeLength = 40f;

        [Tooltip("What happens at the end of the route. Loop: jump back to the start. Park: stop there. Destroy: remove the car.")]
        [SerializeField] RouteEnd routeEnd = RouteEnd.Loop;

        [Header("Wheels")]
        [Tooltip("Child meshes whose name contains this are spun as wheels. The surrounding dots keep the Caliper.WheelBrake meshes from matching.")]
        [SerializeField] string wheelNameToken = ".Wheel.";

        Vector3 startPosition;
        Vector3 direction;
        float distanceTravelled;

        Transform[] wheels;
        Quaternion[] wheelRestRotations;
        float[] wheelRadii;

        void Awake()
        {
            // Captured once so disabling and re-enabling the car resumes the same route instead of starting a new one mid-street.
            startPosition = transform.position;
            direction = transform.forward;

            var foundWheels = new List<MeshFilter>();
            foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
                if (meshFilter.name.Contains(wheelNameToken)) foundWheels.Add(meshFilter);

            wheels = new Transform[foundWheels.Count];
            wheelRestRotations = new Quaternion[foundWheels.Count];
            wheelRadii = new float[foundWheels.Count];

            for (int i = 0; i < foundWheels.Count; i++)
            {
                wheels[i] = foundWheels[i].transform;
                wheelRestRotations[i] = wheels[i].localRotation;
                // The axle runs along local X, so the mesh's half-height is the distance from hub to tread.
                wheelRadii[i] = foundWheels[i].sharedMesh.bounds.extents.y * wheels[i].lossyScale.y;
            }
        }

        public void Configure(float newSpeed, float newRouteLength, RouteEnd newRouteEnd)
        {
            speed = newSpeed;
            routeLength = newRouteLength;
            routeEnd = newRouteEnd;
        }

        void Update()
        {
            distanceTravelled += speed * Time.deltaTime;

            if (routeEnd == RouteEnd.Destroy && distanceTravelled >= routeLength)
            {
                Destroy(gameObject);
                return;
            }

            // Repeat wraps any overshoot past the end back into the route, so a long frame doesn't lose distance at the loop point.
            distanceTravelled = routeEnd == RouteEnd.Loop
                ? Mathf.Repeat(distanceTravelled, routeLength)
                : Mathf.Min(distanceTravelled, routeLength);

            transform.position = startPosition + direction * distanceTravelled;

            for (int i = 0; i < wheels.Length; i++)
            {
                // Rolling without slipping: the wheel turns one radian per radius-length of road covered.
                float spinDegrees = distanceTravelled / wheelRadii[i] * Mathf.Rad2Deg;
                // Rest first, then spin, so the spin axis is the wheel's own axle rather than the parent's X.
                wheels[i].localRotation = wheelRestRotations[i] * Quaternion.AngleAxis(spinDegrees, Vector3.right);
            }
        }

        void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? startPosition : transform.position;
            Vector3 heading = Application.isPlaying ? direction : transform.forward;

            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            Gizmos.DrawLine(origin, origin + heading * routeLength);
            Gizmos.DrawWireSphere(origin + heading * routeLength, 0.3f);
        }
    }
}
