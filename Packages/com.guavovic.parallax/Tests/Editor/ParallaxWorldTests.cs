using NUnit.Framework;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxWorldTests
    {
        private GameObject _object;
        private ParallaxWorld _world;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("World");
            _world = _object.AddComponent<ParallaxWorld>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
        }

        [Test]
        public void GustAddsToTheBaseWind()
        {
            _world.BaseWindStrength = 0.5f;
            _world.AddGust(1f);

            Assert.AreEqual(1.5f, _world.WindStrength, 0.001f);
        }

        [Test]
        public void GustIsCapped()
        {
            _world.AddGust(100f);

            Assert.AreEqual(2f, _world.Gust, 0.001f);
        }

        [Test]
        public void NegativeGustNeverGoesBelowZero()
        {
            _world.AddGust(-5f);

            Assert.AreEqual(0f, _world.Gust, 0.001f);
        }

        [Test]
        public void WindPointsAlongTheDirectionWithTheTotalStrength()
        {
            _world.WindDirection = new Vector2(0f, 3f);
            _world.BaseWindStrength = 2f;

            Assert.AreEqual(0f, _world.Wind.x, 0.001f);
            Assert.AreEqual(2f, _world.Wind.y, 0.001f);
        }

        [Test]
        public void ApplyProfileCopiesTheGlobalValues()
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            profile.SetSpeedMultiplier(0.25f);
            profile.SetWind(Vector2.left, 1.25f, 3f);

            _world.ApplyProfile(profile);

            Assert.AreEqual(0.25f, _world.SpeedMultiplier, 0.001f);
            Assert.AreEqual(1.25f, _world.BaseWindStrength, 0.001f);
            Assert.AreEqual(3f, _world.WindSpeed, 0.001f);
            Object.DestroyImmediate(profile);
        }
    }
}
