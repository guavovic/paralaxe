using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Opcional, no objeto que passa pelas passagens (o herói). Sem ele, a passagem vale para qualquer um com a tag
    /// certa, e a chegada só muda a posição.
    /// </summary>
    public interface IParallaxTraveler
    {
        /// <summary>Se pode atravessar agora (o herói andando sozinho numa demonstração, por exemplo, não pode).</summary>
        bool CanTravel { get; }

        /// <summary>Chegou por uma passagem: vai para <paramref name="position"/> e entra andando para <paramref name="direction"/> (1 ou -1).</summary>
        void Arrive(Vector3 position, float direction);
    }
}
