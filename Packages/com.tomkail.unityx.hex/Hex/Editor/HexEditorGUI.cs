using System;
using UnityEditor;
using UnityEngine;

namespace UnityX.HexGrid {
	// Shared look and building blocks for the hex package's inspectors and property drawers, so they all speak the
	// same visual language: compact inline number fields with scrubbable mini labels, one accent colour for "set /
	// selected", bold section headers with a hairline rule, and a small clickable hexagon diagram.
	//
	// Colours come in light/dark skin pairs; nothing here assumes one skin.
	public static class HexEditorGUI {
		public enum SideState { Off, On, Mixed }

		// --- Palette ---------------------------------------------------------------------------------------------

		static bool pro => EditorGUIUtility.isProSkin;
		public static Color Accent => pro ? new Color(0.33f, 0.64f, 1f) : new Color(0.07f, 0.38f, 0.82f);
		public static Color AccentFill => WithAlpha(Accent, pro ? 0.22f : 0.16f);
		public static Color Facing => pro ? new Color(1f, 0.76f, 0.24f) : new Color(0.85f, 0.52f, 0f);
		public static Color Warning => pro ? new Color(1f, 0.76f, 0.2f) : new Color(0.75f, 0.45f, 0f);
		public static Color Line => pro ? new Color(1f, 1f, 1f, 0.28f) : new Color(0f, 0f, 0f, 0.3f);
		public static Color Fill => pro ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.05f);
		public static Color Separator => pro ? new Color(0f, 0f, 0f, 0.35f) : new Color(0f, 0f, 0f, 0.15f);
		public static Color DimText => WithAlpha(EditorStyles.label.normal.textColor, 0.6f);

		public static Color WithAlpha (Color c, float a) {
			return new Color(c.r, c.g, c.b, a);
		}

		// --- Styles (cached; built lazily because EditorStyles isn't ready during static init) -------------------

		static GUIStyle _miniCentered, _summary, _section, _miniLabel;
		public static GUIStyle MiniCentered => _miniCentered ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleCenter, padding = new RectOffset(), margin = new RectOffset(),
		};
		// Greyed, right-aligned summary text for the right end of a header row ("12 cells").
		public static GUIStyle Summary => _summary ??= new GUIStyle(EditorStyles.miniLabel) {
			alignment = TextAnchor.MiddleRight, normal = { textColor = DimText },
		};
		public static GUIStyle MiniLabel => _miniLabel ??= new GUIStyle(EditorStyles.miniLabel) {
			normal = { textColor = DimText }, wordWrap = true,
		};
		public static GUIStyle SectionHeader => _section ??= new GUIStyle(EditorStyles.boldLabel) {
			margin = new RectOffset(3, 3, 0, 0),
		};

		public static float Line1 => EditorGUIUtility.singleLineHeight;
		public static float Spacing => EditorGUIUtility.standardVerticalSpacing;

		// --- Inline fields ---------------------------------------------------------------------------------------

		// Lays `props` out side by side in `rect`, each with a short scrubbable label (drag the letter to change the
		// value, like Vector3 fields). Uses PropertyField per child, so mixed values, prefab overrides and undo all
		// behave per component. `extra` reserves trailing columns (e.g. a read-only S) the caller fills itself;
		// their rects come back in `extraRects`.
		public static void InlineFields (Rect rect, SerializedProperty[] props, GUIContent[] labels, int extra, out Rect[] extraRects) {
			int columns = props.Length + extra;
			const float gap = 4f;
			float width = (rect.width - gap * (columns - 1)) / columns;
			int indent = EditorGUI.indentLevel;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUI.indentLevel = 0;
			for(int i = 0; i < props.Length; i++) {
				var cell = new Rect(rect.x + (width + gap) * i, rect.y, width, Line1);
				EditorGUIUtility.labelWidth = LabelWidth(labels[i]);
				EditorGUI.PropertyField(cell, props[i], labels[i]);
			}
			extraRects = new Rect[extra];
			for(int i = 0; i < extra; i++) extraRects[i] = new Rect(rect.x + (width + gap) * (props.Length + i), rect.y, width, Line1);
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indent;
		}

		public static void InlineFields (Rect rect, SerializedProperty[] props, GUIContent[] labels) {
			InlineFields(rect, props, labels, 0, out _);
		}

		// A greyed-out int with a mini label, for derived values such as a HexCoord's s.
		public static void ReadOnlyInt (Rect rect, GUIContent label, int value, bool mixed) {
			int indent = EditorGUI.indentLevel;
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = LabelWidth(label);
			using(new EditorGUI.DisabledScope(true)) {
				EditorGUI.showMixedValue = mixed;
				EditorGUI.IntField(rect, label, value);
				EditorGUI.showMixedValue = false;
			}
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indent;
		}

		static float LabelWidth (GUIContent label) {
			return string.IsNullOrEmpty(label.text) ? 0f : EditorStyles.label.CalcSize(label).x + 2f;
		}

		// --- Layout helpers for custom editors ------------------------------------------------------------------

		// Bold title with a hairline underneath; the start of a group of related fields.
		public static void Section (string title, string tooltip = null, bool first = false) {
			if(!first) EditorGUILayout.Space(6);
			var rect = EditorGUILayout.GetControlRect(false, Line1);
			EditorGUI.LabelField(rect, new GUIContent(title, tooltip), SectionHeader);
			if(Event.current.type == EventType.Repaint) {
				var rule = new Rect(rect.x, rect.yMax, rect.width, 1f);
				EditorGUI.DrawRect(rule, Separator);
			}
			EditorGUILayout.Space(3);
		}

		// Label + selectable value text, for live read-outs that aren't editable.
		public static void ReadOut (GUIContent label, string value) {
			var rect = EditorGUILayout.GetControlRect(true, Line1);
			rect = EditorGUI.PrefixLabel(rect, label);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.SelectableLabel(rect, value, EditorStyles.label);
			EditorGUI.indentLevel = indent;
		}

		public static string Format (Vector3 v) {
			return $"({v.x:0.###}, {v.y:0.###}, {v.z:0.###})";
		}

		// --- Hexagon diagram -------------------------------------------------------------------------------------

		static readonly int DiagramHash = "HexEditorGUI.Diagram".GetHashCode();
		static readonly Vector3[] Polygon = new Vector3[6];
		static readonly Vector3[] Segment = new Vector3[2];
		static readonly Vector3[] Triangle = new Vector3[3];

		// A clickable pointy hexagon whose six sides each show a state (see HexDiagram for the orientation). Returns
		// the side clicked this event, or -1. `hoverSide` is the side under the mouse (for callers that want to
		// preview a click). Only draws on Repaint; the caller draws any overlays (dots, arrows) afterwards using
		// DiagramGeometry.
		public static int SideDiagram (Rect rect, Func<int, SideState> state, bool showIndices, out int hoverSide, string tooltip = null) {
			int id = GUIUtility.GetControlID(DiagramHash, FocusType.Passive, rect);
			var evt = Event.current;
			DiagramGeometry(rect, out var centre, out var radius);
			hoverSide = GUI.enabled && rect.Contains(evt.mousePosition) ? HexDiagram.SideAtPoint(centre, radius, evt.mousePosition) : -1;
			int clicked = -1;

			if(GUI.enabled) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			if(!string.IsNullOrEmpty(tooltip)) GUI.Label(rect, new GUIContent(string.Empty, tooltip));

			switch(evt.GetTypeForControl(id)) {
				case EventType.MouseDown:
					if(evt.button == 0 && hoverSide >= 0) {
						GUIUtility.hotControl = id;
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if(GUIUtility.hotControl == id) {
						GUIUtility.hotControl = 0;
						if(hoverSide >= 0) {
							clicked = hoverSide;
							GUI.changed = true;
						}
						evt.Use();
					}
					break;
				case EventType.MouseMove:
					// Inspectors don't repaint on plain mouse moves; nudge one so the hover wedge tracks the cursor.
					if(rect.Contains(evt.mousePosition)) HandleUtility.Repaint();
					break;
				case EventType.Repaint:
					DrawSides(centre, radius, state, hoverSide, showIndices);
					break;
			}
			return clicked;
		}

		public static void DiagramGeometry (Rect rect, out Vector2 centre, out float radius) {
			centre = rect.center;
			radius = HexDiagram.FitRadius(rect, 2f);
		}

		static void DrawSides (Vector2 centre, float radius, Func<int, SideState> state, int hoverSide, bool showIndices) {
			float alpha = GUI.enabled ? 1f : 0.45f;
			for(int i = 0; i < 6; i++) Polygon[i] = HexDiagram.Corner(centre, radius, i);
			Handles.color = WithAlpha(Fill, Fill.a * alpha);
			Handles.DrawAAConvexPolygon(Polygon);

			if(hoverSide >= 0) {
				HexDiagram.SideEndpoints(centre, radius, hoverSide, out var a, out var b);
				Triangle[0] = centre; Triangle[1] = a; Triangle[2] = b;
				Handles.color = AccentFill;
				Handles.DrawAAConvexPolygon(Triangle);
			}

			for(int i = 0; i < 6; i++) {
				HexDiagram.SideEndpoints(centre, radius, i, out var a, out var b);
				Segment[0] = a; Segment[1] = b;
				switch(state(i)) {
					case SideState.On:
						Handles.color = WithAlpha(Accent, alpha);
						Handles.DrawAAPolyLine(4f, Segment);
						break;
					case SideState.Mixed:
						Handles.color = WithAlpha(Accent, alpha);
						Handles.DrawDottedLine(a, b, 2.5f);
						break;
					default:
						Handles.color = WithAlpha(Line, Line.a * alpha);
						Handles.DrawAAPolyLine(1.5f, Segment);
						break;
				}
			}

			if(showIndices && radius >= 16f) {
				var style = MiniCentered;
				for(int i = 0; i < 6; i++) {
					var p = centre + HexDiagram.SideNormal(i) * radius * 0.55f;
					var on = state(i) == SideState.On;
					var color = on ? Accent : DimText;
					var old = style.normal.textColor;
					style.normal.textColor = WithAlpha(color, color.a * alpha);
					style.Draw(new Rect(p.x - 7f, p.y - 7f, 14f, 14f), new GUIContent(i.ToString()), false, false, false, false);
					style.normal.textColor = old;
				}
			}
			Handles.color = Color.white;
		}

		// Filled dot on a side's midpoint (e.g. the start of a HexArc).
		public static void DrawSideDot (Rect rect, int side, Color color) {
			if(Event.current.type != EventType.Repaint) return;
			DiagramGeometry(rect, out var centre, out var radius);
			Handles.color = color;
			Handles.DrawSolidDisc(HexDiagram.SideMidpoint(centre, radius, side), Vector3.forward, Mathf.Max(2.5f, radius * 0.12f));
			Handles.color = Color.white;
		}

		// Small arrowhead on a side's midpoint pointing along the outline: counter-clockwise for a positive sign.
		public static void DrawTravelArrow (Rect rect, int side, int sign, Color color) {
			if(Event.current.type != EventType.Repaint || sign == 0) return;
			DiagramGeometry(rect, out var centre, out var radius);
			var n = HexDiagram.SideNormal(side);
			// Counter-clockwise on screen (y down) is the normal turned by -90 degrees: (x, y) -> (y, -x).
			var tangent = new Vector2(n.y, -n.x) * sign;
			DrawArrowhead(HexDiagram.SideMidpoint(centre, radius, side) + tangent * radius * 0.12f, tangent, Mathf.Max(4f, radius * 0.22f), color);
		}

		// Arrow from the centre to a side: a facing direction.
		public static void DrawFacingArrow (Rect rect, int side, Color color) {
			if(Event.current.type != EventType.Repaint) return;
			DiagramGeometry(rect, out var centre, out var radius);
			var n = HexDiagram.SideNormal(side);
			var tip = centre + n * radius * 0.78f;
			float head = Mathf.Max(5f, radius * 0.3f);
			Handles.color = color;
			Segment[0] = centre; Segment[1] = tip - n * head * 0.6f;
			Handles.DrawAAPolyLine(3f, Segment);
			Handles.DrawSolidDisc(centre, Vector3.forward, 2.5f);
			DrawArrowhead(tip - n * head * 0.4f, n, head, color);
		}

		static void DrawArrowhead (Vector2 at, Vector2 dir, float size, Color color) {
			var side = new Vector2(-dir.y, dir.x);
			Triangle[0] = at + dir * size * 0.6f;
			Triangle[1] = at - dir * size * 0.4f + side * size * 0.45f;
			Triangle[2] = at - dir * size * 0.4f - side * size * 0.45f;
			Handles.color = color;
			Handles.DrawAAConvexPolygon(Triangle);
			Handles.color = Color.white;
		}

		// Square rect of `size` at the start of `rect`, vertically centred.
		public static Rect DiagramRect (Rect rect, float size) {
			return new Rect(rect.x, rect.y + (rect.height - size) * 0.5f, size, size);
		}
	}
}
