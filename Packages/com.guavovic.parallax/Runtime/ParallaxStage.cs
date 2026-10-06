using System;
using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Um trecho do cenário numa camada: a partir de onde a câmera passa de <see cref="StartX"/>,
    /// os blocos novos da camada usam <see cref="Sprite"/>. A transição, se houver, entra no primeiro bloco do trecho.
    /// </summary>
    [Serializable]
    public sealed class ParallaxStage
    {
        [Tooltip("Posição X da câmera a partir da qual este trecho começa.")]
        [SerializeField] private float startX;
        [SerializeField] private Sprite sprite;
        [Tooltip("Bloco que faz a costura com o trecho anterior. Opcional.")]
        [SerializeField] private Sprite transition;
        [Tooltip("Quanto o vento balança os blocos deste trecho, sobre o vento da camada. 0 deixa parado (pedra, construção).")]
        [SerializeField, Min(0f)] private float windInfluence = 1f;

        public float StartX => startX;
        public Sprite Sprite => sprite;
        public Sprite Transition => transition;
        public float WindInfluence => windInfluence;

        public ParallaxStage() { }

        public ParallaxStage(float startX, Sprite sprite, Sprite transition = null, float windInfluence = 1f)
        {
            this.startX = startX;
            this.sprite = sprite;
            this.transition = transition;
            this.windInfluence = Mathf.Max(0f, windInfluence);
        }
    }
}
