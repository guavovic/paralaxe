using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Passagem para outra cena, no fim do cenário. O herói entra, a tela escurece, o fundo guarda onde estava
    /// e a outra cena carrega; lá ele aparece no ponto <see cref="spawnId"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DemoScenePortal : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [Tooltip("Caminho da cena no projeto, para carregar no editor mesmo fora da Build Settings.")]
        [SerializeField] private string scenePath;
        [SerializeField] private string spawnId;
        [SerializeField] private ParallaxRig rig;
        [SerializeField] private DemoSceneFader fader;

        private bool _leaving;

        // Stay, e não Enter: se o herói estava no modo automático em cima da passagem, ela vale assim que o jogador assume.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (_leaving || DemoSceneTravel.Arriving)
                return;

            // No modo automático o herói fica no cenário; senão ele passaria de uma cena para a outra sem parar.
            var player = other.GetComponent<DemoPlayer2D>();
            if (player == null || player.IsAutonomous)
                return;

            _leaving = true;
            StartCoroutine(Leave());
        }

        private IEnumerator Leave()
        {
            if (fader != null)
            {
                fader.FadeOut();
                while (!fader.IsDark)
                    yield return null;
            }

            if (rig != null)
                rig.SaveForNextScene();
            DemoSceneTravel.NextSpawn = spawnId;

#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(sceneName) && !string.IsNullOrEmpty(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                yield break;
            }
#endif
            SceneManager.LoadScene(sceneName);
        }
    }
}
