using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Valores globais do parallax. O fundo lê daqui, e o jogador pode ler e escrever.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class ParallaxWorld : MonoBehaviour
    {
        private static ParallaxWorld _current;

        [SerializeField, Min(0f)] private float speedMultiplier = 1f;
        [SerializeField] private Vector2 windDirection = Vector2.right;
        [SerializeField, Min(0f)] private float windStrength = 0.5f;
        [SerializeField, Min(0f)] private float windSpeed = 1f;
        [SerializeField, Min(0f)] private float gustDecay = 1.5f;
        [SerializeField, Min(0f)] private float maxGust = 2f;

        private float _gust;

        public static ParallaxWorld Current
        {
            get
            {
                if (_current == null)
                    _current = FindAnyObjectByType<ParallaxWorld>();

                return _current;
            }
        }

        public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = Mathf.Max(0f, value); }
        public Vector2 WindDirection { get => windDirection; set => windDirection = value; }
        public float BaseWindStrength { get => windStrength; set => windStrength = Mathf.Max(0f, value); }
        public float WindSpeed { get => windSpeed; set => windSpeed = Mathf.Max(0f, value); }
        public float Gust => _gust;

        public float WindStrength => windStrength + _gust;
        public Vector2 Wind => windDirection.sqrMagnitude > 0f ? windDirection.normalized * WindStrength : Vector2.zero;

        private void OnEnable()
        {
            _current = this;
        }

        private void OnDisable()
        {
            if (_current == this)
                _current = null;
        }

        private void Update()
        {
            if (_gust > 0f)
                _gust = Mathf.MoveTowards(_gust, 0f, gustDecay * Time.deltaTime);
        }

        public void ApplyProfile(ParallaxProfile profile)
        {
            if (profile == null)
                return;

            speedMultiplier = profile.SpeedMultiplier;
            windDirection = profile.WindDirection;
            windStrength = profile.WindStrength;
            windSpeed = profile.WindSpeed;
            gustDecay = profile.GustDecay;
            maxGust = profile.MaxGust;
        }

        /// <summary>
        /// Soma uma rajada ao vento. Ela perde força sozinha com o tempo.
        /// </summary>
        public void AddGust(float amount)
        {
            _gust = Mathf.Clamp(_gust + amount, 0f, maxGust);
        }
    }
}
