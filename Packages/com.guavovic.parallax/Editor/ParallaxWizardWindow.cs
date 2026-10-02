using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    public sealed class ParallaxWizardWindow : EditorWindow
    {
        private string _rigName = "Parallax Rig";
        private ParallaxMode _mode = ParallaxMode.Simulated2D;
        private readonly List<Sprite> _sprites = new List<Sprite>();
        private Vector2 _scroll;

        [MenuItem("GameObject/Parallax/Novo parallax", false, 10)]
        [MenuItem("Window/Parallax/Novo parallax")]
        [MenuItem("Tools/Parallax/Novo parallax")]
        public static void Open()
        {
            GetWindow<ParallaxWizardWindow>("Novo parallax");
        }

        private void OnGUI()
        {
            _rigName = EditorGUILayout.TextField("Nome", _rigName);
            _mode = (ParallaxMode)EditorGUILayout.EnumPopup("Modo", _mode);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Imagens, de trás para a frente", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(260));
            int remove = -1;

            for (int i = 0; i < _sprites.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _sprites[i] = (Sprite)EditorGUILayout.ObjectField(_sprites[i], typeof(Sprite), false);
                if (GUILayout.Button("×", GUILayout.Width(26)))
                    remove = i;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (remove >= 0)
                _sprites.RemoveAt(remove);

            if (GUILayout.Button("+ Imagem"))
                _sprites.Add(null);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!CanCreate()))
            {
                if (GUILayout.Button("Criar"))
                    Create();
            }

            if (!CanCreate())
                EditorGUILayout.HelpBox("Adicione ao menos uma imagem.", MessageType.Info);
        }

        private bool CanCreate()
        {
            foreach (var sprite in _sprites)
            {
                if (sprite != null)
                    return true;
            }

            return false;
        }

        private void Create()
        {
            string path = EditorUtility.SaveFilePanelInProject("Salvar o ParallaxProfile", _rigName, "asset", "Onde salvar o profile do parallax.");
            if (string.IsNullOrEmpty(path))
                return;

            var chosen = _sprites.FindAll(sprite => sprite != null);
            var profile = CreateInstance<ParallaxProfile>();
            profile.SetMode(_mode);

            var camera = Camera.main;
            var rigObject = new GameObject(_rigName);
            Undo.RegisterCreatedObjectUndo(rigObject, "Novo parallax");
            var rig = rigObject.AddComponent<ParallaxRig>();
            rig.Profile = profile;
            rig.TargetCamera = camera;

            for (int i = 0; i < chosen.Count; i++)
            {
                // A primeira imagem é a mais distante. A distribuição vai de perto de 1 até perto de 0.
                float t = chosen.Count == 1 ? 0.5f : 1f - i / (float)(chosen.Count - 1);
                float factor = Mathf.Lerp(0.1f, 0.9f, t);
                float depth = profile.FactorToDepth(factor);

                profile.AddLayer(new ParallaxLayerSettings(chosen[i].name, new Vector2(factor, factor * 0.1f), depth));

                var layerObject = new GameObject("Camada " + i + " - " + chosen[i].name);
                layerObject.transform.SetParent(rigObject.transform, false);
                var layer = layerObject.AddComponent<ParallaxLayer>();
                layer.SettingsIndex = i;

                var imageObject = new GameObject(chosen[i].name);
                imageObject.transform.SetParent(layerObject.transform, false);
                var spriteRenderer = imageObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = chosen[i];
                spriteRenderer.sortingOrder = i - chosen.Count;
            }

            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            rig.CollectLayers();
            EditorUtility.SetDirty(rig);

            Selection.activeGameObject = rigObject;
            ParallaxEditorWindow.Open(rig);
            Close();
        }
    }
}
