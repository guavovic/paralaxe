using System;
using UnityEngine;

namespace Guavovic.Parallax
{
    [Serializable]
    public sealed class ParallaxLayerSettings
    {
        [SerializeField] private string name = "Layer";
        [Tooltip("Modo 2D. 0 fica parado no mundo, 1 acompanha a câmera (muito longe), negativo corre mais que o mundo (perto).")]
        [SerializeField] private Vector2 factor = new Vector2(0.5f, 0f);
        [Tooltip("Modo perspectiva. Distância da camada até o plano de foco, em unidades. Positivo fica atrás, negativo na frente.")]
        [SerializeField] private float depth;
        [Tooltip("Quanto o vento global balança esta camada.")]
        [SerializeField, Min(0f)] private float windInfluence = 1f;
        [SerializeField] private bool loopHorizontally = true;
        [SerializeField] private Color tint = Color.white;

        public string Name => name;
        public Vector2 Factor => factor;
        public float Depth => depth;
        public float WindInfluence => windInfluence;
        public bool LoopHorizontally => loopHorizontally;
        public Color Tint => tint;

        public ParallaxLayerSettings() { }

        public ParallaxLayerSettings(string name, Vector2 factor, float depth, float windInfluence = 1f)
        {
            this.name = name;
            this.factor = factor;
            this.depth = depth;
            this.windInfluence = windInfluence;
        }

        public void SetFactor(Vector2 value) { factor = value; }
        public void SetDepth(float value) { depth = value; }
        public void SetWindInfluence(float value) { windInfluence = Mathf.Max(0f, value); }
        public void SetLoopHorizontally(bool value) { loopHorizontally = value; }
        public void SetTint(Color value) { tint = value; }
        public void SetName(string value) { name = value; }
    }
}
