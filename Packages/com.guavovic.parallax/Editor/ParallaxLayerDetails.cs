using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Painel da camada selecionada: distância no topo, o resto em seções recolhidas.
    /// </summary>
    internal sealed class ParallaxLayerDetails
    {
        private bool _effectsOpen = true;
        private bool _motionOpen;
        private bool _advancedOpen;

        /// <summary>
        /// Devolve true quando o usuário pediu para remover a camada.
        /// </summary>
        public bool Draw(ParallaxRig rig, SerializedProperty layer, ParallaxLayer layerObject)
        {
            var name = layer.FindPropertyRelative("name");
            name.stringValue = EditorGUILayout.TextField(name.stringValue, EditorStyles.boldLabel);

            var preview = GUILayoutUtility.GetRect(10f, 90f, GUILayout.ExpandWidth(true));
            ParallaxLayerList.DrawThumbnail(preview, layerObject);
            EditorGUILayout.Space(4f);

            DrawDistance(rig.Profile, layer);
            EditorGUILayout.Space(6f);

            _effectsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_effectsOpen, "Efeitos");
            if (_effectsOpen)
            {
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("blur"), new GUIContent("Desfoque"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("windInfluence"), new GUIContent("Vento"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("tint"), new GUIContent("Cor"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _motionOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_motionOpen, "Movimento");
            if (_motionOpen)
            {
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("autoScroll"), new GUIContent("Rolagem sozinha"));
                EditorGUILayout.PropertyField(layer.FindPropertyRelative("loopHorizontally"), new GUIContent("Repetir na horizontal"));
            }
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

        private static void DrawDistance(ParallaxProfile profile, SerializedProperty layer)
        {
            EditorGUI.BeginChangeCheck();
            float distance = EditorGUILayout.Slider(
                new GUIContent("Distância", "Quanto a camada acompanha a câmera. Negativo fica na frente do herói."),
                ParallaxDistance.Get(profile, layer), ParallaxDistance.Near, ParallaxDistance.Far);
            if (EditorGUI.EndChangeCheck())
                ParallaxDistance.Set(profile, layer, distance);

            var labels = GUILayoutUtility.GetRect(10f, 14f, GUILayout.ExpandWidth(true));
            labels.xMin += EditorGUIUtility.labelWidth;
            labels.xMax -= 55f;
            var style = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            GUI.Label(labels, "perto", style);
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(labels, "foco", style);
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(labels, "longe", style);
        }

        private static void DrawAdvanced(SerializedProperty layer, ParallaxLayer layerObject)
        {
            EditorGUILayout.PropertyField(layer.FindPropertyRelative("factor"), new GUIContent("Fator (X, Y)", "Modo 2D. O Y diz quanto a camada acompanha a câmera na vertical."));
            EditorGUILayout.PropertyField(layer.FindPropertyRelative("depth"), new GUIContent("Profundidade", "Modo perspectiva, em unidades a partir do plano de foco."));

            if (layerObject == null)
                return;

            var renderers = layerObject.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
                return;

            EditorGUI.BeginChangeCheck();
            int order = EditorGUILayout.IntField(new GUIContent("Ordem de desenho", "Quem tem o número maior aparece na frente."), renderers[0].sortingOrder);
            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObjects(renderers, "Ordem de desenho");
            foreach (var spriteRenderer in renderers)
                spriteRenderer.sortingOrder = order;
        }
    }
}
