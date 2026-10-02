using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    public sealed class DemoCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField] private bool followY;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            var goal = new Vector3(target.position.x, followY ? target.position.y : transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
        }
    }
}
