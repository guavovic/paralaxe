using UnityEngine;

public sealed class ParallaxLayerController : MonoBehaviour
{
    [SerializeField] private float parallaxFactor;

    private float _lenght;
    private Vector2 _startPosition;
    private Transform _camera;

    private void Awake()
    {
        if (!HasChildren())
            return;

        _startPosition = new Vector2(transform.position.x, transform.position.y);
        _lenght = transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;
        _camera = Camera.main.transform;

        CreateCopies();
    }

    public void SetParallaxFactor(float parallaxFactor) { this.parallaxFactor = parallaxFactor; }

    public void Move(Vector2 delta)
    {
        if (!HasChildren())
            return;

        Vector2 rePosition = _camera.transform.position * (1 - parallaxFactor);

        var newPosition = new Vector2(_camera.transform.position.x * parallaxFactor, _camera.transform.position.y * parallaxFactor);

        transform.position = _startPosition + newPosition;

        HorizontalBoundsCheck(rePosition.x);
    }

    private void HorizontalBoundsCheck(float delta)
    {
        if (delta > _startPosition.x + _lenght)
        {
            _startPosition.x += _lenght;
        }
        else if (delta < _startPosition.x - _lenght)
        {
            _startPosition.x -= _lenght;
        }
    }

    private bool HasChildren() => transform.childCount != 0;

    private void CreateCopies()
    {
        var childTransform = transform.GetChild(0);
        var startPositionX = childTransform.position.x;
        var startPositionY = childTransform.position.y;
        var startPositionZ = childTransform.position.z;
        var rotation = childTransform.rotation;
        var parent = transform.gameObject.transform;

        Instantiate(childTransform, new Vector3(startPositionX - _lenght, startPositionY, startPositionZ), rotation, parent);
        Instantiate(childTransform, new Vector3(-(startPositionX - _lenght), startPositionY, startPositionZ), rotation, parent);
    }
}