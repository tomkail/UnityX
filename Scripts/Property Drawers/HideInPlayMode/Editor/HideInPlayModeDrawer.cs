using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideInPlayModeAttribute))]
public class HideInPlayModeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(Application.isPlaying) return;
        EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if(Application.isPlaying) return 0;
		else return EditorGUI.GetPropertyHeight(property, label);
	}
}