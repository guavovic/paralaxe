using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Guavovic.Parallax.Tests
{
    /// <summary>
    /// Trava o custo por quadro do rig: sem lixo nenhum e dentro de um orçamento de tempo folgado,
    /// que pega regressões grandes sem depender da máquina.
    /// </summary>
    public sealed class PerformanceTests
    {
        private GameObject _root;
        private ParallaxProfile _profile;
        private Material _material;
        private Texture2D _texture;
        private Sprite _sprite;
        private ParallaxRig _rig;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_material);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        [TestCase(15)]
        [TestCase(50)]
        [TestCase(200)]
        public void ApplyDoesNotAllocate(int layerCount)
        {
            Build(layerCount, ParallaxMode.Simulated2D);

            Assert.That(() => _rig.Apply(), Is.Not.AllocatingGCMemory());
        }

        [TestCase(15)]
        [TestCase(200)]
        public void ApplyWithChangingWindDoesNotAllocate(int layerCount)
        {
            Build(layerCount, ParallaxMode.Perspective);

            Assert.That(() =>
            {
                _rig.World.AddGust(0.01f);
                _rig.Apply();
            }, Is.Not.AllocatingGCMemory());
        }

        [TestCase(15)]
        [TestCase(50)]
        [TestCase(200)]
        public void ApplyStaysWithinBudget(int layerCount)
        {
            Build(layerCount, ParallaxMode.Simulated2D);
            const int frames = 500;

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < frames; i++)
            {
                _rig.World.AddGust(0.001f);
                _rig.Apply();
            }
            watch.Stop();

            // Medido em 05/10/2026: cerca de 0,0002 ms por camada. O orçamento dá margem de sobra.
            double perFrameMs = watch.Elapsed.TotalMilliseconds / frames;
            double budgetMs = 0.2 + layerCount * 0.005;
            Assert.Less(perFrameMs, budgetMs, $"{layerCount} camadas: {perFrameMs:F4} ms por quadro");
        }

        private void Build(int layerCount, ParallaxMode mode)
        {
            _root = new GameObject("Performance");
            _profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            _material = new Material(Shader.Find("Parallax/Wind Sprite"));
            _texture = new Texture2D(64, 16);
            _sprite = Sprite.Create(_texture, new Rect(0f, 0f, 64f, 16f), new Vector2(0.5f, 0.5f), 16f);

            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.transform.SetParent(_root.transform, false);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = mode == ParallaxMode.Simulated2D;

            _rig = new GameObject("Rig").AddComponent<ParallaxRig>();
            _rig.transform.SetParent(_root.transform, false);
            _rig.Profile = _profile;
            _rig.TargetCamera = camera;

            for (int i = 0; i < layerCount; i++)
            {
                float distance = Mathf.Lerp(0.9f, -0.5f, i / (float)layerCount);
                _profile.AddLayer(new ParallaxLayerSettings("Camada " + i, new Vector2(distance, 0.1f), _profile.FactorToDepth(distance)));

                var layer = new GameObject("Camada " + i).AddComponent<ParallaxLayer>();
                layer.transform.SetParent(_rig.transform, false);
                layer.SettingsIndex = i;

                var image = new GameObject("Imagem").AddComponent<SpriteRenderer>();
                image.transform.SetParent(layer.transform, false);
                image.sprite = _sprite;
                image.sharedMaterial = _material;
            }

            _rig.Initialize();
            _rig.Apply();
        }
    }
}
