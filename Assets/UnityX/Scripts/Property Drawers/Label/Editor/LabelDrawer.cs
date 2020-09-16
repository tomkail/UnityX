using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : BaseAttributePropertyDrawer<LabelAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if(attribute.label == null) {
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        } else {
            label = new GUIContent(attribute.label);
            EditorGUI.PropertyField(position, property, label, true);
        }
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}