using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Seção "Trechos" do painel da camada: em que ponto da fase a arte muda e para qual imagem.
    /// </summary>
    internal static class ParallaxStagesSection
    {
        private static readonly GUIContent StartLabel = new GUIContent("Começa em X", "Posição X da câmera a partir da qual os blocos novos usam esta imagem.");
        private static readonly GUIContent SpriteLabel = new GUIContent("Imagem");
        private static readonly GUIContent TransitionLabel = new GUIContent("Transição", "Bloco de costura com o trecho anterior. Opcional.");
        private static readonly GUIContent FadeLabel = new GUIContent("Esmaecer a camada inteira", "Para camadas muito distantes, que quase não andam e não trocariam de bloco.");

        public static void Draw(ParallaxLayer layerObject)
        {
            if (layerObject == null)
                return;

            using (var serialized = new SerializedObject(layerObject))
            {
                serialized.Update();
                var stages = serialized.FindProperty("stages");
                EditorGUILayout.LabelField("A arte desta camada muda conforme a câmera avança. Antes do primeiro trecho vale a imagem atual.", EditorStyles.wordWrappedMiniLabel);

                int remove = -1;
                for (int i = 0; i < stages.arraySize; i++)
                {
                    var stage = stages.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Trecho " + (i + 1), EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("×", GUILayout.Width(22f)))
                        remove = i;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(stage.FindPropertyRelative("startX"), StartLabel);
                    EditorGUILayout.PropertyField(stage.FindPropertyRelative("sprite"), SpriteLabel);
                    EditorGUILayout.PropertyField(stage.FindPropertyRelative("transition"), TransitionLabel);
                    EditorGUILayout.EndVertical();
                }

                if (remove >= 0)
                    stages.DeleteArrayElementAtIndex(remove);

                if (GUILayout.Button("+ Trecho"))
                {
                    stages.InsertArrayElementAtIndex(stages.arraySize);
                    var added = stages.GetArrayElementAtIndex(stages.arraySize - 1);
                    if (stages.arraySize == 1)
                    {
                        added.FindPropertyRelative("startX").floatValue = 20f;
                        added.FindPropertyRelative("sprite").objectReferenceValue = null;
                        added.FindPropertyRelative("transition").objectReferenceValue = null;
                    }
                    else
                    {
                        added.FindPropertyRelative("startX").floatValue += 20f;
                    }
                }

                EditorGUILayout.PropertyField(serialized.FindProperty("fadeBetweenStages"), FadeLabel);
                serialized.ApplyModifiedProperties();
            }
        }
    }
}
