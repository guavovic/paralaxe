using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Seção "Elementos espalhados" do painel da camada: grupos de sprites soltos pela camada, sem repetir a cada tela.
    /// </summary>
    internal static class ParallaxScatterSection
    {
        private static readonly GUIContent CountLabel = new GUIContent("Quantidade");
        private static readonly GUIContent SpanLabel = new GUIContent("Trecho", "Largura por onde os elementos se espalham. Quanto maior que a imagem da camada, menos repetição.");
        private static readonly GUIContent HeightLabel = new GUIContent("Altura (mín, máx)");
        private static readonly GUIContent ScaleLabel = new GUIContent("Tamanho (mín, máx)");
        private static readonly GUIContent FlipLabel = new GUIContent("Espelhar alguns");
        private static readonly GUIContent SeedLabel = new GUIContent("Sorteio", "Mude para outra arrumação com os mesmos valores.");
        private static readonly GUIContent MaterialLabel = new GUIContent("Material", "Vazio, usa o da camada (com o vento e o desfoque dela).");
        private static readonly GUIContent OrderLabel = new GUIContent("Ordem de desenho");
        private static readonly GUIContent WindLabel = new GUIContent("Vento", "Quanto o vento balança estes elementos. 0 deixa parado (pedra, construção).");
        private static readonly GUIContent ReactionLabel = new GUIContent("Reage ao herói", "Balança ou dá um pulinho quando o herói passa por eles na tela.");
        private static readonly GUIContent RadiusLabel = new GUIContent("Distância da reação");
        private static readonly GUIContent StrengthLabel = new GUIContent("Força da reação");
        private static readonly GUIContent CutLabel = new GUIContent("Pode cortar", "O golpe do herói corta estes elementos (mato, cipó). Chame ParallaxScatter.Cut no seu golpe.");
        private static readonly GUIContent RegrowLabel = new GUIContent("Volta em (s)", "0 não volta.");
        private static readonly GUIContent CutColorLabel = new GUIContent("Cor dos pedaços");
        private static readonly GUIContent RangeLabel = new GUIContent("Só entre X da câmera", "Para a fase que muda de cenário: os elementos só passam pela tela com a câmera neste trecho. Os dois iguais: sempre.");

        public static void Draw(ParallaxLayer layerObject)
        {
            if (layerObject == null)
                return;

            EditorGUILayout.LabelField("Sprites soltos pela camada, em posições sorteadas. Eles andam com a camada, mas não repetem com a imagem.", EditorStyles.wordWrappedMiniLabel);

            var scatters = layerObject.GetComponentsInChildren<ParallaxScatter>(true);
            ParallaxScatter removed = null;
            foreach (var scatter in scatters)
            {
                if (DrawScatter(scatter))
                    removed = scatter;
            }

            if (removed != null)
            {
                Undo.DestroyObjectImmediate(removed.gameObject);
                GUIUtility.ExitGUI();
            }

            var dropped = ParallaxDropZone.Draw("+ Arraste sprites para um grupo novo", 30f);
            if (dropped.Count > 0)
            {
                Create(layerObject, dropped.ToArray());
                GUIUtility.ExitGUI();
            }
        }

        private static bool DrawScatter(ParallaxScatter scatter)
        {
            bool remove = false;
            using (var serialized = new SerializedObject(scatter))
            {
                serialized.Update();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                scatter.gameObject.name = EditorGUILayout.DelayedTextField(scatter.gameObject.name, EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Selecionar", EditorStyles.miniButton, GUILayout.Width(70f)))
                    Selection.activeGameObject = scatter.gameObject;
                if (GUILayout.Button("×", GUILayout.Width(22f)))
                    remove = true;
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                DrawVariants(serialized.FindProperty("variants"));
                EditorGUILayout.PropertyField(serialized.FindProperty("count"), CountLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("span"), SpanLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("heightRange"), HeightLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("scaleRange"), ScaleLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("randomFlip"), FlipLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("seed"), SeedLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("material"), MaterialLabel);
                EditorGUILayout.PropertyField(serialized.FindProperty("sortingOrder"), OrderLabel);
                // Daqui para baixo nada muda a arrumação: não precisa espalhar de novo (nem encher o Undo).
                bool layoutChanged = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serialized.FindProperty("windInfluence"), WindLabel);
                bool windChanged = EditorGUI.EndChangeCheck();
                var reaction = serialized.FindProperty("reaction");
                EditorGUILayout.PropertyField(reaction, ReactionLabel);
                if (reaction.enumValueIndex != 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serialized.FindProperty("reactionRadius"), RadiusLabel);
                    EditorGUILayout.PropertyField(serialized.FindProperty("reactionStrength"), StrengthLabel);
                    EditorGUI.indentLevel--;
                }

                var cuttable = serialized.FindProperty("cuttable");
                EditorGUILayout.PropertyField(cuttable, CutLabel);
                if (cuttable.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serialized.FindProperty("regrowSeconds"), RegrowLabel);
                    EditorGUILayout.PropertyField(serialized.FindProperty("cutColor"), CutColorLabel);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(serialized.FindProperty("visibleRangeX"), RangeLabel);
                serialized.ApplyModifiedProperties();

                if (GUILayout.Button("Espalhar de novo", EditorStyles.miniButton) || layoutChanged)
                    Rebuild(scatter);
                else if (windChanged)
                    RefreshLayer(scatter);
                EditorGUILayout.EndVertical();
            }

            return remove;
        }

        // Lista à mão: o PropertyField de array abre um cabeçalho recolhível, e a seção já está dentro de um.
        private static void DrawVariants(SerializedProperty variants)
        {
            EditorGUILayout.LabelField("Sprites", EditorStyles.miniBoldLabel);
            int remove = -1;
            for (int i = 0; i < variants.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var element = variants.GetArrayElementAtIndex(i);
                element.objectReferenceValue = EditorGUILayout.ObjectField(element.objectReferenceValue, typeof(Sprite), false);
                if (GUILayout.Button("×", GUILayout.Width(22f)))
                    remove = i;
                EditorGUILayout.EndHorizontal();
            }

            if (remove >= 0)
            {
                variants.GetArrayElementAtIndex(remove).objectReferenceValue = null;
                variants.DeleteArrayElementAtIndex(remove);
            }

            foreach (var sprite in ParallaxDropZone.Draw("+ Arraste mais sprites", 22f))
            {
                variants.InsertArrayElementAtIndex(variants.arraySize);
                variants.GetArrayElementAtIndex(variants.arraySize - 1).objectReferenceValue = sprite;
            }
        }

        private static void Create(ParallaxLayer layerObject, Sprite[] sprites)
        {
            var go = new GameObject("Elementos");
            Undo.RegisterCreatedObjectUndo(go, "Adicionar elementos espalhados");
            go.transform.SetParent(layerObject.transform, false);
            go.transform.SetAsLastSibling();

            // Fica logo na frente da imagem da camada e no chão dela.
            var first = layerObject.GetComponentInChildren<SpriteRenderer>();
            int order = first != null ? first.sortingOrder + 1 : 0;
            float floor = first != null ? first.bounds.min.y - layerObject.transform.position.y + first.bounds.size.y * 0.2f : 0f;
            var scatter = go.AddComponent<ParallaxScatter>();
            scatter.Configure(sprites, 10, 60f, new Vector2(floor, floor), Vector2.one, Random.Range(1, 9999), order);
            Rebuild(scatter);
            Selection.activeGameObject = go;
        }

        // O vento de cada elemento é aplicado pela camada; ela precisa reaplicar quando o vento do grupo muda.
        private static void RefreshLayer(ParallaxScatter scatter)
        {
            var layer = scatter.GetComponentInParent<ParallaxLayer>();
            if (layer != null)
                layer.RefreshRenderers();
        }

        internal static void Rebuild(ParallaxScatter scatter)
        {
            Undo.RegisterFullObjectHierarchyUndo(scatter.gameObject, "Espalhar de novo");
            scatter.Rebuild();
            EditorUtility.SetDirty(scatter.gameObject);
        }
    }
}
