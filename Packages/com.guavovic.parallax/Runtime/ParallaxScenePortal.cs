using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Passagem para outra cena. Quem entra (com a tag escolhida, e se ele deixar) escurece a tela, o fundo guarda
    /// onde estava e a outra cena carrega; lá, o <see cref="ParallaxSpawnPoint"/> com o mesmo id recebe o viajante.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ParallaxScenePortal : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [Tooltip("Caminho da cena no projeto, para carregar no editor mesmo fora da Build Settings.")]
        [SerializeField] private string scenePath;
        [Tooltip("Id do ponto de chegada na outra cena.")]
        [SerializeField] private string spawnId;
        [SerializeField] private string travelerTag = "Player";
        [Tooltip("Vazio, usa o rig da cena.")]
        [SerializeField] private ParallaxRig rig;
        [SerializeField, Min(0f)] private float fadeSeconds = 0.45f;

        private bool _leaving;

        public string SceneName { get => sceneName; set => sceneName = value; }
        public string ScenePath { get => scenePath; set => scenePath = value; }
        public string SpawnId { get => spawnId; set => spawnId = value; }

        // Stay, e não Enter: quem estava em cima da passagem sem poder viajar passa assim que puder.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (_leaving || ParallaxSceneTravel.Arriving || !other.CompareTag(travelerTag))
                return;

            var traveler = other.GetComponentInParent<IParallaxTraveler>();
            if (traveler != null && !traveler.CanTravel)
                return;

            _leaving = true;
            StartCoroutine(Leave());
        }

        private IEnumerator Leave()
        {
            if (fadeSeconds > 0f)
            {
                ParallaxScreenFade.FadeOut(fadeSeconds);
                while (!ParallaxScreenFade.IsDark)
                    yield return null;
            }

            var sceneRig = rig != null ? rig : FindFirstObjectByType<ParallaxRig>();
            if (sceneRig != null)
                sceneRig.SaveForNextScene();
            ParallaxSceneTravel.Begin(spawnId);

#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(sceneName) && !string.IsNullOrEmpty(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                yield break;
            }
#endif
            SceneManager.LoadScene(sceneName);
        }

        private void OnDrawGizmos()
        {
            var area = GetComponent<Collider2D>();
            if (area == null)
                return;

            Gizmos.color = new Color(0.62f, 0.83f, 0.85f, 0.8f);
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }
    }
}
