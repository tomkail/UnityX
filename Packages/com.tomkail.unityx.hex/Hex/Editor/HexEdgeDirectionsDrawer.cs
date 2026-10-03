#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Drawer for HexEdgeDirections: a small hexagon you click to toggle each side (side i faces Direction(i), drawn as
// seen from above on a default grid), with a summary of the set sides and All / None / rotate buttons beside it.
// Sides that differ across a multi-selection are dotted; clicking one sets it on everywhere.
[CustomPropertyDrawer(typeof(HexEdgeDirections))]
public class HexEdgeDirectionsDrawer : PropertyDrawer {
	const int numEdges = 6;
	const float diagramSize = 58f;

	static readonly GUIContent allLabel = new GUIContent("All", "Set all six sides.");
	static readonly GUIContent noneLabel = new GUIContent("None", "Clear all six sides.");
	static readonly GUIContent ccwLabel = new GUIContent("↺", "Rotate one side counter-clockwise (each set side i moves to i + 1).");
	static readonly GUIContent cwLabel = new GUIContent("↻", "Rotate one side clockwise (each set side i moves to i - 1).");
	const string diagramTooltip = "Click a side to toggle it. Side i faces HexCoord.Direction(i): 0 is right, counting counter-clockwise.";

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		var field = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);
		field.yMax = position.yMax;

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		var arr = property.FindPropertyRelative("_edgeDirections");
		if(arr == null || !arr.isArray) {
			EditorGUI.LabelField(field, "(no _edgeDirections)");
		} else if(arr.arraySize != numEdges) {
			DrawWrongSize(field, arr);
		} else {
			DrawEditor(field, property, arr);
		}

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}

	void DrawEditor(Rect field, SerializedProperty property, SerializedProperty arr) {
		var elements = new SerializedProperty[numEdges];
		for(int i = 0; i < numEdges; i++) elements[i] = arr.GetArrayElementAtIndex(i);

		var diagram = HexEditorGUI.DiagramRect(field, diagramSize);
		int clicked = HexEditorGUI.SideDiagram(diagram, i => StateOf(elements[i]), true, out _, diagramTooltip);
		if(clicked >= 0) elements[clicked].boolValue = StateOf(elements[clicked]) != HexEditorGUI.SideState.On;

		// Right-hand column: summary, then two button rows.
		float line = EditorGUIUtility.singleLineHeight, space = EditorGUIUtility.standardVerticalSpacing;
		var column = new Rect(diagram.xMax + 8f, field.y, Mathf.Max(0f, field.xMax - diagram.xMax - 8f), line);
		EditorGUI.LabelField(column, Summary(elements), HexEditorGUI.MiniLabel);

		column.y += line + space;
		float half = column.width * 0.5f;
		if(GUI.Button(new Rect(column.x, column.y, half, line), allLabel, EditorStyles.miniButtonLeft)) SetAll(elements, true);
		if(GUI.Button(new Rect(column.x + half, column.y, half, line), noneLabel, EditorStyles.miniButtonRight)) SetAll(elements, false);

		column.y += line + space;
		if(GUI.Button(new Rect(column.x, column.y, half, line), ccwLabel, EditorStyles.miniButtonLeft)) Rotate(property, arr, +1);
		if(GUI.Button(new Rect(column.x + half, column.y, half, line), cwLabel, EditorStyles.miniButtonRight)) Rotate(property, arr, -1);
	}

	static HexEditorGUI.SideState StateOf(SerializedProperty element) {
		if(element.hasMultipleDifferentValues) return HexEditorGUI.SideState.Mixed;
		return element.boolValue ? HexEditorGUI.SideState.On : HexEditorGUI.SideState.Off;
	}

	static GUIContent Summary(SerializedProperty[] elements) {
		var sides = new bool[numEdges];
		for(int i = 0; i < numEdges; i++) {
			if(elements[i].hasMultipleDifferentValues) return new GUIContent("Mixed values");
			sides[i] = elements[i].boolValue;
		}
		int count = 0;
		foreach(var s in sides) if(s) count++;
		string text = count == 0 ? "No sides" : count == numEdges ? "All sides" : (count == 1 ? "Side " : "Sides ") + HexDiagram.SidesToString(sides);
		return new GUIContent(text, text);
	}

	static void SetAll(SerializedProperty[] elements, bool value) {
		foreach(var element in elements) element.boolValue = value;
	}

	// Rotates each selected object's own sides (so a multi-selection with different values keeps its differences)
	// through a per-object SerializedObject, which keeps undo and prefab overrides intact.
	static void Rotate(SerializedProperty property, SerializedProperty arr, int offset) {
		var outer = property.serializedObject;
		outer.ApplyModifiedProperties();
		foreach(var target in outer.targetObjects) {
			var so = new SerializedObject(target);
			var a = so.FindProperty(arr.propertyPath);
			if(a == null || a.arraySize != numEdges) continue;
			var sides = new bool[numEdges];
			for(int i = 0; i < numEdges; i++) sides[i] = a.GetArrayElementAtIndex(i).boolValue;
			var rotated = HexDiagram.Rotated(sides, offset);
			for(int i = 0; i < numEdges; i++) a.GetArrayElementAtIndex(i).boolValue = rotated[i];
			so.ApplyModifiedProperties();
		}
		outer.Update();
	}

	static void DrawWrongSize(Rect field, SerializedProperty arr) {
		float line = EditorGUIUtility.singleLineHeight;
		var message = new Rect(field.x, field.y, field.width, line * 2);
		EditorGUI.HelpBox(message, $"Expected {numEdges} sides, found {arr.arraySize}.", MessageType.Warning);
		if(GUI.Button(new Rect(field.x, message.yMax + 2f, Mathf.Min(120f, field.width), line), "Resize to 6", EditorStyles.miniButton)) {
			arr.arraySize = numEdges;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return diagramSize;
	}
}
#endif
