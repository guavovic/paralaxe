using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// O vento global empurra este corpo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ParallaxWindReceiver2D : MonoBehaviour
    {
        [Tooltip("Força aplicada por unidade de vento.")]
        [SerializeField, Min(0f)] private float force = 4f;

        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            var world = ParallaxWorld.Current;
            if (world == null)
                return;

            _body.AddForce(world.Wind * force);
        }
    }
}
