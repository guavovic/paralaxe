using System.Collections.Generic;
using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Raiz do parallax. Lê o profile, encontra as camadas filhas e as posiciona a cada quadro.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ParallaxRig : MonoBehaviour
    {
        [SerializeField] private ParallaxProfile profile;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool applyProfileToWorld = true;
        [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

        private ParallaxWorld _world;
        private IParallaxSolver _solver;
        private ParallaxMode _solverMode;
        private Vector3 _cameraOrigin;
        private bool _initialized;
        private bool _previewing;

        public ParallaxProfile Profile { get => profile; set => profile = value; }
        public Camera TargetCamera { get => targetCamera; set => targetCamera = value; }
        public IReadOnlyList<ParallaxLayer> Layers => layers;
        public ParallaxWorld World => _world;

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (!_initialized)
                Initialize();

            if (!_initialized)
                return;

            Apply();
        }

        public void CollectLayers()
        {
            layers.Clear();
            GetComponentsInChildren(true, layers);
        }

        public void Initialize()
        {
            if (profile == null)
                return;

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                return;

            _world = ParallaxWorld.Current;
            if (_world == null)
                _world = gameObject.AddComponent<ParallaxWorld>();

            if (applyProfileToWorld)
                _world.ApplyProfile(profile);

            if (layers.Count == 0)
                CollectLayers();

            var all = profile.Layers;
            foreach (var layer in layers)
            {
                layer.Initialize(createCopies: true);
                if (layer.SettingsIndex >= 0 && layer.SettingsIndex < all.Count)
                    layer.ApplyTint(all[layer.SettingsIndex].Tint);
            }

            _cameraOrigin = targetCamera.transform.position;
            _initialized = true;
        }

        public void Apply()
        {
            if (profile == null || targetCamera == null)
                return;

            if (_solver == null || _solverMode != profile.Mode)
            {
                _solverMode = profile.Mode;
                _solver = ParallaxSolvers.Create(profile.Mode);
            }

            var context = new ParallaxContext(targetCamera.transform.position, _cameraOrigin, _world.SpeedMultiplier, profile.FocusDistance, Time.time);
            var all = profile.Layers;

            foreach (var layer in layers)
            {
                if (layer == null || layer.SettingsIndex < 0 || layer.SettingsIndex >= all.Count)
                    continue;

                var settings = all[layer.SettingsIndex];
                _solver.Solve(layer, settings, context);
                layer.ApplyWind(_world.WindStrength * settings.WindInfluence, _world.WindSpeed);
                layer.ApplyBlur(settings.Blur);
            }
        }

        /// <summary>
        /// Simula um deslocamento da câmera sem mover a câmera. Serve para o preview do editor.
        /// </summary>
        public void Preview(Vector3 cameraOffset, bool withWind)
        {
            if (profile == null)
                return;

            if (!_previewing)
            {
                if (layers.Count == 0)
                    CollectLayers();

                foreach (var layer in layers)
                {
                    if (layer == null)
                        continue;

                    layer.Initialize(createCopies: false);
                    if (layer.SettingsIndex >= 0 && layer.SettingsIndex < profile.Layers.Count)
                        layer.ApplyTint(profile.Layers[layer.SettingsIndex].Tint);
                }

                _previewing = true;
            }

            var cameraOrigin = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
            var context = new ParallaxContext(cameraOrigin + cameraOffset, cameraOrigin, profile.SpeedMultiplier, profile.FocusDistance);
            var solver = ParallaxSolvers.Create(profile.Mode);
            var all = profile.Layers;

            foreach (var layer in layers)
            {
                if (layer == null || layer.SettingsIndex < 0 || layer.SettingsIndex >= all.Count)
                    continue;

                var settings = all[layer.SettingsIndex];
                solver.Solve(layer, settings, context);
                layer.ApplyWind(withWind ? profile.WindStrength * settings.WindInfluence : 0f, profile.WindSpeed);
                layer.ApplyBlur(settings.Blur);
            }
        }

        public void ResetPreview()
        {
            if (!_previewing)
                return;

            foreach (var layer in layers)
            {
                if (layer != null)
                    layer.Restore();
            }

            _previewing = false;
        }
    }
}
