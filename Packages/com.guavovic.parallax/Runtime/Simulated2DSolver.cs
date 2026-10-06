using UnityEngine;

namespace Guavovic.Parallax
{
    public sealed class Simulated2DSolver : IParallaxSolver
    {
        public void Solve(ParallaxLayer layer, ParallaxLayerSettings settings, in ParallaxContext context)
        {
            Vector2 factor = settings.Factor * context.SpeedMultiplier;
            Vector3 origin = layer.Origin;
            Vector3 offset = context.CameraOffset + context.VirtualCameraOffset;

            Vector2 scroll = settings.AutoScroll * context.Time;
            float shiftX = offset.x * factor.x + scroll.x;

            // O loop acompanha a câmera real. No preview ela fica parada e só o deslocamento virtual anda.
            // Conta a partir da posição da câmera em relação à camada, e não do deslocamento desde o início:
            // a câmera pode começar longe da camada (ao continuar o fundo da cena anterior, por exemplo).
            float view = context.CameraPosition.x - (origin.x + shiftX);
            float wrap = 0f;
            if (settings.LoopHorizontally && layer.TileWidth > 0f)
                wrap = layer.TileWidth * Mathf.Round(view / layer.TileWidth);

            // Os trechos seguem a câmera que o jogador veria (no preview, a virtual), não só a real.
            layer.LoopOffset = wrap;
            // Sem repetir, os blocos não andam: o índice fica fixo, senão a arte trocaria na frente da câmera.
            layer.WrapIndex = settings.LoopHorizontally && layer.TileWidth > 0f ? Mathf.RoundToInt((view + context.VirtualCameraOffset.x) / layer.TileWidth) : 0;

            layer.transform.position = new Vector3(
                origin.x + shiftX + wrap,
                origin.y + offset.y * factor.y + scroll.y,
                origin.z);
        }
    }
}
