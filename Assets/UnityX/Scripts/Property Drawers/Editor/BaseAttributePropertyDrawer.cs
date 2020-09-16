using UnityEngine;
using UnityEditor;

public abstract class BaseAttributePropertyDrawer<T> : PropertyDrawer where T : PropertyAttribute {

	protected new T attribute {
        get {
        	return (T)base.attribute;
        }
    }

	protected abstract bool IsSupported (SerializedProperty property);
	protected virtual void DrawNotSupportedGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.HelpBox(position, "Type "+property.propertyType+" is not supported with this property attribute", MessageType.Error);
	}
}
