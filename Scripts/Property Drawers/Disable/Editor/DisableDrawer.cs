using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisableAttribute))]
public class DisableDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label, property.isExpanded);
        EditorGUI.EndDisabledGroup();
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
	}
}