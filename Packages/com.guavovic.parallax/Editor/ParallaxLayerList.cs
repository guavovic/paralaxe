using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Guavovic.Parallax.Editor
{
    /// <summary>
    /// Lista compacta das camadas: miniatura, nome, indicador de distância e visibilidade.
    /// Arrastar muda a ordem; clicar seleciona.
    /// </summary>
    internal sealed class ParallaxLayerList
    {
        private const float RowHeight = 38f;
        private const int Dots = 5;

        private static readonly Color DotOn = new Color(0.55f, 0.8f, 1f);
        private static readonly Color DotOff = new Color(1f, 1f, 1f, 0.15f);

        private readonly ParallaxRig _rig;
        private readonly SerializedObject _profileObject;
        private readonly ReorderableList _list;
        private ParallaxLayer[] _objects = new ParallaxLayer[0];

        public ParallaxLayerList(ParallaxRig rig, SerializedObject profileObject)
        {
            _rig = rig;
            _profileObject = profileObject;
            _list = new ReorderableList(profileObject, profileObject.FindProperty("layers"), true, false, false, false)
            {
                elementHeight = RowHeight,
                drawElementCallback = DrawRow,
                onReorderCallbackWithDetails = (list, from, to) =>
                {
                    _profileObject.ApplyModifiedProperties();
                    ParallaxLayerCommands.ReorderDrawing(_rig, _objects, from, to);
                    ParallaxLayerCommands.RemapAfterMove(_rig, from, to);
                }
            };
        }

        public int Selected
        {
            get => _list.index;
            set => _list.index = value;
        }

        public ParallaxLayer ObjectAt(int index)
        {
            return index >= 0 && index < _objects.Length ? _objects[index] : null;
        }

        public void Draw()
        {
            if (Event.current.type == EventType.Layout)
                _objects = FindLayerObjects(_rig, _list.serializedProperty.arraySize);

            if (_list.index >= _list.serializedProperty.arraySize)
                _list.index = _list.serializedProperty.arraySize - 1;

            _list.DoLayoutList();
        }

        /// <summary>
        /// O objeto de cada configuração, pelo índice. Fica nulo quando a configuração não tem objeto na cena.
        /// </summary>
        public static ParallaxLayer[] FindLayerObjects(ParallaxRig rig, int count)
        {
            var result = new ParallaxLayer[count];
            foreach (var layer in rig.GetComponentsInChildren<ParallaxLayer>(true))
            {
                if (layer.SettingsIndex >= 0 && layer.SettingsIndex < count && result[layer.SettingsIndex] == null)
                    result[layer.SettingsIndex] = layer;
            }

            return result;
        }

        public static void DrawThumbnail(Rect rect, ParallaxLayer layer)
        {
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
            var spriteRenderer = layer != null ? layer.GetComponentInChildren<SpriteRenderer>(true) : null;
            var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
            if (sprite == null || sprite.texture == null)
                return;

            // Sprite compactado em atlas não tem um retângulo na textura; usa a prévia da Unity.
            if (sprite.packed && sprite.packingMode == SpritePackingMode.Tight)
            {
                var previewTexture = AssetPreview.GetAssetPreview(sprite);
                if (previewTexture != null)
                    GUI.DrawTexture(rect, previewTexture, ScaleMode.ScaleToFit);
                return;
            }

            var texture = sprite.texture;
            var area = sprite.textureRect;
            var uv = new Rect(area.x / texture.width, area.y / texture.height, area.width / texture.width, area.height / texture.height);

            // Mantém a proporção da imagem dentro do retângulo.
            float aspect = area.width / area.height;
            var fit = rect;
            if (aspect > rect.width / rect.height)
            {
                fit.height = rect.width / aspect;
                fit.y += (rect.height - fit.height) * 0.5f;
            }
            else
            {
                fit.width = rect.height * aspect;
                fit.x += (rect.width - fit.width) * 0.5f;
            }

            GUI.DrawTextureWithTexCoords(fit, texture, uv);
        }

        private void DrawRow(Rect rect, int index, bool active, bool focused)
        {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            var layer = index < _objects.Length ? _objects[index] : null;
            rect.y += 3f;
            rect.height -= 6f;

            DrawThumbnail(new Rect(rect.x, rect.y, rect.height * 1.6f, rect.height), layer);

            float eyeWidth = 22f;
            float dotsWidth = Dots * 9f;
            var nameRect = new Rect(rect.x + rect.height * 1.6f + 6f, rect.y, rect.width - rect.height * 1.6f - dotsWidth - eyeWidth - 14f, rect.height);
            EditorGUI.LabelField(nameRect, element.FindPropertyRelative("name").stringValue, EditorStyles.label);

            float distance = ParallaxDistance.Normalized(ParallaxDistance.Get(_rig.Profile, element));
            DrawDots(new Rect(nameRect.xMax + 4f, rect.y + rect.height * 0.5f - 3f, dotsWidth, 6f), distance);

            if (layer != null)
                DrawEye(new Rect(rect.xMax - eyeWidth, rect.y + rect.height * 0.5f - 9f, eyeWidth, 18f), layer.gameObject);
        }

        private static void DrawDots(Rect rect, float distance)
        {
            // Da esquerda (perto) para a direita (longe).
            int lit = Mathf.Clamp(Mathf.RoundToInt(distance * (Dots - 1)), 0, Dots - 1);
            for (int i = 0; i < Dots; i++)
                EditorGUI.DrawRect(new Rect(rect.x + i * 9f, rect.y, 6f, 6f), i == lit ? DotOn : DotOff);
        }

        private static void DrawEye(Rect rect, GameObject layerObject)
        {
            bool visible = layerObject.activeSelf;
            var icon = EditorGUIUtility.IconContent(visible ? "scenevis_visible_hover" : "scenevis_hidden_hover");
            icon.tooltip = visible ? "Desligar a camada" : "Ligar a camada";

            if (!GUI.Button(rect, icon, EditorStyles.iconButton))
                return;

            Undo.RecordObject(layerObject, visible ? "Desligar camada" : "Ligar camada");
            layerObject.SetActive(!visible);
        }
    }
}
