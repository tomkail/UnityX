using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LockAttribute))]
public class LockDrawer : BaseAttributePropertyDrawer<LockAttribute> {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// The attribute instance is shared by every field that uses [Lock], so per-field lock state
		// lives in SessionState, keyed by object + property path. attribute.locked is the initial state.
		string key = $"UnityX.Lock:{property.serializedObject.targetObject.GetEntityId()}:{property.propertyPath}";
		bool locked = SessionState.GetBool(key, attribute.locked);

		EditorGUI.BeginDisabledGroup(locked);
		EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - 50, position.height), property, label);
        EditorGUI.EndDisabledGroup();
		bool newLocked = GUI.Toggle(new Rect(position.x + (position.width - 40), position.y, 40, position.height), locked, "Lock", GUI.skin.button);
		if (newLocked != locked) SessionState.SetBool(key, newLocked);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label);
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}
