#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Compact one-line drawer for HexCoordVert: an X / Y int pair in the same style as HexCoordDrawer's Q / R.
[CustomPropertyDrawer(typeof(HexCoordVert))]
public class HexCoordVertDrawer : PropertyDrawer {
	public static readonly GUIContent XLabel = new GUIContent("X", "Corner-lattice x of this vertex.");
	public static readonly GUIContent YLabel = new GUIContent("Y", "Corner-lattice y of this vertex.");

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		HexEditorGUI.InlineFields(position,
			new[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y") },
			new[] { XLabel, YLabel });
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}
}
#endif
