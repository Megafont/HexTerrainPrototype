using UnityEditor;
using UnityEngine;

namespace HexTerrainPrototype.Data.Editor
{
    // NOTE: In Unity v6000.2.6f2, there is a bug with the inspector when a script has a field of type List.
    //       The inspector UI becomes very glitchy. To fix this, go to Project Settings. In the Editor section, enable “Use IMGUI Default Inspector,”
    [CustomPropertyDrawer(typeof(TerrainGenRule))]
    public class TerrainGenRuleDrawer : PropertyDrawer
    {
        private const float _SPACING = 10f;
        
        private const float _CLIMATE_LABEL_WIDTH = 55f;
        private const float _TERRAIN_TYPE_LABEL_WIDTH = 80f;
        private const float _PROBABILITY_LABEL_WIDTH = 70f;
        
        private const float _FIELD_WIDTH = 120f;
        private const float _INT_FIELD_WIDTH = 40f;


        private Rect uiPosition;

        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            uiPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            PropertyDrawerUtils.AddPropertyInHorizontalGroup(property, ref uiPosition, _CLIMATE_LABEL_WIDTH, "Climate:", _FIELD_WIDTH, "Climate", _SPACING);
            PropertyDrawerUtils.AddPropertyInHorizontalGroup(property, ref uiPosition, _TERRAIN_TYPE_LABEL_WIDTH, "Terrain Type:", _FIELD_WIDTH, "TerrainType", _SPACING);
            PropertyDrawerUtils.AddPropertyInHorizontalGroup(property, ref uiPosition, _PROBABILITY_LABEL_WIDTH, "Probability:", _INT_FIELD_WIDTH, "Probability", _SPACING);

            EditorGUI.indentLevel = indent;
            
            EditorGUI.EndProperty();
        }

        
    }
    
}
