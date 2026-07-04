using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(PropertyPopupAttribute))]
public class PropertyPopupDrawer : BaseAttributePropertyDrawer<PropertyPopupAttribute> {

	private Action<int> setValue;
	private Func<string> getValue;
	private Func<int, int> validateValue;

	private string[] list = null;

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var subProperty = SerializedPropertyX.FindPropertyRelative(property, attribute.relativePropertyPath);
		if(subProperty != null) {
			list = new string[subProperty.arraySize + (attribute.addDefault ? 1 : 0)];
			if(attribute.addDefault) {
				list[0] = "NONE";
			}
			for(int i = (attribute.addDefault ? 1 : 0); i < list.Length; i++) {
				list[i] = subProperty.GetArrayElementAtIndex(i - (attribute.addDefault ? 1 : 0)).GetValue().ToString();
			}

			SetUp(property);
			if (validateValue == null && setValue == null && getValue == null) {
				EditorGUI.HelpBox(position, "Popup drawer error.", MessageType.Error);
				return;
			}
			
			int selectedIndex = list.IndexOf(getValue());
			if(selectedIndex == -1) {
				selectedIndex = 0;
				setValue(selectedIndex);
			}

			for (int i = 0; i < list.Length; i++) {
				selectedIndex = validateValue(i);
				if (selectedIndex != 0)
					break;
			}
			
			EditorGUI.BeginChangeCheck();
			selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, list);
			if (EditorGUI.EndChangeCheck()) {
				setValue(selectedIndex);
			}
		} else {
			EditorGUI.HelpBox(position, "No property found at path "+attribute.relativePropertyPath+"!", MessageType.Error);
		}
	}

	
	void SetUp(SerializedProperty property) {
		if (property.propertyType == SerializedPropertyType.String) {
			validateValue = (index) => {
				return property.stringValue == list[index] ? index : 0;
			};
			setValue = (index) => {
				if(list.Length == 0 || attribute.addDefault && index == 0) property.stringValue = default(string);
				else property.stringValue = list[index];
			};
			getValue = () => {
				return property.stringValue;
			};
		} else if (property.propertyType == SerializedPropertyType.Integer) {
			validateValue = (index) => {
				return property.intValue == Convert.ToInt32(list[index]) ? index : 0;
			};
			setValue = (index) => {
				if(list.Length == 0 || attribute.addDefault && index == 0) property.intValue = default(int);
				else property.intValue = Convert.ToInt32(list[index]);
			};
			getValue = () => {
				return property.intValue.ToString();
			};
		} else if (property.propertyType == SerializedPropertyType.Float) {
			validateValue = (index) => {
				return property.floatValue == Convert.ToSingle(list[index]) ? index : 0;
			};
			setValue = (index) => {
				if(list.Length == 0 || attribute.addDefault && index == 0) property.floatValue = default(float);
				else property.floatValue = Convert.ToSingle(list[index]);
			};
			getValue = () => {
				return property.floatValue.ToString();
			};
		}
	}
}