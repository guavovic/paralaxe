using Guavovic.Parallax.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxLayerCommandsTests
    {
        private ParallaxProfile _profile;
        private ParallaxRig _rig;
        private SerializedObject _profileObject;

        [SetUp]
        public void SetUp()
        {
            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            _rig.Profile = _profile;
            _profileObject = new SerializedObject(_profile);

            for (int i = 0; i < 4; i++)
                ParallaxLayerCommands.Add(_rig, _profileObject);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rig.gameObject);
            Object.DestroyImmediate(_profile);
        }

        [Test]
        public void AddedLayerUsesTheDefaultsInsteadOfCopyingTheLastLayer()
        {
            _profile.Layers[3].SetBlur(3f);
            _profile.Layers[3].SetAutoScroll(new Vector2(0.5f, 0f));

            ParallaxLayerCommands.Add(_rig, _profileObject);

            var added = _profile.Layers[4];
            Assert.AreEqual(0f, added.Blur);
            Assert.AreEqual(Vector2.zero, added.AutoScroll);
            Assert.AreEqual(new Vector2(0.5f, 0f), added.Factor);
            Assert.AreEqual(5, _rig.Layers.Count);
        }

        [Test]
        public void MovingToAFarSlotKeepsEveryObjectOnItsOwnSettings()
        {
            _profileObject.Update();
            ParallaxLayerCommands.Move(_rig, _profileObject, _profileObject.FindProperty("layers"), 0, 3);

            AssertEveryObjectMatchesItsSettings();
            Assert.AreEqual("Camada 0", _profile.Layers[3].Name);
        }

        [Test]
        public void RemovingDestroysEveryObjectThatUsesTheIndex()
        {
            ParallaxLayerCommands.CreateLayerObject(_rig, 1, "Camada 1");
            ParallaxLayerCommands.RegisterLayers(_rig);

            _profileObject.Update();
            ParallaxLayerCommands.Remove(_rig, _profileObject, _profileObject.FindProperty("layers"), 1);

            Assert.AreEqual(3, _profile.Layers.Count);
            Assert.AreEqual(3, _rig.GetComponentsInChildren<ParallaxLayer>(true).Length);
            AssertEveryObjectMatchesItsSettings();
        }

        private void AssertEveryObjectMatchesItsSettings()
        {
            foreach (var layer in _rig.GetComponentsInChildren<ParallaxLayer>(true))
                Assert.AreEqual(layer.name, _profile.Layers[layer.SettingsIndex].Name);
        }
    }
}
