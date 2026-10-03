using UnityEngine;

// Small helpers to give Vector2Int the few conveniences Point had, so call sites can migrate off Point.
public static class Vector2IntX {
	// x * y — cell count for a grid size. (Point had this as the `.area` property.)
	public static int Area (this Vector2Int v) {
		return v.x * v.y;
	}

	// Grid (taxicab) distance: steps along axes only. (Point.ManhattanDistance)
	public static int ManhattanDistance (this Vector2Int a, Vector2Int b) {
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	// Chebyshev distance: steps allowing diagonals. (Point.DiagonalDistance)
	public static int DiagonalDistance (this Vector2Int a, Vector2Int b) {
		return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
	}

	// Neighbour offsets, matching Point's ordering exactly so grid behaviour is unchanged.
	static readonly Vector2Int[] cardinalOffsets = {
		new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0)
	};
	static readonly Vector2Int[] ordinalOffsets = {
		new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1)
	};
	static readonly Vector2Int[] compassOffsets = {
		new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
		new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
	};

	// The four cardinal neighbours (N E S W) of a point.
	public static Vector2Int[] CardinalDirections (this Vector2Int p) {
		return Offsets(cardinalOffsets, p);
	}
	// The four ordinal (diagonal) neighbours (NE SE SW NW) of a point.
	public static Vector2Int[] OrdinalDirections (this Vector2Int p) {
		return Offsets(ordinalOffsets, p);
	}
	// All eight cardinal + ordinal neighbours of a point.
	public static Vector2Int[] CompassDirections (this Vector2Int p) {
		return Offsets(compassOffsets, p);
	}

	// The four corners of a unit square cell centred on a point: top left, top right, bottom right, bottom left.
	// (Point's corner ordering, so outlines trace the same way.)
	static readonly Vector2[] cornerOffsets = {
		new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
	};
	public const int numCorners = 4;

	// Position of corner `index` (wrapped into 0..3) of the cell at `cell`. (Point.Corner)
	public static Vector2 Corner (this Vector2Int cell, int index) {
		index %= numCorners;
		if (index < 0) index += numCorners;
		return cornerOffsets[index] + (Vector2)cell;
	}

	// The corner index of `otherCell` that sits on corner `cellCornerIndex` of `cell`, or -1 if they don't share it.
	// With Corner, this is what UnityX.Islands.OutlineDetector.GetOutlinePoly needs to trace square cells. (Point.GetTouchingCornerPointIndex)
	public static int GetTouchingCornerPointIndex (Vector2Int cell, int cellCornerIndex, Vector2Int otherCell) {
		if (Mathf.Abs(cell.x - otherCell.x) != 1 && Mathf.Abs(cell.y - otherCell.y) != 1) return -1;
		var cornerPoint = cell.Corner(cellCornerIndex);
		for (int otherCornerIndex = 0; otherCornerIndex < numCorners; otherCornerIndex++) {
			if ((cornerPoint - otherCell.Corner(otherCornerIndex)).sqrMagnitude < 0.1f) return otherCornerIndex;
		}
		return -1;
	}

	static Vector2Int[] Offsets (Vector2Int[] offsets, Vector2Int p) {
		var result = new Vector2Int[offsets.Length];
		for (int i = 0; i < offsets.Length; i++) result[i] = offsets[i] + p;
		return result;
	}
}
