using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PlaceholderAttribute))]
public class PlaceholderDrawer : BaseAttributePropertyDrawer<PlaceholderAttribute> {
	
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight (property, label);
	}

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    	EditorGUI.BeginProperty(position, label, property);
		
		property.stringValue = EditorGUIX.TextField(position, label, property.stringValue, attribute.placeholder);

		EditorGUI.EndProperty();
    }
}