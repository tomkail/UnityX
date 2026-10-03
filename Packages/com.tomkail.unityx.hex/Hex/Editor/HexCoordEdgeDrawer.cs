#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// One-line drawer for HexCoordEdge: the two endpoint verts side by side, "From x y  To x y", so an edge reads at a
// glance instead of expanding into a nested start/end foldout. Edges are undirected, so the order is cosmetic.
[CustomPropertyDrawer(typeof(HexCoordEdge))]
public class HexCoordEdgeDrawer : PropertyDrawer {
	static readonly GUIContent StartLabel = new GUIContent("From", "Start vertex (x, y). Edges are undirected: swapping the ends gives an equal edge.");
	static readonly GUIContent EndLabel = new GUIContent("To", "End vertex (x, y). Edges are undirected: swapping the ends gives an equal edge.");

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		const float gap = 8f;
		float half = (position.width - gap) * 0.5f;
		DrawVert(new Rect(position.x, position.y, half, position.height), StartLabel, property.FindPropertyRelative("start"));
		DrawVert(new Rect(position.x + half + gap, position.y, half, position.height), EndLabel, property.FindPropertyRelative("end"));
		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}

	// Dim tag ("From"/"To") followed by the vert's X / Y fields.
	static void DrawVert(Rect r, GUIContent tag, SerializedProperty vert) {
		if(vert == null) return;
		float tagWidth = EditorStyles.miniLabel.CalcSize(tag).x + 2f;
		EditorGUI.LabelField(new Rect(r.x, r.y, tagWidth, r.height), tag, HexEditorGUI.MiniLabel);
		HexEditorGUI.InlineFields(new Rect(r.x + tagWidth, r.y, r.width - tagWidth, r.height),
			new[] { vert.FindPropertyRelative("x"), vert.FindPropertyRelative("y") },
			new[] { HexCoordVertDrawer.XLabel, HexCoordVertDrawer.YLabel });
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return EditorGUIUtility.singleLineHeight;
	}
}
#endif
