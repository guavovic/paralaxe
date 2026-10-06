using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Painel da camada selecionada: distância no topo, o resto em seções recolhidas.
    /// </summary>
    internal sealed class ParallaxLayerDetails
    {
        private static readonly GUIContent BlurLabel = new GUIContent("Desfoque");
        private static readonly GUIContent WindLabel = new GUIContent("Vento");
        private static readonly GUIContent TintLabel = new GUIContent("Cor");
        private static readonly GUIContent ScrollLabel = new GUIContent("Rolagem sozinha");
        private static readonly GUIContent LoopLabel = new GUIContent("Repetir na horizontal");
        private static readonly GUIContent DistanceLabel = new GUIContent("Distância", "Quanto a camada acompanha a câmera. Negativo fica na frente do herói.");
        private static readonly GUIContent FactorLabel = new GUIContent("Fator (X, Y)", "Modo 2D. O Y diz quanto a camada acompanha a câmera na vertical.");
        private static readonly GUIContent DepthLabel = new GUIContent("Profundidade", "Modo perspectiva, em unidades a partir do plano de foco.");
        private static readonly GUIContent OrderLabel = new GUIContent("Ordem de desenho", "Quem tem o número maior aparece na frente.");
        private static GUIStyle _scaleLabel;

        private bool _effectsOpen = true;
        private bool _motionOpen;
        private bool _advancedOpen;
        private bool _stagesOpen;

        /// <summary>
        /// Devolve true quando o usuário pediu para remover a camada.
        /// </summary>
        public bool Draw(ParallaxRig rig, SerializedProperty layer, ParallaxLayer layerObject, Sprite sprite)
        {
            var name = layer.FindPropertyRelative("name");
            name.stringValue = EditorGUILayout.TextField(name.stringValue, EditorStyles.boldLabel);

            var preview = GUILayoutUtility.GetRect(10f, 90f, GUILayout.ExpandWidth(true));
            ParallaxLayerList.DrawThumbnail(preview, sprite);
            EditorGUILayout.Space(4f);

            DrawDistance(rig.Profile, rig.Mode, layer);
            EditorGUILayout.Space(6f);

            _effectsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_effectsOpen, "Efeitos");
            if (_effectsOpen)
            {
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("blur"), BlurLabel);
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("windInfluence"), WindLabel);
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("tint"), TintLabel);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _motionOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_motionOpen, "Movimento");
            if (_motionOpen)
            {
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("autoScroll"), ScrollLabel);
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("loopHorizontally"), LoopLabel);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _stagesOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_stagesOpen, "Trechos");
            if (_stagesOpen)
                ParallaxStagesSection.Draw(layerObject);
            EditorGUILayout.EndFoldoutHeaderGroup();

            _advancedOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_advancedOpen, "Avançado");
            if (_advancedOpen)
                DrawAdvanced(layer, layerObject);
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(layerObject == null))
            {
                if (GUILayout.Button("Selecionar na cena"))
                    Selection.activeGameObject = layerObject.gameObject;
            }

            bool remove = GUILayout.Button("Remover camada");
            EditorGUILayout.EndHorizontal();
            return remove;
        }

        private static void DrawDistance(ParallaxProfile profile, ParallaxMode mode, SerializedProperty layer)
        {
            EditorGUI.BeginChangeCheck();
            float distance = EditorGUILayout.Slider(
                DistanceLabel,
                ParallaxDistance.Get(profile, mode, layer), ParallaxDistance.Near, ParallaxDistance.Far);
            if (EditorGUI.EndChangeCheck())
                ParallaxDistance.Set(profile, layer, distance);

            var labels = GUILayoutUtility.GetRect(10f, 14f, GUILayout.ExpandWidth(true));
            labels.xMin += EditorGUIUtility.labelWidth;
            labels.xMax -= 55f;
            _scaleLabel ??= new GUIStyle(EditorStyles.miniLabel);
            _scaleLabel.alignment = TextAnchor.MiddleLeft;
            GUI.Label(labels, "perto", _scaleLabel);
            _scaleLabel.alignment = TextAnchor.MiddleCenter;
            GUI.Label(labels, "foco", _scaleLabel);
            _scaleLabel.alignment = TextAnchor.MiddleRight;
            GUI.Label(labels, "longe", _scaleLabel);
        }

        private static void DrawAdvanced(SerializedProperty layer, ParallaxLayer layerObject)
        {
            EditorGUILayout.PropertyField(layer.FindPropertyRelative("factor"), FactorLabel);
            EditorGUILayout.PropertyField(layer.FindPropertyRelative("depth"), DepthLabel);

            if (layerObject == null)
                return;

            var first = layerObject.GetComponentInChildren<SpriteRenderer>(true);
            if (first == null)
                return;

            EditorGUI.BeginChangeCheck();
            int order = EditorGUILayout.IntField(OrderLabel, first.sortingOrder);
            if (!EditorGUI.EndChangeCheck())
                return;

            var renderers = layerObject.GetComponentsInChildren<SpriteRenderer>(true);

            Undo.RecordObjects(renderers, "Ordem de desenho");
            foreach (var spriteRenderer in renderers)
                spriteRenderer.sortingOrder = order;
        }
    }
}
