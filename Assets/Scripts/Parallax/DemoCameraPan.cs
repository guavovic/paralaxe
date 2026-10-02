using UnityEngine;

public sealed class DemoCameraPan : MonoBehaviour
{
    [SerializeField] private float amplitude = 12f;
    [SerializeField] private float speed = 0.4f;

    private float _startX;

    private void Start()
    {
        _startX = transform.position.x;
    }

    private void Update()
    {
        var position = transform.position;
        position.x = _startX + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = position;
    }
}
