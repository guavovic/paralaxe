using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Sons do herói e do que ele faz: passos pelo chão da zona, pulo, pouso, golpe, escalada, batidas e quebras,
    /// mato cortado e passagem entre cenas.
    /// </summary>
    public sealed class DemoSfx : MonoBehaviour
    {
        [SerializeField] private DemoPlayer2D player;
        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

        [Header("Passos")]
        [SerializeField] private AudioClip[] stepsGrass = new AudioClip[0];
        [SerializeField] private AudioClip[] stepsStone = new AudioClip[0];
        [SerializeField] private AudioClip[] stepsRock = new AudioClip[0];
        [SerializeField, Min(0.05f)] private float stepInterval = 0.32f;

        [Header("Herói")]
        [SerializeField] private AudioClip jump;
        [SerializeField] private AudioClip airJump;
        [SerializeField] private AudioClip land;
        [SerializeField] private AudioClip swing;
        [SerializeField] private AudioClip[] climbVine = new AudioClip[0];
        [SerializeField] private AudioClip[] climbChain = new AudioClip[0];

        [Header("Mundo")]
        [SerializeField] private AudioClip hitClay;
        [SerializeField] private AudioClip hitWood;
        [SerializeField] private AudioClip hitCrystal;
        [SerializeField] private AudioClip breakClay;
        [SerializeField] private AudioClip breakWood;
        [SerializeField] private AudioClip breakCrystal;
        [SerializeField] private AudioClip cut;
        [SerializeField] private AudioClip portal;

        private AudioSource _source;
        private Rigidbody2D _body;
        private float _stepTimer;
        private float _climbTimer;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            if (player != null)
                _body = player.GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (player != null)
            {
                player.Jumped += OnJumped;
                player.AirJumped += OnAirJumped;
                player.Landed += OnLanded;
                player.AttackStarted += OnAttack;
            }
            DemoBreakable.Struck += OnStruck;
            DemoAttackHits.CutSomething += OnCut;
            ParallaxScenePortal.Leaving += OnPortal;
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.Jumped -= OnJumped;
                player.AirJumped -= OnAirJumped;
                player.Landed -= OnLanded;
                player.AttackStarted -= OnAttack;
            }
            DemoBreakable.Struck -= OnStruck;
            DemoAttackHits.CutSomething -= OnCut;
            ParallaxScenePortal.Leaving -= OnPortal;
        }

        private void Update()
        {
            if (player == null || _body == null)
                return;

            if (player.IsClimbing)
            {
                _stepTimer = 0f;
                if (Mathf.Abs(player.ClimbInput) < 0.01f)
                    return;
                _climbTimer -= Time.deltaTime;
                if (_climbTimer <= 0f)
                {
                    _climbTimer = 0.38f;
                    Play(Surface() == DemoSurface.Stone ? climbChain : climbVine, 0.6f);
                }
                return;
            }

            float speed = Mathf.Abs(_body.linearVelocity.x);
            if (!player.IsGrounded() || speed < 0.6f)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer -= Time.deltaTime;
            if (_stepTimer > 0f)
                return;

            _stepTimer = stepInterval * (player.IsRunning ? 0.65f : 1f);
            Play(StepsFor(Surface()), 0.55f);
        }

        private static DemoSurface Surface()
        {
            var zone = DemoAudio.Current != null ? DemoAudio.Current.CurrentZone : null;
            return zone != null ? zone.surface : DemoSurface.Grass;
        }

        private AudioClip[] StepsFor(DemoSurface surface)
        {
            switch (surface)
            {
                case DemoSurface.Stone: return stepsStone;
                case DemoSurface.Rock: return stepsRock;
                default: return stepsGrass;
            }
        }

        private void OnJumped() => Play(jump, 0.5f);
        private void OnAirJumped() => Play(airJump, 0.5f);
        private void OnAttack() => Play(swing, 0.6f);

        private void OnLanded()
        {
            Play(land, 0.6f);
            Play(StepsFor(Surface()), 0.5f);
        }

        private void OnCut(int count) => Play(cut, Mathf.Min(1f, 0.5f + count * 0.1f));
        private void OnPortal(ParallaxScenePortal _) => Play(portal, 0.8f);

        private void OnStruck(DemoBreakable target, bool broke)
        {
            switch (target.Sound)
            {
                case DemoBreakableSound.Wood: Play(broke ? breakWood : hitWood, 0.8f); break;
                case DemoBreakableSound.Crystal: Play(broke ? breakCrystal : hitCrystal, 0.7f); break;
                default: Play(broke ? breakClay : hitClay, 0.8f); break;
            }
        }

        private void Play(AudioClip[] clips, float gain)
        {
            if (clips != null && clips.Length > 0)
                Play(clips[Random.Range(0, clips.Length)], gain);
        }

        private void Play(AudioClip clip, float gain)
        {
            if (clip == null)
                return;
            _source.pitch = Random.Range(0.94f, 1.06f);
            _source.PlayOneShot(clip, gain * volume);
        }
    }
}
