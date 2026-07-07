using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityX.Islands;

// A set of grid cells (a square-grid shape, e.g. a tetromino). The cells are the topology; the
// value this type adds over a bare list is the cached geometry — bounds/center/cellBounds — which
// is why it stays concrete on Vector2Int. Hex shapes would be their own concrete type (their
// bounds depend on hex orientation/size/origin), reusing the generic CreateRandomContiguous below.
[Serializable]
public class GridShape {
	[SerializeField] List<Vector2Int> cells;
	public IReadOnlyList<Vector2Int> Cells => cells;

	// Cached geometry, recomputed whenever the cells change (see Recompute).
	public Rect bounds;          // float bounds of the cells (each cell treated as a unit point)
	public Vector2 center;       // centre of bounds
	public RectInt cellBounds;   // integer bounding box of the cells

	public GridShape () {
		cells = new List<Vector2Int>();
		Recompute();
	}

	public GridShape (IEnumerable<Vector2Int> cells) {
		this.cells = new List<Vector2Int>(cells);
		Recompute();
	}

	public bool Contains (Vector2Int cell) => cells.Contains(cell);

	// Folded in from the old Structure type: predicate membership test.
	public bool Contains (Func<Vector2Int, bool> predicate) => cells.Any(predicate);

	public IEnumerable<Vector2Int> GetTranslatedCells (Vector2Int offset) => cells.Select(c => c + offset);

	public GridShape Translated (Vector2Int offset) => new GridShape(GetTranslatedCells(offset));

	// Recompute the cached geometry from the current cells. Called by the constructors; call it
	// again if you mutate the cell list in place.
	public void Recompute () {
		if (cells.Count == 0) {
			bounds = new Rect();
			center = Vector2.zero;
			cellBounds = new RectInt();
			return;
		}
		int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
		for (int i = 0; i < cells.Count; i++) {
			var c = cells[i];
			if (c.x < minX) minX = c.x;
			if (c.y < minY) minY = c.y;
			if (c.x > maxX) maxX = c.x;
			if (c.y > maxY) maxY = c.y;
		}
		bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
		center = bounds.center;
		// Extent (not cell count) with a minimum of 1, so a single cell still spans a 1x1 box.
		cellBounds = new RectInt(minX, minY, Mathf.Max(maxX - minX, 1), Mathf.Max(maxY - minY, 1));
	}

	public override string ToString () => $"[GridShape] cells={cells.Count} cellBounds={cellBounds}";

	// --- Random contiguous generation -----------------------------------------------------------

	static readonly Vector2Int[] fourNeighbourOffsets = {
		new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0)
	};

	static IEnumerable<Vector2Int> FourNeighbours (Vector2Int cell) {
		for (int i = 0; i < fourNeighbourOffsets.Length; i++)
			yield return cell + fourNeighbourOffsets[i];
	}

	// Square-grid convenience: a random connected shape of `count` cells, normalised so its minimum
	// corner sits at (0,0). Thin wrapper over the generic connected-region generator in
	// UnityX.Islands, adding the 4-neighbour rule and wrapping the result with GridShape's bounds.
	public static GridShape CreateRandomContiguous (int count) {
		var cells = IslandGenerator.CreateRandomIsland(Vector2Int.zero, FourNeighbours, count).points;
		if (cells.Count > 0) {
			int minX = int.MaxValue, minY = int.MaxValue;
			for (int i = 0; i < cells.Count; i++) {
				if (cells[i].x < minX) minX = cells[i].x;
				if (cells[i].y < minY) minY = cells[i].y;
			}
			var min = new Vector2Int(minX, minY);
			for (int i = 0; i < cells.Count; i++) cells[i] -= min;
		}
		return new GridShape(cells);
	}
}
