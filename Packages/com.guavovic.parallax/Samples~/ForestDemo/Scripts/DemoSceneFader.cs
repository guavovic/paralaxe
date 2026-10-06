using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Escurece e clareia a tela inteira nas passagens entre cenas.
    /// </summary>
    public sealed class DemoSceneFader : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float seconds = 0.45f;

        private float _alpha;
        private float _target;

        public bool IsDark => _alpha >= 0.999f;

        private void Awake()
        {
            useGUILayout = false;
            // Chegando por uma passagem, a cena começa escura e clareia.
            _alpha = DemoSceneTravel.Arriving ? 1f : 0f;
            _target = 0f;
        }

        public void FadeOut() => _target = 1f;

        private void Update()
        {
            _alpha = Mathf.MoveTowards(_alpha, _target, Time.unscaledDeltaTime / seconds);
        }

        private void OnGUI()
        {
            if (_alpha <= 0f)
                return;

            GUI.color = new Color(0f, 0f, 0f, _alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
