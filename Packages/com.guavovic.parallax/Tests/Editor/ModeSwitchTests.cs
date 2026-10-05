using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ModeSwitchTests
    {
        [Test]
        public void BackTo2DRestoresTheScaleSetByPerspective()
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            var rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            try
            {
                profile.AddLayer(new ParallaxLayerSettings("Céu", new Vector2(0.99f, 1f), 990f));
                profile.SetMode(ParallaxMode.Perspective);
                rig.Profile = profile;
                var layer = new GameObject("Camada").AddComponent<ParallaxLayer>();
                layer.transform.SetParent(rig.transform, false);

                rig.Preview(new Vector3(2f, 0f, 0f), withWind: false);
                Assert.Greater(layer.transform.localScale.x, 50f);

                profile.SetMode(ParallaxMode.Simulated2D);
                rig.Preview(new Vector3(2f, 0f, 0f), withWind: false);
                Assert.AreEqual(Vector3.one, layer.transform.localScale);
            }
            finally
            {
                Object.DestroyImmediate(rig.gameObject);
                Object.DestroyImmediate(profile);
            }
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
