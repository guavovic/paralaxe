using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ModeSwitchTests
    {
        private GameObject _object;
        private ParallaxLayer _layer;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("Layer");
            _layer = _object.AddComponent<ParallaxLayer>();
            _layer.Initialize(createCopies: false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
        }

        [Test]
        public void BackTo2DRestoresTheScaleSetByPerspective()
        {
            var settings = new ParallaxLayerSettings("Céu", new Vector2(0.99f, 1f), 990f);
            var context = new ParallaxContext(new Vector3(2f, 0f, -10f), new Vector3(0f, 0f, -10f), 1f, 10f);

            new PerspectiveSolver().Solve(_layer, settings, context);
            Assert.Greater(_layer.transform.localScale.x, 50f);

            new Simulated2DSolver().Solve(_layer, settings, context);
            Assert.AreEqual(Vector3.one, _layer.transform.localScale);
        }

        [Test]
        public void CameraMatchKeepsTheVisibleAreaAtTheFocusPlane()
        {
            var camera = new GameObject("Camera").AddComponent<Camera>();
            try
            {
                camera.orthographic = true;
                camera.orthographicSize = 5f;

                ParallaxCamera.Match(camera, ParallaxMode.Perspective, 10f);
                Assert.IsFalse(camera.orthographic);
                Assert.AreEqual(53.13f, camera.fieldOfView, 0.01f);

                ParallaxCamera.Match(camera, ParallaxMode.Simulated2D, 10f);
                Assert.IsTrue(camera.orthographic);
                Assert.AreEqual(5f, camera.orthographicSize, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        [Test]
        public void CameraMatchesOnlyTheProjectionOfItsMode()
        {
            var camera = new GameObject("Camera").AddComponent<Camera>();
            try
            {
                camera.orthographic = true;
                Assert.IsTrue(ParallaxCamera.Matches(camera, ParallaxMode.Simulated2D));
                Assert.IsFalse(ParallaxCamera.Matches(camera, ParallaxMode.Perspective));
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }
    }
}
