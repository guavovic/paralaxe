using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Bicho que voa em volta da câmera batendo asas (pássaro, borboleta, morcego, esporo). Quando sai muito longe,
    /// volta pelo outro lado. Com fator de parallax, ele parece estar numa camada de trás (positivo) ou da frente (negativo).
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
        [Tooltip("Como o fator 2D das camadas: 0 fica no plano do herói, 0.5 anda metade da câmera (longe), negativo passa na frente.")]
        [SerializeField] private float parallaxFactor;
        [Tooltip("Só aparece com a câmera entre estes X (some e volta devagar). Mínimo igual ao máximo: sempre.")]
        [SerializeField] private Vector2 activeRangeX;

        private SpriteRenderer _renderer;
        private Transform _camera;
        private float _baseY;
        private float _phase;
        private float _lastCameraX;
        private float _baseAlpha = 1f;
        private float _presence = 1f;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseAlpha = _renderer.color.a;
            _baseY = transform.position.y;
            _phase = Random.value * 10f;
        }

        private void Update()
        {
            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main.transform;
                _lastCameraX = _camera.position.x;
            }

            var position = transform.position;
            position.x += speed * Time.deltaTime;
            position.y = _baseY + Mathf.Sin((Time.time + _phase) * bobSpeed) * bobAmplitude;
            if (_camera != null)
            {
                float cameraX = _camera.position.x;
                position.x += (cameraX - _lastCameraX) * parallaxFactor;
                _lastCameraX = cameraX;
                float distance = position.x - cameraX;
                if (distance > wrapDistance) position.x -= wrapDistance * 2f;
                else if (distance < -wrapDistance) position.x += wrapDistance * 2f;
            }

            transform.position = position;
            _renderer.flipX = speed < 0f;
            UpdatePresence();

            if (frames != null && frames.Length > 0)
                _renderer.sprite = frames[(int)((Time.time + _phase) / frameTime) % frames.Length];
        }

        private void UpdatePresence()
        {
            if (activeRangeX.y <= activeRangeX.x || _camera == null)
                return;

            float x = _camera.position.x;
            bool inside = x >= activeRangeX.x && x <= activeRangeX.y;
            _presence = Mathf.MoveTowards(_presence, inside ? 1f : 0f, Time.deltaTime);
            var color = _renderer.color;
            color.a = _baseAlpha * _presence;
            _renderer.color = color;
        }
    }
}
