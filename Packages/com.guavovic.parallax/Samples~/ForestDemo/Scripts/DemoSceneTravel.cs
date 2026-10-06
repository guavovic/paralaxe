namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// O que passa de uma cena para a outra numa passagem: em qual ponto o herói aparece.
    /// </summary>
    public static class DemoSceneTravel
    {
        public static string NextSpawn { get; set; }
        public static bool Arriving => !string.IsNullOrEmpty(NextSpawn);

        // Com "Enter Play Mode Options" sem recarregar o domínio, estáticos sobrevivem entre um Play e outro.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            NextSpawn = null;
        }
    }
}
