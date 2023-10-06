using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LockAttribute))]
public class LockDrawer : BaseAttributePropertyDrawer<LockAttribute> {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginDisabledGroup(attribute.locked);
		EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - 50, position.height), property, label);
        EditorGUI.EndDisabledGroup();
		attribute.locked = GUI.Toggle(new Rect(position.x + (position.width - 40), position.y, 40, position.height), attribute.locked, "Lock", GUI.skin.button);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label);
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}