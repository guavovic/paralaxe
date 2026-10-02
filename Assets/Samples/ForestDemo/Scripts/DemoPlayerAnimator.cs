using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(DemoPlayer2D))]
    public sealed class DemoPlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite idle;
        [SerializeField] private Sprite[] walk;
        [SerializeField] private Sprite jump;
        [SerializeField] private Sprite fall;
        [SerializeField, Min(0.01f)] private float walkFrameTime = 0.12f;
        [SerializeField, Min(0f)] private float walkThreshold = 0.3f;

        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private DemoPlayer2D _player;
        private float _walkTimer;

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

            if (Mathf.Abs(velocity.x) > walkThreshold && walk != null && walk.Length > 0)
            {
                _walkTimer += Time.deltaTime;
                int frame = (int)(_walkTimer / walkFrameTime) % walk.Length;
                _renderer.sprite = walk[frame];
                return;
            }

            _walkTimer = 0f;
            _renderer.sprite = idle;
        }
    }
}
