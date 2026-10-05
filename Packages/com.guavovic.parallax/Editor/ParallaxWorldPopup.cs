using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Valores que valem para o parallax todo e quase nunca mudam: velocidade, vento e foco.
    /// </summary>
    internal sealed class ParallaxWorldPopup : PopupWindowContent
    {
        private readonly SerializedObject _profileObject;

        public ParallaxWorldPopup(SerializedObject profileObject)
        {
            _profileObject = profileObject;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(320f, 190f);
        }

        public override void OnGUI(Rect rect)
        {
            _profileObject.Update();
            EditorGUILayout.LabelField("Mundo", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_profileObject.FindProperty("speedMultiplier"), new GUIContent("Velocidade"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windDirection"), new GUIContent("Direção do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windStrength"), new GUIContent("Força do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("windSpeed"), new GUIContent("Ritmo do vento"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("gustDecay"), new GUIContent("Rajada some em"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("maxGust"), new GUIContent("Rajada máxima"));

            if ((ParallaxMode)_profileObject.FindProperty("mode").enumValueIndex == ParallaxMode.Perspective)
                EditorGUILayout.PropertyField(_profileObject.FindProperty("focusDistance"), new GUIContent("Distância de foco"));

            _profileObject.ApplyModifiedProperties();
        }
    }
}
