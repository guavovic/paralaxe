using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(DemoPlayer2D))]
    public sealed class DemoPlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] idle;
        [SerializeField] private Sprite[] walk;
        [SerializeField] private Sprite jump;
        [SerializeField] private Sprite fall;
        [SerializeField, Min(0.01f)] private float idleFrameTime = 0.6f;
        [SerializeField, Min(0.01f)] private float walkFrameTime = 0.11f;
        [SerializeField, Min(0f)] private float walkThreshold = 0.3f;
        [Tooltip("Velocidade a partir da qual a caminhada acelera, para a corrida.")]
        [SerializeField, Min(0.1f)] private float walkReferenceSpeed = 3.2f;

        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private DemoPlayer2D _player;
        private float _timer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _player = GetComponent<DemoPlayer2D>();
        }

        private void Update()
        {
            var velocity = _body.linearVelocity;

            if (Mathf.Abs(velocity.x) > 0.1f)
                _renderer.flipX = velocity.x < 0f;

            if (!_player.IsGrounded())
            {
                _renderer.sprite = velocity.y > 0f ? jump : fall;
                return;
            }

            _timer += Time.deltaTime;

            if (Mathf.Abs(velocity.x) > walkThreshold && walk != null && walk.Length > 0)
            {
                float pace = Mathf.Clamp(Mathf.Abs(velocity.x) / walkReferenceSpeed, 1f, 2.2f);
                _renderer.sprite = walk[(int)(_timer * pace / walkFrameTime) % walk.Length];
                return;
            }

            if (idle != null && idle.Length > 0)
                _renderer.sprite = idle[(int)(_timer / idleFrameTime) % idle.Length];
        }
    }
}
