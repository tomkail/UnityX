using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(ClampAttribute))]
public class ClampDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		ClampAttribute clampAttribute = (ClampAttribute) attribute;
		
		if (property.propertyType == SerializedPropertyType.Float) {
			property.floatValue = EditorGUI.FloatField (position, label, Mathf.Clamp(property.floatValue, clampAttribute.minFloat, clampAttribute.maxFloat));
		}
		if (property.propertyType == SerializedPropertyType.Integer) {
			property.intValue = EditorGUI.IntField (position, label, Mathf.Clamp(property.intValue, clampAttribute.minInt, clampAttribute.maxInt));
		}
	}
}