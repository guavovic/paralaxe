using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Criar um rig a partir de imagens, ou dar um profile a um rig que não tem, perguntando onde salvar.
    /// </summary>
    internal static class ParallaxRigSetup
    {
        public const string IconFolder = "Packages/com.guavovic.parallax/Editor/Icons/";

        /// <summary>
        /// Com <paramref name="preset"/>, as camadas recebem os valores daquele bioma; sem, as distâncias só se espalham.
        /// </summary>
        public static ParallaxRig CreateFromSprites(IReadOnlyList<Sprite> sprites, ParallaxMode mode, string preset = null)
        {
            string path = AskProfilePath();
            if (string.IsNullOrEmpty(path))
                return null;

            var rig = ParallaxLayerCommands.CreateRig("Parallax", mode, path);
            ParallaxLayerCommands.AddSprites(rig, new SerializedObject(rig.Profile), sprites, spread: true);
            if (!string.IsNullOrEmpty(preset))
            {
                var template = ParallaxPresets.CreateBuiltIn(preset);
                ParallaxPresets.Apply(template, rig.Profile);
                Object.DestroyImmediate(template);
                EditorUtility.SetDirty(rig.Profile);
            }

            AssetDatabase.SaveAssetIfDirty(rig.Profile);
            return rig;
        }

        /// <summary>
        /// O modo do profile novo segue a câmera que o rig já usa. Devolve false se a pessoa cancelar.
        /// </summary>
        public static bool CreateProfileFor(ParallaxRig rig, Camera camera)
        {
            string path = AskProfilePath();
            if (string.IsNullOrEmpty(path))
                return false;

            var mode = camera != null && !camera.orthographic ? ParallaxMode.Perspective : ParallaxMode.Simulated2D;
            var profile = ParallaxLayerCommands.CreateProfile(path, mode);
            Undo.RecordObject(rig, "Criar profile");
            rig.Profile = profile;
            EditorUtility.SetDirty(rig);
            return true;
        }

        private static string AskProfilePath()
        {
            // Sugere um nome livre, para não substituir o profile de outro rig sem querer.
            string suggested = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath("Assets/ParallaxProfile.asset"));
            return EditorUtility.SaveFilePanelInProject("Salvar o ParallaxProfile", suggested, "asset", "Onde salvar o profile do parallax.");
        }
    }
}
