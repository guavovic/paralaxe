using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Pássaro que bate asas e voa em volta da câmera. Quando sai muito longe, volta pelo outro lado.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DemoBird : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float bobAmplitude = 0.25f;
        [SerializeField] private float bobSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float frameTime = 0.14f;
        [SerializeField] private float wrapDistance = 16f;

        private SpriteRenderer _renderer;
        private Transform _camera;
        private float _baseY;
        private float _phase;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseY = transform.position.y;
            _phase = Random.value * 10f;
        }

        private void Update()
        {
            if (_camera == null && Camera.main != null)
                _camera = Camera.main.transform;

            var position = transform.position;
            position.x += speed * Time.deltaTime;
            position.y = _baseY + Mathf.Sin((Time.time + _phase) * bobSpeed) * bobAmplitude;

            if (_camera != null)
            {
                float distance = position.x - _camera.position.x;
                if (distance > wrapDistance) position.x -= wrapDistance * 2f;
                else if (distance < -wrapDistance) position.x += wrapDistance * 2f;
            }

            transform.position = position;
            _renderer.flipX = speed < 0f;

            if (frames != null && frames.Length > 0)
                _renderer.sprite = frames[(int)((Time.time + _phase) / frameTime) % frames.Length];
        }
    }
}
