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
        [Tooltip("Trechos do cenário: a arte da camada muda conforme a câmera avança. Vazio, a camada só repete a mesma imagem.")]
        [SerializeField] private List<ParallaxStage> stages = new List<ParallaxStage>();
        [Tooltip("Troca a camada inteira com um esmaecer, em vez de bloco a bloco. Bom para camadas muito distantes, que quase não andam.")]
        [SerializeField] private bool fadeBetweenStages;
        [SerializeField, Min(0.05f)] private float fadeSeconds = 1f;

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
        private readonly ParallaxStageTracker _stageTracker = new ParallaxStageTracker();
        private Color _tint = Color.white;
        private float _lastStageTime = -1f;
        private ParallaxScatter[] _scatters = System.Array.Empty<ParallaxScatter>();

        public int SettingsIndex { get => settingsIndex; set => settingsIndex = value; }
        public Vector3 Origin => _origin;
        public Vector3 BaseScale => _baseScale;
        public float TileWidth => _tileWidth;
        public IReadOnlyList<ParallaxStage> Stages => stages;
        public bool FadeBetweenStages { get => fadeBetweenStages; set => fadeBetweenStages = value; }

        /// <summary>Índice do loop atual, escrito pelo solver: quantos blocos a camada já deu a volta.</summary>
        public int WrapIndex { get; internal set; }

        /// <summary>Quanto o loop deslocou a camada no último quadro, em unidades do mundo. Escrito pelo solver.</summary>
        public float LoopOffset { get; internal set; }

        public void AddStage(ParallaxStage stage) => stages.Add(stage);

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
            _scatters = GetComponentsInChildren<ParallaxScatter>(true);

            if (transform.childCount == 0)
                return;

            var first = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _tileWidth = first != null ? first.bounds.size.x : 0f;

            if (createCopies && !_copiesCreated && _tileWidth > 0f)
                CreateCopies(temporaryCopies);

            CollectTiles();
            ResetStages();
        }

        /// <summary>
        /// Atualiza a arte dos blocos conforme os trechos e a posição X da câmera.
        /// </summary>
        internal void UpdateStages(float cameraX)
        {
            if (stages.Count == 0)
                return;

            if (!fadeBetweenStages)
            {
                _stageTracker.UpdateBlocks(stages, WrapIndex, cameraX);
                return;
            }

            float now = Time.realtimeSinceStartup;
            float delta = _lastStageTime < 0f ? 0f : now - _lastStageTime;
            _lastStageTime = now;
            _stageTracker.UpdateFade(stages, cameraX, delta, fadeSeconds);
            ApplyTint(_tint);
        }

        /// <summary>
        /// Leva os elementos espalhados para perto da câmera, sem seguir o loop da imagem.
        /// </summary>
        internal void UpdateScatters(float cameraX, float factor)
        {
            foreach (var scatter in _scatters)
            {
                if (scatter != null)
                    scatter.Recycle(this, cameraX, factor);
            }
        }

        internal void ResetStages()
        {
            _stageTracker.Reset();
            _lastStageTime = -1f;
            ApplyTint(_tint);
        }

        private void CollectTiles()
        {
            var tiles = new List<(SpriteRenderer renderer, int index)>();
            var first = transform.GetChild(0);
            foreach (Transform child in transform)
            {
                var spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    continue;

                if (child == first)
                    tiles.Add((spriteRenderer, 0));
                else if (child.name.EndsWith(" (esquerda)"))
                    tiles.Add((spriteRenderer, -1));
                else if (child.name.EndsWith(" (direita)"))
                    tiles.Add((spriteRenderer, 1));
            }
            _stageTracker.SetTiles(tiles);
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
            if (transform.childCount > 0)
            {
                ResetStages();
                CollectTiles();
            }
        }

        public void Restore()
        {
            transform.position = _origin;
            transform.localScale = _baseScale;
            LoopOffset = 0f;
            foreach (var scatter in _scatters)
            {
                if (scatter != null)
                    scatter.Restore();
            }
        }

        public void ApplyTint(Color tint)
        {
            _tint = tint;
            if (_renderers == null)
                return;

            tint.a *= _stageTracker.Alpha;
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
