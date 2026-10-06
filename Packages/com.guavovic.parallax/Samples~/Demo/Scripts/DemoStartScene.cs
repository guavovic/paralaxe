using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// A jornada sempre começa na floresta: dar Play direto numa cena do meio (a caverna) abre a cena do começo.
    /// Quem chega por uma passagem fica onde chegou.
    /// </summary>
    public sealed class DemoStartScene : MonoBehaviour
    {
        [SerializeField] private string sceneName = "ForestDemo";
        [Tooltip("Caminho da cena no projeto, para carregar no editor mesmo fora da Build Settings.")]
        [SerializeField] private string scenePath = "Assets/Samples/Demo/ForestDemo.unity";

        private void Awake()
        {
            if (ParallaxSceneTravel.Arriving)
                return;

#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(sceneName) && !string.IsNullOrEmpty(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(sceneName);
        }
    }
}
