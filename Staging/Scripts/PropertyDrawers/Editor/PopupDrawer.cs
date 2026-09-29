using UnityEngine;
using UnityEditor;
using System;

// Draws a string/int/float field as a popup of the values given in the attribute.
// Stateless: the previous version cached delegates closing over the first-seen SerializedProperty,
// so on a list every element read from and wrote to element 0.
[CustomPropertyDrawer(typeof(PopupAttribute))]
public class PopupDrawer : BaseAttributePropertyDrawer<PopupAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Float || property.propertyType == SerializedPropertyType.Integer;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var list = new string[attribute.list.Length];
		for (int i = 0; i < list.Length; i++) list[i] = attribute.list[i].ToString();

		string current = property.propertyType switch {
			SerializedPropertyType.String => property.stringValue,
			SerializedPropertyType.Integer => property.intValue.ToString(),
			_ => property.floatValue.ToString(),
		};
		int selectedIndex = Mathf.Max(0, Array.IndexOf(list, current));

		label = EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();
		selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, list);
		if (EditorGUI.EndChangeCheck()) {
			switch (property.propertyType) {
				case SerializedPropertyType.String: property.stringValue = list[selectedIndex]; break;
				case SerializedPropertyType.Integer: property.intValue = Convert.ToInt32(list[selectedIndex]); break;
				default: property.floatValue = Convert.ToSingle(list[selectedIndex]); break;
			}
		}
		EditorGUI.EndProperty();
	}
}
