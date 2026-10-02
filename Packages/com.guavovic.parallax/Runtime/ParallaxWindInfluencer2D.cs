using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Este corpo levanta rajadas no vento global quando se move depressa.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ParallaxWindInfluencer2D : MonoBehaviour
    {
        [Tooltip("Abaixo desta velocidade o corpo não levanta vento.")]
        [SerializeField, Min(0f)] private float minSpeed = 3f;
        [Tooltip("Rajada por unidade de velocidade acima do mínimo, por segundo.")]
        [SerializeField, Min(0f)] private float gustPerSpeed = 0.6f;

        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            var world = ParallaxWorld.Current;
            if (world == null)
                return;

            float excess = _body.linearVelocity.magnitude - minSpeed;
            if (excess > 0f)
                world.AddGust(excess * gustPerSpeed * Time.deltaTime);
        }
    }
}
