using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(ArrayIndexSliderAttribute))]
public class ArrayIndexSliderDrawer : BaseAttributePropertyDrawer<ArrayIndexSliderAttribute> {
	protected override bool IsSupported (SerializedProperty property) {
		var subProperty = SerializedPropertyX.FindPropertyRelative(property, attribute.relativePropertyPath);
		return property.propertyType == SerializedPropertyType.Integer && subProperty != null && subProperty.isArray;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		EditorGUI.BeginProperty(position, label, property);
		var subProperty = SerializedPropertyX.FindPropertyRelative(property, attribute.relativePropertyPath);
		if(subProperty != null) {
			EditorGUI.BeginDisabledGroup(subProperty.arraySize == 0);
			if(subProperty.arraySize <= 1) {
				property.intValue = 0;
				EditorGUI.IntSlider(position, label.text, property.intValue, 0, 0);
			} else {
				property.intValue = EditorGUI.IntSlider(position, label.text, property.intValue, 0, subProperty.arraySize-1);
			}
			EditorGUI.EndDisabledGroup();
		} else {
			EditorGUI.HelpBox(position, "No property found at path "+attribute.relativePropertyPath+"!", MessageType.Error);
		}
        EditorGUI.EndProperty();

	}

	protected override void DrawNotSupportedGUI (Rect position, SerializedProperty property, GUIContent label) {
		if(property.propertyType == SerializedPropertyType.Integer) EditorGUI.HelpBox(position, "Type "+property.propertyType+" is not supported with this property attribute", MessageType.Error);
		else {
			var subProperty = SerializedPropertyX.FindPropertyRelative(property, attribute.relativePropertyPath);
			if(subProperty == null) EditorGUI.HelpBox(position, "Target property at path "+attribute.relativePropertyPath+" not found", MessageType.Error);
			else if(!subProperty.isArray) EditorGUI.HelpBox(position, "Target property "+attribute.relativePropertyPath+" must be an array", MessageType.Error);
		}
	}
}