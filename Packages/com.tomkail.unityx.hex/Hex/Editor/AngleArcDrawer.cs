#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Drawer for AngleArc: a foldout whose header summarises the total angle, opening onto a dial that paints each range
// (0 = up, clockwise, as Util.Degrees) beside an editable From / To row per range. Ranges outside 0..360 or running
// backwards are flagged, since AngleArc.Set never produces them and Overlaps won't treat them sensibly.
[CustomPropertyDrawer(typeof(AngleArc))]
public class AngleArcDrawer : PropertyDrawer {
	const float dialSize = 64f;
	const float removeWidth = 20f;

	static readonly GUIContent fromLabel = new GUIContent("From", "Start of the range in degrees (0 = up, 90 = right).");
	static readonly GUIContent toLabel = new GUIContent("To", "End of the range in degrees; at least From, at most 360.");
	static readonly GUIContent removeLabel = new GUIContent("−", "Remove this range.");
	static readonly GUIContent addLabel = new GUIContent("Add Range", "Append a range. An arc that crosses 0/360 is stored as two ranges.");
	static readonly GUIContent fullLabel = new GUIContent("Full Circle", "Replace the ranges with a single 0..360 range.");
	static readonly GUIContent dialTooltip = new GUIContent(string.Empty, "Covered angles, 0 at the top running clockwise. The faint hexagon shows the six hex directions.");

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		float line = EditorGUIUtility.singleLineHeight, space = EditorGUIUtility.standardVerticalSpacing;
		var ranges = property.FindPropertyRelative("ranges");

		var header = new Rect(position.x, position.y, position.width, line);
		property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, label, true);
		var summaryRect = new Rect(header.x + EditorGUIUtility.labelWidth, header.y, header.width - EditorGUIUtility.labelWidth, line);
		EditorGUI.LabelField(summaryRect, Summary(ranges), HexEditorGUI.Summary);

		if(property.isExpanded && ranges != null) {
			EditorGUI.indentLevel++;
			var body = EditorGUI.IndentedRect(new Rect(position.x, header.yMax + space, position.width, position.yMax - header.yMax - space));
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var dial = new Rect(body.x, body.y, dialSize, dialSize);
			DrawDial(dial, ranges);
			GUI.Label(dial, dialTooltip);

			var row = new Rect(dial.xMax + 8f, body.y, Mathf.Max(0f, body.xMax - dial.xMax - 8f), line);
			for(int i = 0; i < ranges.arraySize; i++) {
				var element = ranges.GetArrayElementAtIndex(i);
				var fields = new Rect(row.x, row.y, row.width - removeWidth - 4f - 18f, line);
				HexEditorGUI.InlineFields(fields, new[] { element.FindPropertyRelative("x"), element.FindPropertyRelative("y") }, new[] { fromLabel, toLabel });
				if(!element.hasMultipleDifferentValues && !HexDiagram.IsValidAngleRange(element.vector2Value)) {
					var icon = EditorGUIUtility.IconContent("console.warnicon.sml");
					GUI.Label(new Rect(fields.xMax + 1f, row.y, 18f, line), new GUIContent(icon.image, "Out of range: expected 0 <= From <= To <= 360."));
				}
				if(GUI.Button(new Rect(row.xMax - removeWidth, row.y, removeWidth, line), removeLabel, EditorStyles.miniButton)) {
					ranges.DeleteArrayElementAtIndex(i);
					break;
				}
				row.y += line + space;
			}
			float half = row.width * 0.5f;
			if(GUI.Button(new Rect(row.x, row.y, half, line), addLabel, EditorStyles.miniButtonLeft)) {
				ranges.arraySize++;
				ranges.GetArrayElementAtIndex(ranges.arraySize - 1).vector2Value = new Vector2(0f, 60f);
			}
			if(GUI.Button(new Rect(row.x + half, row.y, half, line), fullLabel, EditorStyles.miniButtonRight)) {
				ranges.arraySize = 1;
				ranges.GetArrayElementAtIndex(0).vector2Value = new Vector2(0f, 360f);
			}

			EditorGUI.indentLevel = indent;
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
	}

	static GUIContent Summary(SerializedProperty ranges) {
		if(ranges == null) return GUIContent.none;
		if(ranges.hasMultipleDifferentValues) return new GUIContent("Mixed values");
		var values = new Vector2[ranges.arraySize];
		for(int i = 0; i < values.Length; i++) values[i] = ranges.GetArrayElementAtIndex(i).vector2Value;
		if(values.Length == 0) return new GUIContent("Empty");
		float total = HexDiagram.TotalDegrees(values);
		return new GUIContent($"{total:0.#}° in {values.Length} range{(values.Length == 1 ? "" : "s")}");
	}

	// GUI space is y-down, so rotating about +Z turns clockwise on screen - the same way AngleArc's degrees run.
	static void DrawDial(Rect rect, SerializedProperty ranges) {
		if(Event.current.type != EventType.Repaint) return;
		var centre = (Vector3)rect.center;
		float radius = rect.width * 0.5f - 2f;
		var up = Vector3.down;

		Handles.color = HexEditorGUI.Fill;
		Handles.DrawSolidDisc(centre, Vector3.forward, radius);

		// Faint pointy hexagon for reference: its sides face the six hex directions.
		var hex = new Vector3[7];
		for(int i = 0; i < 6; i++) hex[i] = HexDiagram.Corner(centre, radius * 0.62f, i);
		hex[6] = hex[0];
		Handles.color = HexEditorGUI.WithAlpha(HexEditorGUI.Line, HexEditorGUI.Line.a * 0.5f);
		Handles.DrawAAPolyLine(1f, hex);

		for(int i = 0; i < ranges.arraySize; i++) {
			var element = ranges.GetArrayElementAtIndex(i);
			if(element.hasMultipleDifferentValues) continue;
			var range = element.vector2Value;
			bool valid = HexDiagram.IsValidAngleRange(range);
			var from = Quaternion.AngleAxis(range.x, Vector3.forward) * up;
			float sweep = Mathf.Clamp(range.y - range.x, -360f, 360f);
			var color = valid ? HexEditorGUI.Accent : HexEditorGUI.Warning;
			Handles.color = HexEditorGUI.WithAlpha(color, 0.3f);
			Handles.DrawSolidArc(centre, Vector3.forward, from, sweep, radius);
			Handles.color = color;
			Handles.DrawWireArc(centre, Vector3.forward, from, sweep, radius, 2.5f);
		}

		Handles.color = HexEditorGUI.Line;
		Handles.DrawWireDisc(centre, Vector3.forward, radius, 1f);
		// Tick at 0 (up) so the dial's origin is unambiguous.
		Handles.DrawAAPolyLine(2f, centre + up * (radius - 5f), centre + up * (radius + 1f));
		Handles.color = Color.white;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		float line = EditorGUIUtility.singleLineHeight, space = EditorGUIUtility.standardVerticalSpacing;
		if(!property.isExpanded) return line;
		var ranges = property.FindPropertyRelative("ranges");
		int rows = (ranges != null ? ranges.arraySize : 0) + 1;
		return line + space + Mathf.Max(dialSize, rows * (line + space) - space);
	}
}
#endif
