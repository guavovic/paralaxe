using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    public sealed class ParallaxEditorWindow : EditorWindow
    {
        private static readonly string[] ModeNames = { "2D", "Perspectiva" };
        private static readonly string[] StyleNames = BuildStyleNames();
        private static GUIContent _gear;

        private ParallaxRig _rig;
        private SerializedObject _profileObject;
        private ParallaxLayerList _list;
        private readonly ParallaxLayerDetails _details = new ParallaxLayerDetails();
        private readonly ParallaxPreviewBar _preview = new ParallaxPreviewBar();
        private ParallaxMode _newMode = ParallaxMode.Simulated2D;
        private int _newStyle;
        private Vector2 _listScroll;
        private Vector2 _detailsScroll;

        [MenuItem("Window/Parallax/Editor")]
        [MenuItem("Tools/Parallax/Editor")]
        public static void Open()
        {
            GetWindow<ParallaxEditorWindow>();
        }

        [MenuItem("GameObject/Parallax/Novo parallax", false, 10)]
        [MenuItem("Window/Parallax/Novo parallax")]
        [MenuItem("Tools/Parallax/Novo parallax")]
        public static void OpenNew()
        {
            GetWindow<ParallaxEditorWindow>().SetRig(null);
        }

        public static void Open(ParallaxRig rig)
        {
            GetWindow<ParallaxEditorWindow>().SetRig(rig);
        }

        private void OnEnable()
        {
            minSize = new Vector2(480f, 320f);
            titleContent = new GUIContent("Parallax", AssetDatabase.LoadAssetAtPath<Texture2D>(ParallaxRigSetup.IconFolder + "ParallaxRig.png"));
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

            if (_rig.Profile == null)
            {
                DrawMissingProfile();
                return;
            }

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
                var current = _rig.Mode;
                var chosen = (ParallaxMode)GUILayout.Toolbar((int)current, ModeNames, EditorStyles.toolbarButton, GUILayout.Width(150f));
                if (chosen != current)
                    SetMode(chosen);

                var presetsRect = GUILayoutUtility.GetRect(new GUIContent("Presets"), EditorStyles.toolbarDropDown, GUILayout.Width(70f));
                if (GUI.Button(presetsRect, "Presets", EditorStyles.toolbarDropDown))
                    ParallaxPresets.ShowMenu(presetsRect, _rig, OnWorldChanged);

                _gear ??= new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(ParallaxRigSetup.IconFolder + "ParallaxWorld.png"), "Mundo: velocidade, vento e foco");
                var gearRect = GUILayoutUtility.GetRect(_gear, EditorStyles.toolbarButton, GUILayout.Width(28f));
                if (GUI.Button(gearRect, _gear, EditorStyles.toolbarButton))
                    PopupWindow.Show(gearRect, new ParallaxWorldPopup(_profileObject, _rig, OnWorldChanged));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnWorldChanged()
        {
            _preview.Refresh();
            Repaint();
        }

        private Camera RigCamera => _rig.TargetCamera != null ? _rig.TargetCamera : Camera.main;

        /// <summary>
        /// O modo vem da câmera, então trocar o modo troca a câmera (mantendo a área no plano de foco).
        /// O profile acompanha, para valer quando o rig não tiver câmera.
        /// </summary>
        private void SetMode(ParallaxMode mode)
        {
            var camera = RigCamera;
            Undo.RecordObjects(camera != null ? new Object[] { camera, _rig.Profile } : new Object[] { _rig.Profile }, "Trocar modo");
            if (camera != null)
                ParallaxCamera.Match(camera, mode, _rig.Profile.FocusDistance);
            _rig.Profile.SetMode(mode);
            EditorUtility.SetDirty(_rig.Profile);
            _profileObject.Update();
            _preview.Refresh();
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
                if (_details.Draw(_rig, layers.GetArrayElementAtIndex(selected), _list.ObjectAt(selected), _list.SpriteAt(selected)))
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
            _newStyle = EditorGUILayout.Popup("Estilo", _newStyle, StyleNames);
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

            if (ParallaxRigSetup.CreateProfileFor(_rig, RigCamera))
                SetRig(_rig);
            GUIUtility.ExitGUI();
        }

        private static string[] BuildStyleNames()
        {
            var names = new List<string> { "Distâncias espalhadas" };
            names.AddRange(ParallaxPresets.BuiltInNames);
            return names.ToArray();
        }

        private void CreateRig(List<Sprite> sprites)
        {
            var rig = ParallaxRigSetup.CreateFromSprites(sprites, _newMode, _newStyle == 0 ? null : StyleNames[_newStyle]);
            if (rig == null)
                return;

            Selection.activeGameObject = rig.gameObject;
            SetRig(rig);
        }
    }
}
