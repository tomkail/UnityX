using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Inspector for the HexTester debug visualiser: which grid it's reading, live read-outs for the cell under the
// object (the same numbers the gizmos label), the draw toggles laid out compactly, the measuring tool with a
// distance read-out, and the colours tucked into a foldout.
[CustomEditor(typeof(HexTester)), CanEditMultipleObjects]
public class HexTesterEditor : Editor {
	const string coloursPref = "UnityX.HexGrid.HexTester.ColoursExpanded";

	static readonly GUIContent gridLabel = new GUIContent("Grid", "Grid to inspect. Leave empty to use the first WorldSpaceHexGrid in the open scenes.");
	static readonly GUIContent cellLabel = new GUIContent("Cell", "The cell under this object's position (axial q, r, s).");
	static readonly GUIContent offsetLabel = new GUIContent("Offset Cell", "The same cell in Unity's Grid (offset, odd-r) coordinates.");
	static readonly GUIContent closestLabel = new GUIContent("Closest", "Index of the corner and edge of the cell nearest to this object.");
	static readonly GUIContent targetLabel = new GUIContent("Target", "The cell to measure to.");
	static readonly GUIContent distanceLabel = new GUIContent("Distance", "Hex distance from the inspected cell to the target, plus the direction index when they're neighbours.");
	static readonly GUIContent toTargetLabel = new GUIContent("Move to Target", "Move this object onto the target cell's centre.");
	static readonly GUIContent targetHereLabel = new GUIContent("Target Here", "Set the target to the cell under this object.");
	static readonly GUIContent resetColoursLabel = new GUIContent("Reset Colours", "Restore the default gizmo colours.");

	SerializedProperty grid, drawNeighbours, labelCoord, labelCorners, labelEdges, showClosest, drawNeighbourVectors, measureToTarget, target_;
	SerializedProperty[] colours;

	void OnEnable () {
		grid = serializedObject.FindProperty("grid");
		drawNeighbours = serializedObject.FindProperty("drawNeighbours");
		labelCoord = serializedObject.FindProperty("labelCoord");
		labelCorners = serializedObject.FindProperty("labelCorners");
		labelEdges = serializedObject.FindProperty("labelEdges");
		showClosest = serializedObject.FindProperty("showClosest");
		drawNeighbourVectors = serializedObject.FindProperty("drawNeighbourVectors");
		measureToTarget = serializedObject.FindProperty("measureToTarget");
		target_ = serializedObject.FindProperty("target");
		colours = new[] {
			serializedObject.FindProperty("hexColor"),
			serializedObject.FindProperty("centerColor"),
			serializedObject.FindProperty("cornerColor"),
			serializedObject.FindProperty("edgeColor"),
			serializedObject.FindProperty("closestColor"),
		};
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		var tester = (HexTester)target;
		var resolved = Resolve(tester);
		bool single = !serializedObject.isEditingMultipleObjects;

		HexEditorGUI.Section("Grid", null, true);
		// ObjectField rather than PropertyField: the field's [Header] would repeat our section title.
		EditorGUILayout.ObjectField(grid, typeof(WorldSpaceHexGrid), gridLabel);
		if(single && tester.grid == null) {
			if(resolved != null) EditorGUILayout.LabelField(" ", $"Using {resolved.name} (first in scene)", HexEditorGUI.MiniLabel);
			else EditorGUILayout.HelpBox("No WorldSpaceHexGrid in the open scenes, so nothing is drawn.", MessageType.Warning);
		}

		if(single && resolved != null && resolved.grid != null) {
			HexEditorGUI.Section("Inspected Cell");
			var c = resolved.WorldToAxial(tester.transform.position);
			var offset = HexCoord.AxialToOffset(c);
			HexEditorGUI.ReadOut(cellLabel, $"({c.q}, {c.r}, {c.s})");
			HexEditorGUI.ReadOut(offsetLabel, $"({offset.x}, {offset.y})");
			Closest(resolved, c, tester.transform.position, out int corner, out int edge);
			HexEditorGUI.ReadOut(closestLabel, $"corner c{corner}, edge e{edge}");
		}

		HexEditorGUI.Section("Draw");
		ToggleRow(new GUIContent("Coord Label", "Label the cell with its coordinate."), labelCoord,
		          new GUIContent("Neighbour Cells", "Outline the six neighbouring cells."), drawNeighbours);
		ToggleRow(new GUIContent("Corner Labels", "Label corners c0..c5."), labelCorners,
		          new GUIContent("Edge Labels", "Label edge midpoints e0..e5."), labelEdges);
		ToggleRow(new GUIContent("Closest", "Mark the corner and edge closest to this object (tests GetCornerPosition / GetEdgePosition)."), showClosest,
		          new GUIContent("Direction Vectors", "Draw a labelled vector to each neighbour (tests AxialToWorldVector / Direction)."), drawNeighbourVectors);

		HexEditorGUI.Section("Measure");
		EditorGUILayout.PropertyField(measureToTarget, new GUIContent("Measure to Target", "Draw a line to the target cell with the hex distance."));
		bool measuring = measureToTarget.hasMultipleDifferentValues || measureToTarget.boolValue;
		using(new EditorGUI.DisabledScope(!measuring)) {
			EditorGUILayout.PropertyField(target_, targetLabel);
			if(single && resolved != null && resolved.grid != null) {
				var from = resolved.WorldToAxial(tester.transform.position);
				int distance = HexCoord.Distance(from, tester.target);
				string text = distance.ToString();
				if(distance == 1) text += $"   (direction {HexCoord.GetClosestDirectionIndex(from, tester.target)})";
				HexEditorGUI.ReadOut(distanceLabel, text);
				using(new EditorGUILayout.HorizontalScope()) {
					GUILayout.Space(EditorGUIUtility.labelWidth + 2f);
					if(GUILayout.Button(targetHereLabel, EditorStyles.miniButtonLeft)) {
						target_.FindPropertyRelative("q").intValue = from.q;
						target_.FindPropertyRelative("r").intValue = from.r;
					}
					if(GUILayout.Button(toTargetLabel, EditorStyles.miniButtonRight)) {
						Undo.RecordObject(tester.transform, "Move to Target");
						tester.transform.position = resolved.AxialToWorld(tester.target);
					}
				}
			}
		}

		EditorGUILayout.Space(6);
		bool expanded = EditorGUILayout.Foldout(SessionState.GetBool(coloursPref, false), "Colours", true, EditorStyles.foldoutHeader);
		SessionState.SetBool(coloursPref, expanded);
		if(expanded) {
			EditorGUI.indentLevel++;
			foreach(var colour in colours) ColourField(colour);
			using(new EditorGUILayout.HorizontalScope()) {
				GUILayout.FlexibleSpace();
				if(GUILayout.Button(resetColoursLabel, EditorStyles.miniButton, GUILayout.Width(110f))) ResetColours();
			}
			EditorGUI.indentLevel--;
		}

		serializedObject.ApplyModifiedProperties();
		if(GUI.changed) SceneView.RepaintAll();
	}

	// Two toggles on one line, each with its own label, so six booleans take three lines.
	static void ToggleRow (GUIContent leftLabel, SerializedProperty left, GUIContent rightLabel, SerializedProperty right) {
		var rect = EditorGUILayout.GetControlRect();
		float half = rect.width * 0.5f;
		ToggleLeft(new Rect(rect.x, rect.y, half - 4f, rect.height), leftLabel, left);
		ToggleLeft(new Rect(rect.x + half, rect.y, half, rect.height), rightLabel, right);
	}

	static void ToggleLeft (Rect rect, GUIContent label, SerializedProperty property) {
		EditorGUI.BeginProperty(rect, label, property);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		bool value = EditorGUI.ToggleLeft(rect, label, property.boolValue);
		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck()) property.boolValue = value;
		EditorGUI.EndProperty();
	}

	// Colour field without the property's decorators (hexColor carries a [Header] for the default inspector).
	static void ColourField (SerializedProperty property) {
		var rect = EditorGUILayout.GetControlRect();
		var label = new GUIContent(property.displayName, property.tooltip);
		EditorGUI.BeginProperty(rect, label, property);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		var value = EditorGUI.ColorField(rect, label, property.colorValue);
		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck()) property.colorValue = value;
		EditorGUI.EndProperty();
	}

	void ResetColours () {
		var defaults = new[] { Color.white, Color.yellow, Color.cyan, new Color(1f, 0.5f, 1f), new Color(0.2f, 1f, 0.4f) };
		for(int i = 0; i < colours.Length; i++) colours[i].colorValue = defaults[i];
	}

	static WorldSpaceHexGrid Resolve (HexTester tester) {
		return tester.grid != null ? tester.grid : Object.FindAnyObjectByType<WorldSpaceHexGrid>(FindObjectsInactive.Include);
	}

	// Same nearest-corner / nearest-edge search the gizmo uses.
	static void Closest (WorldSpaceHexGrid grid, HexCoord c, Vector3 probe, out int corner, out int edge) {
		corner = 0;
		edge = 0;
		float cornerD = float.MaxValue, edgeD = float.MaxValue;
		for(int i = 0; i < 6; i++) {
			float dc = (grid.GetCornerPosition(c, i) - probe).sqrMagnitude;
			if(dc < cornerD) { cornerD = dc; corner = i; }
			float de = (grid.GetEdgePosition(c, i) - probe).sqrMagnitude;
			if(de < edgeD) { edgeD = de; edge = i; }
		}
	}
}
