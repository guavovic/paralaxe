using System.Collections.Generic;
using UnityEngine;

public sealed class LayerTypeCollection : MonoBehaviour
{
    [SerializeField] private LayerType layerType;
    [SerializeField] private List<ParallaxLayerController> parallaxLayers;

    public void SetLayerType(LayerType type)
    {
        layerType = type;
    }

    public void AddParallaxLayer(ParallaxLayerController parallaxLayer)
    {
        if (parallaxLayers == null)
            parallaxLayers = new List<ParallaxLayerController>();

        parallaxLayers.Add(parallaxLayer);
    }
}
