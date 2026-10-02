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
        [Tooltip("Se ligado, o vento só empurra o corpo quando ele não está tocando em nada, como durante um pulo.")]
        [SerializeField] private bool onlyInAir = true;

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

            if (onlyInAir && _body.IsTouchingLayers(Physics2D.AllLayers))
                return;

            _body.AddForce(world.Wind * force);
        }
    }
}
