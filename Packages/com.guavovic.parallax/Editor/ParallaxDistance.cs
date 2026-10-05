using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Um valor só de distância no lugar de fator e profundidade. É o fator X do modo 2D:
    /// negativo fica na frente do plano de foco, 0 no plano e perto de 1 bem longe.
    /// Ao mudar, grava o fator e a profundidade equivalentes, então trocar de modo mantém a distância da camada.
    /// </summary>
    internal static class ParallaxDistance
    {
        public const float Near = -1f;
        public const float Far = 0.99f;

        public static float Get(ParallaxProfile profile, SerializedProperty layer)
        {
            return profile.Mode == ParallaxMode.Perspective
                ? profile.DepthToFactor(layer.FindPropertyRelative("depth").floatValue)
                : layer.FindPropertyRelative("factor").vector2Value.x;
        }

        public static void Set(ParallaxProfile profile, SerializedProperty layer, float value)
        {
            value = Mathf.Clamp(value, Near, Far);
            var factor = layer.FindPropertyRelative("factor");
            factor.vector2Value = new Vector2(value, factor.vector2Value.y);
            layer.FindPropertyRelative("depth").floatValue = profile.FactorToDepth(value);
        }

        /// <summary>
        /// De 0 (mais perto) a 1 (mais longe), para desenhar o indicador da lista.
        /// </summary>
        public static float Normalized(float value)
        {
            return Mathf.InverseLerp(Near, Far, value);
        }
    }
}
