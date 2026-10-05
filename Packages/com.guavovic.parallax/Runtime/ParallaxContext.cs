using UnityEngine;

namespace Guavovic.Parallax
{
    public readonly struct ParallaxContext
    {
        public readonly Vector3 CameraPosition;
        public readonly Vector3 CameraOrigin;
        public readonly float SpeedMultiplier;
        public readonly float FocusDistance;
        public readonly float Time;

        public ParallaxContext(Vector3 cameraPosition, Vector3 cameraOrigin, float speedMultiplier, float focusDistance, float time = 0f)
        {
            CameraPosition = cameraPosition;
            CameraOrigin = cameraOrigin;
            SpeedMultiplier = speedMultiplier;
            FocusDistance = focusDistance;
            Time = time;
        }

        public Vector3 CameraOffset => CameraPosition - CameraOrigin;
    }
}
