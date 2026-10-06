namespace Guavovic.Parallax
{
    /// <summary>
    /// Como os elementos espalhados reagem quando o herói passa na frente (ou atrás) deles.
    /// </summary>
    public enum ParallaxReaction
    {
        None,
        /// <summary>Balança e assenta, como planta ou estandarte.</summary>
        Sway,
        /// <summary>Dá um pulinho (estica e achata), como cogumelo ou cristal.</summary>
        Hop
    }
}
