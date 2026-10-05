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
            if (_rig != null && (_profileObject == null || _profileObject.targetObject != _rig.Profile))
                SetRig(_rig);

            DrawToolbar();

            if (_rig == null || _profileObject == null)
            {
                DrawEmptyState();
                return;
            }

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
                    _preview.Refresh();

                var gear = EditorGUIUtility.IconContent("_Popup");
                gear.tooltip = "Mundo: velocidade, vento e foco";
                var gearRect = GUILayoutUtility.GetRect(gear, EditorStyles.toolbarButton, GUILayout.Width(28f));
                if (GUI.Button(gearRect, gear, EditorStyles.toolbarButton))
                    PopupWindow.Show(gearRect, new ParallaxWorldPopup(_profileObject));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawListColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Mathf.Max(220f, position.width * 0.45f)));
            EditorGUILayout.LabelField("longe ↑", EditorStyles.centeredGreyMiniLabel);

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            _list.Draw();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField("perto ↓", EditorStyles.centeredGreyMiniLabel);
            var sprites = DropZone("+ Arraste imagens aqui", 34f);
            if (sprites.Count > 0)
            {
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
                var objects = ParallaxLayerList.FindLayerObjects(_rig, layers.arraySize);
                if (_details.Draw(_rig, layers.GetArrayElementAtIndex(selected), objects[selected]))
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

            var sprites = DropZone("Arraste as imagens aqui", 120f);
            EditorGUILayout.LabelField("Ou selecione um ParallaxRig na cena.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();

            if (sprites.Count > 0)
            {
                CreateRig(sprites);
                GUIUtility.ExitGUI();
            }
        }

        private void CreateRig(List<Sprite> sprites)
        {
            string path = EditorUtility.SaveFilePanelInProject("Salvar o ParallaxProfile", "ParallaxProfile", "asset", "Onde salvar o profile do parallax.");
            if (string.IsNullOrEmpty(path))
                return;

            var rig = ParallaxLayerCommands.CreateRig("Parallax", _newMode, path);
            var profileObject = new SerializedObject(rig.Profile);
            ParallaxLayerCommands.AddSprites(rig, profileObject, sprites, spread: true);
            AssetDatabase.SaveAssetIfDirty(rig.Profile);

            Selection.activeGameObject = rig.gameObject;
            SetRig(rig);
        }

        /// <summary>
        /// Área que aceita sprites e texturas arrastados do Project. Devolve as imagens soltas nela neste evento.
        /// </summary>
        private static List<Sprite> DropZone(string label, float height)
        {
            var result = new List<Sprite>();
            var rect = GUILayoutUtility.GetRect(10f, height, GUILayout.ExpandWidth(true));
            GUI.Box(rect, label, EditorStyles.helpBox);

            var current = Event.current;
            if (!rect.Contains(current.mousePosition) || (current.type != EventType.DragUpdated && current.type != EventType.DragPerform))
                return result;

            foreach (var item in DragAndDrop.objectReferences)
            {
                var sprite = item as Sprite;
                if (sprite == null && item is Texture2D)
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(item));
                if (sprite != null)
                    result.Add(sprite);
            }

            DragAndDrop.visualMode = result.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (current.type != EventType.DragPerform || result.Count == 0)
            {
                result.Clear();
                return result;
            }

            DragAndDrop.AcceptDrag();
            current.Use();
            return result;
        }
    }
}
