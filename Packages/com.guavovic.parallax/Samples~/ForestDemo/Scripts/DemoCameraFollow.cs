using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    public sealed class DemoCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField] private bool followY = true;
        [Tooltip("Quanto da subida ou descida do alvo a câmera acompanha na vertical.")]
        [SerializeField, Range(0f, 1f)] private float verticalFactor = 0.6f;
        [Tooltip("Limites da câmera na vertical, para não mostrar o vazio acima do céu nem abaixo do chão.")]
        [SerializeField] private float minY = -1.6f;
        [SerializeField] private float maxY = 2.4f;

        private Vector3 _velocity;
        private float _restY;
        private float _targetRestY;
        private bool _hasRest;
        private DemoPlayer2D _player;

        private void Start()
        {
            if (target != null)
                _player = target.GetComponent<DemoPlayer2D>();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            // A referência vertical é a posição do alvo quando ele toca o chão pela primeira vez.
            if (!_hasRest && (_player == null || _player.IsGrounded()))
            {
                _restY = transform.position.y;
                _targetRestY = target.position.y;
                _hasRest = true;
            }

            float goalY = transform.position.y;
            if (followY && _hasRest)
                goalY = Mathf.Clamp(_restY + (target.position.y - _targetRestY) * verticalFactor, minY, maxY);

            var goal = new Vector3(target.position.x, goalY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
        }
    }
}
