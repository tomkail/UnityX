// Editor-only debug visualiser for the hex system. Attach to any GameObject; it inspects the cell under the
// object's position (from the live WorldSpaceHexGrid) and draws that cell's centre, corners (c0..c5) and
// edges (e0..e5) labelled by index - so the corner/edge conventions and grid orientation are visible at a
// glance. It also marks the corner and edge closest to the object, so you can drag it around and check the
// closest-lookup geometry resolves correctly. Independent of SnapToGrid. Gizmos only, so it's stripped from
// builds.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityX.HexGrid;

public class HexTester : MonoBehaviour {
	[Header("Grid")]
	[Tooltip("Grid to inspect. Leave empty to use the first WorldSpaceHexGrid found in the scene.")]
	public WorldSpaceHexGrid grid;

	[Header("Draw")]
	[Tooltip("Outline the six neighbouring cells.")]
	public bool drawNeighbours = false;
	[Tooltip("Label the cell with its coordinate.")]
	public bool labelCoord = true;
	[Tooltip("Label corners c0..c5.")]
	public bool labelCorners = true;
	[Tooltip("Label edge midpoints e0..e5.")]
	public bool labelEdges = true;
	[Tooltip("Mark the corner and edge closest to this object's position, with a line from the cell centre to " +
	         "each (tests GetCornerPosition / GetEdgePosition).")]
	public bool showClosest = true;

	[Header("Neighbours")]
	[Tooltip("Draw a labelled vector to each of the six neighbouring cells (tests AxialToWorldVector / Direction).")]
	public bool drawNeighbourVectors = false;
	[Tooltip("Measure distance (and, if adjacent, direction index) from the inspected cell to this target cell.")]
	public bool measureToTarget = false;
	[Tooltip("The cell to measure to.")]
	public HexCoord target;

	[Header("Colours")]
	public Color hexColor = Color.white;
	public Color centerColor = Color.yellow;
	public Color cornerColor = Color.cyan;
	public Color edgeColor = new Color(1f, 0.5f, 1f);
	public Color closestColor = new Color(0.2f, 1f, 0.4f);

	void OnDrawGizmos () {
		// WorldSpaceHexGrid is no longer a singleton; use the assigned grid, or fall back to the first in the scene.
		var g = grid != null ? grid : FindAnyObjectByType<WorldSpaceHexGrid>(FindObjectsInactive.Include);
		if(g == null || g.grid == null) return;

		// Always inspect the cell under this object's position.
		var c = g.WorldToAxial(transform.position);
		DrawHex(g, c, true);
		if(drawNeighbours)
			for(int i = 0; i < 6; i++) DrawHex(g, c.Neighbor(i), false);
		if(showClosest) DrawClosest(g, c);
		if(drawNeighbourVectors) DrawNeighbourVectors(g, c);
		if(measureToTarget) DrawMeasure(g, c, target);
	}

	void DrawHex (WorldSpaceHexGrid grid, HexCoord c, bool primary) {
		var normal = grid.floorNormal;
		var center = grid.AxialToWorld(c);
		float s = HandleUtility.GetHandleSize(center);
		float fade = primary ? 1f : 0.3f;

		// Outline, drawn from the six corner positions.
		var loop = new Vector3[7];
		for(int i = 0; i < 6; i++) loop[i] = grid.GetCornerPosition(c, i);
		loop[6] = loop[0];
		Handles.color = Util.WithAlpha(hexColor, hexColor.a * fade);
		Handles.DrawAAPolyLine(primary ? 3f : 2f, loop);

		// Centre + coord label.
		Handles.color = Util.WithAlpha(centerColor, centerColor.a * fade);
		Handles.DrawSolidDisc(center, normal, s * 0.04f);
		if(primary && labelCoord) Handles.Label(center, c.ToString(), Style(centerColor, 13));

		// Neighbour cells are drawn as bare outlines so the labelled primary cell stays readable.
		if(!primary) return;

		// Corners c0..c5 - labels nudged outward from the centre so they clear the outline.
		Handles.color = cornerColor;
		for(int i = 0; i < 6; i++) {
			var p = loop[i];
			Handles.DrawSolidDisc(p, normal, s * 0.03f);
			if(labelCorners) Handles.Label(p + Outward(p, center) * s * 0.28f, "c" + i, Style(cornerColor, 11));
		}

		// Edges e0..e5 - each at the midpoint toward Direction(i).
		Handles.color = edgeColor;
		for(int i = 0; i < 6; i++) {
			var p = grid.GetEdgePosition(c, i);
			Handles.DrawSolidDisc(p, normal, s * 0.025f);
			if(labelEdges) Handles.Label(p + Outward(p, center) * s * 0.18f, "e" + i, Style(edgeColor, 11));
		}
	}

	// Mark the corner and edge nearest to this object's position. Draws a faint line from the cell centre to
	// the object, and bold lines from the centre to the closest corner and closest edge, labelled with their
	// index. Drag the object around a cell to confirm GetCornerPosition / GetEdgePosition resolve correctly.
	void DrawClosest (WorldSpaceHexGrid grid, HexCoord c) {
		var probe = transform.position;
		var center = grid.AxialToWorld(c);
		float s = HandleUtility.GetHandleSize(center);

		int bestCorner = 0, bestEdge = 0;
		float cornerD = float.MaxValue, edgeD = float.MaxValue;
		for(int i = 0; i < 6; i++) {
			float dc = (grid.GetCornerPosition(c, i) - probe).sqrMagnitude;
			if(dc < cornerD) { cornerD = dc; bestCorner = i; }
			float de = (grid.GetEdgePosition(c, i) - probe).sqrMagnitude;
			if(de < edgeD) { edgeD = de; bestEdge = i; }
		}

		// Faint centre -> object probe line.
		Handles.color = Util.WithAlpha(closestColor, 0.5f);
		Handles.DrawAAPolyLine(1.5f, center, probe);
		Handles.DrawSolidDisc(probe, grid.floorNormal, s * 0.03f);

		var cp = grid.GetCornerPosition(c, bestCorner);
		var ep = grid.GetEdgePosition(c, bestEdge);
		Handles.color = closestColor;
		Handles.DrawAAPolyLine(3f, center, cp);
		Handles.DrawAAPolyLine(3f, center, ep);
		Handles.DrawSolidDisc(cp, grid.floorNormal, s * 0.05f);
		Handles.DrawSolidDisc(ep, grid.floorNormal, s * 0.045f);
		Handles.Label(cp + Outward(cp, center) * s * 0.5f, "closest c" + bestCorner, Style(closestColor, 12));
		Handles.Label(ep + Outward(ep, center) * s * 0.4f, "closest e" + bestEdge, Style(closestColor, 12));
	}

	// A labelled vector to each of the six neighbours - the "d{i}" label is the direction/edge index, so you
	// can read off which way each Direction(i) points in world space (and confirm it lines up with edge e{i}).
	void DrawNeighbourVectors (WorldSpaceHexGrid grid, HexCoord c) {
		var center = grid.AxialToWorld(c);
		Handles.color = Color.green;
		for(int i = 0; i < 6; i++) {
			var n = grid.AxialToWorld(c.Neighbor(i));
			Handles.DrawAAPolyLine(2f, center, n);
			Handles.Label(Vector3.Lerp(center, n, 0.6f), "d" + i, Style(Color.green, 11));
		}
	}

	// Straight line to the target cell with the hex distance, and the direction index when they're adjacent.
	void DrawMeasure (WorldSpaceHexGrid grid, HexCoord from, HexCoord to) {
		var a = grid.AxialToWorld(from);
		var b = grid.AxialToWorld(to);
		Handles.color = Color.red;
		Handles.DrawAAPolyLine(3f, a, b);
		Handles.DrawSolidDisc(b, grid.floorNormal, HandleUtility.GetHandleSize(b) * 0.04f);
		int dist = HexCoord.Distance(from, to);
		string label = "dist " + dist;
		if(dist == 1) label += "   dir " + HexCoord.GetClosestDirectionIndex(from, to);
		Handles.Label(Vector3.Lerp(a, b, 0.5f), label, Style(Color.red, 12));
	}

	static Vector3 Outward (Vector3 point, Vector3 center) {
		var d = point - center;
		return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.zero;
	}

	static GUIStyle Style (Color color, int fontSize) {
		var style = new GUIStyle(EditorStyles.boldLabel);
		style.normal.textColor = color;
		style.fontSize = fontSize;
		return style;
	}
}
#endif
