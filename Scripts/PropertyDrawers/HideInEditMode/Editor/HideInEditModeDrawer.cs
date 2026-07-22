using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideInEditModeAttribute))]
public class HideInEditModeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if(!Application.isPlaying) return;
        EditorGUIX.DrawSerializedProperty(position, property);
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		// Returning the negative standard spacing when hidden is the deliberate row-collapse idiom: it cancels the inter-property spacing so the hidden field leaves ~0 net height (no blank gap or overlap).
		if(!Application.isPlaying) return -EditorGUIUtility.standardVerticalSpacing;
		else return EditorGUI.GetPropertyHeight(property, label);
	}
}