using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Poeira nos pés do herói (pousar, andar, pular, pulo duplo) e um brilho na ponta da lâmina no golpe.
    /// Só escuta os avisos do DemoPlayer2D; para trocar o efeito, troque os sistemas de partícula.
    /// </summary>
    public sealed class DemoDust : MonoBehaviour
    {
        [SerializeField] private DemoPlayer2D player;
        [SerializeField] private ParticleSystem dust;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField, Min(0)] private int landCount = 8;
        [SerializeField, Min(0)] private int jumpCount = 5;
        [SerializeField, Min(0)] private int airJumpCount = 8;
        [SerializeField, Min(0)] private int attackCount = 6;
        [Tooltip("Segundos entre um sopro de poeira e outro, andando. Correndo, a metade.")]
        [SerializeField, Min(0.02f)] private float stepInterval = 0.16f;
        [SerializeField, Min(0f)] private float stepMinSpeed = 1f;
        [Tooltip("Distância da ponta da lâmina até o centro do herói, em unidades.")]
        [SerializeField] private Vector2 bladeOffset = new Vector2(0.8f, 0f);

        private Rigidbody2D _body;
        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private float _nextStep;

        private void Awake()
        {
            if (player == null)
                return;

            _body = player.GetComponent<Rigidbody2D>();
            _collider = player.GetComponent<Collider2D>();
            _renderer = player.GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (player == null)
                return;

            player.Landed += OnLanded;
            player.Jumped += OnJumped;
            player.AirJumped += OnAirJumped;
            player.AttackStarted += OnAttackStarted;
        }

        private void OnDisable()
        {
            if (player == null)
                return;

            player.Landed -= OnLanded;
            player.Jumped -= OnJumped;
            player.AirJumped -= OnAirJumped;
            player.AttackStarted -= OnAttackStarted;
        }

        private void Update()
        {
            if (player == null || Time.time < _nextStep || Mathf.Abs(_body.linearVelocity.x) < stepMinSpeed || !player.IsGrounded())
                return;

            _nextStep = Time.time + (player.IsRunning ? stepInterval * 0.5f : stepInterval);
            Emit(dust, Feet, 1);
        }

        private Vector3 Feet => new Vector3(_collider.bounds.center.x, _collider.bounds.min.y, 0f);

        private void OnLanded() => Emit(dust, Feet, landCount);
        private void OnJumped() => Emit(dust, Feet, jumpCount);
        private void OnAirJumped() => Emit(dust, Feet, airJumpCount);

        private void OnAttackStarted()
        {
            var offset = bladeOffset;
            if (_renderer.flipX)
                offset.x = -offset.x;
            Emit(sparks, _collider.bounds.center + (Vector3)offset, attackCount);
        }

        private static void Emit(ParticleSystem system, Vector3 position, int count)
        {
            if (system == null || count <= 0)
                return;

            var parameters = new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true };
            system.Emit(parameters, count);
        }
    }
}
