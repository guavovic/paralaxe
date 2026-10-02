using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    [CustomEditor(typeof(ParallaxRig))]
    public sealed class ParallaxRigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Abrir editor de parallax"))
                ParallaxEditorWindow.Open((ParallaxRig)target);
        }
    }
}
