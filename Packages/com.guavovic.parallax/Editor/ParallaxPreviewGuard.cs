using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// O preview mexe nas posições e escalas reais das camadas. Antes de recompilar, salvar a cena,
    /// entrar em Play ou fechar a Unity, tudo volta ao lugar, para o preview nunca ficar gravado na cena.
    /// </summary>
    [InitializeOnLoad]
    internal static class ParallaxPreviewGuard
    {
        static ParallaxPreviewGuard()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ResetAll;
            EditorSceneManager.sceneSaving += (scene, path) => ResetAll();
            EditorApplication.quitting += ResetAll;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                    ResetAll();
            };
        }

        public static void ResetAll()
        {
            foreach (var rig in Object.FindObjectsByType<ParallaxRig>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (rig.IsPreviewing)
                    rig.ResetPreview();
            }
        }
    }
}
