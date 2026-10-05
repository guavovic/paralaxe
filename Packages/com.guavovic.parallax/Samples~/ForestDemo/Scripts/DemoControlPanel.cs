using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Painel de teste no canto da tela. Usa GUI com posições fixas e guarda os textos,
    /// então não gera lixo a cada quadro.
    /// </summary>
    public sealed class DemoControlPanel : MonoBehaviour
    {
        private const string Help = "A/D ou setas andam, espaço pula";

        private int _speedShown = int.MinValue;
        private int _windShown = int.MinValue;
        private int _gustShown = int.MinValue;
        private string _speedLabel;
        private string _windLabel;

        private void Awake()
        {
            // Sem o passo de layout do IMGUI, que aloca a cada evento.
            useGUILayout = false;
        }

        private void OnGUI()
        {
            var world = ParallaxWorld.Current;
            if (world == null)
                return;

            UpdateLabels(world);

            GUI.Box(new Rect(12f, 12f, 260f, 150f), "Parallax");
            GUI.Label(new Rect(20f, 36f, 244f, 20f), _speedLabel);
            world.SpeedMultiplier = GUI.HorizontalSlider(new Rect(20f, 58f, 244f, 16f), world.SpeedMultiplier, 0f, 2f);
            GUI.Label(new Rect(20f, 80f, 244f, 20f), _windLabel);
            world.BaseWindStrength = GUI.HorizontalSlider(new Rect(20f, 102f, 244f, 16f), world.BaseWindStrength, 0f, 3f);
            GUI.Label(new Rect(20f, 126f, 244f, 20f), Help);
        }

        private void UpdateLabels(ParallaxWorld world)
        {
            int speed = Mathf.RoundToInt(world.SpeedMultiplier * 100f);
            if (speed != _speedShown)
            {
                _speedShown = speed;
                _speedLabel = $"Velocidade: {speed / 100f:F2}";
            }

            int wind = Mathf.RoundToInt(world.BaseWindStrength * 100f);
            int gust = Mathf.RoundToInt(world.Gust * 100f);
            if (wind != _windShown || gust != _gustShown)
            {
                _windShown = wind;
                _gustShown = gust;
                _windLabel = $"Vento: {wind / 100f:F2} (rajada {gust / 100f:F2})";
            }
        }
    }
}
