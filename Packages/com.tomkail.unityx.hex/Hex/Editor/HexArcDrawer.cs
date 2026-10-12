#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Drawer for HexArc: the start/steps fields beside a hexagon that highlights the covered sides, marks the start
// with a dot and the end with an arrow showing the direction of travel. Click a side to stretch the arc to it;
// Alt-click to move the start. Reverse walks the same sides the other way.
[CustomPropertyDrawer(typeof(HexArc))]
public class HexArcDrawer : PropertyDrawer {
	const float diagramSize = 58f;

	static readonly GUIContent startLabel = new GUIContent("Start", "initialDirectionIndex: the first direction covered (0..5; other values wrap).");
	static readonly GUIContent stepsLabel = new GUIContent("Steps", "signedSteps: how many directions are covered. Positive runs counter-clockwise (increasing index), negative clockwise, 0 covers nothing.");
	static readonly GUIContent reverseLabel = new GUIContent("Reverse", "Walk the same sides the other way: the arc starts where it used to end.");
	static readonly GUIContent ccwLabel = new GUIContent("↺", "Rotate the arc one side counter-clockwise.");
	static readonly GUIContent cwLabel = new GUIContent("↻", "Rotate the arc one side clockwise.");
	const string diagramTooltip = "Click a side to extend the arc to it. Alt-click a side to move the start there.";

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		var field = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);
		field.yMax = position.yMax;

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		var start = property.FindPropertyRelative("initialDirectionIndex");
		var steps = property.FindPropertyRelative("signedSteps");
		bool mixed = start.hasMultipleDifferentValues || steps.hasMultipleDifferentValues;
		var arc = new HexArc(start.intValue, steps.intValue);
		var covered = HexDiagram.ArcCoverage(arc.initialDirectionIndex, arc.signedSteps);

		var diagram = HexEditorGUI.DiagramRect(field, diagramSize);
		int clicked = HexEditorGUI.SideDiagram(diagram,
			i => mixed ? HexEditorGUI.SideState.Mixed : covered[i] ? HexEditorGUI.SideState.On : HexEditorGUI.SideState.Off,
			true, out _, diagramTooltip);
		if(!mixed && arc.arcLength > 0) {
			HexEditorGUI.DrawSideDot(diagram, HexDiagram.Mod(arc.initialDirectionIndex, 6), HexEditorGUI.Facing);
			HexEditorGUI.DrawTravelArrow(diagram, HexDiagram.Mod(arc.finalDirectionIndex, 6), arc.directionSign, HexEditorGUI.Accent);
		}
		if(clicked >= 0) {
			if(Event.current.alt || arc.arcLength == 0) {
				start.intValue = clicked;
				if(arc.arcLength == 0) steps.intValue = 1;
			} else {
				steps.intValue = HexDiagram.StepsToReach(arc.initialDirectionIndex, arc.signedSteps, clicked);
			}
		}

		float line = EditorGUIUtility.singleLineHeight, space = EditorGUIUtility.standardVerticalSpacing;
		var column = new Rect(diagram.xMax + 8f, field.y, Mathf.Max(0f, field.xMax - diagram.xMax - 8f), line);
		HexEditorGUI.InlineFields(column, new[] { start, steps }, new[] { startLabel, stepsLabel });

		column.y += line + space;
		EditorGUI.LabelField(column, mixed ? new GUIContent("Mixed values") : Summary(arc), HexEditorGUI.MiniLabel);

		column.y += line + space;
		float w = column.width / 4f;
		using(new EditorGUI.DisabledScope(mixed || arc.arcLength == 0)) {
			if(GUI.Button(new Rect(column.x, column.y, w * 2, line), reverseLabel, EditorStyles.miniButtonLeft)) {
				var reversed = HexDiagram.Reversed(arc);
				start.intValue = reversed.initialDirectionIndex;
				steps.intValue = reversed.signedSteps;
			}
			if(GUI.Button(new Rect(column.x + w * 2, column.y, w, line), ccwLabel, EditorStyles.miniButtonMid)) start.intValue = HexDiagram.Mod(arc.initialDirectionIndex + 1, 6);
			if(GUI.Button(new Rect(column.x + w * 3, column.y, w, line), cwLabel, EditorStyles.miniButtonRight)) start.intValue = HexDiagram.Mod(arc.initialDirectionIndex - 1, 6);
		}

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}

	static GUIContent Summary(HexArc arc) {
		if(arc.arcLength == 0) return new GUIContent("Empty (covers nothing)");
		string text = $"{HexDiagram.Mod(arc.initialDirectionIndex, 6)} → {HexDiagram.Mod(arc.finalDirectionIndex, 6)}, " +
		              $"{arc.arcLength} side{(arc.arcLength == 1 ? "" : "s")} {(arc.directionSign > 0 ? "CCW" : "CW")}";
		if(arc.arcLength > 6) text += " (wraps)";
		return new GUIContent(text, text);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return diagramSize;
	}
}
#endif
