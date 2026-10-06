using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// O mapa da fase: até onde a câmera vai, se o fundo continua da cena anterior e as passagens entre cenas.
    /// </summary>
    internal sealed class ParallaxMapPopup : PopupWindowContent
    {
        private readonly ParallaxRig _rig;
        private Vector2 _scroll;

        public ParallaxMapPopup(ParallaxRig rig)
        {
            _rig = rig;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(380f, 460f);
        }

        public override void OnGUI(Rect rect)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawBounds();
            EditorGUILayout.Space(8f);
            DrawContinue();
            EditorGUILayout.Space(8f);
            DrawPortals();
            EditorGUILayout.Space(8f);
            DrawSpawnPoints();
            EditorGUILayout.EndScrollView();
        }

        private Camera RigCamera => _rig.TargetCamera != null ? _rig.TargetCamera : Camera.main;

        private void DrawBounds()
        {
            EditorGUILayout.LabelField("Limites da câmera", EditorStyles.boldLabel);
            var camera = RigCamera;
            if (camera == null)
            {
                EditorGUILayout.HelpBox("O rig não tem câmera.", MessageType.None);
                return;
            }

            var bounds = camera.GetComponent<ParallaxCameraBounds>();
            if (bounds == null)
            {
                EditorGUILayout.LabelField("A câmera mostra tudo. Com limites, ela para nas bordas do mapa.", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Adicionar limites"))
                {
                    bounds = Undo.AddComponent<ParallaxCameraBounds>(camera.gameObject);
                    var center = camera.transform.position;
                    bounds.Area = new Rect(center.x - 30f, center.y - 6f, 60f, 12f);
                }
                return;
            }

            using (var serialized = new SerializedObject(bounds))
            {
                serialized.Update();
                EditorGUILayout.PropertyField(serialized.FindProperty("area"), new GUIContent("Área (x, y, largura, altura)"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serialized.FindProperty("limitX"), new GUIContent("Na horizontal"));
                EditorGUILayout.PropertyField(serialized.FindProperty("limitY"), new GUIContent("Na vertical"));
                EditorGUILayout.EndHorizontal();
                serialized.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Ajustar com as alças na cena"))
            {
                Selection.activeGameObject = camera.gameObject;
                SceneView.lastActiveSceneView?.Frame(new Bounds(bounds.Area.center, bounds.Area.size), false);
            }
        }

        private void DrawContinue()
        {
            EditorGUILayout.LabelField("Entre cenas", EditorStyles.boldLabel);
            using (var serialized = new SerializedObject(_rig))
            {
                serialized.Update();
                EditorGUILayout.PropertyField(serialized.FindProperty("continueFromPreviousScene"),
                    new GUIContent("Continuar o fundo da cena anterior", "Quem chega por uma passagem vê o fundo continuar de onde a outra cena parou."));
                serialized.ApplyModifiedProperties();
            }
        }

        private void DrawPortals()
        {
            EditorGUILayout.LabelField("Passagens", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quem entra vai para outra cena, no ponto de chegada com o mesmo id.", EditorStyles.wordWrappedMiniLabel);
            foreach (var portal in Object.FindObjectsByType<ParallaxScenePortal>(FindObjectsSortMode.InstanceID))
            {
                using (var serialized = new SerializedObject(portal))
                {
                    serialized.Update();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    Header(portal.gameObject);

                    var path = serialized.FindProperty("scenePath");
                    var current = AssetDatabase.LoadAssetAtPath<SceneAsset>(path.stringValue);
                    var chosen = (SceneAsset)EditorGUILayout.ObjectField("Cena", current, typeof(SceneAsset), false);
                    if (chosen != current)
                    {
                        path.stringValue = chosen != null ? AssetDatabase.GetAssetPath(chosen) : "";
                        serialized.FindProperty("sceneName").stringValue = chosen != null ? chosen.name : "";
                    }

                    EditorGUILayout.PropertyField(serialized.FindProperty("spawnId"), new GUIContent("Ponto de chegada"));
                    serialized.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                }
            }

            if (GUILayout.Button("+ Passagem"))
            {
                var go = Create("Passagem");
                var area = go.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(1f, 3f);
                go.AddComponent<ParallaxScenePortal>();
            }
        }

        private void DrawSpawnPoints()
        {
            EditorGUILayout.LabelField("Pontos de chegada", EditorStyles.boldLabel);
            foreach (var spawn in Object.FindObjectsByType<ParallaxSpawnPoint>(FindObjectsSortMode.InstanceID))
            {
                using (var serialized = new SerializedObject(spawn))
                {
                    serialized.Update();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    Header(spawn.gameObject);
                    EditorGUILayout.PropertyField(serialized.FindProperty("id"), new GUIContent("Id"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("walkDirection"), new GUIContent("Entra andando para", "1 direita, -1 esquerda."));
                    serialized.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                }
            }

            if (GUILayout.Button("+ Ponto de chegada"))
                Create("Chegada").AddComponent<ParallaxSpawnPoint>();
        }

        private static void Header(GameObject go)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(go.name + "  (x " + go.transform.position.x.ToString("0.#") + ")", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("Selecionar", EditorStyles.miniButton, GUILayout.Width(70f)))
            {
                Selection.activeGameObject = go;
                SceneView.lastActiveSceneView?.FrameSelected();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Objeto novo no meio da vista da cena, na altura do chão do rig.</summary>
        private GameObject Create(string name)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Adicionar " + name.ToLowerInvariant());
            var view = SceneView.lastActiveSceneView;
            float x = view != null ? view.pivot.x : 0f;
            go.transform.position = new Vector3(x, _rig.transform.position.y - 2f, 0f);
            Selection.activeGameObject = go;
            return go;
        }
    }
}
