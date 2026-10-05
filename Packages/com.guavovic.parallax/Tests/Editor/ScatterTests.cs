using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ScatterTests
    {
        private Texture2D _texture;
        private Sprite _tile;
        private Sprite _bush;
        private ParallaxProfile _profile;
        private ParallaxRig _rig;
        private Camera _camera;
        private ParallaxLayer _layer;
        private ParallaxScatter _scatter;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(16, 16);
            _tile = Sprite.Create(_texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 1f);
            _bush = Sprite.Create(_texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f), 4f);

            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _profile.AddLayer(new ParallaxLayerSettings("Meio", new Vector2(0.5f, 0f), 0f));
            _camera = new GameObject("Camera").AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            _rig.Profile = _profile;
            _rig.TargetCamera = _camera;

            _layer = new GameObject("Camada").AddComponent<ParallaxLayer>();
            _layer.transform.SetParent(_rig.transform, false);
            var image = new GameObject("Imagem").AddComponent<SpriteRenderer>();
            image.transform.SetParent(_layer.transform, false);
            image.sprite = _tile;

            _scatter = new GameObject("Espalhados").AddComponent<ParallaxScatter>();
            _scatter.transform.SetParent(_layer.transform, false);
            _scatter.Configure(new[] { _bush }, 10, 100f, new Vector2(-2f, 2f), new Vector2(0.8f, 1.2f), 7, 3);
            _scatter.Rebuild();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rig.gameObject);
            Object.DestroyImmediate(_camera.gameObject);
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_bush);
            Object.DestroyImmediate(_tile);
            Object.DestroyImmediate(_texture);
        }

        private float[] ItemsX()
        {
            var xs = new float[_scatter.transform.childCount];
            for (int i = 0; i < xs.Length; i++)
                xs[i] = _scatter.transform.GetChild(i).position.x;
            return xs;
        }

        private void MoveCamera(float x)
        {
            _camera.transform.position = new Vector3(x, 0f, -10f);
            _rig.Apply();
        }

        [Test]
        public void SameSeedGivesTheSamePlacement()
        {
            var first = ItemsX();
            _scatter.Rebuild();

            Assert.AreEqual(10, _scatter.transform.childCount);
            CollectionAssert.AreEqual(first, ItemsX());
        }

        [Test]
        public void ItemsStayWithinHalfASpanOfTheCamera()
        {
            _rig.Initialize();

            foreach (var cameraX in new[] { 0f, 37f, 260f, -415f, 1000f })
            {
                MoveCamera(cameraX);
                foreach (var x in ItemsX())
                    Assert.LessOrEqual(Mathf.Abs(x - cameraX), 50f + 0.001f, $"câmera em {cameraX}");
            }
        }

        [Test]
        public void ItemsMoveSmoothlyWhenTheImageLoops()
        {
            _rig.Initialize();
            MoveCamera(0f);
            var before = ItemsX();

            // Andando de pouco em pouco, a imagem da camada dá várias voltas (16 de largura), e os elementos
            // só andam o parallax (metade da câmera) ou dão a volta inteira no próprio trecho.
            for (float cameraX = 0.25f; cameraX <= 40f; cameraX += 0.25f)
            {
                MoveCamera(cameraX);
                var after = ItemsX();
                for (int i = 0; i < after.Length; i++)
                {
                    float step = after[i] - before[i];
                    bool parallax = Mathf.Abs(step - 0.125f) < 0.001f;
                    bool turned = Mathf.Abs(Mathf.Abs(step - 0.125f) - 100f) < 0.001f;
                    Assert.IsTrue(parallax || turned, $"elemento {i} pulou {step} com a câmera em {cameraX}");
                }

                before = after;
            }
        }

        [Test]
        public void PreviewResetPutsTheItemsBack()
        {
            var placed = ItemsX();
            _rig.Preview(new Vector3(300f, 0f, 0f), withWind: false);
            _rig.ResetPreview();

            CollectionAssert.AreEqual(placed, ItemsX());
        }
    }
}
