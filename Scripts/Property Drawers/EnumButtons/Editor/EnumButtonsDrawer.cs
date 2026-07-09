using UnityEngine;
using UnityEditor;

// Draws an enum as a single-select button strip instead of the default popup.
[CustomPropertyDrawer(typeof(EnumButtonsAttribute))]
public class EnumButtonsDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.Enum) {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        label = EditorGUI.BeginProperty(position, label, property);
        var buttonsRect = EditorGUI.PrefixLabel(position, label);

        var prevContentColor = GUI.contentColor;
        if (property.hasMultipleDifferentValues) GUI.contentColor *= new Color(1f, 1f, 1f, 0.5f);

        EditorGUI.BeginChangeCheck();
        int index = property.hasMultipleDifferentValues ? -1 : property.enumValueIndex;
        index = GUI.Toolbar(buttonsRect, index, property.enumDisplayNames);
        if (EditorGUI.EndChangeCheck() && index >= 0) property.enumValueIndex = index;

        GUI.contentColor = prevContentColor;
        EditorGUI.EndProperty();
    }
}
