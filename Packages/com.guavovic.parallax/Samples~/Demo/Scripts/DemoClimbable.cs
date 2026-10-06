using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Algo em que o herói sobe (cipó, corrente, raízes). A área de escalada é o colisor, marcado como gatilho.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DemoClimbable : MonoBehaviour
    {
        private Collider2D _area;

        public float CenterX => Area.bounds.center.x;
        public float Top => Area.bounds.max.y;
        public float Bottom => Area.bounds.min.y;

        private Collider2D Area => _area != null ? _area : _area = GetComponent<Collider2D>();

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
    }
}
