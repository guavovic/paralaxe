using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxLayerMaterialTests
    {
        private static readonly int WindStrengthId = Shader.PropertyToID("_WindStrength");
        private static readonly int BlurId = Shader.PropertyToID("_Blur");

        private GameObject _object;
        private Material _material;
        private ParallaxLayer _layer;
        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _block;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("Layer");
            _layer = _object.AddComponent<ParallaxLayer>();
            _material = new Material(Shader.Find("Parallax/Wind Sprite"));
            _renderer = new GameObject("Image").AddComponent<SpriteRenderer>();
            _renderer.transform.SetParent(_object.transform, false);
            _renderer.sharedMaterial = _material;
            _block = new MaterialPropertyBlock();
            _layer.Initialize(createCopies: false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
            Object.DestroyImmediate(_material);
        }

        [Test]
        public void ScatterWithoutWindStaysStillWhileTheLayerSways()
        {
            var scatter = new GameObject("Pedras").AddComponent<ParallaxScatter>();
            scatter.transform.SetParent(_object.transform, false);
            scatter.WindInfluence = 0f;
            var stone = new GameObject("Pedra").AddComponent<SpriteRenderer>();
            stone.transform.SetParent(scatter.transform, false);
            stone.sharedMaterial = _material;
            _layer.Initialize(createCopies: false);

            _layer.ApplyMaterialProperties(0.8f, 1f, 0f);

            Assert.AreEqual(0.8f, Read(WindStrengthId), 0.001f);
            stone.GetPropertyBlock(_block);
            Assert.AreEqual(0f, _block.GetFloat(WindStrengthId), 0.001f);
        }

        [Test]
        public void StageWithoutWindStopsItsBlocks()
        {
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
            _renderer.sprite = sprite;
            _layer.AddStage(new ParallaxStage(10f, sprite, null, windInfluence: 0f));
            _layer.Initialize(createCopies: false);

            _layer.UpdateStages(5f);
            _layer.ApplyMaterialProperties(0.8f, 1f, 0f);
            Assert.AreEqual(0.8f, Read(WindStrengthId), 0.001f, "antes do trecho");

            _layer.ResetStages();
            _layer.UpdateStages(20f);
            _layer.ApplyMaterialProperties(0.8f, 1f, 0f);
            Assert.AreEqual(0f, Read(WindStrengthId), 0.001f, "dentro do trecho sem vento");

            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void FirstCallWritesWindAndBlur()
        {
            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);

            Assert.AreEqual(0.8f, Read(WindStrengthId), 0.001f);
            Assert.AreEqual(2f, Read(BlurId), 0.001f);
        }

        [Test]
        public void SameValuesAreNotWrittenAgain()
        {
            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);
            Write(WindStrengthId, 5f);

            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);

            Assert.AreEqual(5f, Read(WindStrengthId), 0.001f);
        }

        [Test]
        public void ChangedValueIsWritten()
        {
            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);
            _layer.ApplyMaterialProperties(1.2f, 1f, 2f);

            Assert.AreEqual(1.2f, Read(WindStrengthId), 0.001f);
        }

        [Test]
        public void PublicApplyWindForcesTheNextCallToWrite()
        {
            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);
            _layer.ApplyWind(0f, 1f);

            _layer.ApplyMaterialProperties(0.8f, 1f, 2f);

            Assert.AreEqual(0.8f, Read(WindStrengthId), 0.001f);
        }

        private float Read(int id)
        {
            _renderer.GetPropertyBlock(_block);
            return _block.GetFloat(id);
        }

        private void Write(int id, float value)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(id, value);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
