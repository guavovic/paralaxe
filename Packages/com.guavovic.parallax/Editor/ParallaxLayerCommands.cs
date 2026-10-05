using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Adiciona, remove e reordena camadas mantendo o profile e os objetos da cena em sincronia, com undo.
    /// </summary>
    internal static class ParallaxLayerCommands
    {
        public static void Add(ParallaxRig rig, SerializedObject profileObject, SerializedProperty layers)
        {
            int index = layers.arraySize;
            layers.InsertArrayElementAtIndex(index);
            var added = layers.GetArrayElementAtIndex(index);
            added.FindPropertyRelative("name").stringValue = "Camada " + index;
            added.FindPropertyRelative("factor").vector2Value = new Vector2(0.5f, 0f);
            added.FindPropertyRelative("depth").floatValue = 0f;
            added.FindPropertyRelative("windInfluence").floatValue = 1f;
            added.FindPropertyRelative("blur").floatValue = 0f;
            added.FindPropertyRelative("autoScroll").vector2Value = Vector2.zero;
            added.FindPropertyRelative("loopHorizontally").boolValue = true;
            added.FindPropertyRelative("tint").colorValue = Color.white;
            profileObject.ApplyModifiedProperties();

            var layerObject = new GameObject("Camada " + index);
            Undo.RegisterCreatedObjectUndo(layerObject, "Adicionar camada");
            layerObject.transform.SetParent(rig.transform, false);
            var component = layerObject.AddComponent<ParallaxLayer>();
            component.SettingsIndex = index;
            RegisterLayers(rig);
        }

        public static void Remove(ParallaxRig rig, SerializedObject profileObject, SerializedProperty layers, int index)
        {
            var removed = new List<ParallaxLayer>();

            foreach (var layer in rig.GetComponentsInChildren<ParallaxLayer>(true))
            {
                if (layer.SettingsIndex == index)
                    removed.Add(layer);
                else if (layer.SettingsIndex > index)
                {
                    Undo.RecordObject(layer, "Remover camada");
                    layer.SettingsIndex--;
                }
            }

            layers.DeleteArrayElementAtIndex(index);
            profileObject.ApplyModifiedProperties();

            foreach (var layer in removed)
                Undo.DestroyObjectImmediate(layer.gameObject);

            RegisterLayers(rig);
        }

        public static void Move(ParallaxRig rig, SerializedObject profileObject, SerializedProperty layers, int from, int to)
        {
            foreach (var layer in rig.GetComponentsInChildren<ParallaxLayer>(true))
            {
                int index = layer.SettingsIndex;
                int updated = index;
                if (index == from)
                    updated = to;
                else if (from < to && index > from && index <= to)
                    updated = index - 1;
                else if (from > to && index >= to && index < from)
                    updated = index + 1;

                if (updated != index)
                {
                    Undo.RecordObject(layer, "Mover camada");
                    layer.SettingsIndex = updated;
                }
            }

            layers.MoveArrayElement(from, to);
            profileObject.ApplyModifiedProperties();
            RegisterLayers(rig);
        }

        private static void RegisterLayers(ParallaxRig rig)
        {
            Undo.RecordObject(rig, "Atualizar camadas");
            rig.CollectLayers();
            EditorUtility.SetDirty(rig);
        }
    }
}
