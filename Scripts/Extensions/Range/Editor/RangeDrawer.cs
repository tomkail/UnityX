using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer (typeof (Range))]
public class RangeDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);

		var mainLabelWidth = EditorGUIUtility.labelWidth;

		EditorGUI.LabelField(new Rect(position.x, position.y, mainLabelWidth, position.height), property.displayName);

		float valueX = mainLabelWidth;
		float valueWidth = position.width - mainLabelWidth;

		float compWidth = 0.5f * valueWidth;

		EditorGUIUtility.labelWidth = 45.0f;
		EditorGUI.PropertyField(new Rect(valueX,             position.y, compWidth, position.height), property.FindPropertyRelative ("min"));
		EditorGUI.PropertyField(new Rect(valueX + compWidth, position.y, compWidth, position.height), property.FindPropertyRelative ("max"));
		EditorGUIUtility.labelWidth = mainLabelWidth;

		EditorGUI.EndProperty();
	}
}