using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    public sealed class DemoControlPanel : MonoBehaviour
    {
        private void OnGUI()
        {
            var world = ParallaxWorld.Current;
            if (world == null)
                return;

            GUILayout.BeginArea(new Rect(12, 12, 260, 160), GUI.skin.box);
            GUILayout.Label("Parallax");

            GUILayout.Label($"Velocidade: {world.SpeedMultiplier:F2}");
            world.SpeedMultiplier = GUILayout.HorizontalSlider(world.SpeedMultiplier, 0f, 2f);

            GUILayout.Label($"Vento: {world.BaseWindStrength:F2} (rajada {world.Gust:F2})");
            world.BaseWindStrength = GUILayout.HorizontalSlider(world.BaseWindStrength, 0f, 3f);

            GUILayout.Label("A/D ou setas andam, espaço pula");
            GUILayout.EndArea();
        }
    }
}
