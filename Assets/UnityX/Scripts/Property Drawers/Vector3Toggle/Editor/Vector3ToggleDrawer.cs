using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Vector3ToggleAttribute))]
public class Vector3ToggleDrawer : BaseAttributePropertyDrawer<Vector3ToggleAttribute> {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var current = property.vector3Value;

		position = EditorGUI.PrefixLabel(position, label);

		var oneThird = Mathf.FloorToInt(position.width / 3);

		var xRect = new Rect(position.x, position.y, oneThird, position.height);
		var yRect = new Rect(position.x + oneThird, position.y, oneThird, position.height);
		var zRect = new Rect(position.x + 2 * oneThird, position.y, oneThird, position.height);

		var onX = EditorGUI.ToggleLeft(xRect, "X", current.x == 1);
		var onY = EditorGUI.ToggleLeft(yRect, "Y", current.y == 1);
		var onZ = EditorGUI.ToggleLeft(zRect, "Z", current.z == 1);

		current.x = onX ? 1 : 0;
		current.y = onY ? 1 : 0;
		current.z = onZ ? 1 : 0;

		property.vector3Value = current;
	}

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Vector3;
	}
}