#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// One-line drawer for HexCoord: Q and R as scrubbable int fields, plus the derived cube coordinate S (= -q-r)
// greyed out alongside, so the coord reads the way the maths treats it. Always a single line, wide or narrow.
[CustomPropertyDrawer(typeof (HexCoord))]
public class HexCoordDrawer : PropertyDrawer {
	public static readonly GUIContent QLabel = new GUIContent("Q", "Axial q. Stepping in direction 0 adds 1 to q.");
	public static readonly GUIContent RLabel = new GUIContent("R", "Axial r. Stepping in direction 1 adds 1 to r.");
	public static readonly GUIContent SLabel = new GUIContent("S", "Cube s, derived as -q - r (so q + r + s = 0). Read only.");

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		var q = property.FindPropertyRelative("q");
		var r = property.FindPropertyRelative("r");
		HexEditorGUI.InlineFields(position, new[] { q, r }, new[] { QLabel, RLabel }, 1, out var extra);
		HexEditorGUI.ReadOnlyInt(extra[0], SLabel, -q.intValue - r.intValue, q.hasMultipleDifferentValues || r.hasMultipleDifferentValues);
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}

	// --- Non-serialized drawing, for custom editors and windows that hold a HexCoord value directly ----------

	public static HexCoord Draw (Rect position, HexCoord coord) {
		return Draw(position, GUIContent.none, coord);
	}
	public static HexCoord Draw (Rect position, string label, HexCoord coord) {
		return Draw(position, new GUIContent(label), coord);
	}
	public static HexCoord Draw (Rect position, GUIContent label, HexCoord coord) {
		return Draw(position, label, coord, false);
	}
	// `mixed` greys every field to "-" (several objects with different coords); typing into one still applies it.
	public static HexCoord Draw (Rect position, GUIContent label, HexCoord coord, bool mixed) {
		position.height = EditorGUIUtility.singleLineHeight;
		if(label != GUIContent.none && !string.IsNullOrEmpty(label.text)) position = EditorGUI.PrefixLabel(position, label);

		int indent = EditorGUI.indentLevel;
		float labelWidth = EditorGUIUtility.labelWidth;
		EditorGUI.indentLevel = 0;
		const float gap = 4f;
		float w = (position.width - gap * 2) / 3f;
		var labels = new[] { QLabel, RLabel };
		var values = new[] { coord.q, coord.r };
		bool changed = false;
		for(int i = 0; i < 2; i++) {
			EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(labels[i]).x + 2f;
			EditorGUI.showMixedValue = mixed;
			EditorGUI.BeginChangeCheck();
			int v = EditorGUI.IntField(new Rect(position.x + (w + gap) * i, position.y, w, position.height), labels[i], values[i]);
			if(EditorGUI.EndChangeCheck()) { values[i] = v; changed = true; }
			EditorGUI.showMixedValue = false;
		}
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUI.indentLevel = indent;
		HexEditorGUI.ReadOnlyInt(new Rect(position.x + (w + gap) * 2, position.y, w, position.height), SLabel, -values[0] - values[1], mixed);

		if(changed) {
			GUI.changed = true;
			coord = new HexCoord(values[0], values[1]);
		}
		return coord;
	}

	public static HexCoord DrawLayout (string label, HexCoord coord) {
		return DrawLayout(new GUIContent(label), coord);
	}
	public static HexCoord DrawLayout (GUIContent label, HexCoord coord) {
		return Draw(EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight), label, coord);
	}
	public static HexCoord DrawLayout (HexCoord coord) {
		return Draw(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight), coord);
	}
}

#endif
