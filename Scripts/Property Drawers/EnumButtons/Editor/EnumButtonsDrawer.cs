using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumButtonsAttribute))]
public class EnumButtonsDrawer : PropertyDrawer {
	private new EnumButtonsAttribute attribute {
		get {
			return (EnumButtonsAttribute)attribute;
		}
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType == SerializedPropertyType.Enum) {
			EditorGUI.PrefixLabel(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);
			int newVal = GUI.Toolbar (new Rect(position.x+EditorGUIUtility.labelWidth, position.y, position.width-EditorGUIUtility.labelWidth, position.height), property.enumValueIndex, property.enumNames);
			if(property.enumValueIndex != newVal) {
				property.enumValueIndex = newVal;
			}
        }
    }	
}