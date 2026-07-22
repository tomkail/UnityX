using UnityEngine;

// Small helpers to give Vector2Int the few conveniences Point had, so call sites can migrate off Point.
public static class Vector2IntX {
	// x * y — cell count for a grid size. (Point had this as the `.area` property.)
	public static int Area (this Vector2Int v) {
		return v.x * v.y;
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

	static Vector2Int[] Offsets (Vector2Int[] offsets, Vector2Int p) {
		var result = new Vector2Int[offsets.Length];
		for (int i = 0; i < offsets.Length; i++) result[i] = offsets[i] + p;
		return result;
	}
}
