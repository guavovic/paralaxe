using System.Collections.Generic;
using UnityEngine;

namespace Guavovic.Parallax
{
    [CreateAssetMenu(menuName = "Parallax/Profile", fileName = "ParallaxProfile")]
    public sealed class ParallaxProfile : ScriptableObject
    {
        [SerializeField] private ParallaxMode mode = ParallaxMode.Simulated2D;
        [SerializeField] private List<ParallaxLayerSettings> layers = new List<ParallaxLayerSettings>();

        [Header("Valores globais")]
        [Tooltip("Multiplica o deslocamento de todas as camadas.")]
        [SerializeField, Min(0f)] private float speedMultiplier = 1f;
        [SerializeField] private Vector2 windDirection = Vector2.right;
        [SerializeField, Min(0f)] private float windStrength = 0.5f;
        [SerializeField, Min(0f)] private float windSpeed = 1f;
        [Tooltip("Quanto uma rajada perde de força por segundo.")]
        [SerializeField, Min(0f)] private float gustDecay = 1.5f;
        [Tooltip("Força máxima que as rajadas somam ao vento.")]
        [SerializeField, Min(0f)] private float maxGust = 2f;

        [Header("Perspectiva")]
        [Tooltip("Distância da câmera até o plano de foco, em unidades.")]
        [SerializeField, Min(0.01f)] private float focusDistance = 10f;

        public ParallaxMode Mode => mode;
        public IReadOnlyList<ParallaxLayerSettings> Layers => layers;
        public float SpeedMultiplier => speedMultiplier;
        public Vector2 WindDirection => windDirection;
        public float WindStrength => windStrength;
        public float WindSpeed => windSpeed;
        public float GustDecay => gustDecay;
        public float MaxGust => maxGust;
        public float FocusDistance => focusDistance;

        public void SetMode(ParallaxMode value) { mode = value; }
        public void SetSpeedMultiplier(float value) { speedMultiplier = Mathf.Max(0f, value); }
        public void SetWind(Vector2 direction, float strength, float speed)
        {
            windDirection = direction;
            windStrength = Mathf.Max(0f, strength);
            windSpeed = Mathf.Max(0f, speed);
        }
        public void SetFocusDistance(float value) { focusDistance = Mathf.Max(0.01f, value); }

        public ParallaxLayerSettings AddLayer(ParallaxLayerSettings settings)
        {
            layers.Add(settings);
            return settings;
        }

        public void RemoveLayerAt(int index)
        {
            if (index >= 0 && index < layers.Count)
                layers.RemoveAt(index);
        }

        public void MoveLayer(int from, int to)
        {
            if (from < 0 || from >= layers.Count || to < 0 || to >= layers.Count || from == to)
                return;

            var item = layers[from];
            layers.RemoveAt(from);
            layers.Insert(to, item);
        }

        /// <summary>
        /// Converte a profundidade de uma camada no fator equivalente do modo 2D.
        /// Uma camada no plano de foco (profundidade 0) vale 0, e uma camada no infinito vale 1.
        /// </summary>
        public float DepthToFactor(float depth)
        {
            float distance = focusDistance + depth;
            if (distance <= 0.001f)
                return 0f;

            return 1f - focusDistance / distance;
        }

        public float FactorToDepth(float factor)
        {
            factor = Mathf.Min(factor, 0.999f);
            return focusDistance / (1f - factor) - focusDistance;
        }
    }
}
