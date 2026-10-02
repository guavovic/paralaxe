using UnityEngine;
using UnityEngine.InputSystem;

namespace Guavovic.Parallax.Samples
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DemoPlayer2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(0f)] private float jumpSpeed = 9f;
        [SerializeField, Min(0f)] private float acceleration = 40f;
        [Tooltip("Sem input, a velocidade horizontal cai de forma proporcional, então o vento ainda empurra o jogador.")]
        [SerializeField, Min(0f)] private float idleDrag = 3f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDistance = 0.1f;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private float _input;
        private bool _jumpQueued;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            _input = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _input += 1f;

            if (keyboard.spaceKey.wasPressedThisFrame)
                _jumpQueued = true;
        }

        private void FixedUpdate()
        {
            var velocity = _body.linearVelocity;
            if (Mathf.Abs(_input) > 0.01f)
                velocity.x = Mathf.MoveTowards(velocity.x, _input * moveSpeed, acceleration * Time.fixedDeltaTime);
            else
                velocity.x -= velocity.x * idleDrag * Time.fixedDeltaTime;

            if (_jumpQueued && IsGrounded())
                velocity.y = jumpSpeed;

            _jumpQueued = false;
            _body.linearVelocity = velocity;
        }

        private bool IsGrounded()
        {
            var bounds = _collider.bounds;
            var hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, groundCheckDistance, groundMask);
            return hit.collider != null && hit.collider != _collider;
        }
    }
}
