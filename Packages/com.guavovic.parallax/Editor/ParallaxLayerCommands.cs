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
            layers.MoveArrayElement(from, to);
            profileObject.ApplyModifiedProperties();
            RemapAfterMove(rig, from, to);
        }

        /// <summary>
        /// Acompanha nos objetos da cena uma configuração que já mudou de posição no profile,
        /// como depois de arrastar na lista.
        /// </summary>
        public static void RemapAfterMove(ParallaxRig rig, int from, int to)
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

            RegisterLayers(rig);
        }

        /// <summary>
        /// Cria um rig novo, com o profile salvo em <paramref name="profilePath"/>.
        /// </summary>
        public static ParallaxRig CreateRig(string name, ParallaxMode mode, string profilePath)
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            profile.SetMode(mode);
            AssetDatabase.CreateAsset(profile, profilePath);

            var rigObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(rigObject, "Novo parallax");
            var rig = rigObject.AddComponent<ParallaxRig>();
            rig.Profile = profile;
            rig.TargetCamera = Camera.main;
            return rig;
        }

        /// <summary>
        /// Uma camada por imagem, na ordem dada (a primeira é a mais distante). Com <paramref name="spread"/>,
        /// as distâncias se espalham de longe para perto; sem, todas começam no meio.
        /// </summary>
        public static void AddSprites(ParallaxRig rig, SerializedObject profileObject, IReadOnlyList<Sprite> sprites, bool spread)
        {
            if (sprites.Count == 0)
                return;

            profileObject.ApplyModifiedProperties();
            var profile = rig.Profile;
            int start = profile.Layers.Count;
            int order = NextSortingOrder(rig, sprites.Count);

            Undo.RecordObject(profile, "Adicionar imagens");
            for (int i = 0; i < sprites.Count; i++)
            {
                float t = sprites.Count == 1 ? 0.5f : i / (float)(sprites.Count - 1);
                float distance = spread ? Mathf.Lerp(0.9f, 0.1f, t) : 0.5f;
                var sprite = sprites[i];
                profile.AddLayer(new ParallaxLayerSettings(sprite.name, new Vector2(distance, distance * 0.1f), profile.FactorToDepth(distance)));

                var layer = CreateLayerObject(rig, start + i, "Camada " + (start + i) + " - " + sprite.name);
                var image = new GameObject(sprite.name);
                image.transform.SetParent(layer.transform, false);
                var spriteRenderer = image.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingOrder = order + i;
            }

            EditorUtility.SetDirty(profile);
            profileObject.Update();
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

        private static int NextSortingOrder(ParallaxRig rig, int count)
        {
            var renderers = rig.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
                return -count;

            int highest = int.MinValue;
            foreach (var spriteRenderer in renderers)
                highest = Mathf.Max(highest, spriteRenderer.sortingOrder);
            return highest + 1;
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
