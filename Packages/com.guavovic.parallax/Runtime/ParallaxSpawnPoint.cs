using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Onde o viajante aparece ao chegar por uma passagem. Roda antes do rig, para a câmera já estar no lugar
    /// quando o fundo começa.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class ParallaxSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private string travelerTag = "Player";
        [Tooltip("1 entra andando para a direita, -1 para a esquerda.")]
        [SerializeField] private float walkDirection = 1f;
        [SerializeField, Min(0f)] private float fadeSeconds = 0.45f;

        public string Id { get => id; set => id = value; }
        public float WalkDirection { get => walkDirection; set => walkDirection = value; }

        private void Start()
        {
            if (!ParallaxSceneTravel.TryArrive(id))
                return;

            var position = transform.position;
            var traveler = GameObject.FindGameObjectWithTag(travelerTag);
            if (traveler != null)
            {
                var arrival = new Vector3(position.x, position.y, traveler.transform.position.z);
                var receiver = traveler.GetComponent<IParallaxTraveler>();
                if (receiver != null) receiver.Arrive(arrival, walkDirection);
                else traveler.transform.position = arrival;
            }

            var camera = Camera.main;
            if (camera != null)
                camera.transform.position = new Vector3(position.x, camera.transform.position.y, camera.transform.position.z);

            ParallaxScreenFade.FadeIn(fadeSeconds);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.62f, 0.83f, 0.85f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * (0.6f * Mathf.Sign(walkDirection)));
        }
    }
}
