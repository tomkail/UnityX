using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(OnChangeAttribute))]
public class OnChangeDrawer : BaseAttributePropertyDrawer<OnChangeAttribute> {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginChangeCheck();
		EditorGUI.PropertyField(position, property, label);
		if (EditorGUI.EndChangeCheck()) {
			if (IsMonoBehaviour(property)) {
				// Apply the edit BEFORE invoking the callbacks so they observe the new value rather than the pre-change one.
				property.serializedObject.ApplyModifiedProperties();
				MonoBehaviour mono = (MonoBehaviour)property.serializedObject.targetObject;
				foreach (var callbackName in attribute.callbackNames) {
					mono.Invoke(callbackName, 0);
				}

			}
		}
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}

	bool IsMonoBehaviour(SerializedProperty property) {
		return property.serializedObject.targetObject.GetType().IsSubclassOf(typeof(MonoBehaviour));
	}
}