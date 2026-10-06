using System.Collections.Generic;
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
        [Tooltip("Altura da área que corta mato e cipós; mais alta que o herói, para alcançar o que fica pendurado pulando.")]
        [SerializeField, Min(0.1f)] private float cutHeight = 2.6f;
        [Tooltip("Pedaços que voam do que foi cortado.")]
        [SerializeField] private ParticleSystem cutPieces;
        [SerializeField, Min(0)] private int piecesPerCut = 7;

        private readonly Collider2D[] _found = new Collider2D[8];
        private readonly List<Vector3> _cutHits = new List<Vector3>();
        private ParallaxScatter[] _cuttable;
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

        private void Start()
        {
            var all = FindObjectsByType<ParallaxScatter>(FindObjectsSortMode.None);
            var cuttable = new List<ParallaxScatter>();
            foreach (var scatter in all)
            {
                if (scatter.Cuttable)
                    cuttable.Add(scatter);
            }
            _cuttable = cuttable.ToArray();
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
            var center = (Vector2)_body.bounds.center + offset;
            int count = Physics2D.OverlapCircle(center, radius, filter, _found);
            for (int i = 0; i < count; i++)
            {
                if (_found[i].TryGetComponent<DemoBreakable>(out var target))
                    target.Hit();
            }

            Cut(new Bounds(new Vector3(center.x, center.y + cutHeight * 0.3f, 0f), new Vector3(radius * 2.2f, cutHeight, 100f)));
        }

        /// <summary>Corta mato e cipós na frente da lâmina e solta pedaços da cor de cada um.</summary>
        private void Cut(Bounds area)
        {
            if (_cuttable == null)
                return;

            foreach (var scatter in _cuttable)
            {
                if (scatter == null)
                    continue;

                _cutHits.Clear();
                if (scatter.Cut(area, _cutHits) == 0 || cutPieces == null || scatter.CutColor.a < 0.01f)
                    continue;

                foreach (var hit in _cutHits)
                {
                    var parameters = new ParticleSystem.EmitParams { position = hit, startColor = scatter.CutColor, applyShapeToPosition = true };
                    cutPieces.Emit(parameters, piecesPerCut);
                }
            }
        }
    }
}
