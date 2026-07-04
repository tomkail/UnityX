using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InfoAttribute))]
public class InfoDrawer : BaseAttributePropertyDrawer<InfoAttribute> {
	// Measure the help box height from the actual message + available width so long multi-line text isn't clipped.
	private float GetHelpBoxHeight () {
		return EditorStyles.helpBox.CalcHeight(new GUIContent(attribute.info), EditorGUIUtility.currentViewWidth);
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label) + GetHelpBoxHeight();
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		float helpBoxHeight = GetHelpBoxHeight();
		EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, helpBoxHeight), attribute.info, MessageType.Info);
		EditorGUI.PropertyField(new Rect(position.x, position.y + helpBoxHeight, position.width, position.height-helpBoxHeight), property, label, true);
    }
}