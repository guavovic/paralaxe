using System.Collections;
using UnityEngine;

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

        // Quem já está dentro da passagem logo depois de a cena começar (chegou em cima dela) precisa sair antes de
        // usá-la, senão volta na hora. O tempo cobre a chegada andando do ponto de chegada.
        private const float ArrivalGraceSeconds = 0.5f;

        private bool _leaving;
        private bool _needsExit;

        /// <summary>Alguém entrou na passagem e a troca de cena começou (para som, efeito, salvar o jogo).</summary>
        public static event System.Action<ParallaxScenePortal> Leaving;

        public string SceneName { get => sceneName; set => sceneName = value; }
        public string ScenePath { get => scenePath; set => scenePath = value; }
        public string SpawnId { get => spawnId; set => spawnId = value; }

        // Stay, e não Enter: quem estava em cima da passagem sem poder viajar passa assim que puder.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (_leaving || ParallaxSceneTravel.Arriving || !other.CompareTag(travelerTag))
                return;

            if (_needsExit)
                return;

            var traveler = other.GetComponentInParent<IParallaxTraveler>();
            if (traveler != null && !traveler.CanTravel)
                return;

            _leaving = true;
            Leaving?.Invoke(this);
            StartCoroutine(Leave());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(travelerTag) && Time.timeSinceLevelLoad < ArrivalGraceSeconds)
                _needsExit = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(travelerTag))
                _needsExit = false;
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

            if (!ParallaxSceneTravel.TryLoadScene(sceneName, scenePath))
            {
                // Cena errada ou fora da Build Settings: volta a tela e as passagens, em vez de travar no preto.
                Debug.LogError($"Passagem '{name}': a cena '{sceneName}' não pode ser carregada. Confira o nome e a Build Settings.", this);
                ParallaxSceneTravel.Cancel();
                ParallaxSceneLink.Clear();
                ParallaxScreenFade.FadeIn(fadeSeconds);
                _leaving = false;
            }
        }

        private void OnDrawGizmos()
        {
            var area = GetComponent<Collider2D>();
            if (area == null)
                return;

            Gizmos.color = new Color(ParallaxCameraBounds.GizmoColor.r, ParallaxCameraBounds.GizmoColor.g, ParallaxCameraBounds.GizmoColor.b, 0.8f);
            Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
        }
    }
}
