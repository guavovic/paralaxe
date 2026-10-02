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

            foreach (var layer in layers)
                layer.Initialize(createCopies: true);

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
                _solver = profile.Mode == ParallaxMode.Perspective
                    ? (IParallaxSolver)new PerspectiveSolver()
                    : new Simulated2DSolver();
            }

            var context = new ParallaxContext(targetCamera.transform.position, _cameraOrigin, _world.SpeedMultiplier, profile.FocusDistance);
            var all = profile.Layers;

            foreach (var layer in layers)
            {
                if (layer == null || layer.SettingsIndex < 0 || layer.SettingsIndex >= all.Count)
                    continue;

                var settings = all[layer.SettingsIndex];
                _solver.Solve(layer, settings, context);
                layer.ApplyWind(_world.WindStrength * settings.WindInfluence, _world.WindSpeed);
            }
        }
    }
}
