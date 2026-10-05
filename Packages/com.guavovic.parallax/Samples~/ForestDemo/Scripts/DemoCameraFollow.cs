using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>
    /// Segue o alvo com suavização. Os limites da cena ficam no ParallaxCameraBounds da câmera.
    /// </summary>
    public sealed class DemoCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField] private bool followY = true;
        [Tooltip("Quanto da subida ou descida do alvo a câmera acompanha na vertical.")]
        [SerializeField, Range(0f, 1f)] private float verticalFactor = 0.6f;

        private Vector3 _velocity;
        private float _restY;
        private float _targetRestY;
        private bool _hasRest;
        private DemoPlayer2D _player;

        private void LateUpdate()
        {
            if (target == null)
                return;

            if (_player == null)
                _player = target.GetComponent<DemoPlayer2D>();

            // A referência vertical é a posição do alvo quando ele toca o chão pela primeira vez.
            if (!_hasRest && (_player == null || _player.IsGrounded()))
            {
                _restY = transform.position.y;
                _targetRestY = target.position.y;
                _hasRest = true;
            }

            float goalY = transform.position.y;
            if (followY && _hasRest)
                goalY = _restY + (target.position.y - _targetRestY) * verticalFactor;

            var goal = new Vector3(target.position.x, goalY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
        }
    }
}
