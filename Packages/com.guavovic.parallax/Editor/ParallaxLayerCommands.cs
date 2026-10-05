using System;
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
        public static void Add(ParallaxRig rig, SerializedObject profileObject)
        {
            profileObject.ApplyModifiedProperties();
            var profile = rig.Profile;
            int index = profile.Layers.Count;

            // Os valores padrão vêm do próprio ParallaxLayerSettings.
            Undo.RecordObject(profile, "Adicionar camada");
            profile.AddLayer(new ParallaxLayerSettings()).SetName("Camada " + index);
            EditorUtility.SetDirty(profile);
            profileObject.Update();

            CreateLayerObject(rig, index, "Camada " + index);
            RegisterLayers(rig);
        }

        public static void Remove(ParallaxRig rig, SerializedObject profileObject, SerializedProperty layers, int index)
        {
            var removed = new List<ParallaxLayer>();
            foreach (var layer in rig.GetComponentsInChildren<ParallaxLayer>(true))
            {
                if (layer.SettingsIndex == index)
                    removed.Add(layer);
            }

            RemapIndices(rig, "Remover camada", i => i > index ? i - 1 : i);
            layers.DeleteArrayElementAtIndex(index);
            profileObject.ApplyModifiedProperties();

            foreach (var layer in removed)
                Undo.DestroyObjectImmediate(layer.gameObject);

            RegisterLayers(rig);
        }

        public static void Move(ParallaxRig rig, SerializedObject profileObject, SerializedProperty layers, int from, int to)
        {
            RemapIndices(rig, "Mover camada", i =>
            {
                if (i == from)
                    return to;
                if (from < to && i > from && i <= to)
                    return i - 1;
                if (from > to && i >= to && i < from)
                    return i + 1;
                return i;
            });

            layers.MoveArrayElement(from, to);
            profileObject.ApplyModifiedProperties();
            RegisterLayers(rig);
        }

        /// <summary>
        /// Cria o objeto de uma camada como filho do rig, apontando para a configuração no índice dado.
        /// </summary>
        public static ParallaxLayer CreateLayerObject(ParallaxRig rig, int index, string name)
        {
            var layerObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(layerObject, "Adicionar camada");
            layerObject.transform.SetParent(rig.transform, false);
            var layer = layerObject.AddComponent<ParallaxLayer>();
            layer.SettingsIndex = index;
            return layer;
        }

        public static void RegisterLayers(ParallaxRig rig)
        {
            Undo.RecordObject(rig, "Atualizar camadas");
            rig.CollectLayers();
            EditorUtility.SetDirty(rig);
        }

        private static void RemapIndices(ParallaxRig rig, string undoName, Func<int, int> map)
        {
            foreach (var layer in rig.GetComponentsInChildren<ParallaxLayer>(true))
            {
                int updated = map(layer.SettingsIndex);
                if (updated == layer.SettingsIndex)
                    continue;

                Undo.RecordObject(layer, undoName);
                layer.SettingsIndex = updated;
            }
        }
    }
}
