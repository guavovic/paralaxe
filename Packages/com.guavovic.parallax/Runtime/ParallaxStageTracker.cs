using System.Collections.Generic;
using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Decide a imagem de cada bloco da camada conforme os trechos.
    ///
    /// Cada bloco ocupa uma vaga do loop (índice do loop mais a posição do bloco). Na primeira vez que uma vaga aparece,
    /// ela recebe o trecho em que a câmera está e guarda esse trecho; a arte muda nos blocos que entram, fora da tela,
    /// e cada camada muda no seu ritmo de parallax. No modo esmaecer, a camada inteira troca com um fade.
    /// </summary>
    internal sealed class ParallaxStageTracker
    {
        private readonly Dictionary<int, int> _slotStage = new Dictionary<int, int>();
        private readonly List<(SpriteRenderer renderer, int index)> _tiles = new List<(SpriteRenderer renderer, int index)>();
        private Sprite _baseSprite;
        private int _shownStage = -1;
        private int _fadingTo = -1;
        private float _fade = 1f;
        private bool _started;

        /// <summary>Multiplicador de alpha da camada durante o esmaecer.</summary>
        public float Alpha => _fade;

        public void SetTiles(List<(SpriteRenderer renderer, int index)> tiles)
        {
            _tiles.Clear();
            _tiles.AddRange(tiles);
            if (_baseSprite == null && _tiles.Count > 0)
                _baseSprite = _tiles[0].renderer.sprite;
        }

        public void Reset()
        {
            _slotStage.Clear();
            _shownStage = -1;
            _fadingTo = -1;
            _fade = 1f;
            _started = false;
            foreach (var tile in _tiles)
            {
                if (tile.renderer != null && _baseSprite != null)
                    tile.renderer.sprite = _baseSprite;
            }
        }

        public static int StageAt(IReadOnlyList<ParallaxStage> stages, float cameraX)
        {
            int found = -1;
            for (int i = 0; i < stages.Count; i++)
            {
                if (cameraX >= stages[i].StartX)
                    found = i;
            }
            return found;
        }

        public void UpdateBlocks(IReadOnlyList<ParallaxStage> stages, int wrapIndex, float cameraX)
        {
            int current = StageAt(stages, cameraX);
            foreach (var tile in _tiles)
            {
                if (tile.renderer == null)
                    continue;

                int slot = wrapIndex + tile.index;
                if (!_slotStage.TryGetValue(slot, out int stage))
                {
                    stage = current;
                    _slotStage[slot] = stage;
                }

                int left = _slotStage.TryGetValue(slot - 1, out int leftStage) ? leftStage : stage;
                tile.renderer.sprite = SpriteFor(stages, stage, left);
            }
        }

        public void UpdateFade(IReadOnlyList<ParallaxStage> stages, float cameraX, float deltaTime, float seconds)
        {
            int target = StageAt(stages, cameraX);

            // Começando já dentro de um trecho (ao chegar por uma passagem, por exemplo), mostra o trecho direto:
            // o esmaecer é só para quando a câmera cruza o começo dele.
            if (!_started)
            {
                _started = true;
                _fadingTo = target;
                _fade = 1f;
                ShowStage(stages, target);
                return;
            }

            if (target != _shownStage)
                _fadingTo = target;

            float step = deltaTime / Mathf.Max(0.01f, seconds * 0.5f);
            if (_fadingTo == _shownStage)
            {
                _fade = Mathf.MoveTowards(_fade, 1f, step);
                return;
            }

            _fade = Mathf.MoveTowards(_fade, 0f, step);
            if (_fade > 0f)
                return;

            ShowStage(stages, _fadingTo);
        }

        private void ShowStage(IReadOnlyList<ParallaxStage> stages, int stage)
        {
            _shownStage = stage;
            var sprite = stage < 0 ? _baseSprite : stages[stage].Sprite;
            foreach (var tile in _tiles)
            {
                if (tile.renderer != null)
                    tile.renderer.sprite = sprite;
            }
        }

        private Sprite SpriteFor(IReadOnlyList<ParallaxStage> stages, int stage, int leftStage)
        {
            if (stage < 0)
                return _baseSprite;

            var current = stages[stage];
            if (leftStage < stage && current.Transition != null)
                return current.Transition;
            return current.Sprite != null ? current.Sprite : _baseSprite;
        }
    }
}
