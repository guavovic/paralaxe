using UnityEditor;
using UnityEngine;

public sealed class CreateNewRoomWindow : EditorWindow
{
    private const int MAX_BACKGROUND_LAYER_COUNT = 20;
    private const int MAX_FOREGROUND_LAYER_COUNT = 20;
    private const float DEFAULT_SPACE_VALUE = 1.5f;
    private static readonly Vector2 DEFAULT_WINDOW_RESOLUTION = new Vector2(500, 800);

    private string roomName = "New Room";
    private Transform roomTransformParent;
    private int backgroundLayers;
    private int foregroundLayers;
    private bool _roomCreated = false;

    private static CreateNewRoomWindow _window;

    [MenuItem("Tools/Create New Room")]
    public static void ShowWindow()
    {
        _window = GetWindow<CreateNewRoomWindow>("Create New Room");
        _window.SetWindowSize(DEFAULT_WINDOW_RESOLUTION);
    }

    private void OnGUI()
    {
        if (!_roomCreated)
        {
            GUILayout.BeginArea(new Rect(20, 40, position.width - 40, position.height - 80));
            {
                // Campo de entrada para o nome da sala com limite de caracteres
                EditorGUILayout.LabelField("Room Name:");
                roomName = EditorGUILayout.TextArea(roomName);

                Space(10);
                // ----------------------------------------------------------

                // Campo de entrada para o número de camadas de background
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Room parent: ", GUILayout.Width(225));
                roomTransformParent = (Transform)EditorGUILayout.ObjectField(roomTransformParent, typeof(Transform), true);
                GUILayout.EndHorizontal();

                Space();
                // ----------------------------------------------------------

                // Campo de entrada para o número de camadas de background
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Background Layers: (Max: {MAX_BACKGROUND_LAYER_COUNT})", GUILayout.Width(225));
                backgroundLayers = EditorGUILayout.IntField(backgroundLayers);
                GUILayout.EndHorizontal();

                backgroundLayers = Mathf.Clamp(backgroundLayers, 1, MAX_BACKGROUND_LAYER_COUNT);

                Space();
                // ----------------------------------------------------------

                // Campo de entrada para o número de camadas de foreground
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Foreground Layers: (Max: {MAX_FOREGROUND_LAYER_COUNT})", GUILayout.Width(225));
                foregroundLayers = EditorGUILayout.IntField(foregroundLayers);
                GUILayout.EndHorizontal();

                foregroundLayers = Mathf.Clamp(foregroundLayers, 1, MAX_FOREGROUND_LAYER_COUNT);

                Space(10);
                // ----------------------------------------------------------

                if (GUILayout.Button($"Create {(roomName != null ? roomName : "Room")}"))
                    CreateRoom();
            }

            GUILayout.EndArea();
        }
    }

    private void CreateRoom()
    {
        Debug.Log("Creating Room: " + roomName);
        GameObject newRoom = new GameObject(roomName);

        if (roomTransformParent != null)
        {
            newRoom.transform.SetParent(roomTransformParent);
            newRoom.transform.localPosition = Vector3.zero;
        }

        // ----------------------------------------------------------

        Debug.Log("Creating Environment Art GameObject");
        GameObject environmentArt = new GameObject("EnvironmentArt");
        environmentArt.transform.SetParent(newRoom.transform);

        // EnvironmentArt object setup
        var parallaxBackgroundComponent = environmentArt.AddComponent<ParallaxBackground>();

        // ----------------------------------------------------------

        Debug.Log("Creating Background Layers GameObjects");
        Debug.Log("Background Layer Count: " + backgroundLayers);
        GameObject background = new GameObject("Background");
        background.transform.SetParent(environmentArt.transform);

        // Background object setup
        LayerTypeCollection backgroundTypeCollectionComponent = background.AddComponent<LayerTypeCollection>();

        for (int i = -backgroundLayers; i < 0; i++)
        {
            GameObject backgroundLayer = new GameObject($"Layer ({i})");
            backgroundLayer.transform.SetParent(background.transform);

            // Background layer setup
            OrderInLayer orderInLayerComponent = backgroundLayer.AddComponent<OrderInLayer>();
            orderInLayerComponent.SetOrderInLayerValue(i);

            ParallaxLayerController parallaxLayerComponent = backgroundLayer.AddComponent<ParallaxLayerController>();
            parallaxLayerComponent.SetParallaxFactor(-i / (backgroundLayers + 1f));

            backgroundTypeCollectionComponent.SetLayerType(LayerType.Background);
            backgroundTypeCollectionComponent.AddParallaxLayer(parallaxLayerComponent);

            parallaxBackgroundComponent.AddParallaxLayer(parallaxLayerComponent);
        }

        // ----------------------------------------------------------

        Debug.Log("Creating Center Layer GameObject");
        GameObject center = new GameObject("Center");
        center.transform.SetParent(environmentArt.transform);

        // Center gameobject setup
        LayerTypeCollection centerTypeCollectionComponent = center.AddComponent<LayerTypeCollection>();

        if (true)
        {
            GameObject centerLayer = new GameObject($"Layer (0)");
            centerLayer.transform.SetParent(center.transform);

            // Center layer setup
            OrderInLayer orderInLayerComponent = centerLayer.AddComponent<OrderInLayer>();
            orderInLayerComponent.SetOrderInLayerValue(0);

            ParallaxLayerController parallaxLayerComponent = centerLayer.AddComponent<ParallaxLayerController>();
            parallaxLayerComponent.SetParallaxFactor(0);

            centerTypeCollectionComponent.SetLayerType(LayerType.Center);
            centerTypeCollectionComponent.AddParallaxLayer(parallaxLayerComponent);

            parallaxBackgroundComponent.AddParallaxLayer(parallaxLayerComponent);
        }

        // ----------------------------------------------------------

        Debug.Log("Creating Foreground Layers GameObjects");
        Debug.Log("Foreground Layer Count: " + foregroundLayers);
        GameObject foreground = new GameObject("Foreground");
        foreground.transform.SetParent(environmentArt.transform);

        // Foreground object setup
        LayerTypeCollection foregroundTypeCollectionComponent = foreground.AddComponent<LayerTypeCollection>();

        for (int i = 1; i <= foregroundLayers; i++)
        {
            GameObject foregroundLayer = new GameObject($"Layer ({i})");
            foregroundLayer.transform.SetParent(foreground.transform);

            // Foreground layer setup
            OrderInLayer orderInLayerComponent = foregroundLayer.AddComponent<OrderInLayer>();
            orderInLayerComponent.SetOrderInLayerValue(i);

            ParallaxLayerController parallaxLayerComponent = foregroundLayer.AddComponent<ParallaxLayerController>();
            parallaxLayerComponent.SetParallaxFactor(-i / 10f);

            foregroundTypeCollectionComponent.SetLayerType(LayerType.Foreground);
            foregroundTypeCollectionComponent.AddParallaxLayer(parallaxLayerComponent);

            parallaxBackgroundComponent.AddParallaxLayer(parallaxLayerComponent);
        }

        // ----------------------------------------------------------

        _roomCreated = true;
        _window.Close();
    }

    private void Space(float value = DEFAULT_SPACE_VALUE)
    {
        GUILayout.Space(value);
    }

    private void SetWindowSize(Vector2 resolution)
    {
        var rect = position;
        rect.width = resolution.x;
        rect.height = resolution.y;
        position = rect;
    }
}
