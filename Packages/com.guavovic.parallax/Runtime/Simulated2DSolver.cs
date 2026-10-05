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
            float wrap = 0f;
            if (settings.LoopHorizontally && layer.TileWidth > 0f)
                wrap = layer.TileWidth * Mathf.Round((context.CameraOffset.x - shiftX) / layer.TileWidth);

            // Os trechos seguem a câmera que o jogador veria (no preview, a virtual), não só a real.
            layer.WrapIndex = layer.TileWidth > 0f ? Mathf.RoundToInt((offset.x - shiftX) / layer.TileWidth) : 0;

            layer.transform.position = new Vector3(
                origin.x + shiftX + wrap,
                origin.y + offset.y * factor.y + scroll.y,
                origin.z);
        }
    }
}
