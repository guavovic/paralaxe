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

            InitializeLayers(createCopies: true);
            _cameraOrigin = targetCamera.transform.position;
            _initialized = true;
        }

        public void Apply()
        {
            if (profile == null || targetCamera == null)
                return;

            var context = new ParallaxContext(targetCamera.transform.position, _cameraOrigin, _world.SpeedMultiplier, profile.FocusDistance, Time.time);
            SolveLayers(context, _world.WindStrength, _world.WindSpeed);
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
                InitializeLayers(createCopies: false);
                _previewing = true;
            }

            var cameraOrigin = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
            var context = new ParallaxContext(cameraOrigin + cameraOffset, cameraOrigin, profile.SpeedMultiplier, profile.FocusDistance);
            SolveLayers(context, withWind ? profile.WindStrength : 0f, profile.WindSpeed);
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

        private void InitializeLayers(bool createCopies)
        {
            // Camada apagada na cena deixa uma referência nula; tira só ela e mantém a lista que o usuário montou.
            layers.RemoveAll(layer => layer == null);
            if (layers.Count == 0)
                CollectLayers();

            foreach (var layer in layers)
            {
                layer.Initialize(createCopies);
                if (TryGetSettings(layer, out var settings))
                    layer.ApplyTint(settings.Tint);
            }
        }

        private void SolveLayers(in ParallaxContext context, float windStrength, float windSpeed)
        {
            if (_solver == null || _solverMode != profile.Mode)
            {
                _solverMode = profile.Mode;
                _solver = ParallaxSolvers.Create(profile.Mode);
            }

            foreach (var layer in layers)
            {
                if (!TryGetSettings(layer, out var settings))
                    continue;

                _solver.Solve(layer, settings, context);
                layer.ApplyMaterialProperties(windStrength * settings.WindInfluence, windSpeed, settings.Blur);
            }
        }

        private bool TryGetSettings(ParallaxLayer layer, out ParallaxLayerSettings settings)
        {
            var all = profile.Layers;
            bool valid = layer != null && layer.SettingsIndex >= 0 && layer.SettingsIndex < all.Count;
            settings = valid ? all[layer.SettingsIndex] : null;
            return valid;
        }
    }
}
