using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Escurece e clareia a tela inteira nas passagens. Sobrevive à troca de cena, então a cena nova já começa escura
    /// e clareia quando o viajante chega.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class ParallaxScreenFade : MonoBehaviour
    {
        private static ParallaxScreenFade _instance;

        private float _alpha;
        private float _target;
        private float _seconds = 0.45f;
        private int _loadedFrame = -1;

        public static bool IsDark => _instance != null && _instance._alpha >= 0.999f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
        }

        public static void FadeOut(float seconds) => Get().Fade(1f, seconds);

        public static void FadeIn(float seconds)
        {
            if (_instance != null)
                _instance.Fade(0f, seconds);
        }

        private static ParallaxScreenFade Get()
        {
            if (_instance != null)
                return _instance;

            var go = new GameObject("Parallax Screen Fade") { hideFlags = HideFlags.HideInHierarchy };
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ParallaxScreenFade>();
            _instance.useGUILayout = false;
            return _instance;
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _loadedFrame = Time.frameCount;

        private void Fade(float target, float seconds)
        {
            _target = target;
            _seconds = Mathf.Max(0.01f, seconds);
        }

        private void Update()
        {
            // Se a cena nova não tinha o ponto de chegada esperado, ninguém pediu para clarear: clareia mesmo assim,
            // para a tela não ficar preta, e esquece a chegada, para as passagens voltarem a funcionar.
            if (_loadedFrame >= 0 && Time.frameCount > _loadedFrame + 1)
            {
                _loadedFrame = -1;
                ParallaxSceneTravel.Cancel();
                _target = 0f;
            }

            _alpha = Mathf.MoveTowards(_alpha, _target, Time.unscaledDeltaTime / _seconds);
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
