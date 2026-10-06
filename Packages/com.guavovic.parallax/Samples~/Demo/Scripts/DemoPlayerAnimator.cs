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
        [SerializeField] private Sprite crouch;
        [Tooltip("Pulo duplo: aparece logo depois do pulo no ar.")]
        [SerializeField] private Sprite airJump;
        [Tooltip("Quadros de cada golpe, em ordem: preparação, golpe, acompanhamento.")]
        [SerializeField] private Sprite[] attack;
        [SerializeField] private Sprite[] attackRising;
        [SerializeField] private Sprite[] attackCrouch;
        [SerializeField] private Sprite[] attackAir;
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

            var frames = FramesFor(_player.CurrentAttack);
            if (frames != null && frames.Length > 0)
            {
                _renderer.sprite = frames[Mathf.Min(frames.Length - 1, (int)(_player.AttackProgress * frames.Length))];
                return;
            }

            // Escalando: os passos da caminhada só enquanto sobe ou desce.
            if (_player.IsClimbing && walk != null && walk.Length > 0)
            {
                _renderer.sprite = Mathf.Abs(_player.ClimbInput) > 0.01f ? walk[(int)(Time.time * 8f) % walk.Length] : walk[0];
                return;
            }

            if (_player.IsAirJumping && airJump != null)
            {
                _renderer.sprite = airJump;
                return;
            }

            if (!_player.IsGrounded())
            {
                _renderer.sprite = velocity.y > 0f ? jump : fall;
                return;
            }

            if (_player.IsCrouching && crouch != null)
            {
                _renderer.sprite = crouch;
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

        private Sprite[] FramesFor(DemoAttack kind)
        {
            switch (kind)
            {
                case DemoAttack.Slash: return attack;
                case DemoAttack.Rising: return attackRising;
                case DemoAttack.Crouch: return attackCrouch;
                case DemoAttack.Air: return attackAir;
                default: return null;
            }
        }
    }
}
