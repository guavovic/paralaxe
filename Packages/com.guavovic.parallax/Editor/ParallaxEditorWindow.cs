using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    public sealed class ParallaxEditorWindow : EditorWindow
    {
        private static readonly string[] ModeNames = { "2D", "Perspectiva" };

        private ParallaxRig _rig;
        private SerializedObject _profileObject;
        private ParallaxLayerList _list;
        private readonly ParallaxLayerDetails _details = new ParallaxLayerDetails();
        private readonly ParallaxPreviewBar _preview = new ParallaxPreviewBar();
        private ParallaxMode _newMode = ParallaxMode.Simulated2D;
        private Vector2 _listScroll;
        private Vector2 _detailsScroll;

        [MenuItem("Window/Parallax/Editor")]
        [MenuItem("Tools/Parallax/Editor")]
        public static void Open()
        {
            GetWindow<ParallaxEditorWindow>("Parallax");
        }

        [MenuItem("GameObject/Parallax/Novo parallax", false, 10)]
        [MenuItem("Window/Parallax/Novo parallax")]
        [MenuItem("Tools/Parallax/Novo parallax")]
        public static void OpenNew()
        {
            GetWindow<ParallaxEditorWindow>("Parallax").SetRig(null);
        }

        public static void Open(ParallaxRig rig)
        {
            GetWindow<ParallaxEditorWindow>("Parallax").SetRig(rig);
        }

        private void OnEnable()
        {
            minSize = new Vector2(480f, 320f);
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
            _preview.Stop();
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
                _preview.Stop();
        }

        private void SetRig(ParallaxRig rig)
        {
            _preview.Stop();
            _rig = rig;
            _profileObject = rig != null && rig.Profile != null ? new SerializedObject(rig.Profile) : null;
            _list = _profileObject != null ? new ParallaxLayerList(rig, _profileObject) : null;
            _preview.SetRig(rig);
            Repaint();
        }

        private void OnGUI()
        {
            if (_rig != null && _rig.Profile != null && (_profileObject == null || _profileObject.targetObject != _rig.Profile))
                SetRig(_rig);

            DrawToolbar();

            if (_rig == null)
            {
                DrawEmptyState();
                return;
            }

            if (_rig.Profile == null || _profileObject == null)
            {
                DrawMissingProfile();
                return;
            }

            DrawCameraWarning();
            _profileObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawListColumn();
            DrawDetailsColumn();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                _profileObject.ApplyModifiedProperties();
                _preview.Refresh();
            }

            _preview.Draw();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            var rig = (ParallaxRig)EditorGUILayout.ObjectField(_rig, typeof(ParallaxRig), true, GUILayout.MinWidth(120f), GUILayout.MaxWidth(240f));
            if (EditorGUI.EndChangeCheck())
                SetRig(rig);

            GUILayout.FlexibleSpace();

            if (_profileObject != null)
            {
                _profileObject.Update();
                var mode = _profileObject.FindProperty("mode");
                mode.enumValueIndex = GUILayout.Toolbar(mode.enumValueIndex, ModeNames, EditorStyles.toolbarButton, GUILayout.Width(150f));
                if (_profileObject.ApplyModifiedProperties())
                {
                    MatchCamera();
                    _preview.Refresh();
                }

                var gear = EditorGUIUtility.IconContent("_Popup");
                gear.tooltip = "Mundo: velocidade, vento e foco";
                var gearRect = GUILayoutUtility.GetRect(gear, EditorStyles.toolbarButton, GUILayout.Width(28f));
                if (GUI.Button(gearRect, gear, EditorStyles.toolbarButton))
                    PopupWindow.Show(gearRect, new ParallaxWorldPopup(_profileObject, OnWorldChanged));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnWorldChanged()
        {
            _preview.Refresh();
            Repaint();
        }

        private Camera RigCamera => _rig.TargetCamera != null ? _rig.TargetCamera : Camera.main;

        private void DrawCameraWarning()
        {
            if (ParallaxCamera.Matches(RigCamera, _rig.Profile.Mode))
                return;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(_rig.Profile.Mode == ParallaxMode.Perspective
                ? "O modo Perspectiva precisa de câmera em perspectiva."
                : "O modo 2D precisa de câmera ortográfica.", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("Ajustar câmera", GUILayout.Width(110f)))
                MatchCamera();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Deixa a câmera do rig no tipo que o modo pede, mostrando a mesma área no plano de foco.
        /// </summary>
        private void MatchCamera()
        {
            var camera = RigCamera;
            if (camera == null || ParallaxCamera.Matches(camera, _rig.Profile.Mode))
                return;

            Undo.RecordObject(camera, "Ajustar câmera ao modo");
            ParallaxCamera.Match(camera, _rig.Profile.Mode, _rig.Profile.FocusDistance);
            SceneView.RepaintAll();
        }

        private void DrawListColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Mathf.Max(220f, position.width * 0.45f)));
            EditorGUILayout.LabelField("atrás ↑", EditorStyles.centeredGreyMiniLabel);

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            _list.Draw();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("na frente ↓", EditorStyles.centeredGreyMiniLabel);
            var sprites = ParallaxDropZone.Draw("+ Arraste imagens aqui", 34f);
            if (sprites.Count > 0)
            {
                _preview.Stop();
                ParallaxLayerCommands.AddSprites(_rig, _profileObject, sprites, spread: false);
                _list.Selected = _rig.Profile.Layers.Count - 1;
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsColumn()
        {
            EditorGUILayout.BeginVertical();
            _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);

            var layers = _profileObject.FindProperty("layers");
            int selected = _list.Selected;
            if (selected < 0 || selected >= layers.arraySize)
            {
                EditorGUILayout.HelpBox(layers.arraySize == 0
                    ? "Arraste as imagens do cenário para a lista, da mais distante para a mais próxima."
                    : "Clique numa camada para ver os detalhes.", MessageType.None);
            }
            else
            {
                if (_details.Draw(_rig, layers.GetArrayElementAtIndex(selected), _list.ObjectAt(selected)))
                {
                    _preview.Stop();
                    ParallaxLayerCommands.Remove(_rig, _profileObject, layers, selected);
                    _list.Selected = Mathf.Min(selected, layers.arraySize - 1);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEmptyState()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Novo parallax", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Arraste as imagens do cenário, da mais distante para a mais próxima. As distâncias já saem espalhadas.", EditorStyles.wordWrappedLabel);
            _newMode = (ParallaxMode)GUILayout.Toolbar((int)_newMode, ModeNames);
            EditorGUILayout.Space(6f);

            var sprites = ParallaxDropZone.Draw("Arraste as imagens aqui", 120f);
            EditorGUILayout.LabelField("Ou selecione um ParallaxRig na cena.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            if (sprites.Count > 0)
            {
                CreateRig(sprites);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawMissingProfile()
        {
            EditorGUILayout.HelpBox("Este rig não tem um ParallaxProfile.", MessageType.Warning);
            if (!GUILayout.Button("Criar profile"))
                return;

            string path = AskProfilePath();
            if (string.IsNullOrEmpty(path))
                return;

            var profile = CreateInstance<ParallaxProfile>();
            AssetDatabase.CreateAsset(profile, path);
            Undo.RecordObject(_rig, "Criar profile");
            _rig.Profile = profile;
            EditorUtility.SetDirty(_rig);
            SetRig(_rig);
            GUIUtility.ExitGUI();
        }

        private static string AskProfilePath()
        {
            // Sugere um nome livre, para não substituir o profile de outro rig sem querer.
            string suggested = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath("Assets/ParallaxProfile.asset"));
            return EditorUtility.SaveFilePanelInProject("Salvar o ParallaxProfile", suggested, "asset", "Onde salvar o profile do parallax.");
        }

        private void CreateRig(List<Sprite> sprites)
        {
            string path = AskProfilePath();
            if (string.IsNullOrEmpty(path))
                return;

            var rig = ParallaxLayerCommands.CreateRig("Parallax", _newMode, path);
            var profileObject = new SerializedObject(rig.Profile);
            ParallaxLayerCommands.AddSprites(rig, profileObject, sprites, spread: true);
            AssetDatabase.SaveAssetIfDirty(rig.Profile);

            Selection.activeGameObject = rig.gameObject;
            SetRig(rig);
        }
    }
}
