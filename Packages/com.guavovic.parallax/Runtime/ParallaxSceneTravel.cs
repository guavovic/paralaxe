using UnityEngine;

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
