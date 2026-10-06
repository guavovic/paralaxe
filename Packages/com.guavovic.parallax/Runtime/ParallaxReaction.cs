using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Como os elementos espalhados reagem quando o herói passa na frente (ou atrás) deles.
    /// </summary>
    public enum ParallaxReaction
    {
        [InspectorName("Nenhuma")] None,
        /// <summary>Balança e assenta, como planta ou estandarte.</summary>
        [InspectorName("Balança")] Sway,
        /// <summary>Dá um pulinho (estica e achata), como cogumelo ou cristal.</summary>
        [InspectorName("Pula")] Hop
    }
}
