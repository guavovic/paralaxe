using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Faz a luz de fogo cintilar variando o alpha do sprite.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DemoFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float amount = 0.18f;
        [SerializeField, Min(0f)] private float speed = 7f;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(Time.time * speed, 0.37f);
            var color = _renderer.color;
            color.a = baseAlpha * (1f - amount + amount * noise * 2f);
            _renderer.color = color;
        }
    }
}
