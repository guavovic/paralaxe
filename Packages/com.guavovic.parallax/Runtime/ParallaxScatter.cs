using System.Collections.Generic;
using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Espalha variações de sprites pela camada, num trecho bem maior que a imagem do loop e com um sorteio fixo.
    /// Os elementos andam com a camada e voltam pelo outro lado quando saem do trecho, sem seguir o loop da imagem,
    /// então o mesmo padrão não se repete a cada tela. Fica como filho da camada (nunca o primeiro), sem escala nem rotação.
    /// </summary>
    public sealed class ParallaxScatter : MonoBehaviour
    {
        [SerializeField] private Sprite[] variants = new Sprite[0];
        [SerializeField, Min(1)] private int count = 12;
        [Tooltip("Largura do trecho, em unidades da camada. Quanto maior que a imagem do loop, menos repetição.")]
        [SerializeField, Min(1f)] private float span = 60f;
        [Tooltip("Altura dos elementos, de mínimo a máximo, em unidades da camada.")]
        [SerializeField] private Vector2 heightRange;
        [SerializeField] private Vector2 scaleRange = Vector2.one;
        [SerializeField] private bool randomFlip = true;
        [SerializeField] private int seed = 1;
        [Tooltip("Vazio, usa o material da primeira imagem da camada (com o vento e o desfoque dela).")]
        [SerializeField] private Material material;
        [SerializeField] private int sortingOrder;
        [Tooltip("Quanto o vento balança estes elementos, sobre o vento da camada. 0 deixa parado (pedra, construção).")]
        [SerializeField, Min(0f)] private float windInfluence = 1f;
        [Tooltip("Mostra só os elementos que passam pelo meio da tela com a câmera entre estes X. Serve para a fase que muda de cenário. Mínimo igual ao máximo desliga.")]
        [SerializeField] private Vector2 visibleRangeX;
        [Tooltip("Como os elementos reagem quando o herói passa por eles na tela.")]
        [SerializeField] private ParallaxReaction reaction;
        [Tooltip("Distância na horizontal, em unidades do mundo, em que o herói faz o elemento reagir.")]
        [SerializeField, Min(0.1f)] private float reactionRadius = 0.8f;
        [SerializeField, Min(0f)] private float reactionStrength = 1f;
        [SerializeField] private string reactionTag = "Player";

        private const float ReactionSeconds = 1.6f;

        private readonly List<Transform> _items = new List<Transform>();
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<float> _baseX = new List<float>();
        private readonly List<Vector3> _baseScale = new List<Vector3>();
        private readonly List<float> _reactedAt = new List<float>();
        private readonly List<bool> _near = new List<bool>();
        private Transform _target;
        private float _nextTargetSearch;

        public int Count => count;
        public float Span => span;
        public Vector2 VisibleRangeX { get => visibleRangeX; set => visibleRangeX = value; }
        public float WindInfluence { get => windInfluence; set => windInfluence = Mathf.Max(0f, value); }
        public ParallaxReaction Reaction { get => reaction; set => reaction = value; }
        public float ReactionRadius { get => reactionRadius; set => reactionRadius = Mathf.Max(0.1f, value); }

        public void Configure(Sprite[] sprites, int amount, float width, Vector2 height, Vector2 scale, int randomSeed, int order)
        {
            variants = sprites;
            count = Mathf.Max(1, amount);
            span = Mathf.Max(1f, width);
            heightRange = height;
            scaleRange = scale;
            seed = randomSeed;
            sortingOrder = order;
        }

        /// <summary>
        /// Apaga os elementos e sorteia de novo. Cada elemento cai numa faixa própria do trecho, para não amontoar.
        /// </summary>
        [ContextMenu("Espalhar de novo")]
        public void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }

            _items.Clear();
            _baseX.Clear();
            _renderers.Clear();
            if (variants == null || variants.Length == 0)
                return;

            var random = new System.Random(seed);
            var shared = material != null ? material : LayerMaterial();
            float slot = span / count;
            for (int i = 0; i < count; i++)
            {
                var sprite = variants[random.Next(variants.Length)];
                float x = -span * 0.5f + slot * (i + 0.15f + 0.7f * (float)random.NextDouble());
                float y = Mathf.Lerp(heightRange.x, heightRange.y, (float)random.NextDouble());
                float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)random.NextDouble());
                bool flip = randomFlip && random.Next(2) == 1;

                var item = new GameObject(sprite != null ? sprite.name : "Elemento");
                item.transform.SetParent(transform, false);
                item.transform.localPosition = new Vector3(x, y, 0f);
                item.transform.localScale = new Vector3(scale, scale, 1f);
                var spriteRenderer = item.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.flipX = flip;
                spriteRenderer.sortingOrder = sortingOrder;
                if (shared != null)
                    spriteRenderer.sharedMaterial = shared;
            }

            CollectItems();
        }

        /// <param name="factor">Fator X da camada (modo 2D), para saber onde a câmera vai estar quando cada elemento passar pelo meio da tela.</param>
        internal void Recycle(ParallaxLayer layer, float cameraX, float factor)
        {
            if (_items.Count != transform.childCount)
                CollectItems();

            float scale = Mathf.Abs(layer.transform.lossyScale.x);
            if (scale <= 0f)
                return;

            // Sem o loop, a camada anda contínua; cada elemento dá a volta no próprio trecho, perto da câmera.
            float offset = transform.localPosition.x;
            float unwrapped = layer.transform.position.x - layer.LoopOffset;
            float loopLocal = layer.LoopOffset / scale;
            bool limited = visibleRangeX.y > visibleRangeX.x;
            bool reacts = reaction != ParallaxReaction.None && Application.isPlaying && FindTarget();
            float targetX = reacts ? _target.position.x : 0f;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == null)
                    continue;

                float worldX = unwrapped + (offset + _baseX[i]) * scale;
                float turns = Mathf.Round((cameraX - worldX) / (span * scale));
                var position = item.localPosition;
                position.x = _baseX[i] + turns * span - loopLocal;
                item.localPosition = position;

                if (reacts)
                    React(i, layer.transform.position.x + (offset + position.x) * scale, targetX);

                if (limited && _renderers[i] != null)
                {
                    // O elemento anda "factor" por unidade da câmera, então cruza o meio da tela quando a câmera chega
                    // em (x - factor * câmera) / (1 - factor). Esse ponto não muda enquanto a câmera anda: nada pisca.
                    float x = layer.transform.position.x + (offset + position.x) * scale;
                    float crossing = Mathf.Abs(1f - factor) < 0.001f ? cameraX : (x - factor * cameraX) / (1f - factor);
                    _renderers[i].enabled = crossing >= visibleRangeX.x && crossing <= visibleRangeX.y;
                }
            }
        }

        internal void Restore()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null)
                    continue;

                var position = _items[i].localPosition;
                position.x = _baseX[i];
                _items[i].localPosition = position;
                _items[i].localRotation = Quaternion.identity;
                _items[i].localScale = _baseScale[i];
                _reactedAt[i] = float.NegativeInfinity;
                if (_renderers[i] != null)
                    _renderers[i].enabled = true;
            }
        }

        private void CollectItems()
        {
            _items.Clear();
            _baseX.Clear();
            _renderers.Clear();
            _baseScale.Clear();
            _reactedAt.Clear();
            _near.Clear();
            foreach (Transform child in transform)
            {
                _items.Add(child);
                _baseX.Add(child.localPosition.x);
                _renderers.Add(child.GetComponent<SpriteRenderer>());
                _baseScale.Add(child.localScale);
                _reactedAt.Add(float.NegativeInfinity);
                _near.Add(false);
            }
        }

        private bool FindTarget()
        {
            if (_target != null)
                return true;
            if (Time.time < _nextTargetSearch)
                return false;

            // Procura de vez em quando, e não a cada quadro, se o herói ainda não existe.
            _nextTargetSearch = Time.time + 1f;
            var found = string.IsNullOrEmpty(reactionTag) ? null : GameObject.FindGameObjectWithTag(reactionTag);
            _target = found != null ? found.transform : null;
            return _target != null;
        }

        /// <summary>
        /// Reage quando o herói chega perto (na horizontal da tela) e assenta com amortecimento.
        /// </summary>
        private void React(int i, float itemX, float targetX)
        {
            bool near = Mathf.Abs(itemX - targetX) < reactionRadius;
            if (near && !_near[i])
                _reactedAt[i] = Time.time;
            _near[i] = near;

            float t = Time.time - _reactedAt[i];
            if (float.IsInfinity(t))
                return;

            var item = _items[i];
            if (t >= ReactionSeconds)
            {
                item.localRotation = Quaternion.identity;
                item.localScale = _baseScale[i];
                _reactedAt[i] = float.NegativeInfinity;
                return;
            }

            float decay = Mathf.Exp(-t * 3.5f);
            if (reaction == ParallaxReaction.Sway)
            {
                float side = targetX < itemX ? 1f : -1f;
                item.localRotation = Quaternion.Euler(0f, 0f, side * 12f * reactionStrength * Mathf.Sin(t * 14f) * decay);
            }
            else
            {
                float stretch = 0.18f * reactionStrength * Mathf.Sin(t * 16f) * decay;
                var baseScale = _baseScale[i];
                item.localScale = new Vector3(baseScale.x * (1f - stretch * 0.5f), baseScale.y * (1f + stretch), baseScale.z);
            }
        }

        private Material LayerMaterial()
        {
            var layer = transform.parent;
            if (layer == null || layer.childCount == 0)
                return null;

            var first = layer.GetChild(0).GetComponent<SpriteRenderer>();
            return first != null ? first.sharedMaterial : null;
        }
    }
}
