using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Uma camada do parallax. As imagens ficam nos filhos, e o primeiro filho define a largura do loop.
    /// </summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        private static readonly int WindStrengthId = Shader.PropertyToID("_WindStrength");
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");

        [SerializeField] private int settingsIndex;

        private SpriteRenderer[] _renderers;
        private MaterialPropertyBlock _block;
        private Vector3 _origin;
        private Vector3 _baseScale = Vector3.one;
        private float _tileWidth;
        private bool _copiesCreated;

        public int SettingsIndex { get => settingsIndex; set => settingsIndex = value; }
        public Vector3 Origin => _origin;
        public Vector3 BaseScale => _baseScale;
        public float TileWidth => _tileWidth;

        public void Initialize(bool createCopies)
        {
            _origin = transform.position;
            _baseScale = transform.localScale;
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _block = new MaterialPropertyBlock();

            if (transform.childCount == 0)
                return;

            var first = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _tileWidth = first != null ? first.bounds.size.x : 0f;

            if (createCopies && !_copiesCreated && _tileWidth > 0f)
                CreateCopies();
        }

        public void ApplyTint(Color tint)
        {
            if (_renderers == null)
                return;

            foreach (var spriteRenderer in _renderers)
                spriteRenderer.color = tint;
        }

        public void ApplyWind(float strength, float speed)
        {
            if (_renderers == null)
                return;

            foreach (var spriteRenderer in _renderers)
            {
                var material = spriteRenderer.sharedMaterial;
                if (material == null || !material.HasProperty(WindStrengthId))
                    continue;

                spriteRenderer.GetPropertyBlock(_block);
                _block.SetFloat(WindStrengthId, strength);
                _block.SetFloat(WindSpeedId, speed);
                spriteRenderer.SetPropertyBlock(_block);
            }
        }

        private void CreateCopies()
        {
            var source = transform.GetChild(0);

            foreach (var side in new[] { -1f, 1f })
            {
                var copy = Instantiate(source, transform);
                copy.name = source.name + (side < 0 ? " (esquerda)" : " (direita)");
                copy.position = source.position + Vector3.right * (_tileWidth * side);
            }

            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _copiesCreated = true;
        }
    }
}
