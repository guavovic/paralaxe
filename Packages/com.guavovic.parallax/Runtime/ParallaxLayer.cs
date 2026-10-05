using System.Collections.Generic;
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
        private static readonly int BlurId = Shader.PropertyToID("_Blur");

        [SerializeField] private int settingsIndex;

        private SpriteRenderer[] _renderers;
        private MaterialPropertyBlock _block;
        private Vector3 _origin;
        private Vector3 _baseScale = Vector3.one;
        private float _tileWidth;
        private bool _copiesCreated;
        private readonly List<GameObject> _temporaryCopies = new List<GameObject>();
        private bool _materialApplied;
        private float _windStrength;
        private float _windSpeed;
        private float _blur;

        public int SettingsIndex { get => settingsIndex; set => settingsIndex = value; }
        public Vector3 Origin => _origin;
        public Vector3 BaseScale => _baseScale;
        public float TileWidth => _tileWidth;

        public void Initialize(bool createCopies)
        {
            Initialize(createCopies, temporaryCopies: false);
        }

        /// <summary>
        /// Com <paramref name="temporaryCopies"/>, as cópias do loop não são salvas na cena e saem em
        /// <see cref="RemoveTemporaryCopies"/>. Serve para o preview do editor.
        /// </summary>
        internal void Initialize(bool createCopies, bool temporaryCopies)
        {
            _origin = transform.position;
            _baseScale = transform.localScale;
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _block = new MaterialPropertyBlock();
            _materialApplied = false;

            if (transform.childCount == 0)
                return;

            var first = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _tileWidth = first != null ? first.bounds.size.x : 0f;

            if (createCopies && !_copiesCreated && _tileWidth > 0f)
                CreateCopies(temporaryCopies);
        }

        internal void RemoveTemporaryCopies()
        {
            if (_temporaryCopies.Count == 0)
                return;

            foreach (var copy in _temporaryCopies)
            {
                if (copy != null)
                    DestroyImmediate(copy);
            }

            _temporaryCopies.Clear();
            _copiesCreated = false;
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        public void Restore()
        {
            transform.position = _origin;
            transform.localScale = _baseScale;
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
            _materialApplied = false;
            SetMaterialProperties(strength, speed, null);
        }

        public void ApplyBlur(float blur)
        {
            _materialApplied = false;
            SetMaterialProperties(null, 0f, blur);
        }

        /// <summary>
        /// Vento e desfoque numa passada só por renderer, e só quando algum valor mudou desde a última vez.
        /// </summary>
        internal void ApplyMaterialProperties(float windStrength, float windSpeed, float blur)
        {
            if (_materialApplied && windStrength == _windStrength && windSpeed == _windSpeed && blur == _blur)
                return;

            SetMaterialProperties(windStrength, windSpeed, blur);
            _windStrength = windStrength;
            _windSpeed = windSpeed;
            _blur = blur;
            _materialApplied = true;
        }

        private void SetMaterialProperties(float? windStrength, float windSpeed, float? blur)
        {
            if (_renderers == null)
                return;

            foreach (var spriteRenderer in _renderers)
            {
                var material = spriteRenderer.sharedMaterial;
                if (material == null)
                    continue;

                bool wind = windStrength.HasValue && material.HasProperty(WindStrengthId);
                bool blurs = blur.HasValue && material.HasProperty(BlurId);
                if (!wind && !blurs)
                    continue;

                spriteRenderer.GetPropertyBlock(_block);
                if (wind)
                {
                    _block.SetFloat(WindStrengthId, windStrength.Value);
                    _block.SetFloat(WindSpeedId, windSpeed);
                }

                if (blurs)
                    _block.SetFloat(BlurId, blur.Value);

                spriteRenderer.SetPropertyBlock(_block);
            }
        }

        private void CreateCopies(bool temporary)
        {
            var source = transform.GetChild(0);

            foreach (var side in new[] { -1f, 1f })
            {
                var copy = Instantiate(source, transform);
                copy.name = source.name + (side < 0 ? " (esquerda)" : " (direita)");
                copy.position = source.position + Vector3.right * (_tileWidth * side);

                if (!temporary)
                    continue;

                copy.gameObject.hideFlags = HideFlags.HideAndDontSave;
                _temporaryCopies.Add(copy.gameObject);
            }

            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _copiesCreated = true;
        }
    }
}
