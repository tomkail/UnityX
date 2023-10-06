using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(FakeNullableAttribute))]
public class FakeNullableDrawer : BaseAttributePropertyDrawer<FakeNullableAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

	SerializedProperty GetPropertyFromPath (SerializedProperty property, string attributePath) {
		int lastDot = property.propertyPath.LastIndexOf('.');
		if(lastDot != -1) attributePath = property.propertyPath.Substring(0,lastDot+1) + attribute.boolBackingName;
		return property.serializedObject.FindProperty(attributePath);
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		SerializedProperty hasValueProperty = GetPropertyFromPath(property, attribute.boolBackingName);
		if(hasValueProperty == null || hasValueProperty.propertyType != SerializedPropertyType.Boolean || !hasValueProperty.boolValue) {
			return EditorGUIUtility.singleLineHeight;
		} else {
			return EditorGUI.GetPropertyHeight(property);
		}
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		SerializedProperty hasValueProperty = GetPropertyFromPath(property, attribute.boolBackingName);
		if(hasValueProperty == null) {
			EditorGUI.HelpBox(position, "No property was found at relative path "+attribute.boolBackingName, MessageType.Error);
			return;
		} else if (hasValueProperty.propertyType != SerializedPropertyType.Boolean) {
			EditorGUI.HelpBox(position, "Property was found at relative path is not of type bool "+attribute.boolBackingName, MessageType.Error);
			return;
		}

		if(hasValueProperty.boolValue) {
			Rect propertyRect = new Rect(position.x, position.y, position.width - 20, position.height);
			EditorGUI.PropertyField(propertyRect, property, label, property.hasVisibleChildren);
			Rect buttonRect = new Rect(position.x + position.width - 20, position.y, 20, position.height);
			if(GUI.Button(buttonRect, "X")) {
				hasValueProperty.boolValue = false;
			}
		} else {
			position = EditorGUI.PrefixLabel(position, label);
			if(GUI.Button(position, "Set Value")) {
				hasValueProperty.boolValue = true;
			}
		}

		EditorGUI.EndProperty();
	}
}