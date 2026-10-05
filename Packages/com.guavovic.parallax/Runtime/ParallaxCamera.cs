using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Cada modo pede um tipo de câmera: 2D usa ortográfica e perspectiva usa câmera em perspectiva.
    /// O rig escolhe o modo pela câmera, então os dois nunca ficam desencontrados.
    /// </summary>
    public static class ParallaxCamera
    {
        /// <summary>
        /// O modo que a câmera pede. Sem câmera, vale <paramref name="fallback"/>.
        /// </summary>
        public static ParallaxMode ModeOf(Camera camera, ParallaxMode fallback)
        {
            if (camera == null)
                return fallback;

            return camera.orthographic ? ParallaxMode.Simulated2D : ParallaxMode.Perspective;
        }

        public static bool Matches(Camera camera, ParallaxMode mode)
        {
            return camera == null || camera.orthographic == (mode == ParallaxMode.Simulated2D);
        }

        /// <summary>
        /// Troca a projeção da câmera para o modo, mantendo a mesma área visível no plano de foco.
        /// </summary>
        public static void Match(Camera camera, ParallaxMode mode, float focusDistance)
        {
            if (Matches(camera, mode))
                return;

            if (mode == ParallaxMode.Simulated2D)
            {
                camera.orthographicSize = focusDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                camera.orthographic = true;
            }
            else
            {
                camera.fieldOfView = 2f * Mathf.Atan(camera.orthographicSize / focusDistance) * Mathf.Rad2Deg;
                camera.orthographic = false;
            }
        }
    }
}
