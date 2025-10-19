using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace HexTerrainPrototype
{
    public static class PropertyDrawerUtils
    {
        public static void AddPropertyInHorizontalGroup(SerializedProperty property, ref Rect uiPosition, float labelWidth, string labelText, float fieldWidth, string fieldPropertyName, float horizontalSpacing)
        {
            Rect labelRect = new Rect(uiPosition.x, uiPosition.y, labelWidth, uiPosition.height);
            uiPosition.x += labelWidth;
            
            Rect fieldRect = new Rect(uiPosition.x, uiPosition.y, fieldWidth, uiPosition.height);
            uiPosition.x += fieldWidth;
            
            EditorGUI.LabelField(labelRect, labelText);
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative(fieldPropertyName), GUIContent.none);
            
            // Add spacing after this field.
            uiPosition.x += horizontalSpacing;
        }
    }
}
