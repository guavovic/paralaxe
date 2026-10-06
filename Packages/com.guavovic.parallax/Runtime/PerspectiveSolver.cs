using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Para câmera em perspectiva. A camada vai para a profundidade real e é ampliada na mesma
    /// proporção, então parece igual de onde a câmera começou e o parallax vem da própria perspectiva.
    /// </summary>
    public sealed class PerspectiveSolver : IParallaxSolver
    {
        public void Solve(ParallaxLayer layer, ParallaxLayerSettings settings, in ParallaxContext context)
        {
            float focus = context.FocusDistance;
            float distance = Mathf.Max(0.01f, focus + settings.Depth);
            float scale = distance / focus;

            Vector3 origin = layer.Origin;
            Vector3 cameraOrigin = context.CameraOrigin;
            float centerX = cameraOrigin.x + (origin.x - cameraOrigin.x) * scale;
            float centerY = cameraOrigin.y + (origin.y - cameraOrigin.y) * scale;

            // Visto da câmera parada, uma camada com escala s anda (s - 1) vezes o deslocamento,
            // o mesmo que pareceria andar se a câmera tivesse se movido.
            centerX += context.VirtualCameraOffset.x * (scale - 1f);

            Vector2 scroll = settings.AutoScroll * context.Time * scale;
            centerX += scroll.x;
            centerY += scroll.y;

            // O fator Y diz quanto a camada acompanha a câmera na vertical. Com 1 ela fica sempre cobrindo a tela,
            // então a borda do sprite nunca aparece quando a câmera sobe ou desce.
            centerY += context.CameraOffset.y * settings.Factor.y;

            float wrap = 0f;
            float tileWidth = layer.TileWidth * scale;
            if (settings.LoopHorizontally && tileWidth > 0f)
                wrap = tileWidth * Mathf.Round((context.CameraPosition.x - centerX) / tileWidth);

            layer.LoopOffset = wrap;

            // Os trechos seguem a câmera que o jogador veria (no preview, a virtual), não só a real.
            float virtualX = context.VirtualCameraOffset.x;
            layer.WrapIndex = settings.LoopHorizontally && tileWidth > 0f
                ? Mathf.RoundToInt((context.CameraPosition.x + virtualX - (centerX - virtualX * (scale - 1f))) / tileWidth)
                : 0;

            layer.transform.localScale = layer.BaseScale * scale;
            layer.transform.position = new Vector3(centerX + wrap, centerY, cameraOrigin.z + distance);
        }
    }
}
