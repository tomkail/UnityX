#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// One-line drawer for HexCoordEdge: the two endpoint verts side by side as "S x y   E x y", so an edge reads
// at a glance instead of expanding into a nested start/end foldout.
[CustomPropertyDrawer(typeof(HexCoordEdge))]
public class HexCoordEdgeDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);

		var start = property.FindPropertyRelative("start");
		var end = property.FindPropertyRelative("end");

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float half = position.width * 0.5f;
		DrawVert(new Rect(position.x, position.y, half - 6, position.height), "S", start);
		DrawVert(new Rect(position.x + half, position.y, half, position.height), "E", end);

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}

	// Tag label (S/E) followed by two small int fields for x and y.
	static void DrawVert(Rect r, string tag, SerializedProperty vert) {
		if(vert == null) return;
		var x = vert.FindPropertyRelative("x");
		var y = vert.FindPropertyRelative("y");
		const float tagW = 14f;
		EditorGUI.LabelField(new Rect(r.x, r.y, tagW, r.height), tag);
		float fw = (r.width - tagW) * 0.5f;
		x.intValue = EditorGUI.IntField(new Rect(r.x + tagW, r.y, fw - 2, r.height), x.intValue);
		y.intValue = EditorGUI.IntField(new Rect(r.x + tagW + fw, r.y, fw - 2, r.height), y.intValue);
	}
}
#endif
