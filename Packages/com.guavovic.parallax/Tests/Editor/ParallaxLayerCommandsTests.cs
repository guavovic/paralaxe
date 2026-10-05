using System.Collections.Generic;
using Guavovic.Parallax.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxLayerCommandsTests
    {
        private readonly List<Sprite> _sprites = new List<Sprite>();
        private Texture2D _texture;
        private ParallaxProfile _profile;
        private ParallaxRig _rig;
        private SerializedObject _profileObject;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(16, 16);
            for (int i = 0; i < 4; i++)
            {
                var sprite = Sprite.Create(_texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f));
                sprite.name = "Camada " + i;
                _sprites.Add(sprite);
            }

            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            _rig.Profile = _profile;
            _profileObject = new SerializedObject(_profile);
            ParallaxLayerCommands.AddSprites(_rig, _profileObject, _sprites, spread: true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rig.gameObject);
            Object.DestroyImmediate(_profile);
            foreach (var sprite in _sprites)
                Object.DestroyImmediate(sprite);
            _sprites.Clear();
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void ImagesSpreadFromFarToNearAndDrawBackToFront()
        {
            Assert.AreEqual(4, _rig.Layers.Count);
            Assert.AreEqual(0.9f, _profile.Layers[0].Factor.x, 0.001f);
            Assert.AreEqual(0.1f, _profile.Layers[3].Factor.x, 0.001f);

            var orders = DrawOrders();
            for (int i = 1; i < orders.Length; i++)
                Assert.Greater(orders[i], orders[i - 1]);
        }

        [Test]
        public void DroppedImageUsesTheDefaultsAndGoesInFront()
        {
            _profile.Layers[3].SetBlur(3f);
            int front = DrawOrders()[3];

            ParallaxLayerCommands.AddSprites(_rig, _profileObject, new[] { _sprites[0] }, spread: false);

            var added = _profile.Layers[4];
            Assert.AreEqual(0f, added.Blur);
            Assert.AreEqual(0.5f, added.Factor.x, 0.001f);
            Assert.AreEqual(front + 1, DrawOrders()[4]);
        }

        [Test]
        public void DraggingToAFarSlotKeepsEveryObjectOnItsOwnSettings()
        {
            var before = ParallaxLayerList.FindLayerObjects(_rig, 4);
            _profileObject.Update();
            _profileObject.FindProperty("layers").MoveArrayElement(0, 3);
            _profileObject.ApplyModifiedProperties();

            ParallaxLayerCommands.Move(_rig, before, 0, 3);

            AssertEveryObjectMatchesItsSettings();
            var orders = DrawOrders();
            for (int i = 1; i < orders.Length; i++)
                Assert.Greater(orders[i], orders[i - 1]);
        }

        [Test]
        public void DraggingKeepsAHandTunedDrawOrder()
        {
            var before = ParallaxLayerList.FindLayerObjects(_rig, 4);
            before[1].GetComponentInChildren<SpriteRenderer>().sortingOrder = 10;

            ParallaxLayerCommands.Move(_rig, before, 0, 3);

            Assert.AreEqual(10, before[1].GetComponentInChildren<SpriteRenderer>().sortingOrder);
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

        [Test]
        public void DistanceWritesFactorAndDepthTogether()
        {
            _profileObject.Update();
            var layer = _profileObject.FindProperty("layers").GetArrayElementAtIndex(1);

            ParallaxDistance.Set(_profile, layer, 0.75f);
            _profileObject.ApplyModifiedProperties();
            _profile.SetMode(ParallaxMode.Perspective);
            _profileObject.Update();

            Assert.AreEqual(0.75f, _profile.Layers[1].Factor.x, 0.001f);
            Assert.AreEqual(0.75f, ParallaxDistance.Get(_profile, ParallaxMode.Perspective, _profileObject.FindProperty("layers").GetArrayElementAtIndex(1)), 0.01f);
        }

        private int[] DrawOrders()
        {
            var objects = ParallaxLayerList.FindLayerObjects(_rig, _profile.Layers.Count);
            var orders = new int[objects.Length];
            for (int i = 0; i < objects.Length; i++)
                orders[i] = objects[i].GetComponentInChildren<SpriteRenderer>().sortingOrder;
            return orders;
        }

        private void AssertEveryObjectMatchesItsSettings()
        {
            foreach (var layer in _rig.GetComponentsInChildren<ParallaxLayer>(true))
                Assert.IsTrue(layer.name.EndsWith(_profile.Layers[layer.SettingsIndex].Name), layer.name);
        }
    }
}
