using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class StageTests
    {
        private Texture2D _texture;
        private Sprite _forest;
        private Sprite _cave;
        private Sprite _seam;
        private ParallaxProfile _profile;
        private ParallaxRig _rig;
        private Camera _camera;
        private ParallaxLayer _layer;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(64, 16);
            _forest = Sprite.Create(_texture, new Rect(0, 0, 64, 16), new Vector2(0.5f, 0.5f), 4f);
            _cave = Sprite.Create(_texture, new Rect(0, 0, 64, 16), new Vector2(0.5f, 0.5f), 4f);
            _seam = Sprite.Create(_texture, new Rect(0, 0, 64, 16), new Vector2(0.5f, 0.5f), 4f);
            _forest.name = "floresta";
            _cave.name = "caverna";
            _seam.name = "costura";

            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _profile.AddLayer(new ParallaxLayerSettings("Chão", new Vector2(0f, 0f), 0f));
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
            image.sprite = _forest;
            _layer.AddStage(new ParallaxStage(20f, _cave, _seam));
        }

        [TearDown]
        public void TearDown()
        {
            _rig.ResetPreview();
            Object.DestroyImmediate(_rig.gameObject);
            Object.DestroyImmediate(_camera.gameObject);
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_forest);
            Object.DestroyImmediate(_cave);
            Object.DestroyImmediate(_seam);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void StageDependsOnTheCameraPosition()
        {
            Assert.AreEqual(-1, ParallaxStageTracker.StageAt(_layer.Stages, 5f));
            Assert.AreEqual(0, ParallaxStageTracker.StageAt(_layer.Stages, 25f));
        }

        [Test]
        public void BlocksThatShowUpAfterTheStartUseTheNewArtWithTheSeamFirst()
        {
            _rig.Preview(Vector3.zero, withWind: false);
            Assert.That(SpriteNames(), Has.All.EqualTo("floresta"));

            // Andando para a direita, cada bloco novo aparece já depois do começo da caverna.
            for (float x = 0f; x <= 60f; x += 2f)
                _rig.Preview(new Vector3(x, 0f, 0f), withWind: false);

            var names = SpriteNames();
            Assert.That(names, Has.Some.EqualTo("caverna"));
            Assert.That(names, Has.None.EqualTo("floresta"));
        }

        [Test]
        public void GoingBackShowsTheBlocksAsTheyWere()
        {
            for (float x = 0f; x <= 60f; x += 2f)
                _rig.Preview(new Vector3(x, 0f, 0f), withWind: false);
            for (float x = 60f; x >= 0f; x -= 2f)
                _rig.Preview(new Vector3(x, 0f, 0f), withWind: false);

            Assert.That(SpriteNames(), Has.All.EqualTo("floresta"));
        }

        [Test]
        public void FadingLayerStartingInsideAStageShowsItWithoutFading()
        {
            // Chegando por uma passagem já depois do começo do trecho, a camada não pode esmaecer da arte antiga.
            _layer.FadeBetweenStages = true;
            _rig.Preview(new Vector3(60f, 0f, 0f), withWind: false);

            foreach (var image in _layer.GetComponentsInChildren<SpriteRenderer>())
            {
                Assert.AreEqual("caverna", image.sprite.name);
                Assert.AreEqual(1f, image.color.a, 0.001f);
            }
        }

        [Test]
        public void StagesOutOfOrderUseTheLatestStartAlreadyPassed()
        {
            var stages = new System.Collections.Generic.List<ParallaxStage>
            {
                new ParallaxStage(40f, _cave),
                new ParallaxStage(20f, _seam),
            };

            Assert.AreEqual(-1, ParallaxStageTracker.StageAt(stages, 10f));
            Assert.AreEqual(1, ParallaxStageTracker.StageAt(stages, 30f));
            Assert.AreEqual(0, ParallaxStageTracker.StageAt(stages, 50f));
        }

        [Test]
        public void FadeComesBackWhenTheCameraReturnsMidway()
        {
            var image = _layer.GetComponentInChildren<SpriteRenderer>();
            var tracker = new ParallaxStageTracker();
            tracker.SetTiles(new System.Collections.Generic.List<(SpriteRenderer, int)> { (image, 0) });
            tracker.Reset();

            tracker.UpdateFade(_layer.Stages, 0f, 0f, 1f);
            tracker.UpdateFade(_layer.Stages, 30f, 0.2f, 1f);
            Assert.Less(tracker.Alpha, 1f, "começou a esmaecer ao passar do trecho");

            tracker.UpdateFade(_layer.Stages, 0f, 0.2f, 1f);
            Assert.AreEqual(1f, tracker.Alpha, 0.001f, "voltou a aparecer");
            Assert.AreEqual("floresta", image.sprite.name, "continua no trecho em que a câmera está");
        }

        [Test]
        public void ResetPreviewPutsTheOriginalArtBack()
        {
            for (float x = 0f; x <= 60f; x += 2f)
                _rig.Preview(new Vector3(x, 0f, 0f), withWind: false);

            _rig.ResetPreview();

            Assert.AreEqual("floresta", _layer.GetComponentInChildren<SpriteRenderer>().sprite.name);
        }

        [Test]
        public void SceneLinkHandsTheOffsetOnceAndThenClears()
        {
            var world = new GameObject("Mundo").AddComponent<ParallaxWorld>();
            try
            {
                world.SpeedMultiplier = 1.5f;
                ParallaxSceneLink.Save(new Vector3(12f, 0f, 0f), world);

                Assert.IsTrue(ParallaxSceneLink.TryTake(out var offset, out float speed, out _));
                Assert.AreEqual(12f, offset.x);
                Assert.AreEqual(1.5f, speed);
                Assert.IsFalse(ParallaxSceneLink.TryTake(out _, out _, out _));
            }
            finally
            {
                ParallaxSceneLink.Clear();
                Object.DestroyImmediate(world.gameObject);
            }
        }

        private string[] SpriteNames()
        {
            var renderers = _layer.GetComponentsInChildren<SpriteRenderer>(true);
            var names = new string[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                names[i] = renderers[i].sprite.name;
            return names;
        }
    }
}
