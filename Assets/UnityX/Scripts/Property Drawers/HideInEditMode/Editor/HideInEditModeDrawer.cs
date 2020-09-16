using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideInEditModeAttribute))]
public class HideInEditModeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(!Application.isPlaying) return;
        EditorGUIX.DrawSerializedProperty(position, property);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if(!Application.isPlaying) return -EditorGUIUtility.standardVerticalSpacing;
		else return EditorGUI.GetPropertyHeight(property, label);
	}
}