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

            float wrap = 0f;
            if (settings.LoopHorizontally && layer.TileWidth > 0f)
                wrap = layer.TileWidth * Mathf.Round((offset.x * (1f - factor.x) - scroll.x) / layer.TileWidth);

            layer.transform.position = new Vector3(
                origin.x + offset.x * factor.x + scroll.x + wrap,
                origin.y + offset.y * factor.y + scroll.y,
                origin.z);
        }
    }
}
