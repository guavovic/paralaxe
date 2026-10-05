using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Conjuntos prontos de camadas e valores do mundo por bioma, e os que o usuário salva.
    /// Aplicar distribui as camadas do preset pelas do rig por posição relativa, de trás para a frente.
    /// </summary>
    internal static class ParallaxPresets
    {
        public const string UserFolder = "Assets/Paralaxe Presets";

        public static readonly string[] BuiltInNames = { "Floresta", "Caverna", "Pântano", "Neve", "Deserto", "Cidade" };

        private static readonly string[] GlobalProperties = { "speedMultiplier", "windDirection", "windStrength", "windSpeed", "gustDecay", "maxGust", "focusDistance" };

        /// <summary>
        /// Um profile temporário com o preset pronto. Quem chama destrói depois de usar.
        /// </summary>
        public static ParallaxProfile CreateBuiltIn(string name)
        {
            var profile = ScriptableObject.CreateInstance<ParallaxProfile>();
            profile.hideFlags = HideFlags.DontSave;

            switch (name)
            {
                case "Floresta":
                    profile.SetWind(Vector2.right, 0.6f, 1f);
                    Add(profile, "Céu", 0.99f);
                    Add(profile, "Nuvens", 0.96f, blur: 0.6f, scroll: new Vector2(0.12f, 0f), alpha: 0.55f);
                    Add(profile, "Montanhas", 0.93f, blur: 1f);
                    Add(profile, "Floresta distante", 0.88f, blur: 0.8f, wind: 0.15f);
                    Add(profile, "Neblina", 0.85f, blur: 1.2f, scroll: new Vector2(-0.18f, 0f), alpha: 0.3f);
                    Add(profile, "Árvores", 0.75f, blur: 0.5f, wind: 0.2f);
                    Add(profile, "Floresta do meio", 0.58f, wind: 0.5f);
                    Add(profile, "Chão", 0f);
                    Add(profile, "Primeiro plano", -0.3f, blur: 1.5f, wind: 0.6f);
                    break;
                case "Caverna":
                    profile.SetWind(Vector2.right, 0.1f, 0.5f);
                    Add(profile, "Fundo", 0.97f, tint: new Color(0.7f, 0.75f, 0.9f));
                    Add(profile, "Estalactites distantes", 0.88f, blur: 1f);
                    Add(profile, "Cristais", 0.72f, blur: 0.5f);
                    Add(profile, "Esporos", 0.6f, scroll: new Vector2(0.03f, 0.08f), alpha: 0.6f);
                    Add(profile, "Paredes", 0.45f);
                    Add(profile, "Chão", 0f);
                    Add(profile, "Estalactites da frente", -0.4f, blur: 2f);
                    break;
                case "Pântano":
                    profile.SetWind(Vector2.right, 0.3f, 0.7f);
                    Add(profile, "Céu", 0.99f);
                    Add(profile, "Árvores mortas", 0.85f, blur: 1f, wind: 0.1f);
                    Add(profile, "Névoa", 0.8f, blur: 1.5f, scroll: new Vector2(-0.1f, 0f), alpha: 0.4f);
                    Add(profile, "Juncos", 0.5f, wind: 0.6f);
                    Add(profile, "Água", 0f);
                    Add(profile, "Primeiro plano", -0.3f, blur: 1.2f, wind: 0.8f);
                    break;
                case "Neve":
                    profile.SetWind(Vector2.right, 0.8f, 1.2f);
                    Add(profile, "Céu", 0.99f);
                    Add(profile, "Montanhas", 0.95f, blur: 1f);
                    Add(profile, "Pinheiros distantes", 0.85f, blur: 0.8f, wind: 0.1f);
                    Add(profile, "Neve caindo", 0.6f, scroll: new Vector2(0.05f, -0.6f), alpha: 0.7f);
                    Add(profile, "Pinheiros", 0.5f, wind: 0.3f);
                    Add(profile, "Chão", 0f);
                    Add(profile, "Neve da frente", -0.4f, blur: 1.5f, scroll: new Vector2(0.1f, -1.2f));
                    break;
                case "Deserto":
                    profile.SetWind(Vector2.right, 1f, 1.5f);
                    profile.SetSpeedMultiplier(1.1f);
                    Add(profile, "Céu", 0.99f, tint: new Color(1f, 0.95f, 0.85f));
                    Add(profile, "Dunas distantes", 0.92f, blur: 1f);
                    Add(profile, "Dunas", 0.8f, blur: 0.4f);
                    Add(profile, "Rochas", 0.55f);
                    Add(profile, "Chão", 0f);
                    Add(profile, "Poeira", -0.2f, blur: 1f, scroll: new Vector2(0.4f, 0f), alpha: 0.4f);
                    break;
                case "Cidade":
                    profile.SetWind(Vector2.right, 0.1f, 0.5f);
                    Add(profile, "Céu", 0.99f);
                    Add(profile, "Prédios distantes", 0.9f, blur: 1f, tint: new Color(0.75f, 0.8f, 0.95f));
                    Add(profile, "Prédios", 0.75f, blur: 0.4f);
                    Add(profile, "Prédios próximos", 0.5f);
                    Add(profile, "Rua", 0f);
                    Add(profile, "Postes", -0.3f, blur: 1f);
                    break;
                default:
                    throw new ArgumentException("Preset desconhecido: " + name, nameof(name));
            }

            return profile;
        }

        public static List<ParallaxProfile> FindUserPresets()
        {
            var presets = new List<ParallaxProfile>();
            if (!AssetDatabase.IsValidFolder(UserFolder))
                return presets;

            foreach (string guid in AssetDatabase.FindAssets("t:ParallaxProfile", new[] { UserFolder }))
                presets.Add(AssetDatabase.LoadAssetAtPath<ParallaxProfile>(AssetDatabase.GUIDToAssetPath(guid)));
            return presets;
        }

        /// <summary>
        /// Leva os valores do preset para o profile, mantendo os nomes e a quantidade de camadas do profile.
        /// A camada i de n pega a camada do preset na mesma posição relativa.
        /// </summary>
        public static void Apply(ParallaxProfile preset, ParallaxProfile target)
        {
            using (var source = new SerializedObject(preset))
            using (var destination = new SerializedObject(target))
            {
                foreach (string name in GlobalProperties)
                    destination.CopyFromSerializedProperty(source.FindProperty(name));
                destination.ApplyModifiedProperties();
            }

            int from = preset.Layers.Count;
            int to = target.Layers.Count;
            if (from == 0)
                return;

            for (int i = 0; i < to; i++)
            {
                int j = to == 1 ? 0 : Mathf.RoundToInt(i * (from - 1) / (float)(to - 1));
                var a = preset.Layers[j];
                var b = target.Layers[i];
                b.SetFactor(a.Factor);
                b.SetDepth(a.Depth);
                b.SetWindInfluence(a.WindInfluence);
                b.SetBlur(a.Blur);
                b.SetAutoScroll(a.AutoScroll);
                b.SetLoopHorizontally(a.LoopHorizontally);
                b.SetTint(a.Tint);
            }
        }

        /// <summary>
        /// Salva uma cópia do profile na pasta de presets do usuário e devolve o caminho.
        /// </summary>
        public static string SaveAsPreset(ParallaxProfile source, string name)
        {
            if (!AssetDatabase.IsValidFolder(UserFolder))
                AssetDatabase.CreateFolder("Assets", "Paralaxe Presets");

            string path = AssetDatabase.GenerateUniqueAssetPath(UserFolder + "/" + name + ".asset");
            var copy = UnityEngine.Object.Instantiate(source);
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            return path;
        }

        /// <summary>
        /// Menu da barra da janela: aplicar um preset pronto ou do usuário, ou salvar o atual.
        /// </summary>
        public static void ShowMenu(Rect rect, ParallaxRig rig, Action changed)
        {
            var menu = new GenericMenu();
            foreach (string name in BuiltInNames)
                menu.AddItem(new GUIContent("Aplicar/" + name), false, () => ApplyWithUndo(CreateBuiltIn(name), rig, changed, destroyPreset: true));

            foreach (var preset in FindUserPresets())
            {
                var chosen = preset;
                menu.AddItem(new GUIContent("Aplicar/Meus presets/" + preset.name), false, () => ApplyWithUndo(chosen, rig, changed, destroyPreset: false));
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Salvar como preset"), false, () =>
            {
                string path = SaveAsPreset(rig.Profile, rig.Profile.name);
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<ParallaxProfile>(path));
            });
            menu.DropDown(rect);
        }

        private static void ApplyWithUndo(ParallaxProfile preset, ParallaxRig rig, Action changed, bool destroyPreset)
        {
            Undo.RecordObject(rig.Profile, "Aplicar preset");
            Apply(preset, rig.Profile);
            EditorUtility.SetDirty(rig.Profile);
            if (destroyPreset)
                UnityEngine.Object.DestroyImmediate(preset);
            changed?.Invoke();
        }

        private static void Add(ParallaxProfile profile, string name, float factor, float blur = 0f, float wind = 0f, Vector2 scroll = default, float alpha = 1f, Color? tint = null)
        {
            // Camadas de fundo acompanham a câmera na vertical; as próximas, menos.
            float vertical = factor >= 0.7f ? 1f : Mathf.Max(0f, factor);
            var settings = new ParallaxLayerSettings(name, new Vector2(factor, vertical), profile.FactorToDepth(factor), wind);
            settings.SetBlur(blur);
            settings.SetAutoScroll(scroll);
            var color = tint ?? Color.white;
            color.a = alpha;
            settings.SetTint(color);
            profile.AddLayer(settings);
        }
    }
}
