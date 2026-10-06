using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Alças na cena para arrastar as bordas da área da câmera.
    /// </summary>
    [CustomEditor(typeof(ParallaxCameraBounds))]
    public sealed class ParallaxCameraBoundsEditor : UnityEditor.Editor
    {
        private static readonly Color EdgeColor = ParallaxCameraBounds.GizmoColor;
        private static readonly Color FillColor = new Color(EdgeColor.r, EdgeColor.g, EdgeColor.b, 0.04f);

        private void OnSceneGUI()
        {
            var bounds = (ParallaxCameraBounds)target;
            var area = bounds.Area;
            float z = 0f;

            Handles.color = EdgeColor;
            Handles.DrawSolidRectangleWithOutline(new Rect(area.x, area.y, area.width, area.height), FillColor, EdgeColor);

            EditorGUI.BeginChangeCheck();
            float xMin = Edge(new Vector3(area.xMin, area.center.y, z), Vector3.right).x;
            float xMax = Edge(new Vector3(area.xMax, area.center.y, z), Vector3.right).x;
            float yMin = Edge(new Vector3(area.center.x, area.yMin, z), Vector3.up).y;
            float yMax = Edge(new Vector3(area.center.x, area.yMax, z), Vector3.up).y;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bounds, "Mover limite da câmera");
                bounds.Area = Rect.MinMaxRect(Mathf.Min(xMin, xMax - 1f), Mathf.Min(yMin, yMax - 1f), xMax, yMax);
                EditorUtility.SetDirty(bounds);
            }

            Handles.Label(new Vector3(area.xMin, area.yMax + 0.4f, z), "Limite da câmera");
        }

        private static Vector3 Edge(Vector3 position, Vector3 direction)
        {
            float size = HandleUtility.GetHandleSize(position) * 0.12f;
            return Handles.Slider(position, direction, size, Handles.DotHandleCap, 0f);
        }
    }
}
