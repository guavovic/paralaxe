using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ScrollTests
    {
        private Texture2D _texture;
        private Sprite _sprite;
        private ParallaxProfile _profile;
        private ParallaxRig _rig;
        private Camera _camera;
        private ParallaxLayer _layer;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(16, 16);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 1f);

            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            var settings = _profile.AddLayer(new ParallaxLayerSettings("Nuvens", Vector2.zero, 0f));
            settings.SetLoopHorizontally(false);
            settings.SetAutoScroll(new Vector2(2f, 0f));
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
            image.sprite = _sprite;

            _rig.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rig.gameObject);
            Object.DestroyImmediate(_camera.gameObject);
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        private float LayerX => _layer.transform.position.x;

        [Test]
        public void FirstFrameAfterInitializeStartsAtZero()
        {
            _rig.Apply(5f);

            Assert.AreEqual(0f, LayerX, 0.001f);
        }

        [Test]
        public void SpeedMultiplierOneKeepsTheAutoScrollSpeed()
        {
            _rig.Apply(1f);
            _rig.Apply(1f);
            _rig.Apply(1f);

            Assert.AreEqual(4f, LayerX, 0.001f);
        }

        [Test]
        public void ScrollFollowsTheSpeedMultiplier()
        {
            _rig.World.SpeedMultiplier = 0.25f;

            _rig.Apply(1f);
            _rig.Apply(1f);
            _rig.Apply(1f);

            Assert.AreEqual(1f, LayerX, 0.001f);
        }

        [Test]
        public void SpeedMultiplierZeroStopsAndResumesWithoutAJump()
        {
            _rig.Apply(1f);
            _rig.Apply(1f);
            Assert.AreEqual(2f, LayerX, 0.001f);

            _rig.World.SpeedMultiplier = 0f;
            _rig.Apply(1f);
            _rig.Apply(1f);
            _rig.Apply(1f);
            Assert.AreEqual(4f, LayerX, 0.001f, "parada");

            _rig.World.SpeedMultiplier = 1f;
            _rig.Apply(1f);
            Assert.AreEqual(4f, LayerX, 0.001f, "volta de onde parou");
            _rig.Apply(1f);
            Assert.AreEqual(6f, LayerX, 0.001f, "e segue na velocidade normal");
        }

        [Test]
        public void ApplyTwiceInTheSameFrameScrollsOnce()
        {
            _rig.Apply();
            _rig.Apply();
            float afterSecondCall = LayerX;

            _rig.Apply();

            Assert.AreEqual(afterSecondCall, LayerX, 0.001f);
        }

        [Test]
        public void ContinuingFromThePreviousSceneKeepsTheScroll()
        {
            var serialized = new UnityEditor.SerializedObject(_rig);
            serialized.FindProperty("continueFromPreviousScene").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                ParallaxSceneLink.Save(Vector3.zero, _rig.World, 3d);

                _rig.Initialize();

                Assert.AreEqual(new Vector2(6f, 0f), _layer.Scroll);
            }
            finally
            {
                ParallaxSceneLink.Clear();
            }
        }

        [Test]
        public void InitializeStartsTheScrollFromZeroAgain()
        {
            _rig.Apply(1f);
            _rig.Apply(1f);
            _rig.Apply(1f);
            Assert.AreEqual(new Vector2(6f, 0f), _layer.Scroll);

            _rig.Initialize();

            Assert.AreEqual(Vector2.zero, _layer.Scroll);
        }
    }
}
