using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public abstract class BaseIfAttributeDrawer<T> : BaseAttributePropertyDrawer<T> where T : BaseIfAttribute {

	protected string GetAbsoluteBoolPath (SerializedProperty property) {
		var absolutePath = property.propertyPath;
		if(absolutePath.EndsWith("]")) absolutePath = property.propertyPath.BeforeLast(".").BeforeLast(".").BeforeLast(".");
		else absolutePath = property.propertyPath.BeforeLast(".");
		absolutePath = absolutePath+"."+attribute.relativeBoolPath;
		return absolutePath;
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

	protected bool GetCondition (SerializedProperty property) {
		bool state = false;
		if(attribute.relativeBoolPath == null) {
			switch(attribute.option) {
			case BaseIfAttribute.Options.EditMode:
				state = !Application.isPlaying;
				break;
			case BaseIfAttribute.Options.PlayMode:
				state = Application.isPlaying;
				break;
			}
		}
		else state = ReflectionX.GetValueFromObject<bool>(property.serializedObject.targetObject as object, GetAbsoluteBoolPath(property));
		state = state ^ attribute.invert;
		return state;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		if(GetCondition(property)) OnGUITrue(position, property, label);
		else OnGUIFalse(position, property, label);

		EditorGUI.EndProperty();
	}

	public virtual void OnGUITrue (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUIX.DrawSerializedProperty(position, property);
	}
	public virtual void OnGUIFalse (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUIX.DrawSerializedProperty(position, property);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIX.GetPropertyHeight(property);
	}
}
