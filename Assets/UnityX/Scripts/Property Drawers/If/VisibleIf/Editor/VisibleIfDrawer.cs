using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(VisibleIfAttribute))]
public class VisibleIfAttributeDrawer : BaseIfAttributeDrawer<VisibleIfAttribute> {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		bool hasValue = GetCondition(property);
		if(hasValue) {
			return EditorGUI.GetPropertyHeight(property);
		} else {
			return - EditorGUIUtility.standardVerticalSpacing;
		}
	}

	public override void OnGUITrue (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginDisabledGroup(attribute.disable);
		base.OnGUITrue(position, property, label);
		EditorGUI.EndDisabledGroup();
	}

	public override void OnGUIFalse (Rect position, SerializedProperty property, GUIContent label) {}   
}