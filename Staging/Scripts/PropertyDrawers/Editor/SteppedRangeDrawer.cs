using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer (typeof (SteppedRangeAttribute))]
public class SteppedRangeDrawer : BaseAttributePropertyDrawer<SteppedRangeAttribute> {
	
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		EditorGUI.Slider(position, property, attribute.min, attribute.max);
		// Intentionally snaps via the RoundToNearest extension. MinMaxDrawer snaps its two values via Round(v/step)*step instead — different math for different purposes, not copy-paste.
		property.floatValue = property.floatValue.RoundToNearest(attribute.step);
    }

    protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Float || property.propertyType == SerializedPropertyType.Integer;
	}
}