using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexTerrainPrototype
{
    [CustomPropertyDrawer(typeof(Tile))]
    public class TileDrawer : PropertyDrawer
    {
        private const float _DEFAULT_FIELD_HEIGHT = 18f; // This is the default height of one property in the inspector. It's from the default implementation of PropertyDrawer.GetPropertyHeight().
        
        private const float _SPACING = 5f;
        private const float _PREVIEW_IMAGE_SIZE = 64f;
        private const float _PREVIEW_IMAGE_BORDER_SIZE = 1f;


        private Texture2D _CheckerBoardTexture;
        private Material _TransparentMat;
        
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetCheckerBoardTexture();
            
            
            EditorGUI.BeginProperty(position, label, property);

            Rect uiPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            // A property is 18 pixels tall by default, so we will use the max value of that and the image size to determine the size of our property UI.
            uiPosition.height = Mathf.Max(18f, _PREVIEW_IMAGE_SIZE);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float fieldWidth = uiPosition.width - (_PREVIEW_IMAGE_SIZE + _SPACING + _PREVIEW_IMAGE_BORDER_SIZE * 2f);
            Rect fieldRect = new Rect(uiPosition.x, uiPosition.y, fieldWidth, _DEFAULT_FIELD_HEIGHT + _PREVIEW_IMAGE_BORDER_SIZE * 2f);
            uiPosition.x += fieldWidth + _SPACING;

            Rect previewRect = new Rect(uiPosition.x + _PREVIEW_IMAGE_BORDER_SIZE, 
                                        uiPosition.y + _PREVIEW_IMAGE_BORDER_SIZE, 
                                        _PREVIEW_IMAGE_SIZE, 
                                        _PREVIEW_IMAGE_SIZE);

            Rect previewBorderRect = new Rect(uiPosition.x, 
                                              uiPosition.y, 
                _PREVIEW_IMAGE_SIZE + _PREVIEW_IMAGE_BORDER_SIZE * 2f, 
                _PREVIEW_IMAGE_SIZE + _PREVIEW_IMAGE_BORDER_SIZE * 2f);
            
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

            // Draw the texture preview.
            if (property.objectReferenceValue != null)
            {
                Tile tile = ((Tile) property.objectReferenceValue);
                
                if (_TransparentMat == null)
                {
                    // Get or create a material with a shader that supports alpha blending.
                    // The "UI/Unlit/Transparent" shader is a good choice for editor GUI.
                    _TransparentMat = new Material(Shader.Find("UI/Unlit/Transparent"));
                }

                if (tile != null && tile.sprite != null && _TransparentMat != null)
                {


                    // Define a GUIStyle for the border
                    GUIStyle borderStyle = new GUIStyle(GUI.skin.box);
                    borderStyle.normal.background = null; // Don't use a built-in background
                    borderStyle.border = new RectOffset(1, 1, 1, 1); // Set the border thickness

                    // Fill in the entire border preview rect with black.
                    EditorGUI.DrawRect(previewBorderRect, Color.black);
                    // Draw the checkerboard texture on top of all of the previewBorderRect except for the thickness of the border itself.
                    EditorGUI.DrawPreviewTexture(previewRect, _CheckerBoardTexture);
                    // Finally, draw the sprite preview on top of the background.
                    Texture2D preview = AssetPreview.GetAssetPreview(tile.sprite);
                    if (preview != null)
                    {
                        EditorGUI.DrawPreviewTexture(previewRect,
                            AssetPreview.GetAssetPreview(tile.sprite),
                            _TransparentMat);
                    }
                }
            }
            
            
            EditorGUI.indentLevel = indent;
            
            EditorGUI.EndProperty();
            
        }    
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Max(_DEFAULT_FIELD_HEIGHT, _PREVIEW_IMAGE_SIZE + _PREVIEW_IMAGE_BORDER_SIZE * 2f); // The _PREVIEW_IMAGE_BORDER_SIZE * 2f adds some extra space outside the borders of the image preview box. 
        }
        
        private void GetCheckerBoardTexture()
        {
            if (_CheckerBoardTexture != null)
            {
                return;
            }

            // Attempt to load the dark-theme checker texture.
            _CheckerBoardTexture = (Texture2D)EditorGUIUtility.Load("d_CheckerTexture");

            // If that fails, try the light-theme version.
            if (_CheckerBoardTexture == null)
            {
                _CheckerBoardTexture = (Texture2D)EditorGUIUtility.Load("CheckerTexture");
            }

            // As a final fallback, generate a texture manually.
            if (_CheckerBoardTexture == null)
            {
                _CheckerBoardTexture = new Texture2D(16, 16);
                _CheckerBoardTexture.hideFlags = HideFlags.HideAndDontSave;
                _CheckerBoardTexture.filterMode = FilterMode.Point;
            
                Color c0 = new Color(0.6f, 0.6f, 0.6f, 1f);
                Color c1 = new Color(0.5f, 0.5f, 0.5f, 1f);

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        _CheckerBoardTexture.SetPixel(x, y, c0);
                        _CheckerBoardTexture.SetPixel(x + 8, y, c1);
                        _CheckerBoardTexture.SetPixel(x, y + 8, c1);
                        _CheckerBoardTexture.SetPixel(x + 8, y + 8, c0);
                    }
                }
                _CheckerBoardTexture.Apply();
            }
        }        
    }
}
