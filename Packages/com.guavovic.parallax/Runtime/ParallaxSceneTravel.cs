using UnityEngine;
using UnityEngine.SceneManagement;

namespace Guavovic.Parallax
{
    /// <summary>
    /// O que passa de uma cena para a outra numa passagem: em qual ponto de chegada o viajante aparece.
    /// </summary>
    public static class ParallaxSceneTravel
    {
        public static string NextSpawn { get; private set; }
        public static bool Arriving => !string.IsNullOrEmpty(NextSpawn);

        // Com "Enter Play Mode Options" sem recarregar o domínio, estáticos sobrevivem entre um Play e outro.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            NextSpawn = null;
        }

        public static void Begin(string spawnId) => NextSpawn = spawnId;

        /// <summary>
        /// Carrega a cena pelo nome; no editor, pelo caminho do asset quando ela não está na Build Settings.
        /// Devolve false se nenhum dos dois serve (nome errado, cena fora da Build Settings num build).
        /// </summary>
        public static bool TryLoadScene(string sceneName, string scenePath)
        {
#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(sceneName) && !string.IsNullOrEmpty(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return true;
            }
#endif
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
                return false;

            SceneManager.LoadScene(sceneName);
            return true;
        }
        public static void Cancel() => NextSpawn = null;

        /// <summary>Se a chegada esperada é <paramref name="spawnId"/>, consome e devolve true.</summary>
        public static bool TryArrive(string spawnId)
        {
            if (!Arriving || NextSpawn != spawnId)
                return false;

            NextSpawn = null;
            return true;
        }
    }
}
