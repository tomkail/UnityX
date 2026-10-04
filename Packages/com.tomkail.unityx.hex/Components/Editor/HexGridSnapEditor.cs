using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Inspector for HexGridSnap (and subclasses without their own editor). The stored cell + facing are the component's
// real data but are hidden fields, so this surfaces them: an editable cell, a clickable facing hexagon, live
// read-outs, and a warning (with fixes) when the transform has been moved off its stored cell with the regular
// tools. In the Scene view it adds a light-touch handle: drag the centre dot to move cell by cell, click an edge dot
// to face that way. Every change records the component and its Transform for undo.
[CustomEditor(typeof(HexGridSnap), true), CanEditMultipleObjects]
public class HexGridSnapEditor : Editor {
	const string handlesPref = "UnityX.HexGrid.HexGridSnap.SceneHandles";
	const float diagramSize = 58f;

	static readonly GUIContent positionLabel = new GUIContent("Snap Position", "Place the transform on the stored cell's centre.");
	static readonly GUIContent rotationLabel = new GUIContent("Snap Rotation", "Rotate the transform to face the stored direction.");
	static readonly GUIContent findModeLabel = new GUIContent("Find Grid", "How the grid is found. Parent: the nearest WorldSpaceHexGrid on this object or its parents. Manual: the grid assigned below. Master: resolved by a project subclass (override GetGrid).");
	static readonly GUIContent gridLabel = new GUIContent("Grid", "The WorldSpaceHexGrid this object snaps to.");
	static readonly GUIContent resolvedLabel = new GUIContent("Grid", "The grid found by the current Find Grid mode (read only).");
	static readonly GUIContent cellLabel = new GUIContent("Cell", "The stored cell (axial q, r). This is the authoritative position: the transform is derived from it.");
	static readonly GUIContent facingLabel = new GUIContent("Facing", "The stored facing: direction index 0..5. Click a side to face it.");
	static readonly GUIContent offsetLabel = new GUIContent("Offset Cell", "The cell in Unity's Grid (offset, odd-r) coordinates, as Grid.CellToWorld uses.");
	static readonly GUIContent worldLabel = new GUIContent("World Position", "The transform's current world position.");
	static readonly GUIContent snapNowLabel = new GUIContent("Re-snap", "Place the transform from the stored cell and facing again.");
	static readonly GUIContent adoptLabel = new GUIContent("Adopt Transform", "Store the cell and facing nearest the transform's current position and rotation, then snap to them.");
	static readonly GUIContent handlesLabel = new GUIContent("Scene Handles", "Show the cell outline in the Scene view, drag the centre dot to move cell by cell, and click an edge dot to change facing. Replaces the transform tool while shown.");
	static readonly GUIContent ccwLabel = new GUIContent("↺", "Turn one side counter-clockwise (index + 1).");
	static readonly GUIContent cwLabel = new GUIContent("↻", "Turn one side clockwise (index - 1).");

	// Fields this editor draws itself; anything else (e.g. a subclass's own fields) goes under "Other".
	static readonly string[] handledFields = { "m_Script", "position", "rotation", "gridFindMode", "_grid", "_coord", "_directionIndex", "_hasCoord" };

	SerializedProperty position, rotation, gridFindMode, grid, coord, directionIndex, hasCoord;
	bool hidTools;

	static bool showHandles {
		get => EditorPrefs.GetBool(handlesPref, true);
		set => EditorPrefs.SetBool(handlesPref, value);
	}

	protected virtual void OnEnable () {
		position = serializedObject.FindProperty("position");
		rotation = serializedObject.FindProperty("rotation");
		gridFindMode = serializedObject.FindProperty("gridFindMode");
		grid = serializedObject.FindProperty("_grid");
		coord = serializedObject.FindProperty("_coord");
		directionIndex = serializedObject.FindProperty("_directionIndex");
		hasCoord = serializedObject.FindProperty("_hasCoord");
	}

	protected virtual void OnDisable () {
		if(hidTools) Tools.hidden = false;
		hidTools = false;
	}

	IEnumerable<HexGridSnap> tiles {
		get { foreach(var t in targets) if(t is HexGridSnap tile && tile != null) yield return tile; }
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();

		HexEditorGUI.Section("Snapping", null, true);
		EditorGUILayout.PropertyField(position, positionLabel);
		EditorGUILayout.PropertyField(rotation, rotationLabel);

		HexEditorGUI.Section("Grid");
		EditorGUILayout.PropertyField(gridFindMode, findModeLabel);
		bool manual = !gridFindMode.hasMultipleDifferentValues && gridFindMode.enumValueIndex == (int)HexGridSnap.GridFindMode.Manual;
		if(manual) {
			EditorGUILayout.PropertyField(grid, gridLabel);
		} else if(!serializedObject.isEditingMultipleObjects) {
			using(new EditorGUI.DisabledScope(true))
				EditorGUILayout.ObjectField(resolvedLabel, ((HexGridSnap)target).grid, typeof(WorldSpaceHexGrid), true);
		}
		DrawMissingGridWarning();

		if(EditorGUI.EndChangeCheck()) {
			// Toggling snapping or changing the grid re-places the transform from OnValidate; record it first.
			RecordTiles("Edit Hex Grid Snap");
			serializedObject.ApplyModifiedProperties();
		}

		HexEditorGUI.Section("Cell");
		bool anyGrid = false;
		foreach(var tile in tiles) anyGrid |= tile.grid != null;
		using(new EditorGUI.DisabledScope(!anyGrid)) {
			DrawCell();
			DrawFacing();
			DrawReadOuts();
			DrawDriftWarning();
			DrawActions();
		}

		HexEditorGUI.Section("Scene View");
		EditorGUI.BeginChangeCheck();
		bool show = EditorGUILayout.Toggle(handlesLabel, showHandles);
		if(EditorGUI.EndChangeCheck()) {
			showHandles = show;
			SceneView.RepaintAll();
		}

		DrawOtherProperties();
		serializedObject.ApplyModifiedProperties();
	}

	void DrawMissingGridWarning () {
		foreach(var tile in tiles) {
			if(tile.grid != null) continue;
			string message;
			switch(tile.gridFindMode) {
				case HexGridSnap.GridFindMode.Parent:
					message = "No WorldSpaceHexGrid found on this object or its parents. Parent the object under a grid, or switch Find Grid to Manual.";
					break;
				case HexGridSnap.GridFindMode.Manual:
					message = "No grid assigned. Assign a WorldSpaceHexGrid above.";
					break;
				default:
					message = "No grid found. HexGridSnap only resolves Parent and Manual itself; Master needs a subclass that overrides GetGrid.";
					break;
			}
			if(serializedObject.isEditingMultipleObjects) message = $"{tile.name}: {message}";
			EditorGUILayout.HelpBox(message, MessageType.Warning);
			if(serializedObject.isEditingMultipleObjects) break;
		}
	}

	void DrawCell () {
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(coord, cellLabel);
		if(EditorGUI.EndChangeCheck()) {
			hasCoord.boolValue = true;
			RecordTiles("Move to Cell");
			serializedObject.ApplyModifiedProperties();
			foreach(var tile in tiles) Reapply(tile);
		}
	}

	void DrawFacing () {
		var rect = EditorGUILayout.GetControlRect(true, diagramSize);
		var labelRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
		var field = EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), facingLabel);
		field.yMax = rect.yMax;
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		bool mixed = directionIndex.hasMultipleDifferentValues;
		int current = HexDiagram.Mod(directionIndex.intValue, 6);
		var diagram = HexEditorGUI.DiagramRect(field, diagramSize);
		int clicked = HexEditorGUI.SideDiagram(diagram,
			i => mixed ? HexEditorGUI.SideState.Off : i == current ? HexEditorGUI.SideState.On : HexEditorGUI.SideState.Off,
			true, out _, "Click a side to face that direction.");
		if(!mixed) HexEditorGUI.DrawFacingArrow(diagram, current, HexEditorGUI.Facing);
		if(clicked >= 0) SetFacing(_ => clicked, "Set Facing");

		float line = EditorGUIUtility.singleLineHeight, space = EditorGUIUtility.standardVerticalSpacing;
		var column = new Rect(diagram.xMax + 8f, field.y, Mathf.Max(0f, field.xMax - diagram.xMax - 8f), line);
		var d = HexCoord.Direction(current);
		EditorGUI.LabelField(column, mixed ? "Mixed facings" : $"Direction {current}  ({d.q}, {d.r})", HexEditorGUI.MiniLabel);
		column.y += line + space;
		float half = column.width * 0.5f;
		if(GUI.Button(new Rect(column.x, column.y, half, line), ccwLabel, EditorStyles.miniButtonLeft)) SetFacing(i => i + 1, "Turn Counter-clockwise");
		if(GUI.Button(new Rect(column.x + half, column.y, half, line), cwLabel, EditorStyles.miniButtonRight)) SetFacing(i => i - 1, "Turn Clockwise");
		if(!rotation.hasMultipleDifferentValues && !rotation.boolValue) {
			column.y += line + space;
			EditorGUI.LabelField(column, "Snap Rotation is off", HexEditorGUI.MiniLabel);
		}
		EditorGUI.indentLevel = indent;
	}

	void DrawReadOuts () {
		if(serializedObject.isEditingMultipleObjects) return;
		var tile = (HexGridSnap)target;
		var c = new HexCoord(coord.FindPropertyRelative("q").intValue, coord.FindPropertyRelative("r").intValue);
		var offset = HexCoord.AxialToOffset(c);
		HexEditorGUI.ReadOut(offsetLabel, $"({offset.x}, {offset.y})");
		HexEditorGUI.ReadOut(worldLabel, HexEditorGUI.Format(tile.transform.position));
	}

	// The cell is authoritative, but moving the object with the regular transform tools doesn't update it; the next
	// grid change would then snap the object back. Point that out while it can still be fixed either way.
	void DrawDriftWarning () {
		var drifted = new List<HexGridSnap>();
		foreach(var tile in tiles) if(HasDrifted(tile)) drifted.Add(tile);
		if(drifted.Count == 0) return;

		string message;
		if(drifted.Count == 1 && !serializedObject.isEditingMultipleObjects) {
			var tile = drifted[0];
			var stored = tile.GetPosition();
			var under = tile.grid.WorldToAxial(tile.transform.position);
			message = under == stored
				? $"The transform is off its stored cell's centre or facing. It will snap back to cell ({stored.q}, {stored.r}) on the next grid change."
				: $"The transform has moved to cell ({under.q}, {under.r}) but the stored cell is ({stored.q}, {stored.r}). It will snap back on the next grid change.";
		} else {
			message = $"{drifted.Count} objects have moved off their stored cell or facing.";
		}
		EditorGUILayout.HelpBox(message, MessageType.Warning);
		using(new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(new GUIContent("Snap Back", "Return to the stored cell and facing."), EditorStyles.miniButtonLeft, GUILayout.Width(100f))) {
				Record(drifted, "Snap Back");
				foreach(var tile in drifted) Reapply(tile);
			}
			if(GUILayout.Button(new GUIContent("Keep New Cell", "Store the cell and facing under the transform, then snap to them."), EditorStyles.miniButtonRight, GUILayout.Width(100f))) {
				Record(drifted, "Adopt Transform");
				foreach(var tile in drifted) Adopt(tile);
			}
		}
	}

	bool HasDrifted (HexGridSnap tile) {
		var g = tile.grid;
		if(g == null || g.grid == null) return false;
		var stored = tile.GetPosition();
		if(tile.position) {
			var normal = g.floorNormal;
			var inPlane = Vector3.ProjectOnPlane(tile.transform.position - g.AxialToWorld(stored), normal);
			float tolerance = Mathf.Max(1e-4f, g.grid.cellSize.x * 0.01f);
			if(inPlane.magnitude > tolerance) return true;
		}
		if(tile.rotation) {
			if(Quaternion.Angle(tile.transform.rotation, g.HexCoordDirectionIndexToRotation(tile.GetDirectionIndex())) > 0.5f) return true;
		}
		return false;
	}

	void DrawActions () {
		using(new EditorGUILayout.HorizontalScope()) {
			GUILayout.Space(EditorGUIUtility.labelWidth + 2f);
			if(GUILayout.Button(snapNowLabel, EditorStyles.miniButtonLeft)) {
				RecordTiles("Re-snap");
				foreach(var tile in tiles) Reapply(tile);
			}
			if(GUILayout.Button(adoptLabel, EditorStyles.miniButtonRight)) {
				RecordTiles("Adopt Transform");
				foreach(var tile in tiles) Adopt(tile);
			}
		}
	}

	// Anything not handled above, typically a subclass's own fields.
	void DrawOtherProperties () {
		var it = serializedObject.GetIterator();
		bool header = false;
		for(bool enter = true; it.NextVisible(enter); enter = false) {
			if(System.Array.IndexOf(handledFields, it.name) >= 0) continue;
			if(!header) {
				HexEditorGUI.Section("Other");
				header = true;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(it, true);
			if(EditorGUI.EndChangeCheck()) RecordTiles("Edit " + it.displayName);
		}
	}

	// --- Mutations ---------------------------------------------------------------------------------------------

	// Sets each tile's facing to `next(current)` through its own SerializedObject, so a relative turn on a
	// multi-selection keeps each object's own facing.
	void SetFacing (System.Func<int, int> next, string undoName) {
		serializedObject.ApplyModifiedProperties();
		SetFacing(new List<HexGridSnap>(tiles), next, undoName);
		serializedObject.Update();
	}

	// Doesn't touch this editor's serializedObject, so it's safe from OnSceneGUI; the inspector re-reads on its next Update.
	void SetFacing (List<HexGridSnap> snaps, System.Func<int, int> next, string undoName) {
		Record(snaps, undoName);
		foreach(var tile in snaps) {
			var so = new SerializedObject(tile);
			var index = so.FindProperty("_directionIndex");
			index.intValue = HexDiagram.Mod(next(tile.GetDirectionIndex()), 6);
			so.FindProperty("_hasCoord").boolValue = true;
			so.ApplyModifiedProperties();
			Reapply(tile);
		}
	}

	static void Adopt (HexGridSnap tile) {
		var g = tile.grid;
		if(g == null) return;
		var facing = HexCoord.ClosestDirectionIndex(g.RotationToHexCoordDirection(tile.transform.rotation));
		var so = new SerializedObject(tile);
		so.FindProperty("_directionIndex").intValue = facing;
		so.ApplyModifiedProperties();
		tile.SnapToCoord(g.WorldToAxial(tile.transform.position));
		MarkChanged(tile);
	}

	static void Reapply (HexGridSnap tile) {
		if(tile.grid == null) return;
		tile.ReapplyFromStoredCoord();
		MarkChanged(tile);
	}

	static void MarkChanged (HexGridSnap tile) {
		EditorUtility.SetDirty(tile);
		if(PrefabUtility.IsPartOfPrefabInstance(tile)) {
			PrefabUtility.RecordPrefabInstancePropertyModifications(tile);
			PrefabUtility.RecordPrefabInstancePropertyModifications(tile.transform);
		}
	}

	void RecordTiles (string undoName) {
		Record(tiles, undoName);
	}

	static void Record (IEnumerable<HexGridSnap> snaps, string undoName) {
		var objects = new List<Object>();
		foreach(var tile in snaps) {
			objects.Add(tile);
			objects.Add(tile.transform);
		}
		Undo.RecordObjects(objects.ToArray(), undoName);
	}

	// --- Scene view ----------------------------------------------------------------------------------------------

	static readonly Color sceneAccent = new Color(0.33f, 0.68f, 1f, 1f);
	static readonly Color sceneFacing = new Color(1f, 0.78f, 0.24f, 1f);
	static readonly Color sceneIdle = new Color(1f, 1f, 1f, 0.45f);
	static GUIStyle _sceneLabel;
	static GUIStyle sceneLabel => _sceneLabel ??= new GUIStyle(EditorStyles.miniBoldLabel) {
		alignment = TextAnchor.UpperCenter, normal = { textColor = sceneAccent },
	};

	protected virtual void OnSceneGUI () {
		var tile = target as HexGridSnap;
		var g = tile != null ? tile.grid : null;
		bool wantHandles = showHandles && tile != null && (tile.position || tile.rotation)
			&& UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(tile.gameObject) == null
			&& g != null && g.grid != null;
		// Our move dot sits where the transform tool's would; hide Unity's only while ours are actually drawn.
		if(wantHandles != hidTools) {
			Tools.hidden = wantHandles;
			hidTools = wantHandles;
		}
		if(!wantHandles) return;

		var cell = tile.GetPosition();
		var normal = g.floorNormal;
		// Lift the handles to the object's height above the grid plane (e.g. terrain) so they hug it.
		var lift = Vector3.Project(tile.transform.position - g.AxialToWorld(cell), normal);
		var centre = g.AxialToWorld(cell) + lift;
		float size = HandleUtility.GetHandleSize(centre);

		var loop = new Vector3[7];
		for(int i = 0; i < 6; i++) loop[i] = g.GetCornerPosition(cell, i) + lift;
		loop[6] = loop[0];
		using(new Handles.DrawingScope(new Color(sceneAccent.r, sceneAccent.g, sceneAccent.b, 0.08f)))
			Handles.DrawAAConvexPolygon(loop);
		using(new Handles.DrawingScope(sceneAccent))
			Handles.DrawAAPolyLine(2.5f, loop);
		Handles.Label(centre - g.axis * Vector3.forward * size * 0.25f, $"({cell.q}, {cell.r})", sceneLabel);

		if(tile.rotation) DrawFacingHandles(tile, g, cell, lift, size);
		if(tile.position) DrawMoveHandle(tile, g, centre, size);
	}

	// A dot on each edge midpoint; click one to face it. The current facing is an amber arrow.
	void DrawFacingHandles (HexGridSnap tile, WorldSpaceHexGrid g, HexCoord cell, Vector3 lift, float size) {
		int current = HexDiagram.Mod(tile.GetDirectionIndex(), 6);
		var normal = g.floorNormal;
		var centre = g.AxialToWorld(cell) + lift;
		for(int i = 0; i < 6; i++) {
			var p = g.GetEdgePosition(cell, i) + lift;
			if(i == current) {
				using(new Handles.DrawingScope(sceneFacing)) {
					Handles.DrawAAPolyLine(3f, Vector3.Lerp(centre, p, 0.25f), p);
					Handles.ConeHandleCap(0, p, Quaternion.LookRotation(p - centre, normal), size * 0.12f, EventType.Repaint);
				}
				continue;
			}
			using(new Handles.DrawingScope(sceneIdle)) {
				if(Handles.Button(p, Quaternion.LookRotation(normal), size * 0.05f, size * 0.08f, Handles.DotHandleCap)) {
					int facing = i;
					SetFacing(new List<HexGridSnap> { tile }, _ => facing, "Set Facing");
				}
			}
		}
	}

	// Drag the centre dot across the grid plane; the object jumps cell to cell as the cursor crosses them.
	void DrawMoveHandle (HexGridSnap tile, WorldSpaceHexGrid g, Vector3 centre, float size) {
		var normal = g.floorNormal;
		using(new Handles.DrawingScope(sceneAccent)) {
			EditorGUI.BeginChangeCheck();
			var dragged = Handles.Slider2D(centre, normal, g.axis * Vector3.right, g.axis * Vector3.forward, size * 0.09f, Handles.CircleHandleCap, 0f);
			if(EditorGUI.EndChangeCheck()) {
				var cell = g.WorldToAxial(dragged);
				if(cell != tile.GetPosition()) {
					Record(new[] { tile }, "Move to Cell");
					tile.SnapToCoord(cell);
					MarkChanged(tile);
					Repaint();
				}
			}
		}
	}
}
