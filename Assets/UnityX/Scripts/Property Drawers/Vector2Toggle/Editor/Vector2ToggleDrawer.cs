using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Vector2ToggleAttribute))]
public class Vector2ToggleDrawer : BaseAttributePropertyDrawer<Vector2ToggleAttribute> {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var current = property.vector2Value;

		position = EditorGUI.PrefixLabel(position, label);

		var oneThird = Mathf.FloorToInt(position.width / 3);

		var xRect = new Rect(position.x, position.y, oneThird, position.height);
		var yRect = new Rect(position.x + oneThird, position.y, oneThird, position.height);

		var onX = EditorGUI.ToggleLeft(xRect, "X", current.x == 1);
		var onY = EditorGUI.ToggleLeft(yRect, "Y", current.y == 1);

		current.x = onX ? 1 : 0;
		current.y = onY ? 1 : 0;

		property.vector2Value = current;
	}

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Vector2;
	}
}