using System.Collections.Generic;
using UnityEngine;

public sealed class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private ParallaxCamera parallaxCamera;
    [SerializeField] private List<ParallaxLayerController> parallaxLayers;

    private void Start()
    {
        if (Camera.main.TryGetComponent(out ParallaxCamera parallaxCamera))
        {
            this.parallaxCamera = parallaxCamera;
            this.parallaxCamera.onCameraTranslate += Move;
        }
    }

    private void OnDestroy()
    {
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate -= Move;
    }

    public void AddParallaxLayer(ParallaxLayerController parallaxLayer)
    {
        if (parallaxLayers == null)
            parallaxLayers = new List<ParallaxLayerController>();

        parallaxLayers.Add(parallaxLayer);
    }

    private void Move(Vector3 delta)
    {
        foreach (ParallaxLayerController layer in parallaxLayers)
            layer.Move(delta);
    }
}