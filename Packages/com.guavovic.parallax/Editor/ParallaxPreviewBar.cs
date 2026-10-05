using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Mostra o parallax sem entrar em Play: a câmera se desloca de mentira, pelo slider ou sozinha.
    /// </summary>
    internal sealed class ParallaxPreviewBar
    {
        private const float Range = 30f;

        private ParallaxRig _rig;
        private float _offset;
        private bool _animating;
        private double _animationStart;

        private double _lastTick;

        public bool Active => _rig != null && _rig.IsPreviewing;

        public void SetRig(ParallaxRig rig)
        {
            Stop();
            _rig = rig;
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Preview", EditorStyles.miniBoldLabel, GUILayout.Width(52f));

            using (new EditorGUI.DisabledScope(_animating))
            {
                EditorGUI.BeginChangeCheck();
                _offset = GUILayout.HorizontalSlider(_offset, -Range, Range, GUILayout.MinWidth(80f));
                if (EditorGUI.EndChangeCheck())
                    Apply();
            }

            bool animate = GUILayout.Toggle(_animating, _animating ? "■ Parar" : "▶ Animar", EditorStyles.toolbarButton, GUILayout.Width(70f));
            if (animate != _animating)
                SetAnimating(animate);

            using (new EditorGUI.DisabledScope(!Active))
            {
                if (GUILayout.Button("↺ Voltar", EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    Stop();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Refaz o preview com os valores atuais, depois de uma edição.
        /// </summary>
        public void Refresh()
        {
            if (Active)
                Apply();
        }

        public void Stop()
        {
            SetAnimating(false);
            _offset = 0f;
            if (_rig != null)
                _rig.ResetPreview();

            SceneView.RepaintAll();
        }

        private void SetAnimating(bool value)
        {
            if (value == _animating)
                return;

            _animating = value;
            if (value)
            {
                _animationStart = EditorApplication.timeSinceStartup;
                EditorApplication.update += Tick;
            }
            else
            {
                EditorApplication.update -= Tick;
            }
        }

        private void Tick()
        {
            if (_rig == null)
            {
                SetAnimating(false);
                return;
            }

            // O update do editor roda bem mais que 60 vezes por segundo; mais que isso não muda nada na tela.
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastTick < 1.0 / 60.0)
                return;

            _lastTick = now;
            float time = (float)(now - _animationStart);
            _offset = Mathf.Sin(time * 0.35f) * Range;
            Apply();
        }

        private void Apply()
        {
            if (_rig == null || EditorApplication.isPlaying)
                return;

            _rig.Preview(new Vector3(_offset, 0f, 0f), withWind: _animating);
            SceneView.RepaintAll();
        }
    }
}
