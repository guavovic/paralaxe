using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>Material do elemento, para o som da batida e da quebra.</summary>
    public enum DemoBreakableSound
    {
        Clay,
        Wood,
        Crystal
    }

    /// <summary>
    /// Elemento que o golpe acerta: treme e solta faíscas a cada batida e quebra em cacos na última.
    /// Volta sozinho depois de um tempo, para a demonstração seguir viva.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class DemoBreakable : MonoBehaviour
    {
        [SerializeField, Min(1)] private int hits = 2;
        [SerializeField] private Color shardColor = Color.white;
        [SerializeField] private ParticleSystem shards;
        [SerializeField, Min(0)] private int shardCount = 14;
        [SerializeField, Min(0)] private int hitCount = 4;
        [Tooltip("Segundos até voltar depois de quebrar. 0 não volta.")]
        [SerializeField, Min(0f)] private float respawnSeconds = 8f;
        [SerializeField] private DemoBreakableSound sound;

        /// <summary>Aviso de batida para sons e efeitos: quem levou e se quebrou.</summary>
        public static event System.Action<DemoBreakable, bool> Struck;

        public DemoBreakableSound Sound { get => sound; set => sound = value; }

        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private Vector3 _home;
        private int _left;
        private float _shakeUntil;
        private float _respawnAt = float.PositiveInfinity;

        public bool IsBroken => !_collider.enabled;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _home = transform.localPosition;
            _left = hits;
        }

        public void Hit()
        {
            if (IsBroken)
                return;

            _left--;
            if (_left > 0)
            {
                _shakeUntil = Time.time + 0.15f;
                Emit(hitCount);
                Struck?.Invoke(this, false);
                return;
            }

            Emit(shardCount);
            Struck?.Invoke(this, true);
            _renderer.enabled = false;
            _collider.enabled = false;
            _respawnAt = respawnSeconds > 0f ? Time.time + respawnSeconds : float.PositiveInfinity;
        }

        private void Update()
        {
            if (IsBroken && Time.time >= _respawnAt)
            {
                _left = hits;
                _renderer.enabled = true;
                _collider.enabled = true;
                _respawnAt = float.PositiveInfinity;
            }

            transform.localPosition = Time.time < _shakeUntil
                ? _home + (Vector3)(Random.insideUnitCircle * 0.04f)
                : _home;
        }

        private void Emit(int count)
        {
            if (shards == null || count <= 0)
                return;

            var parameters = new ParticleSystem.EmitParams
            {
                position = _renderer.bounds.center,
                startColor = shardColor,
                applyShapeToPosition = true
            };
            shards.Emit(parameters, count);
        }
    }
}
