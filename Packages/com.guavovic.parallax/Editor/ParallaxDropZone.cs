using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    internal static class ParallaxDropZone
    {
        /// <summary>
        /// Área que aceita sprites e texturas arrastados do Project. Devolve as imagens soltas nela neste evento.
        /// </summary>
        public static List<Sprite> Draw(string label, float height)
        {
            var result = new List<Sprite>();
            var rect = GUILayoutUtility.GetRect(10f, height, GUILayout.ExpandWidth(true));
            GUI.Box(rect, label, EditorStyles.helpBox);

            var current = Event.current;
            if (!rect.Contains(current.mousePosition) || (current.type != EventType.DragUpdated && current.type != EventType.DragPerform))
                return result;

            foreach (var item in DragAndDrop.objectReferences)
            {
                var sprite = item as Sprite;
                if (sprite == null && item is Texture2D)
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(item));
                if (sprite != null)
                    result.Add(sprite);
            }

            DragAndDrop.visualMode = result.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (current.type != EventType.DragPerform || result.Count == 0)
            {
                result.Clear();
                return result;
            }

            DragAndDrop.AcceptDrag();
            current.Use();
            return result;
        }
    }
}
