#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Drawer for HexEdgeDirections: six inline toggles labelled 0..5 (one per edge direction) instead of the
// default expandable bool[] array, so a set of edge directions reads and edits on a single line.
[CustomPropertyDrawer(typeof(HexEdgeDirections))]
public class HexEdgeDirectionsDrawer : PropertyDrawer {

	const int numEdges = 6;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);

		var arr = property.FindPropertyRelative("_edgeDirections");
		if(arr == null || !arr.isArray) {
			EditorGUI.LabelField(position, "(no _edgeDirections)");
			EditorGUI.EndProperty();
			return;
		}

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float w = position.width / numEdges;
		for(int i = 0; i < numEdges; i++) {
			var cell = new Rect(position.x + w * i, position.y, w, position.height);
			if(i < arr.arraySize) {
				var element = arr.GetArrayElementAtIndex(i);
				element.boolValue = EditorGUI.ToggleLeft(cell, i.ToString(), element.boolValue);
			}
		}

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}
}
#endif
