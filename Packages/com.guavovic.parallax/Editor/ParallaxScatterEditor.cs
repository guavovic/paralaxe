using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Espalha de novo assim que um campo muda, para ver o resultado na cena enquanto ajusta.
    /// </summary>
    [CustomEditor(typeof(ParallaxScatter))]
    public sealed class ParallaxScatterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            bool changed = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();
            if (GUILayout.Button("Espalhar de novo") || changed)
            {
                foreach (var scatter in targets)
                    Rebuild((ParallaxScatter)scatter);
            }
        }

        private static void Rebuild(ParallaxScatter scatter)
        {
            Undo.RegisterFullObjectHierarchyUndo(scatter.gameObject, "Espalhar de novo");
            scatter.Rebuild();
            EditorUtility.SetDirty(scatter.gameObject);
        }
    }
}
