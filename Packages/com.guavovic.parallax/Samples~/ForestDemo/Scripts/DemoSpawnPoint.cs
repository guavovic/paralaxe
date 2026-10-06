using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Onde o herói aparece ao chegar por uma passagem. Ele entra andando para dentro do cenário.
    /// </summary>
    public sealed class DemoSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private DemoPlayer2D player;
        [Tooltip("1 entra andando para a direita, -1 para a esquerda.")]
        [SerializeField] private float walkDirection = 1f;
        [SerializeField, Min(0f)] private float walkSeconds = 0.5f;

        private void Start()
        {
            if (player == null || DemoSceneTravel.NextSpawn != id)
                return;

            DemoSceneTravel.NextSpawn = null;
            player.transform.position = new Vector3(transform.position.x, transform.position.y, player.transform.position.z);
            var camera = Camera.main;
            if (camera != null)
                camera.transform.position = new Vector3(transform.position.x, camera.transform.position.y, camera.transform.position.z);
            player.WalkFor(walkDirection, walkSeconds);
        }
    }
}
