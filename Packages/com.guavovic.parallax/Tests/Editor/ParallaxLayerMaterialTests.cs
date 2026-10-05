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
