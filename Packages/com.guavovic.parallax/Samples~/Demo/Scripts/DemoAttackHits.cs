using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Acerta o que estiver na frente da lâmina quando o herói começa um golpe.
    /// </summary>
    public sealed class DemoAttackHits : MonoBehaviour
    {
        [SerializeField] private DemoPlayer2D player;
        [Tooltip("Centro do golpe em relação ao herói, olhando para a direita.")]
        [SerializeField] private Vector2 reach = new Vector2(0.8f, 0f);
        [SerializeField, Min(0.05f)] private float radius = 0.6f;

        private readonly Collider2D[] _found = new Collider2D[8];
        private SpriteRenderer _renderer;
        private Collider2D _body;

        private void Awake()
        {
            if (player == null)
                return;

            _renderer = player.GetComponent<SpriteRenderer>();
            _body = player.GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            if (player != null)
                player.AttackStarted += OnAttackStarted;
        }

        private void OnDisable()
        {
            if (player != null)
                player.AttackStarted -= OnAttackStarted;
        }

        private void OnAttackStarted()
        {
            var offset = reach;
            if (_renderer.flipX)
                offset.x = -offset.x;

            var filter = new ContactFilter2D { useTriggers = true };
            int count = Physics2D.OverlapCircle((Vector2)_body.bounds.center + offset, radius, filter, _found);
            for (int i = 0; i < count; i++)
            {
                if (_found[i].TryGetComponent<DemoBreakable>(out var target))
                    target.Hit();
            }
        }
    }
}
