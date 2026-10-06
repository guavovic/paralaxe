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
        [Tooltip("Começa o fundo de onde a cena anterior parou, se ela chamou SaveForNextScene.")]
        [SerializeField] private bool continueFromPreviousScene;
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

        /// <summary>A câmera do rig, ou a principal se nenhuma foi escolhida.</summary>
        public Camera ResolvedCamera => targetCamera != null ? targetCamera : Camera.main;
        public bool IsPreviewing => _previewing;

        /// <summary>
        /// O modo vem da câmera: ortográfica usa 2D e em perspectiva usa Perspectiva.
        /// Sem câmera, vale o modo do profile.
        /// </summary>
        public ParallaxMode Mode
        {
            get
            {
                var fallback = profile != null ? profile.Mode : ParallaxMode.Simulated2D;
                return ParallaxCamera.ModeOf(ResolvedCamera, fallback);
            }
        }

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

            InitializeLayers(createCopies: true, temporaryCopies: false);
            _cameraOrigin = targetCamera.transform.position;
            // Sempre consome o que foi salvo: se esta cena não continua, o fundo da anterior não pode valer numa próxima.
            if (ParallaxSceneLink.TryTake(out var offset, out float speed, out float wind) && continueFromPreviousScene)
            {
                _cameraOrigin -= offset;
                _world.SpeedMultiplier = speed;
                _world.BaseWindStrength = wind;
            }
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
                InitializeLayers(createCopies: true, temporaryCopies: true);
                _previewing = true;
            }

            var cameraOrigin = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
            var context = new ParallaxContext(cameraOrigin, cameraOrigin, profile.SpeedMultiplier, profile.FocusDistance, 0f, cameraOffset);
            SolveLayers(context, withWind ? profile.WindStrength : 0f, profile.WindSpeed);
        }

        /// <summary>
        /// Guarda onde o fundo está, para o rig da próxima cena continuar dali.
        /// </summary>
        public void SaveForNextScene()
        {
            if (_initialized)
                ParallaxSceneLink.Save(targetCamera.transform.position - _cameraOrigin, _world);
        }

        public void ResetPreview()
        {
            if (!_previewing)
                return;

            foreach (var layer in layers)
            {
                if (layer == null)
                    continue;

                layer.RemoveTemporaryCopies();
                layer.Restore();
            }

            _previewing = false;
        }

        private void InitializeLayers(bool createCopies, bool temporaryCopies)
        {
            // Camada apagada na cena deixa uma referência nula; tira só ela e mantém a lista que o usuário montou.
            layers.RemoveAll(layer => layer == null);
            if (layers.Count == 0)
                CollectLayers();

            foreach (var layer in layers)
            {
                layer.Initialize(createCopies, temporaryCopies);
                if (TryGetSettings(layer, out var settings))
                    layer.ApplyTint(settings.Tint);
            }
        }

        private void SolveLayers(in ParallaxContext context, float windStrength, float windSpeed)
        {
            var mode = Mode;
            if (_solver == null || _solverMode != mode)
            {
                // O modo perspectiva amplia as camadas; ao trocar de modo, todas voltam à escala original.
                if (_solver != null)
                {
                    foreach (var layer in layers)
                    {
                        if (layer != null)
                            layer.transform.localScale = layer.BaseScale;
                    }
                }

                _solverMode = mode;
                _solver = ParallaxSolvers.Create(mode);
            }

            foreach (var layer in layers)
            {
                if (!TryGetSettings(layer, out var settings))
                    continue;

                _solver.Solve(layer, settings, context);
                layer.UpdateStages(context.CameraPosition.x + context.VirtualCameraOffset.x);
                layer.UpdateScatters(context.CameraPosition.x);
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
