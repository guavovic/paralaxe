using UnityEngine;

public sealed class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(Vector3 deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (transform.position != _startPosition)
        {
            if (onCameraTranslate != null)
            {
                Vector3 delta = _startPosition - transform.position;
                onCameraTranslate(delta);
            }

            _startPosition = transform.position;
        }
    }
}
