using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Balão de fala com pontinhos que aparece quando o herói fica parado, como se ele estivesse impaciente.
    /// O primeiro ponto surge em <c>startSeconds</c> e o último exatamente quando ele começa a se mexer sozinho.
    /// </summary>
    public sealed class DemoImpatience : MonoBehaviour
    {
        [SerializeField] private DemoPlayer2D player;
        [SerializeField] private GameObject bubble;
        [SerializeField] private Transform[] dots;
        [SerializeField, Min(0f)] private float startSeconds = 2f;
        [Tooltip("Quanto o balão fica depois do último ponto, para dar tempo de ver.")]
        [SerializeField, Min(0f)] private float holdSeconds = 0.4f;
        [SerializeField, Min(0.01f)] private float popTime = 0.12f;

        private float[] _shownAt;

        private void Awake()
        {
            _shownAt = new float[dots != null ? dots.Length : 0];
            ResetDots();
        }

        private void Update()
        {
            if (player == null || bubble == null || dots == null || dots.Length == 0)
                return;

            float idle = player.IdleSeconds;
            float end = player.AutonomousAfterSeconds;
            bool visible = end > 0f && idle >= startSeconds && idle <= end + holdSeconds;

            if (bubble.activeSelf != visible)
                bubble.SetActive(visible);

            if (!visible)
            {
                ResetDots();
                return;
            }

            for (int i = 0; i < dots.Length; i++)
            {
                float appearsAt = dots.Length == 1
                    ? startSeconds
                    : startSeconds + i * (end - startSeconds) / (dots.Length - 1);

                bool on = idle >= appearsAt;
                if (on && _shownAt[i] < 0f)
                    _shownAt[i] = Time.time;

                dots[i].gameObject.SetActive(on);
                if (on)
                {
                    float t = Mathf.Clamp01((Time.time - _shownAt[i]) / popTime);
                    float scale = Mathf.Lerp(1.6f, 1f, t);
                    dots[i].localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        private void ResetDots()
        {
            for (int i = 0; i < _shownAt.Length; i++)
                _shownAt[i] = -1f;
        }
    }
}
