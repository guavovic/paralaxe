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
        [Tooltip("O golpe do herói corta estes elementos (mato, cipó): somem e voltam depois de um tempo.")]
        [SerializeField] private bool cuttable;
        [Tooltip("Segundos até o que foi cortado voltar. 0 não volta.")]
        [SerializeField, Min(0f)] private float regrowSeconds = 8f;
        [Tooltip("Cor dos pedaços que voam ao cortar. Transparente: sem pedaços.")]
        [SerializeField] private Color cutColor = new Color(0.4f, 0.75f, 0.55f, 1f);

        private const float ReactionSeconds = 1.6f;

        /// <summary>Estado de um elemento espalhado: onde nasceu e o que está acontecendo com ele.</summary>
        private sealed class Item
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float BaseX;
            public Vector3 BaseScale;
            public float ReactedAt = float.NegativeInfinity;
            public bool Near;
            public float CutUntil = float.NegativeInfinity;
        }

        private readonly List<Item> _items = new List<Item>();
        private Transform _target;
        private float _nextTargetSearch;
        private bool _wasLimited;

        public int Count => count;
        public float Span => span;
        public Vector2 VisibleRangeX { get => visibleRangeX; set => visibleRangeX = value; }
        public float WindInfluence { get => windInfluence; set => windInfluence = Mathf.Max(0f, value); }
        public ParallaxReaction Reaction { get => reaction; set => reaction = value; }
        public float ReactionRadius { get => reactionRadius; set => reactionRadius = Mathf.Max(0.1f, value); }
        public bool Cuttable { get => cuttable; set => cuttable = value; }
        public Color CutColor { get => cutColor; set => cutColor = value; }
        public float RegrowSeconds { get => regrowSeconds; set => regrowSeconds = Mathf.Max(0f, value); }

        /// <summary>
        /// Corta os elementos visíveis que encostam em <paramref name="area"/> (no mundo). Devolve quantos cortou e,
        /// em <paramref name="hits"/>, onde estavam, para quem quiser soltar pedaços ou tocar um som.
        /// </summary>
        public int Cut(Bounds area, List<Vector3> hits = null)
        {
            if (!cuttable)
                return 0;
            if (_items.Count != transform.childCount)
                CollectItems();

            int cut = 0;
            foreach (var item in _items)
            {
                var spriteRenderer = item.Renderer;
                if (spriteRenderer == null || !spriteRenderer.enabled || item.CutUntil > Time.time)
                    continue;
                if (!spriteRenderer.bounds.Intersects(area))
                    continue;

                item.CutUntil = regrowSeconds > 0f ? Time.time + regrowSeconds : float.PositiveInfinity;
                spriteRenderer.enabled = false;
                hits?.Add(spriteRenderer.bounds.center);
                cut++;
            }
            return cut;
        }

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
            // Sai da hierarquia antes de destruir: em Play o Destroy espera o fim do quadro, e a contagem de filhos
            // (que a camada e este grupo usam para se atualizar) já tem que estar certa agora.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                child.SetParent(null, false);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            var layer = GetComponentInParent<ParallaxLayer>();
            if (variants != null && variants.Length > 0)
            {
                var random = new System.Random(seed);
                var first = layer != null ? layer.FirstTile : null;
                var shared = material != null ? material : first != null ? first.sharedMaterial : null;
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
            }

            CollectItems();

            // A camada guarda os renderers para cor, vento e desfoque: sem avisar, ela segue mexendo nos apagados.
            if (layer != null)
                layer.RefreshRenderers();
        }

        /// <summary>
        /// Leva cada elemento para perto da câmera. O fator da câmera vem do solver da camada (quanto ela anda por
        /// unidade da câmera), para saber onde a câmera vai estar quando cada elemento passar pelo meio da tela.
        /// </summary>
        internal void Recycle(ParallaxLayer layer, float layerX, float scale, float cameraX)
        {
            if (_items.Count != transform.childCount)
                CollectItems();
            if (scale <= 0f)
                return;

            // Sem o loop, a camada anda contínua; cada elemento dá a volta no próprio trecho, perto da câmera.
            float factor = layer.CameraFactor;
            float offset = transform.localPosition.x;
            float unwrapped = layerX - layer.LoopOffset;
            float loopLocal = layer.LoopOffset / scale;
            bool limited = visibleRangeX.y > visibleRangeX.x;
            bool unlimitedNow = _wasLimited && !limited;
            _wasLimited = limited;
            bool reacts = reaction != ParallaxReaction.None && Application.isPlaying && FindTarget();
            float targetX = reacts ? _target.position.x : 0f;
            foreach (var item in _items)
            {
                if (item.Transform == null)
                    continue;

                float turns = Mathf.Round((cameraX - (unwrapped + (offset + item.BaseX) * scale)) / (span * scale));
                var position = item.Transform.localPosition;
                position.x = item.BaseX + turns * span - loopLocal;
                item.Transform.localPosition = position;
                float worldX = layerX + (offset + position.x) * scale;

                if (reacts)
                    React(item, worldX, targetX);

                bool wasCut = !float.IsNegativeInfinity(item.CutUntil);
                if ((!limited && !wasCut && !unlimitedNow) || item.Renderer == null)
                    continue;

                bool show = true;
                if (limited)
                {
                    // O elemento anda "factor" por unidade da câmera, então cruza o meio da tela quando a câmera chega
                    // em (x - factor * câmera) / (1 - factor). Esse ponto não muda enquanto a câmera anda: nada pisca.
                    float crossing = Mathf.Abs(1f - factor) < 0.001f ? cameraX : (worldX - factor * cameraX) / (1f - factor);
                    show = crossing >= visibleRangeX.x && crossing <= visibleRangeX.y;
                }

                if (wasCut && Time.time >= item.CutUntil)
                    item.CutUntil = float.NegativeInfinity;
                else if (wasCut)
                    show = false;

                if (item.Renderer.enabled != show)
                    item.Renderer.enabled = show;
            }
        }

        internal void Restore()
        {
            foreach (var item in _items)
            {
                if (item.Transform == null)
                    continue;

                var position = item.Transform.localPosition;
                position.x = item.BaseX;
                item.Transform.localPosition = position;
                item.Transform.localRotation = Quaternion.identity;
                item.Transform.localScale = item.BaseScale;
                item.ReactedAt = float.NegativeInfinity;
                item.CutUntil = float.NegativeInfinity;
                if (item.Renderer != null)
                    item.Renderer.enabled = true;
            }
        }

        private void CollectItems()
        {
            _items.Clear();
            foreach (Transform child in transform)
            {
                _items.Add(new Item
                {
                    Transform = child,
                    Renderer = child.GetComponent<SpriteRenderer>(),
                    BaseX = child.localPosition.x,
                    BaseScale = child.localScale,
                });
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
        private void React(Item item, float itemX, float targetX)
        {
            bool near = Mathf.Abs(itemX - targetX) < reactionRadius;
            if (near && !item.Near)
                item.ReactedAt = Time.time;
            item.Near = near;

            float t = Time.time - item.ReactedAt;
            if (float.IsInfinity(t))
                return;

            if (t >= ReactionSeconds)
            {
                item.Transform.localRotation = Quaternion.identity;
                item.Transform.localScale = item.BaseScale;
                item.ReactedAt = float.NegativeInfinity;
                return;
            }

            float decay = Mathf.Exp(-t * 3.5f);
            if (reaction == ParallaxReaction.Sway)
            {
                float side = targetX < itemX ? 1f : -1f;
                item.Transform.localRotation = Quaternion.Euler(0f, 0f, side * 12f * reactionStrength * Mathf.Sin(t * 14f) * decay);
            }
            else
            {
                float stretch = 0.18f * reactionStrength * Mathf.Sin(t * 16f) * decay;
                var baseScale = item.BaseScale;
                item.Transform.localScale = new Vector3(baseScale.x * (1f - stretch * 0.5f), baseScale.y * (1f + stretch), baseScale.z);
            }
        }
    }
}
