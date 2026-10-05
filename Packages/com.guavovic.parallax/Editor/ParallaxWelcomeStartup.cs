using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Ao carregar o editor, abre as boas-vindas quando for a hora. Fica fora da janela porque a Unity
    /// não deixa consultar o editor no construtor estático de um ScriptableObject.
    /// </summary>
    [InitializeOnLoad]
    internal static class ParallaxWelcomeStartup
    {
        static ParallaxWelcomeStartup()
        {
            if (Application.isBatchMode)
                return;

            EditorApplication.delayCall += ParallaxWelcomeWindow.ShowIfDue;
        }
    }
}
