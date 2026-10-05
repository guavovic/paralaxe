using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    public sealed class ParallaxEditorWindow : EditorWindow
    {
        private ParallaxRig _rig;
        private SerializedObject _profileObject;
        private Vector2 _scroll;
        private Vector2 _previewOffset;
        private bool _previewWind;
        private bool _previewActive;

        [MenuItem("Window/Parallax/Editor")]
        [MenuItem("Tools/Parallax/Editor")]
        public static void Open()
        {
            GetWindow<ParallaxEditorWindow>("Parallax");
        }

        public static void Open(ParallaxRig rig)
        {
            var window = GetWindow<ParallaxEditorWindow>("Parallax");
            window.SetRig(rig);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Undo.undoRedoPerformed += Repaint;
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Undo.undoRedoPerformed -= Repaint;
            StopPreview();
        }

        private void OnSelectionChanged()
        {
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<ParallaxRig>()
                : null;

            if (selected != null && selected != _rig)
                SetRig(selected);

            Repaint();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                StopPreview();
        }

        private void SetRig(ParallaxRig rig)
        {
            StopPreview();
            _rig = rig;
            _profileObject = rig != null && rig.Profile != null ? new SerializedObject(rig.Profile) : null;
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            var rig = (ParallaxRig)EditorGUILayout.ObjectField("Rig", _rig, typeof(ParallaxRig), true);
            if (EditorGUI.EndChangeCheck())
                SetRig(rig);

            if (_rig == null)
            {
                EditorGUILayout.HelpBox("Selecione um ParallaxRig na cena ou crie um em GameObject > Parallax > Novo parallax.", MessageType.Info);
                return;
            }

            if (_rig.Profile == null)
            {
                EditorGUILayout.HelpBox("O rig não tem um ParallaxProfile.", MessageType.Warning);
                return;
            }

            if (_profileObject == null || _profileObject.targetObject != _rig.Profile)
                _profileObject = new SerializedObject(_rig.Profile);

            _profileObject.Update();
            EditorGUI.BeginChangeCheck();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawGlobals();
            EditorGUILayout.Space();
            DrawLayers();
            EditorGUILayout.Space();
            DrawPreview();
            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                _profileObject.ApplyModifiedProperties();
                if (_previewActive)
                    ApplyPreview();
            }
        }

        private void DrawGlobals()
        {
            EditorGUILayout.LabelField("Valores globais", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_profileObject.FindProperty("mode"), new GUIContent("Modo"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("speedMultiplier"), new GUIContent("Velocidade"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windDirection"), new GUIContent("Direção do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windStrength"), new GUIContent("Força do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windSpeed"), new GUIContent("Velocidade do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("gustDecay"), new GUIContent("Perda da rajada"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("maxGust"), new GUIContent("Rajada máxima"));

            if (_rig.Profile.Mode == ParallaxMode.Perspective)
                EditorGUILayout.PropertyField(_profileObject.FindProperty("focusDistance"), new GUIContent("Distância de foco"));
        }

        private void DrawLayers()
        {
            EditorGUILayout.LabelField("Camadas (de trás para a frente)", EditorStyles.boldLabel);

            var layers = _profileObject.FindProperty("layers");
            var perspective = _rig.Profile.Mode == ParallaxMode.Perspective;
            int remove = -1;
            int moveFrom = -1;
            int moveTo = -1;

            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                var nameProperty = layer.FindPropertyRelative("name");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                nameProperty.stringValue = EditorGUILayout.TextField(nameProperty.stringValue);

                using (new EditorGUI.DisabledScope(i == 0))
                {
                    if (GUILayout.Button("↑", GUILayout.Width(26))) { moveFrom = i; moveTo = i - 1; }
                }

                using (new EditorGUI.DisabledScope(i == layers.arraySize - 1))
                {
                    if (GUILayout.Button("↓", GUILayout.Width(26))) { moveFrom = i; moveTo = i + 1; }
                }

                if (GUILayout.Button("×", GUILayout.Width(26)))
                    remove = i;

                EditorGUILayout.EndHorizontal();

                if (perspective)
                {
                    var depth = layer.FindPropertyRelative("depth");
                    EditorGUILayout.PropertyField(depth, new GUIContent("Profundidade"));
                    EditorGUILayout.LabelField("Fator equivalente", _rig.Profile.DepthToFactor(depth.floatValue).ToString("F2"));
                }
                else
                {
                    var factor = layer.FindPropertyRelative("factor");
                    EditorGUILayout.PropertyField(factor, new GUIContent("Fator (X, Y)"));
                    EditorGUILayout.LabelField("Profundidade equivalente", _rig.Profile.FactorToDepth(factor.vector2Value.x).ToString("F1"));
                }

                EditorGUILayout.PropertyField(layer.FindPropertyRelative("windInfluence"), new GUIContent("Influência do vento"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("blur"), new GUIContent("Desfoque"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("autoScroll"), new GUIContent("Rolagem automática"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("loopHorizontally"), new GUIContent("Repetir na horizontal"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("tint"), new GUIContent("Cor"));
                EditorGUILayout.EndVertical();
            }

            if (remove >= 0)
                RemoveLayer(layers, remove);
            else if (moveFrom >= 0)
                MoveLayer(layers, moveFrom, moveTo);

            if (GUILayout.Button("+ Camada"))
                AddLayer(layers);
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _previewOffset.x = EditorGUILayout.Slider("Câmera X", _previewOffset.x, -40f, 40f);
            _previewOffset.y = EditorGUILayout.Slider("Câmera Y", _previewOffset.y, -10f, 10f);
            _previewWind = EditorGUILayout.Toggle("Vento", _previewWind);
            if (EditorGUI.EndChangeCheck())
            {
                _previewActive = true;
                ApplyPreview();
            }

            using (new EditorGUI.DisabledScope(!_previewActive))
            {
                if (GUILayout.Button("Voltar ao normal"))
                {
                    _previewOffset = Vector2.zero;
                    StopPreview();
                }
            }
        }

        private void ApplyPreview()
        {
            if (_rig == null || EditorApplication.isPlaying)
                return;

            _rig.Preview(new Vector3(_previewOffset.x, _previewOffset.y, 0f), _previewWind);
            SceneView.RepaintAll();
        }

        private void StopPreview()
        {
            if (_rig != null && _previewActive)
                _rig.ResetPreview();

            _previewActive = false;
            SceneView.RepaintAll();
        }

        private void AddLayer(SerializedProperty layers)
        {
            StopPreview();
            ParallaxLayerCommands.Add(_rig, _profileObject);
        }

        private void RemoveLayer(SerializedProperty layers, int index)
        {
            StopPreview();
            ParallaxLayerCommands.Remove(_rig, _profileObject, layers, index);
        }

        private void MoveLayer(SerializedProperty layers, int from, int to)
        {
            StopPreview();
            ParallaxLayerCommands.Move(_rig, _profileObject, layers, from, to);
        }
    }
}
