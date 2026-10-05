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
        [Tooltip("Mostra só os elementos que estão entre estes X do mundo. Serve para a cena que muda de cenário. Mínimo igual ao máximo desliga.")]
        [SerializeField] private Vector2 visibleRangeX;

        private readonly List<Transform> _items = new List<Transform>();
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<float> _baseX = new List<float>();

        public int Count => count;
        public float Span => span;
        public Vector2 VisibleRangeX { get => visibleRangeX; set => visibleRangeX = value; }

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

        internal void Recycle(ParallaxLayer layer, float cameraX)
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

                if (limited && _renderers[i] != null)
                {
                    float x = layer.transform.position.x + (offset + position.x) * scale;
                    _renderers[i].enabled = x >= visibleRangeX.x && x <= visibleRangeX.y;
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
                if (_renderers[i] != null)
                    _renderers[i].enabled = true;
            }
        }

        private void CollectItems()
        {
            _items.Clear();
            _baseX.Clear();
            _renderers.Clear();
            foreach (Transform child in transform)
            {
                _items.Add(child);
                _baseX.Add(child.localPosition.x);
                _renderers.Add(child.GetComponent<SpriteRenderer>());
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
