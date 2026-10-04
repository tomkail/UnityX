using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Inspector for WorldSpaceHexGrid. Checks the wiring (a Grid assigned, on the Hexagon layout) with one-click fixes,
// gathers the cell-shape settings that live on the Grid component next to it, shows the grid plane's orientation,
// lists how many HexGridSnap objects use this grid, and can show the cell under the cursor in the Scene view.
[CustomEditor(typeof(WorldSpaceHexGrid)), CanEditMultipleObjects]
public class WorldSpaceHexGridEditor : Editor {
	const string hoverPref = "UnityX.HexGrid.WorldSpaceHexGrid.ShowHoveredCell";

	static readonly GUIContent gridLabel = new GUIContent("Grid", "The UnityEngine.Grid this wraps; normally the one on this GameObject. Must use the Hexagon cell layout.");
	static readonly GUIContent cellSizeLabel = new GUIContent("Cell Size", "Grid.cellSize. For a regular hexagon keep x = y (pointy cells: x is the width across flats).");
	static readonly GUIContent cellGapLabel = new GUIContent("Cell Gap", "Grid.cellGap. Corner and edge positions assume no gap.");
	static readonly GUIContent swizzleLabel = new GUIContent("Cell Swizzle", "Grid.cellSwizzle: which world plane cells lie on. XZY lays the grid flat on the ground.");
	static readonly GUIContent normalLabel = new GUIContent("Plane Normal", "World-space normal of the grid plane (floorNormal).");
	static readonly GUIContent forwardLabel = new GUIContent("Plane Forward", "The grid's own forward, flattened onto the plane (axis * forward). Facing rotations are measured against this.");
	static readonly GUIContent layoutLabel = new GUIContent("Coordinates", "HexCoord's offset layout. Unity's hexagon Grid always reports odd-r (pointy) offset cells, so this is fixed to OddR. Flat-topped grids come from rotating or swizzling the grid, not from changing this.");
	static readonly GUIContent snapsLabel = new GUIContent("Using This Grid", "HexGridSnap components in open scenes that use this grid.");
	static readonly GUIContent selectLabel = new GUIContent("Select", "Select every HexGridSnap that uses this grid.");
	static readonly GUIContent resnapLabel = new GUIContent("Re-snap All", "Place every HexGridSnap that uses this grid from its stored cell again.");
	static readonly GUIContent hoverLabel = new GUIContent("Cell Under Cursor", "Outline and label the cell under the mouse in the Scene view while this grid is selected.");
	static readonly GUIContent hoveredLabel = new GUIContent("Hovered Cell", "Axial (q, r) and offset (x, y) of the cell under the mouse in the Scene view.");

	SerializedProperty grid;
	SerializedObject gridObject;
	readonly List<HexGridSnap> snaps = new List<HexGridSnap>();
	bool snapsDirty = true;
	HexCoord? hovered;

	static bool showHovered {
		get => EditorPrefs.GetBool(hoverPref, true);
		set => EditorPrefs.SetBool(hoverPref, value);
	}

	void OnEnable () {
		grid = serializedObject.FindProperty("grid");
		EditorApplication.hierarchyChanged += MarkSnapsDirty;
	}

	void OnDisable () {
		EditorApplication.hierarchyChanged -= MarkSnapsDirty;
		gridObject?.Dispose();
		gridObject = null;
	}

	void MarkSnapsDirty () {
		snapsDirty = true;
	}

	IEnumerable<WorldSpaceHexGrid> hexGrids {
		get { foreach(var t in targets) if(t is WorldSpaceHexGrid g && g != null) yield return g; }
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();

		HexEditorGUI.Section("Grid", null, true);
		EditorGUILayout.PropertyField(grid, gridLabel);
		DrawWiringChecks();
		serializedObject.ApplyModifiedProperties();

		var grids = new List<Object>();
		foreach(var g in hexGrids) if(g.grid != null) grids.Add(g.grid);
		if(grids.Count == 0) return;

		HexEditorGUI.Section("Cell Shape", "Settings stored on the Grid component, gathered here. Objects snapped to this grid follow changes automatically.");
		DrawCellShape(grids);

		if(!serializedObject.isEditingMultipleObjects) {
			var hexGrid = (WorldSpaceHexGrid)target;
			HexEditorGUI.Section("Orientation");
			HexEditorGUI.ReadOut(normalLabel, HexEditorGUI.Format(hexGrid.floorNormal));
			HexEditorGUI.ReadOut(forwardLabel, HexEditorGUI.Format(hexGrid.axis * Vector3.forward));
			HexEditorGUI.ReadOut(layoutLabel, $"{HexCoord.offsetLayout} ({HexCoord.orientation.ToString().ToLowerInvariant()}), axial q / r");

			HexEditorGUI.Section("Snapped Objects");
			DrawSnaps(hexGrid);

			HexEditorGUI.Section("Scene View");
			EditorGUI.BeginChangeCheck();
			bool show = EditorGUILayout.Toggle(hoverLabel, showHovered);
			if(EditorGUI.EndChangeCheck()) {
				showHovered = show;
				SceneView.RepaintAll();
			}
			if(show) {
				string text = "Hover the grid in the Scene view";
				if(hovered.HasValue) {
					var c = hovered.Value;
					var offset = HexCoord.AxialToOffset(c);
					text = $"({c.q}, {c.r})   offset ({offset.x}, {offset.y})";
				}
				HexEditorGUI.ReadOut(hoveredLabel, text);
			}
		}
	}

	void DrawWiringChecks () {
		foreach(var hexGrid in hexGrids) {
			string prefix = serializedObject.isEditingMultipleObjects ? hexGrid.name + ": " : "";
			if(hexGrid.grid == null) {
				var sibling = hexGrid.GetComponent<UnityEngine.Grid>();
				EditorGUILayout.HelpBox(prefix + "No Grid assigned. WorldSpaceHexGrid converts coordinates through a UnityEngine.Grid; nothing works until one is set.", MessageType.Error);
				if(sibling != null && FixButton("Use Grid on This Object")) {
					Undo.RecordObject(hexGrid, "Assign Grid");
					hexGrid.grid = sibling;
					EditorUtility.SetDirty(hexGrid);
					serializedObject.Update();
				}
			} else if(hexGrid.grid.cellLayout != GridLayout.CellLayout.Hexagon) {
				EditorGUILayout.HelpBox(prefix + $"The Grid uses the {hexGrid.grid.cellLayout} layout. Hex coordinates need Hexagon.", MessageType.Warning);
				if(FixButton("Set to Hexagon")) {
					Undo.RecordObject(hexGrid.grid, "Set Hexagon Layout");
					hexGrid.grid.cellLayout = GridLayout.CellLayout.Hexagon;
					EditorUtility.SetDirty(hexGrid.grid);
				}
			}
		}
	}

	static bool FixButton (string text) {
		using(new EditorGUILayout.HorizontalScope()) {
			GUILayout.FlexibleSpace();
			return GUILayout.Button(text, EditorStyles.miniButton, GUILayout.MinWidth(140f));
		}
	}

	void DrawCellShape (List<Object> grids) {
		if(gridObject == null || !SameTargets(gridObject, grids)) {
			gridObject?.Dispose();
			gridObject = new SerializedObject(grids.ToArray());
		}
		gridObject.Update();
		var size = gridObject.FindProperty("m_CellSize");
		var gap = gridObject.FindProperty("m_CellGap");
		var swizzle = gridObject.FindProperty("m_CellSwizzle");
		if(size != null) EditorGUILayout.PropertyField(size, cellSizeLabel);
		if(gap != null) EditorGUILayout.PropertyField(gap, cellGapLabel);
		if(swizzle != null) EditorGUILayout.PropertyField(swizzle, swizzleLabel);
		gridObject.ApplyModifiedProperties();

		if(size != null && !size.hasMultipleDifferentValues && !Mathf.Approximately(size.vector3Value.x, size.vector3Value.y)) {
			EditorGUILayout.HelpBox("Cell Size x and y differ, so cells are stretched rather than regular hexagons.", MessageType.Info);
		}
		if(gap != null && !gap.hasMultipleDifferentValues && gap.vector3Value != Vector3.zero) {
			EditorGUILayout.HelpBox("A non-zero Cell Gap pushes cells apart; corner and edge positions assume they touch.", MessageType.Info);
		}
	}

	static bool SameTargets (SerializedObject so, List<Object> objects) {
		var current = so.targetObjects;
		if(current.Length != objects.Count) return false;
		for(int i = 0; i < current.Length; i++) if(current[i] != objects[i]) return false;
		return true;
	}

	void DrawSnaps (WorldSpaceHexGrid hexGrid) {
		if(snapsDirty) {
			RefreshSnaps(hexGrid);
			snapsDirty = false;
		}
		snaps.RemoveAll(s => s == null);
		var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
		rect = EditorGUI.PrefixLabel(rect, snapsLabel);
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		float buttons = Mathf.Min(rect.width * 0.65f, 170f);
		EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - buttons - 4f, rect.height), snaps.Count.ToString());
		using(new EditorGUI.DisabledScope(snaps.Count == 0)) {
			var b = new Rect(rect.xMax - buttons, rect.y, buttons * 0.4f, rect.height);
			if(GUI.Button(b, selectLabel, EditorStyles.miniButtonLeft)) {
				var objects = new List<Object>();
				foreach(var s in snaps) objects.Add(s.gameObject);
				Selection.objects = objects.ToArray();
			}
			b.x += b.width;
			b.width = buttons * 0.6f;
			if(GUI.Button(b, resnapLabel, EditorStyles.miniButtonRight)) {
				var objects = new List<Object>();
				foreach(var s in snaps) objects.Add(s.transform);
				Undo.RecordObjects(objects.ToArray(), "Re-snap All");
				foreach(var s in snaps) {
					s.ReapplyFromStoredCoord();
					EditorUtility.SetDirty(s.transform);
				}
			}
		}
		EditorGUI.indentLevel = indent;
	}

	// Reads each HexGridSnap's serialized grid reference rather than its `grid` property, which would resolve (and
	// cache) a grid as a side effect.
	void RefreshSnaps (WorldSpaceHexGrid hexGrid) {
		snaps.Clear();
		foreach(var snap in Object.FindObjectsByType<HexGridSnap>(FindObjectsInactive.Include)) {
			using(var so = new SerializedObject(snap)) {
				var reference = so.FindProperty("_grid");
				if(reference != null && reference.objectReferenceValue == hexGrid) snaps.Add(snap);
			}
		}
	}

	// --- Scene view ----------------------------------------------------------------------------------------------

	static readonly Color sceneAccent = new Color(0.33f, 0.68f, 1f, 1f);
	static GUIStyle _sceneLabel;
	static GUIStyle sceneLabel => _sceneLabel ??= new GUIStyle(EditorStyles.miniBoldLabel) {
		alignment = TextAnchor.MiddleCenter, normal = { textColor = sceneAccent },
	};

	void OnSceneGUI () {
		if(!showHovered || targets.Length > 1) return;
		var hexGrid = (WorldSpaceHexGrid)target;
		if(hexGrid == null || hexGrid.grid == null) return;
		var evt = Event.current;
		if(evt.type == EventType.MouseMove) HandleUtility.Repaint();

		HexCoord? cell = null;
		var ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
		if(hexGrid.floorPlane.Raycast(ray, out float distance)) cell = hexGrid.WorldToAxial(ray.GetPoint(distance));
		if(!cell.Equals(hovered)) {
			hovered = cell;
			Repaint();
		}
		if(!cell.HasValue || evt.type != EventType.Repaint) return;

		var c = cell.Value;
		var loop = new Vector3[7];
		for(int i = 0; i < 6; i++) loop[i] = hexGrid.GetCornerPosition(c, i);
		loop[6] = loop[0];
		using(new Handles.DrawingScope(new Color(sceneAccent.r, sceneAccent.g, sceneAccent.b, 0.1f)))
			Handles.DrawAAConvexPolygon(loop);
		using(new Handles.DrawingScope(sceneAccent))
			Handles.DrawAAPolyLine(2f, loop);
		Handles.Label(hexGrid.AxialToWorld(c), $"({c.q}, {c.r})", sceneLabel);
	}
}
