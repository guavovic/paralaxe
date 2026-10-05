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
            var camera = new GameObject("Camera").AddComponent<Camera>();
            try
            {
                profile.AddLayer(new ParallaxLayerSettings("Céu", new Vector2(0.99f, 1f), 990f));
                rig.Profile = profile;
                rig.TargetCamera = camera;
                var layer = new GameObject("Camada").AddComponent<ParallaxLayer>();
                layer.transform.SetParent(rig.transform, false);

                camera.orthographic = false;
                rig.Preview(new Vector3(2f, 0f, 0f), withWind: false);
                Assert.Greater(layer.transform.localScale.x, 50f);

                camera.orthographic = true;
                rig.Preview(new Vector3(2f, 0f, 0f), withWind: false);
                Assert.AreEqual(Vector3.one, layer.transform.localScale);
            }
            finally
            {
                Object.DestroyImmediate(rig.gameObject);
                Object.DestroyImmediate(camera.gameObject);
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ModeFollowsTheCameraEvenWhenTheProfileSaysOtherwise()
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            var rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            var camera = new GameObject("Camera").AddComponent<Camera>();
            try
            {
                profile.AddLayer(new ParallaxLayerSettings("Céu", new Vector2(0.99f, 1f), 990f));
                profile.SetMode(ParallaxMode.Perspective);
                camera.orthographic = true;
                rig.Profile = profile;
                rig.TargetCamera = camera;
                var layer = new GameObject("Camada").AddComponent<ParallaxLayer>();
                layer.transform.SetParent(rig.transform, false);

                Assert.AreEqual(ParallaxMode.Simulated2D, rig.Mode);
                rig.Preview(new Vector3(2f, 0f, 0f), withWind: false);
                Assert.AreEqual(Vector3.one, layer.transform.localScale);

                camera.orthographic = false;
                Assert.AreEqual(ParallaxMode.Perspective, rig.Mode);
            }
            finally
            {
                Object.DestroyImmediate(rig.gameObject);
                Object.DestroyImmediate(camera.gameObject);
                Object.DestroyImmediate(profile);
            }
        }

        [TestCase(14f, 0.1f)]
        [TestCase(-14f, 0.1f)]
        [TestCase(30f, 0.5f)]
        [TestCase(30f, 0.9f)]
        public void Preview2DKeepsTheLoopUnderTheRealCamera(float offset, float factor)
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            var rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            var camera = new GameObject("Camera").AddComponent<Camera>();
            var texture = new Texture2D(64, 16);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 16f), new Vector2(0.5f, 0.5f), 4f);
            try
            {
                camera.orthographic = true;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                profile.AddLayer(new ParallaxLayerSettings("Camada", new Vector2(factor, 0f), profile.FactorToDepth(factor)));
                rig.Profile = profile;
                rig.TargetCamera = camera;
                var layer = new GameObject("Camada").AddComponent<ParallaxLayer>();
                layer.transform.SetParent(rig.transform, false);
                var image = new GameObject("Imagem").AddComponent<SpriteRenderer>();
                image.transform.SetParent(layer.transform, false);
                image.sprite = sprite;

                rig.Preview(new Vector3(offset, 0f, 0f), withWind: false);

                // O centro da imagem fica a no máximo meio tile da câmera real, então as cópias cobrem a tela.
                float tile = 16f;
                Assert.LessOrEqual(Mathf.Abs(layer.transform.position.x - camera.transform.position.x), tile / 2f + 0.001f);
            }
            finally
            {
                rig.ResetPreview();
                Object.DestroyImmediate(rig.gameObject);
                Object.DestroyImmediate(camera.gameObject);
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
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
