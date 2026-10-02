using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxProfileTests
    {
        private ParallaxProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _profile.SetFocusDistance(10f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_profile);
        }

        [Test]
        public void DepthZeroIsTheFocusPlane()
        {
            Assert.AreEqual(0f, _profile.DepthToFactor(0f), 0.0001f);
        }

        [Test]
        public void DepthToFactorAndBackReturnsTheSameDepth()
        {
            foreach (var depth in new[] { -4f, 5f, 20f, 40f })
                Assert.AreEqual(depth, _profile.FactorToDepth(_profile.DepthToFactor(depth)), 0.001f);
        }

        [Test]
        public void FarLayersApproachOneAndNearLayersAreNegative()
        {
            Assert.Greater(_profile.DepthToFactor(1000f), 0.98f);
            Assert.Less(_profile.DepthToFactor(-4f), 0f);
        }

        [Test]
        public void MoveLayerSwapsOrder()
        {
            _profile.AddLayer(new ParallaxLayerSettings("A", Vector2.zero, 0f));
            _profile.AddLayer(new ParallaxLayerSettings("B", Vector2.zero, 0f));
            _profile.AddLayer(new ParallaxLayerSettings("C", Vector2.zero, 0f));

            _profile.MoveLayer(0, 2);

            Assert.AreEqual("B", _profile.Layers[0].Name);
            Assert.AreEqual("C", _profile.Layers[1].Name);
            Assert.AreEqual("A", _profile.Layers[2].Name);
        }

        [Test]
        public void RemoveLayerAtIgnoresInvalidIndex()
        {
            _profile.AddLayer(new ParallaxLayerSettings("A", Vector2.zero, 0f));

            _profile.RemoveLayerAt(5);
            _profile.RemoveLayerAt(-1);

            Assert.AreEqual(1, _profile.Layers.Count);
        }
    }
}
