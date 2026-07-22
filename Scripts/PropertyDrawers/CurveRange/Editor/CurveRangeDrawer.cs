using UnityEditor;
using UnityEngine;

// Draws an AnimationCurve constrained to the attribute's range. Falls back to the default field for non-curve
// properties so a misapplied attribute is harmless.
[CustomPropertyDrawer(typeof(CurveRangeAttribute))]
public class CurveRangeDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (property.propertyType != SerializedPropertyType.AnimationCurve) {
			EditorGUI.PropertyField(position, property, label);
			return;
		}

		var attr = (CurveRangeAttribute)attribute;
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();
		var curve = EditorGUI.CurveField(position, label, property.animationCurveValue, attr.color, attr.ranges);
		if (EditorGUI.EndChangeCheck()) property.animationCurveValue = curve;
		EditorGUI.EndProperty();
	}
}
