using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LayerController : MonoBehaviour
{
    private float _startPosition;
    private float _length;

    private void Start()
    {
        _startPosition = transform.position.x;

        if (TryGetComponent(out SpriteRenderer spriteRenderer))
            _length = spriteRenderer.bounds.size.x;
    }

    public void Move(Vector3 delta, float parallaxFactor)
    {
        //var newPositon = new Vector2(_startPosition, transform.localPosition.y);

        //newPositon.x += delta.x * parallaxFactor;
        //newPositon.y += delta.y * parallaxFactor;

        //transform.position = newPositon;

        //float temp = (Camera.main.transform.position.x * (1 - parallaxFactor));
        //float dist = (Camera.main.transform.position.x * parallaxFactor);

        //transform.position = new Vector3(_startPosition + dist, transform.position.y, transform.position.z);

        //if (temp > _startPosition + _length)
        //{
        //    _startPosition += _length;
        //}
        //else if (temp < _startPosition - _length)
        //{
        //    _startPosition -= _length;
        //}
    }
}
