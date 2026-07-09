using UnityEngine;
using UnityEditor;
using System;

// Draws a string/int/float field as a popup whose options come from a sibling array property.
// Stateless — options and selection are recomputed from the properties each OnGUI.
[CustomPropertyDrawer(typeof(PropertyPopupAttribute))]
public class PropertyPopupDrawer : BaseAttributePropertyDrawer<PropertyPopupAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var subProperty = SerializedPropertyX.FindPropertyRelative(property, attribute.relativePropertyPath);
		if(subProperty == null) {
			EditorGUI.HelpBox(position, "No property found at path "+attribute.relativePropertyPath+"!", MessageType.Error);
			return;
		}

		int offset = attribute.addDefault ? 1 : 0;
		var list = new string[subProperty.arraySize + offset];
		if(attribute.addDefault) list[0] = "NONE";
		for(int i = offset; i < list.Length; i++) {
			list[i] = subProperty.GetArrayElementAtIndex(i - offset).GetValue().ToString();
		}

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
			bool setDefault = list.Length == 0 || (attribute.addDefault && selectedIndex == 0);
			switch (property.propertyType) {
				case SerializedPropertyType.String: property.stringValue = setDefault ? default : list[selectedIndex]; break;
				case SerializedPropertyType.Integer: property.intValue = setDefault ? default : Convert.ToInt32(list[selectedIndex]); break;
				default: property.floatValue = setDefault ? default : Convert.ToSingle(list[selectedIndex]); break;
			}
		}
		EditorGUI.EndProperty();
	}
}
