using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Boas-vindas com créditos e atalhos. Abre sozinha quando o pacote entra no projeto ou muda de versão,
    /// e também a cada vez que o projeto abre, se a pessoa marcar a opção.
    /// </summary>
    internal sealed class ParallaxWelcomeWindow : EditorWindow
    {
        private const string PackageName = "com.guavovic.parallax";
        private const string RepositoryUrl = "https://github.com/guavovic/paralaxe";
        private const string BannerPath = "Packages/com.guavovic.parallax/Editor/Welcome/Banner.png";
        private const string IconPath = "Packages/com.guavovic.parallax/Editor/Icons/ParallaxRig.png";
        private const string SampleSceneGuid = "06c5b48c88d39bb43a002704d0ba083d";
        private const string ShownThisSession = "Paralaxe.Welcome.ShownThisSession";

        private static GUIStyle _title;
        private static GUIStyle _body;

        private Texture2D _banner;

        [MenuItem("Window/Parallax/Sobre", false, 100)]
        [MenuItem("Tools/Parallax/Sobre", false, 100)]
        [MenuItem("Help/Paralaxe", false, 1000)]
        public static void Open()
        {
            var window = GetWindow<ParallaxWelcomeWindow>(true, "Paralaxe", true);
            window.minSize = window.maxSize = new Vector2(520f, 540f);
        }

        private static string Version => UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ParallaxRig).Assembly)?.version ?? "";

        // As preferências valem por projeto, não para a máquina inteira.
        private static string ProjectKey(string name) => "Paralaxe.Welcome." + name + "." + PlayerSettings.productGUID;

        private static bool ShowOnOpen
        {
            get => EditorPrefs.GetBool(ProjectKey("ShowOnOpen"), false);
            set => EditorPrefs.SetBool(ProjectKey("ShowOnOpen"), value);
        }

        public static void ShowIfDue()
        {
            bool newVersion = EditorPrefs.GetString(ProjectKey("Version"), "") != Version;
            bool openedNow = ShowOnOpen && !SessionState.GetBool(ShownThisSession, false);
            if (!newVersion && !openedNow)
                return;

            EditorPrefs.SetString(ProjectKey("Version"), Version);
            SessionState.SetBool(ShownThisSession, true);
            Open();
        }

        private void OnEnable()
        {
            _banner = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerPath);
            titleContent = new GUIContent("Paralaxe", AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath));
        }

        private void OnGUI()
        {
            _title ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 22 };
            _body ??= new GUIStyle { padding = new RectOffset(14, 14, 0, 0) };

            if (_banner != null)
            {
                // Altura limitada, para o rodapé com a opção de abrir caber na janela.
                var rect = GUILayoutUtility.GetRect(position.width, Mathf.Min(220f, position.width * _banner.height / _banner.width));
                GUI.DrawTexture(rect, _banner, ScaleMode.ScaleAndCrop);
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12f);
                GUILayout.Label("Paralaxe", _title);
                GUILayout.FlexibleSpace();
                GUILayout.Label("v" + Version, EditorStyles.miniLabel);
                GUILayout.Space(12f);
            }

            using (new EditorGUILayout.VerticalScope(_body))
            {
                EditorGUILayout.LabelField("Cenários com parallax em camadas, montados direto no editor, em 2D ou em perspectiva.", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space(10f);
                DrawShortcuts();
                EditorGUILayout.Space(12f);
                DrawCredits();
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                ShowOnOpen = GUILayout.Toggle(ShowOnOpen, " Mostrar ao abrir o projeto");
                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawShortcuts()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Abrir o editor", GUILayout.Height(28f)))
                    ParallaxEditorWindow.Open();
                if (GUILayout.Button("Criar um parallax", GUILayout.Height(28f)))
                    ParallaxEditorWindow.OpenNew();
            }

            // A floresta pode já estar no projeto (importada antes, ou no próprio repositório do pacote).
            string scenePath = AssetDatabase.GUIDToAssetPath(SampleSceneGuid);
            if (!string.IsNullOrEmpty(scenePath))
            {
                if (GUILayout.Button("Abrir a floresta de exemplo", GUILayout.Height(28f)) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(scenePath);
                return;
            }

            var sample = Sample.FindByPackage(PackageName, Version).FirstOrDefault();
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(sample.displayName)))
            {
                if (GUILayout.Button("Importar a floresta de exemplo", GUILayout.Height(28f)))
                    sample.Import();
            }
        }

        private static void DrawCredits()
        {
            EditorGUILayout.LabelField("Créditos", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Feito por Gustavo Victor.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Ao usar num jogo ou projeto, credite \"Paralaxe, por Gustavo Victor\". A arte do exemplo é só para conhecer o pacote.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4f);
            if (EditorGUILayout.LinkButton("github.com/guavovic/paralaxe"))
                Application.OpenURL(RepositoryUrl);
        }
    }
}
