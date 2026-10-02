using UnityEngine;
using UnityEngine.InputSystem;

namespace Guavovic.Parallax.Samples
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DemoPlayer2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(1f)] private float runMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float jumpSpeed = 9f;
        [SerializeField, Min(0f)] private float acceleration = 40f;
        [Tooltip("Sem input, a velocidade horizontal cai de forma proporcional, então o vento ainda empurra o jogador no ar.")]
        [SerializeField, Min(0f)] private float idleDrag = 3f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDistance = 0.1f;

        [Header("Modo autônomo")]
        [Tooltip("Segundos sem nenhuma tecla até o herói passar a se mexer sozinho. 0 desliga.")]
        [SerializeField, Min(0f)] private float autonomousAfterSeconds = 5f;
        [SerializeField, Min(0.1f)] private float actionMinSeconds = 1f;
        [SerializeField, Min(0.1f)] private float actionMaxSeconds = 3f;
        [SerializeField, Range(0f, 1f)] private float jumpChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float runChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float restChance = 0.2f;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private Rigidbody2D _body;
        private Collider2D _collider;
        private float _input;
        private bool _run;
        private bool _jumpQueued;
        private float _lastActivityTime;
        private float _nextActionTime;

        public bool IsAutonomous => autonomousAfterSeconds > 0f && Time.time - _lastActivityTime >= autonomousAfterSeconds;
        public bool IsRunning => _run && Mathf.Abs(_input) > 0.01f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _lastActivityTime = Time.time;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.isPressed)
                _lastActivityTime = Time.time;

            if (IsAutonomous)
            {
                UpdateAutonomous();
                return;
            }

            _nextActionTime = 0f;
            _input = 0f;
            _run = false;

            if (keyboard == null)
                return;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _input += 1f;
            _run = keyboard.leftShiftKey.isPressed;

            if (keyboard.spaceKey.wasPressedThisFrame)
                _jumpQueued = true;
        }

        private void UpdateAutonomous()
        {
            if (Time.time < _nextActionTime)
                return;

            _nextActionTime = Time.time + Random.Range(actionMinSeconds, actionMaxSeconds);

            if (Random.value < restChance)
            {
                _input = 0f;
                _run = false;
            }
            else
            {
                _input = Random.value < 0.5f ? -1f : 1f;
                _run = Random.value < runChance;
            }

            if (Random.value < jumpChance)
                _jumpQueued = true;
        }

        private void FixedUpdate()
        {
            var velocity = _body.linearVelocity;
            if (Mathf.Abs(_input) > 0.01f)
            {
                float speed = moveSpeed * (_run ? runMultiplier : 1f);
                velocity.x = Mathf.MoveTowards(velocity.x, _input * speed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                velocity.x -= velocity.x * idleDrag * Time.fixedDeltaTime;
            }

            if (_jumpQueued && IsGrounded())
                velocity.y = jumpSpeed;

            _jumpQueued = false;
            _body.linearVelocity = velocity;
        }

        public bool IsGrounded()
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundMask, useTriggers = false };
            return _collider.Cast(Vector2.down, filter, _hits, groundCheckDistance) > 0;
        }
    }
}
