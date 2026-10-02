using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    public sealed class DemoControlPanel : MonoBehaviour
    {
        private ParallaxWorld _world;

        private void OnGUI()
        {
            if (_world == null)
                _world = ParallaxWorld.Current;

            if (_world == null)
                return;

            GUILayout.BeginArea(new Rect(12, 12, 260, 160), GUI.skin.box);
            GUILayout.Label("Parallax");

            GUILayout.Label($"Velocidade: {_world.SpeedMultiplier:F2}");
            _world.SpeedMultiplier = GUILayout.HorizontalSlider(_world.SpeedMultiplier, 0f, 2f);

            GUILayout.Label($"Vento: {_world.BaseWindStrength:F2} (rajada {_world.Gust:F2})");
            _world.BaseWindStrength = GUILayout.HorizontalSlider(_world.BaseWindStrength, 0f, 3f);

            GUILayout.Label("A/D ou setas andam, espaço pula");
            GUILayout.EndArea();
        }
    }
}
