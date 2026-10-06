using UnityEngine;

namespace Guavovic.Parallax
{
    /// <summary>
    /// Trava a câmera dentro de uma área do cenário, para o jogo não mostrar o que está fora dele.
    /// Vai na câmera e roda depois de quem a move (o script que segue o herói) e antes do rig.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(90)]
    public sealed class ParallaxCameraBounds : MonoBehaviour
    {
        [Tooltip("Área do cenário que a câmera pode mostrar, em unidades do mundo.")]
        [SerializeField] private Rect area = new Rect(-20f, -6f, 40f, 12f);
        [SerializeField] private bool limitX = true;
        [SerializeField] private bool limitY = true;
        [Tooltip("Câmera em perspectiva: distância até o plano que deve ficar dentro da área. Com um rig, usa a distância de foco dele.")]
        [SerializeField, Min(0.01f)] private float focusDistance = 10f;
        [SerializeField] private ParallaxRig rig;

        private Camera _camera;

        /// <summary>Cor dos gizmos do Paralaxe (limites, passagens, pontos de chegada).</summary>
        public static readonly Color GizmoColor = new Color(0.62f, 0.83f, 0.85f);

        public Rect Area { get => area; set => area = value; }
        public bool LimitX { get => limitX; set => limitX = value; }
        public bool LimitY { get => limitY; set => limitY = value; }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            var position = transform.position;
            var clamped = Clamp(position, HalfExtents(_camera, FocusDistance), area, limitX, limitY);
            transform.position = new Vector3(clamped.x, clamped.y, position.z);
        }

        private float FocusDistance => rig != null && rig.Profile != null ? rig.Profile.FocusDistance : focusDistance;

        /// <summary>
        /// Metade da largura e da altura que a câmera enxerga. Em perspectiva, no plano a <paramref name="focusDistance"/>.
        /// </summary>
        public static Vector2 HalfExtents(Camera camera, float focusDistance)
        {
            float halfHeight = camera.orthographic
                ? camera.orthographicSize
                : focusDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(halfHeight * camera.aspect, halfHeight);
        }

        /// <summary>
        /// Posição do centro da câmera dentro da área. Se a área for menor que a vista, a câmera fica no meio dela.
        /// </summary>
        public static Vector2 Clamp(Vector2 position, Vector2 halfExtents, Rect area, bool limitX, bool limitY)
        {
            if (limitX)
                position.x = ClampAxis(position.x, halfExtents.x, area.xMin, area.xMax);
            if (limitY)
                position.y = ClampAxis(position.y, halfExtents.y, area.yMin, area.yMax);
            return position;
        }

        private static float ClampAxis(float center, float half, float min, float max)
        {
            if (max - min <= half * 2f)
                return (min + max) * 0.5f;
            return Mathf.Clamp(center, min + half, max - half);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireCube(new Vector3(area.center.x, area.center.y, transform.position.z + FocusDistance), new Vector3(area.width, area.height, 0f));
        }
    }
}
