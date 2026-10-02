using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class SolverTests
    {
        private const float TileWidth = 16f;

        private GameObject _layerObject;
        private ParallaxLayer _layer;
        private Texture2D _texture;
        private Sprite _sprite;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(16, 16);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 1f);

            _layerObject = new GameObject("Layer");
            _layer = _layerObject.AddComponent<ParallaxLayer>();

            var image = new GameObject("Image");
            image.transform.SetParent(_layerObject.transform, false);
            image.AddComponent<SpriteRenderer>().sprite = _sprite;

            _layer.Initialize(createCopies: false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_layerObject);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        private static ParallaxContext Context(float cameraX, float speed = 1f, float cameraZ = -10f, float time = 0f)
        {
            return new ParallaxContext(new Vector3(cameraX, 0f, cameraZ), new Vector3(0f, 0f, cameraZ), speed, 10f, time);
        }

        [Test]
        public void TileWidthComesFromTheFirstChild()
        {
            Assert.AreEqual(TileWidth, _layer.TileWidth, 0.001f);
        }

        [Test]
        public void FactorHalfMovesHalfOfTheCamera()
        {
            var settings = new ParallaxLayerSettings("L", new Vector2(0.5f, 0f), 0f);
            settings.SetLoopHorizontally(false);

            new Simulated2DSolver().Solve(_layer, settings, Context(6f));

            Assert.AreEqual(3f, _layer.transform.position.x, 0.001f);
        }

        [Test]
        public void SpeedMultiplierZeroFreezesTheLayer()
        {
            var settings = new ParallaxLayerSettings("L", new Vector2(0.5f, 0f), 0f);
            settings.SetLoopHorizontally(false);

            new Simulated2DSolver().Solve(_layer, settings, Context(6f, speed: 0f));

            Assert.AreEqual(0f, _layer.transform.position.x, 0.001f);
        }

        [Test]
        public void LoopKeepsTheLayerWithinHalfATileOfTheCamera()
        {
            var settings = new ParallaxLayerSettings("L", new Vector2(0.85f, 0f), 0f);
            var solver = new Simulated2DSolver();

            foreach (var cameraX in new[] { 0f, 7f, 40f, 100f, -250f })
            {
                solver.Solve(_layer, settings, Context(cameraX));
                float distance = Mathf.Abs(_layer.transform.position.x - cameraX);
                Assert.LessOrEqual(distance, TileWidth * 0.5f + 0.001f, "câmera em " + cameraX);
            }
        }

        [Test]
        public void PerspectiveAtTheFocusPlaneKeepsScaleOne()
        {
            var settings = new ParallaxLayerSettings("L", Vector2.zero, 0f);

            new PerspectiveSolver().Solve(_layer, settings, Context(0f));

            Assert.AreEqual(1f, _layer.transform.localScale.x, 0.001f);
            Assert.AreEqual(0f, _layer.transform.position.z, 0.001f);
        }

        [Test]
        public void PerspectiveScalesWithDistance()
        {
            var settings = new ParallaxLayerSettings("L", Vector2.zero, 10f);

            new PerspectiveSolver().Solve(_layer, settings, Context(0f));

            Assert.AreEqual(2f, _layer.transform.localScale.x, 0.001f);
            Assert.AreEqual(10f, _layer.transform.position.z, 0.001f);
        }

        [Test]
        public void AutoScrollMovesTheLayerWithTimeWithoutTheCamera()
        {
            var settings = new ParallaxLayerSettings("L", Vector2.zero, 0f);
            settings.SetLoopHorizontally(false);
            settings.SetAutoScroll(new Vector2(0.5f, 0f));

            new Simulated2DSolver().Solve(_layer, settings, Context(0f, time: 4f));

            Assert.AreEqual(2f, _layer.transform.position.x, 0.001f);
        }

        [Test]
        public void AutoScrollWithLoopStaysNearTheCamera()
        {
            var settings = new ParallaxLayerSettings("L", Vector2.zero, 0f);
            settings.SetAutoScroll(new Vector2(3f, 0f));
            var solver = new Simulated2DSolver();

            foreach (var time in new[] { 0f, 1f, 5.5f, 37f, 120f })
            {
                solver.Solve(_layer, settings, Context(0f, time: time));
                Assert.LessOrEqual(Mathf.Abs(_layer.transform.position.x), TileWidth * 0.5f + 0.001f, "tempo " + time);
            }
        }

        [Test]
        public void BlurIsClampedToTheSupportedRange()
        {
            var settings = new ParallaxLayerSettings();
            settings.SetBlur(99f);
            Assert.AreEqual(6f, settings.Blur, 0.001f);

            settings.SetBlur(-3f);
            Assert.AreEqual(0f, settings.Blur, 0.001f);
        }

        [Test]
        public void RestoreBringsBackOriginAndScale()
        {
            var settings = new ParallaxLayerSettings("L", Vector2.zero, 10f);
            new PerspectiveSolver().Solve(_layer, settings, Context(5f));

            _layer.Restore();

            Assert.AreEqual(Vector3.zero, _layer.transform.position);
            Assert.AreEqual(Vector3.one, _layer.transform.localScale);
        }
    }
}
