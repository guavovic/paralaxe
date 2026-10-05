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

        /// <summary>
        /// Deslocamento de uma câmera que não se moveu de verdade, como no preview do editor.
        /// O modo perspectiva depende da câmera real, então simula esse deslocamento camada por camada.
        /// </summary>
        public readonly Vector3 VirtualCameraOffset;

        public ParallaxContext(Vector3 cameraPosition, Vector3 cameraOrigin, float speedMultiplier, float focusDistance, float time = 0f)
            : this(cameraPosition, cameraOrigin, speedMultiplier, focusDistance, time, Vector3.zero)
        {
        }

        public ParallaxContext(Vector3 cameraPosition, Vector3 cameraOrigin, float speedMultiplier, float focusDistance, float time, Vector3 virtualCameraOffset)
        {
            CameraPosition = cameraPosition;
            CameraOrigin = cameraOrigin;
            SpeedMultiplier = speedMultiplier;
            FocusDistance = focusDistance;
            Time = time;
            VirtualCameraOffset = virtualCameraOffset;
        }

        public Vector3 CameraOffset => CameraPosition - CameraOrigin;
    }
}
