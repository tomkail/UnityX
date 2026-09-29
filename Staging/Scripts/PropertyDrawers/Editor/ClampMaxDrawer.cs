using UnityEditor;
using UnityEngine;

// Presents a normal edit box, but bounds the value to the attribute's maximum.
[CustomPropertyDrawer(typeof(ClampMaxAttribute))]
public class ClampMaxDrawer : PropertyDrawer {

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		var bound = attribute as ClampMaxAttribute;
		label = EditorGUI.BeginProperty(position, label, property);
		if (property.propertyType == SerializedPropertyType.Integer) {
			// Clamp what's displayed too, so an out-of-range serialized value heals — but only write on change.
			int clamped = Mathf.Min(EditorGUI.IntField(position, label, property.intValue), bound.IntBound);
			if (clamped != property.intValue) property.intValue = clamped;
		} else if (property.propertyType == SerializedPropertyType.Float) {
			float clamped = Mathf.Min(EditorGUI.FloatField(position, label, property.floatValue), bound.FloatBound);
			if (clamped != property.floatValue) property.floatValue = clamped;
		} else {
			EditorGUI.HelpBox(position, "[ClampMax] requires an int or float field.", MessageType.Error);
		}
		EditorGUI.EndProperty();
    }
}
