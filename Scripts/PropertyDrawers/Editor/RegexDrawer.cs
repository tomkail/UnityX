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
        label = EditorGUI.BeginProperty (position, label, prop);
        EditorGUI.BeginChangeCheck ();
        string val = EditorGUI.TextField (position, label, prop.stringValue);
        if (EditorGUI.EndChangeCheck ())
            prop.stringValue = val;
        EditorGUI.EndProperty ();
    }

    private void DrawHelpBox (Rect position) {
		// Adjust the help box position to appear indented underneath the text field.
		Rect helpPosition = EditorGUI.IndentedRect (position);
		helpPosition.y += textHeight;
		helpPosition.height = helpHeight;
		EditorGUI.HelpBox (helpPosition, attribute.helpMessage, MessageType.Error);
    }

    // Cache the compiled pattern (keyed by the pattern string) so we don't recompile every repaint,
    // and so an invalid pattern is reported at most once rather than throwing on every OnGUI.
    private Regex cachedRegex;
    private string cachedPattern;

    private Regex GetPatternRegex () {
        if (string.IsNullOrEmpty (attribute.pattern)) return null;
        if (cachedPattern != attribute.pattern) {
            cachedPattern = attribute.pattern;
            try {
                cachedRegex = new Regex (attribute.pattern);
            } catch (System.Exception e) {
                cachedRegex = null;
                Debug.LogWarning ("[Regex] Invalid pattern: " + attribute.pattern + "\n" + e.Message);
            }
        }
        return cachedRegex;
    }

    // Test if the propertys string value matches the regex pattern.
    private bool IsValid (SerializedProperty prop) {
        if (attribute.regex != null) {
            try {
                return attribute.regex.IsMatch (prop.stringValue);
            } catch (System.Exception) {
                // Fall through and treat as valid so we draw the field normally instead of spamming exceptions.
                return true;
            }
        }
        Regex regex = GetPatternRegex ();
        // No pattern (or an invalid one that failed to compile): treat as valid and draw the field normally.
        if (regex == null) return true;
        try {
            return regex.IsMatch (prop.stringValue) == attribute.showErrorWhenValid;
        } catch (System.Exception) {
            return true;
        }
    }

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}
}