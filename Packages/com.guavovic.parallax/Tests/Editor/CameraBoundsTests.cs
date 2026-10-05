using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class CameraBoundsTests
    {
        private static readonly Rect Area = new Rect(-20f, -6f, 40f, 12f);
        private static readonly Vector2 Half = new Vector2(8f, 4.5f);

        [Test]
        public void CameraInsideTheAreaDoesNotMove()
        {
            var result = ParallaxCameraBounds.Clamp(new Vector2(3f, 1f), Half, Area, true, true);

            Assert.AreEqual(new Vector2(3f, 1f), result);
        }

        [Test]
        public void CameraStopsWhereTheViewTouchesTheEdge()
        {
            var result = ParallaxCameraBounds.Clamp(new Vector2(50f, -30f), Half, Area, true, true);

            Assert.AreEqual(20f - 8f, result.x, 0.001f);
            Assert.AreEqual(-6f + 4.5f, result.y, 0.001f);
        }

        [Test]
        public void TurnedOffAxisIsFree()
        {
            var result = ParallaxCameraBounds.Clamp(new Vector2(50f, -30f), Half, Area, false, true);

            Assert.AreEqual(50f, result.x);
        }

        [Test]
        public void AreaSmallerThanTheViewCentersTheCamera()
        {
            var result = ParallaxCameraBounds.Clamp(new Vector2(5f, 5f), Half, new Rect(0f, 0f, 10f, 4f), true, true);

            Assert.AreEqual(new Vector2(5f, 2f), result);
        }

        [Test]
        public void PerspectiveSeesTheSameAsOrthographicAtTheFocusPlane()
        {
            var camera = new GameObject("Camera").AddComponent<Camera>();
            try
            {
                camera.aspect = 16f / 9f;
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                var orthographic = ParallaxCameraBounds.HalfExtents(camera, 10f);

                ParallaxCamera.Match(camera, ParallaxMode.Perspective, 10f);
                var perspective = ParallaxCameraBounds.HalfExtents(camera, 10f);

                Assert.AreEqual(orthographic.x, perspective.x, 0.01f);
                Assert.AreEqual(orthographic.y, perspective.y, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }
    }
}
