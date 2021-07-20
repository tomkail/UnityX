using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InfoAttribute))]
public class InfoDrawer : BaseAttributePropertyDrawer<InfoAttribute> {
	private int helpBoxHeight = 38;
	
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label) + helpBoxHeight;
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, helpBoxHeight), attribute.info, MessageType.Info);
		EditorGUI.PropertyField(new Rect(position.x, position.y + helpBoxHeight, position.width, position.height-helpBoxHeight), property, label, true);
    }
}