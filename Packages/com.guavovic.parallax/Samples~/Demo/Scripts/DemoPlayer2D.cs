using UnityEngine;
using UnityEngine.InputSystem;

namespace Guavovic.Parallax.Samples
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DemoPlayer2D : MonoBehaviour, IParallaxTraveler
    {
        [SerializeField, Min(0f)] private float moveSpeed = 3.2f;
        [SerializeField, Min(1f)] private float runMultiplier = 1.7f;
        [SerializeField, Min(0f)] private float jumpSpeed = 8f;
        [SerializeField, Min(0f)] private float acceleration = 22f;
        [Tooltip("Sem input, a velocidade horizontal cai de forma proporcional, então o vento ainda empurra o jogador no ar.")]
        [SerializeField, Min(0f)] private float idleDrag = 3f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [Tooltip("Duração do golpe, em segundos.")]
        [SerializeField, Min(0.05f)] private float attackDuration = 0.3f;
        [Tooltip("Golpe dado até este tempo depois do anterior vira o segundo do combo (subindo).")]
        [SerializeField, Min(0f)] private float comboWindow = 0.35f;
        [Tooltip("Pulos extras no ar. 1 dá o pulo duplo.")]
        [SerializeField, Min(0)] private int airJumps = 1;
        [SerializeField, Min(0f)] private float airJumpSpeed = 7f;

        [Header("Escalada")]
        [SerializeField, Min(0f)] private float climbSpeed = 2.6f;

        [Header("Modo autônomo")]
        [Tooltip("Segundos sem nenhuma tecla até o herói passar a se mexer sozinho. 0 desliga.")]
        [SerializeField, Min(0f)] private float autonomousAfterSeconds = 5f;
        [SerializeField, Min(0.1f)] private float actionMinSeconds = 1f;
        [SerializeField, Min(0.1f)] private float actionMaxSeconds = 3f;
        [SerializeField, Range(0f, 1f)] private float jumpChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float runChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float restChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float attackChance = 0.25f;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private Rigidbody2D _body;
        private Collider2D _collider;
        private float _input;
        private bool _run;
        private bool _jumpQueued;
        private bool _crouch;
        private float _attackStart = float.NegativeInfinity;
        private DemoAttack _attack;
        private int _airJumpsLeft;
        private float _airJumpTime = float.NegativeInfinity;
        private bool _wasGrounded = true;
        private float _forcedUntil = float.NegativeInfinity;
        private float _forcedInput;
        private float _gravity;
        private DemoClimbable _climbable;
        private bool _climbing;
        private float _climbInput;
        private float _forcedClimbUntil = float.NegativeInfinity;
        private float _forcedClimb;

        /// <summary>Avisos para efeitos (poeira, som): pousou, pulou do chão, pulou no ar, começou um golpe.</summary>
        public event System.Action Landed;
        public event System.Action Jumped;
        public event System.Action AirJumped;
        public event System.Action AttackStarted;
        private float _lastActivityTime;
        private float _nextActionTime;

        public float IdleSeconds => Time.time - _lastActivityTime;
        public float AutonomousAfterSeconds => autonomousAfterSeconds;
        public bool IsAutonomous => autonomousAfterSeconds > 0f && IdleSeconds >= autonomousAfterSeconds;
        public bool IsRunning => _run && Mathf.Abs(_input) > 0.01f;
        public bool IsCrouching => _crouch;
        public bool IsClimbing => _climbing;
        public float ClimbInput => _climbInput;
        public bool IsAirJumping => Time.time - _airJumpTime < 0.25f;
        public DemoAttack CurrentAttack => AttackProgress >= 0f ? _attack : DemoAttack.None;

        /// <summary>
        /// De 0 a 1 durante o golpe; negativo fora dele.
        /// </summary>
        public float AttackProgress
        {
            get
            {
                float t = (Time.time - _attackStart) / attackDuration;
                return t >= 0f && t < 1f ? t : -1f;
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _gravity = _body.gravityScale;
            _lastActivityTime = Time.time;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var climbable = other.GetComponent<DemoClimbable>();
            if (climbable != null)
                _climbable = climbable;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_climbable == null || other.GetComponent<DemoClimbable>() != _climbable)
                return;

            _climbable = null;
            _climbing = false;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.isPressed)
                _lastActivityTime = Time.time;

            if (Time.time < _forcedClimbUntil)
            {
                _climbInput = _forcedClimb;
                if (_climbable != null)
                    _climbing = true;
                _input = 0f;
                return;
            }

            if (Time.time < _forcedUntil)
            {
                _input = _forcedInput;
                _run = false;
                _crouch = false;
                return;
            }

            if (IsAutonomous)
            {
                _climbing = false;
                UpdateAutonomous();
                return;
            }

            _nextActionTime = 0f;
            _input = 0f;
            _run = false;
            _crouch = false;

            if (keyboard == null)
                return;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _input += 1f;
            _run = keyboard.leftShiftKey.isPressed;

            bool up = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            bool down = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
            _climbInput = (up ? 1f : 0f) - (down ? 1f : 0f);
            if (!_climbing && _climbable != null && (up || (down && !IsGrounded())))
                _climbing = true;

            if (_climbing)
            {
                if (keyboard.spaceKey.wasPressedThisFrame)
                    _jumpQueued = true;
                return;
            }

            _crouch = down;
            if (_crouch)
                _input = 0f;

            if (keyboard.spaceKey.wasPressedThisFrame && !_crouch)
                _jumpQueued = true;

            if (keyboard.jKey.wasPressedThisFrame)
                Attack();
        }

        public bool CanTravel => !IsAutonomous;

        public void Arrive(Vector3 position, float direction)
        {
            transform.position = position;
            _body.position = position;
            _body.linearVelocity = Vector2.zero;
            WalkFor(direction, 0.5f);
        }

        /// <summary>
        /// Sobe (1) ou desce (-1) o que estiver escalando por um tempo, ignorando as teclas.
        /// </summary>
        public void ClimbFor(float direction, float seconds)
        {
            _forcedClimb = Mathf.Sign(direction);
            _forcedClimbUntil = Time.time + seconds;
        }

        /// <summary>
        /// Anda sozinho numa direção por um tempo, ignorando as teclas. Serve para entrar na cena por uma passagem.
        /// </summary>
        public void WalkFor(float direction, float seconds)
        {
            _forcedInput = Mathf.Sign(direction);
            _forcedUntil = Time.time + seconds;
            _lastActivityTime = Time.time;
        }

        private void Attack()
        {
            if (AttackProgress >= 0f)
                return;

            bool combo = _attack == DemoAttack.Slash && Time.time - (_attackStart + attackDuration) < comboWindow;
            if (!IsGrounded())
                _attack = DemoAttack.Air;
            else if (_crouch)
                _attack = DemoAttack.Crouch;
            else
                _attack = combo ? DemoAttack.Rising : DemoAttack.Slash;

            _attackStart = Time.time;
            AttackStarted?.Invoke();
        }

        private void UpdateAutonomous()
        {
            if (Time.time < _nextActionTime)
                return;

            _nextActionTime = Time.time + Random.Range(actionMinSeconds, actionMaxSeconds);

            // Passando por um cipó, às vezes sobe um pouco e solta.
            if (_climbable != null && Random.value < 0.5f)
            {
                ClimbFor(1f, Random.Range(0.8f, 2f));
                return;
            }

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
            else if (Random.value < attackChance)
                Attack();
        }

        private void FixedUpdate()
        {
            if (_climbing && Climb())
                return;

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

            bool grounded = IsGrounded();
            if (grounded && !_wasGrounded && velocity.y <= 0.01f)
                Landed?.Invoke();
            _wasGrounded = grounded;

            if (grounded)
                _airJumpsLeft = airJumps;

            if (_jumpQueued && grounded)
            {
                velocity.y = jumpSpeed;
                Jumped?.Invoke();
            }
            else if (_jumpQueued && _airJumpsLeft > 0)
            {
                _airJumpsLeft--;
                _airJumpTime = Time.time;
                velocity.y = airJumpSpeed;
                AirJumped?.Invoke();
            }

            _jumpQueued = false;

            // Parado no chão (de rampa também), sem gravidade: o herói não escorrega sem atrito.
            bool standing = grounded && Mathf.Abs(_input) < 0.01f && velocity.y <= 0.01f;
            _body.gravityScale = standing ? 0f : _gravity;
            if (standing)
                velocity.y = 0f;
            _body.linearVelocity = velocity;
        }

        /// <summary>
        /// Sobe e desce centrado no que escala. Pulo solta; chegar ao chão descendo também. Devolve false ao soltar.
        /// </summary>
        private bool Climb()
        {
            if (_climbable == null)
            {
                _climbing = false;
                return false;
            }

            if (_jumpQueued || (_climbInput < 0f && IsGrounded()))
            {
                _climbing = false;
                _body.gravityScale = _gravity;
                if (_jumpQueued)
                {
                    _body.linearVelocity = new Vector2(_input * moveSpeed, jumpSpeed);
                    _jumpQueued = false;
                    _airJumpsLeft = airJumps;
                    Jumped?.Invoke();
                }
                return false;
            }

            _body.gravityScale = 0f;
            float vertical = _climbInput * climbSpeed;
            float headY = _collider.bounds.center.y;
            if (vertical > 0f && headY >= _climbable.Top)
                vertical = 0f;
            float toCenter = (_climbable.CenterX - _body.position.x) * 10f;
            _body.linearVelocity = new Vector2(toCenter, vertical);
            return true;
        }

        public bool IsGrounded()
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundMask, useTriggers = false };
            return _collider.Cast(Vector2.down, filter, _hits, groundCheckDistance) > 0;
        }
    }
}
