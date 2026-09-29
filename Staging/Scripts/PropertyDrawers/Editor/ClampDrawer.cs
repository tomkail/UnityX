using UnityEditor;
using UnityEngine;

// Presents a normal edit box, but clamps the value to the attribute's range.
[CustomPropertyDrawer(typeof(ClampAttribute))]
public class ClampDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		ClampAttribute clampAttribute = (ClampAttribute) attribute;
		label = EditorGUI.BeginProperty(position, label, property);
		if (property.propertyType == SerializedPropertyType.Float) {
			// Clamp what's displayed too, so an out-of-range serialized value heals — but only write on change.
			float clamped = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), clampAttribute.minFloat, clampAttribute.maxFloat);
			if (clamped != property.floatValue) property.floatValue = clamped;
		} else if (property.propertyType == SerializedPropertyType.Integer) {
			int clamped = Mathf.Clamp(EditorGUI.IntField(position, label, property.intValue), clampAttribute.minInt, clampAttribute.maxInt);
			if (clamped != property.intValue) property.intValue = clamped;
		} else {
			EditorGUI.HelpBox(position, "[Clamp] requires an int or float field.", MessageType.Error);
		}
		EditorGUI.EndProperty();
	}
}
