using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Leva o fundo de uma cena para a próxima: quanto a câmera já andou e os valores do mundo.
    /// Chame <see cref="ParallaxRig.SaveForNextScene"/> antes de trocar de cena; o rig da cena seguinte,
    /// com "continuar da cena anterior" ligado, começa de onde o outro parou.
    /// </summary>
    public static class ParallaxSceneLink
    {
        private static bool _pending;
        private static Vector3 _cameraOffset;
        private static float _speedMultiplier = 1f;
        private static float _windStrength;
        private static double _scrollTime;

        public static bool HasPending => _pending;

        // Com "Enter Play Mode Options" sem recarregar o domínio, estáticos sobrevivem entre um Play e outro.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _pending = false;
        }

        /// <param name="scrollTime">O tempo de rolagem automática do rig, para as camadas seguirem rolando de onde estavam.</param>
        public static void Save(Vector3 cameraOffset, ParallaxWorld world, double scrollTime = 0d)
        {
            _cameraOffset = cameraOffset;
            _scrollTime = scrollTime;
            _speedMultiplier = world != null ? world.SpeedMultiplier : 1f;
            _windStrength = world != null ? world.BaseWindStrength : 0f;
            _pending = true;
        }

        /// <summary>
        /// Devolve o que foi salvo e limpa, para não valer de novo numa terceira cena por engano.
        /// </summary>
        public static bool TryTake(out Vector3 cameraOffset, out float speedMultiplier, out float windStrength)
        {
            return TryTake(out cameraOffset, out speedMultiplier, out windStrength, out _);
        }

        public static bool TryTake(out Vector3 cameraOffset, out float speedMultiplier, out float windStrength, out double scrollTime)
        {
            cameraOffset = _cameraOffset;
            scrollTime = _scrollTime;
            speedMultiplier = _speedMultiplier;
            windStrength = _windStrength;
            bool had = _pending;
            _pending = false;
            return had;
        }

        public static void Clear() => _pending = false;
    }
}
