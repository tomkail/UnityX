using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

[CustomPropertyDrawer (typeof (RegexAttribute))]
public class RegexDrawer : BaseAttributePropertyDrawer<RegexAttribute> {
    
    const int helpHeight = 30;
    const int textHeight = 16;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if (IsSupported(property) && IsValid (property)) return base.GetPropertyHeight (property, label);
        else return base.GetPropertyHeight (property, label) + helpHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

        // Adjust height of the text field
        Rect textFieldPosition = position;
        textFieldPosition.height = textHeight;
        DrawTextField (textFieldPosition, property, label);

		if (!IsValid (property)) {
			DrawHelpBox (position);
		}
    }

    private void DrawTextField (Rect position, SerializedProperty prop, GUIContent label) {
        // Draw the text field control GUI.
        EditorGUI.BeginChangeCheck ();
        string val = EditorGUI.TextField (position, label, prop.stringValue);
        if (EditorGUI.EndChangeCheck ())
            prop.stringValue = val;
    }

    private void DrawHelpBox (Rect position) {
		// Adjust the help box position to appear indented underneath the text field.
		Rect helpPosition = EditorGUI.IndentedRect (position);
		helpPosition.y += textHeight;
		helpPosition.height = helpHeight;
		EditorGUI.HelpBox (helpPosition, attribute.helpMessage, MessageType.Error);
    }

    // Test if the propertys string value matches the regex pattern.
    private bool IsValid (SerializedProperty prop) {
        if(attribute.regex != null) return attribute.regex.IsMatch(prop.stringValue);
        else return Regex.IsMatch (prop.stringValue, attribute.pattern) == attribute.showErrorWhenValid;
    }

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
}