#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Compact one-line drawer for HexCoordVert: an X / Y int pair, mirroring HexCoordDrawer's Q / R layout.
[CustomPropertyDrawer(typeof(HexCoordVert))]
public class HexCoordVertDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var fields = property.Copy();
		fields.NextVisible(true); // step to first child (x)
		EditorGUI.MultiPropertyField(position, new GUIContent[] {
			new GUIContent("X"),
			new GUIContent("Y")
		}, fields, label);
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.wideMode
			? base.GetPropertyHeight(property, label)
			: base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
	}
}
#endif
