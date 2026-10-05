using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Liga as partículas só com a câmera num trecho da fase (folhas na floresta, gotas na caverna).
    /// As que já saíram terminam de cair, então a troca não corta nada no meio.
    /// </summary>
    public sealed class DemoZone : MonoBehaviour
    {
        [SerializeField] private Vector2 rangeX = new Vector2(-100f, 100f);
        [SerializeField] private ParticleSystem[] particles = new ParticleSystem[0];

        private Transform _camera;
        private bool _inside = true;

        private void Update()
        {
            if (_camera == null && Camera.main != null)
                _camera = Camera.main.transform;
            if (_camera == null)
                return;

            bool inside = _camera.position.x >= rangeX.x && _camera.position.x <= rangeX.y;
            if (inside == _inside)
                return;

            _inside = inside;
            foreach (var system in particles)
            {
                if (system == null)
                    continue;

                var emission = system.emission;
                emission.enabled = inside;
            }
        }
    }
}
