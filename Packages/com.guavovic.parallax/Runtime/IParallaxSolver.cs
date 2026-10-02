using UnityEngine;

namespace Guavovic.Parallax
{
    public readonly struct ParallaxContext
    {
        public readonly Vector3 CameraPosition;
        public readonly Vector3 CameraOrigin;
        public readonly float SpeedMultiplier;
        public readonly float FocusDistance;

        public ParallaxContext(Vector3 cameraPosition, Vector3 cameraOrigin, float speedMultiplier, float focusDistance)
        {
            CameraPosition = cameraPosition;
            CameraOrigin = cameraOrigin;
            SpeedMultiplier = speedMultiplier;
            FocusDistance = focusDistance;
        }

        public Vector3 CameraOffset => CameraPosition - CameraOrigin;
    }

    public interface IParallaxSolver
    {
        void Solve(ParallaxLayer layer, ParallaxLayerSettings settings, in ParallaxContext context);
    }
}
