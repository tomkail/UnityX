#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityX.HexGrid;

// Drawer for HexMap<T>. HexMap serializes as two parallel lists (serializedCoords / serializedValues); this shows
// them as one table of cell -> value rows instead, under a foldout whose header gives the cell count. Above the
// table a footprint preview draws every cell (click one to jump to its row). Large maps are paged.
//
// Problems the raw lists hide are surfaced: mismatched list lengths (extra entries are ignored on load) and
// duplicate coords (only the last one survives, since the lists rebuild a dictionary).
[CustomPropertyDrawer(typeof(HexMap<>), true)]
public class HexMapDrawer : PropertyDrawer {
	const int pageSize = 20;
	const float previewHeight = 96f;
	const float indexWidth = 30f;
	const float coordWidth = 116f;
	const float removeWidth = 20f;

	static readonly GUIContent addLabel = new GUIContent("Add Cell", "Add a cell at the nearest free coord to the origin.");
	static readonly GUIContent clearLabel = new GUIContent("Clear", "Remove every cell.");
	static readonly GUIContent removeLabel = new GUIContent("−", "Remove this cell.");
	static readonly GUIContent prevLabel = new GUIContent("‹", "Previous page");
	static readonly GUIContent nextLabel = new GUIContent("›", "Next page");
	static readonly GUIContent overriddenIcon = new GUIContent();

	// Per-field UI state (page, selected row) plus the coords read from the serialized list, re-read only when the
	// list's content hash changes so big maps don't walk every element each repaint.
	class State {
		public int page;
		public int selected = -1;
		public uint hash;
		public bool valid;
		public readonly List<HexCoord> coords = new List<HexCoord>();
		public HashSet<int> overridden = new HashSet<int>();
	}
	static readonly Dictionary<string, State> states = new Dictionary<string, State>();

	static State GetState(SerializedProperty property, SerializedProperty coords) {
		var key = property.serializedObject.targetObject.GetEntityId() + "/" + property.propertyPath;
		if(!states.TryGetValue(key, out var state)) states[key] = state = new State();
		uint hash = coords.contentHash;
		if(!state.valid || state.hash != hash) {
			state.coords.Clear();
			for(int i = 0; i < coords.arraySize; i++) {
				var element = coords.GetArrayElementAtIndex(i);
				state.coords.Add(new HexCoord(element.FindPropertyRelative("q").intValue, element.FindPropertyRelative("r").intValue));
			}
			state.overridden = HexDiagram.OverriddenIndices(state.coords);
			state.hash = hash;
			state.valid = true;
		}
		int pages = Mathf.Max(1, Mathf.CeilToInt(coords.arraySize / (float)pageSize));
		state.page = Mathf.Clamp(state.page, 0, pages - 1);
		return state;
	}

	static float line => EditorGUIUtility.singleLineHeight;
	static float space => EditorGUIUtility.standardVerticalSpacing;
	static float helpHeight => line * 2f + 4f;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		if(!property.isExpanded) return line;
		var coords = property.FindPropertyRelative("serializedCoords");
		var values = property.FindPropertyRelative("serializedValues");
		if(coords == null || values == null) return line;
		float h = line + space;
		if(property.serializedObject.isEditingMultipleObjects) return h + helpHeight;

		var state = GetState(property, coords);
		if(coords.arraySize != values.arraySize) h += helpHeight + space + line + space;
		if(coords.arraySize > 0) h += previewHeight + space;
		if(state.overridden.Count > 0) h += helpHeight + space;
		int count = Mathf.Min(coords.arraySize, values.arraySize);
		for(int i = state.page * pageSize; i < Mathf.Min(count, (state.page + 1) * pageSize); i++)
			h += RowHeight(values.GetArrayElementAtIndex(i)) + space;
		return h + line;
	}

	static float RowHeight(SerializedProperty value) {
		return Mathf.Max(line, EditorGUI.GetPropertyHeight(value, GUIContent.none, true));
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		var coords = property.FindPropertyRelative("serializedCoords");
		var values = property.FindPropertyRelative("serializedValues");

		var header = new Rect(position.x, position.y, position.width, line);
		if(coords == null || values == null) {
			EditorGUI.LabelField(header, label, new GUIContent("Value type isn't serializable"));
			EditorGUI.EndProperty();
			return;
		}
		property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, label, true);

		bool multi = property.serializedObject.isEditingMultipleObjects;
		var state = multi ? null : GetState(property, coords);
		var summary = new Rect(header.x + EditorGUIUtility.labelWidth, header.y, header.width - EditorGUIUtility.labelWidth, line);
		EditorGUI.LabelField(summary, multi ? new GUIContent("Mixed") : Summary(coords.arraySize, state.overridden.Count), HexEditorGUI.Summary);

		if(property.isExpanded) {
			EditorGUI.indentLevel++;
			var body = EditorGUI.IndentedRect(new Rect(position.x, header.yMax + space, position.width, position.yMax - header.yMax - space));
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if(multi) {
				EditorGUI.HelpBox(new Rect(body.x, body.y, body.width, helpHeight), "Hex maps can't be edited on several objects at once.", MessageType.Info);
			} else {
				DrawBody(body, coords, values, state);
			}
			EditorGUI.indentLevel = indent;
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
	}

	static GUIContent Summary(int count, int overridden) {
		string text = count == 1 ? "1 cell" : $"{count} cells";
		if(overridden > 0) text += $", {overridden} duplicate{(overridden == 1 ? "" : "s")}";
		return new GUIContent(text);
	}

	static void DrawBody(Rect body, SerializedProperty coords, SerializedProperty values, State state) {
		float y = body.y;

		if(coords.arraySize != values.arraySize) {
			EditorGUI.HelpBox(new Rect(body.x, y, body.width, helpHeight),
				$"{coords.arraySize} coords but {values.arraySize} values. Entries without a partner are dropped when the map loads.", MessageType.Warning);
			y += helpHeight + space;
			if(GUI.Button(new Rect(body.x, y, Mathf.Min(body.width, 160f), line), "Trim to Matching Pairs", EditorStyles.miniButton)) {
				int n = Mathf.Min(coords.arraySize, values.arraySize);
				coords.arraySize = n;
				values.arraySize = n;
			}
			y += line + space;
		}

		if(coords.arraySize > 0) {
			DrawPreview(new Rect(body.x, y, body.width, previewHeight), state);
			y += previewHeight + space;
		}

		if(state.overridden.Count > 0) {
			EditorGUI.HelpBox(new Rect(body.x, y, body.width, helpHeight),
				"Some coords appear more than once. Only the last entry for a coord is kept; the flagged rows are ignored.", MessageType.Warning);
			y += helpHeight + space;
		}

		int count = Mathf.Min(coords.arraySize, values.arraySize);
		int first = state.page * pageSize, last = Mathf.Min(count, first + pageSize);
		for(int i = first; i < last; i++) {
			var value = values.GetArrayElementAtIndex(i);
			float h = RowHeight(value);
			var row = new Rect(body.x, y, body.width, h);
			if(DrawRow(row, i, coords, values, value, state)) return;
			y += h + space;
		}

		DrawFooter(new Rect(body.x, y, body.width, line), coords, values, state, count);
	}

	// Returns true if the row deleted itself (the arrays changed under us, so stop drawing this pass).
	static bool DrawRow(Rect row, int index, SerializedProperty coords, SerializedProperty values, SerializedProperty value, State state) {
		if(Event.current.type == EventType.Repaint && index == state.selected) {
			EditorGUI.DrawRect(new Rect(row.x - 2f, row.y - 1f, row.width + 4f, row.height + 2f), HexEditorGUI.AccentFill);
		}
		var indexRect = new Rect(row.x, row.y, indexWidth, line);
		if(state.overridden.Contains(index)) {
			var icon = EditorGUIUtility.IconContent("console.warnicon.sml");
			overriddenIcon.image = icon.image;
			overriddenIcon.tooltip = "A later row has the same coord, so this one is ignored.";
			GUI.Label(indexRect, overriddenIcon);
		} else {
			GUI.Label(indexRect, new GUIContent(index.ToString(), "Row " + index), HexEditorGUI.MiniLabel);
		}

		var coord = coords.GetArrayElementAtIndex(index);
		var coordRect = new Rect(indexRect.xMax, row.y, coordWidth, line);
		HexEditorGUI.InlineFields(coordRect, new[] { coord.FindPropertyRelative("q"), coord.FindPropertyRelative("r") },
			new[] { HexCoordDrawer.QLabel, HexCoordDrawer.RLabel });

		var valueRect = new Rect(coordRect.xMax + 8f, row.y, row.xMax - coordRect.xMax - 8f - removeWidth - 4f, row.height);
		float labelWidth = EditorGUIUtility.labelWidth;
		if(value.hasVisibleChildren) {
			// Nested structs get a real label column so their children line up, sized to the space we have.
			EditorGUIUtility.labelWidth = Mathf.Max(40f, valueRect.width * 0.4f);
			EditorGUI.PropertyField(valueRect, value, new GUIContent("Value"), true);
		} else {
			EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);
		}
		EditorGUIUtility.labelWidth = labelWidth;

		if(GUI.Button(new Rect(row.xMax - removeWidth, row.y, removeWidth, line), removeLabel, EditorStyles.miniButton)) {
			DeleteElement(coords, index);
			DeleteElement(values, index);
			if(state.selected == index) state.selected = -1;
			return true;
		}
		return false;
	}

	// Older Unity versions null an object-reference element on the first delete instead of removing it.
	static void DeleteElement(SerializedProperty array, int index) {
		int size = array.arraySize;
		array.DeleteArrayElementAtIndex(index);
		if(array.arraySize == size) array.DeleteArrayElementAtIndex(index);
	}

	static void DrawFooter(Rect rect, SerializedProperty coords, SerializedProperty values, State state, int count) {
		int pages = Mathf.Max(1, Mathf.CeilToInt(count / (float)pageSize));
		float x = rect.x;
		if(pages > 1) {
			using(new EditorGUI.DisabledScope(state.page == 0))
				if(GUI.Button(new Rect(x, rect.y, 22f, line), prevLabel, EditorStyles.miniButtonLeft)) state.page--;
			using(new EditorGUI.DisabledScope(state.page >= pages - 1))
				if(GUI.Button(new Rect(x + 22f, rect.y, 22f, line), nextLabel, EditorStyles.miniButtonRight)) state.page++;
			x += 48f;
			int first = state.page * pageSize;
			GUI.Label(new Rect(x, rect.y, 120f, line), $"{first}–{Mathf.Min(count, first + pageSize) - 1} of {count}", HexEditorGUI.MiniLabel);
		}

		var clearRect = new Rect(rect.xMax - 60f, rect.y, 60f, line);
		var addRect = new Rect(clearRect.x - 80f, rect.y, 80f, line);
		if(GUI.Button(addRect, addLabel, EditorStyles.miniButtonLeft)) {
			var free = HexDiagram.NextFreeCoord(new HashSet<HexCoord>(state.coords));
			int index = coords.arraySize;
			coords.arraySize = index + 1;
			values.arraySize = index + 1;
			var coord = coords.GetArrayElementAtIndex(index);
			coord.FindPropertyRelative("q").intValue = free.q;
			coord.FindPropertyRelative("r").intValue = free.r;
			state.selected = index;
			state.page = index / pageSize;
		}
		using(new EditorGUI.DisabledScope(coords.arraySize == 0 && values.arraySize == 0)) {
			if(GUI.Button(clearRect, clearLabel, EditorStyles.miniButtonRight)) {
				coords.arraySize = 0;
				values.arraySize = 0;
				state.selected = -1;
				state.page = 0;
			}
		}
	}

	// Footprint of every cell, scaled to fit. Selected row in the accent colour, ignored duplicates in the warning
	// colour, origin marked with a dot. Click a cell to select its row (and jump to its page).
	static void DrawPreview(Rect rect, State state) {
		var evt = Event.current;
		float scale = HexDiagram.FitCoords(state.coords, new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), 14f, out var origin);
		HexCoord? hovered = null;
		if(rect.Contains(evt.mousePosition)) {
			var c = HexDiagram.DiagramToCoord(evt.mousePosition, origin, scale);
			if(state.coords.Contains(c)) hovered = c;
		}

		if(evt.type == EventType.MouseDown && evt.button == 0 && hovered.HasValue) {
			int index = state.coords.LastIndexOf(hovered.Value);
			state.selected = index;
			state.page = index / pageSize;
			evt.Use();
			GUI.changed = true;
		}
		if(evt.type == EventType.MouseMove && rect.Contains(evt.mousePosition)) HandleUtility.Repaint();
		if(hovered.HasValue) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
		if(evt.type != EventType.Repaint) return;

		EditorGUI.DrawRect(rect, HexEditorGUI.Fill);
		var poly = new Vector3[6];
		var cellColor = HexEditorGUI.WithAlpha(HexEditorGUI.Line, HexEditorGUI.Line.a * 1.2f);
		float r = scale * (scale > 4f ? 0.9f : 1f);
		HexCoord? selected = state.selected >= 0 && state.selected < state.coords.Count ? state.coords[state.selected] : (HexCoord?)null;
		GUI.BeginClip(rect);
		var offset = origin - rect.position;
		for(int i = 0; i < state.coords.Count; i++) {
			var c = state.coords[i];
			var p = offset + HexDiagram.CoordToDiagram(c) * scale;
			for(int k = 0; k < 6; k++) poly[k] = HexDiagram.Corner(p, r, k);
			Handles.color = state.overridden.Contains(i) ? HexEditorGUI.Warning
				: (selected.HasValue && selected.Value == c) ? HexEditorGUI.Accent
				: (hovered.HasValue && hovered.Value == c) ? HexEditorGUI.WithAlpha(HexEditorGUI.Accent, 0.5f)
				: cellColor;
			Handles.DrawAAConvexPolygon(poly);
		}
		Handles.color = HexEditorGUI.DimText;
		Handles.DrawSolidDisc(offset, Vector3.forward, Mathf.Clamp(scale * 0.2f, 1.5f, 3f));
		Handles.color = Color.white;
		GUI.EndClip();

		if(hovered.HasValue) {
			var c = hovered.Value;
			GUI.Label(new Rect(rect.x + 4f, rect.yMax - line, rect.width - 8f, line), $"({c.q}, {c.r})", HexEditorGUI.MiniLabel);
		}
	}
}
#endif
